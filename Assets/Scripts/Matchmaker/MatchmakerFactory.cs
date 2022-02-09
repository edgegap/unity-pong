public class ArbitriumMatchmakerFactory : IMatchmakerFactory
{
    public IMatchmaker CreateMatchmaker()
    {
        return new ArbitriumMatchmaker(new ArbitriumMatchmakerConfiguration());
    }
}
