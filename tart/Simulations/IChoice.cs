using System;

namespace tart.Simulations {
    public interface IChoice<out T> where T : IGame {
        T Game { get; }
        float Time { get; }

        double Price { get; }
        Enum Type { get; }

        IStats<T> CurrentStats { get; }
        IStats<T> NextStats { get; }
    }
}