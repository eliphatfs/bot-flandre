using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using LitJson;

namespace Connector {
    public class TGConnector : BaseConnector {
        public HttpClient client = new HttpClient();
        // private string _botServiceUrl;
        public async Task<JsonData> PostJson(string url, JsonData jsonData) {
            var resp = await client.PostAsync(url, new StringContent(JsonMapper.ToJson(
                jsonData
            ), Encoding.UTF8, "application/json"));
            var data = JsonMapper.ToObject(await resp.Content.ReadAsStringAsync());
            if (data.ContainsKey("ok") && !(bool)data["ok"])
                throw new Exception($"Error POST {url}. Reason: {(string)data["description"]}.");
            return data;
        }
        public async Task<JsonData> PostMPF(string url, MultipartFormDataContent mpf) {
            var resp = await client.PostAsync(url, mpf);
            var data = JsonMapper.ToObject(await resp.Content.ReadAsStringAsync());
            if (data.ContainsKey("ok") && !(bool)data["ok"])
                throw new Exception($"Error POST {url}. Reason: {(string)data["description"]}.");
            return data;
        }
        public long offset = 0;
        public override async Task<Message[]> FetchMessages() {
            var fetched = await PostJson("getUpdates", new JsonData { ["offset"] = offset });
            var result = fetched["result"];
            var stage = new List<Message>();
            for (var i = 0; i < result.Count; i++) {
                offset = Math.Max(offset, (long)result[i]["update_id"] + 1);
                if (!result[i].ContainsKey("message"))
                    continue;
                var msg = result[i]["message"];
                var message = new Message { new TextSub { text = (string)msg["text"] } };
                message.source = this;
                switch ((string)msg["chat"]["type"]) {
                    case "group":
                        message.sender = new GroupTarget {
                            id = (long)msg["chat"]["id"],
                            name = (string)msg["chat"]["title"],
                        };
                        break;
                    case "private":
                        message.sender = new UserTarget {
                            id = (long)msg["chat"]["id"],
                            name = (string)msg["chat"]["first_name"],
                        };
                        break;
                }
                stage.Add(message);
            }
            return stage.ToArray();
        }

        public override async Task Initialize(Config config) {
            client.BaseAddress = new Uri($"https://api.telegram.org/bot{config.telegramToken}/");
            // _botServiceUrl = config.botServiceUrl;
            await FetchMessages();
        }

        public override async Task SendMessage(IMessageTarget target, Message message) {
            if (message.Count == 0) return;
            long id;
            switch (target) {
                case GroupTarget g:
                    id = g.id;
                    break;
                case UserTarget u:
                    id = u.id;
                    break;
                default:
                    return;
            }
            foreach (var sub in message) {
                switch (sub) {
                    case TextSub textSub:
                        await PostJson("sendMessage", new JsonData {
                            ["chat_id"] = id,
                            ["text"] = textSub.text,
                            ["parse_mode"] = "Markdown"
                        });
                        break;
                    case LocalImageSub localImageSub:
                        var photoContent = new MultipartFormDataContent();
                        photoContent.Add(new StringContent(id.ToString()), "chat_id");
                        photoContent.Add(new ByteArrayContent(
                            await File.ReadAllBytesAsync(Path.Join("./Resources/images", localImageSub.resourcePath))
                        ), "photo", Path.GetFileName(localImageSub.resourcePath));
                        await PostMPF("sendPhoto", photoContent);
                        /*await PostJson("sendPhoto", new JsonData {
                            ["chat_id"] = id,
                            ["photo"] = new Uri(new Uri(new Uri(_botServiceUrl), "images"), localImageSub.resourcePath).ToString()
                        });*/
                        break;
                }
            }
        }
    }
}
