using System;
using System.Linq;
using System.Threading.Tasks;

using Connector;

namespace Module {
    public class ClockModule : BaseModule {
        public override async Task Initialize(Config config, BaseConnector[] connectors) {
            Clock(config, connectors);
            await Task.Yield();
        }

        public async void Clock(Config config, BaseConnector[] connectors) {
            var old = DateTime.Now;
            while (true) {
                var cur = DateTime.Now;
                if (old.Hour != cur.Hour) {
                    old = cur;
                    /*foreach (var connector in connectors)
                        foreach (var gid in config.groupsClock)
                            await connector.SendMessage(new GroupTarget { id = gid }, new Message {
                                new LocalImageSub { resourcePath = $"clock/{cur.Hour % 12}.jpg" }
                            });*/
                }
                await Task.Delay(1000);
            }
        }

        public override async Task HandleMessage(Message message) {
            if (
                message.Any((x) => x is TextSub textSub && textSub.text.Contains("几点了"))
                && message.Any((x) => x is AtSub atSub && atSub.target == Helper.MyID)
            ) {
                await message.Reply (new Message {
                    new LocalImageSub { resourcePath = $"clock/{DateTime.Now.Hour % 12}.jpg" }
                });
            }
        }
    }
}
