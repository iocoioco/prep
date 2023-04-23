

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
            List<List<string>> GL = new List<List<string>>(); // 실제 프로그램 내에서 사용하지 않음
            List<string> alist = new List<string>();
            //_gl = Pre_Processor_Class1.read_group();
            //_gl = Pre_Processor_Class1.read_시총_일정액수이상(Convert.ToInt32(textBox2.Text));
            //_gl = Pre_Processor_Class1.read_그룹_네이버_업종(Gl, GL);
            //_gl = Pre_Processor_Class1.read_전종목코드();


            //_gl = Pre_Processor_Class1.read_시총_일정액수이상(Convert.ToInt32(textBox2.Text));


            _gl = Pre_Processor_Class1.read_그룹_네이버_테마();
            _variable[0] = textBox3.Text; // 일, 주, 월 중 하나 입력
            _variable[1] = textBox4.Text; // 오늘부터 과거로 가면서 최대갯수 

            //_gl = Pre_Processor_Class1.read_분별종목(); // _oL unique list : 옛날 방식의 groupList and singleList
            for (int i = 0; i < _gl.Count; i++)
            {
                _aV[i] = Pre_Processor_Class1.calculate_종목20일기준일평균거래량(_gl[i]);
            }

            Pre_Processor_Class1.read_누적(_mF); // read from file "일중분별누적%" 901 0.017 etc


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

            Pre_Processor_Class1.read_날짜_삼성전자일자료로부터(Convert.ToInt32(_variable[0]),
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

            path = @"C:\WORK\틱\";
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

            Pre_Processor_Class1.read_날짜_삼성전자일자료로부터(Convert.ToInt32(_variable[0]),
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

                    path = @"C:\WORK\분\";
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

            path = @"C:\WORK\분\";
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
            int 전일종가 = Pre_Processor_Class1.read_일자제시_전일종가(_workingDay, stockname);
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
                alist = Pre_Processor_Class1.find_리스트(x[i, 0].ToString(), _mF);
                double minuteaverage = Convert.ToDouble(alist[1]);
                string code1 = _Stock_Chart_분.GetHeaderValue(0);
                string name = _cm.CodeToName(code1);
                int nthfile = Pre_Processor_Class1.find_순서(name, _gl);

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

            List<string> Gl = new List<string>(); // 실제 프로그램 내에서 사용하지 않음
            List<List<string>> GL = new List<List<string>>(); // 실제 프로그램 내에서 사용하지 않음
            List<string> kodex_list = new List<string>();

            _gl.Clear();
            Pre_Processor_Class1.read_KODEX(_gl);

            _gl = Pre_Processor_Class1.read_그룹_네이버_업종(GL);
            Pre_Processor_Class1.read_그룹_네이버_테마(_gl, Gl, GL);

            textBox6.Text = _gl.Count.ToString() + " - 일 진행 중";


            //_gl = Pre_Processor_Class1.read_전종목코드();
            //_gl = Pre_Processor_Class1.read_시총_일정액수이상(Convert.ToInt32(textBox2.Text));
            //_gl = Pre_Processor_Class1.read_그룹4(Gl);
            _gl.Add("KODEX 레버리지"); // 전체적으로 지수의 흐름을 보고 단타를 치기위함
            _gl.Add("KODEX 200선물인버스2X");
            _gl.Add("KODEX 코스닥150레버리지");
            _gl.Add("KODEX 코스닥150선물인버스");

            _Stock_Chart_일주월.Received += new CPSYSDIBLib._ISysDibEvents_ReceivedEventHandler(_Stock_Chart_일주월_Received);

            _variable[0] = "일"; // 일, 주, 월 중 하나 입력
            _variable[1] = "700"; // 오늘부터 과거로 가면서 최대갯수 
            _Stock_Chart_일주월_stockChart();

            /*
            _variable[0] = "주"; // 일, 주, 월 중 하나 입력
            _variable[1] = "300"; // 오늘부터 과거로 가면서 최대갯수 
            _Stock_Chart_일주월_stockChart();

            _variable[0] = "월"; // 일, 주, 월 중 하나 입력
            _variable[1] = "120"; // 오늘부터 과거로 가면서 최대갯수 
            _Stock_Chart_일주월_stockChart();
            */

            /*
            foreach(var stockname in _gl)
            {
                _cpsvr7254 = new CPSYSDIBLib.CpSvr7254();
                _cpsvr7254.Received += new CPSYSDIBLib._ISysDibEvents_ReceivedEventHandler(_cpsvr7254_Received);
                _cpsvr7254.SetInputValue(0, _cpstockcode.NameToCode(stockname));
                _cpsvr7254.SetInputValue(1, 3); //0:사용자지정 1:1개,월, 2:2개월 3:3개월 4:6개월,5:최근5일 6:일별
                _cpsvr7254.SetInputValue(2, 20190102); // 0:사용자지정일 아닐 경우 시작일
                _cpsvr7254.SetInputValue(3, 20190315); // 0:사용자지정일 아닐 경우 종료일
                _cpsvr7254.SetInputValue(4, '1'); //'0' 순매수, '1' 매매비중
                _cpsvr7254.SetInputValue(5, 0);// 전체, 1 개인, 2 외국인 ...
                while (true)
                {
                    int result = _cpsvr7254.BlockRequest();
                    if (result != 0)
                    {
                        continue;
                    }
                }
            }
            */

            textBox6.Text = "일 진행 중";
        }
        // 일, 주, 월 & 갯수 입력 후 10개 자료 저장
        //
        //
        private void button1_Click_1(object sender, EventArgs e)
        {
            일주월();
        }

        public void _Stock_Chart_일주월_stockChart()
        {
            for (int i = 0; i < _gl.Count; i++)
            {

                string code = _cpstockcode.NameToCode(_gl[i]);
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
                    path = @"C:\WORK\data\일\";
                    break;
                case "주":
                    path = @"C:\WORK\data\주\";
                    break;
                case "월":
                    path = @"C:\WORK\data\월\";
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
                    _Stock_Chart_일주월.GetDataValue(0, k), _Stock_Chart_일주월.GetDataValue(1, k), _Stock_Chart_일주월.GetDataValue(2, k),
                    _Stock_Chart_일주월.GetDataValue(3, k), _Stock_Chart_일주월.GetDataValue(4, k), _Stock_Chart_일주월.GetDataValue(5, k),
                    _Stock_Chart_일주월.GetDataValue(6, k), _Stock_Chart_일주월.GetDataValue(7, k), _Stock_Chart_일주월.GetDataValue(8, k),
                    _Stock_Chart_일주월.GetDataValue(9, k));
                // 날짜, 시가, 고가, 저가, 종가, 거래량, 누적매도, 누적매수, 외국인보유, 기관누적순매수
            }
            sw.Close();
        }


        /*
        void _cpsvr7254_Received()
        {
            string text1 = _cpsvr7254.GetDibMsg1();
            string text2 = _cpsvr7254.GetDibMsg2();

            string code = _cpsvr7254.GetHeaderValue(0); // 종목코드
            string stockname = _cpstockcode.CodeToName(code);
            string path = @"C:\WORK\량\" + stockname + ".txt";
            if (File.Exists(path))
                File.Delete(path);

            StreamWriter sw = File.CreateText(path);
            
            int count = (int)_cpsvr7254.GetHeaderValue(1); // 종목갯수

            // 어떤 시점을 기준으로 누적수량으로 표시하는 데 보유수량은 아님
            // 문제점 현재 어떻게 변수를 지정하여도 17일 밖에 나오지 않아 아쉬움
            for (int k = 0; k < count - 1; k++)
            {
                long 일자 = _cpsvr7254.GetDataValue(0, k);
                long 개인 = _cpsvr7254.GetDataValue(1, k) - _cpsvr7254.GetDataValue(1, k + 1);
                long 외인 = _cpsvr7254.GetDataValue(2, k) - _cpsvr7254.GetDataValue(2, k + 1);
                long 기계 = _cpsvr7254.GetDataValue(3, k) - _cpsvr7254.GetDataValue(3, k + 1);
                long 금투 = _cpsvr7254.GetDataValue(3, k) - _cpsvr7254.GetDataValue(3, k + 1);
                long 보험 = _cpsvr7254.GetDataValue(3, k) - _cpsvr7254.GetDataValue(3, k + 1);
                long 투신 = _cpsvr7254.GetDataValue(3, k) - _cpsvr7254.GetDataValue(3, k + 1);
                long 은행 = _cpsvr7254.GetDataValue(3, k) - _cpsvr7254.GetDataValue(3, k + 1);
                long 기금 = _cpsvr7254.GetDataValue(3, k) - _cpsvr7254.GetDataValue(3, k + 1);
                long 연기 = _cpsvr7254.GetDataValue(3, k) - _cpsvr7254.GetDataValue(3, k + 1);
                long 기법 = _cpsvr7254.GetDataValue(3, k) - _cpsvr7254.GetDataValue(3, k + 1);
                long 기외 = _cpsvr7254.GetDataValue(3, k) - _cpsvr7254.GetDataValue(3, k + 1);
                long 사모 = _cpsvr7254.GetDataValue(3, k) - _cpsvr7254.GetDataValue(3, k + 1);
                long 국가 = _cpsvr7254.GetDataValue(3, k) - _cpsvr7254.GetDataValue(3, k + 1);
                sw.WriteLine("{0} {1} {2} {3}", 일자, 개인, 외인, 기계);

            }
            sw.Close();
        }
        */

        // 시총계산
        //
        //

        public void 시가총액()
        {
            textBox6.Text = "시가총액 진행 중";
            List<string> Gl = new List<string>(); // 실제 프로그램 내에서 사용하지 않음
            List<List<string>> GL = new List<List<string>>(); // 실제 프로그램 내에서 사용하지 않음
            List<string> alist = new List<string>();

            //_gl = Pre_Processor_Class1.read_시총_일정액수이상(Convert.ToInt32(textBox2.Text));
            //_gl = Pre_Processor_Class1.read_그룹_네이버_업종(Gl, GL);



            _gl.Clear();

            Pre_Processor_Class1.read_그룹_네이버_테마(_gl, Gl, GL);
            _gl = Pre_Processor_Class1.read_그룹_네이버_업종(GL);
            Pre_Processor_Class1.read_KODEX(_gl);
            Pre_Processor_Class1.read_그룹_상관(_gl, Gl, GL);


            //_gl = Pre_Processor_Class1.read_전종목코드();
            //_gl = Pre_Processor_Class1.read_그룹4(Gl);
            //Pre_Processor_Class1.calc_평균(_gl, _aV); // calculate day average for each stock, averaging  20 days
            //Pre_Processor_Class1.read_누적(_mF); // read from file "일중분별누적%" 901 0.017 etc

            // 종목이름 맞는 지 확인
            foreach (string stockname in _gl)
            {

                string code = _cpstockcode.NameToCode(stockname);
                if (code == "")
                {

                }
            }

            _Stock_Chart_시총.Received += new CPSYSDIBLib._ISysDibEvents_ReceivedEventHandler(_Stock_Chart_시총_Received);

            _Stock_Chart_시총_stockChart();
            textBox6.Text = "시가총액 Processed";
        }




        private void button3_Click_1(object sender, EventArgs e)
        {
            시가총액();
        }

        public void _Stock_Chart_시총_stockChart()
        {
            string file = @"C:\WORK\data\시총.txt";

            if (File.Exists(file))
                File.Delete(file);

            StreamWriter sw = File.CreateText(file);
            sw.Close();

            for (int i = 0; i < _gl.Count; i++)
            {
                //Thread.Sleep(1000); // Less than 15 request in 15 minutes



                if (_Stock_Chart_시총.GetDibStatus() == 1)
                {
                    Trace.TraceInformation("DibRq 요청 수신대기 중 입니다. 수신이 완료된 후 다시 호출 하십시오.");
                    continue;
                }

                string code = _cpstockcode.NameToCode(_gl[i]);
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

            Stream FS = new FileStream(@"C:\WORK\data\시총.txt", FileMode.Append, FileAccess.Write);
            StreamWriter sw = new System.IO.StreamWriter(FS, System.Text.Encoding.Default);

            int count = (int)_Stock_Chart_시총.GetHeaderValue(1);

            string code = _Stock_Chart_시총.GetHeaderValue(0);
            string stockname = _cm.CodeToName(code);

            int numberofData = (int)_Stock_Chart_시총.GetHeaderValue(3);
            if (numberofData == 0)
                return;

            ulong 시총 = _Stock_Chart_시총.GetDataValue(0, 0); //0

            string temp_stockname = stockname.Replace(" ", "_");
            sw.WriteLine("{0} {1}", temp_stockname, 시총 / 100000000); // 단위 : 천억

            sw.Close();
        }


        // 상관관계
        //
        //
        private void button4_Click_1(object sender, EventArgs e)
        {
            textBox6.Text = "상관관계" + " is Processing";
            _gl = Pre_Processor_Class1.read_그룹_네이버_테마();
            //_gl = Pre_Processor_Class1.read_시총_일정액수이상(Convert.ToInt32(textBox2.Text));
            Pre_Processor_Class1.시총순서(_gl);

            List<string> selected_gl = new List<string>();
            int days = 20;
            double avr = 0, dev = 0;
            int 일평균거래액 = 0, 일최소거래액 = 0, 일최대거래액 = 0;
            int MaximumDate = 0;
            double MaximumPriceRiseRate = 0;
            foreach (var stock in _gl)
            {
                Pre_Processor_Class1.calcurate_종목일중변동평균편차(stock, days, ref avr, ref dev, ref 일평균거래액,
                                             ref 일최소거래액, ref 일최대거래액, ref MaximumDate, ref MaximumPriceRiseRate);
                if (일최대거래액 > 500)
                    selected_gl.Add(stock);
            }

            Pre_Processor_Class1.cal_상관관계(selected_gl);
            textBox6.Text = "상관관계" + " is Processed";
        }


        //	투자주체별현황을 일별/기간별, 순매수/매매비중을 일자별
        private void button5_Click(object sender, EventArgs e)
        {
            // List<List<string>> Gl = new List<List<string>>(); // 실제 프로그램 내에서 사용하지 않음
            List<List<string>> GL = new List<List<string>>(); // 실제 프로그램 내에서 사용하지 않음
            List<string> alist = new List<string>();
            //_gl = Pre_Processor_Class1.read_그룹4(Gl);
            //_gl = Pre_Processor_Class1.read_시총_일정액수이상(Convert.ToInt32(textBox2.Text));
            _gl = Pre_Processor_Class1.read_그룹_네이버_업종(GL);
            string path = @"C:\WORK\그룹_" + textBox2.Text + ".txt";
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
            string path = @"C:\WORK\매\" + _cm.CodeToName(code) + ".txt";

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

        /*
        private void button6_click(object sender, EventArgs e)
        {
            List<string> alist = new List<string>();
            _code = Pre_Processor_Class1.read_전종목코드();
            _gl.Clear();
            foreach (var code in _code)
            {
                string stockname = _cpstockcode.CodeToName(code);
                _gl.Add(stockname);
            }

            //Pre_Processor_Class1.calc_평균(_gl, _aV); // calculate day average for each stock, averaging  20 days
            //Pre_Processor_Class1.read_누적(_mF); // read from file "일중분별누적%" 901 0.017 etc

            // 종목이름 맞는 지 확인
            foreach (string stockname in _gl)
            {
                string code = _cpstockcode.NameToCode(stockname);
                if (code == null)
                {
                    MessageBox.Show(stockname + " Error");
                    return;
                }
            }

            _Stock_Chart6.Received += new CPSYSDIBLib._ISysDibEvents_ReceivedEventHandler(_Stock_Chart6_Received);

            _Stock_Chart6_stockChart();


            Stream FS = new FileStream(@"C:\WORK\시총_.txt", FileMode.Append, FileAccess.Write);
            StreamWriter sw = new System.IO.StreamWriter(FS, System.Text.Encoding.Default);

            시총.Sort();

            foreach (var a in 시총)
            {
                sw.WriteLine("{0} {1}", a[1], a[0]);
            }

            sw.Close();


            textBox6.Text = "시가총액 Processed";
        }

        public void _Stock_Chart6_stockChart()
        {
            string file = @"C:\WORK\시총.txt";

            if (File.Exists(file))
                File.Delete(file);

            StreamWriter sw = File.CreateText(file);
            sw.Close();

            for (int i = 0; i < _gl.Count; i++)
            {
                //Thread.Sleep(1000); // Less than 15 request in 15 minutes
                textBox6.Text = "Currently: " + _gl[i] + " is Processing";

                if (_Stock_Chart6.GetDibStatus() == 1)
                {
                    Trace.TraceInformation("DibRq 요청 수신대기 중 입니다. 수신이 완료된 후 다시 호출 하십시오.");
                    continue;
                }

                string code = _cpstockcode.NameToCode(_gl[i]);
                if (code.Length < 7)
                {
                    continue;
                }

                object[] fields = new object[1] { 13 };

                _Stock_Chart6.SetInputValue(0, code);   //종목코드
                _Stock_Chart6.SetInputValue(1, 2);   //요청코드 '1'(기간), '2'(개수)
                _Stock_Chart6.SetInputValue(5, fields);   // 필드 갯수
                _Stock_Chart6.SetInputValue(6, 'D');   // 틱,분,일,주,월 단위 데이터

                // 출력자료 요청
                int result = _Stock_Chart6.BlockRequest();
                if (result != 0)
                {
                    i--; // to try again with the saem stock
                    continue;
                }
            }
        }

        private void _Stock_Chart6_Received()
        {
            string text1 = _Stock_Chart6.GetDibMsg1();
            string text2 = _Stock_Chart6.GetDibMsg2();

            Stream FS = new FileStream(@"C:\WORK\시총.txt", FileMode.Append, FileAccess.Write);
            StreamWriter sw = new System.IO.StreamWriter(FS, System.Text.Encoding.Default);

            int count = (int)_Stock_Chart6.GetHeaderValue(1);

            string code = _Stock_Chart6.GetHeaderValue(0);
            string stockname = _cm.CodeToName(code);

            int numberofData = (int)_Stock_Chart6.GetHeaderValue(3);
            if (numberofData == 0)
                return;

            ulong price = _Stock_Chart6.GetDataValue(0, 0); // 시총

            string temp_stockname = stockname.Replace(" ", "_");

            sw.WriteLine("{0} {1}", temp_stockname, price / 100000000);

            sw.Close();
        }
        */


        private void button7_Click(object sender, EventArgs e)
        {
            var list = Pre_Processor_Class1.read_시총_일정액수이상(Convert.ToInt32(textBox2.Text));

            string path = @"C:\WORK\" + "그룹" + textBox2.Text + ".txt";

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

        /*
        private void button8_Click(object sender, EventArgs e)
        {

        */

        public void 네이버_업종()
        {
            textBox6.Text = "업종 진행 중";

            var doc = new HtmlAgilityPack.HtmlDocument();
            HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();

            string file = @"C:\WORK\data\" + "그룹_네이버_업종" + ".txt";
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            Stream FS = new FileStream(@"C:\WORK\data\" + "그룹_네이버_업종" + ".txt", FileMode.CreateNew, FileAccess.Write);
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
                if (themeRemove.Contains(i))
                    continue;

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

                textBox6.Text = theme_count.ToString() + "/" + i.ToString() + " 업종 Processe";
            }
            sw.Close();
            textBox6.Text = theme_count.ToString() + "/" + count.ToString() + " 업종 Processed";
        }



        private void button9_Click(object sender, EventArgs e)
        {
            네이버_업종();
            return;

            textBox6.Text = "네이버 업종 정리 Processing";

            var doc = new HtmlAgilityPack.HtmlDocument();
            HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();

            string file = @"C:\WORK\data\" + "그룹_네이버_업종" + ".txt";
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            Stream FS = new FileStream(@"C:\WORK\data\" + "그룹_네이버_업종" + ".txt", FileMode.CreateNew, FileAccess.Write);
            StreamWriter sw = new System.IO.StreamWriter(FS, System.Text.Encoding.Default);

            var lines = File.ReadAllLines(@"C:\WORK\data\" + "업종_url" + ".txt");

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


            //File.WriteAllText(@"C:\WORK\" + "temp" + ".txt", all_stock);

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

        public static string GetFinalRedirect(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            int maxRedirCount = 8;  // prevent infinite loops
            string newUrl = url;
            do
            {
                HttpWebRequest req = null;
                HttpWebResponse resp = null;
                try
                {
                    req = (HttpWebRequest)HttpWebRequest.Create(url);
                    req.Method = "HEAD";
                    req.AllowAutoRedirect = false;
                    resp = (HttpWebResponse)req.GetResponse();
                    switch (resp.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            return newUrl;
                        case HttpStatusCode.Redirect:
                        case HttpStatusCode.MovedPermanently:
                        case HttpStatusCode.RedirectKeepVerb:
                        case HttpStatusCode.RedirectMethod:
                            newUrl = resp.Headers["Location"];
                            if (newUrl == null)
                                return url;

                            if (newUrl.IndexOf("://", System.StringComparison.Ordinal) == -1)
                            {
                                // Doesn't have a URL Schema, meaning it's a relative or absolute URL
                                Uri u = new Uri(new Uri(url), newUrl);
                                newUrl = u.ToString();
                            }
                            break;
                        default:
                            return newUrl;
                    }
                    url = newUrl;
                }
                catch (WebException)
                {
                    // Return the last known good URL
                    return newUrl;
                }
                catch (Exception ex)
                {
                    return null;
                }
                finally
                {
                    if (resp != null)
                        resp.Close();
                }
            } while (maxRedirCount-- > 0);

            return newUrl;
        }

        public void 네이버_테마(int version)
        {
            textBox6.Text = "테마 진행 중";

            var doc = new HtmlAgilityPack.HtmlDocument();
            HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();

            string file = @"C:\WORK\data\" + "그룹_네이버_테마" + ".txt";
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            Stream FS = new FileStream(@"C:\WORK\data\" + "그룹_네이버_테마" + ".txt", FileMode.CreateNew, FileAccess.Write);
            StreamWriter sw = new System.IO.StreamWriter(FS, System.Text.Encoding.Default);

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
            textBox6.Text = theme_count.ToString() + "/" + count.ToString() + " 테마 Processed";
        }



        public void 네이버_테마()
        {
            textBox6.Text = "테마 진행 중";

            var doc = new HtmlAgilityPack.HtmlDocument();
            HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();

            string file = @"C:\WORK\data\" + "그룹_네이버_테마" + ".txt";
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            Stream FS = new FileStream(@"C:\WORK\data\" + "그룹_네이버_테마" + ".txt", FileMode.CreateNew, FileAccess.Write);
            StreamWriter sw = new System.IO.StreamWriter(FS, System.Text.Encoding.Default);

            var lines = File.ReadAllLines(@"C:\WORK\data\" + "테마_url" + ".txt");

            int count = 0;
            int theme_count = 0;
            foreach (var url in lines)
            {
                if (url == "")
                    continue;

                var newurl = "https://finance.naver.com/sise/sise_group_detail.nhn?type=theme&no=" + url;
                doc = web.Load(newurl);
                if (doc == null)
                {
                    return;
                }

                // Title Extracted
                var t = doc.DocumentNode.SelectSingleNode("//title").InnerText;
                int index = t.IndexOf(':');
                string title = t.Remove(index - 1);
                sw.WriteLine(title);

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
            textBox6.Text = theme_count.ToString() + "/" + count.ToString() + " 테마 Processed";
        }
        private void button6_Click(object sender, EventArgs e)
        {
            네이버_테마(1);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            네이버_업종();
            네이버_테마(1);
            일주월();
            시가총액();
            통계_working();
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

            string[] grlines = File.ReadAllLines(@"C:\WORK\data\시총.txt", Encoding.Default);

            // 저장파일 "프돈외기" 있으면 삭제 후 재생성
            string path = @"C:\WORK\data\프돈외기.txt";
            if (File.Exists(path))
                File.Delete(path);


            // 헤드라인 기록
            //string str = "";
            //str += "종목" + "\t" + "시총" + "\t" +
            //          "프돈_평균" + "\t" + "프돈_편차" + "\t" +
            //          "외돈_평균" + "\t" + "외돈_편차" + "\t" +
            //          "기돈_평균" + "\t" + "기돈_편차";
            //sw.Write(str);


            string temp_path = @"C:\WORK\temp.txt";
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
                int 전일종가 = Pre_Processor_Class1.read_일자제시_전일종가(reference_date, 종목);
                if (전일종가 <= 0)
                {
                    str_temp += 종목 + "\n";
                    continue;
                }

                int moving_date = reference_date;

                int count = 0;
                for (int i = 0; i < 300; i++)
                {
                    moving_date = Pre_Processor_Class1.directory_분전후(moving_date, -1); // 거래익일

                    int nword = Pre_Processor_Class1.Read_Stock_Minute_LasLine(moving_date, 종목, x);
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

                double money_factor = 전일종가 / 100000000.0; // divided by 억원
                
                프돈_평균 = 프돈.Sum() / 프돈.Count * money_factor;
                프돈_편차 = Math.Sqrt(프돈.Sum(t => Math.Pow(t - 프돈_평균, 2)) / (프돈.Count - 1)) * money_factor;
                프돈_최대 *= money_factor;

                외돈_평균 = 외돈.Sum() / 외돈.Count * money_factor;
                외돈_편차 = Math.Sqrt(외돈.Sum(t => Math.Pow(t - 외돈_평균, 2)) / (외돈.Count - 1)) * money_factor;
                외돈_최대 *= money_factor;

                기돈_평균 = 기돈.Sum() / 기돈.Count * money_factor;
                기돈_편차 = Math.Sqrt(기돈.Sum(t => Math.Pow(t - 기돈_평균, 2)) / (기돈.Count - 1)) * money_factor;
                기돈_최대 *= money_factor;

                
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

            //_gl = Pre_Processor_Class1.read_시총_일정액수이상(Convert.ToInt32(textBox2.Text));
            _gl = Pre_Processor_Class1.read_그룹_네이버_업종(GL);
            int days = 20;
            double avr = 0.0, dev = 0.0;
            int 일평균거래액 = 0, 일최소거래액 = 0, 일최대거래액 = 0;
            int MaximumDate = 0;
            double MaximumPriceRiseRate = 0;
            foreach (var stock in _gl)
            {
                string dev_avr = Pre_Processor_Class1.calcurate_종목일중변동평균편차(stock, days, ref avr, ref dev, ref 일평균거래액,
                           ref 일최소거래액, ref 일최대거래액, ref MaximumDate, ref MaximumPriceRiseRate);
                a_tuple.Add(Tuple.Create(일최대거래액, MaximumDate, 일평균거래액, dev, avr, stock, (int)MaximumPriceRiseRate));
            }
            a_tuple = a_tuple.OrderByDescending(t => t.Item1).ToList(); // 분당거래액_천만원이 0인 경우 tuple 아이템 수가 0인 문제 발생





            string path = @"C:\WORK\평점.txt";
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
                string item = (string) dataGridView1.Rows[e.RowIndex].Cells[2].Value;
                string[] words = item.Split(' ');
                
                int value = Convert.ToInt32(words[0]);
                if(e.ColumnIndex == 3)
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
                string file = @"C:\WORK\" + "평점" + ".txt";
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
                Stream FS = new FileStream(@"C:\WORK\" + "평점" + ".txt", FileMode.CreateNew, FileAccess.Write);
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



      
        private void 통계_working() // MOD
        {
            // rd.read_변수();
            textBox6.Text = "통계 진행 중";

            rd.read_업종_상관(); // 업종 & 상관 : 다 읽은 후 종목은 전일 거래액 순서로 정리함

            int start_date = 20220302; // MOD
            int end_date = Convert.ToInt32(DateTime.Now.Date.ToString("yyyyMMdd"));
           
            string path = @"C:\WORK\data\통계.txt";

            if (File.Exists(path))
                File.Delete(path);

            StreamWriter sw = File.CreateText(path);

            int processing_count = 0;

            foreach (var o in g.ogl_data) // 혼합 2 종목 빠져시 to-jsb 보다 2 종목 작음
            {
                textBox6.Text = "통계 진행 : " + processing_count++.ToString();

                int count_success_read_stock_minute = 0;
                string stock = o.종목;

                var 프분 = new List<double>();
                var 거분 = new List<double>();

                var 프누 = new List<List<double>>(); 
                var 종누 = new List<List<double>>();

                for (int i = 0; i < 382; i++)
                {
                    프누.Add(new List<double>());
                    종누.Add(new List<double>());
                }

                double value = 0.0;

                int 전일종가 = rd.read_전일종가(stock);
                double money_factor = 전일종가 / g.억원;



                // find g.nCol * g.nRow maximum date and time
                // in order of descending
                for (int i = start_date; i <= end_date; i++)
                {
                    g.date = i;
                    if (stock.Contains("KODEX") || stock.Contains("혼합"))
                        continue;

                    int[,] x = new int[400, 12];
                    int nrow = rd.read_Stock_Minute(i, stock, x); // i -> date
                    if (!rd.readStockMinuteCheck(nrow, x)) // if minute data is not absolute continue
                        continue;
                    else
                        count_success_read_stock_minute++;

                    for (int j = 1; j < nrow; j++)
                    {
                        value = (double)(x[j, 4] - x[j - 1, 4]) * money_factor;
                        if (value > 0.01) // positive side only
                            프분.Add(value);
                        value = (double)(x[j, 7] - x[j - 1, 7]) * money_factor;
                        거분.Add(value);

                        int id = (x[j, 0] / 10000 - 9) * 60 + (x[j, 0] % 10000) / 100 + 1;

                        프누[id].Add(x[j, 4] * money_factor); 
                        종누[id].Add(x[j, 7] * money_factor);
                    }
                    if (프누.Count != 종누.Count)
                    {
                        MessageBox.Show("프누 & 종누 숫자 차이");
                    }
                }
                
                if (count_success_read_stock_minute == 0) // no successful minute data
                    continue;

                for (int i = 1; i < 382; i++)
                {
                    if(count_success_read_stock_minute != 프누[i].Count)
                        MessageBox.Show("프누 array 숫자 불일치");
                    if (count_success_read_stock_minute != 종누[i].Count)
                        MessageBox.Show("종누 array 숫자 불일치");
                }

                double[] 프누_avr = new double[382];
                double[] 프누_dev = new double[382];
                double[] 종누_avr = new double[382];
                double[] 종누_dev = new double[382];

                for (int i = 1; i < 382; i++)
                {
                    프누_avr[i] = 프누[i].Sum() / 프누[i].Count;
                    프누_dev[i] = Math.Sqrt(프누[i].Sum(y => Math.Pow(y - 프누_avr[i], 2)) / (프누[i].Count - 1));
                    종누_avr[i] = 종누[i].Sum() / 종누[i].Count;
                    종누_dev[i] = Math.Sqrt(종누[i].Sum(y => Math.Pow(y - 종누_avr[i], 2)) / (종누[i].Count - 1));
                }


                //g.clicked_Stock = stock;
                //ms.Naver_호가_txt(2, -1, -1, 0, 0);

                string str = stock;
                double avr = 0.0;
                double dev = 0.0;
                if (프분.Count > 10)
                {
                    avr = 프분.Sum() / 프분.Count;
                    dev = Math.Sqrt(프분.Sum(y => Math.Pow(y - avr, 2)) / (프분.Count - 1));
                }
                str += "\t" + 프분.Count;
                if (avr < 0.001)
                    str += "\t" + "0.0";
                else
                    str += "\t" + avr.ToString("#.####");
                if (dev < 0.001)
                    str += "\t" + "0.0"; 
                else
                    str += "\t" + dev.ToString("#.####");
                 
                avr = 0.0;
                dev = 0.0;
                if (거분.Count >10) 
                {
                    avr = 거분.Sum() / 거분.Count;
                    dev = Math.Sqrt(거분.Sum(y => Math.Pow(y - avr, 2)) / (거분.Count - 1));
                }
                str += "\t" + 거분.Count;
                if (avr < 0.001)
                    str += "\t" + "0.0";
                else
                    str += "\t" + avr.ToString("#.####");
                if (dev < 0.001)
                    str += "\t" + "0.0";
                else
                    str += "\t" + dev.ToString("#.####");

                sw.WriteLine("{0}", str);

                if (프분.Count < 5000)
                    wr.w(o.종목);
            }
            sw.Close();
            textBox6.Text = "통계 진행 완료";


        }



        private void 통계_old()
        {
            return;
            rd.read_변수();
            rd.read_제어();
            rd.read_업종_상관(); // 업종 & 상관 : 다 읽은 후 종목은 전일 거래액 순서로 정리함

            int start_date = Convert.ToInt32(textBox3.Text);
            int end_date = Convert.ToInt32(textBox4.Text);

            string path = @"C:\WORK\data\통계.txt";

            if (File.Exists(path))
                File.Delete(path);

            StreamWriter sw = File.CreateText(path);

            foreach (var o in g.ogl_data) // 혼합 2 종목 빠져시 to-jsb 보다 2 종목 작음
            {
                string stock = o.종목;

                var 프분_list = new List<double>();
                var 거분_list = new List<double>();

                var 프누_avr = new List<List<double>>();
                var 프누_dev = new List<List<double>>();
                var 종누_avr = new List<List<double>>();
                var 종누_dev = new List<List<double>>();

                for (int i = 0; i < 382; i++)
                {
                    프누_avr.Add(new List<double>());
                    프누_dev.Add(new List<double>());
                    종누_avr.Add(new List<double>());
                    종누_dev.Add(new List<double>());
                }

                double value = 0.0;

                int 전일종가 = rd.read_전일종가(stock);

                if (stock.Contains("KODEX") || stock.Contains("KBSTAR"))
                    continue;

                // find g.nCol * g.nRow maximum date and time
                // in order of descending
                for (int i = start_date; i <= end_date; i++)
                {
                    g.date = i;
                    if (stock.Contains("KODEX") || stock.Contains("혼합"))
                        continue;

                    int[,] x = new int[382, 12];
                    int nrow = rd.read_Stock_Minute(i, stock, x); // i -> date


                    if (nrow < 200)
                        continue;

                    for (int j = 1; j < nrow; j++)
                    {
                        if (x[j, 0] / 100 - x[j - 1, 0] / 100 != 1 || // minute difference = 1
                            x[j, 1] < -3000 || x[j, 1] > 3000 ||     // price less than 3,000, and larger than -3,000
                            x[j, 4] - x[j - 1, 4] < 0 ||
                        x[j, 7] - x[j - 1, 7] < 0 ||                        // 거분 > 0
                        x[j, 0] > 150000)                                // before 1500
                        {
                            continue;
                        }

                        value = (double)(x[j, 4] - x[j - 1, 4]) * 전일종가;
                        value /= g.억원;
                        if (value > 0.01)
                            프분_list.Add(value);
                        value = (double)(x[j, 7] - x[j - 1, 7]) * 전일종가;
                        value /= g.억원;
                        거분_list.Add(value);
                    }
                }

                g.clicked_Stock = stock;
                //ms.Naver_호가_txt(2, -1, -1, 0, 0);

                string str = stock;
                double avr = 0.0;
                double dev = 0.0;
                if (프분_list.Count > 10)
                {
                    avr = 프분_list.Sum() / 프분_list.Count;
                    dev = Math.Sqrt(프분_list.Sum(y => Math.Pow(y - avr, 2)) / (프분_list.Count - 1));
                }
                str += "\t" + 프분_list.Count.ToString();
                if (avr < 0.001)
                    str += "\t" + "0.0";
                else
                    str += "\t" + avr.ToString("#.####");
                if (dev < 0.001)
                    str += "\t" + "0.0";
                else
                    str += "\t" + dev.ToString("#.####");

                avr = 0.0;
                dev = 0.0;
                if (거분_list.Count > 10)
                {
                    avr = 거분_list.Sum() / 거분_list.Count;
                    dev = Math.Sqrt(거분_list.Sum(y => Math.Pow(y - avr, 2)) / (거분_list.Count - 1));
                }
                str += "\t" + 거분_list.Count.ToString();
                if (avr < 0.001)
                    str += "\t" + "0.0";
                else
                    str += "\t" + avr.ToString("#.####");
                if (dev < 0.001)
                    str += "\t" + "0.0";
                else
                    str += "\t" + dev.ToString("#.####");

                sw.WriteLine("{0}", str);
            }
            sw.Close();
        }



        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void 통계(object sender, EventArgs e)
        {
            통계_working();   
        }
    }
}

        //var htmlBody = doc.DocumentNode.SelectSingleNode("//tbody").InnerText;
        //DocumentNode.SelectSingleNode("//tbody").InnerText
        //var tbody = doc.DocumentNode.SelectNodes("//table[@id='type_1']//tbody");
        //foreach (var elem in tbody)
        //{
        //    //Dump only works in LinqPad
        //    elem.InnerText.ToString();
        //}
        //var result = doc.DocumentNode.SelectSingleNode("//table[@class='table table-striped']")

//var UserTable = doc.DocumentNode.SelectNodes("//div[@id='contentarea_left']/table/tr");
//    string a = UserTable[3].SelectSingleNode("td[1]").InnerText;
//    int r_c = a.IndexOf('\n');
//    a = a.Substring(0, r_c);
//foreach (var row in UserTable)
//{
//    string a = row.SelectSingleNode("td[1]").InnerText;
//    //string value = row.InnerText.Attributes["value"].Value;
//    //if (row.Attributes["data-source"] != null)
//    //{
//    //    string Source = row.Attributes["data-source"].Value;
//    //    string UserName = row.SelectSingleNode("td[@class='tbl_col1']/a[@id='UserLink']/text()").InnerText;
//    //    string Points = row.SelectSingleNode("td[@class='tbl_col2']/a[@id='PointLink']/text()").InnerText;
//    //    Console.WriteLine(Source + "\t" + UserName + "\t" + Points);
//    //}
//}

//var result = doc.DocumentNode.SelectSingleNode("//tbody")
//.Descendants("tr")
//.Skip(0)
//.Select(tr => new
//{
//    Desc = tr.SelectSingleNode("td[1]").InnerText,
//    //Val = WebUtility.HtmlDecode(tr.SelectSingleNode("td[2]").InnerText)
//})
//.ToList();

//var htmlBody = doc.DocumentNode.SelectSingleNode("//tbody");
//htmlBody.SelectSingleNode("$0");

//var nodes = doc.DocumentNode.SelectNodes("//tr"); //.Where(x => x.InnerHtml.Contains("== $0")).ToArray();

//var str = nodes[3].Elements("td").First().InnerText;

//foreach (var item in nodes)
//{


//    var stock = item.InnerText.ToString();
//    string stock_trimmed = stock.TrimEnd('*', ' ');
//    sw.WriteLine(stock_trimmed);
//    count++;
//}
//sw.WriteLine();
//    theme_count++;
//}
//sw.Close();

//textBox6.Text = theme_count.ToString() + "/" + count.ToString() + " are Processed";


//File.WriteAllText(@"C:\WORK\" + "temp" + ".txt", all_stock);

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





/*
private static void _stockmst_Received()
{


    int count = (int)_stockmst.GetHeaderValue(0);                                  // code 내 종목숫자
    for (int k = 0; k < count; k++)
    {
        string 종목명 = _stockmst2.GetDataValue(1, k);
        long 시간 = _stockmst2.GetDataValue(2, k);
        long 현재가 = _stockmst2.GetDataValue(3, k);
        long 전일대비 = _stockmst2.GetDataValue(4, k); // 하락액수
        char 상태구분 = (char)_stockmst2.GetDataValue(5, k);
        //'1' 상한 '2' 상승 '3' 보함 '4' 하한 '5' 하락 '6' 기세상한 '7' 기세상승 '8' 기세하한 '9' 기세하락
        long 시가 = _stockmst2.GetDataValue(6, k);
        long 매도호가 = _stockmst2.GetDataValue(9, k);
        long 거래량 = _stockmst2.GetDataValue(11, k);
        long 거래대금 = _stockmst2.GetDataValue(12, k); // 메뉴얼 천원 그러나 실제 만원 단위
        long 총매도잔량 = _stockmst2.GetDataValue(13, k); // 호가창 그대로 나타남
        long 매도잔량 = _stockmst2.GetDataValue(15, k); // 호가창 그대로 나타남
        float 외국인보유비율 = _stockmst2.GetDataValue(18, k);
        long 전일거래량 = _stockmst2.GetDataValue(20, k);
        float 체결강도 = _stockmst2.GetDataValue(21, k); // 누적체결강도 나의 그래프와 동일
        long 순간체결량 = _stockmst2.GetDataValue(22, k); // 2주 등 불필요
        char 체결가비교 = (char)_stockmst2.GetDataValue(23, k); // 'O' 매도 'B' 매수 불필요
        char 호가비교 = (char)_stockmst2.GetDataValue(24, k);
        char 동시호가구분 = (char)_stockmst2.GetDataValue(25, k); // '1' 동시호가 '2' 장중
        long 예상체결가 = _stockmst2.GetDataValue(26, k); // 현재가 217000, 예상체결가는 시가인 221000 ...이해 곤란
        long 예상체결가전일대비 = _stockmst2.GetDataValue(27, k); // 0으로 나오는 데 시가를 비교하는 것으로 보임
        long 예상체결가상태구분 = _stockmst2.GetDataValue(28, k); // 메뉴얼은 '1' 상한 '2' 상승 등으로 나오는 데 long 이해 곤란
        long 예상체결가거래량 = _stockmst2.GetDataValue(29, k); // 1098 출력 ... 이해 곤란 
    }
}
*/


/* List<string> alist = new List<string>();
            alist.Add((price / 100000000).ToString());
            alist.Add(temp_stockname);
             List<Employee> employeeList = new List<Employee>();
            employeeList.Add(new Employee(1, "Roger"));
            시총.Add(new employee(시총 / 100000000, temp_stockname));
            */



/*var items = new List<Tuple<int, string>> { };
            foreach (var name in sL)
            {
                string path = @"C:\WORK\분\" + date.ToString() +
                    "\\" + name + ".txt";

                if (!File.Exists(path))
                {
                    continue;
                }


                int nline = File.ReadAllLines(path).Length;  // MOD
                if (endtime > nline)
                {
                    endtime = nline; ;
                }
               

                string line = File.ReadLines(path).Skip(endtime -1).Take(1).First(); // endtime not included for draw
                string[] words = line.Split(' ');
                int savedvalue = Convert.ToInt32(words[col]); // use end value
                /*
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

                   
                    //if (Convert.ToInt32(words[0]) > 0)
                     //   savedvalue = Convert.ToInt32(words[col]);
                    //else
                      //  break;
                        

                    savedvalue = Convert.ToInt32(words[col]); // use end value

    items.Add(Tuple.Create(savedvalue, name));
            }
            items = items.OrderByDescending(t => t.Item1).ToList();*/



