using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pre_Processor
{
    internal class indx
    {
        private static CPUTILLib.CpStockCode _cpstockcode = new CPUTILLib.CpStockCode();
        public class mixed_data
        {
            private static CPUTILLib.CpStockCode _cpstockcode;

            //public string[] files_to_open_by_clicking_edge = new string[8];
            public string[] stocks = new string[10];
            public string[] codes = new string[10];
            public int[] numberofStocks = new int[10];
            public double[] weight = new double[10];

        }


        public static void 지수_종목_비중()
        {
            string path = @"C:\병신\data\temp.txt";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            Stream FS = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
            StreamWriter sw = new System.IO.StreamWriter(FS, System.Text.Encoding.Default);

            string[] 지수종목 = new string[] { "KODEX 레버리지", "KODEX 코스닥150레버리지" };

            foreach (var 종목 in 지수종목)
            {
                mixed_data main_table_data = scrapeTableData(종목, 1.0);

                string[] sub_stock = new string[5];
                double[] factors = new double[5];
                int sub_stock_count = 0;

                for (int i = 0; i < main_table_data.stocks.Length; i++)
                {
                    if (main_table_data.stocks[i].Contains("KO") ||
                        main_table_data.stocks[i].Contains("KB"))
                    {
                        sub_stock[sub_stock_count] = main_table_data.stocks[i];
                        factors[sub_stock_count++] = main_table_data.weight[i];
                    }
                }
                foreach (var stock in main_table_data.stocks)
                {

                }

                mixed_data sub_table_data_0 = scrapeTableData(sub_stock[0], factors[0]);
                mixed_data sub_table_data_1 = scrapeTableData(sub_stock[1], factors[1]);

                List<string> stocks = new List<string>();
                double[] weight = new double[15];

                for (int i = 0; i < main_table_data.stocks.Length; i++)
                {
                    if (main_table_data.stocks[i].Contains("KO") ||
                        main_table_data.stocks[i].Contains("KB"))
                        continue;

                    if (!stocks.Contains(main_table_data.stocks[i]))
                    {
                        stocks.Add(main_table_data.stocks[i]);
                    }
                    int index = stocks.IndexOf(main_table_data.stocks[i]);
                    weight[index] += main_table_data.weight[index];
                }

                for (int i = 0; i < sub_table_data_0.stocks.Length; i++)
                {
                    if (!stocks.Contains(sub_table_data_0.stocks[i]))
                    {
                        stocks.Add(sub_table_data_0.stocks[i]);
                    }
                    int index = stocks.IndexOf(sub_table_data_0.stocks[i]);
                    weight[index] += sub_table_data_0.weight[index];
                }

                for (int i = 0; i < sub_table_data_1.stocks.Length; i++)
                {
                    if (!stocks.Contains(sub_table_data_1.stocks[i]))
                    {
                        stocks.Add(sub_table_data_1.stocks[i]);
                    }
                    int index = stocks.IndexOf(sub_table_data_1.stocks[i]);
                    weight[index] += sub_table_data_1.weight[index];
                }

                for (int i = 0; i < stocks.Count; i++)
                {
                    sw.WriteLine("{0}\t{1}", stocks[i], weight[i]);
                }
                sw.WriteLine();
            }
            sw.Close();
        }

        // 종목                                   정확한 table name
        // KODEX 레버리지                   tp_type1 tp_type1_b
        // KODEX 200                         tp_type1 tp_type1_a
        // KODEX 200TR                      tp_type1 tp_type1_a
        //
        // KODEX 코스닥150레버리지      tp_type1 tp_type1_b
        // KODEX 코스닥150                 tp_type1 tp_type1_a
        // KBSTAR 코스닥150                tp_type1 tp_type1_a
        // 
        // wild card : tp_type1 tp_type1

        private static mixed_data scrapeTableData(string 종목, double factor)
        {
            string base_url = "https://finance.naver.com/item/main.naver?code=";
            string code = _cpstockcode.NameToCode(종목);

            code = code.Replace("A", "");
            string url = base_url + code;

            mixed_data table_data = new mixed_data();

            HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();

            doc = web.Load(url);

            List<List<string>> table =
                doc.DocumentNode.SelectSingleNode(".//table[contains(@class, 'tb_type1 tb_type1')]")
                .Descendants("tr")
                .Skip(1)
                .Where(tr => tr.Elements("td").Count() > 1)
                .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
                .ToList();

            int stock_count = 0;
            string s = "";
            double total_amount = 0.0;
            for (int i = 0; i < table.Count; i++)
            {
                s = Encoding.GetEncoding("EUC-KR").GetString(Encoding.GetEncoding("EUC-KR").GetBytes(table[i][0]));
                if (s.Length > 0)
                {
                    table_data.stocks[stock_count] = s;
                    table_data.numberofStocks[stock_count] = int.Parse(table[i][1].Replace(",", ""));

                    int 전일종가 = rd.read_전일종가(table_data.stocks[stock_count]);

                    table_data.weight[stock_count] = table_data.numberofStocks[stock_count] *  전일종가; // replace with 전일종가
                    total_amount += table_data.weight[stock_count];
                    table_data.codes[stock_count] = _cpstockcode.NameToCode(table_data.stocks[stock_count]);
                    stock_count++;
                }
            }

            for (int i = 0; i < stock_count; i++)
            {
                table_data.weight[i] /= total_amount;
                table_data.weight[i] *= factor;

                string[] str = new string[2];

                str[0] = table_data.stocks[i];
                str[1] = table_data.weight[i].ToString();

                wr.w(str);
            }

            return table_data;
        }
    }
}
