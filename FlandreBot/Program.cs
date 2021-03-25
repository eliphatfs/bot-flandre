using System;
using System.Threading.Tasks;

using Connector;
using Module;
using Module.Calendar;

namespace FlandreBot
{
    class Program
    {
        async static Task Main(string[] args)
        {
            var cfg = Config.FromFile();
            var connectors = new BaseConnector[] { new TGConnector() };
            var modules = new BaseModule[] { new ClockModule(), new GuthibModule(), new CalendarModule() };

            foreach (var connector in connectors) await connector.Initialize(cfg);
            foreach (var module in modules) await module.Initialize(cfg, connectors);
            Console.WriteLine("Online.");
            while (true) {
                foreach (var connector in connectors) {
                    try {
                        var msgs = await connector.FetchMessages();
                        foreach (var msg in msgs) {
                            foreach (var module in modules) {
                                await module.HandleMessage(msg);
                            }
                            Console.WriteLine("Handled: " + LitJson.JsonMapper.ToJson(msg));
                        }
                    } catch (Exception e) {
                        Console.WriteLine("Error: " + e.Message);
                    }
                }
                await Task.Delay(1000);
            }
        }
    }
}
