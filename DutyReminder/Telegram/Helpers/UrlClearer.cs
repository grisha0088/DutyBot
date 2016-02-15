using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegram
{
    static class UrlClearer
    {
        public static string Clear(string str)
        {
            str = str.Replace("#", "%23");
            str = str.Replace(@"

", "");
            str = str.Replace("http://www.2gis.ru/", "2gis.ru");
            
            return str;
        }
    }
}
