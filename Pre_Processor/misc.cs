using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pre_Processor
{
    public class ms
    {
        static CPUTILLib.CpStockCode _cpstockcode;

        public static bool is_stock(string stock)
        {
            _cpstockcode = new CPUTILLib.CpStockCode();
            if (stock == "")
                return false;


            string code = _cpstockcode.NameToCode(stock); // 코스피혼합, 코스닥혼합 code.Length = 0 제외될 것임
            if (code.Length == 7)
                return true;
            else
                return false;
        }

        public static string six_digit_integer_time_to_string_time(int value)
        {
            int sec = value % 100;
            int min = value % 10000 / 100;
            int hour = value / 10000;
            return hour + ":" + min + ":" + sec;
        }

        public static int time_to_int(string value)
        {
            string[] words = value.Split(':');
            return Convert.ToInt32(words[0]) * 10000 +
                Convert.ToInt32(words[1]) * 100 +
                Convert.ToInt32(words[2]);
        }

        public static double total_Seconds(int from, int to)
        {
            string string_type_from = six_digit_integer_time_to_string_time(from);
            string string_type_to = six_digit_integer_time_to_string_time(to);
            double total_seconds = DateTime.Parse(string_type_to).Subtract(DateTime.Parse(string_type_from)).TotalSeconds;
            return total_seconds;
        }
    }
}
