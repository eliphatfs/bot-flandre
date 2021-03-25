using System.Linq;
using System.Threading.Tasks;
using Connector;

namespace Module {
    public class GuthibModule : BaseModule {
        public override async Task HandleMessage(Message message) {
            if (message.Count == 1 && message[0] is TextSub textSub
            && new[] { "/guthib", "guthib", "/guthib@guthib_bot" }.Contains(textSub.text.Trim().ToLower())) {
                await message.Reply(new Message {
                    new TextSub { text = "You spelled it wrong" },
                    new TextSub { text = "You should go to guthub.com" }
                });
            }
        }

        public override async Task Initialize(Config config, BaseConnector[] connectors) {
            await Task.CompletedTask;
        }
    }
}