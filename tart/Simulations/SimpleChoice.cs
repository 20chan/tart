using System;

namespace tart.Simulations {
    public class SimpleChoice<TGame, TKind> : IChoice<TGame> where TGame : IGame where TKind : Enum {
        public TGame Game { get; }
        public float Time { get; }

        public TKind Kind { get; }
        public double Price { get; }
        public int Level { get; }

        public bool Available => Game.Money >= Price;

        public IStats<TGame> CurrentStats { get; }
        public IStats<TGame> NextStats { get; }

        public SimpleChoice(TGame game, float time, TKind kind, double price, int level, IStats<TGame> current, IStats<TGame> next) {
            Game = game;
            Time = time;
            Kind = kind;
            Price = price;
            Level = level;

            CurrentStats = current;
            NextStats = next;
        }

        int IChoice<TGame>.Type => Convert.ToInt32(Kind);
    }
}