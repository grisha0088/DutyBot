using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Telegram;


namespace DutyBot
{
    class Logger
    {
        #if DEBUG
        static string dutyBotDB = "Data Source=uk-duty01\\duty01;Initial Catalog=DutyBot_debug; Integrated Security=false; User ID=DutyBot; Password=123qwe!;";
        #else
        static string dutyBotDB = "Data Source=uk-duty01\\duty01;Initial Catalog=DutyBot; Integrated Security=false; User ID=DutyBot; Password=123qwe!;";
        #endif

        static void MessageClearer(string Message)
        {
            Message = Message.Replace("'", "''");
        }
        
        
        public static void LogOpperation(string MessageTipe, int UserID, string Operation, string AddInfo)
        {
            MessageClearer(MessageTipe);
            MessageClearer(Operation);
            MessageClearer(AddInfo);

            try
            {
                string query = @"
INSERT INTO [dbo].[Log]([MessageTipe], [UserID], [Operation], [Exception], [AddInfo])
VALUES ('" + MessageTipe + "', " + UserID + ", '" + Operation + "',  null, '" + AddInfo + "' )";
                using (SqlConnection conn = new SqlConnection(dutyBotDB))
                {
                    conn.Open();
                    using (SqlCommand todc1 = new SqlCommand(query, conn))
                    {

                        todc1.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Sender.send("https://api.telegram.org/bot179261100:AAGqaQ8Fum0xK8JQL0FE_N4LugS_MmO36zM/sendmessage?chat_id=38651047&text=" + "Ошибка при записи операции в лог: " + ex, "");
            }
        
        }

        public static void LogException(string MessageTipe, int UserID, string Operation, string Exception, string AddInfo)
        {
            MessageClearer(MessageTipe);
            MessageClearer(Operation);
            MessageClearer(Exception);
            MessageClearer(AddInfo);
            
            try
            {
                string query = @"
INSERT INTO [dbo].[Log]([MessageTipe], [UserID], [Operation], [Exception], [AddInfo])
VALUES ('" + MessageTipe + "', " + UserID + ", '" + Operation + "', '" + Exception + "', '" + AddInfo + "' )";
                using (SqlConnection conn = new SqlConnection(dutyBotDB))
                {
                    conn.Open();
                    using (SqlCommand todc1 = new SqlCommand(query, conn))
                    {

                        todc1.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Sender.send("https://api.telegram.org/bot179261100:AAGqaQ8Fum0xK8JQL0FE_N4LugS_MmO36zM/sendmessage?chat_id=38651047&text=" + "Ошибка при записи ошибки в лог: " + ex, "");
            }
        }

    }
}
