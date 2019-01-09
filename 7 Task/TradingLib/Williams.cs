using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLoading;
using Trady;
using Trady.Core;
using Trady.Analysis;
using Trady.Analysis.Indicator;
using Trady.Core.Infrastructure;

namespace TradingLib
{
    public class Williams : IIndicatorCalculator
    {
       

        public IIndicatorSerie CountWilliams(StockReportStream report)
        {
            IIndicatorSerie sarSerie = new SimpleIndicatorSerie();
            var candlesSerie = report.Candles.ToList();
            //double[] highList = candlesSerie.Select(x => (double)x.High).ToArray();
            //double[] lowList = candlesSerie.Select(x => (double)x.Low).ToArray();
            int CandlesCount = report.Candles.Count;
            var Candles = report.Candles;
            int lastiteration = CandlesCount % 14;//остаток от деления
            int iterationsamount = 1 + CandlesCount / 14;
            //Queue<Candle> local = new Queue<Candle>();
            int border = 14;
            for (int i = 0; i < iterationsamount; i++)
            {
                Queue<DataLoading.Candle> local = new Queue<DataLoading.Candle>();
                if (i == iterationsamount - 1)
                    border = lastiteration;
                for (int j = 0; j < border; j++)
                {
                    local.Enqueue(Candles.Dequeue());
                }
                decimal[] highList = local.Select(x => (decimal)x.High).ToArray();
                decimal[] lowList = local.Select(x => (decimal)x.Low).ToArray();
                //var last = local.Last();
                double r = (double)((highList.Max() - local.Last().Close) / (highList.Max() - lowList.Min())) * 100;
                sarSerie.PushValue(r);
                local.Clear();
            }
            return sarSerie;
        }

        public IIndicatorSerie Calculate(StockReportStream report)
        {
            Dictionary<int, int> v;
            //IIndicatorSerie adSerie = new SimpleIndicatorSerie();
            IIndicatorSerie sarSerie = new SimpleIndicatorSerie();
            var candlesSerie = report.Candles.ToList();

            List<double> differences = new List<double>();

            for (int i = 0; i < candlesSerie.Count; i++)
            {
                double difference = (double)(candlesSerie[i].High - candlesSerie[i].Low);
                differences.Add(difference);
            }

            double stDev = CalculateDeviation(differences);

            double?[] sarArr = new double?[candlesSerie.Count];

            double[] highList = candlesSerie.Select(x => (double)x.High).ToArray();
            double[] lowList = candlesSerie.Select(x => (double)x.Low).ToArray();

            int beginIndex = 1;
            for (int i = 0; i < candlesSerie.Count; i++)
            {
                if (candlesSerie[i].High == 0 || candlesSerie[i].Low == 0)
                {
                    sarArr[i] = 0;
                    beginIndex++;
                }
                else
                {
                    break;
                }
            }

            int sign0 = 1, sign1 = 0;
            double xpoint = highList[beginIndex - 1], xpoint1 = 0;
            double apoint = AccelerationFactor, apoint1 = 0;
            double lmin, lmax;
            sarArr[beginIndex - 1] = lowList[beginIndex - 1] - stDev;

            for (int i = beginIndex; i < candlesSerie.Count; i++)
            {
                sign1 = sign0;
                xpoint1 = xpoint;
                apoint1 = apoint;

                lmin = (lowList[i - 1] > lowList[i]) ? lowList[i] : lowList[i - 1];
                lmax = (highList[i - 1] > highList[i]) ? highList[i - 1] : highList[i];

                if (sign1 == 1)
                {
                    sign0 = (lowList[i] > sarArr[i - 1]) ? 1 : -1;
                    xpoint = (lmax > xpoint1) ? lmax : xpoint1;
                }
                else
                {
                    sign0 = (highList[i] < sarArr[i - 1]) ? -1 : 1;
                    xpoint = (lmin > xpoint1) ? xpoint1 : lmin;
                }

                if (sign0 == sign1)
                {
                    sarArr[i] = sarArr[i - 1] + (xpoint1 - sarArr[i - 1]) * apoint1;
                    apoint = (apoint1 == MaximumAccelerationFactor) ? MaximumAccelerationFactor : (AccelerationFactor + apoint1);

                    if (sign0 == 1)
                    {
                        apoint = (xpoint > xpoint1) ? apoint : apoint1;
                        sarArr[i] = (sarArr[i] > lmin) ? lmin : sarArr[i];
                    }
                    else
                    {
                        apoint = (xpoint < xpoint1) ? apoint : apoint1;
                        sarArr[i] = (sarArr[i] > lmax) ? sarArr[i] : lmax;
                    }
                }
                else
                {
                    apoint = AccelerationFactor;
                    sarArr[i] = xpoint;
                }
            }

            foreach (var sarValue in sarArr)
                sarSerie.PushValue(sarValue);

            return sarSerie;
        }

        private double CalculateDeviation(IEnumerable<double> values)
        {
            double ret = 0;

            if (values.Count() > 0)
            {
                double avg = values.Average();
                double sum = values.Sum(d => Math.Pow(d - avg, 2));

                ret = Math.Sqrt((sum) / (values.Count() - 1));
            }

            return ret;
        }
        public double AccelerationFactor = 0.02;
        public double MaximumAccelerationFactor = 0.2;
    }
}
