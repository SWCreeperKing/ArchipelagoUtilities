namespace KaitoKid.ArchipelagoUtilities.Net.Client.ConnectionResults
{
    public abstract class ConnectionResult
    {
        public abstract bool Success { get; }
        public virtual string Message => Success.ToString();
    }
}
