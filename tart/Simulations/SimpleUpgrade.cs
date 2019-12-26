using System;
using System.Collections.Generic;
using System.Linq;

namespace tart.Simulations {
    public class SimpleUpgrade<TKind> : IUpgrade where TKind : Enum {
        public List<double> Prices, Values;
        public TKind Kind { get; set; }

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

        public SimpleUpgrade(int level, TKind kind) {
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

        int IUpgrade.Type => Convert.ToInt32(Kind);
        IEnumerable<double> IUpgrade.Prices => Prices;
        IEnumerable<double> IUpgrade.Values => Values;
    }
}