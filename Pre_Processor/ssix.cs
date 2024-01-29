using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace Pre_Processor
{
    internal class ssix
    {
        public static void 지수_종목_비중()
        {
            string url = "https://www.samsungfund.com/etf/product/view.do?id=2ETF25";
                
            scrapeTableData(url, "table.mt-24");
        }

        private static void scrapeTableData(string url, string table_name)
        {
            HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();

            doc = web.Load(url);
            List<List<string>> table =
doc.DocumentNode.SelectSingleNode("//table [@class='table']")
    .Descendants("tr")
    .Skip(1)
    .Where(tr => tr.Elements("td").Count() > 1)
    .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
    .ToList();



            for (int i = 0; i < table.Count; i++)
            {
                string s = Encoding.GetEncoding("EUC-KR").GetString(Encoding.GetEncoding("EUC-KR").GetBytes(table[i][0]));
                if (s.Length > 0)
                {
                }
            }

           
        }
    }
}
