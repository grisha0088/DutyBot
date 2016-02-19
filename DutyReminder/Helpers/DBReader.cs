﻿using System;
using System.Data.SqlClient;


namespace DutyBot
{
    class DbReader
    {
        #if DEBUG
        static readonly string dutyBotDB = "Data Source=uk-duty01\\duty01;Initial Catalog=DutyBot_debug; Integrated Security=false; User ID=DutyBot; Password=123qwe!;";

        #else
        static readonly string dutyBotDB = "Data Source=uk-duty01\\duty01;Initial Catalog=DutyBot; Integrated Security=false; User ID=DutyBot; Password=123qwe!;";
        #endif


        public static string Readbot()
        {
            string query = @"
     SELECT [ValueString]
  FROM  [dbo].[Parametrs]
  where Parametr = 'TelegramBot'";
            using (SqlConnection conn = new SqlConnection(dutyBotDB))
            {
                conn.Open();
                using (SqlCommand todc1 = new SqlCommand(query, conn))
                {
                    var a = (string)todc1.ExecuteScalar();
                    return a;
                }
            }
        }

        public static string Readticket(int userId)
        {
            var query = @"
     SELECT [TicketNumber]
  FROM  [dbo].[Users]
  where [id] = " + userId;
            using (SqlConnection conn = new SqlConnection(dutyBotDB))
            {
                conn.Open();
                using (SqlCommand todc1 = new SqlCommand(query, conn))
                {
                    var a = (string)todc1.ExecuteScalar();
                    return a;
                }
            }
        }

        public static void Updateticket(int userId, string ticket)
        {
            string query = "UPDATE [dbo].[Users] SET [TicketNumber] = '" + ticket + "' WHERE [id] =" + userId;
            using (SqlConnection conn = new SqlConnection(dutyBotDB))
            {
                conn.Open();
                using (SqlCommand todc1 = new SqlCommand(query, conn))
                {

                    todc1.ExecuteNonQuery();

                }
            }
        }


        public static string Readjira()
        {
            const string query = @"
     SELECT [ValueString]
  FROM  [dbo].[Parametrs]
  where Parametr = 'jira'";
            using (SqlConnection conn = new SqlConnection(dutyBotDB))
            {
                conn.Open();
                using (SqlCommand todc1 = new SqlCommand(query, conn))
                {
                    var a = (string)todc1.ExecuteScalar();
                    return a;
                }
            }
        }

        public static string Readdefaultuser()
        {
            const string query = @"
     SELECT [ValueString]
  FROM  [dbo].[Parametrs]
  where Parametr = 'dafaultuserlogin'";
            using (SqlConnection conn = new SqlConnection(dutyBotDB))
            {
                conn.Open();
                using (SqlCommand todc1 = new SqlCommand(query, conn))
                {
                    var a = (string)todc1.ExecuteScalar();
                    return a;
                }
            }
        }

        public static string Readdefaultpassword()
        {
            const string query = @"
     SELECT [ValueString]
  FROM  [dbo].[Parametrs]
  where Parametr = 'dafaultuserpassword'";
            using (SqlConnection conn = new SqlConnection(dutyBotDB))
            {
                conn.Open();
                using (SqlCommand todc1 = new SqlCommand(query, conn))
                {
                    var a = (string)todc1.ExecuteScalar();
                    return a;
                }
            }
        }

        public static string Readfilter()
        {
            string a;
            string query = @"
  SELECT [ValueString]
  FROM  [dbo].[Parametrs]
  where Parametr = 'Filter'";
            using (SqlConnection conn = new SqlConnection(dutyBotDB))
            {
                conn.Open();
                using (SqlCommand todc1 = new SqlCommand(query, conn))
                {

                    a = (string)todc1.ExecuteScalar();
                    return a;
                }
            }
        }


        public static DateTime Readdutyend(int userId)
        {
            var query = @"
declare @a datetime	=	
(SELECT[DutyEnd] FROM  [dbo].[Users]  where [id] = " +  userId + @")
select ISNULL(@a, '1900-01-01 00:00:00.000')";
            using (SqlConnection conn = new SqlConnection(dutyBotDB))
            {
                conn.Open();
                using (var todc1 = new SqlCommand(query, conn))
                {
                    var a = (DateTime)todc1.ExecuteScalar();

                    return a;
                }
            }
        }
            
        

        public static int[] Readresppeople()
        {
            var users = new int[20];
            using (var conn = new SqlConnection(dutyBotDB))
            {
                SqlCommand command = new SqlCommand(
                  @"SELECT [id]
                    FROM  [dbo].[Users]
                    where DutyStart <= GETDATE() and DutyEnd > GETDATE() and State = 5",
                  conn);
                conn.Open();

                SqlDataReader reader = command.ExecuteReader();
                
                var i = 0;

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        users[i] = reader.GetInt32(0);
                        i += 1;
                    }
                }
                reader.Close();
            }

            return users;
        }

        public static int[] Readallpeople()
        {
            var users = new int[20];
            using (var conn = new SqlConnection(dutyBotDB))
            {
                var command = new SqlCommand(
                  @"SELECT [id]
                    FROM  [dbo].[Users]",
                  conn);
                conn.Open();

                SqlDataReader reader = command.ExecuteReader();

                var i = 0;

                if (reader.HasRows)
                {
                   
                    while ( reader.Read())
                    {
                        users[i] = reader.GetInt32(0);
                        i += 1;
                        
                    }
                }
                reader.Close();
            }

            return users;
        }

         public static int Readrespcount()
         {
             const string query = @"SELECT count([id])
  FROM  [dbo].[Users]
  where [DutyEnd] > getdate()";
             using (var conn = new SqlConnection(dutyBotDB))
            {
                conn.Open();
                using (var todc1 = new SqlCommand(query, conn))
                {
                    var a = (int)todc1.ExecuteScalar();
                    return a;
                }
            }
         }

        public static string Readrespersone()
        {
            string query = @"
     DECLARE @namest as nvarchar(100)
    DECLARE @date as nvarchar(100)
       

  IF datepart(hour, GETDATE()) BETWEEN '5' AND '8'   SET  @namest = 'First'   
  IF datepart(hour, GETDATE()) BETWEEN '9' AND '12' SET  @namest = 'Second'  
  IF datepart(hour, GETDATE()) BETWEEN '13' AND '16' SET  @namest = 'Third'  
  IF datepart(hour, GETDATE()) BETWEEN '17' AND '20' SET  @namest = 'Fourth'  
  IF datepart(hour, GETDATE()) BETWEEN '21' AND '23' SET  @namest = 'Fifth'  
  IF datepart(hour, GETDATE()) BETWEEN '0' AND '0' SET  @namest = 'Fifth'  
  IF datepart(hour, GETDATE()) BETWEEN '1' AND '4' SET  @namest = 'Sixth'

  IF datepart(hour, GETDATE()) BETWEEN '5' AND '8'   SET  @date = CONVERT(date, GETDATE())
  IF datepart(hour, GETDATE()) BETWEEN '9' AND '12' SET  @date = CONVERT(date, GETDATE())
  IF datepart(hour, GETDATE()) BETWEEN '13' AND '16' SET  @date = CONVERT(date, GETDATE())
  IF datepart(hour, GETDATE()) BETWEEN '17' AND '20' SET  @date = CONVERT(date, GETDATE())
  IF datepart(hour, GETDATE()) BETWEEN '21' AND '23' SET  @date = CONVERT(date, GETDATE())
  IF datepart(hour, GETDATE()) BETWEEN '0' AND '0' SET  @date = CONVERT(date, GETDATE() -1)
  IF datepart(hour, GETDATE()) BETWEEN '1' AND '4' SET  @date = CONVERT(date, GETDATE() -1)


    
DECLARE @SQLText nvarchar(max)
set @SQLText = 
'SELECT UP.Name
FROM [Duty].[dbo].[Schedule_Internal] 
JOIN [Duty].dbo.UsersPreProduction UP ON '+@namest+' = UP.Name
WHERE DutyDate = '''+ @date+''''

EXEC(@SQLText)";
            using (SqlConnection conn = new SqlConnection(dutyBotDB))
            {
                conn.Open();
                using (SqlCommand todc1 = new SqlCommand(query, conn))
                {

                    var a = (string)todc1.ExecuteScalar();

                    if (a == "") a = "Сейчас никто не дежурит";
                    return a;
                }
            }
        }

        public static DateTime Readuserdutystart(int userId)
        {
            string query = @"
  declare @userid int = " + userId + @"

  declare @DutyDate datetime
  declare @First nvarchar(max)
  declare @Second nvarchar(max)
  declare @Third nvarchar(max)
  declare @Fourth nvarchar(max)
  declare @Fifth nvarchar(max)
  declare @Sixth nvarchar(max)
  declare @user nvarchar(max)
  declare @start datetime
  declare @end datetime
  declare @flag int = 0



  set @user = (select [Name] from [Duty].[dbo].[UsersPreProduction] where [TelegramNumber] = @userid)



  declare ksn cursor LOCAL for select 
      DutyDate 
         ,[First]
         ,[Second]
      ,[Third]
         ,[Fourth] 
         ,[Fifth]
      ,[Sixth]
      
   
         from [Duty].[dbo].[Schedule_Internal]
         where DutyDate >= CONVERT(date, GETDATE()) 
         open ksn
FETCH NEXT FROM ksn into @DutyDate, @First, @Second, @Third, @Fourth, @Fifth, @Sixth



WHILE @@FETCH_STATUS = 0 BEGIN


if (@First = @user and @flag = 0) 
begin
set @start = DATEADD(hour, 5, @DutyDate)
set @flag = 1
end

if (@First <> @user and @flag = 1) 
begin
set @end = DATEADD  (hour, 5, @DutyDate)
break 
end

if (@Second = @user and @flag = 0) 
begin
set @start = DATEADD(hour, 9, @DutyDate)
set @flag = 1
end

if (@Second <> @user and @flag = 1) 
begin
set @end = DATEADD(hour, 9, @DutyDate)
break 
end

if (@Third = @user and @flag = 0) 
begin
set @start = DATEADD(hour, 13, @DutyDate)
set @flag = 1
end

if (@Third <> @user and @flag = 1) 
begin
set @end = DATEADD(hour, 13, @DutyDate)
break 
end

if (@Fourth = @user and @flag = 0) 
begin
set @start = DATEADD(hour, 17, @DutyDate)
set @flag = 1
end

if (@Fourth <> @user and @flag = 1) 
begin
set @end = DATEADD(hour, 17, @DutyDate)
break 
end

if (@Fifth = @user and @flag = 0) 
begin
set @start = DATEADD(hour, 21, @DutyDate)
set @flag = 1
end

if (@Fifth <> @user and @flag = 1) 
begin
set @end = DATEADD(hour, 21, @DutyDate)
break 
end

if (@Sixth = @user and @flag = 0) 
begin
set @start = DATEADD(hour, 25, @DutyDate)
set @flag = 1
end

if (@Sixth <> @user and @flag = 1) 
begin
set @end = DATEADD(hour, 25, @DutyDate)
break 
end



FETCH NEXT FROM ksn into @DutyDate, @First, @Second, @Third, @Fourth, @Fifth, @Sixth
END


close ksn
deallocate ksn


select isnull(@start, '')      ";
            using (SqlConnection conn = new SqlConnection(dutyBotDB))
            {
                conn.Open();
                using (SqlCommand todc1 = new SqlCommand(query, conn))
                {
                    var a = (DateTime)todc1.ExecuteScalar();
                    return a;
                }
            }
        }


        public static DateTime Readuserdutyend(int userId)
        {
            string query = @"
  declare @userid int = " + userId + @"

  declare @DutyDate datetime
  declare @First nvarchar(max)
  declare @Second nvarchar(max)
  declare @Third nvarchar(max)
  declare @Fourth nvarchar(max)
  declare @Fifth nvarchar(max)
  declare @Sixth nvarchar(max)
  declare @user nvarchar(max)
  declare @start datetime
  declare @end datetime
  declare @flag int = 0



  set @user = (select [Name] from [Duty].[dbo].[UsersPreProduction] where [TelegramNumber] = @userid)



  declare ksn cursor LOCAL for select 
      DutyDate 
         ,[First]
         ,[Second]
      ,[Third]
         ,[Fourth] 
         ,[Fifth]
      ,[Sixth]
      
   
         from [Duty].[dbo].[Schedule_Internal]
         where DutyDate >= CONVERT(date, GETDATE()) 
         open ksn
FETCH NEXT FROM ksn into @DutyDate, @First, @Second, @Third, @Fourth, @Fifth, @Sixth



WHILE @@FETCH_STATUS = 0 BEGIN


if (@First = @user and @flag = 0) 
begin
set @start = DATEADD(hour, 5, @DutyDate)
set @flag = 1
end

if (@First <> @user and @flag = 1) 
begin
set @end = DATEADD  (hour, 5, @DutyDate)
break 
end

if (@Second = @user and @flag = 0) 
begin
set @start = DATEADD(hour, 9, @DutyDate)
set @flag = 1
end

if (@Second <> @user and @flag = 1) 
begin
set @end = DATEADD(hour, 9, @DutyDate)
break 
end

if (@Third = @user and @flag = 0) 
begin
set @start = DATEADD(hour, 13, @DutyDate)
set @flag = 1
end

if (@Third <> @user and @flag = 1) 
begin
set @end = DATEADD(hour, 13, @DutyDate)
break 
end

if (@Fourth = @user and @flag = 0) 
begin
set @start = DATEADD(hour, 17, @DutyDate)
set @flag = 1
end

if (@Fourth <> @user and @flag = 1) 
begin
set @end = DATEADD(hour, 17, @DutyDate)
break 
end

if (@Fifth = @user and @flag = 0) 
begin
set @start = DATEADD(hour, 21, @DutyDate)
set @flag = 1
end

if (@Fifth <> @user and @flag = 1) 
begin
set @end = DATEADD(hour, 21, @DutyDate)
break 
end

if (@Sixth = @user and @flag = 0) 
begin
set @start = DATEADD(hour, 25, @DutyDate)
set @flag = 1
end

if (@Sixth <> @user and @flag = 1) 
begin
set @end = DATEADD(hour, 25, @DutyDate)
break 
end



FETCH NEXT FROM ksn into @DutyDate, @First, @Second, @Third, @Fourth, @Fifth, @Sixth
END


close ksn
deallocate ksn


select isnull(@end, '')      ";
            using (SqlConnection conn = new SqlConnection(dutyBotDB))
            {
                conn.Open();
                using (SqlCommand todc1 = new SqlCommand(query, conn))
                {
                    var a = (DateTime)todc1.ExecuteScalar();
                    return a;
                }
            }
        }

        public static int Readuserstate(int userId)
        {
            var query = @"
   select isnull((
  SELECT [State]
  FROM  [dbo].[Users]
  where id = " + userId + "), -1)";
            using (SqlConnection conn = new SqlConnection(dutyBotDB))
            {
                conn.Open();
                using (SqlCommand todc1 = new SqlCommand(query, conn))
                {
                    var a = (int)todc1.ExecuteScalar();


                    return a;
                }
            }
        }

        public static string Readuserlogin(int userId)
        {
            var query = @"
  SELECT [Login]
  FROM  [dbo].[Users]
  where id = " + userId;
            using (var conn = new SqlConnection(dutyBotDB))
            {
                conn.Open();
                using (var todc1 = new SqlCommand(query, conn))
                {
                    var a = (string)todc1.ExecuteScalar();


                    return a;
                }
            }
        }

        public static string Readuserpassword(int userId)
        {
            var query = @"
  SELECT [Password]
  FROM  [dbo].[Users]
  where id = " + userId;
            using (var conn = new SqlConnection(dutyBotDB))
            {
                conn.Open();
                using (SqlCommand todc1 = new SqlCommand(query, conn))
                {
                    var a = (string)todc1.ExecuteScalar();


                    return a;
                }
            }
        }

        public static void Updateuserlogin(int userId, string login)
        {
            string query = @"
   UPDATE [dbo].[Users]
   SET [Login] = '" + login + 
 "' WHERE [id] =" + userId;
            using (var conn = new SqlConnection(dutyBotDB))
            {
                conn.Open();
                using (SqlCommand todc1 = new SqlCommand(query, conn))
                {

                    todc1.ExecuteNonQuery();
                }
            }
        }

        public static void Updateuserpassword(int userId, string password)
        {
            var query = @"
   UPDATE [dbo].[Users]
   SET [Password] = '" + password +
 "' WHERE [id] =" + userId;
            using (SqlConnection conn = new SqlConnection(dutyBotDB))
            {
                conn.Open();
                using (SqlCommand todc1 = new SqlCommand(query, conn))
                {
                    todc1.ExecuteNonQuery();

                }
            }
        }

        public static void Updateuserstate(int userId, int state)
        {
            string query = @"
   UPDATE [dbo].[Users]
   SET [State] =" + state +
 " WHERE [id] =" + userId;
            using (SqlConnection conn = new SqlConnection(dutyBotDB))
            {
                conn.Open();
                using (SqlCommand todc1 = new SqlCommand(query, conn))
                {

                    todc1.ExecuteNonQuery();

                }
            }
        }

        public static void Updatedutystart(int userId, DateTime starttime)
        {
            string query = "UPDATE [dbo].[Users] SET [DutyStart] ='" + starttime + "' WHERE [id] =" + userId;
            using (SqlConnection conn = new SqlConnection(dutyBotDB))
            {
                conn.Open();
                using (SqlCommand todc1 = new SqlCommand(query, conn))
                {

                    todc1.ExecuteNonQuery();

                }
            }
        }

        public static void Updatedutyend(int userId, DateTime endtime)
        {
            string query = "UPDATE [dbo].[Users] SET [DutyEnd] = '" + endtime + "' WHERE [id] =" + userId;
            using (SqlConnection conn = new SqlConnection(dutyBotDB))
            {
                conn.Open();
                using (SqlCommand todc1 = new SqlCommand(query, conn))
                {

                    todc1.ExecuteNonQuery();

                }
            }
        }


        public static void Insertuser(string username, int userId, int userstate = 0)
        {
            string query = @"
  INSERT INTO [dbo].[Users]
           ([Name]
           ,[id]
           ,[State]
           ,[Login]
           ,[Password])
     VALUES ('" + username + "', " + userId + ", " + userstate + ", ' ', ' ')";
  
            using (SqlConnection conn = new SqlConnection(dutyBotDB))
            {
                conn.Open();
                using (SqlCommand todc1 = new SqlCommand(query, conn))
                {

                    todc1.ExecuteScalar();

                }
            }
        }

        public static void Deletetuser(int userId)
        {
            string query = @"
  delete from [dbo].[Users]
  where [id] = " + userId;

            using (SqlConnection conn = new SqlConnection(dutyBotDB))
            {
                conn.Open();
                using (SqlCommand todc1 = new SqlCommand(query, conn))
                {

                    todc1.ExecuteScalar();

                }
            }
        }
    }


}

