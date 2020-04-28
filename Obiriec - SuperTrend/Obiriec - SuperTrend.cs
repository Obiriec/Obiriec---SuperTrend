using System;
using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, AccessRights = AccessRights.FullAccess)]
    //change to FullAccess to use alerts
    public class ObiriecSuperTrend : Indicator
    {
        [Parameter(DefaultValue = 50)]
        public int LongPeriod { get; set; }

        [Parameter(DefaultValue = 10)]
        public double LongMultiplier { get; set; }

        [Parameter(DefaultValue = 10)]
        public int ShortPeriod { get; set; }

        [Parameter(DefaultValue = 3)]
        public double ShortMultiplier { get; set; }

        [Output("UpLong", Color = Colors.DodgerBlue, PlotType = PlotType.Points, Thickness = 3)]
        public IndicatorDataSeries UpTrendLong { get; set; }

        [Output("DownLong", Color = Colors.Red, PlotType = PlotType.Points, Thickness = 3)]
        public IndicatorDataSeries DownTrendLong { get; set; }

        [Output("UpTrendShort", Color = Colors.SpringGreen, PlotType = PlotType.Points, Thickness = 2)]
        public IndicatorDataSeries UpTrendShort { get; set; }

        [Output("DownTrendShort", Color = Colors.Orange, PlotType = PlotType.Points, Thickness = 2)]
        public IndicatorDataSeries DownTrendShort { get; set; }

        private IndicatorDataSeries _upBufferlong;
        private IndicatorDataSeries _downBufferlong;
        private IndicatorDataSeries _upBuffershort;
        private IndicatorDataSeries _downBuffershort;
        private AverageTrueRange _averageTrueRangelong;
        private AverageTrueRange _averageTrueRangeshort;
        private int[] _trendlong;
        private int[] _trendshort;
        private bool _changeofTrendlong;
        private bool _changeofTrendshort;

        protected override void Initialize()
        {
            //Algoline here
            _trendlong = new int[1];
            _trendshort = new int[1];
            _upBufferlong = CreateDataSeries();
            _downBufferlong = CreateDataSeries();
            _upBuffershort = CreateDataSeries();
            _downBuffershort = CreateDataSeries();
            _averageTrueRangelong = Indicators.AverageTrueRange(LongPeriod, MovingAverageType.WilderSmoothing);
            _averageTrueRangeshort = Indicators.AverageTrueRange(ShortPeriod, MovingAverageType.WilderSmoothing);
        }

        public override void Calculate(int index)
        {
            // Init
            UpTrendLong[index] = double.NaN;
            DownTrendLong[index] = double.NaN;
            UpTrendShort[index] = double.NaN;
            DownTrendShort[index] = double.NaN;

            double median = (MarketSeries.High[index] + MarketSeries.Low[index]) / 2;
            double atrlong = _averageTrueRangelong.Result[index];
            double atrshort = _averageTrueRangeshort.Result[index];

            _upBufferlong[index] = median + LongMultiplier * atrlong;
            _downBufferlong[index] = median - LongMultiplier * atrlong;
            _upBuffershort[index] = median + ShortMultiplier * atrshort;
            _downBuffershort[index] = median - ShortMultiplier * atrshort;


            if (index < 1)
            {
                _trendlong[index] = 1;
                return;
            }

            Array.Resize(ref _trendlong, _trendlong.Length + 1);

            // Main Logic
            if (MarketSeries.Close[index] > _upBufferlong[index - 1])
            {
                _trendlong[index] = 1;
                if (_trendlong[index - 1] == -1)
                    _changeofTrendlong = true;
            }
            else if (MarketSeries.Close[index] < _downBufferlong[index - 1])
            {
                _trendlong[index] = -1;
                if (_trendlong[index - 1] == -1)
                    _changeofTrendlong = true;
            }
            else if (_trendlong[index - 1] == 1)
            {
                _trendlong[index] = 1;
                _changeofTrendlong = false;
            }
            else if (_trendlong[index - 1] == -1)
            {
                _trendlong[index] = -1;
                _changeofTrendlong = false;
            }

            if (_trendlong[index] < 0 && _trendlong[index - 1] > 0)
                _upBufferlong[index] = median + (LongMultiplier * atrlong);
            else if (_trendlong[index] < 0 && _upBufferlong[index] > _upBufferlong[index - 1])
                _upBufferlong[index] = _upBufferlong[index - 1];

            if (_trendlong[index] > 0 && _trendlong[index - 1] < 0)
                _downBufferlong[index] = median - (LongMultiplier * atrlong);
            else if (_trendlong[index] > 0 && _downBufferlong[index] < _downBufferlong[index - 1])
                _downBufferlong[index] = _downBufferlong[index - 1];

            // Draw Indicator
            if (_trendlong[index] == 1)
            {

                UpTrendLong[index] = _downBufferlong[index];
                if (_changeofTrendlong)
                {
                    UpTrendLong[index - 1] = DownTrendLong[index - 1];
                    _changeofTrendlong = false;
                }
            }
            else if (_trendlong[index] == -1)
            {

                DownTrendLong[index] = _upBufferlong[index];
                if (_changeofTrendlong)
                {
                    DownTrendLong[index - 1] = UpTrendLong[index - 1];
                    _changeofTrendlong = false;
                }

            }

            if (index < 1)
            {
                _trendshort[index] = 1;
                return;
            }

            Array.Resize(ref _trendshort, _trendshort.Length + 1);

            // Main Logic
            if (MarketSeries.Close[index] > _upBuffershort[index - 1])
            {
                _trendshort[index] = 1;
                if (_trendshort[index - 1] == -1)
                    _changeofTrendshort = true;
            }
            else if (MarketSeries.Close[index] < _downBuffershort[index - 1])
            {
                _trendshort[index] = -1;
                if (_trendshort[index - 1] == -1)
                    _changeofTrendshort = true;
            }
            else if (_trendshort[index - 1] == 1)
            {
                _trendshort[index] = 1;
                _changeofTrendshort = false;
            }
            else if (_trendshort[index - 1] == -1)
            {
                _trendshort[index] = -1;
                _changeofTrendshort = false;
            }

            if (_trendshort[index] < 0 && _trendshort[index - 1] > 0)
                _upBuffershort[index] = median + (ShortMultiplier * atrshort);
            else if (_trendshort[index] < 0 && _upBuffershort[index] > _upBuffershort[index - 1])
                _upBuffershort[index] = _upBuffershort[index - 1];

            if (_trendshort[index] > 0 && _trendshort[index - 1] < 0)
                _downBuffershort[index] = median - (ShortMultiplier * atrshort);
            else if (_trendshort[index] > 0 && _downBuffershort[index] < _downBuffershort[index - 1])
                _downBuffershort[index] = _downBuffershort[index - 1];

            // Draw Indicator
            if (_trendshort[index] == 1)
            {

                UpTrendShort[index] = _downBuffershort[index];
                if (_changeofTrendshort)
                {
                    UpTrendShort[index - 1] = DownTrendShort[index - 1];
                    _changeofTrendshort = false;
                }
            }
            else if (_trendshort[index] == -1)
            {

                DownTrendShort[index] = _upBuffershort[index];
                if (_changeofTrendshort)
                {
                    DownTrendShort[index - 1] = UpTrendShort[index - 1];
                    _changeofTrendshort = false;
                }
            }

            // Test Notifications on chart
            {
                var value = MarketSeries.Close[index - 1];
                if (Functions.HasCrossedAbove(_upBufferlong, value, 1))
                {
                    var name = "BuyLong";
                    var high = MarketSeries.High[index - 1];
                    var text = high.ToString();
                    var xPos = index - 1;
                    var yPos = high;
                    var vAlign = VerticalAlignment.Top;
                    var hAlign = HorizontalAlignment.Right;
                    ChartObjects.DrawText(name, text, xPos, yPos, vAlign, hAlign, Colors.DodgerBlue);
                    //Alertline here
                }
            }

            {
                var value = MarketSeries.Close[index - 1];
                if (Functions.HasCrossedBelow(_downBufferlong, value, 1))
                {
                    var name = "SellLong";
                    var low = MarketSeries.Low[index - 1];
                    var text = low.ToString();
                    var xPos = index - 1;
                    var yPos = low;
                    var vAlign = VerticalAlignment.Bottom;
                    var hAlign = HorizontalAlignment.Right;
                    ChartObjects.DrawText(name, text, xPos, yPos, vAlign, hAlign, Colors.Red);
                    //Alertline here
                }
            }

            // Test Notifications on chart
            {
                var value = MarketSeries.Close[index - 1];
                if (Functions.HasCrossedAbove(_upBuffershort, value, 1))
                {
                    var name = "BuyShort";
                    var high = MarketSeries.High[index - 1];
                    var text = high.ToString();
                    var xPos = index - 1;
                    var yPos = high;
                    var vAlign = VerticalAlignment.Top;
                    var hAlign = HorizontalAlignment.Left;
                    ChartObjects.DrawText(name, text, xPos, yPos, vAlign, hAlign, Colors.SpringGreen);
                    //Alertline here
                }
            }

            {
                var value = MarketSeries.Close[index - 1];
                if (Functions.HasCrossedBelow(_downBuffershort, value, 1))
                {
                    var name = "SellShort";
                    var low = MarketSeries.Low[index - 1];
                    var text = low.ToString();
                    var xPos = index - 1;
                    var yPos = low;
                    var vAlign = VerticalAlignment.Bottom;
                    var hAlign = HorizontalAlignment.Left;
                    ChartObjects.DrawText(name, text, xPos, yPos, vAlign, hAlign, Colors.Orange);
                    //Alertline here
                }
            }

        }
    }
}
