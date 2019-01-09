using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OfficeOpenXml;

namespace DataLoading
{
	public class XLSXDataLoader : IDataSource
	{
		private string _documentPath;

		public XLSXDataLoader(string path)
		{
			_documentPath = path;
		}

		public StockReportStream GetReport()
		{
			StockReportStream report = new StockReportStream();

			using (ExcelPackage xlPackage = new ExcelPackage(new FileInfo(_documentPath)))
			{
				var myWorksheet = xlPackage.Workbook.Worksheets.First();
				var totalRows = myWorksheet.Dimension.End.Row;
				var totalColumns = myWorksheet.Dimension.End.Column;

				for (int rowNum = 2; rowNum <= totalRows; rowNum++) 
				{
					var row = myWorksheet.Cells[rowNum, 1, rowNum, totalColumns]
						.Select(c => c.Value == null ? string.Empty : c.Value.ToString()).ToArray();

					string rawDateTime = row[2] + " " + row[3];

					DateTime dateTime = DateTime.ParseExact(rawDateTime, "yyyyMMdd HHmmss", null);

					Candle candle = new Candle() {
						High = int.Parse(row[5]),
						Low = int.Parse(row[6]),
						Open = int.Parse(row[4]),
						Close = int.Parse(row[7]),
						Time = dateTime,
						TimeStamp = ((DateTimeOffset)dateTime).ToUnixTimeMilliseconds(),
                        Volume = int.Parse(row[8])
					};

					report.PushCandle(candle);
				}
			}

			return report;
		}
	}
}
