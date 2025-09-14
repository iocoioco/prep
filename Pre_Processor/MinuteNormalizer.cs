using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Pre_Processor
{
    public static class MinuteNormalizer
    {
        // ===== 설정 =====
        public static string RootDir = @"C:\BJS\분";
        public static string OutFile = @"C:\BJS\분\norm_result.txt";
        public static int MinGapSec = 50, MaxGapSec = 70;
        public static int StartCalcHHMM = 904;
        public static int StartFeedHHMM = 903;
        public static int NasdaqJumpAbs = 30;

        private static readonly string[] Targets = {
            "KODEX 레버리지",
            "KODEX 코스닥150레버리지"
        };

        // ===== 데이터 모델 =====
        private sealed class MinuteRow
        {
            public int TimeHMS;
            public double Price;
            public double Prog;
            public double Inst;
            public double Forn;
            public double Individ;
            public double BuyMult;
            public double SellMult;
            public double Nasdaq;
            public double Pension;
        }

        private sealed class DeltaRow
        {
            public int TimeHMS;
            public double DPrice, DProg, DInst, DForn, DIndiv, DPension;
            public double DDiffMult, DSumMult, DNasdaq;
        }

        // ===== 엔트리 =====
        public static void Run(string startYmd, string endYmd)
        {
            DateTime s = DateTime.ParseExact(startYmd, "yyyyMMdd", CultureInfo.InvariantCulture);
            DateTime e = DateTime.ParseExact(endYmd, "yyyyMMdd", CultureInfo.InvariantCulture);

            var sb = new StringBuilder();
            sb.AppendLine("Minute Normalization Results");
            sb.AppendLine($"StartDate={startYmd}, EndDate={endYmd}");
            sb.AppendLine();

            foreach (var target in Targets)
            {
                var allDeltas = new List<DeltaRow>();

                for (var day = s.Date; day <= e.Date; day = day.AddDays(1))
                {
                    var dayFolder = Path.Combine(RootDir, day.ToString("yyyyMMdd"));
                    if (!Directory.Exists(dayFolder)) continue;

                    var file = Directory.EnumerateFiles(dayFolder, "*", SearchOption.TopDirectoryOnly)
                                        .FirstOrDefault(p => Path.GetFileName(p).Contains(target));
                    if (file == null) continue;

                    var rows = LoadRows(file);
                    var deltas = ToDeltas(rows);
                    allDeltas.AddRange(deltas);
                }

                if (allDeltas.Count == 0) continue;

                sb.AppendLine(target);

                sb.AppendLine("--- Standard Deviation ---");
                AppendStats(sb, allDeltas, useMad: false);

                sb.AppendLine();
                sb.AppendLine("--- MAD ---");
                AppendStats(sb, allDeltas, useMad: true);

                sb.AppendLine();
            }

            var utf8bom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
            File.WriteAllText(OutFile, sb.ToString(), utf8bom);
        }

        // ===== 데이터 처리 =====
        private static readonly Regex Splitter = new Regex(@"[,\s]+", RegexOptions.Compiled);

        private static List<MinuteRow> LoadRows(string path)
        {
            var list = new List<MinuteRow>();
            foreach (var raw in File.ReadLines(path, Encoding.UTF8))
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;
                if (raw.StartsWith("#")) continue;

                var tok = Splitter.Split(raw.Trim());
                if (tok.Length < 12) continue;

                if (!int.TryParse(tok[0], out int t)) continue;

                list.Add(new MinuteRow
                {
                    TimeHMS = t,
                    Price = ParseD(tok[1]),
                    Prog = ParseD(tok[3]),
                    Inst = ParseD(tok[4]),
                    Forn = ParseD(tok[5]),
                    Individ = ParseD(tok[6]),
                    BuyMult = ParseD(tok[8]),
                    SellMult = ParseD(tok[9]),
                    Nasdaq = ParseD(tok[10]),
                    Pension = ParseD(tok[11])
                });
            }
            list.Sort((a, b) => a.TimeHMS.CompareTo(b.TimeHMS));
            return list;
        }

        private static double ParseD(string s)
        {
            return double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : 0.0;
        }

        private static List<DeltaRow> ToDeltas(List<MinuteRow> src)
        {
            var dst = new List<DeltaRow>();
            for (int i = 1; i < src.Count; i++)
            {
                var a = src[i - 1];
                var b = src[i];

                int gap = (int)(ToTimeSpan(b.TimeHMS) - ToTimeSpan(a.TimeHMS)).TotalSeconds;
                if (gap < MinGapSec || gap > MaxGapSec) continue;

                if (HMS_to_HHMM(a.TimeHMS) < StartFeedHHMM) continue;
                if (HMS_to_HHMM(b.TimeHMS) < StartCalcHHMM) continue;

                var d = new DeltaRow
                {
                    TimeHMS = b.TimeHMS,
                    DPrice = b.Price - a.Price,
                    DProg = b.Prog - a.Prog,
                    DInst = b.Inst - a.Inst,
                    DForn = b.Forn - a.Forn,
                    DIndiv = b.Individ - a.Individ,
                    DPension = b.Pension - a.Pension,
                    DDiffMult = (b.BuyMult - a.BuyMult) - (b.SellMult - a.SellMult),
                    DSumMult = (b.BuyMult - a.BuyMult) + (b.SellMult - a.SellMult),
                    DNasdaq = b.Nasdaq - a.Nasdaq
                };
                if (Math.Abs(d.DNasdaq) >= NasdaqJumpAbs) d.DNasdaq = 0.0;
                dst.Add(d);
            }
            return dst;
        }

        private static int HMS_to_HHMM(int hms)
        {
            int hh = hms / 10000;
            int mm = (hms / 100) % 100;
            return hh * 100 + mm;
        }

        private static TimeSpan ToTimeSpan(int hms)
        {
            int hh = hms / 10000;
            int mm = (hms / 100) % 100;
            int ss = hms % 100;
            return new TimeSpan(hh, mm, ss);
        }

        // ===== 통계 및 출력 =====
        private static void AppendStats(StringBuilder sb, List<DeltaRow> deltas, bool useMad)
        {
            WriteOne(sb, "Δ가격", deltas.Select(v => v.DPrice), useMad);
            WriteOne(sb, "Δ프로그램", deltas.Select(v => v.DProg), useMad);
            WriteOne(sb, "Δ외국인", deltas.Select(v => v.DForn), useMad);
            WriteOne(sb, "Δ기관", deltas.Select(v => v.DInst), useMad);
            WriteOne(sb, "Δ개인", deltas.Select(v => v.DIndiv), useMad);
            WriteOne(sb, "Δ연금", deltas.Select(v => v.DPension), useMad);
            WriteOne(sb, "Δ배수차", deltas.Select(v => v.DDiffMult), useMad);
            WriteOne(sb, "Δ배수합", deltas.Select(v => v.DSumMult), useMad);
            WriteOne(sb, "Δ나스닥", deltas.Select(v => v.DNasdaq), useMad);
        }

        private static void WriteOne(StringBuilder sb, string name, IEnumerable<double> src, bool useMad)
        {
            var list = src.ToList();
            int n = list.Count;
            if (n == 0)
            {
                sb.AppendLine($"{name}: (no data)");
                return;
            }

            double avr = list.Average();
            double dev = useMad ? MAD(list) : Std(list, avr);

            sb.AppendLine($"{name}: avr={avr:F4}, dev={dev:F4}");

            if (dev > 0)
            {
                int cPos3 = 0, cNeg3 = 0, cPos4 = 0, cNeg4 = 0, cPos5 = 0, cNeg5 = 0;
                foreach (var v in list)
                {
                    var z = (v - avr) / dev;
                    if (z > 3) cPos3++;
                    if (z < -3) cNeg3++;
                    if (z > 4) cPos4++;
                    if (z < -4) cNeg4++;
                    if (z > 5) cPos5++;
                    if (z < -5) cNeg5++;
                }

                sb.AppendLine($"    z>+3 : {cPos3}회 ({(100.0 * cPos3 / n):F1}%)");
                sb.AppendLine($"    z<-3 : {cNeg3}회 ({(100.0 * cNeg3 / n):F1}%)");
                sb.AppendLine($"    z>+4 : {cPos4}회 ({(100.0 * cPos4 / n):F1}%)");
                sb.AppendLine($"    z<-4 : {cNeg4}회 ({(100.0 * cNeg4 / n):F1}%)");
                sb.AppendLine($"    z>+5 : {cPos5}회 ({(100.0 * cPos5 / n):F1}%)");
                sb.AppendLine($"    z<-5 : {cNeg5}회 ({(100.0 * cNeg5 / n):F1}%)");
            }
        }

        private static double Std(List<double> x, double mean)
        {
            int n = x.Count;
            if (n <= 1) return 0.0;
            double s = 0;
            foreach (var v in x) { var d = v - mean; s += d * d; }
            return Math.Sqrt(s / (n - 1));
        }

        private static double MAD(List<double> x)
        {
            if (x.Count == 0) return 0.0;
            x.Sort();
            double med = Median(x);
            var absDev = x.Select(v => Math.Abs(v - med)).ToList();
            absDev.Sort();
            return 1.4826 * Median(absDev);
        }

        private static double Median(List<double> x)
        {
            int n = x.Count;
            if (n == 0) return 0.0;
            if (n % 2 == 1) return x[n / 2];
            return 0.5 * (x[n / 2 - 1] + x[n / 2]);
        }
    }
}
