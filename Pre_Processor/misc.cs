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

       
     
        public static int current_date()
        {
            // 토, 일요일이면 시작판의 Default 시작날짜를 금요일로 수정
            // 평일인 데 900 이전은 전일로 수정 (당일 데이터 부재하므로)
            DateTime date = DateTime.Now;

            if (date.DayOfWeek == DayOfWeek.Sunday)
            {
                date = DateTime.Now.Date.AddDays(-2);
            }
            else if (date.DayOfWeek == DayOfWeek.Saturday)
            {
                date = DateTime.Now.Date.AddDays(-1);
            }
            else
            {
                int HHmm = Convert.ToInt32(DateTime.Now.ToString("HHmm"));
                if (HHmm < 855) // 855 이전이면 전일 데이터 사용, 아니면 당일 데이터 다운로드 시작 // 
                {
                    if (date.DayOfWeek == DayOfWeek.Monday)
                    {
                        date = DateTime.Now.Date.AddDays(-3);
                    }
                    else
                    {
                        date = DateTime.Now.Date.AddDays(-1);
                    }
                }
                else
                {
                    date = DateTime.Now.Date;
                }
            }

            return Convert.ToInt32(date.ToString("yyyyMMdd"));
        }

        public static void dgvs_color(DataGridView dgv1, DataGridView dgv3, DataGridView dgv4)
        {
            if (g.confirm_sell)
            {
                if (g.hoga[0].dgv.RowCount > 0) dgv1.Columns[0].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                if (dgv3.RowCount > 0) dgv3.Columns[0].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                if (dgv4.RowCount > 0) dgv4.Columns[0].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
            }
            else
            {
                if (g.hoga[0].dgv.RowCount > 0) dgv1.Columns[0].DefaultCellStyle.BackColor = Color.FromArgb(175, 255, 255);
                if (dgv3.RowCount > 0) dgv3.Columns[0].DefaultCellStyle.BackColor = Color.FromArgb(175, 255, 255);
                if (dgv4.RowCount > 0) dgv4.Columns[0].DefaultCellStyle.BackColor = Color.FromArgb(175, 255, 255);
            }

            if (g.confirm_buy)
            {
                if (g.hoga[0].dgv.RowCount > 0) g.hoga[0].dgv.Columns[2].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                if (dgv3.RowCount > 0) dgv3.Columns[2].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                if (dgv4.RowCount > 0) dgv4.Columns[2].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);

            }
            else
            {
                if (g.hoga[0].dgv.RowCount > 0) g.hoga[0].dgv.Columns[2].DefaultCellStyle.BackColor = Color.FromArgb(255, 175, 255);
                if (dgv3.RowCount > 0) dgv3.Columns[2].DefaultCellStyle.BackColor = Color.FromArgb(255, 175, 255);
                if (dgv4.RowCount > 0) dgv4.Columns[2].DefaultCellStyle.BackColor = Color.FromArgb(255, 175, 255);
            }
        }
        public static void Sound_돈(int total_amount)
        {
            switch (total_amount)
            {
                case 0:
                    ms.Sound("돈", "single stock");
                    break;
                case 100:
                    ms.Sound("돈", "one hundred");
                    break;
                case 500:
                    ms.Sound("돈", "five hundred");
                    break;
                case 1000:
                    ms.Sound("돈", "one thousand");
                    break;
                case 2000:
                    ms.Sound("돈", "two thousand");
                    break;
                case 4000:
                    ms.Sound("돈", "four thousand");
                    break;
                case 8000:
                    ms.Sound("돈", "eight thousand");
                    break;
                case 16000:
                    ms.Sound("돈", "sixteen thousand");
                    break;
                case 32000:
                    ms.Sound("돈", "thirty two thousand");
                    break;
                case 64000:
                    ms.Sound("돈", "sixty four thousand");
                    break;
                default:
                    ms.Sound("돈", "limit exceed");
                    total_amount = 0;
                    Thread.Sleep(1000);
                    ms.Sound("돈", "single stock");
                    break;
            }
        }
        public static void toggle_gq(string given_gq, int row_id, int col_id)
        {
            switch (given_gq)
            {
                case "a&g":
                    g.q = "a&s";
                    break;
                case "a&s":
                    g.q = "a&g";
                    break;

                case "h&s":
                    g.q = "h&g";
                    int clicked_sequence = g.nRow * col_id + row_id;
                    g.saved_hs_date = g.date;
                    g.saved_stock = g.clicked_Stock;
                    g.date = g.date_list[clicked_sequence];
                    break;
                case "h&g":
                    g.q = "h&s";
                    g.date = g.saved_hs_date;
                    g.clicked_Stock = g.saved_stock;
                    break;

                case "kodex_leverage_single":
                    g.q = "o&g";
                    break;
                case "kodex_inverse_single":
                    g.q = "o&g";
                    break;
                case "kodex_kospi_group":
                    g.q = "o&g";
                    break;
                case "kodex_kosdaq_group":
                    g.q = "o&g";
                    break;

                case "o&s":
                    g.q = "o&g";
                    //g.SavedKeyString = g.KeyString;
                    g.info.dtb.Rows[0][1] = g.clicked_title;
                    break;
                case "o&g":
                    g.q = "o&s";
                    //g.KeyString  = g.SavedKeyString;
                    g.info.dtb.Rows[0][1] = g.KeyString;
                    break;

                case "s&g":
                    g.q = "s&s";
                    break;
                case "s&s":
                    g.q = "s&g";
                    break;
            }
        }

        // 코스피혼합 코스닥혼합 계산
        public static string textbox_date()
        {
            if (g.q == "o&g" || g.q == "s&g" || g.q == "a&g" || g.q == "h&g")
                return g.clicked_title;

            else
            {
                string t = g.current_key_char.ToString();
                switch (t)
                {
                    case "e":
                    case "s":
                        t = "프누";
                        break;
                    case "ㄷ":
                    case "ㄴ":
                        t = "종누";
                        break;
                    case "a":
                        t = "프퍼";
                        break;
                    case "d":
                        t = "프분";
                        break;
                    case "ㅇ":
                        t = "배차";
                        break;
                }
                return g.date.ToString().Substring(g.date.ToString().Length - 4) + "(" + t + ")";

            }
        }



        public static int index_of_optimum_time_interval(g.stock o)
        {
            for (int i = 1; i < g.array_size; i++)
            {
                if (o.틱의시간[i] == 0) // if not all data is filled and the last time not exceed 30 secs, return the last index filled
                {
                    return i - 1;
                }
                double elapsed_seconds = ms.total_Seconds(o.틱의시간[0], o.틱의시간[i]); // if elapsed_secons > 30, then return the index
                if (elapsed_seconds > 30)
                    return i;
            }
            return g.array_size - 1; // this is not possible, but in the case
        }


       

        // Not used
        public static void revovling_naver(int kospi_or_kosdaq)
        {

            wk.시총순서(g.sl); // revolving_naver

            if (kospi_or_kosdaq == 1) // kospi
            {
                for (int i = g.revolving_number_for_kospi; i <= g.sl.Count; i++)
                {
                    if (i == g.sl.Count)
                        g.revolving_number_for_kospi = 0;

                    if (rd.read_코스피코스닥시장구분(g.sl[i]) == 'S')
                    {
                        wk.call_네이버(g.sl[i], 0); // second and third vaiable does not have meaning
                        g.revolving_number_for_kospi = i + 1;
                        break;
                    }
                }
            }
            else // kosdaq
            {
                for (int i = g.revolving_number_for_kosdaq; i <= g.sl.Count; i++)
                {
                    if (i == g.sl.Count)
                        if (i == g.sl.Count)
                            g.revolving_number_for_kosdaq = 0;

                    if (rd.read_코스피코스닥시장구분(g.sl[i]) == 'D')
                    {
                        wk.call_네이버(g.sl[i], 0); // second and third vaiable does not have meaning
                        g.revolving_number_for_kosdaq = i + 1;
                        break;
                    }
                }
            }
        }


        // 천만원 단위로 리턴




        public static string message(string caption, string message, string button_selection)
        {
            DialogResult result;
            if (button_selection == "Yes")
                result = MessageBox.Show(message, caption, MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            else
                result = MessageBox.Show(message, caption, MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);


            if (result == System.Windows.Forms.DialogResult.No)
                return "No";
            else
                return "Yes";
        }

        public static void message(string message)
        {
            MessageBox.Show(message);
        }

        public static void Sound(string sub_directory, string sound)
        {
            if (sound == "")
                return;

            string sound_file;
            if (sub_directory == "")
                sound_file = @"C:\병신\소\" + sound + ".wav";
            else
                sound_file = @"C:\병신\소\" + sub_directory + "\\" + sound + ".wav";

            if (!File.Exists(sound_file))
            {
                return;
            }

            System.Media.SoundPlayer player = new System.Media.SoundPlayer();
            player.SoundLocation = sound_file;

            player.Play();
        }


        public static void process_start(int date, string stock)
        {
            int index = wk.return_index_of_ogldata(stock);

            if (index < 0) // 혼합과 ogl_data 등록된 종목만 draw
            {
                return;
            }

            g.stock o = g.ogl_data[index];

            string temp_file = @"C:\병신\" + "temp" + ".txt";

            lock (g.lockObject)
            {
                if (File.Exists(temp_file))
                {
                    File.Delete(temp_file);
                }
            }

            Stream FS = new FileStream(temp_file, FileMode.CreateNew, FileAccess.Write);
            StreamWriter sw = new System.IO.StreamWriter(FS, System.Text.Encoding.Default);

            if (g.testing)
            {
                int[,] x = new int[g.MAX_ROW, 12];
                int nrow = rd.read_Stock_Minute(date, stock, x);

                if (nrow <= 1)
                {
                    return;
                }
                for (int i = g.time[0]; i < g.time[1]; i++)
                {
                    if (x[i, 0] == 0) // lines less than g.time[1]
                        break;
                    for (int j = 0; j < 12; j++)
                    {
                        sw.Write("{0}\t", x[i, j]);
                    }
                    sw.WriteLine();
                }
            }
            else
            {
                for (int i = 0; i < o.nrow; i++)
                {
                    if (i >= g.time[0] && i < g.time[1])
                    {
                        for (int j = 0; j < 12; j++)
                        {
                            sw.Write("{0}\t", o.x[i, j]);
                        }
                        sw.WriteLine();
                    }
                }
            }
            sw.Close();
            Process.Start(temp_file);
        }


        // 지수 혼합 설정
        public static void setting_코스피_코스닥합성()
        {
            string file;
            file = @"C:\병신\data\KODEX_FACTOR" + ".txt";

            if (!File.Exists(file))
                File.Create(file).Dispose();
            string str_add = "";

            List<string> copy = new List<string>(g.코스피합성);
            double total_value = 0;
            foreach (string t in copy)
            {
                string[] words = t.Split('\t');

                int index = wk.return_index_of_ogldata(words[0]);
                if (index < 0) continue;

                total_value += (int)g.ogl_data[index].전일종가 * int.Parse(words[1], NumberStyles.AllowThousands);
            }
            g.코스피합성.Clear();
            foreach (string t in copy)
            {
                string[] words = t.Split('\t');

                int index = wk.return_index_of_ogldata(words[0]);
                if (index < 0) continue;

                double individual_value = g.ogl_data[index].전일종가 * Convert.ToDouble(words[1]);
                double factor = individual_value / total_value;
                g.코스피합성.Add(words[0] + "\t" + factor.ToString());

                string x = (factor * 100).ToString("#.#") + "%" + "\n";
                str_add += String.Format("{0, -20}  {1, 10}", words[0], x);
            }
            str_add += "\n";

            List<string> copy1 = new List<string>(g.코스닥합성);
            total_value = 0;
            foreach (string t in copy1)
            {
                string[] words = t.Split('\t');

                int index = wk.return_index_of_ogldata(words[0]);
                if (index < 0)
                    continue;

                total_value += g.ogl_data[index].전일종가 * Convert.ToDouble(words[1]);
            }
            g.코스닥합성.Clear();
            foreach (string t in copy1)
            {
                string[] words = t.Split('\t');

                int index = wk.return_index_of_ogldata(words[0]);
                if (index < 0)
                    continue;

                double individual_value = g.ogl_data[index].전일종가 * Convert.ToDouble(words[1]);
                double factor = individual_value / total_value;
                g.코스닥합성.Add(words[0] + "\t" + factor.ToString());

                string x = (factor * 100).ToString("#.#") + "%" + "\n";
                str_add += String.Format("{0, -20}  {1}", words[0], x);
            }

            File.WriteAllText(file, str_add);
        }


        public static void setting_kodex_magnifier_shifter()
        {
            // KODEX 
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (i == 0 || i == 2)
                    {
                        if (j == 2)
                            g.k.magnifier[i, j] = 1.0;
                        else
                            g.k.magnifier[i, j] = 1.0;
                    }

                    else
                        g.k.magnifier[i, j] = 1.0;

                    g.k.shifter[i, j] = 0;
                }
            }
        }





        //public static void read_or_set_stocks_Empty_Creation(string file)
        //{
        //    File.Create(file).Close();
        //    string minutestr = "85959\t0\t100\t10000\t0\t0\t0\t0\t0\t0\t0\t0"; // 12 items
        //    using (StreamWriter w = File.AppendText(file))
        //    {
        //        w.WriteLine("{0}", minutestr);
        //        w.Close(); 
        //}





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
        public static double total_Seconds(string from, string to)
        {
            return DateTime.Parse(to).Subtract(DateTime.Parse(from)).TotalSeconds;
        }





        //      public static void divide_그룹()
        //{
        //	_cpstockcode = new CPUTILLib.CpStockCode();

        //	int accumulated = 0;
        //	int ncodes = 0;
        //	bool first = true;

        //	for (int i = 0; i < g.sl.Count; i++)
        //	{
        //		string code = _cpstockcode.NameToCode(g.sl[i]);

        //		if (first)
        //		{
        //			g.codes[ncodes] += code;
        //			first = false;
        //		}
        //		else
        //		{
        //			g.codes[ncodes] += "," + code;
        //		}

        //		accumulated++;

        //		if (accumulated == g.stocksinCode)
        //		{
        //			accumulated = 0;
        //			ncodes++;
        //			first = true;
        //		}
        //	}
        //}

        //public static void divide_그룹_기()
        //{
        //	_cpstockcode = new CPUTILLib.CpStockCode();

        //	int accumulated = 0;
        //	int ncodes = 0;
        //	bool first = true;

        //	for (int i = 0; i < g.sl.Count; i++)
        //	{
        //		string code = _cpstockcode.NameToCode(g.sl[i]);

        //		if (first)
        //		{
        //			g.codes[ncodes] += code;
        //			first = false;
        //		}
        //		else
        //		{
        //			g.codes[ncodes] += "," + code;
        //		}

        //		accumulated++;

        //		if (accumulated == g.stocksinCode)
        //		{
        //			accumulated = 0;
        //			ncodes++;
        //			first = true;
        //		}
        //	}
        //}


        /*
		public void buy_종목 (string stock)
		{
		DateTime date = DateTime.Now; // Or whatever
		string temp = date.ToString("HHmm") + " " + stock + " :  buy";
		string path = @"C:\병신\매매.txt";
		StreamWriter sw = File.AppendText(path);
		sw.WriteLine("{0}", temp);
		sw.Close();
		}


		public void sell_종목(string stock)
		{
		DateTime date = DateTime.Now; // Or whatever
		string temp = date.ToString("HHmm") + " " + stock + " : sell";
		string path = @"C:\병신\매매.txt";
		StreamWriter sw = File.AppendText(path);
		sw.WriteLine("{0}", temp);
		sw.Close();
		}
		*/

    }
}
