using System;
using Newtonsoft.Json;
using Atlassian.Jira;

namespace Telegram
{
    internal class TelegramBot 
    {
        readonly string _botkey;
        public TelegramBot(string botkey)
        {
            try
            {
                _botkey = botkey;
            }
            catch (Exception ex)
            {
                Sender.Send("https://api.telegram.org/bot179261100:AAGqaQ8Fum0xK8JQL0FE_N4LugS_MmO36zM/sendmessage?chat_id=38651047&text=" + ex.Message);
            }
        }

        public void SendMessage(int chatid, string message, string replyMarkup)
        {
            Sender.Send("https://api.telegram.org/bot" + _botkey + "/sendmessage?chat_id=" + chatid + "&text=" + message + "&replyMarkup=" + replyMarkup);
        }

        public void SendMessage(int chatid, string message)
        {
            Sender.Send("https://api.telegram.org/bot" + _botkey + "/sendmessage?chat_id=" + chatid + "&text=" + message);
        }

        public void SendMessage(int chatid, Issue tc)
        {

            SendMessage(chatid, tc.Key + @"
            " + tc.Reporter + @"
            " + tc.Summary + @"
            " + tc.Description, "{\"keyboard\": [[\"Распределить\"], [\"Решить\"], [\"Назначить себе\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
   
        }

        public updates Getupdates(int offset)
        {
            updates newupdate = JsonConvert.DeserializeObject<updates>(Sender.Send("https://api.telegram.org/bot" + _botkey + "/Getupdates?offset=" + offset));
            return newupdate;
        }

    }
}  
