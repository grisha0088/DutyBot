using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace DutyBot
{
    public class DutyBotDbContext : DbContext
    {
        public DutyBotDbContext(): base("DbConnection"){ }

        public DbSet<User> Users { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Parametr> Parametrs { get; set; }
    }
}
