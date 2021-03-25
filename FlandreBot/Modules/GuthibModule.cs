using System.Linq;
using System.Threading.Tasks;
using Connector;

namespace Module {
    public class GuthibModule : BaseModule {
        public override async Task HandleMessage(Message message) {
            if (message.Count == 1 && message[0] is TextSub textSub
            && new[] { "/guthib", "guthib" }.Contains(textSub.text.Trim().ToLower())) {
                await message.Reply(new Message { new TextSub { text = "You spelled it wrong." } });
            }
        }

        public override async Task Initialize(Config config, BaseConnector[] connectors) {
            await Task.CompletedTask;
        }
    }
}