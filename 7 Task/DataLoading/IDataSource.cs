using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLoading
{
    public interface IDataSource
    {
		StockReportStream GetReport();
    }
}
