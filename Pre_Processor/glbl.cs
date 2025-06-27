using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms;

namespace Pre_Processor
{
    public class g
    {
        public static double HUNDRED = 100.0;
        public static double 천만원 = 10000000.0;
        public static double 억원 = 100000000.0;
        public static int date;
        public static int MAX_ROW = 382; // XX 382 -> 500
        public static List<string> dl = new List<string>();   // selected stocks for display
        public static bool connected = true;
        public static int stocks_per_marketeye = 200;
        public static object lockObject = new object(); // BLOCK

        public static int ogl_data_next = 0;
        public static List<g.stock> ogl_data = new List<g.stock>();
        public static List<string> sl = new List<string>();   // selected single stock list from eval_stock

        public static List<g.group_data> oGL_data = new List<group_data>();
        public class group_data
        {
            public string title;
            public List<string> stocks = new List<string>();

            public double 총점;
            public double 푀누, 종누, 분푀, 배차, 가증, 분거, 상순, 저순;
            public double average_price;
        }
        public static List<string> KODEX4 = new List<string>();   // 클릭된 종목, Toggle로 선택 & 취소
        public static List<string> 지수보유관심종목 = new List<string>();
        public static List<string> 지수종목 = new List<string>();
        public static List<string> 보유종목 = new List<string>();
        public static List<string> 관심종목 = new List<string>();

        
        public static int array_size = 45; // ERROR
        public class stock
        {
            public bool included = false;

            public string 종목;

            public string code; //0
            public class score
            {
                public double dev, mkc, avr;
                public double 돌파, 눌림;
                public double 가연, 가분, 가틱, 가반, 가지, 가위;
                public double 수연, 수지, 수위;
                public double 강연, 강지, 강위;
                public double 푀분, 프틱, 프지, 프퍼, 프일;
                public double 거분, 거틱, 거일;
                public double 배차, 배반, 배합;

                public double 급락, 잔잔;

                public double 그룹;

                public double 총점;
            }
            public score 점수 = new score();

            public class level
            {
                public double 돌파, 눌림, 가반, 가지, 강지, 배반, 프퍼, 퍼센, 프지, 프가, 급락, 잔잔;
            }
            public level 정도 = new level();



            public int from_time, to_time;


            //1 시간ulong hhmm
            public char 전일대비부호; // 2 char
            public long 전일대비; // 3 long
            public long 현재가; //4 long
            public long 시초가; //5 long  
            public int 시초;
            public long 전고가; //6 long
            public int 전고;
            public long 전저가; //7 long
            public int 전저;

            public long 매도1호가; // 8 long
            public long 매수1호가; // 9 long

            public ulong 거래량; // 10 ulong
                              //public ulong 거래액_원; // 11 ulong
            public int 전일거래액_천만원; // marketeye not provide, calculated from "일"

            //public char 장구분; // 12 char '0' 장전 '1' 동시호가 '2' 장중
            public ulong 총매도호가잔량; //13 ulong
            public ulong 총매수호가잔량; //14 ulong
            public int 최우선매도호가잔량; //15 (ulong) converted to int
            public int 최우선매수호가잔량; //16 (ulong) converted to int
                                  //public ulong 전일거래량; //22 ulong
            public long 전일종가; //23 long
                              //public double 체결강도; //24 float

            public long 예상체결가; //28 long
            public ulong 예상체결수량; //31 ulong

            public char 시간외단일대비부호; //36 char +, -
            public long 시간외단일전일대비; //37 long, 36 필히 하여야 함
            public long 시간외단일현재가; //38 long
            public ulong 시간외단일거래대금; //45 ulonglong

            public double 수급과장배수 = 1;
            public long 당일프로그램순매수량; // 116 long
            public long 당일외인순매수량; //118 long

            public long 당일기관순매수량; //120 long
                                  //public long 전일외국인순매수; //121 long

            //public long 전일기관순매수; //122 long
            public ulong 공매도수량; //127 ulong



            public int[] StopLoss = new int[2];

            public double 시총;

            public int 시간;
            public int 가격;

            public int 수급;
            //public doubl종가기준추정누적거래액_천만원;

            public double 체강;

            public int 보유량;
            public double 수익률;
            public double 전수익률;
            public double 장부가;

            public long 평가금액; // Error, if use int instead of long
            public long 손익단가; // Error, if use int instead of long


            public string dev_avr;
            public double avr;
            public double dev;

            public int max_dealt; // in testing, these are diplayed in textBox 10, 11,, 12 etc
            public int min_dealt;
            public int avr_dealt;
            public ulong 일평균거래량;
            public int 일평균거래액;

            public char 시장구분; // P 코스피, D 코스닥

            public int nrow = 0;
            public int[,] x = new int[MAX_ROW, 12];



            // 틱 데이터
            public int[] 틱의시간 = new int[array_size]; // 틱의시간    // 호가창 tT
            public int[] 틱의가격 = new int[array_size]; // 틱의가격    // 호가창
            public int[] 틱의수급 = new int[array_size]; // 틱의수급
            public int[] 틱의체강 = new int[array_size]; // 틱의체강
            public int[] 틱매수량 = new int[array_size]; // 틱매수량
            public int[] 틱매도량 = new int[array_size]; // 틱매도량
            public int[] 틱매수배 = new int[array_size]; // 틱매수배
            public int[] 틱매도배 = new int[array_size]; // 틱매도배

            public int[] 틱배수차 = new int[array_size];  // 틱배수차
            public int[] 틱배수합 = new int[array_size];  // 틱배수합
            public int[] 틱프외퍼 = new int[array_size];  // 틱프외퍼

            public int[] 틱프로량 = new int[array_size]; // 틱프로량
            public int[] 틱프돈천 = new int[array_size]; // 틱프돈천

            public int[] 틱외인량 = new int[array_size]; // 틱외인량
            public int[] 틱외돈천 = new int[array_size]; // 틱외돈천

            public int[] 틱거돈천 = new int[array_size]; // 틱거돈천

            public int[] 틱최우선매도호가잔량 = new int[array_size]; // 최우선매도호가잔량
            public int[] 틱최우선매수호가잔량 = new int[array_size]; // 최우선매수호가잔량




            public int 종거천;
            public int 프거천; // 
            public int 외누천; // 
            public int 기누천; // 
            public int 매수호가거래액_백만원; // ok
            public int 매도호가거래액_백만원; // ok
            public int 분당가격차; // ok


            //public int 분거천;
            //public int 분프천;
            //public int 분외천;
            //public int 배수차;
            //public int 배수합;
            public int 매수배;
            public int 매도배;


            public int[] 분프로천 = new int[array_size]; // 분프로천
            public int[] 분외인천 = new int[array_size]; // 분외인천 ?
            public int[] 분거래천 = new int[array_size]; // 분거래천
            public int[] 분매수배 = new int[array_size]; // 분매수배
            public int[] 분매도배 = new int[array_size]; // 분매도배
            public int[] 분배수차 = new int[array_size];  // 분배수차
            public int[] 분배수합 = new int[array_size];  // 분배수차


            public class 돌파
            {
                public int pass_level = 0;

                public int high;
                public int high_time;
                public int high_id;

                public int passed_high;
                public int passed_high_time;
                public int passed_high_id;

                public int low;
                public int low_time;
                public int low_id;
                public bool low_exist = false;

                public int long_high;
                public bool long_high_passed = false;
            }
            public 돌파 pass = new 돌파();

            public int oGL_sequence_id; // assigned in the program, but not used 

            

        }
    }
}
