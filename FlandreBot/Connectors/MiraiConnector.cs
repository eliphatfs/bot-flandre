using System;
using System.Net.Http;
using System.Threading.Tasks;
using LitJson;

namespace Connector {
    public class MiraiConnector : BaseConnector {
        public HttpClient client = new HttpClient();
        public string sessionKey = null;
        public async Task<JsonData> PostJson(string url, JsonData jsonData) {
            var resp = await client.PostAsync(url, new StringContent(JsonMapper.ToJson(
                jsonData
            )));
            resp.EnsureSuccessStatusCode();
            var data = JsonMapper.ToObject(await resp.Content.ReadAsStringAsync());
            if ((int)data["code"] != 0)
                throw new Exception($"Error sending request to {url}. Return code is {(int)data["code"]}.");
            return data;
        }
        public override async Task Initialize(Config config) {
            client.BaseAddress = new Uri(config.miraiEndpoint);
            var receipt = await PostJson("/auth", new JsonData { ["authKey"] = config.authKey });
            sessionKey = (string)receipt["session"];
            await PostJson("/verify", new JsonData { ["sessionKey"] = sessionKey, ["qq"] = config.miraiQQ });

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
    }
}
