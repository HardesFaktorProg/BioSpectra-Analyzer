// Program name:
// (EN): BioSpectra Analyzer: Automated Analysis of Bioorganic Compounds from Spectral Data
// (UA): Система аналізу біоорганічних сполук за спектральними даними
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Globalization; 

namespace BioSpectraAnalyzer
{
	public partial class MainWindow : Window
	{
		public PlotModel SpectrumModel { get; private set; }
		public string ResultText { get; set; }

		// Явное создание списка — совместимо с C# 7.3
		private List<Tuple<double, double>> spectrumData = new List<Tuple<double, double>>();

		public MainWindow()
		{
			InitializeComponent();
			CreateEmptyPlot();
			DataContext = this;
		}

		// Пустой график
		private void CreateEmptyPlot()
		{
			SpectrumModel = new PlotModel
			{
				Title = "Spectral Data Visualization",
				Background = OxyColors.Black,
				TextColor = OxyColors.White,
				PlotAreaBorderColor = OxyColors.White
			};

			SpectrumModel.Axes.Add(new LinearAxis
			{
				Position = AxisPosition.Bottom,
				Title = "Wavelength (nm)",
				TitleColor = OxyColors.LightGray,
				AxislineColor = OxyColors.Gray,
				MajorGridlineStyle = LineStyle.Dash,
				TextColor = OxyColors.White
			});

			SpectrumModel.Axes.Add(new LinearAxis
			{
				Position = AxisPosition.Left,
				Title = "Intensity (a.u.)",
				TitleColor = OxyColors.LightGray,
				AxislineColor = OxyColors.Gray,
				MajorGridlineStyle = LineStyle.Dash,
				TextColor = OxyColors.White
			});

			SpectrumPlot.Model = SpectrumModel;
		}

		// 📂 Загрузка спектра
		private void BtnLoad_Click(object sender, RoutedEventArgs e)
		{
			var open = new OpenFileDialog
			{
				Filter = "Text or CSV files|*.txt;*.csv",
				Title = "Open Spectral Data File"
			};

			if (open.ShowDialog() == true)
			{
				try
				{
					var lines = File.ReadAllLines(open.FileName);
					var list = new List<Tuple<double, double>>();

					foreach (var line in lines)
					{
						if (string.IsNullOrWhiteSpace(line)) continue;
						var parts = line.Split(new[] { ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
						if (parts.Length < 2) continue;

						double w, iVal;
						if (double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out w) &&
							double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out iVal))
						{
							list.Add(new Tuple<double, double>(w, iVal));
						}
					}

					spectrumData = list;
					DrawSpectrum();

					ResultText = "✅ Spectrum loaded successfully";
					DataContext = null;
					DataContext = this;
				}
				catch (Exception ex)
				{
					MessageBox.Show("Error reading file:\n" + ex.Message);
				}
			}
		}

		// 📈 Построение графика
		private void DrawSpectrum()
		{
			var lineSeries = new LineSeries
			{
				Title = "Intensity",
				Color = OxyColors.Cyan,
				StrokeThickness = 2
				// Без Smooth — чтобы не зависеть от версии OxyPlot
			};

			for (int idx = 0; idx < spectrumData.Count; idx++)
			{
				var point = spectrumData[idx];
				lineSeries.Points.Add(new DataPoint(point.Item1, point.Item2));
			}

			SpectrumModel.Series.Clear();
			SpectrumModel.Series.Add(lineSeries);
			SpectrumModel.InvalidatePlot(true);
		}

		// 🔍 Анализ и определение соединения
		private void BtnAnalyze_Click(object sender, RoutedEventArgs e)
		{
			if (spectrumData == null || spectrumData.Count == 0)
			{
				MessageBox.Show("Please load spectrum data first!");
				return;
			}

			// Простой поиск локальных максимумов
			var peaks = new List<double>();
			for (int i = 1; i < spectrumData.Count - 1; i++)
			{
				var prev = spectrumData[i - 1];
				var cur = spectrumData[i];
				var next = spectrumData[i + 1];

				if (cur.Item2 > prev.Item2 && cur.Item2 > next.Item2)
				{
					peaks.Add(cur.Item1);
				}
			}

			// Шаблоны пиков (IR/общие)
			var templates = new Dictionary<string, double[]>
			{
				{ "Alcohol",          new double[] { 3200, 1040 } },
				{ "Ketone",           new double[] { 1715 } },
				{ "Amine",            new double[] { 3300, 1600 } },
				{ "Carboxylic Acid",  new double[] { 1700, 2500 } },
				{ "Ester",            new double[] { 1740, 1200 } },
				{ "Alkene",           new double[] { 1650, 3080 } }
			};

			string bestMatch = "Unknown";
			int bestScore = 0;
			const double tolerance = 20.0;

			foreach (var kv in templates)
			{
				var name = kv.Key;
				var tpl = kv.Value;

				int score = 0;
				for (int j = 0; j < tpl.Length; j++)
				{
					for (int k = 0; k < peaks.Count; k++)
					{
						if (Math.Abs(peaks[k] - tpl[j]) < tolerance)
						{
							score++;
							break;
						}
					}
				}

				if (score > bestScore)
				{
					bestScore = score;
					bestMatch = name;
				}
			}

			ResultText = "🧪 Detected compound: " + bestMatch;
			DataContext = null;
			DataContext = this;

			// Отметим пики красными маркерами
			DrawSpectrum();
			var peakSeries = new ScatterSeries
			{
				MarkerType = MarkerType.Circle,
				MarkerSize = 4,
				MarkerFill = OxyColors.Red
			};

			for (int m = 0; m < peaks.Count; m++)
			{
				double x = peaks[m];

				// ищем ближайшую точку по X, чтобы взять Y
				double y = 0.0;
				double bestDx = double.MaxValue;

				for (int n = 0; n < spectrumData.Count; n++)
				{
					double dx = Math.Abs(spectrumData[n].Item1 - x);
					if (dx < bestDx)
					{
						bestDx = dx;
						y = spectrumData[n].Item2;
					}
				}

				// ВАЖНО: для ScatterSeries используем ScatterPoint, не DataPoint
				peakSeries.Points.Add(new OxyPlot.Series.ScatterPoint(x, y));
			}

			SpectrumModel.Series.Add(peakSeries);
			SpectrumModel.InvalidatePlot(true);
		}
	}
}
