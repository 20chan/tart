using System;
using System.Collections.Generic;

namespace tart.Simulations.Models.Example {
    public class ExampleGame : IGame {
        public double Money;
        public double MoneyInc, MoneyDec;
        public float Time;

        public IUpgrade[] Upgrades = {
            new SimpleUpgrade(10), // speed
            new SimpleUpgrade(10), // money
        };

        public ref IUpgrade SpeedUpgrade => ref Upgrades[0];
        public ref IUpgrade MoneyUpgrade => ref Upgrades[0];

        public int SpeedLevel;
        public int MoneyLevel;

        private void Income(double amount) {
            Money += amount;
            MoneyInc += amount;
        }

        public void Tick(float deltaTime) {
            Time += deltaTime;
        }

        private Stats GetStats() {
            return GetStats(Money, SpeedLevel, MoneyLevel);
        }

        private Stats GetStats(double money, int speedLevel, int moneyLevel) {
            return new Stats {
                Money = money,
                SpeedLevel = speedLevel,
                MoneyLevel = moneyLevel,
                MoneyPerSecond = GetMoneyPerSecond(speedLevel, moneyLevel),
            };
        }

        private double GetMoneyPerSecond(int speedLevel, int moneyLevel) {
            return SpeedUpgrade.Value(speedLevel) * MoneyUpgrade.Value(moneyLevel);
        }

        private int GetLevelOf(ExampleKind kind) {
            if (kind == ExampleKind.Speed) return SpeedLevel;
            if (kind == ExampleKind.Money) return MoneyLevel;
            return -1;
        }

        private IUpgrade GetUpgradeOf(ExampleKind kind) {
            if (kind == ExampleKind.Speed) return SpeedUpgrade;
            if (kind == ExampleKind.Money) return MoneyUpgrade;
            return null;
        }

        private Stats GetStatsAfterUpgrade(ExampleKind kind) {
            var price = GetUpgradeOf(kind).Price(GetLevelOf(kind));
            var speedLevel = kind == ExampleKind.Speed ? SpeedLevel + 1 : SpeedLevel;
            var moneyLevel = kind == ExampleKind.Money ? MoneyLevel + 1 : MoneyLevel;
            return GetStats(Money - price, speedLevel, moneyLevel);
        }

        private IEnumerable<Upgrade> GetAvailableUpgrades() {
            var stats = GetStats();

            var kinds = new[] { ExampleKind.Speed, ExampleKind.Money };
            foreach (var kind in kinds) {
                var level = GetLevelOf(kind);
                var upgrade = GetUpgradeOf(kind);
                if (level < upgrade.MaxLevel) {
                    yield return new Upgrade(this, Time, kind, upgrade.Price(level), stats, GetStatsAfterUpgrade(kind));
                }
            }
        }

        double IGame.Money => Money;
        double IGame.MoneyInc => MoneyInc;
        double IGame.MoneyDec => MoneyDec;

        float IGame.Time => Time;

        IReadOnlyCollection<IUpgrade> IGame.Upgrades => Upgrades;
        IStats<IGame> IGame.GetStats() {
            return GetStats();
        }
        IEnumerable<IChoice<IGame>> IGame.GetAvailableChoices() {
            return GetAvailableUpgrades();
        }

        Type IGame.UpgradeType => typeof(ExampleKind);

        public class SimpleUpgrade : IUpgrade {
            public int MaxLevel => Values.Length;
            public double[] Prices, Values;

            public SimpleUpgrade(int level) {
                Prices = new double[level - 1];
                Values = new double[level];
            }

            public double Price(int x) {
                return Prices[x];
            }

            public double Value(int x) {
                return Values[x];
            }
        }

        public class Upgrade : IChoice<ExampleGame> {
            public ExampleGame Game;
            public float Time;

            public ExampleKind UpgradeKind;
            public double Price;

            public Stats CurrentStats, NextStats;

            public Upgrade(ExampleGame game, float time, ExampleKind kind, double price, Stats current, Stats next) {
                Game = game;
                Time = time;
                UpgradeKind = kind;
                Price = price;

                CurrentStats = current;
                NextStats = next;
            }

            ExampleGame IChoice<ExampleGame>.Game => Game;
            float IChoice<ExampleGame>.Time => Time;
            double IChoice<ExampleGame>.Price => Price;
            int IChoice<ExampleGame>.Type => (int)UpgradeKind;

            IStats<ExampleGame> IChoice<ExampleGame>.CurrentStats => CurrentStats;
            IStats<ExampleGame> IChoice<ExampleGame>.NextStats => NextStats;
        }

        public struct Stats : IStats<ExampleGame> {
            public double Money { get; set; }
            public double MoneyPerSecond { get; set; }
            public int SpeedLevel { get; set; }
            public int MoneyLevel { get; set; }
        }
    }

    public enum ExampleKind {
        Speed,
        Money,
    }
}