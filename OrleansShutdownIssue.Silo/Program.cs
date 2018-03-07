using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Storage;
using OrleansShutdownIssue.Interfaces;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace OrleansShutdownIssue.Silo {

    /// <summary>
    /// Demonstrates the console app "hanging open" when the Console.CancelKeyPress handler is allowed to return before the Main method does.
    /// </summary>
    class Program {

        // When true, the program will close down gracefully.
        // When false, the orleans silo will shutdown, but the console app will be left "hanging" forever.
        const bool MAIN_THREAD_EXITS_FIRST = true;

        static readonly ManualResetEvent _siloStarted = new ManualResetEvent(false);
        static readonly ManualResetEvent _siloStopped = new ManualResetEvent(false);

        static ISiloHost silo;

        static void Main(string[] args) {

            Console.CancelKeyPress += (s, a) => {
                Task.Run(StopSilo);
                _siloStopped.WaitOne();
                if (MAIN_THREAD_EXITS_FIRST)
                    Thread.Sleep(100);
            };

            silo = CreateSilo();
            Task.Run(StartSilo);
            _siloStarted.WaitOne();
            Console.WriteLine("Silo has completed startup. Press Ctrl+C to (hopefully) exit the program.");


            _siloStopped.WaitOne();
            if (!MAIN_THREAD_EXITS_FIRST)
                Thread.Sleep(100);
        }

        static ISiloHost CreateSilo() {

            var siloIP = IPAddress.Loopback;
            var siloPort = 11111;
            var gatewayPort = 30000;
            var siloEndPoint = new IPEndPoint(IPAddress.Loopback, siloPort);

            return new SiloHostBuilder()
                .Configure(options => options.ClusterId = "nadexdata")
                .UseDevelopmentClustering(options => options.PrimarySiloEndpoint = siloEndPoint)
                .ConfigureEndpoints(advertisedIP: siloIP, siloPort: siloPort, gatewayPort: gatewayPort, listenOnAllHostAddresses: true)
                .ConfigureApplicationParts(parts => {
                    parts.AddApplicationPart(typeof(ISampleGrain).Assembly).WithReferences();
                    parts.AddApplicationPart(typeof(MemoryGrainStorage).Assembly).WithReferences();
                })
                .ConfigureLogging(builder => builder.SetMinimumLevel(LogLevel.Warning).AddConsole())
                .AddMemoryGrainStorageAsDefault(options => options.NumStorageGrains = 1)
                // this setting makes no difference - I've experimented with both true and false
                .Configure<ProcessExitHandlingOptions>(options => options.FastKillOnProcessExit = true)
                .Build();
        }


        static async Task StartSilo() {
            Console.WriteLine("Silo is starting. Please wait.");
            await silo.StartAsync();
            _siloStarted.Set();
        }

        static async Task StopSilo() {
            await silo.StopAsync();
            _siloStopped.Set();
            Console.WriteLine("Silo stopped - program should exit now.");
        }
    }
}
