using System;
using System.Text.Json.Serialization;

namespace tart.Simulations {
    public interface IChoice<out T> where T : IGame {
        [JsonIgnore]
        T Game { get; }
        float Time { get; }

        double Price { get; }
        int Type { get; }
        int Level { get; }
        bool Available { get; }

        IStats CurrentStats { get; }
        IStats NextStats { get; }
    }
}