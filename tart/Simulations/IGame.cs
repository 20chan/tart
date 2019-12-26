using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace tart.Simulations {
    public interface IGame {
        double Money { get; }
        double MoneyInc { get; }
        double MoneyDec { get; }

        float Time { get; }

        void Tick(float interval);
        bool TryChoice(IChoice<IGame> choice);

        IEnumerable<IUpgrade> Upgrades { get; }
        IEnumerable<int> Levels { get; }
        IStats Stats { get; }
        IEnumerable<IChoice<IGame>> GetAvailableChoices();
        IEnumerable<IChoice<IGame>> GetChoiceHistory();

        [JsonIgnore]
        Type UpgradeType { get; }
    }
}