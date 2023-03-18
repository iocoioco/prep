using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pre_Processor
{
    internal class rd
    {
        static CPUTILLib.CpCodeMgr _cpcodemgr;
        static CPUTILLib.CpStockCode _cpstockcode;



        public static void read_관심제거추가(string stock)
        {
            return;

            DateTime date = DateTime.Now;
            int HHmmss = Convert.ToInt32(date.ToString("HHmmss"));

            foreach (var item in g.관심삭제) // 3분 이상 지난 종목은 제외
            {
                if (HHmmss - item.Value > 300)
                {
                    g.관심삭제.Remove(item.Key);
                }
            }

            g.관심삭제.Remove(stock); // by key, if exist remove
            g.관심삭제.Add(stock, HHmmss);
        }

        public static bool read_관심제거여부(string stock)
        {
            return false;

            DateTime date = DateTime.Now;
            int HHmmss = Convert.ToInt32(date.ToString("HHmmss"));

            foreach (var item in g.관심삭제)
            {
                if (HHmmss - item.Value > 300) // 3분 이상 지난 종목은 제외
                {
                    g.관심삭제.Remove(item.Key);
                }
            }

            return g.관심삭제.ContainsKey(stock);
        }

        public static void read_stocks_in_directory(List<string> gl)
        {
            g.sl.Clear();
            string path = @"C:\Work\분\" + g.date.ToString();
            if (!Directory.Exists(path))
            {
                return;
            }
            var sl = Directory.GetFiles(path, "*.txt")
                     .Select(Path.GetFileName)
                     .ToList();

            List<string> 제외종목 = new List<string>();
            제외종목 = read_제외();  // read_stocks_in_directory


            g.관심종목.Clear();
            foreach (var stock in sl)
            {
                if (제외종목.Contains(stock))
                {
                    continue;
                }
                if (stock.Contains("processed"))
                {
                    continue;
                }
                string stock_without_txt = stock.Replace(".txt", "");

                gl.Add(stock_without_txt);
            }
        }

        public static void read_시간별거래비율(List<List<string>> 누적)
        {
            string[] grlines = File.ReadAllLines(@"C:\WORK\누적.txt", Encoding.Default);
            foreach (string line in grlines)
            {
                List<string> alist = new List<string>();

                string[] words = line.Split(' ');
                foreach (string item in words)
                {
                    if (item == "") continue; // WN code check needed for misspelling

                    alist.Add(item);
                }
                누적.Add(alist);
            }
        }

        public static void read_누적(double[] 누적)
        {
            string[] grlines = File.ReadAllLines(@"C:\WORK\누적.txt", Encoding.Default);
            int count = 0;
            foreach (string line in grlines)
            {
                string[] words = line.Split(' ');
                누적[count++] = Convert.ToDouble(words[1]);
            }
        }

        public static void read_or_set_stocks()
        {
            string path = @"C:\WORK\분" + "\\" + g.date.ToString();
            Directory.CreateDirectory(path); // read_or_set_stocks

            if (wk.return_index_of_ogldata("코스피혼합") < 0)
            {
                g.stock a = new g.stock();
                a.종목 = "코스피혼합";

                g.ogl_data.Insert(0, a);
            }
            if (wk.return_index_of_ogldata("코스닥혼합") < 0)
            {
                g.stock b = new g.stock();
                b.종목 = "코스닥혼합";
                g.ogl_data.Insert(0, b);
            }

            for (int i = 0; i < g.ogl_data.Count; i++) // read_or_set_stocks
            {
                g.stock o = g.ogl_data[i];

                // read in file into o.x[382, 12]
                // if file not exist, generate a new file and read it
                #region
                string file = path + "\\" + o.종목 + ".txt";
                if (!(File.Exists(file))) // if file not exist, create new
                {
                    File.Create(file).Close();
                    string minutestr = "85959\t0\t100\t10000\t0\t0\t0\t0\t0\t0\t0\t0"; // 12 items
                    using (StreamWriter w = File.AppendText(file))
                    {
                        w.WriteLine("{0}", minutestr);
                        w.Close(); // modified
                    }
                }

                var lines = File.ReadAllLines(file); // Nothing in file
                if (lines.Length == 0)
                {
                    File.Delete(file);
                    File.Create(file).Close();
                    string minutestr = "85959\t0\t100\t10000\t0\t0\t0\t0\t0\t0\t0\t0"; // 12 items
                    using (StreamWriter w = File.AppendText(file))
                    {
                        w.WriteLine("{0}", minutestr);
                        w.Close(); // modified
                    }
                    lines = File.ReadAllLines(file);
                }

                o.nrow = 0;

                int current_time = 0;

                foreach (var line in lines)
                {
                    string[] words = line.Split(' ');
                    if (words.Length == 1)
                    {
                        words = line.Split('\t');
                    }
                    if (words[0].Contains(":"))
                    {
                        current_time = ms.time_to_int(words[0]);
                    }
                    else
                    {
                        current_time = Convert.ToInt32(words[0]);
                    }

                    if (current_time >= 85959 && current_time < 152100)
                    {
                        o.x[o.nrow, 0] = current_time;
                        for (int j = 1; j < words.Length; j++)
                        {
                            o.x[o.nrow, j] = Convert.ToInt32(words[j]);
                        }
                        o.nrow++;
                    }
                    else
                    {
                        break;
                    }
                }

                // make the other rows not in the file all zeros for marketeye_received
                for (int j = o.nrow; j < g.MAX_ROW; j++) // read_or_set_stocks again ... not zero and trouble in draw_stock
                {
                    for (int m = 0; m < 12; m++)
                    {
                        o.x[j, m] = 0;
                    }
                }

                if (o.nrow == 0)
                    continue;

                o.당일프로그램순매수량 = o.x[o.nrow - 1, 4];
                o.당일외인순매수량 = o.x[o.nrow - 1, 5];
                o.당일기관순매수량 = o.x[o.nrow - 1, 6];
                int 거래량 = o.x[o.nrow - 1, 7];

                o.틱의시간[0] = o.x[o.nrow - 1, 0];
                o.틱의가격[0] = o.x[o.nrow - 1, 1];
                o.틱의수급[0] = o.x[o.nrow - 1, 2];
                o.틱의체강[0] = o.x[o.nrow - 1, 3];
                double intensity_double = o.틱의체강[0] / g.HUNDRED;
                o.틱매수량[0] = (int)(거래량 * intensity_double / (100.0 + intensity_double));
                o.틱매도량[0] = (int)(거래량 * 100.0 / (100.0 + intensity_double));
                o.틱매수배[0] = o.x[o.nrow - 1, 8];
                o.틱매도배[0] = o.x[o.nrow - 1, 9];

                o.종거천 = (int)(o.전일종가 * (거래량 / g.천만원));

                #endregion

                // continuity setting as default
                #region
                if ((!(g.KODEX4.Contains(o.종목) || o.종목.Contains("혼합"))) && o.nrow >= 2)
                {
                    for (int j = 1; j < o.nrow; j++)
                    {
                        // Continuity of amount
                        if (o.x[j, 7] == o.x[j - 1, 7])
                            o.x[j, 10] = o.x[j - 1, 10];
                        else if (o.x[j, 2] > o.x[j - 1, 2]) // including VI
                            o.x[j, 10] = o.x[j - 1, 10] + 1;
                        else
                            o.x[j, 10] = 0;

                        // Continuity of intensity : the intensity was multiplied by g.HUNDRED -> too many cyan  -> divided by g.HUNDRED again, let's see
                        if (o.x[j, 7] == o.x[j - 1, 7])
                            o.x[j, 11] = o.x[j - 1, 11];
                        else if (((int)(o.x[j, 3] / g.HUNDRED)) > ((int)(o.x[j - 1, 3] / g.HUNDRED))) // including VI
                            o.x[j, 11] = o.x[j - 1, 11] + 1;
                        else
                            o.x[j, 11] = 0;
                    }
                }
                #endregion


            }
            //pa.pass_reset_all_stocks();
        }


        public static void read_업종_상관()
        {
            List<string> total_stock_list = new List<string>();
            List<string> tgl_title = new List<string>();
            List<List<string>> tgl = new List<List<string>>();
            List<string> 상관_group_total_stock_list = new List<string>();
            List<List<string>> Gl = new List<List<string>>();
            List<List<string>> GL = new List<List<string>>();

            if (g.shortform)
            {
            }
            else
                total_stock_list = rd.read_그룹_네이버_업종(Gl, GL); // if file not exist, return nothing, 제외종목 제거

            // total_stock_list : 10일간 평균거래액 10억 이상만 선택
            wk.일평균거래액10억이상종목선택(total_stock_list, 10);

            rd.read_상관(상관_group_total_stock_list, tgl_title, tgl); // if file not exist, just stock added

            foreach (var item in 상관_group_total_stock_list)
            {
                if (total_stock_list.Contains(item))
                    continue;
                else
                    total_stock_list.Add(item);
            }

            // 지수종목 
            rd.read_KODEX(); // stored in g.지수종목
            foreach (string t in g.지수종목)
            {
                if (!total_stock_list.Contains(t))
                    total_stock_list.Add(t);
            }

            foreach (var stock in total_stock_list)
                wk.gen_ogl_data(stock);

            g.ogl_data = g.ogl_data.OrderByDescending(x => x.전일거래액_천만원).ToList();

            foreach (var item in g.ogl_data)
                g.sl.Add(item.종목);

            wk.gen_oGL_data(tgl_title, tgl); // generate oGL_data
        }

        public static void read_관심종목()
        {
            string working_directory = System.IO.Directory.GetCurrentDirectory();
            // replaces "Programming" with "C#" 
            string filename = working_directory.Replace("bin\\Debug", "");
            filename += "관심.txt";


            if (File.Exists(filename))
            {
                string[] grlines = File.ReadAllLines(filename);

                List<string> temp_list = new List<string>();
                foreach (string line in grlines)
                {
                    string[] words = line.Split(' '); // empty spaces also recognized as words, word.lenght can be larger than 4

                    for (int i = 0; i < words.Length; i++)
                    {
                        if (words[i] == "//")
                            break;
                        if (words[i] == "")
                            continue;
                        words[i].Replace('_', ' ');
                        g.관심종목.Add(words[i]);
                    }
                }
            }
        }


        public static void read_상승()
        {
            string file = @"C:\WORK\상승.txt";

            if (!File.Exists(file))
            {
                g.eval_score[0, 0] = -2; // if file not exist, set the first data -2 (no more data)
                return;
            }

            string[] grlines = File.ReadAllLines(file);
            List<string> list = new List<string>();

            int row_count = 0;
            foreach (string line in grlines)
            {
                string[] words = line.Split('\t');
                if (line == "") // first empty line met
                    break;

                int lastRows = g.eval_score.GetUpperBound(0); // lastRows is 9, this is the dimension
                int lastColumns = g.eval_score.GetUpperBound(1); // lastColu,mns is 11, this is the dimension
                if (words.Length > lastColumns + 1) // number of columns should be less than 10
                {
                    MessageBox.Show("First Data Error in 상승.txt");
                    break;
                }

                for (int i = 0; i <= lastColumns; i++)
                {
                    if (i >= words.Length)
                    {
                        g.eval_score[row_count, i] = -1;
                        continue;
                    }

                    bool success = true;
                    success = int.TryParse(words[i], out g.eval_score[row_count, i]);
                    if (!success)
                    {
                        g.eval_score[row_count, i] = -1;
                    }
                }
                row_count++;

                if (row_count >= lastRows) // if row of data is more than 10
                {
                    MessageBox.Show("Second Data Error in 상승.txt");
                    break;
                }
            }
            g.eval_score[row_count, 0] = -2; // no more date sign in the first column of (last + 1) line
        }


        public static bool read_단기과열(string stock)
        {
            _cpcodemgr = new CPUTILLib.CpCodeMgr();
            _cpstockcode = new CPUTILLib.CpStockCode();
            int t = (int)_cpcodemgr.GetOverHeating(_cpstockcode.NameToCode(stock));

            if (t == 2 || t == 3)
                return true;
            else
                return false;
        }

        public static char read_코스피코스닥시장구분(string stock)
        {
            _cpcodemgr = new CPUTILLib.CpCodeMgr();
            _cpstockcode = new CPUTILLib.CpStockCode();
            int marketKind = (int)_cpcodemgr.GetStockMarketKind(_cpstockcode.NameToCode(stock));
            if (marketKind == 1)

            {
                return 'S';

            }
            else if (marketKind == 2)
            {
                return 'D';
            }
            else
                return 'N';
        }

        public static int read_데이터컬럼들(string filename, int[] c_id, string[,] x)
        {
            /* 파일이름, 구하고자하는 컬럼 번호를 주면 x[,] 저장 nrow 반환
	   * public static int read_데이터컬럼들
		 (string filename, int[] c_id, string[,] x)
	   * */

            if (!File.Exists(filename)) return -1;
            string[] grlines = System.IO.File.ReadAllLines(filename, Encoding.Default);

            int nrow = 0;
            foreach (string line in grlines)
            {
                List<string> alist = new List<string>();

                string[] words = line.Split(' ');
                for (int k = 0; k < c_id.Length; k++)
                {
                    x[nrow, k] = words[c_id[k]];
                }
                nrow++;


            }
            return nrow;
        }


        public static void read_제어()
        {

            string filename = @"C:\WORK\제어.txt";


            if (!File.Exists(filename)) return;
            string[] grlines = System.IO.File.ReadAllLines(filename, Encoding.Default);


            if (grlines[0] == "r")
                g.testing = false;
            else
                g.testing = true;

            if (grlines[1] == "s")
                g.shortform = true;
            else
                g.shortform = false;

            string[] strs = grlines[2].Split(' ');
            g.Account = strs[0];

            g.date = Convert.ToInt32(grlines[3]);
        }



        public static void read_변수()
        {
            string working_directory = System.IO.Directory.GetCurrentDirectory();
            // replaces "Programming" with "C#" 
            string filename = working_directory.Replace("bin\\Debug", "");
            filename += "변수.txt";

            if (!File.Exists(filename)) return;
            string[] grlines = System.IO.File.ReadAllLines(filename, Encoding.Default);


            foreach (string line in grlines)
            {
                var words = line.Split('\t');

                switch (words[0])
                {
                    //case "textbox_date_char_to_string":
                    //    라인분리(line, ref g.v.textbox_date_char_to_string);
                    //    break;
                    case "files_to_open_by_clicking_edge":
                        라인분리(line, ref g.v.files_to_open_by_clicking_edge);
                        break;
                    case "q_advance_lines":
                        라인분리(line, ref g.v.q_advance_lines);
                        break;
                    case "Q_advance_lines":
                        라인분리(line, ref g.v.Q_advance_lines);
                        break;
                    case "r3_display_lines":
                        라인분리(line, ref g.v.r3_display_lines);
                        break;
                    case "index_difference_sound":
                        라인분리(line, ref g.v.index_difference_sound);
                        break;



                    case "dev":
                        라인분리(line, ref g.s.dev);
                        break;
                    case "mkc":
                        라인분리(line, ref g.s.mkc);
                        break;

                    case "돌파":
                        라인분리(line, ref g.s.돌파);
                        break;
                    case "눌림":
                        라인분리(line, ref g.s.눌림);
                        break;

                    case "가연":
                        라인분리(line, ref g.s.가연);
                        break;
                    case "가분":
                        라인분리(line, ref g.s.가분);
                        break;
                    case "가틱":
                        라인분리(line, ref g.s.가틱);
                        break;
                    case "가반":
                        라인분리(line, ref g.s.가반);
                        break;
                    case "가지":
                        라인분리(line, ref g.s.가지);
                        break;

                    case "수연":
                        라인분리(line, ref g.s.수연);
                        break;
                    case "수지":
                        라인분리(line, ref g.s.수지);
                        break;
                    case "수위":
                        라인분리(line, ref g.s.수위);
                        break;

                    case "강연":
                        라인분리(line, ref g.s.강연);
                        break;
                    case "강지":
                        라인분리(line, ref g.s.강지);
                        break;
                    case "강위":
                        라인분리(line, ref g.s.강위);
                        break;


                    case "프분":
                        라인분리(line, ref g.s.프분);
                        break;
                    case "프틱":
                        라인분리(line, ref g.s.프틱);
                        break;
                    case "프지":
                        라인분리(line, ref g.s.프지);
                        break;
                    case "프퍼":
                        라인분리(line, ref g.s.프퍼);
                        break;
                    case "프일":
                        라인분리(line, ref g.s.프일);
                        break;


                    case "거분":
                        라인분리(line, ref g.s.거분);
                        break;
                    case "거틱":
                        라인분리(line, ref g.s.거틱);
                        break;
                    case "거일":
                        라인분리(line, ref g.s.거일);
                        break;

                    case "배차":
                        라인분리(line, ref g.s.배차);
                        break;
                    case "배반":
                        라인분리(line, ref g.s.배반);
                        break;

                    case "표편":
                        라인분리(line, ref g.s.표편);
                        break;

                    case "급락":
                        라인분리(line, ref g.s.급락);
                        break;
                    case "잔잔":
                        라인분리(line, ref g.s.잔잔);
                        break;

                    case "그룹":
                        라인분리(line, ref g.s.그룹);
                        break;

                    default:
                        break;
                }
            }
        }


        public static void 라인분리(string line, ref int data) // scalar -> no 비중, dev, mkc
        {
            string[] words = line.Split('\t');
            Int32.TryParse(words[1], out data);
        }

        public static void 라인분리(string line, ref string[] data) // single vector -> no 비중, dev, mkc
        {
            string[] words = line.Split('\t');

            for (int i = 1; i < words.Length; i++)
            {
                data[i - 1] = words[i];
            }
        }

        public static void 라인분리(string line, ref int[] data) // single vector -> no 비중, dev, mkc
        {
            string[] words = line.Split('\t');

            for (int i = 1; i < words.Length; i++)
            {
                bool success = false;
                success = Int32.TryParse(words[i], out data[i - 1]);
                if (!success)
                    return;
            }
        }

        public static void 라인분리(string line, ref List<List<double>> data) // double list, with 비중, dev. mkc
        {
            data.Clear();

            string[] words = line.Split('\t');

            List<double> t = new List<double>();
            for (int i = 1; i < words.Length; i++)
            {
                if (words[i].Contains("//"))
                    break;

                string[] items = words[i].Split('/');

                if (items.Length == 1)
                {
                    double a;
                    bool success = false;
                    success = double.TryParse(items[0], out a);

                    if (success)
                        t = new List<double> { a, 0.0 };
                    else
                        continue;
                }

                if (items.Length == 2)
                {
                    double a, b;

                    bool success_1 = false;
                    success_1 = double.TryParse(items[0], out a);

                    bool success_2 = false;
                    success_2 = double.TryParse(items[1], out b);
                    if (!success_2)
                        break;

                    if (success_1 && success_2)
                        t = new List<double> { a, b };
                    else
                        break;
                }
                data.Add(t);
            }

            // Integrity Check
            if (line.Contains("dev") || line.Contains("mkc") || line.Contains("잔잔"))
            {
            }
            else
            {
                bool error_exist = false;
                for (int i = 2; i < data.Count - 1; i++)
                {
                    if (data[i + 1][0] < data[i][0])
                        error_exist = true;
                    if (data[i + 1][1] < data[i][1])
                        error_exist = true;
                }
                if (error_exist)
                    MessageBox.Show("Error in 변수 파일 : " + line);
            }

            // the following lines to check the integrity of data
            string path = @"C:\WORK\temp_변수.txt";
            StreamWriter sw = File.AppendText(path);

            string str = line;
            if (data.Count > 0)
                str += "\t" + data[data.Count - 1][0].ToString() + "/" + data[data.Count - 1][1].ToString() +
                         "\t" + data.Count.ToString();
            sw.WriteLine("{0}", str);
            sw.Close();
        }

        public static int read_stock_minute(int date, string stock, int[,] x)
        {
            if (date < 10)
            {
                DateTime now = DateTime.Now;
                date = Convert.ToInt32(now.ToString("yyyyMMdd"));
            }

            string file = @"C:\WORK\분\" + date.ToString() + "\\" + stock + ".txt";
            if (!File.Exists(file)) return -1;
            string[] grlines = System.IO.File.ReadAllLines(file, Encoding.Default);

            int nrow = 0;
            foreach (string line in grlines)
            {
                List<string> alist = new List<string>();

                string[] words = line.Split(' ');
                for (int k = 0; k < words.Length; k++)
                {
                    if (k == 4)
                    {
                        x[nrow, k] = Convert.ToInt32((int)Convert.ToDouble(words[k]));
                    }
                    else
                    {
                        x[nrow, k] = Convert.ToInt32(words[k]);
                    }
                }
                if (x[nrow, 0] < 10)
                    break;
                else
                    nrow++;
            }
            return nrow;
        }

        public static double read_시총(string stock)
        {
            string[] grlines = File.ReadAllLines(@"C:\WORK\시총.txt", Encoding.Default);
            foreach (string line in grlines)
            {
                string[] words = line.Split(' ');
                string newname = words[0].Replace("_", " ");
                if (string.Equals(newname, stock))
                {
                    return Convert.ToDouble(words[1]);
                }
            }
            return -1;
        }

        public static List<string> read_제외()
        {
            List<string> gl_list = new List<string>();

            string working_directory = System.IO.Directory.GetCurrentDirectory();
            // replaces "Programming" with "C#" 
            string filename = working_directory.Replace("bin\\Debug", "");
            filename += "제외.txt";

            string[] grlines = File.ReadAllLines(filename);

            foreach (string line in grlines)
            {
                string[] words = line.Split(' ');
                foreach (string stock in words)
                {
                    string newname = stock.Replace("_", " ");
                    if (!wk.is_stock(newname) || gl_list.Contains(newname))
                        continue;

                    gl_list.Add(newname); // for single
                }
            }
            var uniqueItemsList = gl_list.Distinct().ToList();
            return uniqueItemsList;
        }


        public static void read_KODEX()
        {
            // 종목별 배당된 주식숫자로부터 각 종목의 Weighting Factor 계산하여 저장

            string working_directory = System.IO.Directory.GetCurrentDirectory();
            // replaces "Programming" with "C#" 
            string filename = working_directory.Replace("bin\\Debug", "");
            filename += "지수.txt";

            if (!File.Exists(filename))
            {
                MessageBox.Show("지수.txt Not Exist");
                return;
            }

            string[] grlines = File.ReadAllLines(filename);
            List<string> list = new List<string>();

            bool empty_line_met = false;
            foreach (string line in grlines)
            {
                string[] words = line.Split('\t');
                if (words[0] == "")
                    empty_line_met = true;

                if (words[0].Length > 0)
                {
                    if (empty_line_met == false)
                        g.코스피합성.Add(words[0] + '\t' + words[1]);
                    else
                        g.코스닥합성.Add(words[0] + '\t' + words[1]);
                }
            }

            g.평불종목.Add("코스피혼합");
            g.평불종목.Add("코스닥혼합");

            g.지수종목.Clear();
            foreach (string t in g.코스피합성)
            {
                string[] words = t.Split('\t');
                g.지수종목.Add(words[0]);
            }
            foreach (string t in g.코스닥합성)
            {
                string[] words = t.Split('\t');
                g.지수종목.Add(words[0]);
            }

            g.지수종목.Add("KODEX 레버리지"); // 0504
            g.지수종목.Add("KODEX 200선물인버스2X");
            g.지수종목.Add("KODEX 코스닥150레버리지");
            g.지수종목.Add("KODEX 코스닥150선물인버스");
        }

        public static void read_그룹_네이버_테마(List<string> tsl, List<string> tsl_그룹_상관, List<string> GL_title, List<List<string>> GL)
        {
            string filepath = @"C:\WORK\그룹_네이버_테마.txt"; // QQQ
            if (!File.Exists(filepath))
                return;

            var lines = File.ReadAllLines(filepath, Encoding.Default);

            List<List<string>> temp_GL = new List<List<string>>(); // temporary working space for group list
            List<string> temp_Gl = new List<string>();

            foreach (var item in lines)
            {
                if (item == "" && temp_Gl.Count > 1)
                {
                    GL_title.Add(temp_Gl[0]);
                    temp_Gl.RemoveAt(0);
                    GL.Add(temp_Gl.ToList());

                    foreach (var stock in temp_Gl)
                    {
                        if (!tsl.Contains(stock))
                            tsl.Add(stock);
                    }
                    temp_Gl.Clear();
                }
                if (item != "" && !tsl_그룹_상관.Contains(item))
                    temp_Gl.Add(item);
            }
        }



        public static void read_상관(List<string> gl, List<string> GL_title, List<List<string>> GL)
        {
            string working_directory = System.IO.Directory.GetCurrentDirectory();
            // replaces "Programming" with "C#" 
            string filename = working_directory.Replace("bin\\Debug", "");
            filename += "상관.txt";

            if (!File.Exists(filename))
            {
                return;
            }
            string[] grlines = File.ReadAllLines(filename, Encoding.Default);

            List<string> temp_gl = new List<string>();
            List<string> temp_Gl = new List<string>();
            string temp_title = "";

            foreach (var line in grlines)
            {
                string[] words = line.Split(' ');

                if (words[0].Contains("//"))
                {
                    if (temp_Gl.Count >= 2)
                    {
                        if (!GL_title.Contains(temp_title))
                        {
                            if (g.shortform)
                            {
                                if (GL.Count == 4)
                                    break;
                            }
                            GL_title.Add(temp_title);
                            GL.Add(temp_Gl.ToList());
                        }
                    }
                    temp_Gl.Clear();
                    if (words.Length > 1)
                        temp_title = words[1];

                    continue;
                }
                else
                {
                    string new_name = "";
                    foreach (string stock in words)
                    {
                        if (stock == "//") // not a first word, then go to next line
                            break;

                        new_name = stock.Replace("_", " ");
                        if (!wk.is_stock(new_name))
                            continue;

                        if (!temp_Gl.Contains(new_name))
                            temp_Gl.Add(new_name); // large group GL 
                        if (!gl.Contains(new_name))
                            gl.Add(new_name);
                    }
                }
            }

            if (temp_Gl.Count >= 2) // 그룹 상관, 2개 이상의 종목으로 구성된 그룹
            {
                GL_title.Add(temp_title);
                GL.Add(temp_Gl.ToList()); // 임시저장으로
            }
        }


        public static int read_Stock_Seven_Lines_Reverse(int date, string stock, int nline, int[,] x)
        {
            if (date < 0)
            {
                DateTime now = DateTime.Now;
                date = Convert.ToInt32(now.ToString("yyyyMMdd"));
            }

            string file = @"C:\WORK\분\" + date.ToString() + "\\" + stock + ".txt";
            if (!File.Exists(file))
                return -1;

            int n = File.ReadAllLines(file).Length;
            if (n < nline)
            {
                nline = n;
            }
            List<string> lines = File.ReadLines(file).Reverse().Take(nline).ToList();

            // Error Prone : Below 8 lines not Tested Exactly
            if (g.testing)
            {
                if (n < g.time[1] + 1)
                {
                    g.time[1] = n;
                }
                lines = File.ReadLines(file).Skip(g.time[1] - nline).Take(nline).Reverse().ToList();
            }

            int nrow = 0;
            foreach (var line in lines)
            {
                string[] words = line.Split('\t');
                if (words.Length == 1)
                {
                    words = line.Split(' ');
                }

                // values are crossed, later rearrange ZZZ
                string[] time = words[0].Split(':');
                if (time.Length == 1)
                {
                    x[nrow, 0] = Convert.ToInt32(words[0]);
                }
                else
                {
                    x[nrow, 0] = Convert.ToInt32(time[0]) * 100 + Convert.ToInt32(time[1]);
                }

                x[nrow, 1] = Convert.ToInt32(words[1]);   // price
                x[nrow, 2] = Convert.ToInt32(words[2]);   // amount
                x[nrow, 3] = Convert.ToInt32(words[3]);   // intensity

                x[nrow, 4] = Convert.ToInt32(words[4]);   // institue from marketeye
                x[nrow, 5] = Convert.ToInt32(words[5]);   // foreign 
                x[nrow, 6] = Convert.ToInt32(words[6]);   // foreign from marketeye

                x[nrow, 7] = Convert.ToInt32(words[7]);   // total amount dealt
                x[nrow, 8] = Convert.ToInt32(words[8]);   // buy multiple 10 times
                x[nrow, 9] = Convert.ToInt32(words[9]);   // sell multiple 10 times

                nrow++;
                if (nrow == nline) // can not exceeed the maximum allocation of x[nline,]
                {
                    break;
                }
            }
            return nrow;
        }


        // This is for eval. of stock for testing purpose
        public static int read_Stock_Seven_Lines_Reverse_From_Endtime
            (int date, string stock, int end_time, int nline, int[,] x)
        {
            string file = @"C:\WORK\분\" + date.ToString() + "\\" + stock + ".txt";
            if (!File.Exists(file))
                return -1;

            int n = File.ReadAllLines(file).Length;

            if (end_time > n)
            {
                end_time = n;
            }
            else
            {
                if (end_time < nline)
                {
                    nline = end_time;
                }
            }


            //end_time - 1 : - 1 because start from end_time to reverse
            // Skip, Reverse, Take does not work, Reverse is done by using Reverse.array();
            var lines = File.ReadLines(file).Skip(end_time - nline).Take(nline);

            n = nline - 1; // to reverse

            int nrow = 0;
            foreach (var line in lines)
            {
                string[] words = line.Split('\t');
                if (words.Length == 1)
                {
                    words = line.Split(' ');
                }

                // values are crossed, later rearrange ZZZ
                string[] time = words[0].Split(':');
                if (time.Length == 1)
                {
                    x[n, 0] = Convert.ToInt32(words[0]);
                }
                else
                {
                    x[n, 0] = Convert.ToInt32(time[0]) * 100 + Convert.ToInt32(time[1]);
                }

                x[n, 1] = Convert.ToInt32(words[1]);   // price
                x[n, 2] = Convert.ToInt32(words[2]);   // amount
                x[n, 3] = Convert.ToInt32(words[3]);   // intensity

                x[n, 4] = Convert.ToInt32(words[4]);   // institue from marketeye
                x[n, 5] = Convert.ToInt32(words[5]);   // foreign 
                x[n, 6] = Convert.ToInt32(words[6]);   // foreign from marketeye

                x[n, 7] = Convert.ToInt32(words[7]);   // total amount dealt
                x[n, 8] = Convert.ToInt32(words[8]);   // buy multiple 10 times
                x[n, 9] = Convert.ToInt32(words[9]);   // sell multiple 10 times

                n--;

                nrow++;
            }

            //x[nrow, 0] = 0; // used to check the end of data, no need actually
            //if (x[nrow - 1, 1] == 0 && x[nrow - 1, 2] == 0 && x[nrow - 1, 3] == 0) // maybe trading suspended
            //{
            //    nrow = 0;
            //}

            return nrow; // nrow = nline as the result from above
        }

        public static int read_Stock_Minute(int date, string stock, int[,] x)
        {
            if (date < 0)
            {
                DateTime now = DateTime.Now;
                date = Convert.ToInt32(now.ToString("yyyyMMdd"));
            }

            string file = @"C:\WORK\분\" + date.ToString() + "\\" + stock + ".txt";
            if (!File.Exists(file))
            {
                return 0;
            }



            string[] lines = File.ReadAllLines(file, Encoding.Default);
            if (lines.Length == 0)
            {
                return 0;
            }

            int nrow = 0;
            foreach (string line in lines)
            {
                string[] words = line.Split('\t');
                if (words.Length == 1)
                {
                    words = line.Split(' ');
                }

                // values are crossed, later rearrange ZZZ
                string[] time = words[0].Split(':');
                if (time.Length == 1)
                {
                    x[nrow, 0] = Convert.ToInt32(words[0]); // words[0] = time[0], no difference
                }
                else
                {
                    x[nrow, 0] = Convert.ToInt32(time[0]) * 10000 + Convert.ToInt32(time[1]) * 100 + Convert.ToInt32(time[2]);
                }

                x[nrow, 1] = Convert.ToInt32(words[1]);   // price
                x[nrow, 2] = Convert.ToInt32(words[2]);   // amount
                x[nrow, 3] = Convert.ToInt32(words[3]);   // intensity

                x[nrow, 4] = Convert.ToInt32(words[4]);   // institue from marketeye
                x[nrow, 5] = Convert.ToInt32(words[5]);   // foreign 
                x[nrow, 6] = Convert.ToInt32(words[6]);   // foreign from marketeye

                x[nrow, 7] = Convert.ToInt32(words[7]);   // total amount dealt
                x[nrow, 8] = Convert.ToInt32(words[8]);   // buy multiple 10 times
                x[nrow, 9] = Convert.ToInt32(words[9]);   // sell multiple 10 times

                if (words.Length == 12)
                {
                    x[nrow, 10] = Convert.ToInt32(words[10]);   // buy multiple 10 times
                    x[nrow, 11] = Convert.ToInt32(words[11]);   // sell multiple 10 times
                }
                nrow++;
                if (nrow == g.MAX_ROW)
                    break;
                //if (x[nrow - 1 , 0] > 152100) //0505 from nrow > 390 to current if
                //{
                //    x[nrow - 1, 0] = 0;
                //    nrow--;
                //    break;
                //}
            }
            for (int i = nrow; i < g.MAX_ROW; i++)
            {
                for (int j = 0; j < 12; j++)
                {
                    x[i, j] = 0;
                }
            }

            //x[nrow, 0] = 0; // used to check the end of data, no need actually
            //if (x[nrow - 1, 1] == 0 && x[nrow - 1, 2] == 0 && x[nrow - 1, 3] == 0) // maybe trading suspended, 아래 3 라인이 없으면 축 개체 문제 발생
            //{
            //    nrow = 0;
            //}

            if (g.dl.Count == 4) // 코스피혼합 & 코스닥혼합 인버스로 만들기 위해 가격 X -1 & 매수배수 매도배수 Swap
            {
                if ((g.dl[0] == "KODEX 200선물인버스2X" && stock == "코스피혼합") ||
                   (g.dl[2] == "KODEX 코스닥150선물인버스" && stock == "코스닥혼합"))
                {
                    for (int i = 0; i < nrow; i++)
                    {
                        x[i, 1] *= -1;
                        wk.Swap(ref x[i, 8], ref x[i, 9]);
                    }
                }
            }

            //if(stock == "KODEX 레버리지" || 
            //  stock == "KODEX 200선물인버스2X" ||
            //  stock == "KODEX 코스닥150레버리지" ||
            //  stock == "KODEX 코스닥150선물인버스")
            //{
            //    for (int i = 0; i < g.money_shift; i++)
            //    {
            //        x[nrow + i, 4] = x[nrow - 1, 4];
            //        x[nrow + i, 5] = x[nrow - 1, 5];
            //        x[nrow + i, 6] = x[nrow - 1, 6];
            //        x[nrow + i, 10] = x[nrow - 1, 10];
            //        x[nrow + i, 11] = x[nrow - 1, 11];
            //    }
            //    for (int i = 0; i < nrow; i++)
            //    {
            //        x[i, 4] = x[i + g.money_shift, 4];
            //        x[i, 5] = x[i + g.money_shift, 5];
            //        x[i, 6] = x[i + g.money_shift, 6];
            //        x[i, 10] = x[i + g.money_shift, 10];
            //        x[i, 11] = x[i + g.money_shift, 11];
            //    }
            //}


            return nrow;
        }


        public static int read_Stock_Minute_no_multiply(int date, string stock, int[,] x)
        {
            if (date < 0)
            {
                DateTime now = DateTime.Now;
                date = Convert.ToInt32(now.ToString("yyyyMMdd"));
            }

            string file = @"C:\WORK\분\" + date.ToString() + "\\" + stock + ".txt";
            if (!File.Exists(file))
                return 0;

            //var lineCount = File.ReadLines(file).Count();
            string[] lines = File.ReadAllLines(file, Encoding.Default);

            int nrow = 0;
            foreach (string line in lines)
            {
                string[] words = line.Split('\t');
                if (words.Length == 1)
                {
                    words = line.Split(' ');
                }

                // values are crossed, later rearrange ZZZ
                string[] time = words[0].Split(':');
                if (time.Length == 1)
                {

                    x[nrow, 0] = Convert.ToInt32(words[0]);
                }
                else
                {
                    x[nrow, 0] = Convert.ToInt32(time[0]) * 100 + Convert.ToInt32(time[1]);
                }

                x[nrow, 1] = Convert.ToInt32(words[1]);   // price
                x[nrow, 2] = Convert.ToInt32(words[2]);   // amount
                x[nrow, 3] = Convert.ToInt32(words[3]);   // intensity

                x[nrow, 4] = Convert.ToInt32(words[4]);   // institue from marketeye
                x[nrow, 5] = Convert.ToInt32(words[5]);   // foreign 
                x[nrow, 6] = Convert.ToInt32(words[6]);   // foreign from marketeye

                x[nrow, 7] = Convert.ToInt32(words[7]);   // total amount dealt
                x[nrow, 8] = Convert.ToInt32(words[8]);   // buy multiple 10 times
                x[nrow, 9] = Convert.ToInt32(words[9]);   // sell multiple 10 times

                if (words.Length == 12)
                {
                    x[nrow, 10] = Convert.ToInt32(words[10]);   // buy multiple 10 times
                    x[nrow, 11] = Convert.ToInt32(words[11]);   // sell multiple 10 times
                }
                nrow++;
                if (x[nrow - 1, 0] > 1420) //0505 from nrow > 390 to current if
                {
                    x[nrow - 1, 0] = 0;
                    nrow--;
                    break;
                }
            }

            x[nrow, 0] = 0; // used to check the end of data, no need actually
            if (x[nrow - 1, 1] == 0 && x[nrow - 1, 2] == 0 && x[nrow - 1, 3] == 0) // maybe trading suspended
            {
                nrow = 0;
            }
            return nrow;
        }


        public static int read_일주월(string stock, string dwm, int nrow, int[] col, int[,] x)
        {
            string file = "";
            switch (dwm)
            {
                case "일":
                    file = @"C:\WORK\일\" + stock + ".txt"; ;   // 틱,분,일,주,월 단위 데이터
                    break;
                case "주":
                    file = @"C:\WORK\주\" + stock + ".txt"; ;   // 틱,분,일,주,월 단위 데이터
                    break;
                case "월":
                    file = @"C:\WORK\월\" + stock + ".txt"; ;   // 틱,분,일,주,월 단위 데이터
                    break;
                default:
                    break;
            }
            if (!File.Exists(file))
                return 0;

            List<string> lines = File.ReadLines(file).Reverse().Take(nrow).ToList();

            nrow = 0;
            foreach (string line in lines)
            {
                string[] words = line.Split(' ');

                for (int i = 0; i < col.Length; i++)
                {
                    x[nrow, i] = Convert.ToInt32(words[col[i]]);

                }
                nrow++;
            }
            return nrow;
        }


        public static int read_전일종가_전일거래액_천만원(string stock)
        {
            string path = @"C:\WORK\일\" + stock + ".txt";
            if (!File.Exists(path))
            {
                return -1;
            }

            string lastline = File.ReadLines(path).Last(); // last line read 

            string[] words = lastline.Split(' ');
            int 전일종가 = Convert.ToInt32(words[4]);
            ulong 전일거래량 = Convert.ToUInt32(words[5]);
            return (int)(전일종가 * (전일거래량 / g.천만원));
        }


        public static int read_전일종가(string stock)
        {

            string path = @"C:\WORK\일\" + stock + ".txt";
            if (!File.Exists(path))
            {
                return -1;
            }

            string lastline = File.ReadLines(path).Last(); // last line read 

            string[] words = lastline.Split(' ');
            return Convert.ToInt32(words[4]);
        }


        public static List<string> read_그룹_네이버_업종(List<List<string>> Gl, List<List<string>> GL)
        {
            _cpstockcode = new CPUTILLib.CpStockCode();

            List<string> gl_list = new List<string>();

            string filepath = @"C:\WORK\그룹_네이버_업종.txt";
            if (!File.Exists(filepath))
            {
                return gl_list;
            }

            List<string> 제외 = new List<string>();
            제외 = read_제외(); // read_그룹_네이버_업종

            string[] grlines = File.ReadAllLines(filepath, Encoding.Default);

            List<string> GL_list = new List<string>();

            int count = 0;
            foreach (string line in grlines)
            {
                List<string> Gl_list = new List<string>();

                string[] words = line.Split('\t');

                if (words[0] != "")
                {
                    string stock = words[0].Replace(" *", "");
                    string code = _cpstockcode.NameToCode(stock);
                    if (code.Length != 7)
                    {
                        continue;
                    }
                    if (code[0] != 'A')
                        continue;

                    char marketKind = read_코스피코스닥시장구분(stock);
                    if (marketKind == 'S' || marketKind == 'D')
                    { }
                    else
                        continue;

                    if (제외.Contains(stock))
                        continue;

                    gl_list.Add(stock); // for single
                    GL_list.Add(stock); // for small group
                    if (count == grlines.Length - 1) // the last stock added as group
                    {
                        GL.Add(GL_list.ToList()); //modified to create a new List when adding
                        GL_list.Clear();
                    }
                }
                else
                {
                    GL.Add(GL_list.ToList()); // to create a new List when adding
                    GL_list.Clear();
                }
                count++;


            }

            var uniqueItemsList = gl_list.Distinct().ToList();
            return uniqueItemsList;
        }

        public static List<string> read_그룹_네이버_업종() // this is for single list of stocks in 그룹_네이버_업종
        {
            _cpstockcode = new CPUTILLib.CpStockCode();

            List<string> gl_list = new List<string>();

            string filepath = @"C:\WORK\그룹_네이버_업종.txt";
            if (!File.Exists(filepath))
            {
                return gl_list;
            }

            string[] grlines = File.ReadAllLines(filepath, Encoding.Default);

            List<string> GL_list = new List<string>();


            foreach (string line in grlines)
            {
                string[] words = line.Split('\t');

                if (words[0] != "")
                {
                    string stock = words[0].Replace(" *", "");
                    string code = _cpstockcode.NameToCode(stock);
                    if (code.Length != 7)
                    {
                        continue;
                    }
                    if (code[0] != 'A')
                        continue;

                    char marketKind = read_코스피코스닥시장구분(stock);
                    if (marketKind == 'S' || marketKind == 'D')
                    { }
                    else
                        continue;

                    gl_list.Add(stock); // for single
                }
            }

            var uniqueItemsList = gl_list.Distinct().ToList();
            return uniqueItemsList;
        }
    }
}
