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

        protected static JsonSerializerOptions defaultJsonOptions;

        public SimulationServer(params Type[] gameModelTypes) {
            _gameModelTypes = gameModelTypes;
            _simulations = new List<IGame>();

            defaultJsonOptions = new JsonSerializerOptions {
                PropertyNamingPolicy = new CamelBack(),
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
            var response = JsonSerializer.Serialize(models, defaultJsonOptions);
            return new JsonResponse(response);
        }

        [Get("/simulations")]
        public Response GetSimulations(Request req) {
            var simuls = _simulations.Select(g => g.GetType().Name).ToArray();
            var response = JsonSerializer.Serialize(simuls, defaultJsonOptions);
            return new JsonResponse(response);
        }

        [Get("/simulations/{simulationId}")]
        public Response GetSimulation(Request req) {
            var simulIndex = TryParseIndex(req, "simulationId", _simulations.Count, out var errorResp);
            if (simulIndex < 0) return errorResp;

            var simul = _simulations[simulIndex];
            return new JsonResponse(JsonSerializer.Serialize(simul, defaultJsonOptions));
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

        [Get("/simulations/{simulationId}/types")]
        public Response GetSimulationTypes(Request req) {
            var simulIndex = TryParseIndex(req, "simulationId", _simulations.Count, out var errorResp);
            if (simulIndex < 0) return errorResp;

            var simul = _simulations[simulIndex];
            var names = Enum.GetNames(simul.UpgradeType);
            return new JsonResponse(new JSON {
                ["names"] = names,
            });
        }

        [Get("/simulations/{simulationId}/choices")]
        public Response GetChoicesOfSimulation(Request req) {
            var simulIndex = TryParseIndex(req, "simulationId", _simulations.Count, out var errorResp);
            if (simulIndex < 0) return errorResp;

            var simul = _simulations[simulIndex];
            var choices = simul.GetAvailableChoices();
            return new JsonResponse(JsonSerializer.Serialize(choices, defaultJsonOptions));
        }

        [Get("/simulations/{simulationId}/choices/{choiceId}/type")]
        public Response GetChoiceTypeInfo(Request req) {
            var simulIndex = TryParseIndex(req, "simulationId", _simulations.Count, out var errorResp);
            if (simulIndex < 0) return errorResp;

            var simul = _simulations[simulIndex];
            var choices = simul.GetAvailableChoices().ToArray();

            var choiceIndex = TryParseIndex(req, "choiceId", choices.Length, out errorResp);
            if (choiceIndex < 0) return errorResp;

            var type = simul.UpgradeType;
            var name = Enum.GetName(type, choices[choiceIndex].Type);
            return new JsonResponse(new JSON {
                ["name"] = name,
            });
        }

        [Get("/simulations/{simulationId}/upgrades")]
        public Response GetUpgradeOfSimulation(Request req) {
            var simulIndex = TryParseIndex(req, "simulationId", _simulations.Count, out var errorResp);
            if (simulIndex < 0) return errorResp;

            var simul = _simulations[simulIndex];
            var upgrades = simul.Upgrades;
            return new JsonResponse(JsonSerializer.Serialize(upgrades, defaultJsonOptions));
        }

        private int TryParseIndex(Request req, string name, int length, out Response resp) {
            var idRaw = (string)req.Query[name].ToString();

            if (!int.TryParse(idRaw, out var id)) {
                resp = ErrorResp($"Invalid {name} format");
                return -1;
            }
            if (id < 0 || length <= id) {
                resp = ErrorResp($"{name} out of range");
                return -1;
            }

            resp = null;
            return id;
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
                return JsonSerializer.Serialize(json, defaultJsonOptions);
            }
        }

        class CamelBack : JsonNamingPolicy {
            public override string ConvertName(string name) {
                return $"{char.ToLower(name[0])}{name.Substring(1)}";
            }
        }
    }
}