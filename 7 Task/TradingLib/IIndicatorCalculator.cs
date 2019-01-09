using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLoading;

namespace TradingLib
{
    public interface IIndicatorCalculator
    {
		IIndicatorSerie Calculate(StockReportStream report);
    }
}
