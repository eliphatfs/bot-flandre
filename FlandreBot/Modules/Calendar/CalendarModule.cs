using System;
using System.Text;
using System.Linq;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Connector;

namespace Module.Calendar {
    public class CalendarModule : BaseModule {
        public List<CalendarEvent> calendar = new List<CalendarEvent>();
        public List<string> subscribes = new List<string>();
        public override async Task HandleMessage(Message message) {
            foreach (var textSub in message.Where((x) => x is TextSub)) {
                var text = ((TextSub)textSub).text;
                if (text.TryCommandParse("canvas subscribe", out var uri)) {
                    subscribes.Add(uri);
                }
                if (text.TryCommandParse("canvas", out var args)) {
                    int page = 1;
                    if (args.TryCommandParse("page", out var pageStr)) {
                        page = int.Parse(pageStr);
                    }
                    calendar = await PollEvents();
                    var selector = calendar as IEnumerable<CalendarEvent>;
                    if (args.Contains("future only"))
                        selector = selector.Where((ev) => ev.DateStart >= DateTime.Now.Date - new TimeSpan(8, 0, 0));
                    selector = selector.Skip((page - 1) * 10).Take(10);
                    var mb = new StringBuilder();
                    int index = 1;
                    foreach (var ev in selector) {
                        mb.Append(index).Append(". ").AppendLine(ev.Summary);
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

        public override async Task Initialize(Config config, BaseConnector[] connectors) {
            var persistSub = await Persistence.GetObject("calendar_subscribes");
            for (int i = 0; i < persistSub.Count; i++) {
                subscribes.Add((string)persistSub[i]);
            }
        }
    }
}
