using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;


namespace DutyBot
{
    class Logger
    {
        #if DEBUG
        static string dutyBotDB = "Data Source=uk-duty01\\duty01;Initial Catalog=DutyBot_debug; Integrated Security=false; User ID=DutyBot; Password=123qwe!;";
        #else
        static string dutyBotDB = "Data Source=uk-duty01\\duty01;Initial Catalog=DutyBot; Integrated Security=false; User ID=DutyBot; Password=123qwe!;";
        #endif

        public static void LogOpperation(string MessageTipe, int UserID, string Operation)
        {

            string query = @"
INSERT INTO [dbo].[Log]([MessageTipe], [UserID], [Operation], [Exception])
VALUES ('" + MessageTipe + "', " + UserID + ", '" + Operation + "',  null )";
            using (SqlConnection conn = new SqlConnection(dutyBotDB))
            {
                conn.Open();
                using (SqlCommand todc1 = new SqlCommand(query, conn))
                {

                    todc1.ExecuteNonQuery();
                }
            }
        
        }

        public static void LogException(string MessageTipe, int UserID, string Operation, string Exception)
        {

            string query = @"
INSERT INTO [dbo].[Log]([MessageTipe], [UserID], [Operation], [Exception])
VALUES ('" + MessageTipe + "', " + UserID + ", '" + Operation + "', '" + Exception + "')";
            using (SqlConnection conn = new SqlConnection(dutyBotDB))
            {
                conn.Open();
                using (SqlCommand todc1 = new SqlCommand(query, conn))
                {

                    todc1.ExecuteNonQuery();
                }
            }

        }

    }
}
