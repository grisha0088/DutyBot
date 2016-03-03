﻿using System;
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
            try  //этот код создаёт и конфигурирует службу DutyBot, используется Topshelf
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
                    Thread.Sleep(10000); // еcли не доступна БД и не получается залогировать запуск, ждём 10 секунд и пробуем еще раз.
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
        private TelegramBot _bot; // бот telegramm
        private Jira _jiraConn;  // коннект а jira
        private Issue _issue; // Тикет в jira. Используется для передачи тикета из потока _checkTicketsThread в _readmessageThread
        private Issue _ticket; //тикет в jira, используется в потоке при проверке тикетов вручную
        private bool _readmessagesflag = true; //пока true, работает потог чтения сообщений
        private bool _checkjiraflag = true; //пока true, работает потог проверки jira

        public void Checkjira() // метод запускается в потоке _checkTicketsThread и вычитывает тикеты из фильтра в jira
        {
            _jiraConn = Jira.CreateRestClient(DbReader.Readjira(), DbReader.Readdefaultuser(), DbReader.Readdefaultpassword());
            while (_checkjiraflag)
            {
                try  //если дежурство закончилось сбрасываю статус пользователя на 3
                {
                    using (var repository = new Repository<DutyBotDbContext>())
                    {
                        var users = repository.GetList<User>(u => u.DutyEnd < DateTime.Now & u.State == 5);
                        foreach(var user in users)
                        {
                            user.State = 3;
                            repository.Update<User>(user);
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

                    using (var repository = new Repository<DutyBotDbContext>())
                    {
                        var users =
                            repository.GetList<User>(
                                u => u.DutyStart > DateTime.Now & u.DutyEnd < DateTime.Now & u.State == 5);
                        if (enumerable.Any() && users.Any())
                        {
                            _issue = enumerable.Last();
                            foreach (var user in users)
                            {
                                _jiraConn = Jira.CreateRestClient(DbReader.Readjira(), user.Login, user.Password);
                                user.TicketNumber = _issue.Key.ToString();
                                _bot.SendMessage(user.TlgNumber, _issue);
                                repository.Update<User>(user);
                            }
                        }
                        else
                        {
                            Thread.Sleep(10000);
                        }
                    }
                }
            
                catch (Exception ex)
                {
                    using (var repository = new Repository<DutyBotDbContext>())
                    {
                        var users =
                            repository.GetList<User>(
                                u => u.DutyStart > DateTime.Now & u.DutyEnd < DateTime.Now & u.State == 5);
                        if (users.Any())
                        {
                            foreach (var user in users)
                            {
                                _bot.SendMessage(user.TlgNumber,
                                    "Похоже, что jira не доступна. Мониторинг остановлен. Что будем делать?",
                                    "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                                user.State = 3;
                                repository.Update<User>(user);
                            }
                        }
                        Logger.LogException("error", 1, "ReadTicketsFromJira", ex.Message, "");
                        Thread.Sleep(30000);
                    }
                }
            }
        }

        public void Readmessages()
        {
            var offset = 0;  //Id последнего прочитанного сообщения из telegramm
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

        private void Processmessage(Message message) // Вся логика обработки обращений. Завязана на статус пользователя и его сообщение в telegramm. 
        {
            User user;
            try
            {

                using (var repository = new Repository<DutyBotDbContext>())
                {
                    user = repository.Get<User>(u => u.TlgNumber == message.chat.id);
                    if (user == null)
                    {
                        user = new User
                        {
                            Name = message.chat.first_name,
                            TlgNumber = message.chat.id,
                            State = -1,
                            Login = "",
                            Password = "",
                            TicketNumber = ""
                        };
                        repository.Create<User>(user);
                    }
                }   
            }
            catch (Exception ex)
            {
                _bot.SendMessage(message.chat.id, "Что-то пошло не так при обращении. Дальнейшая работа не возможна.");

                Logger.LogException("error", message.chat.id, "CreateUserObject", ex.Message, "");
                Thread.Sleep(5000);
                return;
            }
            try
            {
                using (var repository = new Repository<DutyBotDbContext>())
                {
                    user = repository.Get<User>(u => u.Id == user.Id);
                    switch (user.State) //смотрю, в каком статусе пользователь 
                        //- 1 его нет, 0 ззнакомство с DUtyBot, 1 Ввод пароля, 2, проверка доступа в jira, 
                        //3 основной статус, ожидаем команды, 4 пользователь решает что делать с тикетом, который получил от бота по кнопке Проверь тикеты
                        //5 идёт мониторинг тикетов, пользователь получает уведомления, 6 пользователь решает что делать с тикетом, который получил при мониторинге
                    {
                        case -1:
                            _bot.SendMessage(message.chat.id,
                                "Привет, " + message.chat.first_name +
                                @"! Меня зовут DutyBot, я создан, чтобы помогать дежурить. Давай знакомиться!
 ","{\"keyboard\": [[\"Рассказать DutyBot'у о себе\"], [\"Не хочу знакомиться, ты мне не нравишься\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                            user.State = 0;
                            break;
                        case 0:
                            if ((message.text) == "Рассказать DutyBot'у о себе" |
                                (message.text) == "Ввести учётку еще раз")
                            {
                                _bot.SendMessage(message.chat.id,
                                    @"Чтобы мы могли работать в Jira, мне нужны твои учётные данные. Напиши мне свой логин в формате d.bot и нажми отправить.
Твои данные будут относительно безопасно храниться на сервере");
                                user.Name = message.chat.first_name;
                                user.TlgNumber = message.chat.id;
                                user.State = 1;
                                break;
                            }

                            if ((message.text) == "Не хочу знакомиться, ты мне не нравишься")
                            {
                                _bot.SendMessage(message.chat.id,
                                    @"Очень жаль, но если надумешь, пиши. Я забуду об этом неприятном разговоре");
                                break;
                            }

                            break;
                        case 1:
                            _bot.SendMessage(message.chat.id, @"Теперь напиши пароль");

                            user.Login = message.text;
                            user.State = 2;
                            break;
                        case 2:

                            user.Password = message.text;

                            _bot.SendMessage(message.chat.id, @"Отлично, сейчас я проверю, есть ли у тебя доступ в Jira");
                            if (JiraAddFuncions.CheckConnection(_jiraConn, user.Login,
                                user.Password))
                            {
                                _bot.SendMessage(message.chat.id, @"Всё хорошо, можно начинать работу",
                                    "{\"keyboard\": [[\"Начнём\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");

                                user.Password = message.text;
                                user.State = 3;
                                break;
                            }
                            else
                            {
                                _bot.SendMessage(message.chat.id,
                                    @"Доступа к JIra нет. Возможно учётные данные не верны. Давай попробуем ввести их еще раз. ",
                                    "{\"keyboard\": [[\"Ввести учётку еще раз\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");

                                user.State = 0;
                                break;
                            }
                        case 3:
                            switch (message.text)
                            {
                                case "Начнём":
                                    _bot.SendMessage(message.chat.id,
                                        "Просто напиши мне сообщение, когда я тебе понадоблюсь. В ответ я пришлю меню с вариантами моих действий. Вот такое ↓",
                                        "{\"keyboard\": [[\"Кто сейчас дежурит?\"], [\"Проверь тикеты\"], [\"Помоги с дежурством\"], [\"Пока ничем\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                                    break;
                                case "Кто сейчас дежурит?":
                                    _bot.SendMessage(message.chat.id, DbReader.Readrespersone(),
                                        "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                                    break;
                                case "Проверь тикеты":
                                    _jiraConn = Jira.CreateRestClient(DbReader.Readjira(),
                                        user.Login, user.Password);

                                    try
                                    {
                                        var issues =
                                            _jiraConn.GetIssuesFromJql(DbReader.Readfilter());
                                        IList<Issue> enumerable = issues as IList<Issue> ?? issues.ToList();
                                        if (enumerable.Any())
                                        {
                                            _ticket = enumerable.Last();

                                            user.TicketNumber = _ticket.Key.ToString();
                                            user.State = 4;
                                            _bot.SendMessage(message.chat.id, _ticket);
                                          }
                                        else
                                        {
                                            _bot.SendMessage(message.chat.id, "Тикетов нет",
                                                "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        _bot.SendMessage(message.chat.id, "Jira не доступна",
                                            "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                                        Logger.LogException("error", 1, "SendThatJiraDoesNotWork", ex.Message, "");
                                    }

                                    break;
                                case "Помоги с дежурством":
                                    _bot.SendMessage(message.chat.id, "Как будем дежурить?",
                                        "{\"keyboard\": [[\"Начать мониторить тикеты\"], [\"Мониторить тикеты в моё дежурство\"], [\"Отмена\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                                    break;
                                case "Пока ничего":
                                    _bot.SendMessage(message.chat.id, "Ок, если что, пиши.");
                                    break;
                                case "Начать мониторить тикеты":
                                    _bot.SendMessage(message.chat.id, @"Начинаю мониторинг.
Я буду мониторить тикеты в течение ближайших 12 часов, после чего мониторинг будет автоматически остановлен.");
                                    user.State = 5;
                                    user.DutyStart = DateTime.Now;
                                    user.DutyEnd = DateTime.Now.AddHours(12);
                                    break;
                                case "Мониторить тикеты в моё дежурство":

                                    if (DbReader.Readuserdutystart(message.chat.id) ==
                                        Convert.ToDateTime("01.01.1900 0:00:00 ") |
                                        DbReader.Readuserdutystart(message.chat.id) ==
                                        Convert.ToDateTime("01.01.1900 0:00:00 "))
                                    {
                                        _bot.SendMessage(message.chat.id, "У тебя нет дежурств в ближайшее время",
                                            "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                                        break;
                                    }
                                    else
                                    {
                                        _bot.SendMessage(message.chat.id,
                                            "Я буду мониторить тикеты с " +
                                            DbReader.Readuserdutystart(message.chat.id).ToShortDateString() + " " +
                                            DbReader.Readuserdutystart(message.chat.id).ToShortTimeString() + " по " +
                                            DbReader.Readuserdutyend(message.chat.id).ToShortDateString() + " " +
                                            DbReader.Readuserdutyend(message.chat.id).ToShortTimeString());

                                        user.State = 5;
                                        user.DutyStart = DbReader.Readuserdutystart(message.chat.id);
                                        user.DutyEnd = DbReader.Readuserdutyend(message.chat.id);
                                        break;
                                    }
                                default:
                                    _bot.SendMessage(message.chat.id, "Чем я могу помочь?",
                                "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                               break;
                            }
                            
                            break;
                        case 4:
                            if (_ticket.Key.ToString().Equals(user.TicketNumber))
                            {
                                switch ((message.text))
                                {
                                    case "Распределить":
                                        _bot.SendMessage(message.chat.id, "Кому назначим?",
                                            "{\"keyboard\": [[\"Технологи\", \"Коммерция\"], [\"Админы\", \"Связисты\"], [\"Олеся\", \"Женя\"], [\"Алексей\", \"Максим\"], [\"Паша\", \"Марина\"], [\"Андрей\", \"Гриша\"], [\"Оля\", \"Настя\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                                        break;
                                    case "Решить":
                                        JiraAddFuncions.ResolveTicket(user, _ticket, message, user.Login, _bot, _jiraConn);
                                        break;
                                    case "Назначить себе":
                                        JiraAddFuncions.AssingTicket(user, _ticket, message, user.Login, _bot, _jiraConn);
                                        break;
                                    case "Технологи":
                                        JiraAddFuncions.AssingTicket(user, _ticket, message, "tecnologsupport", _bot,
                                            _jiraConn);
                                        break;
                                    case "Коммерция":
                                        JiraAddFuncions.AssingTicket(user, _ticket, message, "crm_otdel", _bot, _jiraConn);
                                        break;
                                    case "Админы":
                                        JiraAddFuncions.AssingTicket(user, _ticket, message, "Uk.Jira.TechSupport", _bot,
                                            _jiraConn);
                                        break;
                                    case "Связисты":
                                        JiraAddFuncions.AssingTicket(user, _ticket, message, "uk.jira.noc", _bot,
                                            _jiraConn);
                                        break;
                                    case "Олеся":
                                        JiraAddFuncions.AssingTicket(user, _ticket, message, "o.likhacheva", _bot,
                                            _jiraConn);
                                        break;
                                    case "Женя":
                                        JiraAddFuncions.AssingTicket(user, _ticket, message, "ev.safonov", _bot, _jiraConn);
                                        break;
                                    case "Алексей":
                                        JiraAddFuncions.AssingTicket(user, _ticket, message, "a.sapotko", _bot, _jiraConn);
                                        break;
                                    case "Максим":
                                        JiraAddFuncions.AssingTicket(user, _ticket, message, "m.shemetov", _bot, _jiraConn);
                                        break;
                                    case "Андрей":
                                        JiraAddFuncions.AssingTicket(user, _ticket, message, "an.zarubin", _bot, _jiraConn);
                                        break;
                                    case "Гриша":
                                        JiraAddFuncions.AssingTicket(user, _ticket, message, "g.dementiev", _bot,
                                            _jiraConn);
                                        break;
                                    case "Оля":
                                        JiraAddFuncions.AssingTicket(user, _ticket, message, "o.tkachenko", _bot,
                                            _jiraConn);
                                        break;
                                    case "Настя":
                                        JiraAddFuncions.AssingTicket(user, _ticket, message, "a.zakharova", _bot,
                                            _jiraConn);
                                        break;
                                    case "Марина":
                                        JiraAddFuncions.AssingTicket(user, _ticket, message, "m.vinnikova", _bot,
                                            _jiraConn);
                                        break;
                                    case "Паша":
                                        JiraAddFuncions.AssingTicket(user, _ticket, message, "p.denisov", _bot, _jiraConn);
                                        break;
                                }
                            }
                            else
                            {
                                _bot.SendMessage(message.chat.id, "Похоже, тикет уже распределён.");
                                user.State = 3;
                            }
                            break;
                        case 5:
                            switch (message.text)
                            {
                                case "Распределить":
                                    _bot.SendMessage(message.chat.id, "Кому назначим?",
                                        "{\"keyboard\": [[\"Технологи\", \"Коммерция\"], [\"Админы\", \"Связисты\"], [\"Олеся\", \"Женя\"], [\"Алексей\", \"Максим\"], [\"Паша\", \"Марина\"], [\"Андрей\", \"Гриша\"], [\"Оля\", \"Настя\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                                    break;
                                case "Решить":
                                    JiraAddFuncions.ResolveTicket(user, _issue, message, user.Login, _bot, _jiraConn);
                                    break;
                                case "Назначить себе":
                                    JiraAddFuncions.AssingTicket(user, _issue, message, user.Login, _bot, _jiraConn);
                                    break;
                                case "Технологи":
                                    JiraAddFuncions.AssingTicket(user, _issue, message, "tecnologsupport", _bot,
                                        _jiraConn);
                                    break;
                                case "Коммерция":
                                    JiraAddFuncions.AssingTicket(user, _issue, message, "crm_otdel", _bot, _jiraConn);
                                    break;
                                case "Админы":
                                    JiraAddFuncions.AssingTicket(user, _issue, message, "Uk.Jira.TechSupport", _bot,
                                        _jiraConn);
                                    break;
                                case "Связисты":
                                    JiraAddFuncions.AssingTicket(user, _issue, message, "uk.jira.noc", _bot, _jiraConn);
                                    break;
                                case "Олеся":
                                    JiraAddFuncions.AssingTicket(user, _issue, message, "o.likhacheva", _bot, _jiraConn);
                                    break;
                                case "Женя":
                                    JiraAddFuncions.AssingTicket(user, _issue, message, "ev.safonov", _bot, _jiraConn);
                                    break;
                                case "Алексей":
                                    JiraAddFuncions.AssingTicket(user, _issue, message, "a.sapotko", _bot, _jiraConn);
                                    break;
                                case "Максим":
                                    JiraAddFuncions.AssingTicket(user, _issue, message, "m.shemetov", _bot, _jiraConn);
                                    break;
                                case "Андрей":
                                    JiraAddFuncions.AssingTicket(user, _issue, message, "an.zarubin", _bot, _jiraConn);
                                    break;
                                case "Гриша":
                                    JiraAddFuncions.AssingTicket(user, _issue, message, "g.dementiev", _bot, _jiraConn);
                                    break;
                                case "Оля":
                                    JiraAddFuncions.AssingTicket(user, _issue, message, "o.tkachenko", _bot, _jiraConn);
                                    break;
                                case "Настя":
                                    JiraAddFuncions.AssingTicket(user, _issue, message, "a.zakharova", _bot, _jiraConn);
                                    break;
                                case "Марина":
                                    JiraAddFuncions.AssingTicket(user, _issue, message, "m.vinnikova", _bot, _jiraConn);
                                    break;
                                case "Паша":
                                    JiraAddFuncions.AssingTicket(user, _issue, message, "p.denisov", _bot, _jiraConn);
                                    break;
                                case "Остановить мониторинг":
                                {
                                    user.State = 3;
                                    _bot.SendMessage(message.chat.id, "Готово");
                                    break;
                                }

                                default:
                                    _bot.SendMessage(message.chat.id, "да?",
                                        "{\"keyboard\": [[\"Остановить мониторинг\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                                    break;
                            }
                            break;
                        case 6:
                            if (_issue.Key.ToString().Equals(user.TicketNumber))
                            {
                                switch (message.text)
                                {

                                    case ("Остановить мониторинг"):
                                    {
                                        _bot.SendMessage(message.chat.id, "Готово");
                                        user.State = 3;
                                        break;
                                    }
                                    default:
                                        _bot.SendMessage(message.chat.id, "да?",
                                            "{\"keyboard\": [[\"Остановить мониторинг\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                                        break;
                                }
                            }
                            else
                            {
                                _bot.SendMessage(message.chat.id, "Похоже, тикет уже распределён.");
                                user.State = 5;
                            }
                            break;
                    }
                    repository.Update<User>(user);
                }
            }
            catch (Exception ex)
            {
                using (var repository = new Repository<DutyBotDbContext>())
                {
                    user.State = 3;
                    _bot.SendMessage(message.chat.id, "Что-то пошло не так при обработке сообщения.",
                        "{\"keyboard\": [[\"Проверь тикеты\"], [\"Кто сейчас дежурит?\"], [\"Помоги с дежурством\"], [\"Пока ничего\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
                    Logger.LogException("error", message.chat.id, "ProcessMessage", ex.Message, "");
                    repository.Update<User>(user);
                }
                Thread.Sleep(5000);
            }
        }
        
    }
}
