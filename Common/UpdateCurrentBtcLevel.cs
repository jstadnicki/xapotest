namespace Common
{
    public class UpdateCurrentBtcLevel
    {
        public decimal CurrentLevel { get; set; }
        public static string QueueName => "UpdateCurrentBtcLevel";
    }
}