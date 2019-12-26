using System;
using System.Collections.Generic;
using System.Linq;

namespace tart.Simulations.Models.Port {
    public class IdlePort : IGame {
        public double Money;
        public double MoneyInc, MoneyDec;
        public float Time;

        public Lane[] Lanes;
        public List<Choice> ChoiceHistory = new List<Choice>();

        public IdlePort(int laneCount) {
            Lanes = new Lane[laneCount];
            for (var i = 0; i < laneCount; i++) {
                Lanes[i] = new Lane(i);
            }
        }

        private Stats GetStats() {
            throw new NotImplementedException();
        }

        public void Tick(float deltaTime) {
            throw new NotImplementedException();
        }

        private IEnumerable<Choice> GetAvailableUpgrades() {
            throw new NotImplementedException();
        }

        private bool TryUpgrade(Choice choice) {
            throw new NotImplementedException();
        }

        double IGame.Money => Money;
        double IGame.MoneyInc => MoneyInc;
        double IGame.MoneyDec => MoneyDec;
        float IGame.Time => Time;

        IEnumerable<IUpgrade> IGame.Upgrades => Lanes.SelectMany(l => l.Upgrades);
        IEnumerable<int> IGame.Levels => Lanes.SelectMany(l => l.Levels);
        IStats IGame.Stats => GetStats();
        IEnumerable<IChoice<IGame>> IGame.GetAvailableChoices() {
            return GetAvailableUpgrades();
        }

        IEnumerable<IChoice<IGame>> IGame.GetChoiceHistory() => ChoiceHistory;

        bool IGame.TryChoice(IChoice<IGame> choice) {
            if (choice is Choice ch) {
                return TryUpgrade(ch);
            }
            return false;
        }

        Type IGame.UpgradeType => typeof(PortResources);

        public class Upgrade : SimpleUpgrade<PortResources> {
            public int Lane { get; }

            public Upgrade(int lane, int level, PortResources type) : base(level, type) {
                Lane = lane;
            }
        }

        public class Choice : SimpleChoice<IdlePort, PortResources> {
            public int Lane { get; }

            public Choice(IdlePort game, float time, int lane, PortResources kind, double price, int level, Stats current, Stats next) : base(game, time, kind, price, level, current, next) {
                Lane = lane;
            }
        }

        public class Lane {
            public Upgrade[] Upgrades;
            public int[] Levels;

            public Lane(int index) {
                var types = (PortResources[])Enum.GetValues(typeof(PortResources));

                Levels = new int[types.Length];
                Upgrades = new Upgrade[types.Length];
                for (var i = 0; i < types.Length; i++) {
                    var defaultLevel = types[i] <= PortResources.BuyTrains ? 1 : 10;
                    Upgrades[i] = new Upgrade(index, defaultLevel, types[i]);
                }
            }

            public ref Upgrade this[PortResources p] => ref Upgrades[(int)p];
        }

        public struct Stats : IStats {
            public double Money { get; set; }
            public double MoneyPerSecond { get; set; }
        }
    }

    public enum PortResources {
        BuyLane,

        BuyShipCrane,
        BuyTrucks,
        BuyYardCranes,
        BuyShips,
        BuyYard,
        BuyTrains,

        ShipCraneSpeed,
        ShipCraneMoney,
        TruckSpeed,
        TruckSpawn,
        TruckLevel,
        TruckMoney,
        YardCraneSpeed,
        YardCraneMoney,
        ShipSpeed,
        ShipLevel,
        ShipMoney,
        YardInterval,
        YardMoney,
        YardLevel,
        TrainSpeed,
        TrainSpawn,
        TrainLevel,
        TrainMoney,
    }
}