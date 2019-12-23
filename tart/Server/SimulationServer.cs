using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CreamRoll.Routing;
using tart.Simulations;

namespace tart.Server {
    public class SimulationServer {
        private List<IGame> _simulations;
        private Type[] _gameModelTypes;

        protected JsonSerializerOptions defaultJsonOptions;

        public SimulationServer(params Type[] gameModelTypes) {
            _gameModelTypes = gameModelTypes;
            _simulations = new List<IGame>();

            defaultJsonOptions = new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true,
            };
        }

        [Get("/")]
        public Response Index(Request req) {
            return new TextResponse("it works!");
        }

        [Get("/models")]
        public Response GetModels(Request req) {
            var models = _gameModelTypes.Select(g => g.Name).ToArray();
            var response = JsonSerializer.Serialize(models);
            return new JsonResponse(response);
        }

        [Get("/simulations")]
        public Response GetSimulations(Request req) {
            var simuls = _simulations.Select(g => g.GetType().Name).ToArray();
            var response = JsonSerializer.Serialize(simuls);
            return new JsonResponse(response);
        }

        [Get("/simulations/{id}")]
        public Response GetSimulation(Request req) {
            var idRaw = (string)req.Query["id"].ToString();
            if (!int.TryParse(idRaw, out var id)) {
                return ErrorResp("Invalid simulation id format");
            }
            if (id < 0 || _simulations.Count <= id) {
                return ErrorResp("Simulation id out of range");
            }

            var simul = _simulations[id];
            return new JsonResponse(JsonSerializer.Serialize(simul));
        }

        [Post("/simulations")]
        public async Task<Response> CreateSimulation(Request req) {
            try {
                var body = await JsonSerializer.DeserializeAsync<CreateSimulationRequest>(req.Body, defaultJsonOptions);
                var model = _gameModelTypes[body.ModelIndex];
                var game = (IGame)Activator.CreateInstance(model);
                _simulations.Add(game);
                return new JsonResponse(new JSON {
                    ["id"] = _simulations.Count - 1,
                }, status: StatusCode.Created);
            } catch (JsonException) {
                return ErrorResp("Wrong json format");
            } catch (Exception ex) {
                return ErrorResp(ex.Message);
            }
        }

        [Get("/simulations/{id}/upgrades")]
        public Response GetUpgradesOfSimulation(Request req) {
            var idRaw = (string)req.Query["id"].ToString();
            if (!int.TryParse(idRaw, out var id)) {
                return ErrorResp("Invalid simulation id format");
            }
            if (id < 0 || _simulations.Count <= id) {
                return ErrorResp("Simulation id out of range");
            }

            var simul = _simulations[id];
            var choices = simul.GetAvailableChoices();
            return new JsonResponse(JsonSerializer.Serialize(choices));
        }

        private static JsonResponse ErrorResp(string message) {
            return new JsonResponse(new JSON {
                ["message"] = message,
            }, status: StatusCode.BadRequest);
        }

        struct CreateSimulationRequest {
            public int ModelIndex { get; set; }
        }

        class JSON : Dictionary<string, object> {
            public static implicit operator string(JSON json) {
                return JsonSerializer.Serialize(json);
            }
        }
    }
}