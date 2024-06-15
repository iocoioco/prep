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

            //double sum_weight = 0.0;
            //string[] str = new string[2];
            for (int i = 0; i < stock_count; i++)
            {
                table_data.weight[i] /= total_amount;
                table_data.weight[i] *= factor;

                //str[0] = table_data.stocks[i];
                //str[1] = table_data.weight[i].ToString();
                //sum_weight += table_data.weight[i];
                //wr.w(str);
            }
            //str[0] = factor.ToString() + " = ";
            //str[1] = sum_weight.ToString();
            //wr.w(str);
            //str[0] = " ";
            //str[1] = " ";
            //wr.w(str);
            return table_data;
        }
    }
}
