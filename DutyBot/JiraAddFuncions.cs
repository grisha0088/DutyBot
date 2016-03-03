using System;
using Atlassian.Jira;
using Telegram;

namespace DutyBot

{
    static class JiraAddFuncions
    {
        public static void AssingTicket(User user, Issue issue, Message message, string assignee, TelegramBot bot, Jira jiraConn, string keyboard = "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}")
        {
            try
            {
                issue.Refresh();
                if (issue.Assignee == null & issue.Key.Value.Equals(user.TicketNumber))
                {
                    if (issue.Status.ToString() == "10050")
                    {
                        issue.WorkflowTransition("Распределить");
                    }

                    issue.Assignee = assignee;
                    issue.SaveChanges();

                    user.State -= 1; //безумный костыль для того, чтобы вычислять статус, который нужно перевсети пользоваетля. Так получилось, что это 3 для 4 статуса, и 5 для 6 статуса. 
                    user.TicketNumber = "";
                    bot.SendMessage(message.chat.id, "Готово.", keyboard);
                }
                else
                {
                    user.State -= 1;
                    user.TicketNumber = "";
                    bot.SendMessage(message.chat.id, "Тикет уже распределён", keyboard);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("error", message.chat.id, "AssingTicket", ex.GetType() + ": " + ex.Message, issue.Key.Value);
                user.State -= 1;
                user.TicketNumber = "";
                bot.SendMessage(message.chat.id, "Что-то пошло не так.", "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");

            }
        }

        public static void ResolveTicket(User user, Issue issue, Message message, string assignee, TelegramBot bot, Jira jiraConn, string keyboard = "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}")
        {
            int state = user.State - 1; //безумный костыль для того, чтобы вычислять статус, который нужно перевсети пользоваетля. Так получилось, что это 3 для 4 статуса, и 5 для 6 статуса. 
            try
            {
                issue.Refresh();
                if (issue.Assignee == null & issue.Key.ToString().Equals(user.TicketNumber))
                {
                    if (issue.Status.ToString() != "5")
                    {
                        issue.WorkflowTransition("Решить");
                    }

                    issue.Assignee = assignee;
                    issue.SaveChanges();

                    using (var db = new DutyBotDbContext())
                    {
                        user = db.Users.Find(user.Id);
                        user.State = state;
                        user.TicketNumber = "";
                        db.SaveChanges();
                    }

                    bot.SendMessage(message.chat.id, "Готово", keyboard);
                }
                else
                {
                    using (var db = new DutyBotDbContext())
                    {
                        user = db.Users.Find(user.Id);
                        user.State = state;
                        user.TicketNumber = "";
                        db.SaveChanges();
                    }

                    bot.SendMessage(message.chat.id, "Тикет уже распределён", keyboard);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("error", message.chat.id, "ResolveTicket", ex.GetType() + ": " + ex.Message, issue.Key.Value);
                using (var db = new DutyBotDbContext())
                {
                    user = db.Users.Find(user.Id);
                    user.State = state;
                    db.SaveChanges();
                }
                bot.SendMessage(message.chat.id, "Что-то пошло не так.", "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");

            }
        }

        public static bool CheckConnection(Jira jiraConn, string login, string password)
        {
            if (jiraConn == null) throw new ArgumentNullException(nameof(jiraConn));
            if (login == null) throw new ArgumentNullException(nameof(login));
            if (password == null) throw new ArgumentNullException(nameof(password));

            jiraConn = Jira.CreateRestClient("https://jira.2gis.ru/", login, password);
            try
            {
                var issues = jiraConn.GetFilters();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
