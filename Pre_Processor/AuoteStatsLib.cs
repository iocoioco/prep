using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace QuoteStatsLib
{
    // 누적 평균/분산 계산기 (Welford)
    internal class RunningStats
    {
        public long Count;
        public double Mean;
        public double M2; // Sum of squares of differences from the current mean

        public void Add(double x)
        {
            Count++;
            double delta = x - Mean;
            Mean += delta / Count;
            double delta2 = x - Mean;
            M2 += delta * delta2;
        }

        public double VarianceSample()
        {
            return Count > 1 ? M2 / (Count - 1) : 0.0; // 표본분산
        }

        public double StdSample()
        {
            return Math.Sqrt(VarianceSample());
        }
    }

    // 결과 모델
    public class QuoteStats
    {
        public string Stock;
        public int DaysScanned;   // 스캔한 날짜 폴더 수
        public int FilesFound;    // 실제 파일이 있었던 날짜 수
        public long RowsUsed;     // 사용된 데이터 행 수

        public double BestAskMean, BestAskStd;
        public double BestBidMean, BestBidStd;
        public double TotalAskMean, TotalAskStd;
        public double TotalBidMean, TotalBidStd;
    }

    public static class QuoteStatsCalculator
    {
        // rootDir 예: @"C:\BJS\호가변동자료"
        // stockName 예: "DB하이텍" (확장자 없이 파일명이 이 이름이거나, .txt 붙은 파일까지 모두 시도)
        public static QuoteStats Compute(string rootDir, string stockName)
        {
            var result = new QuoteStats { Stock = stockName };

            if (string.IsNullOrWhiteSpace(rootDir) || string.IsNullOrWhiteSpace(stockName) || !Directory.Exists(rootDir))
                return result; // 모두 0 반환

            // 준비
            var rxWs = new Regex(@"\s+");
            var ci = CultureInfo.InvariantCulture;

            var bestAsk = new RunningStats();
            var bestBid = new RunningStats();
            var totalAsk = new RunningStats();
            var totalBid = new RunningStats();

            string[] dateDirs;
            try
            {
                dateDirs = Directory.GetDirectories(rootDir);
            }
            catch
            {
                return result; // 접근 실패 등: 0 반환
            }

            foreach (var dir in dateDirs)
            {
                // YYYYMMDD 폴더만 스캔 (이름이 8자리 숫자)
                string leaf = Path.GetFileName(dir);
                if (!IsYyyyMmDd(leaf)) continue;

                result.DaysScanned++;

                // 파일 경로 후보: [stockName], [stockName].txt
                string p1 = Path.Combine(dir, stockName);
                string p2 = Path.Combine(dir, stockName + ".txt");
                string path = File.Exists(p1) ? p1 : (File.Exists(p2) ? p2 : null);
                if (path == null) continue;

                result.FilesFound++;

                try
                {
                    using (var sr = new StreamReader(path))
                    {
                        string line;
                        bool isFirst = true; // 첫 줄은 항상 헤더 → 건너뜀

                        while ((line = sr.ReadLine()) != null)
                        {
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            if (isFirst) { isFirst = false; continue; } // 헤더 스킵

                            string[] parts = rxWs.Split(line.Trim());
                            // 기대: HHmm bestAsk bestBid totalAsk totalBid → 최소 5토큰
                            if (parts.Length < 5) continue;

                            // 시간은 무시(parts[0])
                            double vBestAsk, vBestBid, vTotalAsk, vTotalBid;

                            if (!TryParseDouble(parts[1], ci, out vBestAsk)) continue;
                            if (!TryParseDouble(parts[2], ci, out vBestBid)) continue;
                            if (!TryParseDouble(parts[3], ci, out vTotalAsk)) continue;
                            if (!TryParseDouble(parts[4], ci, out vTotalBid)) continue;

                            bestAsk.Add(vBestAsk);
                            bestBid.Add(vBestBid);
                            totalAsk.Add(vTotalAsk);
                            totalBid.Add(vTotalBid);
                            result.RowsUsed++;
                        }
                    }
                }
                catch
                {
                    // 해당 파일 읽기 실패는 무시하고 다음 날짜로
                }
            }

            // 집계 결과 작성 (행이 0이면 자동으로 0)
            result.BestAskMean = bestAsk.Mean;
            result.BestAskStd = bestAsk.StdSample();

            result.BestBidMean = bestBid.Mean;
            result.BestBidStd = bestBid.StdSample();

            result.TotalAskMean = totalAsk.Mean;
            result.TotalAskStd = totalAsk.StdSample();

            result.TotalBidMean = totalBid.Mean;
            result.TotalBidStd = totalBid.StdSample();

            return result;
        }

        private static bool IsYyyyMmDd(string s)
        {
            if (string.IsNullOrEmpty(s) || s.Length != 8) return false;
            for (int i = 0; i < 8; i++)
            {
                if (s[i] < '0' || s[i] > '9') return false;
            }
            return true;
        }

        private static bool TryParseDouble(string text, CultureInfo ci, out double value)
        {
            // 정수/실수 모두 허용 (공백, 콤마 없는 단순 숫자 가정)
            return double.TryParse(text, System.Globalization.NumberStyles.Any, ci, out value);
        }
    }
}
