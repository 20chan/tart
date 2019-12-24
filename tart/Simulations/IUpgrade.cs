using System.Collections.Generic;

namespace tart.Simulations {
    public interface IUpgrade {
        int MaxLevel { get; }
        int Type { get; }

        double Price(int x);
        double Value(int x);

        IEnumerable<double> Prices { get; }

        IEnumerable<double> Values { get; }
    }
}