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

        IReadOnlyCollection<IUpgrade> Upgrades { get; }
        IReadOnlyCollection<int> Levels { get; }
        IStats<IGame> Stats { get; }
        IEnumerable<IChoice<IGame>> GetAvailableChoices();
        IEnumerable<IChoice<IGame>> GetChoiceHistory();

        [JsonIgnore]
        Type UpgradeType { get; }
    }
}