using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Collections.Specialized;
using System.Collections.Generic;
using LitJson;
using System.Linq;

namespace Connector {
    public class MiraiConnector : BaseConnector {
        public HttpClient client = new HttpClient();
        public string sessionKey = null;
        public long QQ;
        public async Task<JsonData> GetJson(string url, NameValueCollection queries) {
            var resp = await client.GetAsync(url + "?" + queries.ToString());
            resp.EnsureSuccessStatusCode();
            var data = JsonMapper.ToObject(await resp.Content.ReadAsStringAsync());
            if (data.ContainsKey("code") && (int)data["code"] != 0)
                throw new Exception($"Error GET {url}. Return code is {(int)data["code"]}.");
            return data;
        }
        public async Task<JsonData> PostJson(string url, JsonData jsonData) {
            var resp = await client.PostAsync(url, new StringContent(JsonMapper.ToJson(
                jsonData
            )));
            resp.EnsureSuccessStatusCode();
            var data = JsonMapper.ToObject(await resp.Content.ReadAsStringAsync());
            if (data.ContainsKey("code") && (int)data["code"] != 0)
                throw new Exception($"Error POST {url}. Return code is {(int)data["code"]}.");
            return data;
        }
        public override async Task Initialize(Config config) {
            client.BaseAddress = new Uri(config.miraiEndpoint);
            var receipt = await PostJson("/auth", new JsonData { ["authKey"] = config.authKey });
            sessionKey = (string)receipt["session"];
            await PostJson("/verify", new JsonData { ["sessionKey"] = sessionKey, ["qq"] = QQ = config.miraiQQ });
        }

        public override async Task SendMessage(IMessageTarget target, Message message) {
            if (message.Count == 0) return;
            JsonData msgChain = new JsonData();
            foreach (var sub in message) {
                switch (sub) {
                    case LocalImageSub localImageSub:
                        msgChain.Add (new JsonData {
                            ["type"] = "Image",
                            ["path"] = localImageSub.resourcePath
                        });
                        break;
                    case TextSub textSub:
                        msgChain.Add (new JsonData {
                            ["type"] = "Plain",
                            ["text"] = textSub.text
                        });
                        break;
                    case AtSub atSub:
                        msgChain.Add (new JsonData {
                            ["type"] = "At",
                            ["target"] = atSub.target
                        });
                        break;
                }
            }
            switch (target) {
                case GroupTarget groupTarget:
                    await PostJson("/sendGroupMessage", new JsonData {
                        ["sessionKey"] = sessionKey,
                        ["target"] = groupTarget.id,
                        ["messageChain"] = msgChain
                    });
                    break;
            }
        }

        public override async Task<Message[]> FetchMessages() {
            var nv = HttpUtility.ParseQueryString(string.Empty);
            nv.Add("sessionKey", sessionKey);
            nv.Add("count", "10");
            var fetched = await GetJson("/fetchMessage", nv);
            var stage = new List<Message>();
            for (int i = 0; i < fetched["data"].Count; i++) {
                var msg = fetched["data"][i];
                if ((string)msg["type"] == "GroupMessage") {
                    var message = new Message {
                        source = this,
                        sender = new GroupTarget {
                            id = (long)msg["sender"]["group"]["id"],
                            name = (string)msg["sender"]["group"]["name"],
                            user = new UserTarget {
                                id = (long)msg["sender"]["id"],
                                name = (string)msg["sender"]["memberName"]
                            }
                        }
                    };
                    for (int j = 0; j < msg["messageChain"].Count; j++) {
                        var sub = msg["messageChain"][j];
                        var t = (string)sub["type"];
                        switch (t) {
                            case "Plain":
                                message.Add(new TextSub { text = (string)sub["text"] });
                                break;
                            case "At":
                                var atTarget = (long)sub["target"] == QQ ? Helper.MyID : (long)sub["target"];
                                message.Add(new AtSub { target = atTarget });
                                break;
                        }
                    }
                    if (message.Any((x) => x is AtSub atSub && atSub.target == Helper.MyID))
                        stage.Add (message);
                }
            }
            return stage.ToArray();
        }
    }
}
