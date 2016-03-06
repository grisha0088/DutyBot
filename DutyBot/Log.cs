﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public class NotDelete : System.Attribute
    {
        public bool notDelete;

        public NotDelete(bool v)
        {
            this.notDelete = v;
        }

     
    }

}
