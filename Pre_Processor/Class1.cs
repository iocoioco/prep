
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Pre_Processor
{


    class Pre_Processor_Class1
    {
        static CPUTILLib.CpCodeMgr _cm = new CPUTILLib.CpCodeMgr();

        static CPUTILLib.CpStockCode _cpstockcode = new CPUTILLib.CpStockCode();
        static DSCBO1Lib.StockMst2 _stockmst2 = new DSCBO1Lib.StockMst2();
        static DSCBO1Lib.StockMst _stockmst = new DSCBO1Lib.StockMst();

        CPSYSDIBLib.StockChart _Stock_Chart_일주월 = new CPSYSDIBLib.StockChart();
        CPSYSDIBLib.StockChart _Stock_Chart_분 = new CPSYSDIBLib.StockChart();
        CPSYSDIBLib.StockChart _Stock_Chart_시총 = new CPSYSDIBLib.StockChart();
        CPSYSDIBLib.StockChart _Stock_Chart_틱 = new CPSYSDIBLib.StockChart();
        //CPSYSDIBLib.StockChart _Stock_Chart6 = new CPSYSDIBLib.StockChart();
        CPSYSDIBLib.CpSvr7254 _cpsvr7254 = new CPSYSDIBLib.CpSvr7254();
        CPSYSDIBLib.CpSvrNew7216 _cpsvrNew7216 = new CPSYSDIBLib.CpSvrNew7216();

        public static string calcurate_종목일중변동평균편차(string stock, int days, ref double avr, ref double dev, ref int 일평균거래액,
                         ref int 일최소거래액, ref int 일최대거래액, ref int MaxmumDate, ref double MaximumPriceRiseRate)
        {
            string path = @"C:\병신\data\일\\" + stock + ".txt";
            if (!File.Exists(path))
                return " ";

            List<string> lines = File.ReadLines(path).Reverse().Take(days + 1).ToList();

            List<Double> list = new List<Double>();

            int 일거래액;
            int 일거래량;
            일평균거래액 = 0;
            일최대거래액 = 0;           // 단위 억원
            일최소거래액 = 1000000; // 단위 억원
            MaximumPriceRiseRate = -30;
            double 전일종가 = 0;
            for (int i = lines.Count - 1; i >= 0; i--)
            {
                string[] words = lines[i].Split(' ');
                if (전일종가 == 0)
                {
                    전일종가 = Convert.ToDouble(words[4]); // 전일종가
                    continue;
                }

                int 시가 = Convert.ToInt32(words[1]); // 시가
                int 고가 = Convert.ToInt32(words[2]); // 고가
                int 저가 = Convert.ToInt32(words[3]); // 저가
                int 종가 = Convert.ToInt32(words[4]); // 종가

                double riseofrate = (종가 - 전일종가) / 전일종가 * 100;

                if (riseofrate > MaximumPriceRiseRate)
                    MaximumPriceRiseRate = riseofrate;

                일거래량 = Convert.ToInt32(words[5]); // 누적거래량, the last day -> the first
                if (stock == "삼성전자" && 일거래량 == 0)
                    MessageBox.Show("삼성전자 일거래량 = 0");

                일거래액 = (int)(일거래량 * (종가 / 100000000.0)); // 억원
                일평균거래액 += 일거래액;
                if (일거래액 > 일최대거래액)
                {
                    일최대거래액 = 일거래액;
                    MaxmumDate = Convert.ToInt32(words[0]) % 1000;
                }

                if (일거래액 < 일최소거래액)
                    일최소거래액 = 일거래액;

                list.Add(riseofrate);

                전일종가 = Convert.ToDouble(words[4]); // 전일종가 재계산 다음 날 사용준비
            }
            double countminusone = list.Count - 1;

            일평균거래액 = (int)(일평균거래액 / countminusone);
            double temp_avr = 0.0;
            dev = 0.0;
            if (countminusone > 0)
            {
                temp_avr = list.Sum() / countminusone;
                if (countminusone <= 1)
                    dev = 0;
                else
                    dev = Math.Sqrt(list.Sum(x => Math.Pow(x - temp_avr, 2)) / (countminusone - 1));
            }

            string str = temp_avr.ToString("0.#") + "/" + dev.ToString("0.#");
            avr = temp_avr;
            return str;
        }
        public static double read_시총(string stock)
        {
            string[] grlines = File.ReadAllLines(@"C:\병신\data\시총.txt", Encoding.Default);
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

        public static int Read_Stock_Minute(int date, string stock, int[,] x)
        {
            int MAX_ROW = 382;


            string file = @"C:\병신\분\" + date.ToString() + "\\" + stock + ".txt";
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
                if (nrow == MAX_ROW)
                    break;
                //if (x[nrow - 1 , 0] > 152100) //0505 from nrow > 390 to current if
                //{
                //    x[nrow - 1, 0] = 0;
                //    nrow--;
                //    break;
                //}
            }


            return nrow;
        }


        public static int Read_Stock_Minute_LasLine(int date, string stock, int[] x)
        {

            string file = @"C:\병신\분\" + date.ToString() + "\\" + stock + ".txt";
            if (!File.Exists(file))
            {
                return 0;
            }

            string last = File.ReadLines(file).Last();
            string[] words = last.Split('\t');
            for (int i = 0; i < words.Length; i++)
            {
                x[i] = Convert.ToInt32(words[i]);
            }
            return words.Length;
        }



        public static int directory_분전후(int date_int, int updn)
        {
            var subdirs = Directory.GetDirectories(@"C:\병신\분")
                   .Select(Path.GetFileName).ToList();

            List<string> selected_subdirs = new List<string>();   // changing single list

            foreach (var item in subdirs)
            {
                if (item.Length == 8)
                    selected_subdirs.Add(item);
            }

            string date_string = date_int.ToString();
            int index = selected_subdirs.IndexOf(date_string);

            if (updn == 1)
            {
                if (index == selected_subdirs.Count - 1)
                    return -1;
                date_string = selected_subdirs[++index];
            }
            if (updn == -1)
            {
                if (index == 0)
                    return -1;
                date_string = selected_subdirs[--index];
            }

            return Convert.ToInt32(date_string);
        }


        public static void 시총순서(List<string> gl)
        {
            var 종목 = new List<Tuple<double, string>> { };

            foreach (var stock in gl)
            {
                if (stock.Contains("KODEX")) continue;

                double 시총 = read_시총(stock);
                if (시총 < 0) continue;

                종목.Add(Tuple.Create(시총, stock));
            }
            종목 = 종목.OrderByDescending(t => t.Item1).ToList();

            gl.Clear();

            foreach (var item in 종목)
                gl.Add(item.Item2);
        }


        public static void read_KODEX(List<string> _gl)
        {
            // 종목별 배당된 주식숫자로부터 각 종목의 Weighting Factor 계산하여 저장
            string file = @"C:\병신\data\지수.txt";
            if (!File.Exists(file))
            {
                MessageBox.Show("KODEX.txt Not Exist");
                return;
            }

            string[] grlines = File.ReadAllLines(file);
            List<string> list = new List<string>();

            foreach (string line in grlines)
            {
                string[] words = line.Split('\t');

                if (words[0].Length > 0)
                {
                    _gl.Add(words[0]);
                }
            }

            _gl.Add("KODEX 레버리지"); // 0504
            _gl.Add("KODEX 200선물인버스2X");
            _gl.Add("KODEX 코스닥150레버리지");
            _gl.Add("KODEX 코스닥150선물인버스");

        }

        public static bool isStock(string stock)
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

        public static void read_그룹_네이버_테마(List<string> gl, List<string> GL_title, List<List<string>> GL)
        {
            List<string> temp_Gl = new List<string>();

            string filepath = @"C:\병신\data\그룹_네이버_테마.txt";
            if (!File.Exists(filepath))
            {
                return;
            }
            string[] grlines = File.ReadAllLines(filepath, Encoding.Default);

            bool blank_read = true;
            foreach (string item in grlines)
            {
                if (blank_read == true)
                {
                    if (item == "")
                        continue;
                    else
                    {
                        GL_title.Add(item);
                        blank_read = false;
                        continue;
                    }
                }
                else
                {
                    if (item == "")
                    {
                        GL.Add(temp_Gl.ToList()); // independant copy
                        temp_Gl.Clear(); // clear temporary small group
                        blank_read = true;
                        continue;
                    }
                }
                if (!gl.Contains(item))
                    gl.Add(item); // for single
                if (!temp_Gl.Contains(item))
                    temp_Gl.Add(item); // for small group
            }
        }

        public static void read_그룹_상관(List<string> gl, List<string> GL_title, List<List<string>> GL) // 20220301 Modified
        {
            string file = @"C:\병신\data\상관.txt"; // if i == 0

            if (!File.Exists(file))
            {
                MessageBox.Show(file + " Not Exist");
                return;
            }
            string[] grlines = File.ReadAllLines(file, Encoding.Default);

            List<string> temp_gl = new List<string>();
            List<string> temp_Gl = new List<string>();
            string temp_title = "";

            foreach (var line in grlines)
            {
                string[] words = line.Split(' ');

                if (words[0].Contains("//"))
                {
                    temp_Gl.Clear();
                    if (words[1].Length > 1 && !words[1].Contains("/"))
                    {
                        temp_title = words[1];
                        continue;
                    }
                }
                else if (words[0] == "" || words[0].Substring(0, 1) == " ")
                {
                    if (temp_Gl.Count > 1 && temp_title.Length > 1)
                    {
                        GL_title.Add(temp_title);
                        GL.Add(temp_Gl.ToList());
                        foreach (var item in temp_Gl) // 네이버_그룹_테마에 종목이 없는 경우 있음
                        {
                            if (!gl.Contains(item))
                                gl.Add(item);
                        }
                        temp_Gl.Clear();
                        temp_title = "";
                    }
                }
                else
                {
                    string new_name = "";
                    foreach (string stock in words)
                    {
                        if (stock == "//") // not a first word, then go to next line
                            break;

                        new_name = stock.Replace("_", " ");
                        if (!isStock(new_name))
                            continue;

                        if (!temp_Gl.Contains(new_name))
                            temp_Gl.Add(new_name); // large group GL 
                    }
                }
            }

            if (temp_Gl.Count > 1) // 마지막 라인이 "" 또는 " "가 없이 끝나므로 
            {
                GL_title.Add(temp_title);
                GL.Add(temp_Gl.ToList());
                foreach (var item in temp_Gl)
                {
                    if (!gl.Contains(item))
                        gl.Add(item);
                }
            }

            //wk.write_on_temp(GL_title, GL);
        }

        public static string calcurate_종목일중변동평균편차(string stockname, ref double 양의변동, ref double 음의변동)
        {
            string path = @"C:\병신\data\일\\" + stockname + ".txt";
            if (!File.Exists(path))
                return " ";

            int npts_to_read = 20;
            List<string> lines = File.ReadLines(path).Reverse().Take(npts_to_read).ToList();

            // read data file

            List<Double> pos = new List<Double>();
            List<Double> neg = new List<Double>();
            foreach (var line in lines)
            {
                string[] words = line.Split(' ');
                double sta = Convert.ToDouble(words[1]); // 시가
                double end = Convert.ToDouble(words[4]); // 종가

                double diff = (end - sta) / sta * 100;

                if (diff > 0)
                {
                    pos.Add(diff);
                }
                else
                {
                    neg.Add(diff);
                }
            }

            double pos_avr = 0.0;
            if (pos.Count != 0)
                pos_avr = pos.Sum() / pos.Count;
            double pos_dev = 0.0;
            if (pos.Count - 1 != 0)
                pos_dev = Math.Sqrt(pos.Sum(x => Math.Pow(x - pos_avr, 2)) / (pos.Count - 1));
            double neg_avr = 0.0;
            if (neg.Count != 0)
                neg_avr = neg.Sum() / neg.Count;
            double neg_dev = 0.0;
            neg_dev = Math.Sqrt(neg.Sum(x => Math.Pow(x - neg_avr, 2)) / (neg.Count - 1));

            //string str = pos_avr.ToString("0.#") + "(" + pos_dev.ToString("0.#") + ")" + " " +
            //             neg_avr.ToString("0.#") + "(" + neg_dev.ToString("0.#") + ")";
            양의변동 = pos_avr;
            음의변동 = neg_avr;
            string str = pos_avr.ToString("0.#") + "/" + neg_avr.ToString("0.#");

            return str;
        }

        public static List<string> read_시총_일정액수이상(int lower_limit)
        {
            List<string> blist = new List<string>();
            string[] grlines = File.ReadAllLines(@"C:\병신\data\시총.txt", Encoding.Default);
            var items = new List<Tuple<int, string>> { };

            foreach (string line in grlines)
            {
                string[] words = line.Split(' ');

                if (Convert.ToInt32(words[1]) > lower_limit)
                {
                    if (words[0] == "") continue; // WN code check needed for misspelling
                    //string newname = words[0].Replace("_", " "); 사용하려면 아래의 words[0] 수정할 것

                    items.Add(Tuple.Create(Convert.ToInt32(words[1]), words[0]));
                }
            }
            items = items.OrderByDescending(t => t.Item1).ToList();

            foreach (var item in items)
                blist.Add(item.Item2); // stockname in descending order

            return blist;
        }


        public static List<string> read_group()
        {
            List<string> gl_list = new List<string>();

            for (int i = 0; i < 2; i++)
            {
                string file;
                if (i == 0)
                {
                    file = @"C:\병신\data\상관.txt"; // if i == 0
                }
                else
                {
                    file = @"C:\병신\data\상관.txt";
                }

                string[] grlines = File.ReadAllLines(file, Encoding.Default);

                List<string> temp_list = new List<string>();
                foreach (string line in grlines)
                {
                    string[] words = line.Split(' '); // empty spaces also recognized as words, word.lenght can be larger than 4


                    foreach (string stock in words)
                    {
                        if (stock == "") // \N code check needed for misspelling
                            continue;

                        if (stock.Contains("//")) // \N code check needed for misspelling
                            break;

                        string newname = stock.Replace("_", " ");

                        if (gl_list.Contains(newname)) // if stock is already added to above list, then skip 
                            continue;

                        temp_list.Add(newname); // large group GL 
                        gl_list.Add(newname); // for gl
                    }
                }

            }
            var uniqueItemsList = gl_list.Distinct().ToList();
            return uniqueItemsList;

        }


        public static List<string> read_그룹4(List<List<string>> cL)
        {
            List<string> blist = new List<string>();
            string[] grlines = File.ReadAllLines(@"C:\병신\실행.txt", Encoding.Default);
            foreach (string line in grlines)
            {
                List<string> alist = new List<string>();

                string[] words = line.Split(' ');

                foreach (string stockname in words)
                {
                    if (stockname == "") continue; // WN code check needed for misspelling
                    if (stockname.Contains("//")) // \N code check needed for misspelling
                    {
                        break;
                    }


                    string newname = stockname.Replace("_", " ");


                    alist.Add(newname);
                    blist.Add(newname);
                }
                if (alist.Count > 0)
                    cL.Add(alist);

            }
            var uniqueItemsList = blist.Distinct().ToList();
            return uniqueItemsList;
        }

        public static List<string> read_분별종목()
        {
            List<string> alist = new List<string>();
            string[] grlines = File.ReadAllLines(@"C:\병신\종목.txt", Encoding.Default);
            foreach (string line in grlines)
            {
                string[] words = line.Split(' ');

                if (words[0] == "") continue; // WN code check needed for misspelling
                words[0] = words[0].Replace("_", " ");

                alist.Add(words[0]);
            }
            return alist;
        }

        public static List<string> read_전종목코드()
        {
            List<string> alist = new List<string>();
            string[] grlines = File.ReadAllLines(@"C:\병신\종목전종목.txt", Encoding.Default);
            foreach (string line in grlines)
            {
                string[] words = line.Split(' ');

                string code = "";
                foreach (var str in words)
                {
                    if (str == "")
                        continue;
                    if (str[0] == 'A' && str.Length == 7)
                    {
                        code = str;
                        break;
                    }
                }
                alist.Add(code);
            }
            return alist;
        }
        public static int read_데이터컬럼들
          (string filename, int[] c_id, string[,] x)
        {

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
        public static double calculate_종목20일기준일평균거래량(string stock)
        {
            // Extract column 5 from stock filename
            string filename = @"C:\병신\data\일\" + stock + ".txt";
            int[] c_id = new int[1]; // number of columns needed
            string[,] x = new string[1000, 1]; // array declaration
            List<double> alist = new List<double>();
            int nrow = 0;
            double average;

            c_id[0] = 5; // everyday amount dealed 

            nrow = read_데이터컬럼들(filename, c_id, x);

            if (nrow < 0)
            {
                average = 0.0;
                return (ulong)average;
            }
            else if (nrow < 24)
            {
                double sum = 0.0;
                for (int k = 0; k < 24; k++)
                    sum += Convert.ToDouble(x[k, 0]);

                average = sum / nrow;
            }
            else
            {
                // The last 24 Rows Extraction


                for (int k = nrow - 1; k > nrow - 25; k--)
                    alist.Add(Convert.ToDouble(x[k, 0]));

                alist.Sort();

                // Use 20 data and Calcurate Average
                double sum = 0.0;
                for (int k = 2; k < alist.Count - 2; k++)
                    sum += alist[k];

                average = sum / (alist.Count - 4.0);
            }

            return average;
        }


        public static int data_컬럼(string path, int[] c_id, string[,] x)
        {
            if (!File.Exists(path))
                return -1;

            Stream sf = System.IO.File.Open(path,
                                     FileMode.Open,
                                     FileAccess.Read,
                                     FileShare.ReadWrite);
            StreamReader sr = new StreamReader(sf);

            string line;
            int nrow = 0;
            while ((line = sr.ReadLine()) != null)
            {
                //List<string> alist = new List<string>();

                string[] words = line.Split(' ');
                for (int k = 0; k < c_id.Length; k++)
                {
                    x[nrow, k] = words[c_id[k]];
                }
                nrow++;
            }
            return nrow;
        }
        public static void read_누적(List<List<string>> mF)
        {
            string[] grlines = File.ReadAllLines(@"C:\병신\data\누적.txt", Encoding.Default);
            foreach (string line in grlines)
            {
                List<string> alist = new List<string>();

                string[] words = line.Split(' ');
                foreach (string item in words)
                {
                    if (item == "") continue; // WN code check needed for misspelling

                    alist.Add(item);
                }
                mF.Add(alist);
            }
        }
        // 일 데이터가 업데이트되지 않으면 엉터리 ... 그러므로 사용 패소ㅔ
        /*
        public static int read_어제종가(int workingdate, string stockname)
        {
            string savedline = "";
            int found = 0;

            string[] grlines = File.ReadAllLines(@"C:\병신\일\" + stockname + ".txt", Encoding.Default);
            foreach (string line in grlines)
            {
                string[] words0 = line.Split(' ');
                foreach (string item in words0)
                {
                    int t = Convert.ToInt32(words0[0]);
                    if (workingdate == t)
                    {
                        found = 1;
                        break;
                    }
                }
                if (found == 1)
                    break;

                savedline = line;
            }

            string[] words1 = savedline.Split(' ');
            if (words1[0] == "")
                return -1;

            int closeprice = Convert.ToInt32(words1[4]);
            return closeprice;
        }
        */

        public static void read_날짜_삼성전자일자료로부터(int sartday, int finalday, string stockname, int[] workingdays)
        {
            int inc = 0;

            string[] grlines = File.ReadAllLines(@"C:\병신\data\일\" + stockname + ".txt", Encoding.Default);
            foreach (string line in grlines)
            {
                string[] words = line.Split(' ');

                int t = Convert.ToInt32(words[0]);
                if (t >= sartday && t <= finalday)
                {
                    workingdays[inc++] = t;
                }
            }
        }

        public static int read_일자제시_전일종가(int given_date, string stockname)
        {
            string path = @"C:\병신\data\일\" + stockname + ".txt";
            if (!File.Exists(path))
            {
                return -1;
            }
            string[] grlines = File.ReadAllLines(path, Encoding.Default);
            for (int i = 1; i < grlines.Length; i++)
            {
                string[] words = grlines[i].Split(' ');

                if (words[0] == given_date.ToString())
                {
                    words = grlines[i - 1].Split(' ');
                    return Convert.ToInt32(words[4]);
                }
            }
            return -1;
        }

        public static List<string> find_리스트(string name, List<List<string>> groupList)
        {
            foreach (var sublist in groupList)
            {
                if (sublist.Count < 1)
                    return null;
                if (sublist[0] == name)
                    return sublist;
            }
            return null;
        }

        public static int find_순서(string name, List<string> singleList)
        {
            int index = 0;
            foreach (string item in singleList)
            {
                if (item == name)
                    return index;
                index++;
            }
            return -1;
        }


        public static void PearsonRateDifferenceInDays(int ArrayLength, int PrintLength, List<string> sL) 
        {
            double[] values = new double[ArrayLength];

            double[] RateRiseFirst = new double[ArrayLength];
            double[] RateRiseSecond = new double[ArrayLength];

            string path = @"C:\병신\data\";
            path += ("Correlation" + ".txt");
            if (File.Exists(path))
                File.Delete(path);

            Stream FS = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
            StreamWriter sw = new System.IO.StreamWriter(FS, System.Text.Encoding.Default);

            foreach (string stockname1 in sL)
            {
                path = @"C:\병신\data\일\";
                path += (stockname1 + ".txt"); 
                if (!File.Exists(path))
                {
                    continue;
                }

                List<string> lines = File.ReadLines(path).Reverse().Take(ArrayLength).ToList();
                if (lines.Count != ArrayLength) // Array Length
                    continue;
                string[] words_check = lines[0].Split(' '); // 거래중지
                if (words_check[5] == "0")
                    continue;

                int inc = 0;
                foreach (string line in lines)
                {
                    string[] words = line.Split(' ');
                    var start = Convert.ToDouble(words[1]); // 시가
                    var high = Convert.ToDouble(words[2]); // 고가
                    var low  = Convert.ToDouble(words[3]); // 저가
                    RateRiseFirst[inc++] = (high - low) / (double)start;
                }

                var stocks = new List<Tuple<double, string>> { };

                foreach (string stockname2 in sL)
                {
                    path = @"C:\병신\data\일\";
                    path += (stockname2 + ".txt");
                    if (!File.Exists(path))
                    {
                        continue;
                    }

                    if (stockname1 == stockname2 || !File.Exists(path))
                    {
                        continue;
                    }

                    lines = File.ReadLines(path).Reverse().Take(ArrayLength).ToList();
                    if (lines.Count != ArrayLength) // Array Length
                        continue;
                    words_check = lines[0].Split(' '); // 거래중지
                    if (words_check[5] == "0")
                        continue;

                    inc = 0;
                    foreach (string line in lines)
                    {
                        string[] words = line.Split(' ');
                        var start = Convert.ToDouble(words[1]); // 시가
                        var high = Convert.ToDouble(words[2]); // 고가
                        var low = Convert.ToDouble(words[3]); // 저가
                        RateRiseSecond[inc++] = (high - low) / (double)start;
                    }

                    stocks.Add(Tuple.Create(wk.PearsonCorrelationCalculation(RateRiseFirst, RateRiseSecond), stockname2));
                }

                stocks = stocks.OrderByDescending(t => t.Item1).ToList();

                string s = stockname1;
                sw.WriteLine("{0}", s);

                inc = 0;
                foreach (var item in stocks)
                {
                    decimal d = Convert.ToDecimal(item.Item1);
                    string t = String.Format("{0:0.000}", d);
                    if (d < 0)
                        sw.WriteLine("{0}\t{1}", t, item.Item2);
                    else
                        sw.WriteLine("{0}\t{1}", t, item.Item2);

                    if (inc++ > PrintLength) { break; }
                }
                sw.WriteLine();
            }
            sw.Close();
        }


        public static void PearsonRateDifferenceBetweenDays(int ArrayLength, int PrintLength, List<string> sL)
        {
            double[] values = new double[ArrayLength];

            double[] RateRiseFirst = new double[ArrayLength];
            double[] RateRiseSecond = new double[ArrayLength];

            string path = @"C:\병신\data\";
            path += ("Correlation" + ".txt");
            if (File.Exists(path))
                File.Delete(path);

            
            Stream FS = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
            StreamWriter sw = new System.IO.StreamWriter(FS, System.Text.Encoding.Default);

            foreach (string stockname1 in sL)
            {
                path = @"C:\병신\data\일\";
                path += (stockname1 + ".txt");
                if (!File.Exists(path))
                {
                    continue;
                }

                List<string> lines = File.ReadLines(path).Reverse().Take(ArrayLength).ToList();
                if (lines.Count != ArrayLength) // Array Length
                {
                    wr.wt(lines.Count.ToString() + " days : " + stockname1);
                    continue;
                }
                    
                string[] words_check = lines[0].Split(' '); // 거래중지
                if (words_check[5] == "0")
                {
                    wr.wt("거래중지             : " + stockname1);
                    continue;
                }

                int inc = 0;
                foreach (string line in lines)
                {
                    string[] words = line.Split(' ');
                    values[inc++] = Convert.ToDouble(words[4]); // 종가
                }

                for (int i = 0; i < inc - 1; i++)
                {
                    RateRiseFirst[i] = (values[i + 1] - values[i]) / values[i];
                }

                var stocks = new List<Tuple<double, string>> { };

                foreach (string stockname2 in sL)
                {
                    path = @"C:\병신\data\일\";
                    path += (stockname2 + ".txt");
                    if (!File.Exists(path))
                    {
                        continue;
                    }

                    if (stockname1 == stockname2 || !File.Exists(path))
                    {
                        continue;
                    }

                    lines = File.ReadLines(path).Reverse().Take(ArrayLength).ToList();
                    if (lines.Count != ArrayLength) // Array Length
                    {
                        continue;
                    }
                        
                    words_check = lines[0].Split(' '); // 거래중지
                    if (words_check[5] == "0")
                    {
                        continue;
                    }

                    inc = 0;
                    foreach (string line in lines)
                    {
                        string[] words = line.Split(' ');
                        values[inc++] = Convert.ToDouble(words[4]); // 종가
                    }

                    for (int i = 0; i < inc - 1; i++)
                    {
                        RateRiseSecond[i] = (values[i + 1] - values[i]) / values[i];
                    }

                    stocks.Add(Tuple.Create(wk.PearsonCorrelationCalculation(RateRiseFirst, RateRiseSecond), stockname2));
                }

                stocks = stocks.OrderByDescending(t => t.Item1).ToList();

                var s = stockname1;
                sw.WriteLine("{0}", s);

                inc = 0;
                foreach (var item in stocks)
                {
                    decimal d = Convert.ToDecimal(item.Item1);
                    string t = String.Format("{0:0.000}", d);
                    if(d < 0)
                        sw.WriteLine("{0}\t{1}", t, item.Item2);
                    else
                        sw.WriteLine("{0}\t{1}", t, item.Item2);

                    if (inc++ > PrintLength) { break; }
                }
                sw.WriteLine();
            }
            sw.Close();
        }


        public static void cal_상관관계(List<string> sL)
        {
            int nspacefordays = 1000;
            double[] value1 = new double[nspacefordays];
            double[] percent1 = new double[nspacefordays];
            double[] value2 = new double[nspacefordays];
            double[] percent2 = new double[nspacefordays];


            List<string> alist = new List<string>();

            string path = @"C:\병신\data\";
            path += ("Correlation" + ".txt");
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            Stream FS = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
            StreamWriter sw = new System.IO.StreamWriter(FS, System.Text.Encoding.Default);

            foreach (string stockname1 in sL)
            {
                string path1 = @"C:\병신\data\일\";
                path1 += (stockname1 + ".txt");
                if (!File.Exists(path1))
                {
                    continue;
                }

                string[] lines1 = File.ReadAllLines(path1);

                int inc1 = 0;
                foreach (string line1 in lines1)
                {
                    string[] texts1 = line1.Split(' ');
                    value1[inc1++] = Convert.ToDouble(texts1[4]); // 종가
                }

                double sum_percentage1 = 0.0;
                for (int i = 0; i < inc1 - 1; i++)
                {
                    // 전일대비 상승 비율 0.15% -> 0.15/100.0 = 0.0015
                    percent1[i] = (value1[i + 1] - value1[i]) / value1[i];
                    sum_percentage1 += Math.Abs(percent1[i]);
                }
                for (int i = 0; i < inc1 - 1; i++)
                {
                    // 일간 상승율 divided by 상승율 절대값 합
                    // 표준화 ? 
                    percent1[i] /= sum_percentage1;
                }

                foreach (string stockname2 in sL)
                {
                    string path2 = @"C:\병신\data\일\";
                    path2 += (stockname2 + ".txt");

                    if (stockname1 == stockname2 || !File.Exists(path2))
                    {
                        continue;
                    }

                    string[] lines2 = File.ReadAllLines(path2);
                    if (lines2.Length < 20) // 신규상장 제외 (20일 이하는 상관에서 제외)
                        continue;

                    int inc2 = 0;
                    foreach (string line2 in lines2)
                    {
                        string[] texts2 = line2.Split(' ');
                        value2[inc2++] = Convert.ToDouble(texts2[4]);
                    }

                    double sum_percentage2 = 0.0;
                    for (int i = 0; i < inc2 - 1; i++)
                    {
                        percent2[i] = (value2[i + 1] - value2[i]) / value2[i];
                        sum_percentage2 += Math.Abs(percent2[i]);
                    }
                    for (int i = 0; i < inc2 - 1; i++)
                    {
                        // 일간 상승율 divided by 상승율 절대값 합
                        // 표준화 ? 
                        percent2[i] /= sum_percentage2;
                    }

                    int numberofuse = 0;
                    double difference = 0.0;
                    for (int iter = 0; iter < 2; iter++) // 200일, 300일, 400일, 500일 순차진행
                    {
                        if (iter == 0) numberofuse = 20;
                        // if (iter == 1) numberofuse = 60;
                        //if (iter == 2) numberofuse = 400;
                        //if (iter == 3) numberofuse = 500;
                        int min = Math.Min(inc1, inc2);
                        if (min < numberofuse)
                            numberofuse = min;

                        for (int i = 0; i <= numberofuse - 2; i++)
                        {
                            //double fac = (numberofuse - 2 - i) / (double)(numberofuse - 2);
                            //fac = 1.0;
                            // 두 종목 상승 차의 합이 가장 적은 종목이 1위
                            difference += Math.Abs((percent1[inc1 - 2 - i] - percent2[inc2 - 2 - i]));
                            //double value= ComputeCoeff(percent1, percent2);
                        }
                    }

                    string s = difference.ToString() + " " + stockname2;

                    alist.Add(s);
                }

                alist.Sort();

                sw.WriteLine("{0}", stockname1);

                int temp_inc = 0;
                foreach (string item in alist)
                {
                    sw.WriteLine("     {0}", item);
                    if (temp_inc++ > 30) { break; }
                }
                alist.Clear();
            }
            sw.Close();
        }

        public static List<string> read_상관관계(List<List<string>> cL)
        {
            bool first = true;
            List<string> alist = new List<string>();
            string[] grlines = System.IO.File.ReadAllLines(@"C:\병신\data\상관.txt", Encoding.Default);
            if (grlines == null)
                return null;

            foreach (string line in grlines)
            {
                string[] words = line.Split(' ');
                words = words.Where(x => !string.IsNullOrEmpty(x)).ToArray(); // remove "" strings

                if (words.Length == 0)
                    continue;

                bool numeric = words[0].All(c => c >= '0' && c <= '9' || c == '.');
                if (numeric)
                {
                    if (words.Length == 2)
                        words[0] = words[1];
                    if (words.Length == 3)
                        words[0] = words[1] + " " + words[2];
                }
                else
                {
                    if (words.Length == 2)
                        words[0] = words[0] + " " + words[1];
                }

                if (!numeric)
                {
                    if (first)
                    {
                        alist.Add(words[0]);
                        first = false;
                    }
                    else
                    {
                        List<string> blist = new List<string>();
                        foreach (var item in alist)
                            blist.Add(item);
                        cL.Add(blist);
                        alist.Clear();
                        alist.Add(words[0]);
                    }
                }
                else
                {
                    alist.Add(words[0]);
                }
            }
            cL.Add(alist);

            List<string> clist = new List<string>();
            foreach (var list in cL)
                clist.Add(list[0]);

            return clist;
        }

        public static List<string> read_그룹(List<List<string>> cL)
        {
            List<string> blist = new List<string>();
            string[] grlines = File.ReadAllLines(@"C:\병신\그룹.txt", Encoding.Default);
            foreach (string line in grlines)
            {
                List<string> alist = new List<string>();

                string[] words = line.Split(' ');
                foreach (string stockname in words)
                {
                    if (stockname == "") continue; // WN code check needed for misspelling
                    string newname = stockname.Replace("_", " ");


                    alist.Add(newname);
                    blist.Add(newname);
                }
                cL.Add(alist);
            }
            var uniqueItemsList = blist.Distinct().ToList();
            return uniqueItemsList;
        }




        public static List<string> read_그룹_네이버_업종(List<List<string>> GL)
        {
            List<string> gl_list = new List<string>();

            string filepath = @"C:\병신\data\그룹_네이버_업종.txt";
            if (!File.Exists(filepath))
            {
                return gl_list;
            }


            string[] grlines = File.ReadAllLines(filepath, Encoding.Default);

            List<string> GL_list = new List<string>();

            int count = 0;
            foreach (string line in grlines)
            {
                List<string> Gl_list = new List<string>();

                string[] words = line.Split('\t');



                if (words[0] != "")
                {
                    string stockname = words[0].Replace(" *", ""); // 코스닥 * 표시되어 있어 제거, 코스피는 없음
                    if (!isStock(stockname))
                        continue;

                    gl_list.Add(stockname); // for single
                    GL_list.Add(stockname); // for small group
                    if (count == grlines.Length - 1) // the last stock added as group
                    {
                        GL.Add(GL_list.ToList()); //modified to create a new List when adding
                        GL_list.Clear();
                    }
                }
                else
                {
                    GL.Add(GL_list.ToList()); //modified to create a new List when adding
                    GL_list.Clear();
                }
                count++;
            }

            var uniqueItemsList = gl_list.Distinct().ToList();
            return uniqueItemsList;
        }

        public static List<string> read_그룹_네이버_테마()
        {
            List<string> gl_list = new List<string>();

            string filepath = @"C:\병신\data\그룹_네이버_테마.txt";
            if (!File.Exists(filepath))
                return gl_list;

            string[] grlines = File.ReadAllLines(filepath, Encoding.Default);

            foreach (string stockname in grlines)
            {
                string code = _cpstockcode.NameToCode(stockname);
                if (code == "")
                {
                    continue;
                }
                gl_list.Add(stockname); // for single   
            }

            var uniqueItemsList = gl_list.Distinct().ToList();
            return uniqueItemsList;
        }

        public static void disp_sL_순서(int date, char displayOption, List<string> sL)
        {
            int col = 0;
            List<string> alist = new List<string>();

            switch (displayOption)
            {
                case 'q':
                    col = 1;
                    break;
                case 'w':
                    col = 2;
                    break;
                case 'e':
                    col = 3;
                    break;
                default:
                    break;
            }

            var items = new List<Tuple<int, string>> { };
            foreach (var name in sL)
            {
                string path = @"C:\병신\분\" + date.ToString() +
                    "\\" + name + ".txt";

                if (!File.Exists(path))
                {
                    continue;
                }


                Stream sf = System.IO.File.Open(path,
                                      FileMode.Open,
                                      FileAccess.Read,
                                      FileShare.ReadWrite);

                StreamReader sr = new StreamReader(sf);

                string line;
                int savedvalue = 0;

                while ((line = sr.ReadLine()) != null)
                {
                    string[] words = line.Split(' ');

                    if (Convert.ToInt32(words[0]) > 0)
                        savedvalue = Convert.ToInt32(words[col]);
                    else
                        break;
                }
                items.Add(Tuple.Create(savedvalue, name));
            }
            items = items.OrderByDescending(t => t.Item1).ToList();

            sL.Clear();
            foreach (var item in items)
                sL.Add(item.Item2); // stockname in descending order
        }

        public static void disp_agL_순서(int date, char displayOption, List<List<string>> gL)
        {
            int col = 0;
            List<string> alist = new List<string>();

            switch (displayOption)
            {
                case 'a':
                    col = 1;
                    break;
                case 'W':
                    col = 2;
                    break;
                case 'E':
                    col = 3;
                    break;
                default:
                    break;
            }

            // inside rearragement
            List<List<string>> tgL = new List<List<string>>();
            foreach (var blist in gL)
            {
                var items1 = new List<Tuple<int, string>> { };
                foreach (string name in blist)
                {
                    string path = @"C:\병신\분\" + date.ToString() +
                        "\\" + name + ".txt";

                    if (!File.Exists(path))
                    {
                        continue;
                    }


                    Stream sf = System.IO.File.Open(path,
                                          FileMode.Open,
                                          FileAccess.Read,
                                          FileShare.ReadWrite);

                    StreamReader sr = new StreamReader(sf);

                    string line;
                    int savedvalue = 0;

                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] words = line.Split(' ');

                        if (Convert.ToInt32(words[0]) > 0)
                            savedvalue = Convert.ToInt32(words[col]);
                        else
                            break;
                    }
                    items1.Add(Tuple.Create(savedvalue, name));
                }
                if (items1.Count == 0)
                {
                    continue;
                }
                items1 = items1.OrderByDescending(t => t.Item1).ToList();

                blist.Clear();
                foreach (var item in items1)
                    blist.Add(item.Item2); // stockname in descending order
                tgL.Add(blist);
            }

            /* outside rearragement 매우 중요한 문제로서 gL의
            첫번 째 컬럼에 같은 이름이 반복되지 않는다는 가정으로
            아래의 프로그램이 작성되었음 */
            var items = new List<Tuple<int, string>> { };
            List<string> elist = new List<string>();
            foreach (var flist in tgL)
            {
                string path = @"C:\병신\분\" + date.ToString() +
                    "\\" + flist[0] + ".txt";

                if (!File.Exists(path))
                    continue;

                Stream sf = System.IO.File.Open(path,
                                        FileMode.Open,
                                        FileAccess.Read,
                                        FileShare.ReadWrite);

                StreamReader sr = new StreamReader(sf);

                string line;
                int savedvalue = 0;

                while ((line = sr.ReadLine()) != null)
                {
                    string[] words = line.Split(' ');

                    if (Convert.ToInt32(words[0]) > 0)
                        savedvalue = Convert.ToInt32(words[col]);
                    else
                        break;
                }
                items.Add(Tuple.Create(savedvalue, flist[0]));
            }
            items = items.OrderByDescending(t => t.Item1).ToList();

            gL.Clear();
            foreach (var item in items)
            {
                elist = find_리스트(item.Item2, tgL);
                gL.Add(elist);
            }
        }

        public static int read_종목(int date, string stockname, int[,] x)
        {
            if (date < 0)
            {
                DateTime now = DateTime.Now;
                date = Convert.ToInt32(now.ToString("yyyyMMdd"));
            }

            string file = @"C:\병신\분\" + date.ToString() + "\\" + stockname + ".txt";
            if (!File.Exists(file))
            {
                return -1;
            }
            Stream sf = System.IO.File.Open(file,
                                  FileMode.Open,
                                  FileAccess.Read,
                                  FileShare.ReadWrite);

            StreamReader sr = new StreamReader(sf);

            string previous_line = "    "; // the length should be bigger than 4 to compare with line, otherwise error occurs
            string line = "";
            int nrow = 0;


            string[] words;
            int HHmm;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Substring(0, 4) == previous_line.Substring(0, 4))
                {
                    previous_line = line;
                }
                else
                {
                    if (nrow == 0)
                    {
                        nrow++;
                        previous_line = line;
                        continue;
                    }
                    words = previous_line.Split(' ');

                    HHmm = Convert.ToInt32(words[0]);
                    if (HHmm < 901 || HHmm > 1520)
                        continue;

                    x[nrow, 0] = HHmm;    // time
                    x[nrow, 1] = Convert.ToInt32(words[1]);
                    x[nrow, 2] = Convert.ToInt32(words[2]);
                    x[nrow, 3] = Convert.ToInt32(words[3]);

                    nrow++;

                    previous_line = line;

                }
            }

            if (previous_line != "    ")
            {
                words = previous_line.Split(' ');

                HHmm = Convert.ToInt32(words[0]);
                if (HHmm >= 901 || HHmm <= 1520)
                {
                    x[nrow, 0] = Convert.ToInt32(words[0]);    // time
                    x[nrow, 1] = Convert.ToInt32(words[1]);
                    x[nrow, 2] = Convert.ToInt32(words[2]);
                    x[nrow, 3] = Convert.ToInt32(words[3]);
                }
                nrow++;
            }

            x[0, 0] = 900;
            for (int k = 0; k < 3; k++)
                x[0, k + 1] = x[1, k + 1]; //수정
            x[nrow, 0] = 0;
            return 0;
        }



    }
}
