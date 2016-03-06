using System;

namespace DutyBot
{
    [NotDelete(true)]
    public class Log
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string MessageTipe { get; set; }
        public int UserId { get; set; }
        public string Operation { get; set; }
        public string Exception { get; set; }
        public string AddInfo { get; set; }
    }

    public class NotDelete : Attribute
    {
        public bool notDelete { get; }

        public NotDelete(bool v)
        {
            notDelete = v;
        }
    }
}
