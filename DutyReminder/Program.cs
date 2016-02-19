using System;
using System.Collections.Generic;
using System.Linq;
using Telegram;
using System.Threading;
using Atlassian.Jira;
using Topshelf;

namespace DutyBot
{
    internal class Program
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
                Sender.Send("https://api.telegram.org/bot179261100:AAGqaQ8Fum0xK8JQL0FE_N4LugS_MmO36zM/sendmessage?chat_id=38651047&text=Ошибка при старте DutyBot: " + ex);
            }
        }
    }


    internal class Prog
    {
        public void Start() //метод вызывается при старте службы
        {
            try
            {
                Sender.Send("https://api.telegram.org/bot179261100:AAGqaQ8Fum0xK8JQL0FE_N4LugS_MmO36zM/sendmessage?chat_id=38651047&text=" + "Запущен сервис DutyBot");
                try
                {
                    Logger.LogOpperation("info", 1, "StartService", "");
                }
                catch(Exception ex)
                {
                    Thread.Sleep(10000);
                    try
                    {
                        Logger.LogOpperation("info", 1, "StartService", "");
                    }
                    catch
                    {
                        Sender.Send("https://api.telegram.org/bot179261100:AAGqaQ8Fum0xK8JQL0FE_N4LugS_MmO36zM/sendmessage?chat_id=38651047&text=Произошла ошибка при запуске службы. Подождал 10 секунд, но ошибка осталась: " + ex);
                    }
                }

                _bot = new TelegramBot(DbReader.Readbot());
                
                _readmessageThread = new Thread(Readmessages);  //запускаю поток по считыванию и обработке сообщений из telegramm
                _readmessageThread.Start();

                _checkTicketsThread = new Thread(Checkjira);   //запускаю поток по проверке тикетов в jira
                _checkTicketsThread.Start();
                
            }
            catch (Exception ex)
            {
                Sender.Send("https://api.telegram.org/bot179261100:AAGqaQ8Fum0xK8JQL0FE_N4LugS_MmO36zM/sendmessage?chat_id=38651047&text=" + ex.Message);
                Logger.LogException("fatal", 1, "StartService", ex.GetType() + ": " + ex.Message, "");
            }
        }
        public void Stop()  //метод вызывается при остановке службы
        {
            try
            {
                Sender.Send("https://api.telegram.org/bot179261100:AAGqaQ8Fum0xK8JQL0FE_N4LugS_MmO36zM/sendmessage?chat_id=38651047&text=" + "Остановлен сервис DutyBot");
                Logger.LogOpperation("info", 1, "StopService", "");

                _readmessagesflag = false;
                _checkjiraflag = false;
            }
            catch (Exception ex)
            {
                Logger.LogException("fatal", 1, "StopService", ex.GetType() + ": " + ex.Message, "");
            }
        }

        private Thread _readmessageThread;  //поток читает и обрабатывает сообщения из telegramm        
        private Thread _checkTicketsThread; //поток проверяет есть ли тикеты в jira-фильтре 
        private TelegramBot _bot;
        private Jira _jiraConn;
        private Issue _issue;
        private Issue _ticket;
        private bool _readmessagesflag = true;
        private bool _checkjiraflag = true;

        public void Checkjira()
        {
            _jiraConn = Jira.CreateRestClient(DbReader.Readjira(), DbReader.Readdefaultuser(), DbReader.Readdefaultpassword());
            while (_checkjiraflag)
            {
                try
                {
                var u = DbReader.Readallpeople();
                // если дежурство закончилось, меняем статус на 3
              
                    for (int i = 0; i < u.GetLength(0); i++)
                    {
                        if (DbReader.Readdutyend(u[i]) < DateTime.Now & (DbReader.Readuserstate(u[i]) == 5))
                        {
                            DbReader.Updateuserstate(u[i], 3);
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
                    var issues = _jiraConn.GetIssuesFromJql(DbReader.Readfilter());
                    var enumerable = issues as IList<Issue> ?? issues.ToList();
                    if (enumerable.Any() && DbReader.Readrespcount() > 0)
                    {
                        _ticket = enumerable.Last();
                        var a = DbReader.Readresppeople();

                        for (var i = 0; i < a.GetLength(0); i++)
                        {
                            if (a[i] != 0 & _ticket.Assignee == null)
                            {
                                _jiraConn = Jira.CreateRestClient(DbReader.Readjira(), DbReader.Readuserlogin(a[i]), DbReader.Readuserpassword(a[i]));
                                DbReader.Updateticket(a[i], _ticket.Key.ToString());
                                _bot.SendMessage(a[i], _ticket);
                                DbReader.Updateuserstate(a[i], 6);
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
                    var a = DbReader.Readresppeople();

                    for (var i = 0; i < a.GetLength(0); i++)
                    {
                        if (a[i] != 0)
                        {
                            try
                            {
                                _bot.SendMessage(a[i], "Похоже, что jira не доступна. Мониторинг остановлен. Что будем делать?", "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                            }
                            catch (Exception exeption)
                            {
                                Logger.LogException("error", 1, "SendThatJiraDoesNotWork", exeption.Message, "");
                            }
                            DbReader.Updateuserstate(a[i], 3);
                        }
                        else
                        {
                            break;
                        }
                    }
                    
                    Logger.LogException("error", 1, "ReadTicketsFromJira", ex.Message, "");
                    Thread.Sleep(30000);
                }
            }
        }

        public void Readmessages()
        {
            var offset = 0;  //id последнего прочитанного сообщения из telegramm
            while (_readmessagesflag) // в бесконечном цикле начинаю вычитывать сообщения из telegramm
            {
                try
                {
                    var updates = _bot.Getupdates(offset);
                    foreach (var result in updates.result)
                    {
                        Processmessage(result.message);
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

        private void Processmessage(Message message)
        {
            try
            {
                switch (DbReader.Readuserstate(message.chat.id))
                {
                    case -1:
                        _bot.SendMessage(message.chat.id, "Привет, " + message.chat.first_name + @"! Меня зовут DutyBot, я создан, чтобы помогать дежурить. Давай знакомиться!
 ", "{\"keyboard\": [[\"Рассказать DutyBot'у о себе\"], [\"Не хочу знакомиться, ты мне не нравишься\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                        DbReader.Insertuser(message.chat.first_name, message.chat.id);
                        return;
                    case 0:

                        if ((message.text) == "Рассказать DutyBot'у о себе" | (message.text) == "Ввести учётку еще раз")
                        {
                            _bot.SendMessage(message.chat.id, @"Чтобы мы могли работать в Jira, мне нужны твои учётные данные. Напиши мне свой логин в формате d.bot и нажми отправить.
Твои данные будут относительно безопасно храниться на сервере");
                            DbReader.Updateuserstate(message.chat.id, 1);
                            return;
                        }

                        if ((message.text) == "Не хочу знакомиться, ты мне не нравишься")
                        {
                            _bot.SendMessage(message.chat.id, @"Очень жаль, но если надумешь, пиши. Я забуду об этом неприятном разговоре");
                            DbReader.Deletetuser(message.chat.id);
                            return;
                        }

                        DbReader.Deletetuser(message.chat.id);
                        return;
                    case 1:
                        _bot.SendMessage(message.chat.id, @"Теперь напиши пароль");
                        DbReader.Updateuserlogin(message.chat.id, message.text);
                        DbReader.Updateuserstate(message.chat.id, 2);
                        return;
                    case 2:
                        DbReader.Updateuserpassword(message.chat.id, message.text);
                        _bot.SendMessage(message.chat.id, @"Отлично, сейчас я проверю, есть ли у тебя доступ в Jira");
                        if (JiraAddFuncions.CheckConnection(_jiraConn, DbReader.Readuserlogin(message.chat.id), DbReader.Readuserpassword(message.chat.id)))
                        {
                            _bot.SendMessage(message.chat.id, @"Всё хорошо, можно начинать работу", "{\"keyboard\": [[\"Начнём\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                            DbReader.Updateuserstate(message.chat.id, 3);
                            return;
                        }
                        else
                        {
                            _bot.SendMessage(message.chat.id, @"Доступа к JIra нет. Возможно учётные данные не верны. Давай попробуем ввести их еще раз. ", "{\"keyboard\": [[\"Ввести учётку еще раз\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                            DbReader.Updateuserstate(message.chat.id, 0);
                            return;
                        }
                    case 3:
                        switch (message.text)
                        {
                            case "Начнём":
                                _bot.SendMessage(message.chat.id, "Просто напиши мне сообщение, когда я тебе понадоблюсь. В ответ я пришлю меню с вариантами моих действий. Вот такое ↓", "{\"keyboard\": [[\"Кто сейчас дежурит?\"], [\"Проверь тикеты\"], [\"Помоги с дежурством\"], [\"Пока ничем\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                                return;
                            case "Кто сейчас дежурит?":
                                _bot.SendMessage(message.chat.id, DbReader.Readrespersone(), "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                                return;
                            case "Проверь тикеты":
                                _jiraConn = Jira.CreateRestClient(DbReader.Readjira(), DbReader.Readuserlogin(message.chat.id), DbReader.Readuserpassword(message.chat.id));

                                try
                                {
                                    var issues = _jiraConn.GetIssuesFromJql(DbReader.Readfilter());
                                    IList<Issue> enumerable = issues as IList<Issue> ?? issues.ToList();
                                    if (enumerable.Any())
                                    {
                                        _issue = enumerable.Last();
                                        DbReader.Updateticket(message.chat.id, _issue.Key.ToString());
                                        _bot.SendMessage(message.chat.id, _issue);
                                        DbReader.Updateuserstate(message.chat.id, 4);
                                    }
                                    else
                                    {
                                        _bot.SendMessage(message.chat.id, "Тикетов нет", "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                                    }

                                }
                                catch (Exception ex)
                                {
                                    _bot.SendMessage(message.chat.id, "Jira не доступна", "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                                    Logger.LogException("error", 1, "SendThatJiraDoesNotWork", ex.Message, "");
                                }
                        
                                return;
                            case "Помоги с дежурством":
                                _bot.SendMessage(message.chat.id, "Как будем дежурить?", "{\"keyboard\": [[\"Начать мониторить тикеты\"], [\"Мониторить тикеты в моё дежурство\"], [\"Отмена\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                                return;
                            case "Пока ничего":
                                _bot.SendMessage(message.chat.id, "Ок, если что, пиши.");
                                return;
                            case "Начать мониторить тикеты":
                                _bot.SendMessage(message.chat.id, @"Начинаю мониторинг.
Я буду мониторить тикеты в течение ближайших 12 часов, после чего мониторинг будет автоматически остановлен.");


                                DbReader.Updateuserstate(message.chat.id, 5);
                                DbReader.Updatedutystart(message.chat.id, DateTime.Now);
                                DbReader.Updatedutyend(message.chat.id, DateTime.Now.AddHours(12));
                                return;
                            case "Мониторить тикеты в моё дежурство":

                                if (DbReader.Readuserdutystart(message.chat.id) == Convert.ToDateTime("01.01.1900 0:00:00 ") | DbReader.Readuserdutystart(message.chat.id) == Convert.ToDateTime("01.01.1900 0:00:00 "))
                                {
                                    _bot.SendMessage(message.chat.id, "У тебя нет дежурств в ближайшее время", "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                                    return;
                                }
                                else
                                {
                                    _bot.SendMessage(message.chat.id, "Я буду мониторить тикеты с " + DbReader.Readuserdutystart(message.chat.id).ToShortDateString() + " " + DbReader.Readuserdutystart(message.chat.id).ToShortTimeString() + " по " + DbReader.Readuserdutyend(message.chat.id).ToShortDateString() + " " + DbReader.Readuserdutyend(message.chat.id).ToShortTimeString());
                                    DbReader.Updateuserstate(message.chat.id, 5);
                                    DbReader.Updatedutystart(message.chat.id, DbReader.Readuserdutystart(message.chat.id));
                                    DbReader.Updatedutyend(message.chat.id, DbReader.Readuserdutyend(message.chat.id));
                                    return;
                                }
                        }
                        _bot.SendMessage(message.chat.id, "Чем я могу помочь?", "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                        return;
                    case 4:

                        if (_issue.Key.ToString().Equals(DbReader.Readticket(message.chat.id)))
                        {
                            switch ((message.text))
                            {
                                case "Распределить":
                                    _bot.SendMessage(message.chat.id, "Кому назначим?", "{\"keyboard\": [[\"Технологи\", \"Коммерция\"], [\"Админы\", \"Связисты\"], [\"Олеся\", \"Женя\"], [\"Алексей\", \"Максим\"], [\"Паша\", \"Марина\"], [\"Андрей\", \"Гриша\"], [\"Оля\", \"Настя\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                                    return;
                                case "Решить":
                                    JiraAddFuncions.ResolveTicket(_issue, message, DbReader.Readuserlogin(message.chat.id), _bot, _jiraConn);
                                    return;
                                case "Назначить себе":
                                    JiraAddFuncions.AssingTicket(_issue, message, DbReader.Readuserlogin(message.chat.id), _bot, _jiraConn);
                                    return;
                                case "Технологи":
                                    JiraAddFuncions.AssingTicket(_issue, message, "tecnologsupport", _bot, _jiraConn);
                                    return;
                                case "Коммерция":
                                    JiraAddFuncions.AssingTicket(_issue, message, "crm_otdel", _bot, _jiraConn);
                                    return;
                                case "Админы":
                                    JiraAddFuncions.AssingTicket(_issue, message, "Uk.Jira.TechSupport", _bot, _jiraConn);
                                    return;
                                case "Связисты":
                                    JiraAddFuncions.AssingTicket(_issue, message, "uk.jira.noc", _bot, _jiraConn);
                                    return;
                                case "Олеся":
                                    JiraAddFuncions.AssingTicket(_issue, message, "o.likhacheva", _bot, _jiraConn);
                                    return;
                                case "Женя":
                                    JiraAddFuncions.AssingTicket(_issue, message, "ev.safonov", _bot, _jiraConn);
                                    return;
                                case "Алексей":
                                    JiraAddFuncions.AssingTicket(_issue, message, "a.sapotko", _bot, _jiraConn);
                                    return;
                                case "Максим":
                                    JiraAddFuncions.AssingTicket(_issue, message, "m.shemetov", _bot, _jiraConn);
                                    return;
                                case "Андрей":
                                    JiraAddFuncions.AssingTicket(_issue, message, "an.zarubin", _bot, _jiraConn);
                                    return;
                                case "Гриша":
                                    JiraAddFuncions.AssingTicket(_issue, message, "g.dementiev", _bot, _jiraConn);
                                    return;
                                case "Оля":
                                    JiraAddFuncions.AssingTicket(_issue, message, "o.tkachenko", _bot, _jiraConn);
                                    return;
                                case "Настя":
                                    JiraAddFuncions.AssingTicket(_issue, message, "a.zakharova", _bot, _jiraConn);
                                    return;
                                case "Марина":
                                    JiraAddFuncions.AssingTicket(_issue, message, "m.vinnikova", _bot, _jiraConn);
                                    return;
                                case "Паша":
                                    JiraAddFuncions.AssingTicket(_issue, message, "p.denisov", _bot, _jiraConn);
                                    break;
                            }
                        }
                        else
                        {
                            _bot.SendMessage(message.chat.id, "Похоже, тикет уже распределён.");
                            DbReader.Updateuserstate(message.chat.id, 3);
                        }
                        break;
                }


                if (DbReader.Readuserstate(message.chat.id) == 5)
                {
                    switch (message.text)
                    {
                        case ("Остановить мониторинг"):
                            {
                                DbReader.Updateuserstate(message.chat.id, 3);
                                _bot.SendMessage(message.chat.id, "Готово");

                                break;
                            }

                        default: _bot.SendMessage(message.chat.id, "да?", "{\"keyboard\": [[\"Остановить мониторинг\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}"); break;
                    }
                }

                if (DbReader.Readuserstate(message.chat.id) == 6)
                {
                    if (_ticket.Key.ToString().Equals(DbReader.Readticket(message.chat.id)))
                    {

                        switch (message.text)
                        {
                            case ("Распределить"):
                                 {
                                    _bot.SendMessage(message.chat.id, "Кому назначим?", "{\"keyboard\": [[\"Технологи\", \"Коммерция\"], [\"Админы\", \"Связисты\"], [\"Олеся\", \"Женя\"], [\"Алексей\", \"Максим\"], [\"Паша\", \"Марина\"], [\"Андрей\", \"Гриша\"], [\"Оля\", \"Настя\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                                    break;
                                }

                            case ("Решить"):
                                {
                                    JiraAddFuncions.ResolveTicket6(_ticket, message, DbReader.Readuserlogin(message.chat.id), _bot, _jiraConn);
                                    break;
                                }

                            case ("Назначить себе"):
                                {
                                    JiraAddFuncions.AssingTicket6(_ticket, message, DbReader.Readuserlogin(message.chat.id), _bot, _jiraConn);
                                    break;
                                }

                            case ("Остановить мониторинг"):
                                {
                                    _bot.SendMessage(message.chat.id, "Готово");
                                    DbReader.Updateuserstate(message.chat.id, 3);
                                    break;
                                }

                            case ("Технологи"): JiraAddFuncions.AssingTicket6(_ticket, message, "tecnologsupport", _bot, _jiraConn); break;
                            case ("Коммерция"): JiraAddFuncions.AssingTicket6(_ticket, message, "crm_otdel", _bot, _jiraConn); break;
                            case ("Админы"): JiraAddFuncions.AssingTicket6(_ticket, message, "Uk.Jira.TechSupport", _bot, _jiraConn); break;
                            case ("Связисты"): JiraAddFuncions.AssingTicket6(_ticket, message, "uk.jira.noc", _bot, _jiraConn); break;
                            case ("Олеся"): JiraAddFuncions.AssingTicket6(_ticket, message, "o.likhacheva", _bot, _jiraConn); break;
                            case ("Женя"): JiraAddFuncions.AssingTicket6(_ticket, message, "ev.safonov", _bot, _jiraConn); break;
                            case ("Алексей"): JiraAddFuncions.AssingTicket6(_ticket, message, "a.sapotko", _bot, _jiraConn); break;
                            case ("Максим"): JiraAddFuncions.AssingTicket6(_ticket, message, "m.shemetov", _bot, _jiraConn); break;
                            case ("Паша"): JiraAddFuncions.AssingTicket6(_ticket, message, "p.denisov", _bot, _jiraConn); break;
                            case ("Марина"): JiraAddFuncions.AssingTicket6(_ticket, message, "m.vinnikova", _bot, _jiraConn); break;
                            case ("Андрей"): JiraAddFuncions.AssingTicket6(_ticket, message, "an.zarubin", _bot, _jiraConn); break;
                            case ("Гриша"): JiraAddFuncions.AssingTicket6(_ticket, message, "g.dementiev", _bot, _jiraConn); break;
                            case ("Оля"): JiraAddFuncions.AssingTicket6(_ticket, message, "o.tkachenko", _bot, _jiraConn); break;
                            case ("Настя"): JiraAddFuncions.AssingTicket6(_ticket, message, "a.zakharova", _bot, _jiraConn); break;

                            default: _bot.SendMessage(message.chat.id, "да?", "{\"keyboard\": [[\"Остановить мониторинг\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}"); break;
                        }
                    }
                    else
                    {
                        _bot.SendMessage(message.chat.id, "Похоже, тикет уже распределён.");
                        DbReader.Updateuserstate(message.chat.id, 5);
                    }
                }

            }
            catch (Exception ex)
            {
                DbReader.Updateuserstate(message.chat.id, 3);
                _bot.SendMessage(message.chat.id, "Что-то пошло не так при обработке сообщения.", "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");

                Logger.LogException("error", message.chat.id, "ProcessMessage", ex.Message, "");
                Thread.Sleep(5000);
            }
        }
        
    }
}
