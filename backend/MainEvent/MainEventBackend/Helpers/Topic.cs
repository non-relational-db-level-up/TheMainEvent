using System.Drawing.Printing;

namespace MainEvent.Helpers
{
    public interface Itopic
    {
        string topic { get; set; }
        DateTime endDate { get; set;}
    };
    public class Topic: Itopic
    {
        private string _topic;
        private DateTime _dateTime;

        public string topic { get => _topic; set => _topic = value; }
        public DateTime endDate { get => _dateTime;  set => _dateTime = value; }
    }
}
