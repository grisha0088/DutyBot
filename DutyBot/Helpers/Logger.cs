using System;
using System.Data.SqlClient;
using Telegram;


namespace DutyBot
{
    internal class Logger
    {
        #if DEBUG
        static string dutyBotDB = "Data Source=uk-duty01\\duty01;Initial Catalog=DutyBot_debug; Integrated Security=false; User ID=DutyBot; Password=123qwe!;";
        #else
        static string dutyBotDB = "Data Source=uk-duty01\\duty01;Initial Catalog=DutyBot; Integrated Security=false; User ID=DutyBot; Password=123qwe!;";
        #endif

        static string StringClearer(string str)
        {
             var clearstring = str.Replace("'", "''");
             return clearstring;
        }


        public static void LogOpperation(string messageTipe, int userId, string operation, string addInfo)
        {
            StringClearer(messageTipe);
            StringClearer(operation);
            StringClearer(addInfo);
            

            try
            {
                string query = @"
INSERT INTO [dbo].[Log]([messageTipe], [userId], [operation], [exception], [addInfo])
VALUES ('" + messageTipe + "', " + userId + ", '" + operation + "',  null, '" + addInfo + "' )";
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
                Sender.Send("https://api.telegram.org/bot179261100:AAGqaQ8Fum0xK8JQL0FE_N4LugS_MmO36zM/sendmessage?chat_id=38651047&text=" + "Ошибка при записи операции в лог: " + ex);
            }
        
        }

        public static void LogException(string messageTipe, int userId, string operation, string exception, string addInfo)
        {
            StringClearer(messageTipe);
            StringClearer(operation);
            exception = StringClearer(exception);
            StringClearer(addInfo);
            
            try
            {
                string query = @"
INSERT INTO [dbo].[Log]([messageTipe], [userId], [operation], [exception], [addInfo])
VALUES ('" + messageTipe + "', " + userId + ", '" + operation + "', '" + exception + "', '" + addInfo + "' )";
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
                Sender.Send("https://api.telegram.org/bot179261100:AAGqaQ8Fum0xK8JQL0FE_N4LugS_MmO36zM/sendmessage?chat_id=38651047&text=" + "Ошибка при записи ошибки в лог: " + ex);
            }
        }

    }
}
