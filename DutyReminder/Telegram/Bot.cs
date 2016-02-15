using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Atlassian.Jira;

namespace Telegram
{
    class TelegramBot 
    {
        string botkey;
        public TelegramBot(string botkey)
        {
            try
            {
                this.botkey = botkey;
            }
            catch (Exception ex)
            {
                Sender.send("https://api.telegram.org/bot179261100:AAGqaQ8Fum0xK8JQL0FE_N4LugS_MmO36zM/sendmessage?chat_id=38651047&text=" + ex.Message, "");
            }
        }

        public void SendMessage(int chatid, string message, string reply_markup)
        {
            Sender.send("https://api.telegram.org/bot" + botkey + "/sendmessage?chat_id=" + chatid + "&text=" + message + "&reply_markup=" + reply_markup, "");
        }

        public void SendMessage(int chatid, string message)
        {
            Sender.send("https://api.telegram.org/bot" + botkey + "/sendmessage?chat_id=" + chatid + "&text=" + message, "");
        }

        public void SendMessage(int chatid, Issue tc)
        {

            SendMessage(chatid, tc.Key.ToString() + @"
            " + tc.Reporter + @"
            " + tc.Summary + @"
            " + tc.Description, "{\"keyboard\": [[\"Распределить\"], [\"Решить\"], [\"Назначить себе\"]],\"resize_keyboard\":true,\"one_time_keyboard\":true}");
   
        }

        public updates getupdates(int offset)
        {
            updates newupdate = JsonConvert.DeserializeObject<updates>(Sender.send("https://api.telegram.org/bot" + botkey + "/getupdates?offset=" + offset));
            return newupdate;
        }

    }
}  
