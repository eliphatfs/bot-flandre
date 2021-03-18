using System;
using System.Threading.Tasks;

using Connector;
using Module;

namespace FlandreBot
{
    class Program
    {
        async static Task Main(string[] args)
        {
            var cfg = Config.FromFile();
            var connectors = new BaseConnector[] { new MiraiConnector() };
            var modules = new BaseModule[] { new ClockModule() };

            foreach (var connector in connectors) await connector.Initialize(cfg);
            foreach (var module in modules) await module.Initialize(cfg, connectors);
            Console.WriteLine("Online.");
            while (true) {
                foreach (var connector in connectors) {
                    var msgs = await connector.FetchMessages();
                    foreach (var msg in msgs) {
                        foreach (var module in modules) {
                            await module.HandleMessage(msg);
                        }
                        Console.WriteLine("Handled: " + LitJson.JsonMapper.ToJson(msg));
                    }
                }
                await Task.Delay(1000);
            }
        }
    }
}
