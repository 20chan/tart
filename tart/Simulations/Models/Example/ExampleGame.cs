using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace tart.Simulations.Models.Example {
    public class ExampleGame : IGame {
        public double Money;
        public double MoneyInc, MoneyDec;
        public float Time;

        public IUpgrade[] Upgrades = {
            new SimpleUpgrade(10, ExampleKind.Speed),
            new SimpleUpgrade(20, ExampleKind.Money),
        };

        public int[] Levels = {
            0, // speed
            0, // money
        };

        public ref IUpgrade SpeedUpgrade => ref Upgrades[0];
        public ref IUpgrade MoneyUpgrade => ref Upgrades[1];

        public ref int SpeedLevel => ref Levels[0];
        public ref int MoneyLevel => ref Levels[1];

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
            return SpeedUpgrade.GetValue(speedLevel) * MoneyUpgrade.GetValue(moneyLevel);
        }

        private ref int GetLevelOf(ExampleKind kind) {
            if (kind == ExampleKind.Speed) return ref SpeedLevel;
            return ref MoneyLevel;
        }

        private IUpgrade GetUpgradeOf(ExampleKind kind) {
            if (kind == ExampleKind.Speed) return SpeedUpgrade;
            if (kind == ExampleKind.Money) return MoneyUpgrade;
            return null;
        }

        private Stats GetStatsAfterUpgrade(ExampleKind kind) {
            var price = GetUpgradeOf(kind).GetPrice(GetLevelOf(kind));
            var speedLevel = kind == ExampleKind.Speed ? SpeedLevel + 1 : SpeedLevel;
            var moneyLevel = kind == ExampleKind.Money ? MoneyLevel + 1 : MoneyLevel;
            return GetStats(Money - price, speedLevel, moneyLevel);
        }

        private IEnumerable<Choice> GetAvailableUpgrades() {
            var stats = GetStats();

            var kinds = new[] { ExampleKind.Speed, ExampleKind.Money };
            foreach (var kind in kinds) {
                var level = GetLevelOf(kind);
                var upgrade = GetUpgradeOf(kind);
                if (level < upgrade.MaxLevel) {
                    yield return new Choice(this, Time, kind, upgrade.GetPrice(level), level, stats, GetStatsAfterUpgrade(kind));
                }
            }
        }

        private bool TryUpgrade(Choice choice) {
            if (Money < choice.Price) {
                return false;
            }
            ref var level = ref GetLevelOf(choice.UpgradeKind);
            if (level >= GetUpgradeOf(choice.UpgradeKind).MaxLevel) {
                return false;
            }

            Money -= choice.Price;
            MoneyDec -= choice.Price;
            level++;
            return true;
        }

        double IGame.Money => Money;
        double IGame.MoneyInc => MoneyInc;
        double IGame.MoneyDec => MoneyDec;

        float IGame.Time => Time;

        IReadOnlyCollection<IUpgrade> IGame.Upgrades => Upgrades;
        IReadOnlyCollection<int> IGame.Levels => Levels;
        IStats<IGame> IGame.GetStats() {
            return GetStats();
        }
        IEnumerable<IChoice<IGame>> IGame.GetAvailableChoices() {
            return GetAvailableUpgrades();
        }

        bool IGame.TryChoice(IChoice<IGame> choice) {
            if (choice is Choice ch) {
                return TryUpgrade(ch);
            }
            return false;
        }

        Type IGame.UpgradeType => typeof(ExampleKind);

        public class SimpleUpgrade : IUpgrade {
            public int MaxLevel {
                get => Prices.Count;
                set {
                    if (value < 1) value = 1;
                    while (value > Values.Count) {
                        Prices.Add(0);
                        Values.Add(0);
                    }
                    while (value < Values.Count) {
                        Prices.RemoveAt(Prices.Count - 1);
                        Values.RemoveAt(Values.Count - 1);
                    }
                }
            }
            public List<double> Prices, Values;
            public ExampleKind Kind;

            public SimpleUpgrade(int level, ExampleKind kind) {
                Prices = new List<double>(Enumerable.Repeat<double>(0, level));
                Values = new List<double>(Enumerable.Repeat<double>(0, level + 1));
                Kind = kind;
            }

            public double GetPrice(int x) {
                return Prices[x];
            }

            public double GetValue(int x) {
                return Values[x];
            }

            public void SetPrice(int x, double value) {
                if (x < 0 || Prices.Count <= x) return;
                Prices[x] = value;
            }

            public void SetValue(int x, double value) {
                if (x < 0 || Values.Count <= x) return;
                Values[x] = value;
            }

            int IUpgrade.Type => (int)Kind;
            IEnumerable<double> IUpgrade.Prices => Prices;
            IEnumerable<double> IUpgrade.Values => Values;
        }

        public class Choice : IChoice<ExampleGame> {
            public ExampleGame Game;
            public float Time;

            public ExampleKind UpgradeKind;
            public double Price;
            public int CurrentLevel;

            public Stats CurrentStats, NextStats;

            public Choice(ExampleGame game, float time, ExampleKind kind, double price, int level, Stats current, Stats next) {
                Game = game;
                Time = time;
                UpgradeKind = kind;
                Price = price;
                CurrentLevel = level;

                CurrentStats = current;
                NextStats = next;
            }

            ExampleGame IChoice<ExampleGame>.Game => Game;
            float IChoice<ExampleGame>.Time => Time;
            double IChoice<ExampleGame>.Price => Price;
            int IChoice<ExampleGame>.Type => (int)UpgradeKind;
            int IChoice<ExampleGame>.Level => CurrentLevel;

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