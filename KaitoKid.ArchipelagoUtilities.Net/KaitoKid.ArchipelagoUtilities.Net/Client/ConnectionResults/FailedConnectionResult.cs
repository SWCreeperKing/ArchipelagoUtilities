namespace KaitoKid.ArchipelagoUtilities.Net.Client.ConnectionResults
{
    public class FailedConnectionResult : ConnectionResult
    {
        public override bool Success => false;
        public override string Message { get; }
        public virtual bool RetryPossible => false;

        public FailedConnectionResult(string errorMessage)
        {
            Message = errorMessage;
        }

    }
}
