using System;
using System.Threading;
using CreamRoll.Routing;
using tart.Server;
using tart.Simulations.Models.Example;

namespace tart {
    class Program {
        static void Main(string[] args) {
            var simul = new SimulationServer(typeof(ExampleGame));
            var frontend = new FrontEndServer("FrontEnd", "index.html");

            var server = new RouteServer<SimulationServer>(simul);
            server.AppendRoutes(frontend);
            var waiter = new ManualResetEvent(false);

            Console.CancelKeyPress += (o, e) => {
                e.Cancel = true;
                waiter.Set();
            };

            server.StartAsync();
            Console.WriteLine("tart started.. press ctrl+c to stop");

            waiter.WaitOne();
            server.Stop();
        }
    }
}
