namespace tart.Simulations {
    public interface IUpgrade {
        int MaxLevel { get; }

        double Price(int x);
        double Value(int x);
    }
}