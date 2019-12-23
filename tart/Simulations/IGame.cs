using System;
using System.Collections.Generic;

namespace tart.Simulations {
    public interface IGame {
        double Money { get; }
        double MoneyInc { get; }
        double MoneyDec { get; }

        float Time { get; }

        IReadOnlyCollection<IUpgrade> Upgrades { get; }
        IStats<IGame> GetStats();
        IEnumerable<IChoice<IGame>> GetAvailableChoices();
    }
}