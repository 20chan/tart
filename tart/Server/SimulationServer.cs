using System;
using CreamRoll.Routing;

namespace tart.Server {
    public class SimulationServer {
        private Type[] _gameModelTypes;

        public SimulationServer(params Type[] gameModelTypes) {
            _gameModelTypes = gameModelTypes;
        }

        [Get("/")]
        public Response Index(Request req) {
            return new TextResponse("wow it works");
        }
    }
}