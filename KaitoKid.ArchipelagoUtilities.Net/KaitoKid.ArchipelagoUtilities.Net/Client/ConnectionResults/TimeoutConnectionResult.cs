namespace KaitoKid.ArchipelagoUtilities.Net.Client.ConnectionResults
{
    public class TimeoutConnectionResult : FailedConnectionResult
    {
        public override bool RetryPossible => true;

        public TimeoutConnectionResult() : this("Connection timed out")
        {
        }

        public TimeoutConnectionResult(string errorMessage) : base(errorMessage)
        {
        }
    }
}
