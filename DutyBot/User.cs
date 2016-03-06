using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.SqlClient;

namespace DutyBot
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TlgNumber { get; set; }
        public int State { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public DateTime? DutyStart { get; set; }
        public DateTime? DutyEnd { get; set; }
        public string TicketNumber { get; set; }
    }
}
