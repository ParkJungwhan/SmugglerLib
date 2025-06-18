using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using HandyControl.Tools.Extension;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace SmugControl.Views
{
    /// <summary>
    /// WidgetGroupView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WidgetGroupView : UserControl
    {
        public List<ISeries> Series { get; set; } = [
            new LineSeries<double>
            {
                Values = [7, 2, 7, 2, 7, 2],
                Fill = null,
                GeometrySize = 0,
                LineSmoothness = 1
            }
        ];

        public WidgetGroupView()
        {
            InitializeComponent();

            DataContext = new GroupViewModel();
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(WidgetGroupView), new PropertyMetadata(false));

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IsSelected = !IsSelected;
        }

        public static readonly DependencyProperty ServerStateProperty =
           DependencyProperty.Register(nameof(ServerState), typeof(ServerState), typeof(WidgetGroupView), new PropertyMetadata(ServerState.Normal));

        public ServerState ServerState
        {
            get => (ServerState)GetValue(ServerStateProperty);
            set => SetValue(ServerStateProperty, value);
        }
    }

    public class GroupViewModel
    {
        private readonly Random _random = new();
        private readonly List<DateTimePoint> _values = [];
        private readonly DateTimeAxis _customAxis;

        public ObservableCollection<ISeries> Series { get; set; }

        public Axis[] XAxes { get; set; }

        public object Sync { get; } = new object();

        public bool IsReading { get; set; } = true;

        public GroupViewModel()
        {
            Series = [new LineSeries<DateTimePoint>{
                Values = _values,
                Fill = null,
                GeometryFill = null,
                GeometryStroke = null,
                LineSmoothness = 1}
            ];

            _customAxis = new DateTimeAxis(TimeSpan.FromSeconds(1), Formatter)
            {
                CustomSeparators = GetSeparators(),
                AnimationsSpeed = TimeSpan.FromMilliseconds(0),
                //AnimationsSpeed = TimeSpan.FromSeconds(1),
                SeparatorsPaint = new SolidColorPaint(SKColors.Black.WithAlpha(100))
            };

            XAxes = [_customAxis];

            _ = ReadData();
        }

        private async Task ReadData()
        {
            // to keep this sample simple, we run the next infinite loop in a real application you
            // should stop the loop/task when the view is disposed

            while (IsReading)
            {
                await Task.Delay(1000);

                // Because we are updating the chart from a different thread we need to use a lock
                // to access the chart data. this is not necessary if your changes are made on the
                // UI thread.
                lock (Sync)
                {
                    _values.Add(new DateTimePoint(DateTime.Now, _random.Next(0, 10)));
                    if (_values.Count > 250) _values.RemoveAt(0);

                    // we need to update the separators every time we add a new point
                    _customAxis.CustomSeparators = GetSeparators();
                }
            }
        }

        private static double[] GetSeparators()
        {
            var now = DateTime.Now;

            return [
                now.AddSeconds(-25).Ticks,
                now.AddSeconds(-20).Ticks,
                now.AddSeconds(-15).Ticks,
                now.AddSeconds(-10).Ticks,
                now.AddSeconds(-5).Ticks,
                now.Ticks
                ];
        }

        private static string Formatter(DateTime date)
        {
            var secsAgo = (DateTime.Now - date).TotalSeconds;

            return secsAgo < 1 ?
                "now" : "";// $"{secsAgo:N0}s ago";
        }
    }
}