using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OfficeOpenXml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataLoading
{
	public class JSONDataLoader : IDataSource
	{
		private string _documentPath;

		public JSONDataLoader(string path)
		{
			_documentPath = path;
		}

		public StockReportStream GetReport()
		{
			StockReportStream report = new StockReportStream();

			dynamic jsonObject = JsonConvert.DeserializeObject(File.ReadAllText(_documentPath));
			
			var high = ((JArray)jsonObject.h);
			var low = ((JArray)jsonObject.l);

			var open = ((JArray)jsonObject.o);
			var close = ((JArray)jsonObject.c);
			var timestamps = ((JArray)jsonObject.t);
			
			for (int i = 0; i < timestamps.Count; i++)
			{
				Candle candle = new Candle()
				{
					High = (decimal)high[i],
					Low = (decimal)low[i],
					Open = (decimal)open[i],
					Close = (decimal)close[i],
					Time = DateTimeOffset.FromUnixTimeSeconds((long)timestamps[i]).UtcDateTime,
					TimeStamp = (long)timestamps[i],
				};

				report.PushCandle(candle);

			}

			return report;
		}
	}
}
