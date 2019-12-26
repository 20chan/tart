using System;

namespace tart.Simulations {
    public class SimpleChoice<TGame, TKind> : IChoice<TGame> where TGame : IGame where TKind : Enum {
        public TGame Game { get; }
        public float Time { get; }

        public TKind Kind { get; }
        public double Price { get; }
        public int Level { get; }

        public bool Available => Game.Money >= Price;

        public IStats CurrentStats { get; }
        public IStats NextStats { get; }

        public SimpleChoice(TGame game, float time, TKind kind, double price, int level, IStats current, IStats next) {
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