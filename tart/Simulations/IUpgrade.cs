using System.Collections.Generic;

namespace tart.Simulations {
    public interface IUpgrade {
        int MaxLevel { get; set; }
        int Type { get; }

        double GetPrice(int x);
        double GetValue(int x);
        void SetPrice(int x, double value);
        void SetValue(int x, double value);

        IEnumerable<double> Prices { get; }

        IEnumerable<double> Values { get; }
    }
}