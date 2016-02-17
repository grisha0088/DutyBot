using System;
using System.Collections.Generic;
using System.Linq;
using Telegram;
using System.Threading;
using Atlassian.Jira;
using Topshelf;

namespace DutyBot
{
    class Program
    {

        public static void Main()
        {
            try
            {

                HostFactory.Run(x =>
                {
                    x.Service<Prog>(s =>
                    {
                        s.ConstructUsing(name => new Prog());
                        s.WhenStarted(tc => tc.Start());
                        s.WhenStopped(tc => tc.Stop());

                    });
                    x.RunAsNetworkService();

                    x.SetDescription("Service for telegramm DutyBot");
                    x.SetDisplayName("DutyBot");
                    x.SetServiceName("DutyBot");
                    x.StartAutomaticallyDelayed();
                });


            }
            catch (Exception ex)
            {
                Sender.send("https://api.telegram.org/bot179261100:AAGqaQ8Fum0xK8JQL0FE_N4LugS_MmO36zM/sendmessage?chat_id=38651047&text=Ошибка при старте DutyBot: " + ex, "");

            }
            
        }

    }


    class Prog
    {

        public void Start() //метод вызывается при старте службы
        {
            
            try
            {
                Sender.send("https://api.telegram.org/bot179261100:AAGqaQ8Fum0xK8JQL0FE_N4LugS_MmO36zM/sendmessage?chat_id=38651047&text=" + "Запущен сервис DutyBot", "");

                Thread.Sleep(10000); // 

                try
                {
                    Logger.LogOpperation("info", 1, "StartService", "");
                }
                catch(Exception ex)
                {

                }

                Bot = new TelegramBot(DBReader.readbot());
                
                ReadmessageThread = new Thread(new ThreadStart(this.readmessages));  //запускаю поток по считыванию и обработке сообщений из telegramm
                ReadmessageThread.Start();

                CheckTicketsThread = new Thread(new ThreadStart(this.checkjira));   //запускаю поток по проверке тикетов в jira
                CheckTicketsThread.Start();
                
            }
            catch (Exception ex)
            {
                Sender.send("https://api.telegram.org/bot179261100:AAGqaQ8Fum0xK8JQL0FE_N4LugS_MmO36zM/sendmessage?chat_id=38651047&text=" + ex.Message, "");
                Logger.LogException("fatal", 1, "StartService", ex.GetType() + ": " + ex.Message, "");
            }
        }

        public void Stop()  //метод вызывается при остановке службы
        {
            try
            {
                Sender.send("https://api.telegram.org/bot179261100:AAGqaQ8Fum0xK8JQL0FE_N4LugS_MmO36zM/sendmessage?chat_id=38651047&text=" + "Остановлен сервис DutyBot", "");
                Logger.LogOpperation("info", 1, "StopService", "");

                readmessagesflag = false;
                checkjiraflag = false;
                
            }
            catch (Exception ex)
            {
                Logger.LogException("fatal", 1, "StopService", ex.GetType() + ": " + ex.Message, "");
            }
        }

        Thread ReadmessageThread;  //поток читает и обрабатывает сообщения из telegramm        
        Thread CheckTicketsThread; //поток проверяет есть ли тикеты в jira-фильтре 
        TelegramBot Bot;
        Jira jiraConn;
        Issue issue;
        Issue ticket;
        
        bool readmessagesflag = true;
        bool checkjiraflag = true;

        public void checkjira()
        {
            jiraConn = Jira.CreateRestClient(DBReader.readjira(), DBReader.readdefaultuser(), DBReader.readdefaultpassword());
            IEnumerable<Atlassian.Jira.Issue> issues;

            while (checkjiraflag)
            {
                try
                {
                
                var u = DBReader.readallpeople();
                
                // если дежурство закончилось, меняем статус на 3
              
                    for (int i = 0; i < u.GetLength(0); i++)
                    {
                        if (DBReader.readdutyend(u[i]) < DateTime.Now & (DBReader.readuserstate(u[i]) == 5))
                        {
                            DBReader.updateuserstate(u[i], 3);

                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogException("error", 1, "FinishDuty", ex.GetType() + ": " + ex.Message, "");
                }


                //вычитываю тикеты из заданного фильтра
                try
                {
                    issues = jiraConn.GetIssuesFromJql(DBReader.readfilter());
                  
                    if (issues.Count() > 0 && DBReader.readrespcount() > 0)
                    {
                        ticket = issues.First();
                        
                        var a = DBReader.readresppeople();

                        for (int i = 0; i < a.GetLength(0); i++)
                        {

                            if (a[i] != 0 & ticket.Assignee == null)
                            {
                                jiraConn = Jira.CreateRestClient(DBReader.readjira(), DBReader.readuserlogin(a[i]), DBReader.readuserpassword(a[i]));
                                DBReader.updateticket(a[i], ticket.Key.ToString());
                                Bot.SendMessage(a[i], ticket);
                                DBReader.updateuserstate(a[i], 6);
                            }
                            else
                            {
                                Thread.Sleep(10000);
                                break;
                            }
                        }
                    }
                    else
                    {
                        Thread.Sleep(10000);
                    }


                }
                catch (Exception ex)
                {
                    var a = DBReader.readresppeople();

                    for (int i = 0; i < a.GetLength(0); i++)
                    {

                        if (a[i] != 0)
                        {
                            try
                            {
                                Bot.SendMessage(a[i], "Похоже, что jira не доступна. Мониторинг остановлен. Что будем делать?", "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                            }
                            catch (Exception exeption)
                            {
                                Logger.LogException("error", 1, "SendThatJiraDoesNotWork", exeption.Message, "");
                            }
                            DBReader.updateuserstate(a[i], 3);
                        }
                        else
                        {
                            break;
                        }
                    }
                    
                    Logger.LogException("error", 1, "ReadTicketsFromJira", ex.Message, "");
                    Thread.Sleep(60000);
                }

            }
        }

        public void readmessages()
        {
            var offset = 0;  //id последнего прочитанного сообщения из telegramm


            // в бесконечном цикле начинаю вычитывать сообщения из telegramm
            while (readmessagesflag)
            {
                try
                {
                    var updates = Bot.getupdates(offset);

                    foreach (var result in updates.result)
                    {

                        processmessage(result.message);
                        offset = result.update_id + 1;
                    }

                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    Logger.LogException("error", 1, "GetTelegrammUpdates", ex.Message, "");
                    Thread.Sleep(5000);
                }
            }
        }

        void processmessage(Message message)
        {

            try
            {

                if (DBReader.readuserstate(message.chat.id) == -1)
                {
                    Bot.SendMessage(message.chat.id, "Привет, " + message.chat.first_name + @"! Меня зовут DutyBot, я создан, чтобы помогать дежурить. Давай знакомиться!
 ", "{\"keyboard\": [[\"Рассказать DutyBot'у о себе\"], [\"Не хочу знакомиться, ты мне не нравишься\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");


                    DBReader.insertuser(message.chat.first_name, message.chat.id, 0);
                    return;
                }

                if (DBReader.readuserstate(message.chat.id) == 0)
                {

                    if ((message.text) == "Рассказать DutyBot'у о себе" | (message.text) == "Ввести учётку еще раз")
                    {
                        Bot.SendMessage(message.chat.id, @"Чтобы мы могли работать в Jira, мне нужны твои учётные данные. Напиши мне свой логин в формате d.bot и нажми отправить.
Твои данные будут относительно безопасно храниться на сервере");
                        DBReader.updateuserstate(message.chat.id, 1);
                        return;
                    }

                    if ((message.text) == "Не хочу знакомиться, ты мне не нравишься")
                    {
                        Bot.SendMessage(message.chat.id, @"Очень жаль, но если надумешь, пиши. Я забуду об этом неприятном разговоре");
                        DBReader.deletetuser(message.chat.id);
                        return;
                    }

                    DBReader.deletetuser(message.chat.id);
                    return;
                }

                if (DBReader.readuserstate(message.chat.id) == 1)
                {
                    Bot.SendMessage(message.chat.id, @"Теперь напиши пароль");
                    DBReader.updateuserlogin(message.chat.id, message.text);
                    DBReader.updateuserstate(message.chat.id, 2);
                    return;
                }

                if (DBReader.readuserstate(message.chat.id) == 2)
                {
                    DBReader.updateuserpassword(message.chat.id, message.text);
                    Bot.SendMessage(message.chat.id, @"Отлично, сейчас я проверю, есть ли у тебя доступ в Jira");
                    if (JiraAddFuncions.CheckConnection(jiraConn, DBReader.readuserlogin(message.chat.id), DBReader.readuserpassword(message.chat.id)))
                    {
                        Bot.SendMessage(message.chat.id, @"Всё хорошо, можно начинать работу", "{\"keyboard\": [[\"Начнём\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                        DBReader.updateuserstate(message.chat.id, 3);
                        return;
                    }
                    else
                    {
                        Bot.SendMessage(message.chat.id, @"Доступа к JIra нет. Возможно учётные данные не верны. Давай попробуем ввести их еще раз. ", "{\"keyboard\": [[\"Ввести учётку еще раз\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                        DBReader.updateuserstate(message.chat.id, 0);
                        return;
                    }

                }



                if (DBReader.readuserstate(message.chat.id) == 3)
                {

                    if ((message.text) == "Начнём")
                    {
                        Bot.SendMessage(message.chat.id, "Просто напиши мне сообщение, когда я тебе понадоблюсь. В ответ я пришлю меню с вариантами моих действий. Вот такое ↓", "{\"keyboard\": [[\"Кто сейчас дежурит?\"], [\"Проверь тикеты\"], [\"Помоги с дежурством\"], [\"Пока ничем\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                        return;
                    }


                    if ((message.text) == "Кто сейчас дежурит?")
                    {
                        Bot.SendMessage(message.chat.id, DBReader.readrespersone(), "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                        return;  
                    }

                    if ((message.text) == "Проверь тикеты")
                    {
                        jiraConn = Jira.CreateRestClient(DBReader.readjira(), DBReader.readuserlogin(message.chat.id), DBReader.readuserpassword(message.chat.id));

                        try
                        {
                            var issues = jiraConn.GetIssuesFromJql(DBReader.readfilter());

                            if (issues.Count() > 0)
                            {
                                issue = issues.First();
                                DBReader.updateticket(message.chat.id, issue.Key.ToString());
                                Bot.SendMessage(message.chat.id, issue);
                                DBReader.updateuserstate(message.chat.id, 4);
                            }

                            else
                            {
                                Bot.SendMessage(message.chat.id, "Тикетов нет", "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                            }

                        }
                        catch (Exception ex)
                        {
                            Bot.SendMessage(message.chat.id, "Jira не доступна", "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                            Logger.LogException("error", 1, "SendThatJiraDoesNotWork", ex.Message, "");
                        }
                        
                        return;
                    }

                    if ((message.text) == "Помоги с дежурством")
                    {
                        Bot.SendMessage(message.chat.id, "Как будем дежурить?", "{\"keyboard\": [[\"Начать мониторить тикеты\"], [\"Мониторить тикеты в моё дежурство\"], [\"Отмена\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                        return;
                    }

                    if ((message.text) == "Пока ничего")
                    {
                        Bot.SendMessage(message.chat.id, "Ок, если что, пиши.");
                        return;
                    }

                    if ((message.text) == "Начать мониторить тикеты")
                    {
                        Bot.SendMessage(message.chat.id, @"Начинаю мониторинг.
Я буду мониторить тикеты в течение ближайших 12 часов, после чего мониторинг будет автоматически остановлен.");


                        DBReader.updateuserstate(message.chat.id, 5);
                        DBReader.updatedutystart(message.chat.id, DateTime.Now);
                        DBReader.updatedutyend(message.chat.id, DateTime.Now.AddHours(12));
                        return;
                    }

                    if ((message.text) == "Мониторить тикеты в моё дежурство")
                    {

                        if (DBReader.readuserdutystart(message.chat.id) == Convert.ToDateTime("01.01.1900 0:00:00 ") | DBReader.readuserdutystart(message.chat.id) == Convert.ToDateTime("01.01.1900 0:00:00 "))
                        {
                            Bot.SendMessage(message.chat.id, "У тебя нет дежурств в ближайшее время", "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                            return;
                        }
                        else
                        {
                            Bot.SendMessage(message.chat.id, "Я буду мониторить тикеты с " + DBReader.readuserdutystart(message.chat.id).ToShortDateString() + " " + DBReader.readuserdutystart(message.chat.id).ToShortTimeString() + " по " + DBReader.readuserdutyend(message.chat.id).ToShortDateString() + " " + DBReader.readuserdutyend(message.chat.id).ToShortTimeString());

                            DBReader.updateuserstate(message.chat.id, 5);
                            DBReader.updatedutystart(message.chat.id, DBReader.readuserdutystart(message.chat.id));
                            DBReader.updatedutyend(message.chat.id, DBReader.readuserdutyend(message.chat.id));
                            return;
                        }

                    }

                    Bot.SendMessage(message.chat.id, "Чем я могу помочь?", "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                    return;
                }

                if (DBReader.readuserstate(message.chat.id) == 4)
                {

                    if (issue.Key.ToString().Equals(DBReader.readticket(message.chat.id)))
                    {

                        if ((message.text) == "Распределить")
                        {
                            Bot.SendMessage(message.chat.id, "Кому назначим?", "{\"keyboard\": [[\"Технологи\", \"Коммерция\"], [\"Админы\", \"Связисты\"], [\"Олеся\", \"Женя\"], [\"Алексей\", \"Максим\"], [\"Паша\", \"Марина\"], [\"Андрей\", \"Гриша\"], [\"Оля\", \"Настя\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                            return;
                        }

                        if ((message.text) == "Решить")
                        {

                            JiraAddFuncions.ResolveTicket(issue, message, DBReader.readuserlogin(message.chat.id), Bot, jiraConn);
                            return;
                        }

                        if ((message.text) == "Назначить себе")
                        {
                            JiraAddFuncions.AssingTicket(issue, message, DBReader.readuserlogin(message.chat.id), Bot, jiraConn);
                        }

                        if ((message.text) == "Технологи") JiraAddFuncions.AssingTicket(issue, message, "tecnologsupport", Bot, jiraConn);
                        if ((message.text) == "Коммерция") JiraAddFuncions.AssingTicket(issue, message, "crm_otdel", Bot, jiraConn);
                        if ((message.text) == "Админы") JiraAddFuncions.AssingTicket(issue, message, "Uk.Jira.TechSupport", Bot, jiraConn);
                        if ((message.text) == "Связисты") JiraAddFuncions.AssingTicket(issue, message, "uk.jira.noc", Bot, jiraConn);
                        if ((message.text) == "Олеся") JiraAddFuncions.AssingTicket(issue, message, "o.likhacheva", Bot, jiraConn);
                        if ((message.text) == "Женя") JiraAddFuncions.AssingTicket(issue, message, "ev.safonov", Bot, jiraConn);
                        if ((message.text) == "Алексей") JiraAddFuncions.AssingTicket(issue, message, "a.sapotko", Bot, jiraConn);
                        if ((message.text) == "Максим") JiraAddFuncions.AssingTicket(issue, message, "m.shemetov", Bot, jiraConn);
                        if ((message.text) == "Андрей") JiraAddFuncions.AssingTicket(issue, message, "an.zarubin", Bot, jiraConn);
                        if ((message.text) == "Гриша") JiraAddFuncions.AssingTicket(issue, message, "g.dementiev", Bot, jiraConn);
                        if ((message.text) == "Оля") JiraAddFuncions.AssingTicket(issue, message, "o.tkachenko", Bot, jiraConn);
                        if ((message.text) == "Настя") JiraAddFuncions.AssingTicket(issue, message, "a.zakharova", Bot, jiraConn);
                        if ((message.text) == "Марина") JiraAddFuncions.AssingTicket(issue, message, "m.vinnikova", Bot, jiraConn);
                        if ((message.text) == "Паша") JiraAddFuncions.AssingTicket(issue, message, "p.denisov", Bot, jiraConn);

                    }
                    else
                    {
                        Bot.SendMessage(message.chat.id, "Похоже, тикет уже распределён.");
                        DBReader.updateuserstate(message.chat.id, 3);
                    }
                }


                if (DBReader.readuserstate(message.chat.id) == 5)
                {
                    switch (message.text)
                    {
                        case ("Остановить мониторинг"):
                            {
                                DBReader.updateuserstate(message.chat.id, 3);
                                Bot.SendMessage(message.chat.id, "Готово");

                                break;
                            }

                        default: Bot.SendMessage(message.chat.id, "да?", "{\"keyboard\": [[\"Остановить мониторинг\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}"); break;
                    }
                }

                if (DBReader.readuserstate(message.chat.id) == 6)
                {
                    if (ticket.Key.ToString().Equals(DBReader.readticket(message.chat.id)))
                    {

                        switch (message.text)
                        {
                            case ("Распределить"):
                                 {
                                    Bot.SendMessage(message.chat.id, "Кому назначим?", "{\"keyboard\": [[\"Технологи\", \"Коммерция\"], [\"Админы\", \"Связисты\"], [\"Олеся\", \"Женя\"], [\"Алексей\", \"Максим\"], [\"Паша\", \"Марина\"], [\"Андрей\", \"Гриша\"], [\"Оля\", \"Настя\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                                    break;
                                }

                            case ("Решить"):
                                {
                                    JiraAddFuncions.ResolveTicket6(ticket, message, DBReader.readuserlogin(message.chat.id), Bot, jiraConn);
                                    break;
                                }

                            case ("Назначить себе"):
                                {
                                    JiraAddFuncions.AssingTicket6(ticket, message, DBReader.readuserlogin(message.chat.id), Bot, jiraConn);
                                    break;
                                }

                            case ("Остановить мониторинг"):
                                {

                                    Bot.SendMessage(message.chat.id, "Готово");
                                    DBReader.updateuserstate(message.chat.id, 3);
                                    break;
                                }

                            case ("Технологи"): JiraAddFuncions.AssingTicket6(ticket, message, "tecnologsupport", Bot, jiraConn); break;
                            case ("Коммерция"): JiraAddFuncions.AssingTicket6(ticket, message, "crm_otdel", Bot, jiraConn); break;
                            case ("Админы"): JiraAddFuncions.AssingTicket6(ticket, message, "Uk.Jira.TechSupport", Bot, jiraConn); break;
                            case ("Связисты"): JiraAddFuncions.AssingTicket6(ticket, message, "uk.jira.noc", Bot, jiraConn); break;
                            case ("Олеся"): JiraAddFuncions.AssingTicket6(ticket, message, "o.likhacheva", Bot, jiraConn); break;
                            case ("Женя"): JiraAddFuncions.AssingTicket6(ticket, message, "ev.safonov", Bot, jiraConn); break;
                            case ("Алексей"): JiraAddFuncions.AssingTicket6(ticket, message, "a.sapotko", Bot, jiraConn); break;
                            case ("Максим"): JiraAddFuncions.AssingTicket6(ticket, message, "m.shemetov", Bot, jiraConn); break;
                            case ("Паша"): JiraAddFuncions.AssingTicket6(ticket, message, "p.denisov", Bot, jiraConn); break;
                            case ("Марина"): JiraAddFuncions.AssingTicket6(ticket, message, "m.vinnikova", Bot, jiraConn); break;
                            case ("Андрей"): JiraAddFuncions.AssingTicket6(ticket, message, "an.zarubin", Bot, jiraConn); break;
                            case ("Гриша"): JiraAddFuncions.AssingTicket6(ticket, message, "g.dementiev", Bot, jiraConn); break;
                            case ("Оля"): JiraAddFuncions.AssingTicket6(ticket, message, "o.tkachenko", Bot, jiraConn); break;
                            case ("Настя"): JiraAddFuncions.AssingTicket6(ticket, message, "a.zakharova", Bot, jiraConn); break;

                            default: Bot.SendMessage(message.chat.id, "да?", "{\"keyboard\": [[\"Остановить мониторинг\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}"); break;
                        }
                    }
                    else
                    {
                        Bot.SendMessage(message.chat.id, "Похоже, тикет уже распределён.");
                        DBReader.updateuserstate(message.chat.id, 5);
                    }
                }

            }
            catch (Exception ex)
            {
                DBReader.updateuserstate(message.chat.id, 3);
                Bot.SendMessage(message.chat.id, "Что-то пошло не так при обработке сообщения.", "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");

                Logger.LogException("error", message.chat.id, "ProcessMessage", ex.Message, "");
                Thread.Sleep(5000);
            }
        }
        
    }
}
