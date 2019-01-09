using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLoading
{
	public class Candle
	{
		public long TimeStamp { get; set; }
		public decimal High { get; set; }
		public decimal Low { get; set; }
		public decimal Open { get; set; }
		public decimal Close { get; set; }
		public DateTime Time { get; set; }
        public int Volume { get; set; }
	}

}
