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

        protected static JsonSerializerOptions defaultJsonOptions, updateReqJsonOptions;

        public SimulationServer(params Type[] gameModelTypes) {
            _gameModelTypes = gameModelTypes;
            _simulations = new List<IGame>();

            defaultJsonOptions = new JsonSerializerOptions {
                PropertyNamingPolicy = new CamelBack(),
                PropertyNameCaseInsensitive = true,
            };

            updateReqJsonOptions = new JsonSerializerOptions {
                IgnoreNullValues = true,
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
            } catch (JsonException ex) {
                return ErrorResp("Invalid json format", ex);
            } catch (Exception ex) {
                return ErrorResp("error", ex, StatusCode.InternalServerError);
            }
        }

        [Get("/simulations/{simulationId}/tick?{interval}=0.1")]
        public Response TickSimulation(Request req) {
            var simulIndex = TryParseIndex(req, "simulationId", _simulations.Count, out var errorResp);
            if (simulIndex < 0) return errorResp;

            var simul = _simulations[simulIndex];
            var intervalRaw = (string)req.Query["interval"].ToString();
            if (!float.TryParse(intervalRaw, out var interval)) {
                return ErrorResp("Invalid interval format");
            }

            simul.Tick(interval);
            return new JsonResponse(JsonSerializer.Serialize(simul, defaultJsonOptions));
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
        public Response GetUpgradesOfSimulation(Request req) {
            var simulIndex = TryParseIndex(req, "simulationId", _simulations.Count, out var errorResp);
            if (simulIndex < 0) return errorResp;

            var simul = _simulations[simulIndex];
            var upgrades = simul.Upgrades;
            return new JsonResponse(JsonSerializer.Serialize(upgrades, defaultJsonOptions));
        }

        [Get("/simulations/{simulationId}/upgrades/{upgradeId}")]
        public Response GetUpgradeOfSimulation(Request req) {
            var simulIndex = TryParseIndex(req, "simulationId", _simulations.Count, out var errorResp);
            if (simulIndex < 0) return errorResp;

            var simul = _simulations[simulIndex];
            var upgrades = simul.Upgrades.ToArray();
            var upgradeIndex = TryParseIndex(req, "upgradeId", upgrades.Length, out errorResp);
            if (upgradeIndex < 0) return errorResp;

            var upgrade = upgrades[upgradeIndex];
            return new JsonResponse(JsonSerializer.Serialize(upgrade, defaultJsonOptions));
        }

        [Put("/simulations/{simulationId}/upgrades/{upgradeId}")]
        public async Task<Response> UpdateUpgradeOfSimulation(Request req) {
            var simulIndex = TryParseIndex(req, "simulationId", _simulations.Count, out var errorResp);
            if (simulIndex < 0) return errorResp;

            var simul = _simulations[simulIndex];
            var upgrades = simul.Upgrades.ToArray();
            var upgradeIndex = TryParseIndex(req, "upgradeId", upgrades.Length, out errorResp);
            if (upgradeIndex < 0) return errorResp;

            var upgrade = upgrades[upgradeIndex];

            try {
                var body = await JsonSerializer.DeserializeAsync<UpdateUpgradeRequest>(req.Body, updateReqJsonOptions);
                if (body.MaxLevel > 0) {
                    upgrade.MaxLevel = body.MaxLevel;
                }
                if (body.Prices != null) {
                    for (var i = 0; i < body.Prices.Length; i++) {
                        upgrade.SetPrice(i, body.Prices[i]);
                    }
                }
                if (body.Values != null) {
                    for (var i = 0; i < body.Values.Length; i++) {
                        upgrade.SetValue(i, body.Values[i]);
                    }
                }
            }
            catch (JsonException ex) {
                return ErrorResp("Invalid json format", ex);
            } catch (Exception ex) {
                return ErrorResp("error", ex, StatusCode.InternalServerError);
            }

            return new JsonResponse(JsonSerializer.Serialize(upgrade, defaultJsonOptions));
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

        private static JsonResponse ErrorResp(string message, Exception ex = null, StatusCode status = StatusCode.BadRequest) {
            return new JsonResponse(new JSON {
                ["message"] = message,
                ["ex"] = ex?.Message ?? "",
            }, status: status);
        }

        struct CreateSimulationRequest {
            public int ModelIndex { get; set; }
        }

        struct UpdateUpgradeRequest {
            public int MaxLevel { get; set; }
            public double[] Prices { get; set; }
            public double[] Values { get; set; }
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