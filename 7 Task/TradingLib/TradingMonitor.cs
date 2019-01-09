using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using DataLoading;

namespace TradingLib
{
	public class TradingMonitor : IDisposable
	{
		public delegate void ReceiveCandleEvent(Candle candle, double? indicatorValue);
		public event ReceiveCandleEvent OnReceiveCandle;

		private IDataSource dataSource;
		private Thread monitorThread;
		private IIndicatorCalculator indicatorCalculator;

		private StockReportStream dataStream;
		private IIndicatorSerie indicatorSerie;

		public TradingMonitor(IDataSource source, IIndicatorCalculator calculator)
		{
			dataSource = source;
			indicatorCalculator = calculator;
		}

		public void Dispose()
		{
			monitorThread.Abort();
		}

		public void RunMonitor()
		{
			dataStream = dataSource.GetReport();
			indicatorSerie = indicatorCalculator.Calculate(dataStream);

			monitorThread = new Thread(new ThreadStart(UpdateMonitor));
			monitorThread.Start();
		}

		protected void UpdateMonitor()
		{
			while (dataStream.Candles.Count != 0)
			{
                Candle[] williams = new Candle[15];
                for (int i = 0; i < 15; i++)
                {                   
                    williams[i] = dataStream.PopCandle();
                }
                decimal[] highList = williams.Select(x => (decimal)x.High).ToArray();
                decimal[] lowList = williams.Select(x => (decimal)x.Low).ToArray();
                Candle result = new Candle();
                result.High = highList.Max();
                result.Low = lowList.Min();
                result.Close = williams.Last().Close;
                result.Open = williams.First().Open;
                OnReceiveCandle?.Invoke(result, indicatorSerie.PopValue());

				Thread.Sleep(1000);
			}
		}

	}
}
