using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pre_Processor
{
    internal class wk
    {
        static CPUTILLib.CpStockCode _cpstockcode;

        public static bool isWorkingHour()
        {
            if (!g.connected)
                return false;

            DateTime date = DateTime.Now;
            int HHmm = Convert.ToInt32(DateTime.Now.ToString("HHmm"));
            int datenow = Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd"));
            if (g.date != datenow)
                return false;

            if (HHmm < 900 || HHmm > 1530)
                return false;

            if (date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday)
                return false;

            else
                return true;
        }

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

        public static int return_integer_from_mixed_string(string input)
        {
            int value = 0;
            foreach (char c in input)
            {
                if ((c >= '0') && (c <= '9'))
                {
                    value = value * 10 + (c - '0');
                }
            }
            return value;
        }


        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        public static void calculate_cyan_magenta_in_stock()
        {
            for (int i = 0; i < g.ogl_data.Count; i++)
            //for (int i = 0; i < 30; i++)
            {
                int[,] x = new int[400, 12];
                int[,] d = new int[400, 2];

                int nrow = rd.read_Stock_Minute(g.date, g.ogl_data[i].종목, x);
                if (nrow <= 1)
                    continue;

                int[,] a = new int[400, 2];
                a[1, 0] = 1;
                a[1, 1] = 1;

                for (int k = 0; k < 2; k++)
                {
                    for (int j = 2; j < nrow; j++)
                    {
                        int diff_price = x[j, 1] - x[j - 1, 1];
                        int diff_amount_or_intensity = x[j, k + 2] - x[j - 1, k + 2];
                        if (j == 1) // intensity multiplied by g.HUNDRED to make integer with accuracy
                        {
                            diff_amount_or_intensity = (int)(diff_amount_or_intensity / g.HUNDRED);
                        }
                        if (diff_amount_or_intensity > 0)
                        {
                            d[j, k] = 1;
                            a[j, k] = a[j - 1, k] + 1;
                        }
                        else
                        {
                            a[j, k] = 0;
                        }
                    }
                }
            }
        }

        public static double convert_time_to_6_digit_integer(string t)
        {
            double value;
            string[] time = t.Split(':');

            value = Convert.ToInt32(time[0]) * 10000.0 +
                        Convert.ToInt32(time[1]) * 100.0 * 100.0 / 60.0 +
                        Convert.ToInt32(time[2]) * 100.0 / 60.0;

            return value;
        }


        //public static string return_Group_ranking(string stock)
        //{
        //    foreach (var t in g.Group_ranking)
        //    {
        //        if (t == null || t.종목들 == null)
        //            return "X";

        //        if (t.종목들.Contains(stock))
        //        {
        //            return t.랭킹.ToString();
        //        }
        //    }
        //    return "X";
        //}

        //public static string return_Group_ranking_통과종목수(string stock)
        //{
        //    foreach (var t in g.Group_ranking)
        //    {
        //        if (t == null || t.종목들 == null)
        //            return "X";

        //        if (t.종목들.Contains(stock))
        //        {
        //            return t.통과종목수.ToString();
        //        }
        //    }
        //    return "X";
        //}

        public static int return_index_of_ogldata(string stock)
        {
            int index = -1;
            index = g.ogl_data.FindIndex(x => x.종목 == stock);

            return index;
        }

        public static string return_dgv_stock(DataGridView dgv)
        {
            string dgv_stock = "";
            if (dgv.Rows[11].Cells[0].Value != null)
            {
                string a = dgv.Rows[11].Cells[0].Value.ToString(); // dgv에 표시된 주식
                string b = a.Replace("(", "");
                dgv_stock = b.Replace(")", "");
            }
            return dgv_stock;
        }

        public static void marketeye_종목(ref string[] codes)
        {
            int accumulated_count = 0;
            string code;

            g.지수보유관심종목.Clear();

            g.지수보유관심종목.Add("KODEX 레버리지");
            g.지수보유관심종목.Add("KODEX 코스닥150레버리지");

            foreach (var stock in g.보유종목)
            {
                if (!g.지수보유관심종목.Contains(stock))
                {
                    g.지수보유관심종목.Add(stock);
                }
            }
            foreach (var stock in g.관심종목)
            {
                if (!g.지수보유관심종목.Contains(stock))
                {
                    g.지수보유관심종목.Add(stock);
                }
            }

            // KODEX 레버리지, 코스닥 레버리지 + 보유종목 + 관심종목 : 항상 포함
            foreach (var stock in g.지수보유관심종목) // g.지수보유관심종목 추가  
            {
                if (accumulated_count == g.stocks_per_marketeye)  // g.stocks_per_marketeye : max. number of stocks per one cycle
                    break;

                int index = g.ogl_data.FindIndex(x => x.종목 == stock);
                if (index < 0)
                    continue;

                if (!codes.Contains(g.ogl_data[index].code) && accumulated_count < g.stocks_per_marketeye)
                {
                    codes[accumulated_count++] = g.ogl_data[index].code;
                }
            }

            //if (g.시초)
            //{
            //    for (int i = g.ogl_data_next; i < g.ogl_data.Count; i++)
            //    {
            //        if (accumulated_count == g.stocks_per_marketeye) //0504
            //            break;

            //        if (g.ogl_data[i].code != null && g.ogl_data[i].code.Length == 7)
            //        {
            //            if (codes.Contains(g.ogl_data[i].code))
            //                continue;

            //            codes[accumulated_count++] = g.ogl_data[i].code;
            //            g.ogl_data_next = i + 1;
            //            if (g.ogl_data_next == g.ogl_data.Count)
            //            {
            //                i = 0;
            //            }
            //        }
            //    }
            //}
            //else
            //{
            // stock_eval에서 최우선 순위의 g.stocks_per_marketeye 개 선택
            // 다운 전 초기에는 시총이 큰 종목 우선 선택
            // 위의 지수보유관심종목을 넣고 g.stocks_per_marketeye 개까지 codes에 순차적으로 입력

            lock (g.lockObject) // g.sl  추가, 특정조건 하에서 선택된 활동성 높은 종목들
            {
                foreach (var stock in g.sl) //BLOCK g.sl may be changed 컬렉션이 수정되었습니다 ... 메세지
                {
                    if (accumulated_count >= g.stocks_per_marketeye / 2 ||
                        accumulated_count == g.stocks_per_marketeye)  // g.stocks_per_marketeye : max. number of stocks per one cycle
                        break;

                    code = _cpstockcode.NameToCode(stock); // 코스피혼합, 코스닥혼합 code.Length = 0 제외될 것임
                    if (codes.Contains(code))
                        continue;

                    if (code != null && code.Length == 7)
                    {
                        codes[accumulated_count++] = code;
                    }
                }
            }

            // ogl_data 추가
            for (int i = g.ogl_data_next; i < g.ogl_data.Count; i++)
            {
                if (accumulated_count == g.stocks_per_marketeye) //0504
                    break;

                if (g.ogl_data[i].code != null && g.ogl_data[i].code.Length == 7)
                {
                    if (codes.Contains(g.ogl_data[i].code))
                        continue;

                    codes[accumulated_count++] = g.ogl_data[i].code;
                    g.ogl_data_next = i + 1;
                    if (g.ogl_data_next == g.ogl_data.Count)
                    {
                        g.ogl_data_next = 0;
                    }
                }
            }

            // ogl_data 끝까지 가면서 추가하여도 g.stocks_per_marketeye 안 되면 ogl_data 처음부터 추가
            if (accumulated_count != g.stocks_per_marketeye)
            {
                for (int i = 0; i < g.ogl_data.Count; i++)
                {
                    if (accumulated_count == g.stocks_per_marketeye) //0504
                        break;

                    if (g.ogl_data[i].code != null && g.ogl_data[i].code.Length == 7)
                    {
                        if (codes.Contains(g.ogl_data[i].code))
                            continue;

                        codes[accumulated_count++] = g.ogl_data[i].code;
                        g.ogl_data_next = i + 1;
                        if (g.ogl_data_next == g.ogl_data.Count)
                        {
                            g.ogl_data_next = 0;
                        }
                    }
                }
            }
        }


        public static bool gen_ogl_data(string stock)
        {
            if (rd.read_단기과열(stock))
                return false;

            _cpstockcode = new CPUTILLib.CpStockCode();

            var o = new g.stock();

            o.종목 = stock;
            o.code = _cpstockcode.NameToCode(stock);
            if (o.code.Length != 7)
                return false; //루미마이크로, 메디포럼제약 코드 못 찾음, 합병된 것으로 추정

            o.전일종가 = rd.read_전일종가(stock);

            o.avr = 0.0;
            o.dev = 0.0;
            o.dev_avr = ""; 
            int days = 20;
            o.dev_avr = calcurate_종목일중변동평균편차(stock, days, ref o.avr, ref o.dev,
                ref o.avr_dealt, ref o.min_dealt, ref o.max_dealt, ref o.일평균거래량, ref o.pass.long_high);

            if (o.dev_avr == "")
                return false;

            o.일평균거래액 = (int)(o.전일종가 * (o.일평균거래량 / g.억원));
            if (o.일평균거래량 == 0)
                return false;



            o.pass.long_high = (int)((o.pass.long_high - o.전일종가) * 10000.0 / o.전일종가);

            o.시총 = rd.read_시총(stock) / 100; // 시총 값 부정확 점검필요 
            if (o.시총 == -1)
                return false;

            o.전일거래액_천만원 = rd.read_전일종가_전일거래액_천만원(stock);
            if (o.전일거래액_천만원 == -1)
                return false;

            o.시장구분 = rd.read_코스피코스닥시장구분(stock);
            if (o.시장구분 != 'S' && o.시장구분 != 'D')
                return false;

            g.ogl_data.Add(o);
            return true;
        }

        public static void 일평균거래액10억이상종목선택(List<string> tsl, int 최소거래액이상_억원)
        {
            var tuple = new List<Tuple<ulong, string>> { };

            int days = 20;
            foreach (var stock in tsl)
            {
                string path = @"C:\WORK\data\일\" + stock + ".txt";
                if (!File.Exists(path))
                    continue;

                List<string> lines = File.ReadLines(path).Reverse().Take(days).ToList(); // 파일 후반 읽기

                ulong avr_day_dealt_money = 0;

                foreach (var line in lines)
                {
                    string[] words = line.Split(' ');
                    avr_day_dealt_money += (ulong)(Convert.ToDouble(words[4]) * Convert.ToUInt64(words[5]) / g.억원); // 종가 * 당일거래량
                }
                avr_day_dealt_money = avr_day_dealt_money / (ulong)lines.Count;
                tuple.Add(Tuple.Create(avr_day_dealt_money, stock));
            }
            tuple = tuple.OrderByDescending(t => t.Item1).ToList();

            tsl.Clear();

            foreach (var item in tuple)
            {
                if (item.Item1 < (ulong)최소거래액이상_억원)
                    break;

                tsl.Add(item.Item2);
            }
        }

        public static void 전일거래액_천만원_순서(List<string> gl)
        {

            var 종목 = new List<Tuple<int, string>> { };

            foreach (var stock in gl)
            {
                if (g.KODEX4.Contains(stock)) continue;

                int index = g.ogl_data.FindIndex(x => x.종목 == stock);
                if (index < 0) continue;

                종목.Add(Tuple.Create(g.ogl_data[index].전일거래액_천만원, stock));
            }
            종목 = 종목.OrderByDescending(t => t.Item1).ToList();

            gl.Clear();

            foreach (var item in 종목)
                gl.Add(item.Item2);
        }



        public static void 시총순서(List<string> gl)
        {
            var 종목 = new List<Tuple<double, string>> { };

            foreach (var stock in gl)
            {
                if (g.KODEX4.Contains(stock)) continue;

                int index = g.ogl_data.FindIndex(x => x.종목 == stock);
                if (index < 0) continue;

                종목.Add(Tuple.Create(g.ogl_data[index].시총, stock));
            }
            종목 = 종목.OrderByDescending(t => t.Item1).ToList();

            gl.Clear();

            foreach (var item in 종목)
                gl.Add(item.Item2);
        }

        public static List<string> 코피순서(List<string> gl)
        {
            List<string> list = new List<string>();

            var 종목 = new List<Tuple<double, string>> { };

            foreach (var stock in gl)
            {
                int index = g.ogl_data.FindIndex(x => x.종목 == stock);
                if (index < 0) continue;

                if (g.ogl_data[index].시장구분 != 'S')
                    continue;

                종목.Add(Tuple.Create(g.ogl_data[index].시총, stock));
            }
            종목 = 종목.OrderByDescending(t => t.Item1).ToList();

            foreach (var item in 종목)
                list.Add(item.Item2);

            return list;
        }

        public static List<string> 코닥순서(List<string> gl)
        {
            List<string> list = new List<string>();

            var 종목 = new List<Tuple<double, string>> { };

            foreach (var stock in gl)
            {
                int index = g.ogl_data.FindIndex(x => x.종목 == stock);
                if (index < 0) continue;

                if (g.ogl_data[index].시장구분 != 'D')
                    continue;

                종목.Add(Tuple.Create(g.ogl_data[index].시총, stock));
            }
            종목 = 종목.OrderByDescending(t => t.Item1).ToList();

            foreach (var item in 종목)
                list.Add(item.Item2);

            return list;
        }

        public static void 편차순서(List<string> gl)
        {
            var 종목 = new List<Tuple<double, string>> { };
            var uniqueItemsList = gl.Distinct().ToList();


            foreach (var stock in gl)
            {
                int index = g.ogl_data.FindIndex(x => x.종목 == stock);
                if (index < 0)
                    continue;

                종목.Add(Tuple.Create(g.ogl_data[index].dev, stock));
            }
            종목 = 종목.OrderByDescending(t => t.Item1).ToList();

            gl.Clear();

            foreach (var item in 종목)
                gl.Add(item.Item2);
        }

        public static void 평균순서(List<string> gl)
        {
            var 종목 = new List<Tuple<double, string>> { };
            var uniqueItemsList = gl.Distinct().ToList();


            foreach (var stock in gl)
            {
                int index = g.ogl_data.FindIndex(x => x.종목 == stock);
                if (index < 0)
                    continue;

                종목.Add(Tuple.Create(g.ogl_data[index].avr, stock));
            }
            종목 = 종목.OrderByDescending(t => t.Item1).ToList();

            gl.Clear();

            foreach (var item in 종목)
                gl.Add(item.Item2);
        }

        public static void 평거순서(List<string> gl)
        {
            var 종목 = new List<Tuple<int, string>> { };
            var uniqueItemsList = gl.Distinct().ToList();


            foreach (var stock in gl)
            {
                int index = g.ogl_data.FindIndex(x => x.종목 == stock);
                if (index < 0)
                    continue;

                종목.Add(Tuple.Create(g.ogl_data[index].일평균거래액, stock));
            }
            종목 = 종목.OrderByDescending(t => t.Item1).ToList();

            gl.Clear();

            foreach (var item in 종목)
                gl.Add(item.Item2);
        }



        public static void 종가기준추정누적거래액_천만원순서(List<string> gl)
        {
            var 종목 = new List<Tuple<int, string>> { };

            foreach (var stock in gl)
            {
                if (g.KODEX4.Contains(stock)) continue;

                int index = g.ogl_data.FindIndex(x => x.종목 == stock);
                if (index < 0) continue;

                종목.Add(Tuple.Create(g.ogl_data[index].종거천, stock));
            }
            종목 = 종목.OrderByDescending(t => t.Item1).ToList();

            gl.Clear();
            gl.Add("KODEX 레버리지");
            gl.Add("KODEX 코스닥150레버리지");

            foreach (var item in 종목)
            {
                gl.Add(item.Item2);
            }
        }

        public static List<string> 분거래천_순서(List<string> gl)
        {
            var a_tuple = new List<Tuple<int, string>> { };
            List<string> list = new List<string>();

            if (gl == null) // 날짜변경 후 'f' 입력의 경우 발생
                return null;

            foreach (var stock in gl)
            {
                if (g.KODEX4.Contains(stock)) continue;

                int index = g.ogl_data.FindIndex(x => x.종목 == stock);
                if (index < 0) continue;

                a_tuple.Add(Tuple.Create(g.ogl_data[index].분거래천[0], stock));
            }
            a_tuple = a_tuple.OrderByDescending(t => t.Item1).ToList();

            foreach (var t in a_tuple)
            {
                list.Add(t.Item2);
            }
            return list;
        }

        public static void 순서(List<string> gl)
        {

            // deviation order
            var t = g.ogl_data.OrderByDescending(x => x.avr_dealt).ToList();

            //var a_tuple = new List<Tuple<int, string>> { };
            //List<string> list = new List<string>();


            //if (gl == null) // 날짜변경 후 'f' 입력의 경우 발생
            //    return null;

            //foreach (var stock in gl)
            //{
            //    if (stock.Contains("KODEX")) continue;

            //    int index = g.ogl_data.FindIndex(x => x.종목 == stock);
            //    if (index < 0) continue;

            //    a_tuple.Add(Tuple.Create(g.ogl_data[index].avr_dealt, stock));
            //}
            //a_tuple= a_tuple.OrderByDescending(t => t.Item1).ToList();

            foreach (var item in t)
            {
                string[] str = new string[6];

                str[0] = item.종목.ToString();
                str[1] = item.avr_dealt.ToString();
                str[2] = item.max_dealt.ToString();
                str[3] = item.min_dealt.ToString();
                str[4] = item.dev.ToString();
                str[5] = item.avr.ToString();

                wr.w(str);
            }
        }



        public static double ComputeCoeff(string stockname, double[] values1, double[] values2)
        {
            if (values1.Length != values2.Length)
            {
                throw new ArgumentException("values must be the same length");
            }


            var avg1 = values1.Average();
            var avg2 = values2.Average();

            var sum1 = values1.Zip(values2, (x1, y1) => (x1 - avg1) * (y1 - avg2)).Sum();

            var sumSqr1 = values1.Sum(x => Math.Pow((x - avg1), 2.0));
            var sumSqr2 = values2.Sum(y => Math.Pow((y - avg2), 2.0));

            var result = sum1 / Math.Sqrt(sumSqr1 * sumSqr2);

            return result;
        }

        //     public static int read_외인기관일별매수동향(string stockname, g.stock data)
        //     {
        //         /* 양매수신뢰도 // 상관
        // 양매수수 // 전일 1, 2 전전일 3,4, 등
        // 외량 // 16거래일 외인거래량 
        // 기량 // 16거래일 기관거래량
        // 개량 // 16거래일 개인거래량
        //*/

        //         string path = @"C:\Work\매\" + stockname + ".txt";
        //         // 16일 데이터만 있는 데 신규는 적을 수 있음
        //         if (!File.Exists(path))
        //         {
        //             //string t = "매 file not exist " + stockname;
        //             //MessageBox.Show(t);
        //             return -1;
        //         }

        //         string[] grlines = System.IO.File.ReadAllLines(path, Encoding.Default);

        //         int count_매 = 0;
        //         int last_day_매 = 0;
        //         foreach (var line in grlines)
        //         {
        //             string[] words = line.Split(' ');
        //             if (count_매 == 0)
        //                 last_day_매 = Convert.ToInt32(words[0]);

        //             data.개량[count_매] = Convert.ToInt32(words[1]);
        //             data.외량[count_매] = Convert.ToInt32(words[2]);
        //             data.기량[count_매] = Convert.ToInt32(words[3]);
        //             count_매++;
        //         }

        //         path = @"C:\Work\일\" + stockname + ".txt";
        //         if (!File.Exists(path))
        //         {
        //             //string t = "일 file not exist " + stockname;
        //             //MessageBox.Show(t);
        //             return -1;
        //         }


        //         if (count_매 < 16)
        //         {
        //             count_매--;
        //         }
        //         List<string> lines = File.ReadLines(path).Reverse().Take(count_매 + 1).ToList();
        //         double[] pricediffer = new double[count_매];
        //         double[] instandfrig = new double[count_매];
        //         double[] closeprice = new double[count_매 + 1];
        //         double[] day_amount = new double[count_매 + 1];

        //         int count_일 = 0;
        //         int last_day_일 = 0;
        //         foreach (var line in lines)
        //         {
        //             string[] words = line.Split(' ');
        //             if (count_일 == 0)
        //                 last_day_일 = Convert.ToInt32(words[0]);
        //             closeprice[count_일] = Convert.ToDouble(words[4]);
        //             day_amount[count_일] = Convert.ToDouble(words[5]);
        //             count_일++;
        //         }
        //         if (last_day_일 != last_day_매)
        //         {
        //             //string t = "일 매 파일들의 마지막 날짜 불일치 " + stockname;
        //             //MessageBox.Show(t);
        //             return -1;
        //         }

        //         for (int i = 0; i < count_매; i++)
        //         {
        //             if (day_amount[i] == 0)
        //             {
        //                 continue;
        //             }
        //             pricediffer[i] = closeprice[i] - closeprice[i + 1];
        //             instandfrig[i] = data.개량[i] * -1.0 / day_amount[i];
        //             // 당일 개인 매도량 나누기 당일거래량
        //         }

        //         data.양매수신뢰도 = ComputeCoeff(stockname, pricediffer, instandfrig);


        //         if (data.외량[0] > 0 || data.기량[0] > 0)
        //             data.양매수수 = 1;
        //         if (data.외량[0] > 0 && data.기량[0] > 0)
        //             data.양매수수 = 2;
        //         if (data.외량[0] > 0 && data.기량[0] > 0 && (data.외량[1] > 0 || data.기량[1] > 0))
        //             data.양매수수 = 3;
        //         if (data.외량[0] > 0 && data.기량[0] > 0 && data.외량[1] > 0 && data.기량[1] > 0)
        //             data.양매수수 = 4;

        //         return 0;
        //     }


        // avr_dealt, min_dealt, max_dealt : not used, just for reference
        public static string calcurate_종목일중변동평균편차(string stock, int days, ref double avr, ref double dev,
                                    ref int avr_dealt, ref int min_dealt, ref int max_dealt, ref ulong 일평균거래량, ref int long_high)
        {
            string path = @"C:\Work\data\일\" + stock + ".txt";
            if (!File.Exists(path))
                return "";

            List<string> lines = File.ReadLines(path).Reverse().Take(days + 100).ToList(); // 파일 후반 읽기

            if (lines.Count < 1) // 신규상장의 경우 데이터 숫자 20 보다 적음
                return "";

            List<Double> day_list = new List<Double>();
            List<Double> long_day_list = new List<Double>();

            int day_dealt;

            avr_dealt = 0;
            max_dealt = 0;
            min_dealt = 1000000; // 단위 억원
            일평균거래량 = 0;

            int days_count = 0;
            // 20일 일평균거래량, avr, dev, 
            for (int i = 0; i < 20; i++)
            {
                if (i == lines.Count) // 신규 등 lines 숫자 20개 보다 작은 경우
                    break;

                string[] words = lines[i].Split(' ');
                if (words.Length != 10)
                    return "";
                if (Convert.ToDouble(words[5]) == 0 &&
                  Convert.ToDouble(words[6]) == 0 &&
                  Convert.ToDouble(words[7]) == 0 &&
                  Convert.ToDouble(words[8]) == 0)
                    return "";

                double start_price = Convert.ToDouble(words[1]); // 시가
                double close_price = Convert.ToDouble(words[4]); // 종가
                일평균거래량 += Convert.ToUInt64(words[5]); // 

                day_dealt = (int)(Convert.ToInt32(words[5]) * close_price / g.억원); // 일거래량 X 종가 / 억원
                avr_dealt += day_dealt;
                if (day_dealt > max_dealt)
                    max_dealt = day_dealt;
                if (day_dealt < min_dealt)
                    min_dealt = day_dealt;

                double diff = (close_price - start_price) / start_price * 100;

                day_list.Add(diff);

                days_count++;
            }

            if (days_count == 0)
                return "";

            avr_dealt = avr_dealt / days_count;
            double temp_avr = 0.0;
            dev = 0.0;
            if (days_count > 0)
            {
                temp_avr = day_list.Sum() / days_count;
                if (days_count <= 1)
                    dev = 0;
                else
                    dev = Math.Sqrt(day_list.Sum(x => Math.Pow(x - temp_avr, 2)) / (days_count - 1));
            }

            string str = temp_avr.ToString("0.#") + "/" + dev.ToString("0.#");
            avr = temp_avr;

            일평균거래량 = 일평균거래량 / (ulong)days_count;

            // 120일 중 전고
            long_high = 0;
            foreach (var line in lines)
            {
                string[] words = line.Split(' ');
                for (int i = 1; i <= 4; i++)
                {
                    int price = Convert.ToInt32(words[i]); // 시가, 고가, 저가, 종가

                    if (price > long_high) // currently long_high is not percentage
                        long_high = price;
                }
            }

            return str;
        }








        /*
    public static void read_외인기관일별매수동향(string stock, g.stock data)
    {

    string path = @"C:\Work\매\" + stock + ".txt";
    if (!File.Exists(path))
    {
        return;
    }

    string[] grlines = System.IO.File.ReadAllLines(path, Encoding.Default);

    int inc = 0;
    int day = 0;
    foreach(var line in grlines)
    {
        string[] words = line.Split(' ');
        if (inc == 0)
        day = Convert.ToInt32(words[0]);

        data.개량[inc] = Convert.ToInt32(words[1]); 
        data.외량[inc] = Convert.ToInt32(words[2]);
        data.기량[inc] = Convert.ToInt32(words[3]);
        inc++;
    }

    path = @"C:\Work\일\" + stock + ".txt";
    if (!File.Exists(path))
        return;

    List<string> lines = File.ReadLines(path).Reverse().Take(inc+1).ToList();
    double[] pricediffer = new double[inc  ];
    double[] closeprice =  new double[inc+1];
    int add = 0;
    foreach (var line in lines)
    {
        string[] words = line.Split(' ');
        closeprice[add++]= Convert.ToDouble(words[4]);
    }

    double[] instandfrig = new double[16];
    for(int i = 0; i < inc; i++)
    {
        pricediffer[i] = closeprice[i] - closeprice[i + 1];
        instandfrig[i] = data.개량[i] * -1.0;
    }

    data.양매수신뢰도 = cr.ComputeCoeff(pricediffer, instandfrig);


    if (data.외량[0] > 0 || data.기량[0] > 0)
        data.양매수수 = 1;
    if (data.외량[0] > 0 && data.기량[0] > 0)
        data.양매수수 = 2;
    if (data.외량[0] > 0 && data.기량[0] > 0 && (data.외량[1] > 0 || data.기량[1] > 0))
        data.양매수수 = 3;
    if (data.외량[0] > 0 && data.기량[0] > 0 && data.외량[1] > 0 && data.기량[1] > 0)
        data.양매수수 = 4;

    }

        /*  Calling Method : 그룹에서 전체를 읽고 group & single 반환
    *  스펠링 미스에 대한 점검 name to code를 하지 않아 오류 가능
    *  List<List<string>> groupList = new List<List<string>>();
    List<string> singleList = new List<string>();

    FileLib.Class.read_그룹(singleList, groupList);
    */
        /* 24일 거래량 중 상, 하 2개씩 극단을 제외하고 일평균거래량 환산 
    *  public static int calculate_종목20일기준일평균거래량(string stock)
    */
        public static ulong calculate_종목20일기준일평균거래량(string stock)
        {
            // Extract column 5 from stock filename
            string filename = @"C:\WORK\data\일\" + stock + ".txt";
            int[] c_id = new int[1]; // number of columns needed
            string[,] x = new string[1000, 1]; // array declaration
            List<double> alist = new List<double>();
            int nrow = 0;
            double average;

            c_id[0] = 5; // everyday amount dealed 

            nrow = rd.read_데이터컬럼들(filename, c_id, x);

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

            return (ulong)average;
        }

        /* 주어진 date, 두 개의 시간 구간 time[0]에서 time[1]로 1분 씩 증가시키면서 주어진 x[time[0], col]의 x[,col]의 값
	    * 의 최대 차이, 최소 차이를 구하여 반환한다. 예를 들면 가격이 일정량 점프하였는 데 그 후 30분 내 점프한 값으로부터 
		    최대 얼마나 하락할 지 최대 얼마나 상승할 지 알아보는 루틴 */






        public static double 시분초_일중환산율(int hhmmss)
        {
            int hh = hhmmss / 10000;
            int mm = hhmmss % 10000 / 100;
            int ss = hhmmss % 100;

            double numerator = 6 * 60 * 60 + 21 * 60;
            double denominator = (hh - 9) * 60 * 60 + mm * 60 + ss;
            if (denominator < 0.1)
                denominator = 1.0;
            if (denominator > numerator)
                numerator = denominator;

            return numerator / denominator;
        }


        public static double 누적거래액환산율(int hhmmss)
        {
            double value = 0;
            if (hhmmss > 10000) // if 6 digit is passed, make it 4 digit
                hhmmss /= 100;

            int hh = Convert.ToInt32(hhmmss) / 100;
            int mm = Convert.ToInt32(hhmmss) % 100;

            if (hh >= 15)
            {
                if (mm > 20)
                {
                    mm = 20;
                }
            }
            value = (hh - 9) * 60 + mm + 1; // 시작시간이 9시인 경우
            // value = (hh - 10) * 60 + mm + 1; // 시작시간이 10시인 경우
            if (value < 0 || value > 6 * 60 + 20) // 0900 ~ 1520 이외 시간에는 380 리턴
                value = 6 * 60 + 21;

            double return_value = 381.0 / value;
            return return_value;
        }


        public static int directory_분전후(int date_int, int updn, string stock)
        {
            var subdirs = Directory.GetDirectories(@"C:\Work\분")
                    .Select(Path.GetFileName).ToList();

            List<string> selected_subdirs = new List<string>();   // changing single list

            foreach (var item in subdirs)
            {
                if (item.Length == 8)
                    selected_subdirs.Add(item);
            }

            string date_string = "";
            int index = 0;
            for (int i = date_int; i > 20200000; i--)
            {
                date_string = i.ToString();
                index = selected_subdirs.IndexOf(date_string);
                if (index != -1)
                    break; ;
            }


            if (updn == 0)
            {
                while (true)
                {
                    if (index == 0)
                        return -1;
                    date_string = selected_subdirs[index];
                    if (File.Exists(Path.Combine(@"C:\Work\분\" + date_string, stock + ".txt")))
                        return Convert.ToInt32(date_string);
                    else
                        index--;
                }
            }

            if (updn == 1)
            {
                while (true)
                {
                    if (index == selected_subdirs.Count - 1)
                        return -1;
                    date_string = selected_subdirs[++index];
                    if (File.Exists(Path.Combine(@"C:\Work\분\" + date_string, stock + ".txt")))
                        return Convert.ToInt32(date_string);
                }
            }

            if (updn == -1)
            {
                while (true)
                {
                    if (index == 0)
                        return -1;
                    date_string = selected_subdirs[--index];
                    if (File.Exists(Path.Combine(@"C:\Work\분\" + date_string, stock + ".txt")))
                        return Convert.ToInt32(date_string);
                }
            }
            return -1;
        }



        public static int directory_분전후(int date_int, int updn)
        {
            var subdirs = Directory.GetDirectories(@"C:\Work\분")
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




        public static string 종목포함_최고가그룹리스트_title(string finding_stock)
        {
            string title = "";
            double 상순 = -3100;

            foreach (var sublist in g.oGL_data)
            {
                if (sublist.stocks.Contains(finding_stock) && 상순 < sublist.상순)
                {
                    상순 = sublist.상순;
                    title = sublist.title;
                }
            }
            return title;
        }

        public static List<string> 종목포함_그룹리스트(string finding_stock)
        {
            List<string> alist = new List<string>();

            foreach (var sublist in g.oGL_data)
            {
                foreach (string item in sublist.stocks)
                {
                    if (item == finding_stock)
                    {
                        foreach (var name in sublist.stocks)
                        {
                            alist.Add(name);
                        }
                    }

                }
            }
            return alist;
        }


        public static int data_컬럼2(string filepath, int dcol, int[,] x, int xcol)
        { // dcol = 원하는 컬럼위치, 
            if (!File.Exists(filepath))
            {
                return -1;
            }

            string[] grlines = File.ReadAllLines(filepath, Encoding.Default);
            int nrow = 0;
            foreach (string line in grlines)
            {

                string[] words = line.Split(' ');
                if (words.Length == 1)
                {
                    words = line.Split('\t');
                }






                if (dcol == 0)
                {
                    // values are crossed, later rearrange ZZZ
                    string[] time = words[dcol].Split(':');


                    if (time.Length == 1)
                    {
                        x[nrow, xcol] = Convert.ToInt32(words[0]);
                    }
                    else
                    {
                        x[nrow, xcol] = Convert.ToInt32(time[0]) * 100 + Convert.ToInt32(time[1]);
                    }
                }

                else
                {
                    x[nrow, xcol] = Convert.ToInt32(words[dcol]);
                }
                nrow++;
                /*
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
				    if (nrow >= 391)
				    break;
				}
				x[nrow++, xcol] = Convert.ToInt32(words[dcol]);
				*/
                if (nrow > 399)
                    break;
            }
            return nrow;
        }




        public static void gen_oGL_data(List<string> oGL_title, List<List<string>> oGL)
        {
            // rearrange oGL and genereate oGl by market cap
            foreach (var t in g.ogl_data) // assign -1 as default
            {
                t.oGL_sequence_id = -1; // needed ?
            }

            int oGL_count = 0;  // oGL_sequence_id will be assigned step by step

            for (int i = 0; i < oGL.Count; i++)
            {
                if (oGL[i].Count < 2)
                    continue;

                var data = new g.group_data();

                var items1 = new List<Tuple<double, string>> { };
                foreach (var stock in oGL[i])
                {
                    int index = g.ogl_data.FindIndex(r => r.종목 == stock);
                    if (index >= 0)
                    {
                        g.ogl_data[index].oGL_sequence_id = oGL_count; // needed ?
                        items1.Add(Tuple.Create(g.ogl_data[index].시총, stock)); // 시총순으로 그룹 내 종목 재배열
                    }
                    else
                    {
                        continue;
                    }
                }
                items1 = items1.OrderByDescending(t => t.Item1).ToList();
                List<string> list = new List<string>();
                foreach (var item in items1)
                {
                    list.Add(item.Item2);
                }
                if (list.Count < 2)
                    continue;

                data.stocks = list.ToList();
                data.title = oGL_title[i];

                g.oGL_data.Add(data);
                oGL_count++;
            }
        }





        internal struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
        internal static extern bool GetWindowRect(IntPtr hWnd, ref RECT rect);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
        internal static extern void MoveWindow(IntPtr hwnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);


        public static void call_네이버(string stock, int selection)
        {
            CPUTILLib.CpStockCode _cd = new CPUTILLib.CpStockCode();

            if (stock == null)
                Process.Start("chrome.exe", "https://finance.naver.com/");
            //Process.Start("microsoft-edge:https://finance.naver.com/");
            else
            {
                string basestring;
                if (selection == 0) // chart
                {
                    basestring = "https://finance.naver.com/item/fchart.nhn?code=";
                    //basestring = "microsoft-edge:https://finance.naver.com/item/fchart.nhn?code=";
                }
                else if (selection == 1) // main
                {
                    basestring = "https://finance.naver.com/item/main.naver?code="; // 투자자별 매매동향
                    //basestring = "microsoft-edge:https://finance.naver.com/item/main.nhn?code="; // 종합정보
                    //basestring = "microsoft-edge:https://finance.naver.com/item/main.nhn?code=";
                }
                else // foreign & institute buying history
                {
                    basestring = "https://finance.naver.com/item/frgn.naver?code=";
                }

                string code = _cd.NameToCode(stock);
                code = new String(code.Where(Char.IsDigit).ToArray());
                basestring += code;
                Process.Start("chrome.exe", basestring);
            }
        }


        public static void call_네이버_차트(string stock, int selection, double xval)
        {
            CPUTILLib.CpStockCode _cd = new CPUTILLib.CpStockCode();

            if (stock == null)
                Process.Start("https://finance.naver.com/");
            //Process.Start("microsoft-edge:https://finance.naver.com/");
            else
            {
                string basestring;
                if (selection == 0)
                {
                    //basestring = "https://finance.naver.com/item/frgn.nhn?code="; // 투자자별 매매동향
                    basestring = "https://finance.naver.com/item/main.nhn?code="; // 종합정보
                                                                                  //basestring = "microsoft-edge:https://finance.naver.com/item/main.nhn?code=";
                }
                else
                {
                    //basestring = "https://finance.naver.com/item/fchart.nhn?code=";
                    basestring = "microsoft-edge:https://finance.naver.com/item/fchart.nhn?code=";
                }

                string code = _cd.NameToCode(stock);
                code = new String(code.Where(Char.IsDigit).ToArray());
                basestring += code;
                Process.Start(basestring);
            }
        }
    }
}
