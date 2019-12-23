namespace tart.Simulations {
    public interface IStats<out T> where T : IGame {
        double Money { get; }
        double MoneyPerSecond { get; }
    }
}