﻿
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
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

      

        public static int ReadStockMinute_LasLine(int date, string stock, int[] x)
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

                if (words[0].Length > 0 && !_gl.Contains(words[0]))
                {
                    _gl.Add(words[0]);
                }
            }
        }

        public static bool isStock(string stock)
        {
            _cpstockcode = new CPUTILLib.CpStockCode();
            if (stock == "")
                return false;


            string code = _cpstockcode.NameToCode(stock); // 코스피혼합, 코스닥혼합 code.Length = 0 제외될 것임
            if (code.Length == 7 && code[0] == 'A')
                return true;
            else
                return false;
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


      






        public static List<string> read_그룹_네이버_업종(List<List<string>> GL)
        {
            List<string> gl_list = new List<string>();

            string filepath = @"C:\병신\data\그룹_네이버_업종.txt";
            if (!File.Exists(filepath))
            {
                return gl_list;
            }


            string[] grlines = File.ReadAllLines(filepath, Encoding.UTF8);

            List<string> GL_list = new List<string>();

            int count = 0;
            foreach (string line in grlines)
            {
                List<string> Gl_list = new List<string>();

                string[] words = line.Split('\t');



                if (words[0] != "")
                {
                    string stockname = words[0].Replace(" *", ""); // 코스닥 * 표시되어 있어 제거, 코스피는 없음
                    if (!isStock(stockname)) // code starts from 'A'
                        continue;

                    if (gl_list.Contains(stockname))
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

     




    }
}
