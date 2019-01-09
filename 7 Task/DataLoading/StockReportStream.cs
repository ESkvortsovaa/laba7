using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLoading
{
	public class StockReportStream
	{
		public Queue<Candle> Candles { get; private set; } = new Queue<Candle>();

		public StockReportStream()
		{

		}

		public void PushCandle(Candle candle)
		{
			Candles.Enqueue(candle);
		}

		public Candle PopCandle()
		{
			return Candles.Dequeue();
		}

		public Candle PeekCandle()
		{
			return Candles.Peek();
		}
	}
}
