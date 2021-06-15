namespace RabitQueueWrapper
{
    public class RabbitOptions
    {
        public const string SectionName = "RabbitOptions";
        public string HostName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}