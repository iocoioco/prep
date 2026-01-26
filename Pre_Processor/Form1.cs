


using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Linq;
using HtmlAgilityPack;
using static Pre_Processor.g;
//using StockLibrary; 

namespace Pre_Processor
{
    public partial class Form1 : Form
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

        List<List<string>> _mF = new List<List<string>>(); // given stockname, find group
        string[] _variable = new string[10];
        double[] _aV = new double[3000];

        int _workingDay;


        List<string> _gl = new List<string>();       // single stock order can be changed by _qwe        List<List<string>> _sgL = new List<List<string>>(); // given stockname, find group   
        List<string> _code = new List<string>();       // single stock code    
        List<string> _rL = new List<string>();       // stock list registered by me & program
        List<string> _tL = new List<string>();       // stock list bought
        List<string> _aL = new List<string>();       // group correlation 
        List<string> _yL = new List<string>();       // kospi 100 +
        List<string> _uL = new List<string>();       // kosdaq 50 +
        List<string> _wL = new List<string>();       // temporary working space

        List<List<string>> 시총 = new List<List<string>>();
        // var _items = new List<Tuple<int, string>> {1, "this" };


        List<List<string>> _sgL = new List<List<string>>();
        List<List<string>> _agL = new List<List<string>>();

        double[] _screenFactor = new double[2];

        public int[] _time = new int[2] { 0, 381 }; // start and end time

        static CPSYSDIBLib.StockChart _sc = new CPSYSDIBLib.StockChart();

        static CPUTILLib.CpCybos _cc = new CPUTILLib.CpCybos();

        public static List<List<string>> Gl = new List<List<string>>(); // group set list

        public Form1()
        {
            InitializeComponent();
            dataGridView1.Hide();

        }

        // 시작날짜 종료날짜 입력 후 일자별 분 데이터 저장
        //
        //
        private void button2_Click_1(object sender, EventArgs e)
        {
            List<List<string>> Gl = new List<List<string>>(); // 실제 프로그램 내에서 사용하지 않음
            //List<List<string>> GL = new List<List<string>>(); // 실제 프로그램 내에서 사용하지 않음
            List<string> alist = new List<string>();
            //_gl = Library.read_group();
            //_gl = Library.read_시총_일정액수이상(Convert.ToInt32(textBox2.Text));
            //_gl = Library.ReadNaverIndustry(Gl, GL);
            //_gl = Library.read_전종목코드();


            _gl = Library.ReadNaverIndustry(Gl);
            _gl = Library.read_시총_일정액수이상(Convert.ToInt32(textBox2.Text));




            _variable[0] = textBox3.Text; // 일, 주, 월 중 하나 입력
            _variable[1] = textBox4.Text; // 오늘부터 과거로 가면서 최대갯수 

            //_gl = Library.read_분별종목(); // _oL unique list : 옛날 방식의 groupList and singleList
            for (int i = 0; i < _gl.Count; i++)
            {
                _aV[i] = Library.calculate_종목20일기준일평균거래량(_gl[i]);
            }

            Library.read_누적(_mF); // read from file "일중분별누적%" 901 0.017 etc


            // 종목이름 맞는 지 확인
            foreach (string stockname in _gl)
            {
                string code = _cpstockcode.NameToCode(stockname);
                if (code == null)
                {

                    return;
                }
            }

            if (textBox1.Text == "분")
            {
                _Stock_Chart_분.Received += new CPSYSDIBLib._ISysDibEvents_ReceivedEventHandler(_Stock_Chart_분_Received);
                _Stock_Chart_분_stockChart();
            }
            else
            {
                _Stock_Chart_틱.Received += new CPSYSDIBLib._ISysDibEvents_ReceivedEventHandler(_Stock_Chart_틱_Received);
                _Stock_Chart_틱_stockChart();
            }

            textBox6.Text = "All are Processed";
        }

        //틱
        public void _Stock_Chart_틱_stockChart()
        {
            int[] workingdays = new int[1000];

            Library.read_날짜_삼성전자일자료로부터(Convert.ToInt32(_variable[0]),
                        Convert.ToInt32(_variable[1]), "삼성전자", workingdays);
            if (workingdays.Length == 0)
            {
                MessageBox.Show("no working date : 삼성전자 업데이트 필요 ?");
                return;
            }

            for (int iter = 0; iter < workingdays.Length; iter++)
            {
                _workingDay = workingdays[iter];

                if (_workingDay == 0)
                {
                    MessageBox.Show("working date not correct : 삼성전자 업데이트 필요 ?");
                    break;
                }

                for (int i = 0; i < _gl.Count; i++)
                {

                    //Thread.Sleep(1000); // Less than 15 request in 15 minutes
                    textBox6.Text = _gl[i] + "(" + _workingDay.ToString() + ")" + " is Processing";

                    if (_Stock_Chart_틱.GetDibStatus() == 1)
                    {
                        Trace.TraceInformation("DibRq 요청 수신대기 중 입니다. 수신이 완료된 후 다시 호출 하십시오.");
                        continue;
                    }

                    string code = _cpstockcode.NameToCode(_gl[i]);
                    if (code.Length < 7)
                    {
                        continue;
                    }

                    object[] fields = new object[] { 0, 1, 5, 6, 8, 10, 11, 37 };
                    // 0: 날짜, 1: 시간, 2 시가, 3 고가, 5 종가, 6 전일대비, 8 거래량, 10 누적체결매도, 11 누적체결매수
                    // 16 외보, 21 기보

                    _Stock_Chart_틱.SetInputValue(0, code);   //종목코드
                    _Stock_Chart_틱.SetInputValue(1, 2);   //요청코드 '1'(기간), '2'(개수)
                    _Stock_Chart_틱.SetInputValue(2, _workingDay);   //
                    _Stock_Chart_틱.SetInputValue(3, _workingDay);   //
                    _Stock_Chart_틱.SetInputValue(4, 381000);   // 요청이 개수일 때는 의미없음
                    _Stock_Chart_틱.SetInputValue(5, fields);  // 필드 갯수
                    _Stock_Chart_틱.SetInputValue(6, 'T');     // 틱,분,일,주,월 단위 데이터

                    // 출력자료 요청
                    int result = _Stock_Chart_틱.BlockRequest();
                    if (result != 0)
                    {
                        i--; // to try again with the saem stock
                        continue;
                    }
                }
            }
        }

        // 틱 
        private void _Stock_Chart_틱_Received()
        {
            string text1 = _Stock_Chart_틱.GetDibMsg1();
            string text2 = _Stock_Chart_틱.GetDibMsg2();

            int count = (int)_Stock_Chart_틱.GetHeaderValue(1); // 필드갯수

            string path = "";

            path = @"C:\BJS\틱\";
            path = path + _workingDay;

            bool exists = Directory.Exists(path);
            if (!exists)
                Directory.CreateDirectory(path);

            string code = _Stock_Chart_틱.GetHeaderValue(0); // 종목코드
            string stockname = _cm.CodeToName(code);
            path = path + "\\" + stockname + ".txt";

            if (File.Exists(path))
                File.Delete(path);

            StreamWriter sw = File.CreateText(path);
            int numberofData = (int)_Stock_Chart_틱.GetHeaderValue(3); // 수신갯수
            if (numberofData == 0)
                return;

            int workingdate = (int)_Stock_Chart_틱.GetDataValue(0, 0); //일자

            object[] fields = new object[] { 0, 1, 5, 6, 8, 10, 11, 37 };
            // 0: 날짜, 1: 시간, 2 시가, 3 고가, 5 종가, 6 전일대비, 8 거래량, 10 누적체결매도, 11 누적체결매수
            // 16 외보, 21 기보, 37 대비부호

            for (int k = numberofData - 1; k >= 0; k--)
            {
                int day = (int)_Stock_Chart_틱.GetDataValue(0, k); //날짜
                int HHmm = (int)_Stock_Chart_틱.GetDataValue(1, k); //분, 시간
                int currentprice = (int)_Stock_Chart_틱.GetDataValue(2, k); //종가
                long 대비 = _Stock_Chart_틱.GetDataValue(3, k); //대비
                int amount = (int)_Stock_Chart_틱.GetDataValue(4, k); //단위거래량
                int buy = (int)_Stock_Chart_틱.GetDataValue(5, k); // 체결매도
                int sell = (int)_Stock_Chart_틱.GetDataValue(6, k);// 체결매수
                char 대비부호 = _Stock_Chart_틱.GetDataValue(7, k);// 대비부호

                //sw.WriteLine("{0} {1} {2} {3} {4} {5}", day, HHmm, currentprice, amount, buy, sell); // XXX 틱 전체 수정 필요
            }
            sw.Close();
        }

        public void _Stock_Chart_분_stockChart()
        {
            int[] workingdays = new int[1000];

            Library.read_날짜_삼성전자일자료로부터(Convert.ToInt32(_variable[0]),
                        Convert.ToInt32(_variable[1]), "삼성전자", workingdays);
            if (workingdays.Length == 0)
            {
                MessageBox.Show("no working date : 삼성전자 업데이트 필요 ?");
                return;
            }

            for (int iter = 0; iter < workingdays.Length; iter++)
            {
                _workingDay = workingdays[iter];

                if (_workingDay < 2000000)
                {
                    break;
                }


                for (int i = 0; i < _gl.Count; i++)
                {
                    string path = "";

                    path = @"C:\BJS\분\";
                    path = path + _workingDay;

                    bool exists = System.IO.Directory.Exists(path);
                    if (!exists)
                        System.IO.Directory.CreateDirectory(path);


                    path = path + "\\" + _gl[i] + ".txt";



                    Stream FS = new FileStream(path, FileMode.Append, FileAccess.Write);
                    StreamWriter sw = new System.IO.StreamWriter(FS, System.Text.Encoding.Default);
                    long filesize = FS.Length / 1024;
                    sw.Close();
                    if (filesize <= 1)
                    {
                        File.Delete(path);
                    }
                    if (File.Exists(path))
                    {
                        continue;
                    }


                    //Thread.Sleep(1000); // Less than 15 request in 15 minutes
                    textBox6.Text = _gl[i] + "(" + _workingDay.ToString() + ")" + " is Processing";

                    if (_Stock_Chart_분.GetDibStatus() == 1)
                    {
                        Trace.TraceInformation("DibRq 요청 수신대기 중 입니다. 수신이 완료된 후 다시 호출 하십시오.");
                        continue;
                    }


                    string code = _cpstockcode.NameToCode(_gl[i]);
                    if (code.Length < 7)
                    {
                        continue;
                    }

                    object[] fields = new object[6] { 0, 1, 5, 8, 10, 11 };
                    // 0: 날짜(0), 1: 시간(1), 5:종가(2), 8: 거래량(3), 10 : 누적체결매도(4), 11: 누적체결매수(5)

                    _Stock_Chart_분.SetInputValue(0, code);   //종목코드
                    _Stock_Chart_분.SetInputValue(1, 1);   //요청코드 '1'(기간), '2'(개수)
                    _Stock_Chart_분.SetInputValue(2, _workingDay);   //
                    _Stock_Chart_분.SetInputValue(3, _workingDay);   //
                    _Stock_Chart_분.SetInputValue(4, 381);   // 요청이 개수일 때는 의미없음
                    _Stock_Chart_분.SetInputValue(5, fields);  // 필드 갯수
                    _Stock_Chart_분.SetInputValue(6, 'm');     // 틱,분,일,주,월 단위 데이터

                    // 출력자료 요청
                    int result = _Stock_Chart_분.BlockRequest();
                    if (result != 0)
                    {
                        i--; // to try again with the saem stock
                        continue;
                    }
                }
            }
        }

        private void _Stock_Chart_분_Received()
        {
            string text1 = _Stock_Chart_분.GetDibMsg1();
            string text2 = _Stock_Chart_분.GetDibMsg2();


            string path = "";

            path = @"C:\BJS\분\";
            path = path + _workingDay;

            bool exists = System.IO.Directory.Exists(path);
            if (!exists)
                System.IO.Directory.CreateDirectory(path);

            string code = _Stock_Chart_분.GetHeaderValue(0);
            string stockname = _cm.CodeToName(code);
            path = path + "\\" + stockname + ".txt";


            if (File.Exists(path))
            {
                return;
            }

            StreamWriter sw = File.CreateText(path);
            int 전일종가 = Library.read_일자제시_전일종가(_workingDay, stockname);
            if (전일종가 < 0)
            {
                return;
            }

            int numberofData = (int)_Stock_Chart_분.GetHeaderValue(3);
            if (numberofData == 0)
                return;

            //object[] fields = new object[6] {0, 1, 5, 8, 10, 11};
            // 0: 날짜(0), 1: 시간(1), 5:종가(2), 8: 거래량(3), 10 : 누적체결매도(4), 11: 누적체결매수(5)

            var items = new List<Tuple<int, int, int, int, int>> { };

            for (int k = numberofData - 1; k >= 0; k--)
            {
                int 시간 = (int)_Stock_Chart_분.GetDataValue(1, k); //1H
                int 분당종가 = (int)_Stock_Chart_분.GetDataValue(2, k); //5
                int 전일대비 = (int)((분당종가 - 전일종가) / (double)전일종가 * 10000.0);
                int 분당거래량 = (int)_Stock_Chart_분.GetDataValue(3, k); //8

                int 누적체결매도 = (int)_Stock_Chart_분.GetDataValue(4, k); // 누적체결매도      
                int 누적체결매수 = (int)_Stock_Chart_분.GetDataValue(5, k); // 누적체결매수

                if (Convert.ToInt32(시간) > 1520)
                    continue;

                items.Add(Tuple.Create(시간, 전일대비, 분당거래량, 누적체결매수, 누적체결매도));

            }
            items = items.OrderBy(t => t.Item1).ToList(); // 시간순 재정렬

            int[,] x = new int[400, 5];                 // 시간 중복 : 뒷 시간의 데이터 사용, 앞 시간들은 삭제
            int 분전시간 = 0;
            int curr_id = 0;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Item1 != 분전시간)
                {
                    x[curr_id, 0] = items[i].Item1;
                    x[curr_id, 1] = items[i].Item2;
                    x[curr_id, 2] = items[i].Item3;
                    x[curr_id, 3] = items[i].Item4;
                    x[curr_id, 4] = items[i].Item5;

                    분전시간 = x[curr_id, 0];
                    curr_id++;

                    continue;
                }
            }

            // 계산 후 기록
            int 누적거래량 = 0;
            int 분전누적체결매수 = 0;
            int 분전누적체결매도 = 0;
            for (int i = 0; i < 400; i++)
            {
                if (x[i, 0] == 0)
                {
                    break;
                }
                int 분당거래량 = x[i, 2];
                int 누적체결매수 = x[i, 3];
                int 누적체결매도 = x[i, 4];

                List<string> alist = new List<string>();
                alist = Library.find_리스트(x[i, 0].ToString(), _mF);
                double minuteaverage = Convert.ToDouble(alist[1]);
                string code1 = _Stock_Chart_분.GetHeaderValue(0);
                string name = _cm.CodeToName(code1);
                int nthfile = Library.find_순서(name, _gl);

                누적거래량 = 누적체결매수 + 누적체결매도;
                int 수급 = (int)(누적거래량 / (minuteaverage * _aV[nthfile]) * 100.0); //_aV : 일평균거래량

                int 체강 = 0;
                if (누적체결매도 != 0)
                {
                    체강 = (int)(누적체결매수 / (double)누적체결매도 * 100.0 * 100.0);
                }
                int 매수배수 = (int)((누적체결매수 - 분전누적체결매수) / (double)_aV[nthfile] * 381.0 * 10.0);
                int 매도배수 = (int)((누적체결매도 - 분전누적체결매도) / (double)_aV[nthfile] * 381.0 * 10.0);

                int zero = 0;
                sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}",
                            x[i, 0] * 100,             // 시간     
                            x[i, 1],             // 가격 : 전일대비 * 100
                            수급,
                            체강,            // 체강   
                            zero,
                            zero,
                            zero,
                            누적거래량,
                            매수배수,
                            매도배수);

                분전누적체결매수 = 누적체결매수;
                분전누적체결매도 = 누적체결매도;
            }

            sw.Close();

        }

        public void 일주월()
        {

            var groupList = new List<List<string>>();
            var totalStocksToProcede = new List<string>();


            totalStocksToProcede = Library.ReadNaverIndustry(groupList);
            //var themeStocks = Library.read_그룹_네이버_테마();
            //Library.AddIfMissing(themeStocks, totalStocksToProcede);

            textBox6.Text = "일 진행 중";


            if (!totalStocksToProcede.Contains("KODEX 레버리지"))
                totalStocksToProcede.Add("KODEX 레버리지"); // 전체적으로 지수의 흐름을 보고 단타를 치기위함
            if (!totalStocksToProcede.Contains("KODEX 200선물인버스2X"))
                totalStocksToProcede.Add("KODEX 200선물인버스2X");
            if (!totalStocksToProcede.Contains("KODEX 코스닥150레버리지"))
                totalStocksToProcede.Add("KODEX 코스닥150레버리지");
            if (!totalStocksToProcede.Contains("KODEX 코스닥150선물인버스"))
                totalStocksToProcede.Add("KODEX 코스닥150선물인버스");

            _Stock_Chart_일주월.Received += new CPSYSDIBLib._ISysDibEvents_ReceivedEventHandler(_Stock_Chart_일주월_Received);

            _variable[0] = "일"; // 일, 주, 월 중 하나 입력
            _variable[1] = "700"; // 오늘부터 과거로 가면서 최대갯수 
            _Stock_Chart_일주월_stockChart(totalStocksToProcede);




            textBox6.Text = "일 done";
        }
        // 일, 주, 월 & 갯수 입력 후 10개 자료 저장
        //
        //
        private void button1_Click_1(object sender, EventArgs e)
        {
            일주월();

            // to delete all last line containing words[5, 6, 7] = "0"
            _Stock_Chart_일주월_stockChart_post();

        }

        public void _Stock_Chart_일주월_stockChart_post()
        {
            string path = @"C:\BJS\data\일";
            if (!Directory.Exists(path))
            {
                return;
            }
            var sl = Directory.GetFiles(path, "*.txt")
                     .Select(Path.GetFileName)
                     .ToList();

            int countoflastlinezeros = 0;
            foreach (var file in sl)
            {
                // if file not txt type
                if (!file.Contains(".txt"))
                { continue; }

                // if not stock txt file
                string stock = file.Replace(".txt", "");
                if (!ms.is_stock(stock))
                { continue; }

                string filepath = path + "\\" + file;
                string lastline = File.ReadLines(filepath).Last(); // last line read 

                string[] words = lastline.Split(' ');
                if (words[5] == "0" && words[6] == "0" && words[7] == "0")
                    countoflastlinezeros++;
            }

            if (countoflastlinezeros > 1000)
            {
                foreach (var file in sl)
                {
                    // if file not txt type
                    if (!file.Contains(".txt"))
                    { continue; }

                    // if not stock txt file
                    string stock = file.Replace(".txt", "");
                    if (!ms.is_stock(stock))
                    { continue; }

                    string filepath = path + "\\" + file;

                    string lastline = File.ReadLines(filepath).Last(); // last line read 

                    string[] words = lastline.Split(' ');
                    if (words[5] == "0" && words[6] == "0" && words[7] == "0")
                    {
                        List<string> lines = File.ReadAllLines(filepath).ToList();
                        if (File.Exists(filepath))
                        {
                            File.Delete(filepath);
                        }

                        File.WriteAllLines(filepath, lines.GetRange(0, lines.Count - 1).ToArray());
                    }

                }
            }

        }

        public void _Stock_Chart_일주월_stockChart(List<string> totalStocksToProcede)
        {


            for (int i = 0; i < totalStocksToProcede.Count; i++)
            {

                string code = _cpstockcode.NameToCode(totalStocksToProcede[i]);
                if (code.Length < 7)
                {
                    continue;
                }

                // 0 : 0 날짜
                // 1 : 2 시가
                // 2 : 3 고가
                // 3 : 4 저가
                // 4 : 5 종가
                // 5 : 8 거래량
                // 6 : 10 누적체결매도수량
                // 7 : 11 누적체결매수수량
                // 8 : 16 외국인현보유수량
                // 9 : 21 기관누적순매수
                object[] fields = new object[10] { 0, 2, 3, 4, 5, 8, 10, 11, 16, 21 };

                _Stock_Chart_일주월.SetInputValue(0, code);   //종목코드
                _Stock_Chart_일주월.SetInputValue(1, '2');   //요청코드 '1'(기간), '2'(개수), 필히 문자, 숫자입력해서 고생
                //_Stock_Chart_일주월.SetInputValue(2, 20190614);   // 종료일
                //_Stock_Chart_일주월.SetInputValue(3, 20161025);   // 시작일
                int numberofdata = Convert.ToInt32(_variable[1]);
                _Stock_Chart_일주월.SetInputValue(4, numberofdata);   // 개수
                _Stock_Chart_일주월.SetInputValue(5, fields);  // 필드 갯수


                switch (_variable[0])
                {
                    case "일":
                        _Stock_Chart_일주월.SetInputValue(6, 'D');     // 틱,분,일,주,월 단위 데이터
                        break;
                    case "주":
                        _Stock_Chart_일주월.SetInputValue(6, 'W');     // 틱,분,일,주,월 단위 데이터
                        break;
                    case "월":
                        _Stock_Chart_일주월.SetInputValue(6, 'M');     // 틱,분,일,주,월 단위 데이터
                        break;
                    default:
                        break;
                }


                if (_Stock_Chart_일주월.GetDibStatus() == 1)
                {
                    Trace.TraceInformation("DibRq 요청 수신대기 중 입니다. 수신이 완료된 후 다시 호출 하십시오.");
                    continue;
                }
                // 출력자료 요청
                if (_Stock_Chart_일주월.GetDibStatus() == 0)
                {
                    int result = _Stock_Chart_일주월.BlockRequest();
                    if (result != 0)
                    {
                        i--; // to try again with the same stock
                        continue;
                    }
                }
            }


        }

        private void _Stock_Chart_일주월_Received()
        {
            string text1 = _Stock_Chart_일주월.GetDibMsg1();
            string text2 = _Stock_Chart_일주월.GetDibMsg2();

            int count = (int)_Stock_Chart_일주월.GetHeaderValue(1);

            string path = "";

            switch (_variable[0])
            {
                case "일":
                    path = @"C:\BJS\data\일\";
                    break;
                case "주":
                    path = @"C:\BJS\data\주\";
                    break;
                case "월":
                    path = @"C:\BJS\data\월\";
                    break;
                default:
                    break;
            }

            string stock = _cm.CodeToName(_Stock_Chart_일주월.GetHeaderValue(0));
            path = path + stock + ".txt";

            if (File.Exists(path))
                File.Delete(path);


            StreamWriter sw = File.CreateText(path);

            int numberofData = (int)_Stock_Chart_일주월.GetHeaderValue(3);


            for (int k = numberofData - 1; k >= 0; k--)
            {
                sw.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}",
                    _Stock_Chart_일주월.GetDataValue(0, k),
                    _Stock_Chart_일주월.GetDataValue(1, k),
                    _Stock_Chart_일주월.GetDataValue(2, k),
                    _Stock_Chart_일주월.GetDataValue(3, k),
                    _Stock_Chart_일주월.GetDataValue(4, k),
                    _Stock_Chart_일주월.GetDataValue(5, k),
                    _Stock_Chart_일주월.GetDataValue(6, k),
                    _Stock_Chart_일주월.GetDataValue(7, k),
                    _Stock_Chart_일주월.GetDataValue(8, k),
                    _Stock_Chart_일주월.GetDataValue(9, k));
                // 날짜, 시가, 고가, 저가, 종가, 거래량, 누적매도, 누적매수, 외국인보유, 기관누적순매수
            }
            sw.Close();
        }

        public void 시가총액()
        {
            textBox6.Text = "시총 진행 중";




            _Stock_Chart_시총.Received += new CPSYSDIBLib._ISysDibEvents_ReceivedEventHandler(_Stock_Chart_시총_Received);

            _Stock_Chart_시총_stockChart();
            textBox6.Text = "시가총액 done";
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            시가총액();
        }

        public void _Stock_Chart_시총_stockChart()
        {
            string file = @"C:\BJS\data\시총.txt";

            if (File.Exists(file))
                File.Delete(file);

            StreamWriter sw = File.CreateText(file);
            sw.Close();

            var selected1000Stocks = Library.SelectTop1000Stocks();

            var LeverageList = new List<string> { "KODEX 레버리지", "KODEX 코스닥150레버리지" };
            var InverseList = new List<string> { "KODEX 200선물인버스2X", "KODEX 코스닥150선물인버스" };
            var IndexList = LeverageList.Concat(InverseList).ToList();
            selected1000Stocks.AddRange(IndexList);

            for (int i = 0; i < selected1000Stocks.Count; i++)
            {
                //Thread.Sleep(1000); // Less than 15 request in 15 minutes



                if (_Stock_Chart_시총.GetDibStatus() == 1)
                {
                    Trace.TraceInformation("DibRq 요청 수신대기 중 입니다. 수신이 완료된 후 다시 호출 하십시오.");
                    continue;
                }

                string code = _cpstockcode.NameToCode(selected1000Stocks[i]);
                if (code.Length < 7)
                {
                    continue;
                }

                object[] fields = new object[1] { 13 };

                _Stock_Chart_시총.SetInputValue(0, code);   //종목코드
                _Stock_Chart_시총.SetInputValue(1, 2);   //요청코드 '1'(기간), '2'(개수)
                _Stock_Chart_시총.SetInputValue(5, fields);   // 필드 갯수
                _Stock_Chart_시총.SetInputValue(6, 'D');   // 틱,분,일,주,월 단위 데이터

                // 출력자료 요청
                int result = _Stock_Chart_시총.BlockRequest();
                if (result != 0)
                {
                    i--; // to try again with the same stock
                    continue;
                }
                textBox6.Text = "시가총액 Processing : " + i.ToString();
            }
        }

        private void _Stock_Chart_시총_Received()
        {
            string text1 = _Stock_Chart_시총.GetDibMsg1();
            string text2 = _Stock_Chart_시총.GetDibMsg2();

            int numberofData = (int)_Stock_Chart_시총.GetHeaderValue(3);
            if (numberofData == 0)
                return;

            string code = _Stock_Chart_시총.GetHeaderValue(0);
            string stockname = _cm.CodeToName(code);

            ulong 시총 = _Stock_Chart_시총.GetDataValue(0, 0);

            string stock_name = stockname.Replace(" ", "_");

            using (var fs = new FileStream(@"C:\BJS\data\시총.txt", FileMode.Append, FileAccess.Write, FileShare.Read))
            using (var sw = new StreamWriter(fs, System.Text.Encoding.Default))
            {
                // 단위 : 억원 (시총/1e8)
                sw.WriteLine("{0} {1}", stock_name, 시총 / 100000000UL);
            }
        }

         
        private void button4_Click_1(object sender, EventArgs e)
        {
            상관계산();
        }

        private void 상관계산()
        {
            var selected1000Stocks = Library.SelectTop1000Stocks();

            textBox6.Text = selected1000Stocks.Count.ToString() + " 상관(Pearson Correlation) 진행 중";

            int days_of_array = 50;
            int print_length = 20;
            //Library.PearsonRateDifferenceBetweenDays(days_of_array, print_length, _gl);
            Library.SpearmanRankCorrelationBetweenDays(days_of_array, print_length, selected1000Stocks);
            textBox6.Text = "상관(Pearson Correlation) done & All done";
        }

        //	투자주체별현황을 일별/기간별, 순매수/매매비중을 일자별
        private void button5_Click(object sender, EventArgs e)
        {
            // List<List<string>> Gl = new List<List<string>>(); // 실제 프로그램 내에서 사용하지 않음
            List<List<string>> GL = new List<List<string>>(); // 실제 프로그램 내에서 사용하지 않음
            List<string> alist = new List<string>();
            //_gl = Library.read_그룹4(Gl);
            //_gl = Library.read_시총_일정액수이상(Convert.ToInt32(textBox2.Text));
            _gl = Library.ReadNaverIndustry(GL);
            string path = @"C:\BJS\그룹_" + textBox2.Text + ".txt";
            if (File.Exists(path))
                File.Delete(path);


            /*
            Stream FS = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
            StreamWriter sw = new System.IO.StreamWriter(FS, System.Text.Encoding.Default);

            for (int k = 0; k < _gl.Count; k++) // k : 일자별 
            {
                if (k % 5 == 0 && k != 0)
                    sw.WriteLine();
                string newname = _gl[k].Replace(" ", "_");
                sw.Write("{0} ", newname);

            }
            sw.Close();
            */



            //	투자주체별현황을 일별/기간별, 순매수/매매비중을 일자별
            for (int i = 0; i < _gl.Count; i++)
            {
                _cpsvr7254 = new CPSYSDIBLib.CpSvr7254();
                _cpsvr7254.Received += new CPSYSDIBLib._ISysDibEvents_ReceivedEventHandler(_cpsvr7254_Received);

                int startday = Convert.ToInt32(textBox3.Text);
                int endday = Convert.ToInt32(textBox4.Text);

                _cpsvr7254.SetInputValue(0, _cpstockcode.NameToCode(_gl[i]));
                _cpsvr7254.SetInputValue(1, 4); //0:사용자지정 1:1개월, 2:2개월 3:3개월 4:6개월,5:최근5일 6:일별
                _cpsvr7254.SetInputValue(2, startday); // 0:사용자지정일 아닐 경우 시작일
                _cpsvr7254.SetInputValue(3, endday); // 0:사용자지정일 아닐 경우 종료일
                _cpsvr7254.SetInputValue(4, '1'); //'0' 순매수, '1' 매매비중
                _cpsvr7254.SetInputValue(5, 0);// 0 전체, 1 개인, 2 외국인 ...

                // 출력자료 요청

                int result = _cpsvr7254.BlockRequest();
                if (result != 0)
                {
                    i--; // to try again with the saem stock
                    continue;
                }

                textBox6.Text = _gl[i] + " is Processing";
            }
            textBox6.Text = "All are Processed";
        }

        private void _cpsvr7254_Received()
        {
            string text1 = _cpsvr7254.GetDibMsg1();
            string text2 = _cpsvr7254.GetDibMsg2();

            string code = _cpsvr7254.GetHeaderValue(0);
            string path = @"C:\BJS\매\" + _cm.CodeToName(code) + ".txt";

            if (File.Exists(path))
                File.Delete(path);
            StreamWriter sw = File.CreateText(path);

            int count = (int)_cpsvr7254.GetHeaderValue(1); // 데이터 숫자
            for (int k = 0; k < count - 1; k++) // k : 일자별 
            {
                int 일자 = (int)_cpsvr7254.GetDataValue(0, k); // 0 일자, 1 개인, 2 외인, 3 기관계 ...
                int 개인 = (int)_cpsvr7254.GetDataValue(1, k) - (int)_cpsvr7254.GetDataValue(1, k + 1);
                int 외인 = (int)_cpsvr7254.GetDataValue(2, k) - (int)_cpsvr7254.GetDataValue(2, k + 1);
                int 기관계 = (int)_cpsvr7254.GetDataValue(3, k) - (int)_cpsvr7254.GetDataValue(3, k + 1);
                int 금투 = (int)_cpsvr7254.GetDataValue(4, k) - (int)_cpsvr7254.GetDataValue(4, k + 1);
                int 투신 = (int)_cpsvr7254.GetDataValue(6, k) - (int)_cpsvr7254.GetDataValue(6, k + 1);
                int 연금 = (int)_cpsvr7254.GetDataValue(9, k) - (int)_cpsvr7254.GetDataValue(9, k + 1);
                sw.WriteLine("{0} {1} {2} {3} {4} {5} {6}", 일자, 개인, 외인, 기관계, 금투, 투신, 연금);
            }
            sw.Close();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            var list = Library.read_시총_일정액수이상(Convert.ToInt32(textBox2.Text));

            string path = @"C:\BJS\" + "그룹" + textBox2.Text + ".txt";

            Stream FS = new FileStream(path, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new System.IO.StreamWriter(FS, System.Text.Encoding.Default);


            int count = 0;
            foreach (string stockname in list)
            {

                if (count % 4 == 0 && count != 0)
                {
                    sw.WriteLine();
                    count = 0;
                }

                sw.Write("{0} ", stockname);
                count++;
                //string temp_stockname = stockname.Replace(" ", "_");
            }

            sw.Close();

        }

        public void WriteNaverIndustry() // 1st
        {
            textBox6.Text = "네이버업종 진행 중";

            var doc = new HtmlAgilityPack.HtmlDocument();
            HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();

            string file = @"C:\BJS\data\" + "그룹_네이버_업종" + ".txt";
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            Stream FS = new FileStream(@"C:\BJS\data\" + "그룹_네이버_업종" + ".txt", FileMode.CreateNew, FileAccess.Write);
            StreamWriter sw = new System.IO.StreamWriter(FS, System.Text.Encoding.Default);

            int[] themeRemove = new int[] { 25 }; // 환율하락수혜
            //int[] themeRemove = new int [] {31, // 환율하락수혜
            //    38, // 홈쇼핑
            //    66, // 줄기세포
            //    76, // 여름
            //    82, // 태풍 및 장마
            //    111, // 지주사
            //    //126, // 통신
            //    //165, // 은행
            //    266, // 겨울
            //    268, // 전기자전거
            //    285, // 마스크
            //    290, // 스포츠행사
            //    312, // 소모성자재구매대행
            //    313, // 남북러가스관
            //    328, // 제습기
            //    335, // 재난/안전
            //    341, // 밥솥
            //    398, // DMZ 평화공원
            //    414, // 2019 상반기
            //    420, // 2019 하반기
            //    421, // 일본불매
            //    422, // 일본불매 국산화
            //    432, // 2020 상반기
            //    440, // 2020 하반기
            //    456, 457, 458, 459, 460, // K 뉴딜지수
            //    473, // 2021 상반기
            //    485, // 백신여권
            //    499, // 2021 하반기
            //};
            int count = 0;
            int theme_count = 0;
            for (int i = 0; i < 1000; i++)
            {
                //if (themeRemove.Contains(i))
                //    continue;

                string url = i.ToString();

                var newurl = "https://finance.naver.com/sise/sise_group_detail.nhn?type=upjong&no=" + url;


                // if newurl is invalid, skip
                HttpWebResponse response = null;
                var request = (HttpWebRequest)WebRequest.Create(newurl);
                request.Method = "HEAD";
                response = (HttpWebResponse)request.GetResponse();
                if (response.CharacterSet == "MS949")
                {
                    response.Close();
                    continue;
                }
                response.Close();

                doc = web.Load(newurl);
                if (doc == null)
                {
                    continue;
                }


                // Title Extracted
                var t = doc.DocumentNode.SelectSingleNode("//title").InnerText;
                if (t.Contains("기타 :"))
                    continue;

                int index = t.IndexOf(':');
                string title = t.Remove(index - 1);
                sw.WriteLine("{0} {1}", title, i);

                // Stocks Extracted
                var nodes = doc.DocumentNode.SelectNodes("//div [@class='name_area']");
                foreach (var item in nodes)
                {
                    var stock = item.InnerText.ToString();
                    string stock_trimmed = stock.TrimEnd('*', ' ');
                    sw.WriteLine(stock_trimmed);
                    count++;
                }
                sw.WriteLine();
                theme_count++;


            }
            sw.Close();
            textBox6.Text = "네이버업종 done";
        }

        private void button9_Click(object sender, EventArgs e)
        {
            WriteNaverIndustry();
            return;

            textBox6.Text = "네이버 업종 정리 Processing";

            var doc = new HtmlAgilityPack.HtmlDocument();
            HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();

            string file = @"C:\BJS\data\" + "그룹_네이버_업종" + ".txt";
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            Stream FS = new FileStream(@"C:\BJS\data\" + "그룹_네이버_업종" + ".txt", FileMode.CreateNew, FileAccess.Write);
            StreamWriter sw = new System.IO.StreamWriter(FS, System.Text.Encoding.Default);

            var lines = File.ReadAllLines(@"C:\BJS\data\" + "업종_url" + ".txt");

            int count = 0;
            int upjong_count = 0;
            foreach (var url in lines)
            {
                if (url == "")
                    continue;

                var newurl = "https://finance.naver.com/sise/sise_group_detail.nhn?type=upjong&no=" + url;
                doc = web.Load(newurl);
                if (doc == null)
                {
                    return;
                }
                var nodes = doc.DocumentNode.SelectNodes("//div [@class='name_area']");

                foreach (var item in nodes)
                {
                    var stock = item.InnerText.ToString();
                    string stock_trimmed = stock.TrimEnd('*', ' ');
                    sw.WriteLine(stock_trimmed);
                    count++;
                }
                sw.WriteLine();
                upjong_count++;
            }
            sw.Close();

            textBox6.Text = upjong_count.ToString() + "/" + count.ToString() + " are Processed";


            //File.WriteAllText(@"C:\BJS\" + "temp" + ".txt", all_stock);

            //var table = doc.DocumentNode.SelectSingleNode("//table"); // ok

            //foreach (var cell in table.SelectNodes("//tr//td/a[@href]"))
            //{
            //    var t = cell.InnerText;
            //    var s = cell.InnerHtml;
            //    var href = cell.Attributes["//a[@href"].Value;
            //}
            //    //{
            //    //    var s = col.InnerText;
            //    //}



            //var allelement = doc.DocumentNode.SelectNodes("//a[@href]"); // ok
            //foreach (var element in allelement)
            //{
            //    var href = element.Attributes["href"].Value;

            //}



            //var nasdaq_future = doc.DocumentNode.SelectSingleNode("//div [@class='name_area']");
            //var nasdaq_future_percentage = nasdaq_future.InnerHtml.ToString();

            //nasdaq_future = doc.DocumentNode.SelectSingleNode("//div [@class='name_area']");
            //nasdaq_future_percentage = nasdaq_future.InnerHtml.ToString();

            //nasdaq_future = doc.DocumentNode.SelectSingleNode("//div [@class='name_area']");
            //nasdaq_future_percentage = nasdaq_future.InnerHtml.ToString();

            //var allelement = doc.DocumentNode.SelectNodes("//div [@class='name_area']");
            //foreach (var element in allelement)
            //{
            //    var stock = element.InnerText.ToString();
            //}
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public void 네이버_테마(int version)
        {
            textBox6.Text = "네이버테마 진행 중";



            var doc = new HtmlAgilityPack.HtmlDocument();
            HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();

            string file_0 = @"C:\BJS\data\" + "그룹_네이버_테마" + ".txt";
            if (File.Exists(file_0))
            {
                File.Delete(file_0);
            }
            Stream FS = new FileStream(file_0, FileMode.CreateNew, FileAccess.Write);
            StreamWriter sw = new System.IO.StreamWriter(FS, System.Text.Encoding.Default);




            string file_1 = @"C:\BJS\data\" + "그룹_네이버_테마_번호" + ".txt";
            if (File.Exists(file_1))
            {
                File.Delete(file_1);
            }
            Stream FY = new FileStream(file_1, FileMode.CreateNew, FileAccess.Write);
            StreamWriter sy = new System.IO.StreamWriter(FY, System.Text.Encoding.Default);

            int[] themeRemove = new int[1];
            //int[] themeRemove = new int [] {31, // 환율하락수혜
            //    38, // 홈쇼핑
            //    66, // 줄기세포
            //    76, // 여름
            //    82, // 태풍 및 장마
            //    111, // 지주사
            //    //126, // 통신
            //    //165, // 은행
            //    266, // 겨울
            //    268, // 전기자전거
            //    285, // 마스크
            //    290, // 스포츠행사
            //    312, // 소모성자재구매대행
            //    313, // 남북러가스관
            //    328, // 제습기
            //    335, // 재난/안전
            //    341, // 밥솥
            //    398, // DMZ 평화공원
            //    414, // 2019 상반기
            //    420, // 2019 하반기
            //    421, // 일본불매
            //    422, // 일본불매 국산화
            //    432, // 2020 상반기
            //    440, // 2020 하반기
            //    456, 457, 458, 459, 460, // K 뉴딜지수
            //    473, // 2021 상반기
            //    485, // 백신여권
            //    499, // 2021 하반기
            //};
            int count = 0;
            int theme_count = 0;
            for (int i = 0; i < 1000; i++)
            {
                if (themeRemove.Contains(i))
                    continue;

                string url = i.ToString();

                var newurl = "https://finance.naver.com/sise/sise_group_detail.nhn?type=theme&no=" + url;

                // if newurl is invalid, skip
                HttpWebResponse response = null;
                var request = (HttpWebRequest)WebRequest.Create(newurl);
                request.Method = "HEAD";
                response = (HttpWebResponse)request.GetResponse();
                if (response.CharacterSet == "MS949")
                {
                    response.Close();
                    continue;
                }
                response.Close();

                doc = web.Load(newurl);
                if (doc == null)
                {
                    continue;
                }


                // Title Extracted
                var t = doc.DocumentNode.SelectSingleNode("//title").InnerText;
                int index = t.IndexOf(':');
                string title = t.Remove(index - 1);
                sw.WriteLine("{0}", title);


                sy.WriteLine("{0}\t{1}", title, i);

                // Stocks Extracted
                var nodes = doc.DocumentNode.SelectNodes("//div [@class='name_area']");
                foreach (var item in nodes)
                {
                    var stock = item.InnerText.ToString();
                    string stock_trimmed = stock.TrimEnd('*', ' ');
                    sw.WriteLine(stock_trimmed);
                    count++;
                }
                sw.WriteLine();
                theme_count++;

            }
            sw.Close();
            sy.Close();
            textBox6.Text = "네이버테마 done";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            네이버_테마(1);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            string originalDirectory = @"C:\BJS\data work";
            string copyDirectory = @"C:\BJS\data";
            DirectoryHandler.DeleteAndCopyDirectory(originalDirectory, copyDirectory);

            copyDirectory = @"C:\BJS\data copy";
            DirectoryHandler.DeleteAndCopyDirectory(originalDirectory, copyDirectory);

            WriteNaverIndustry(); // 1 분
            //네이버_테마(1); // 1 분
            일주월();
            시가총액();
            통계();
            통계_지수();
            상관계산(); // 12 분 

            originalDirectory = @"C:\BJS\data";
            copyDirectory = @"C:\BJS\data work";
            DirectoryHandler.DeleteAndCopyDirectory(originalDirectory, copyDirectory);
        }

        private void 프돈외기()
        {
            DateTime date = DateTime.Now.Date;

            if (date.DayOfWeek == DayOfWeek.Sunday)
            {
                date = DateTime.Now.Date.AddDays(-2);
            }
            else if (date.DayOfWeek == DayOfWeek.Saturday)
            {
                date = DateTime.Now.Date.AddDays(-1);
            }
            int reference_date = Convert.ToInt32(date.ToString("yyyyMMdd"));
            int HHmm = Convert.ToInt32(date.ToString("HHmm"));
            if (HHmm <= 1530)
                reference_date--; // 당일 데이터가 저장되어 있지 않아 전일 기준 Processing

            string[] grlines = File.ReadAllLines(@"C:\BJS\data\시총.txt", Encoding.Default);

            // 저장파일 "프돈외기" 있으면 삭제 후 재생성
            string path = @"C:\BJS\data\프돈외기.txt";
            if (File.Exists(path))
                File.Delete(path);


            // 헤드라인 기록
            //string str = "";
            //str += "종목" + "\t" + "시총" + "\t" +
            //          "프돈_평균" + "\t" + "프돈_편차" + "\t" +
            //          "외돈_평균" + "\t" + "외돈_편차" + "\t" +
            //          "기돈_평균" + "\t" + "기돈_편차";
            //sw.Write(str);


            string temp_path = @"C:\BJS\temp.txt";
            if (File.Exists(temp_path))
                File.Delete(temp_path);

            StreamWriter tw = File.AppendText(temp_path);
            string str_temp = "";



            int MAX_INCLUDED = 100;

            int total_processed = 0;
            string str = "";

            foreach (string line in grlines)
            {
                string[] words = line.Split(' ');
                string 종목 = words[0].Replace("_", " ");
                string 시총 = words[1];

                int maximum_date = 0;
                int maximum_price = 0;

                int[] x = new int[12];
                List<double> 프돈 = new List<double>(); double 프돈_평균; double 프돈_편차; double 프돈_최대 = 0;
                List<double> 외돈 = new List<double>(); double 외돈_평균; double 외돈_편차; double 외돈_최대 = 0;
                List<double> 기돈 = new List<double>(); double 기돈_평균; double 기돈_편차; double 기돈_최대 = 0;

                // 전일종가 문제 있는 종목은 무시하고 진행
                int 전일종가 = Library.read_일자제시_전일종가(reference_date, 종목);
                if (전일종가 <= 0)
                {
                    str_temp += 종목 + "\n";
                    continue;
                }

                int moving_date = reference_date;

                int count = 0;
                for (int i = 0; i < 300; i++)
                {
                    moving_date = Library.directory_분전후(moving_date, -1); // 거래익일

                    int nword = Library.ReadStockMinute_LasLine(moving_date, 종목, x);
                    if (nword != 12 || x[0] / 100 != 1520)
                        continue;

                    프돈.Add(x[4]);
                    if (x[4] > 프돈_최대)
                        프돈_최대 = x[4];

                    외돈.Add(x[5]);
                    if (x[5] > 외돈_최대)
                        외돈_최대 = x[5];

                    기돈.Add(x[6]);
                    if (x[6] > 기돈_최대)
                        기돈_최대 = x[6];

                    if (count == MAX_INCLUDED)
                        break;
                }

                double MoneyFactor = 전일종가 / 100000000.0; // divided by 억원

                프돈_평균 = 프돈.Sum() / 프돈.Count * MoneyFactor;
                프돈_편차 = Math.Sqrt(프돈.Sum(t => Math.Pow(t - 프돈_평균, 2)) / (프돈.Count - 1)) * MoneyFactor;
                프돈_최대 *= MoneyFactor;

                외돈_평균 = 외돈.Sum() / 외돈.Count * MoneyFactor;
                외돈_편차 = Math.Sqrt(외돈.Sum(t => Math.Pow(t - 외돈_평균, 2)) / (외돈.Count - 1)) * MoneyFactor;
                외돈_최대 *= MoneyFactor;

                기돈_평균 = 기돈.Sum() / 기돈.Count * MoneyFactor;
                기돈_편차 = Math.Sqrt(기돈.Sum(t => Math.Pow(t - 기돈_평균, 2)) / (기돈.Count - 1)) * MoneyFactor;
                기돈_최대 *= MoneyFactor;


                str += 종목 + "\t" + 시총 + "\t" +
                         ((int)프돈_평균).ToString() + "\t" + ((int)프돈_편차).ToString() + "\t" + ((int)프돈_최대).ToString() + "\t" +
                         ((int)외돈_평균).ToString() + "\t" + ((int)외돈_편차).ToString() + "\t" + ((int)외돈_최대).ToString();

                str += "\n";

                total_processed++;
                textBox6.Text = "프돈외기 : " + total_processed.ToString() + "/" + grlines.Length.ToString();

                Thread.Sleep(100);
            }
            File.WriteAllText(path, str);


            tw.Write("{0}", str_temp);
            tw.Close();
        }

        private void 프돈외기(object sender, EventArgs e)
        {
            프돈외기();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            var a_tuple = new List<Tuple<int, int, int, double, double, string, int>> { };

            //List<List<string>> Gl = new List<List<string>>();// 실제 프로그램 내에서 사용하지 않음
            List<List<string>> GL = new List<List<string>>(); // 실제 프로그램 내에서 사용하지 않음
            List<string> alist = new List<string>();

            //_gl = Library.read_시총_일정액수이상(Convert.ToInt32(textBox2.Text));
            _gl = Library.ReadNaverIndustry(GL);
            int days = 20;
            double avr = 0.0, dev = 0.0;
            int 일평균거래액 = 0, 일최소거래액 = 0, 일최대거래액 = 0;
            int MaximumDate = 0;
            double MaximumPriceRiseRate = 0;
            foreach (var stock in _gl)
            {
                string dev_avr = Library.calcurate_종목일중변동평균편차(stock, days, ref avr, ref dev, ref 일평균거래액,
                           ref 일최소거래액, ref 일최대거래액, ref MaximumDate, ref MaximumPriceRiseRate);
                a_tuple.Add(Tuple.Create(일최대거래액, MaximumDate, 일평균거래액, dev, avr, stock, (int)MaximumPriceRiseRate));
            }
            a_tuple = a_tuple.OrderByDescending(t => t.Item1).ToList(); // 분당거래액_천만원이 0인 경우 tuple 아이템 수가 0인 문제 발생





            string path = @"C:\BJS\평점.txt";
            var lines = File.ReadAllLines(path, Encoding.Default);



            dtb1 = new DataTable();
            dtb1.Columns.Add("번호");
            dtb1.Columns.Add("종목");
            dtb1.Columns.Add("평점");
            dtb1.Columns.Add("최대");
            dtb1.Columns.Add("일평");
            dtb1.Columns.Add("편차");
            dtb1.Columns.Add("평균");

            int row = 0;
            string score = "";
            foreach (var item in a_tuple)
            {
                dtb1.Rows.Add("", "", "");
                dtb1.Rows[row][0] = row.ToString(); // 번호
                string stock = item.Item6.ToString(); // 종목
                dtb1.Rows[row][1] = stock;

                foreach (var item2 in lines)
                {
                    if (item2.Contains(stock))
                    {
                        string[] words = item2.Split('\t');
                        score = words[1];
                        break;
                    }
                }

                dtb1.Rows[row][2] = score.ToString(); // 평점
                dtb1.Rows[row][3] = item.Item1.ToString() + "(" + item.Item3.ToString() + ")"; // 최대

                dtb1.Rows[row][4] = item.Item7.ToString() + "(" + item.Item2.ToString() + ")"; // 일평
                dtb1.Rows[row][5] = item.Item4.ToString("#.#"); // 편차
                dtb1.Rows[row][6] = item.Item5.ToString("#.#"); // 평균
                row++;
            }
            dataGridView1.DataSource = dtb1;
            dataGridView1.Show();
            dataGridView1.Height = 1920;
            dataGridView1.Width = 700;
            dataGridView1.DefaultCellStyle.Font = new Font("Tahoma", 11, FontStyle.Bold);
        }

        private static DataTable dtb1;

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            string stock = (string)dataGridView1.Rows[e.RowIndex].Cells[1].Value;
            if (e.ColumnIndex == 0)
                CallNaver(stock, 1);
            if (e.ColumnIndex == 1)
                CallNaver(stock, 0);
            if (e.ColumnIndex == 3 || e.ColumnIndex == 4)
            {
                string item = (string)dataGridView1.Rows[e.RowIndex].Cells[2].Value;
                string[] words = item.Split(' ');

                int value = Convert.ToInt32(words[0]);
                if (e.ColumnIndex == 3)
                    value++;
                else
                    value--;

                string t = value.ToString();
                for (int i = 1; i < words.Length; i++)
                    t += " " + words[i];



                dtb1.Rows[e.RowIndex][2] = t;
            }

            if (e.ColumnIndex == 5)
            {
                string file = @"C:\BJS\" + "평점" + ".txt";
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
                Stream FS = new FileStream(@"C:\BJS\" + "평점" + ".txt", FileMode.CreateNew, FileAccess.Write);
                StreamWriter sw = new System.IO.StreamWriter(FS, System.Text.Encoding.Default);

                for (int k = 0; k < dtb1.Rows.Count; k++)
                {
                    string str = dtb1.Rows[k][1].ToString() + "\t" + dtb1.Rows[k][2].ToString();

                    sw.WriteLine(str);
                }
                sw.Close();
            }
        }

        public static void CallNaver(string stock, int selection)
        {
            CPUTILLib.CpStockCode _cd = new CPUTILLib.CpStockCode();

            string basestring;
            if (selection == 0)
            {
                basestring = "https://finance.naver.com/item/main.naver?code=";
            }
            else
            {
                basestring = "https://finance.naver.com/item/fchart.nhn?code=";
            }

            string code = _cd.NameToCode(stock);
            if (code == null)
                return;

            code = new String(code.Where(Char.IsDigit).ToArray());
            basestring += code;
            Process.Start("chrome", basestring);
        }

        private void button12_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void button12_Click(object sender, EventArgs e)
        {
            프돈외기();
        }


        // (같은 클래스 안 상단 또는 파일 하단에 작은 유틸 추가) 
        // [ADD] 매우 작은 트리밍 유틸 — 표본이 적으면 원본을 그대로 반환하여 동작 동일성 보장
        private static List<double> TrimPercentile(List<double> src, double lowerPct = 0.01, double upperPct = 0.99, int minKeep = 500)
        {
            if (src == null || src.Count == 0) return src ?? new List<double>(0);
            if (upperPct <= lowerPct) return src;

            // NaN/Inf 방어 (거의 필요 없겠지만 안전)
            var clean = src.Where(v => !double.IsNaN(v) && !double.IsInfinity(v)).ToList();
            if (clean.Count < minKeep) return src; // 표본 작으면 원본 유지

            var sorted = new List<double>(clean);
            sorted.Sort();
            int n = sorted.Count;

            int loIdx = (int)Math.Floor(n * lowerPct);
            int hiIdx = (int)Math.Floor(n * upperPct) - 1;
            if (hiIdx < loIdx) hiIdx = loIdx;

            double lo = sorted[Math.Max(0, Math.Min(n - 1, loIdx))];
            double hi = sorted[Math.Max(0, Math.Min(n - 1, hiIdx))];

            // 경계 안쪽만 유지
            var trimmed = clean.Where(v => v >= lo && v <= hi).ToList();
            // 트리밍 후 표본이 너무 작으면 역시 원본 유지
            if (trimmed.Count < minKeep) return src;

            return trimmed;
        }

        private void 통계()
        {
            textBox6.Text = "통계 진행 중";
            var selected1000Stocks = Library.SelectTop1000Stocks();

            int start_date = 20220302;
            int end_date = Convert.ToInt32(DateTime.Now.Date.ToString("yyyyMMdd"));
            string path = @"C:\BJS\data\통계.txt";

            if (File.Exists(path))
                File.Delete(path);

            using (StreamWriter sw = File.CreateText(path))
            {
                foreach (var stock in selected1000Stocks)
                {
                    if (stock.Contains("KODEX") || stock.Contains("혼합"))
                        continue;

                    string filePath = @"C:\BJS\data\일\" + stock + ".txt";
                    if (!File.Exists(filePath))
                        continue;

                    string str = stock;
                    str += "\t" + rd.FindHighestClose(filePath, 20).ToString();
                    str += "\t" + rd.FindHighestClose(filePath, 60).ToString();
                    str += "\t" + rd.FindHighestClose(filePath, 120).ToString();
                    str += "\t" + rd.FindHighestClose(filePath, 240).ToString();

                    var 푀분 = new List<double>();
                    var 거분 = new List<double>();
                    var 배차 = new List<double>();
                    var 배합 = new List<double>();

                    double value = 0.0;
                    int 전일종가 = rd.read_전일종가(stock);
                    if (전일종가 == 0)
                        continue;
                    double MoneyFactor = 전일종가 / g.천만원;

                    int DaysProcessed = 0;
                    int MinutesProcessed = 0;
                    int TotalDaysChecked = 0;

                    for (int i = end_date; i >= start_date; i--)
                    {
                        if (!DateTime.TryParseExact(i.ToString(), "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out _))
                            continue;

                        g.date = i;
                        TotalDaysChecked++;

                        int[,] x = new int[382, 12];
                        int nrow = rd.ReadStockMinute(i, stock, x);
                        if (nrow <= 1)
                            continue;

                        for (int j = 1; j < nrow; j++)
                        {
                            double interval = ms.total_Seconds(x[j - 1, 0], x[j, 0]);
                            if (interval > 70.0 || interval < 50.0)
                                continue;

                            value = (double)(x[j, 4] - x[j - 1, 4] + x[j, 5] - x[j - 1, 5]) * MoneyFactor;
                            if (value > 0.001)
                                푀분.Add(value);

                            value = (double)(x[j, 7] - x[j - 1, 7]) * MoneyFactor;
                            거분.Add(value);

                            배차.Add(x[j, 8] - x[j, 9]);
                            배합.Add(x[j, 8] + x[j, 9]);

                            MinutesProcessed++;
                        }

                        if (DaysProcessed++ >= 30)
                            break;
                    }

                    if (MinutesProcessed <= 1000)
                        continue;

                    // 출력 줄 1: 종목명 + 최고가 정보
                    sw.WriteLine(str);

                    // =========================
                    //  ★★★ 핵심 변경 지점 ★★★
                    //  CalcStats 호출 직전에 '상하 1% 트리밍'만 적용
                    //  (표본이 적으면 유틸이 원본을 그대로 반환하므로 행동 동일)
                    // =========================

                    // [PATCH] 분봉 통계 4종: 푀분, 거분, 배차, 배합 — 각각 트리밍된 사본으로 통계
                    var 푀분_t = TrimPercentile(푀분, 0.01, 0.99, 500);
                    var 거분_t = TrimPercentile(거분, 0.01, 0.99, 500);
                    var 배차_t = TrimPercentile(배차, 0.01, 0.99, 500);
                    var 배합_t = TrimPercentile(배합, 0.01, 0.99, 500);

                    // 출력 줄 2~5: 기존 분봉 통계 (호출부/형식 동일)
                    (var avg, var std) = Library.CalcStats(푀분_t);
                    sw.WriteLine($"{푀분_t.Count}\t{avg:F2}\t{std:F2}");

                    (avg, std) = Library.CalcStats(거분_t);
                    sw.WriteLine($"{avg:F2}\t{std:F2}");

                    (avg, std) = Library.CalcStats(배차_t);
                    sw.WriteLine($"{avg:F2}\t{std:F2}");

                    (avg, std) = Library.CalcStats(배합_t);
                    sw.WriteLine($"{avg:F2}\t{std:F2}");

                    // 출력 줄 6~8: 일봉 기반 통계 (여기는 기존 그대로 사용)
                    double dAvg, dStd, dAvgF, dStdF, dAvgI, dStdI;
                    CalcDailyStats(filePath, out dAvg, out dStd, out dAvgF, out dStdF, out dAvgI, out dStdI);
                    sw.WriteLine($"{dAvg:F2}\t{dStd:F2}");   // 거래대금 (천만)
                    sw.WriteLine($"{dAvgF:F2}\t{dStdF:F2}"); // 외인
                    sw.WriteLine($"{dAvgI:F2}\t{dStdI:F2}"); // 기관 

                    var stats = QuoteStatsLib.QuoteStatsCalculator.Compute(@"C:\BJS\호가변동자료", stock);

                    sw.WriteLine($"{stats.BestAskMean:F0}\t{stats.BestAskStd:F0}");   // 최우선매도호가잔량
                    sw.WriteLine($"{stats.BestBidMean:F0}\t{stats.BestBidStd:F0}");   // 최우선매수호가잔량
                    sw.WriteLine($"{stats.TotalAskMean:F0}\t{stats.TotalAskStd:F0}"); // 총매도호가잔량
                    sw.WriteLine($"{stats.TotalBidMean:F0}\t{stats.TotalBidStd:F0}"); // 총매수호가잔량
                }
            }

            textBox6.Text = "통계 done";
        }

        // 20일전고, 60일 전고, 120일 전고, 240일 전고
        // 푀분평균, 푀분편차, 거분평균, 거분편차, 배차평균, 배차편차,
        // 배합평균, 배합편차, 푀누평균, 푀누편차, 종누평균, 종누편차
        private void 통계_20251019()
        {
            textBox6.Text = "통계 진행 중";
            var selected1000Stocks = Library.SelectTop1000Stocks();

            int start_date = 20220302;
            int end_date = Convert.ToInt32(DateTime.Now.Date.ToString("yyyyMMdd"));
            string path = @"C:\BJS\data\통계.txt";

            if (File.Exists(path))
                File.Delete(path);

            using (StreamWriter sw = File.CreateText(path))
            {
                foreach (var stock in selected1000Stocks)
                {
                    if (stock.Contains("KODEX") || stock.Contains("혼합"))
                        continue;

                    string filePath = @"C:\BJS\data\일\" + stock + ".txt";
                    if (!File.Exists(filePath))
                        continue;

                    string str = stock;
                    str += "\t" + rd.FindHighestClose(filePath, 20).ToString();
                    str += "\t" + rd.FindHighestClose(filePath, 60).ToString();
                    str += "\t" + rd.FindHighestClose(filePath, 120).ToString();
                    str += "\t" + rd.FindHighestClose(filePath, 240).ToString();

                    var 푀분 = new List<double>();
                    var 거분 = new List<double>();
                    var 배차 = new List<double>();
                    var 배합 = new List<double>();

                    double value = 0.0;
                    int 전일종가 = rd.read_전일종가(stock);
                    if (전일종가 == 0)
                        continue;
                    double MoneyFactor = 전일종가 / g.천만원;

                    int DaysProcessed = 0;
                    int MinutesProcessed = 0;
                    int TotalDaysChecked = 0;

                    for (int i = end_date; i >= start_date; i--)
                    {
                        if (!DateTime.TryParseExact(i.ToString(), "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out _))
                            continue;

                        g.date = i;
                        TotalDaysChecked++;

                        int[,] x = new int[382, 12];
                        int nrow = rd.ReadStockMinute(i, stock, x);
                        if (nrow <= 1)
                            continue;

                        for (int j = 1; j < nrow; j++)
                        {
                            double interval = ms.total_Seconds(x[j - 1, 0], x[j, 0]);
                            if (interval > 70.0 || interval < 50.0)
                                continue;

                            value = (double)(x[j, 4] - x[j - 1, 4] + x[j, 5] - x[j - 1, 5]) * MoneyFactor;
                            if (value > 0.001)
                                푀분.Add(value);

                            value = (double)(x[j, 7] - x[j - 1, 7]) * MoneyFactor;
                            거분.Add(value);

                            배차.Add(x[j, 8] - x[j, 9]);
                            배합.Add(x[j, 8] + x[j, 9]);

                            MinutesProcessed++;
                        }

                        if (DaysProcessed++ >= 30)
                            break;
                    }

                    if (MinutesProcessed <= 1000)
                        continue;

                    // 출력 줄 1: 종목명 + 최고가 정보
                    sw.WriteLine(str);

                    // 출력 줄 2~5: 기존 분봉 통계
                    (var avg, var std) = Library.CalcStats(푀분);
                    sw.WriteLine($"{푀분.Count}\t{avg:F2}\t{std:F2}");

                    (avg, std) = Library.CalcStats(거분);
                    sw.WriteLine($"{avg:F2}\t{std:F2}");

                    (avg, std) = Library.CalcStats(배차);
                    sw.WriteLine($"{avg:F2}\t{std:F2}");

                    (avg, std) = Library.CalcStats(배합);
                    sw.WriteLine($"{avg:F2}\t{std:F2}");

                    // 출력 줄 6~8: 일봉 기반 통계
                    // ✔ 아래는 새로운 변수 이름 사용
                    double dAvg, dStd, dAvgF, dStdF, dAvgI, dStdI;
                    CalcDailyStats(filePath, out dAvg, out dStd, out dAvgF, out dStdF, out dAvgI, out dStdI);
                    sw.WriteLine($"{dAvg:F2}\t{dStd:F2}"); // 거래대금 (천만)
                    sw.WriteLine($"{dAvgF:F2}\t{dStdF:F2}"); // 외인
                    sw.WriteLine($"{dAvgI:F2}\t{dStdI:F2}"); // 기관 

                    var stats = QuoteStatsLib.QuoteStatsCalculator.Compute(@"C:\BJS\호가변동자료", stock);

                    sw.WriteLine($"{stats.BestAskMean:F0}\t{stats.BestAskStd:F0}"); // 최우선매도호가잔량
                    sw.WriteLine($"{stats.BestBidMean:F0}\t{stats.BestBidStd:F0}"); // 최우선매수호가잔량
                    sw.WriteLine($"{stats.TotalAskMean:F0}\t{stats.TotalAskStd:F0}"); // 총매도호가잔량
                    sw.WriteLine($"{stats.TotalBidMean:F0}\t{stats.TotalBidStd:F0}"); // 총매수호가잔량
                }
            }

            textBox6.Text = "통계 done";
        } 

        private void CalcDailyStats(string filePath, out double 거래금액_Avg, out double 거래금액_Std,
                            out double 외인_Avg, out double 외인_Std, out double 기관_Avg, out double 기관_Std)
        {
            거래금액_Avg = 거래금액_Std = 외인_Avg = 외인_Std = 기관_Avg = 기관_Std = 0;

            var lines = File.ReadAllLines(filePath).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
            if (lines.Count < 2) return;

            var last = lines.Last().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (!int.TryParse(last[4], out int 종가)) return;

            var 거래List = new List<double>();
            var 외인List = new List<double>();
            var 기관List = new List<double>();

            int i = lines.Count - 1;
            int volumeSamples = 0;
            int minSamples = 30;

            while (i > 0 && (volumeSamples < 60 || 외인List.Count < minSamples || 기관List.Count < minSamples))
            {
                var curr = lines[i].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                var prev = lines[i - 1].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (curr.Length < 10 || prev.Length < 10) { i--; continue; }

                if (volumeSamples < 60 && long.TryParse(curr[5], out long 거래량))
                {
                    거래List.Add(거래량 * 종가 / 10000000.0); // 천만으로 나눔
                    volumeSamples++;
                }

                if (long.TryParse(curr[8], out long f_curr) && long.TryParse(prev[8], out long f_prev) && f_curr - f_prev > 0)
                {
                    외인List.Add((f_curr - f_prev) * 종가 / 10000000.0);
                }

                if (long.TryParse(curr[9], out long i_curr) && long.TryParse(prev[9], out long i_prev) && i_curr - i_prev > 0)
                {
                    기관List.Add((i_curr - i_prev) * 종가 / 10000000.0);
                }

                i--;
            }

            if (거래List.Count > 0)
            {
                double avg = 거래List.Average();
                double std = Math.Sqrt(거래List.Select(x => Math.Pow(x - avg, 2)).Average());
                거래금액_Avg = avg;
                거래금액_Std = std;
            }
            else
            {
                거래금액_Avg = 0;
                거래금액_Std = 0;
            }

            if (외인List.Count > 0)
            {
                double avg = 외인List.Average(); // ✅ 수정됨
                double std = Math.Sqrt(외인List.Select(x => Math.Pow(x - avg, 2)).Average()); // ✅
                외인_Avg = avg;
                외인_Std = std;
            }
            else
            {
                외인_Avg = 0;
                외인_Std = 0;
            }

            if (기관List.Count > 0)
            {
                double avg = 기관List.Average(); // ✅ 수정됨
                double std = Math.Sqrt(기관List.Select(x => Math.Pow(x - avg, 2)).Average()); // ✅
                기관_Avg = avg;
                기관_Std = std;
            }
            else
            {
                기관_Avg = 0;
                기관_Std = 0;
            }

        }

        private void 통계_지수() // MOD,  g.ogldata 1581, 통계 1573
        {
            // rd.read_변수();
            textBox6.Text = "통계지수(지수 분당 변화 avr & dev) 진행 중";

            int start_date = 20220302; // MOD
            int end_date = Convert.ToInt32(DateTime.Now.Date.ToString("yyyyMMdd"));

            string path = @"C:\BJS\data\통계_지수.txt";

            if (File.Exists(path))
                File.Delete(path);

            StreamWriter sw = File.CreateText(path);

            // header writing
            string str = "// 지수\avr/min\tdev/min";
            sw.WriteLine("{0}", str);

            int DaysProcessed = 0;


            var 지수종목 = new List<string>();
            지수종목.Add("KODEX 레버리지");
            지수종목.Add("KODEX 코스닥150레버리지");

            foreach (var stock in 지수종목) // 혼합 2 종목 빠져시 to-jsb 보다 2 종목 작음
            {
                int MinutesProcessed = 0;

                var 지수종목_가격차이 = new List<double>();

                for (int i = end_date; i >= start_date; i--)
                {
                    g.date = i;

                    int[,] x = new int[400, 12];
                    int nrow = rd.ReadStockMinute(i, stock, x); // i -> date
                    if (!rd.readStockMinuteCheck(nrow, x)) // check usability of minute data 
                        continue;
                    else
                        MinutesProcessed++;

                    if (MinutesProcessed == 60)
                        break;

                    for (int j = 1; j < nrow; j++)
                    {
                        double interval_by_seconds = ms.total_Seconds(x[j - 1, 0], x[j, 0]);
                        if (interval_by_seconds > 75.0 || interval_by_seconds < 45.0) // interval_by_seconds larger than 70 seconds
                        {
                            continue;
                        }

                        지수종목_가격차이.Add((double)(x[j, 1] - x[j - 1, 1]));
                    }
                }

                if (MinutesProcessed == 0) // 하루치도 없는 경우 무시하고 다음 종목으로
                    continue;

                // 푀분
                double avr = 0.0;
                double dev = 0.0;
                avr = 지수종목_가격차이.Sum() / 지수종목_가격차이.Count;
                dev = Math.Sqrt(지수종목_가격차이.Sum(y => Math.Pow(y - avr, 2)) / (지수종목_가격차이.Count - 1));
                str = "";
                str += stock + "\t" + avr.ToString("#.##") + "\t" + dev.ToString("#.##");

                sw.WriteLine("{0}", str);
            }
            sw.Close();
            textBox6.Text = "통계지수(지수 분당 변화 avr & dev) done";
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void 통계_지수_Click(object sender, EventArgs e)
        {
            통계_지수();
        }

        private void 통계_Click(object sender, EventArgs e)
        {
            통계();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        private void button15_Click(object sender, EventArgs e)
        {
            var start = "20250102";
            var end = "20250424";
            MinuteNormalizer.Run(start, end);
        }
    }
}