using System;
using System.Text;
using System.Linq;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Connector;
using LitJson;

namespace Module.Calendar {
    public class CalendarModule : BaseModule {
        public List<CalendarEvent> calendar = new List<CalendarEvent>();
        public List<string> subscribes = new List<string>();
        public List<Message> notifications = new List<Message>();
        public override async Task HandleMessage(Message message) {
            foreach (var textSub in message.Where((x) => x is TextSub)) {
                var text = ((TextSub)textSub).text;
                if (text.TryCommandParse("canvas clear notification", out _)) {
                    notifications.Clear();
                    await message.Reply(new Message { new TextSub { text = "Cleared." } });
                }
                else if (text.TryCommandParse("canvas notification", out _)) {
                    notifications.Add(message);
                    await message.Reply(new Message { new TextSub { text = "Setup." } });
                }
                else if (text.TryCommandParse("canvas subscribe", out var uri)) {
                    subscribes.Add(uri);
                    await Persistence.WriteObject("calendar_subscribes", JsonMapper.ToObject(JsonMapper.ToJson(subscribes)));
                }
                else if (text.TryCommandParse("canvas detail", out var idxStr)) {
                    var idx = int.Parse(idxStr);
                    var ev = calendar[idx - 1000];
                    var txt = $"Details for `{ev.Summary.Replace("\\,", ",")}`\nDue: {ev.DateStart.ToString()}\n";
                    await message.Reply(new Message { new TextSub { text = txt + ev.Description } });
                }
                else if (text.TryCommandParse("canvas", out var args)) {
                    int page = 1;
                    if (args.TryCommandParse("page", out var pageStr)) {
                        page = int.Parse(pageStr);
                    }
                    var selector = calendar as IEnumerable<CalendarEvent>;
                    if (args.Contains("future only"))
                        selector = selector.Where((ev) => ev.DateStart >= DateTime.Now.Date - new TimeSpan(8, 0, 0));
                    selector = selector.Skip((page - 1) * 10).Take(10);
                    var mb = new StringBuilder();
                    int index = 1;
                    foreach (var ev in selector) {
                        mb.Append(index).Append(". `")
                        .Append(ev.Summary.Replace("\\,", ","))
                        .Append($"` *Due {ev.DateStart.ToString()}*")
                        .AppendLine($" \\[ID = {1000 + calendar.IndexOf(ev)}]");
                        index++;
                    }
                    await message.Reply(new Message { new TextSub { text = mb.ToString() } });
                }
            }
        }

        public async Task<List<CalendarEvent>> PollEvents() {
            var results = new List<CalendarEvent>();
            using (var client = new HttpClient()) {
                foreach (var subscribe in subscribes) {
                    var ical = await client.GetStringAsync(subscribe);
                    results.AddRange(await new CalendarParser().Parse(ical));
                }
            }
            return results;
        }

        public async void Subscriber() {
            try {
                while (true) {
                    await Task.Delay(60 * 1000);
                    var newer = await PollEvents();
                    foreach (var ev in newer) {
                        if (!calendar.Any((x) => x.Summary == ev.Summary && x.DateStart == ev.DateStart && x.Description == ev.Description))
                        {
                            var txt = $"Canvas Update!\n`{ev.Summary.Replace("\\,", ",")}`\nDue: {ev.DateStart.ToString()}\n";
                            foreach (var notsrc in notifications)
                                await notsrc.Reply(new Message { new TextSub { text = txt + ev.Description } });
                        }
                    }
                    calendar = newer;
                }
            } catch (Exception e) {
                Console.WriteLine("Error in calendar routine: " + e.Message);
                Subscriber();
            }
        }

        public override async Task Initialize(Config config, BaseConnector[] connectors) {
            var persistSub = await Persistence.GetObject("calendar_subscribes");
            if (persistSub != null)
                for (int i = 0; i < persistSub.Count; i++) {
                    subscribes.Add((string)persistSub[i]);
                }
            calendar = await PollEvents();
            Subscriber();
        }
    }
}
