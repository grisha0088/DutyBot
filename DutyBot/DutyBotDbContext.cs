using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace DutyBot
{
    class DutyBotDbContext : DbContext
    {
        public DutyBotDbContext(): base("DbConnection"){ }

        public DbSet<User> Users { get; set; }
    }
}
