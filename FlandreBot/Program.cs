﻿using System;
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
            var modules = new BaseModule[] { };

            foreach (var connector in connectors) await connector.Initialize(cfg);
            foreach (var module in modules) await module.Initialize(cfg, connectors);

            while (true)
                await Task.Delay(100);  // TODO: Message loop after finishing
        }
    }
}