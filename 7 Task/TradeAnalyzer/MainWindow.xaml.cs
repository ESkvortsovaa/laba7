using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SciChart;
using SciChart.Charting.Model.DataSeries;
using DataLoading;
using TradingLib;
using SciChart.Data.Model;
using System.Windows.Threading;
using Microsoft.Win32;

namespace TradeAnalyzer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private OhlcDataSeries<DateTime, double> ohlcSeries;
		private XyDataSeries<DateTime, double> adSeries;

		private TradingMonitor tradingMonitor;

		private int candlesCount = 0;

		public MainWindow()
		{
			InitializeComponent();
            PreviousIndicator = 0;
			ReloadChartSeries();
			StockChart.XAxis.AutoRange = SciChart.Charting.Visuals.Axes.AutoRange.Once;
            StockChart2.XAxis.AutoRange = StockChart.XAxis.AutoRange;
			LoadDataSource(new XLSXDataLoader(@"C:\Users\Геннадий\Desktop\PDP\4GENA\Attestation_2\prices.xlsx"));
			this.Loaded += OnLoaded;
		}

		private void ReloadChartSeries()
		{
			ohlcSeries = new OhlcDataSeries<DateTime, double>() { SeriesName = "Candles", FifoCapacity = 10000 };
			adSeries = new XyDataSeries<DateTime, double>() { SeriesName = "AD", FifoCapacity = 10000 };

			CandleSeries.DataSeries = ohlcSeries;
			ADSeries.DataSeries = adSeries;
		}

        private decimal CalculateIndicator (Candle candle)
        {          
            return (candle.High-candle.Close)/(candle.High-candle.Low);
        }
        private double PreviousIndicator { get; set; }
		private void AddNewCandle(Candle candle, double? indicatorValue)
		{
			using (ohlcSeries.SuspendUpdates())
			using (adSeries.SuspendUpdates())
			{
				ohlcSeries.Append(candle.Time, (double)candle.Open, (double)candle.High, (double)candle.Low, (double)candle.Close);

                //if (indicatorValue != null)
                    //if (candlesCount > 1)
                    adSeries.Append(candle.Time, (double)CalculateIndicator(candle) + PreviousIndicator);
                PreviousIndicator = (double)CalculateIndicator(candle) + PreviousIndicator;

               // adSeries.Append(candle.Time, candle.Volume*1000);

                candlesCount++;
                
				StockChart.XAxis.VisibleRange = new IndexRange(candlesCount - 30, candlesCount);
                StockChart2.XAxis.VisibleRange = StockChart.XAxis.VisibleRange;
			}
		}

		private void OnCandleReceived(Candle candle, double? indicatorValue)
		{
			StockChart.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { AddNewCandle(candle, indicatorValue); }));
            //StockChart2.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { AddNewCandle(candle, 40); }));
		}

		private void MainMenu_File_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "XLSX files (*.xlsx)|*.xlsx|JSON files (*.json)|*.json";

			if (openFileDialog.ShowDialog() == true)
			{
				try
				{
					string extension = System.IO.Path.GetExtension(openFileDialog.FileName);

					if (extension == ".xlsx")
						LoadDataSource(new XLSXDataLoader(openFileDialog.FileName));
					else if (extension == ".json")
						LoadDataSource(new JSONDataLoader(openFileDialog.FileName));
					else
						throw new Exception();
				}
				catch (Exception error)
				{
					MessageBox.Show("Произошла ошибка загрузки данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		private void LoadDataSource(IDataSource source)
		{
			if (this.tradingMonitor != null) {
				tradingMonitor.Dispose();
				tradingMonitor = null;
			}

			tradingMonitor = new TradingMonitor(source, new Williams());
			tradingMonitor.OnReceiveCandle += OnCandleReceived;

			ReloadChartSeries();

			tradingMonitor.RunMonitor();
		}

		private void MainMenu_Exit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
		}

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
