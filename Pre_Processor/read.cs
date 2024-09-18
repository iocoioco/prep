//using StockLibrary;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        // 전고
        public class DailyData
        {
            public DateTime Date { get; set; }
            public int Close { get; set; } // Using int to represent the closing price in cents
        }

        public static int FindHighestClose(string fileName, int duration)
        {
            List<DailyData> dailyDataList = new List<DailyData>();

            foreach (var line in File.ReadLines(fileName))
            {
                var parts = line.Split(' ');
                dailyDataList.Add(new DailyData
                {
                    Date = DateTime.ParseExact(parts[0], "yyyyMMdd", CultureInfo.InvariantCulture),
                    Close = (int)(double.Parse(parts[4])) // Convert to cents
                });
            }

            // If duration is longer than the number of data lines, return 10000000
            if (duration > dailyDataList.Count)
            {
                return 10000000;
            }

            // Reverse the order to make the last line the most recent date
            dailyDataList.Reverse();

            DateTime endDate = dailyDataList[0].Date; // Most recent date
            DateTime startDate = endDate.AddDays(-duration);

            int highestClose = int.MinValue;

            foreach (var dailyData in dailyDataList)
            {
                if (dailyData.Date >= startDate && dailyData.Date <= endDate)
                {
                    if (dailyData.Close > highestClose)
                    {
                        highestClose = dailyData.Close;
                    }
                }
            }

            return highestClose;
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

        public static void 라인분리(string line, ref int data) // scalar -> no 비중, dev, mkc
        {
            string[] words = line.Split('\t');
            Int32.TryParse(words[1], out data);
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

        public static List<string> read_제외()
        {
            List<string> gl_list = new List<string>();

            string filename = @"C:\병신\data\제외.txt"; ;

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

        public static bool readStockMinuteCheck(int nrow, int[,] x)
        {
            if (nrow != 382)
                return false;
            if (x[381, 0] / 100 != 1520) // end time is not 1520
                return false;

            // time inc ? 
            // amount inc ? 
            for (int i = 1; i < 382; i++)
            {
                if (x[i, 0] < x[i - 1, 0]) // time passing backwards
                    return false;

                if (x[i, 7] < x[i - 1, 7]) // negative deal
                    return false;


                // x[i, 0] - x[i - 1, 0] miniute difference should be 1 miniute
                if ((x[i, 0] / 100) % 100 == 0)
                {
                    if (x[i, 0] / 100 - x[i - 1, 0] / 100 != 41) // o'clock i.e. 1000, 1100 ... 
                        return false;
                }
                else
                {
                    if (x[i, 0] / 100 - x[i - 1, 0] / 100 != 1) // not o'clock i.e. 1001 etc
                        return false;
                }

                if (x[i, 1] < -3000 || x[i, 1] > 3000)     // price less than 3,000, and larger than -3,000
                {
                    return false;
                }
                int sequence_id = (x[i, 0] / 10000 - 9) * 60 + (x[i, 0] % 10000) / 100 + 1; 
                if (sequence_id < 1 || sequence_id >= 382)
                    return false;
            }
            return true;
        }

        public static int ReadStockMinute(int date, string stock, int[,] x)
        {
            if (date < 0)
            {
                DateTime now = DateTime.Now;
                date = Convert.ToInt32(now.ToString("yyyyMMdd"));
            }

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
                if (nrow == g.MAX_ROW)
                    break;
                //if (x[nrow - 1 , 0] > 152100) //0505 from nrow > 390 to current if
                //{
                //    x[nrow - 1, 0] = 0;
                //    nrow--;
                //    break;
                //}
            }

            // 이 부분은 필요가 없을 듯 한 데 왜 들어가 있는 지 모르겠네 ? 
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

            return nrow;
        }

        public static int read_전일종가_전일거래액_천만원(string stock)
        {
            string path = @"C:\병신\data\일\" + stock + ".txt";
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

            string path = @"C:\병신\data\일\" + stock + ".txt";
            if (!File.Exists(path))
            {
                return -1;
            }

            string lastline = File.ReadLines(path).Last(); // last line read 

            string[] words = lastline.Split(' ');
            return Convert.ToInt32(words[4]);
        }

        public static List<string> read_그룹_네이버_업종() // this is for single list of stocks in 그룹_네이버_업종
        {
            _cpstockcode = new CPUTILLib.CpStockCode();

            List<string> gl_list = new List<string>();

            string filepath = @"C:\병신\data\그룹_네이버_업종.txt";
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
