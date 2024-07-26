namespace MainEvent.Helpers
{
    // Yes this should be better
    public interface Itopic
    {
        string topic { get; set; }
        DateTime endTime { get; set;}
    };
    public class Topic: Itopic
    {
        private string _topic;
        private DateTime _endTime;

        public string topic { get => _topic; set => _topic = value; }
        public DateTime endTime { get => _endTime;  set => _endTime = value; }
    }
}
