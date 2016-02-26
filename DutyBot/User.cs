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
        public void Save()
        {
            using (var db = new DutyBotDbContext())
            {
                User user = db.Users.FirstOrDefault(u => u.TlgNumber == TlgNumber);
                if (user == null)
                {
                    user = new User
                    {
                        DutyEnd = DutyEnd,
                        DutyStart = DutyStart,
                        Login = Login,
                        Name = Name,
                        Password = Password,
                        State = State,
                        TicketNumber = TicketNumber,
                        TlgNumber = TlgNumber
                    };
                    db.Users.Add(user);
                    db.SaveChanges();
                }
                else
                {
                    user.DutyEnd = DutyEnd;
                    user.DutyStart = DutyStart;
                    user.Login = Login;
                    user.Name = Name;
                    user.Password = Password;
                    user.State = State;
                    user.TicketNumber = TicketNumber;
                    user.TlgNumber = TlgNumber;
                    db.SaveChanges();
                }
            }
        }
        public void Remove()
        {
            using (var db = new DutyBotDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.TlgNumber == TlgNumber);
                db.Users.Remove(user);
                db.SaveChanges();
            }
        }
        public static List<User> GetUsers()
        {
            using (var db = new DutyBotDbContext())
            { 
                var users = new List<User>(db.Users);
                return users;
            }
        }  
    }
}
