using System;
using Telegram;
using Atlassian.Jira;

namespace DutyBot

{
    static class JiraAddFuncions
    {
        public static void AssingTicket(Issue issue, Message message, string user, TelegramBot Bot, Jira jiraConn, string keyboard = "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}")
        {
            try
            {
                    issue.Refresh();

                    var d = issue.Key.Value;
                    var c = issue.Assignee;
                    var e = DBReader.readticket(message.chat.id);
                    var h = issue.Key.Value.Equals(DBReader.readticket(message.chat.id));

                    if (issue.Assignee == null & issue.Key.Value.Equals(DBReader.readticket(message.chat.id)))
                    {
                        if (issue.Status.ToString() == "10050")
                        {
                            issue.WorkflowTransition("Распределить");
                        }

                        issue.Assignee = user;
                        issue.SaveChanges();


                        DBReader.updateticket(message.chat.id, " ");
                        DBReader.updateuserstate(message.chat.id, 3);
                        Bot.SendMessage(message.chat.id, "Готово.", keyboard);

                        return;

                    }
                    else
                    {
                        DBReader.updateticket(message.chat.id, " ");
                        DBReader.updateuserstate(message.chat.id, 3);

                        Bot.SendMessage(message.chat.id, "Тикет уже распределён", keyboard);
                        return;
                    }

            }
            catch (Exception ex)
            {
                Logger.LogException("error", message.chat.id, "AssingTicket", ex.GetType() + ": " + ex.Message, issue.Key.Value);
                DBReader.updateuserstate(message.chat.id, 3);
                Bot.SendMessage(message.chat.id, "Что-то пошло не так.", "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");

            }
        }

        public static void AssingTicket6(Issue issue, Message message, string user, TelegramBot Bot, Jira jiraConn)
        {
            try
            {

                issue.Refresh();

                    if (issue.Assignee == null & issue.Key.ToString().Equals(DBReader.readticket(message.chat.id)))
                    {
                        if (issue.Status.ToString() == "10050")
                        {
                            issue.WorkflowTransition("Распределить");
                        }

                        issue.Assignee = user;
                        issue.SaveChanges();

                        DBReader.updateticket(message.chat.id, " ");
                        DBReader.updateuserstate(message.chat.id, 5);

                        Bot.SendMessage(message.chat.id, "Готово. Продолжаю мониторинг.");
                        return;

                    }
                    else
                    {
                        DBReader.updateticket(message.chat.id, " ");
                        DBReader.updateuserstate(message.chat.id, 5);

                        Bot.SendMessage(message.chat.id, "Тикет уже распределён. Продолжаю мониторинг.");
                        return;
                    }

            }
            catch (Exception ex)
            {
                Logger.LogException("error", message.chat.id, "AssingTicket6", ex.GetType() + ": " + ex.Message, issue.Key.Value);
                DBReader.updateticket(message.chat.id, " ");
                DBReader.updateuserstate(message.chat.id, 5);
                Bot.SendMessage(message.chat.id, "Что-то пошло не так. Возможно, тикет был изменён кем-то еще. Продолжаю мониторинг.");

            }
        }

        public static void ResolveTicket(Issue issue, Message message, string user, TelegramBot Bot, Jira jiraConn, string keyboard = "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}")
        {
            try
            {
                issue.Refresh();
                if (issue.Assignee == null & issue.Key.ToString().Equals(DBReader.readticket(message.chat.id)))
                {
                    if (issue.Status.ToString() != "5")
                    {
                        issue.WorkflowTransition("Решить");
                    }

                    issue.Assignee = user;
                    issue.SaveChanges();

                    DBReader.updateticket(message.chat.id, " ");
                    DBReader.updateuserstate(message.chat.id, 3);
                    Bot.SendMessage(message.chat.id, "Готово", keyboard);
                    return;

                }
                else
                {
                    DBReader.updateticket(message.chat.id, " ");
                    DBReader.updateuserstate(message.chat.id, 3);

                    Bot.SendMessage(message.chat.id, "Тикет уже распределён", keyboard);
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("error", message.chat.id, "ResolveTicket", ex.GetType() + ": " + ex.Message, issue.Key.Value);
                DBReader.updateuserstate(message.chat.id, 3);
                Bot.SendMessage(message.chat.id, "Что-то пошло не так.", "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");

            }
        }

        public static void ResolveTicket6(Issue issue, Message message, string user, TelegramBot Bot, Jira jiraConn)
        {
            try
            {
                issue.Refresh();

                if (issue.Assignee == null & issue.Key.ToString().Equals(DBReader.readticket(message.chat.id)))
                {
                    if (issue.Status.ToString() != "5")
                    {
                        issue.WorkflowTransition("Решить");
                    }

                    issue.Assignee = user;
                    issue.SaveChanges();

                    DBReader.updateticket(message.chat.id, " ");
                    DBReader.updateuserstate(message.chat.id, 5);

                    Bot.SendMessage(message.chat.id, "Готово. Продолжаю мониторинг.");
                    return;

                }
                else
                {
                    DBReader.updateticket(message.chat.id, " ");
                    DBReader.updateuserstate(message.chat.id, 5);

                    Bot.SendMessage(message.chat.id, "Тикет уже распределён. Продолжаю мониторинг.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("error", message.chat.id, "ResolveTicket6", ex.GetType() + ": " + ex.Message, issue.Key.Value);
                DBReader.updateticket(message.chat.id, " ");
                DBReader.updateuserstate(message.chat.id, 5);
                Bot.SendMessage(message.chat.id, "Что-то пошло не так. Возможно, тикет был изменён кем-то еще. Продолжаю мониторинг.");

            }
        }

        public static bool CheckConnection(Jira jiraConn, string login, string password)
        {
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
