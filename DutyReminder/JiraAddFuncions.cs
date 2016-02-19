using System;
using Atlassian.Jira;
using Telegram;

namespace DutyBot

{
    static class JiraAddFuncions
    {
        public static void AssingTicket(Issue issue, Message message, string user, TelegramBot bot, Jira jiraConn, string keyboard = "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}")
        {
            try
            {
                    issue.Refresh();
                    if (issue.Assignee == null & issue.Key.Value.Equals(DbReader.Readticket(message.chat.id)))
                    {
                        if (issue.Status.ToString() == "10050")
                        {
                            issue.WorkflowTransition("Распределить");
                        }

                        issue.Assignee = user;
                        issue.SaveChanges();


                        DbReader.Updateticket(message.chat.id, " ");
                        DbReader.Updateuserstate(message.chat.id, 3);
                        bot.SendMessage(message.chat.id, "Готово.", keyboard);
                    }
                    else
                    {
                        DbReader.Updateticket(message.chat.id, " ");
                        DbReader.Updateuserstate(message.chat.id, 3);

                        bot.SendMessage(message.chat.id, "Тикет уже распределён", keyboard);
                    }

            }
            catch (Exception ex)
            {
                Logger.LogException("error", message.chat.id, "AssingTicket", ex.GetType() + ": " + ex.Message, issue.Key.Value);
                DbReader.Updateuserstate(message.chat.id, 3);
                bot.SendMessage(message.chat.id, "Что-то пошло не так.", "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");

            }
        }

        public static void AssingTicket6(Issue issue, Message message, string user, TelegramBot bot, Jira jiraConn)
        {
            try
            {
                issue.Refresh();

                    if (issue.Assignee == null & issue.Key.ToString().Equals(DbReader.Readticket(message.chat.id)))
                    {
                        if (issue.Status.ToString() == "10050")
                        {
                            issue.WorkflowTransition("Распределить");
                        }

                        issue.Assignee = user;
                        issue.SaveChanges();

                        DbReader.Updateticket(message.chat.id, " ");
                        DbReader.Updateuserstate(message.chat.id, 5);

                        bot.SendMessage(message.chat.id, "Готово. Продолжаю мониторинг.");
                    }
                    else
                    {
                        DbReader.Updateticket(message.chat.id, " ");
                        DbReader.Updateuserstate(message.chat.id, 5);

                        bot.SendMessage(message.chat.id, "Тикет уже распределён. Продолжаю мониторинг.");
                    }

            }
            catch (Exception ex)
            {
                Logger.LogException("error", message.chat.id, "AssingTicket6", ex.GetType() + ": " + ex.Message, issue.Key.Value);
                DbReader.Updateticket(message.chat.id, " ");
                DbReader.Updateuserstate(message.chat.id, 5);
                bot.SendMessage(message.chat.id, "Что-то пошло не так. Возможно, тикет был изменён кем-то еще. Продолжаю мониторинг.");

            }
        }

        public static void ResolveTicket(Issue issue, Message message, string user, TelegramBot bot, Jira jiraConn, string keyboard = "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}")
        {
            try
            {
                issue.Refresh();
                if (issue.Assignee == null & issue.Key.ToString().Equals(DbReader.Readticket(message.chat.id)))
                {
                    if (issue.Status.ToString() != "5")
                    {
                        issue.WorkflowTransition("Решить");
                    }

                    issue.Assignee = user;
                    issue.SaveChanges();

                    DbReader.Updateticket(message.chat.id, " ");
                    DbReader.Updateuserstate(message.chat.id, 3);
                    bot.SendMessage(message.chat.id, "Готово", keyboard);
                }
                else
                {
                    DbReader.Updateticket(message.chat.id, " ");
                    DbReader.Updateuserstate(message.chat.id, 3);

                    bot.SendMessage(message.chat.id, "Тикет уже распределён", keyboard);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("error", message.chat.id, "ResolveTicket", ex.GetType() + ": " + ex.Message, issue.Key.Value);
                DbReader.Updateuserstate(message.chat.id, 3);
                bot.SendMessage(message.chat.id, "Что-то пошло не так.", "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");

            }
        }

        public static void ResolveTicket6(Issue issue, Message message, string user, TelegramBot bot, Jira jiraConn)
        {
            try
            {
                issue.Refresh();

                if (issue.Assignee == null & issue.Key.ToString().Equals(DbReader.Readticket(message.chat.id)))
                {
                    if (issue.Status.ToString() != "5")
                    {
                        issue.WorkflowTransition("Решить");
                    }

                    issue.Assignee = user;
                    issue.SaveChanges();

                    DbReader.Updateticket(message.chat.id, " ");
                    DbReader.Updateuserstate(message.chat.id, 5);

                    bot.SendMessage(message.chat.id, "Готово. Продолжаю мониторинг.");
                }
                else
                {
                    DbReader.Updateticket(message.chat.id, " ");
                    DbReader.Updateuserstate(message.chat.id, 5);

                    bot.SendMessage(message.chat.id, "Тикет уже распределён. Продолжаю мониторинг.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("error", message.chat.id, "ResolveTicket6", ex.GetType() + ": " + ex.Message, issue.Key.Value);
                DbReader.Updateticket(message.chat.id, " ");
                DbReader.Updateuserstate(message.chat.id, 5);
                bot.SendMessage(message.chat.id, "Что-то пошло не так. Возможно, тикет был изменён кем-то еще. Продолжаю мониторинг.");
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
