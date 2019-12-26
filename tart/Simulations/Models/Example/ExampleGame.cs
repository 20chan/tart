using System;
using System.Collections.Generic;
using Upgrade = tart.Simulations.SimpleUpgrade<tart.Simulations.Models.Example.ExampleKind>;
using Choice = tart.Simulations.SimpleChoice<tart.Simulations.Models.Example.ExampleGame, tart.Simulations.Models.Example.ExampleKind>;

namespace tart.Simulations.Models.Example {
    public class ExampleGame : IGame {
        public double Money;
        public double MoneyInc, MoneyDec;
        public float Time;

        public IUpgrade[] Upgrades = {
            new Upgrade(10, ExampleKind.Speed),
            new Upgrade(20, ExampleKind.Money),
        };

        public int[] Levels = {
            0, // speed
            0, // money
        };

        public List<Choice> ChoiceHistory = new List<Choice>();

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

            Income(deltaTime * SpeedUpgrade.GetValue(SpeedLevel) * MoneyUpgrade.GetValue(MoneyLevel) / 1000f);
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
            ref var level = ref GetLevelOf(choice.Kind);
            if (level >= GetUpgradeOf(choice.Kind).MaxLevel) {
                return false;
            }

            ChoiceHistory.Add(choice);

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
        IStats<IGame> IGame.Stats => GetStats();
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

        Type IGame.UpgradeType => typeof(ExampleKind);

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