using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace VisualTimelineDemo
{
    public partial class TimeLineVisualizer : UserControl
    {
        #region events
        public event EventHandler<TimeLineChangedEventArgs> TimeLineChanged;
        public event EventHandler<DetailRequestedEventArgs> DetailRequested;
        #endregion

        #region private Members
        DateTime? _startDate;
        DateTime? _endDate;
        
        //Width of a single day as represented on the plot canvas.
        //This will change depending on the time range. Set by DrawAxis()
        double _widthOfADay;

        //Width that represents difference between the beginning of the day and start time of the time range.
        //This can be computed given _widthOfADay and _StartTime, but since
        //we are using it to figure out where to move _mouseHighlight to on MouseMove,
        //figured we'd store it instead of computing each time. Set by DrawAxis()
        double _axisOffset;
        
        bool _isMouseCaptured;
        Point _mousePosition;
        private readonly Rectangle _mouseHighlight;
        private IEnumerable<TimeLineData> _itemsSource;

        #endregion

        const double MINIMUM_TIMELINE_HEIGHT = 20;
        const double GRIDLINE_WIDTH = 0.1;
        readonly SolidColorBrush _gridlineBrush = new SolidColorBrush { Color = Color.FromArgb(0x99, 0x00, 0x00, 0x00) };

        enum Months
        {
            Jan, Feb, Mar, Apr, May, Jun, Jul, Aug, Sep, Oct, Nov, Dec
        }

        #region Public Properties

        public bool IsSingleSelect { get; set; }

        public IEnumerable<TimeLineData> ItemsSource
        {
            get { return _itemsSource; }
            set
            {
                _itemsSource = value;
                VisualizeItemsSource();
            }
        }

        public DateTime? StartDate
        {
            get { return _startDate; }
            set { _startDate = value; }
        }

        public DateTime? EndDate
        {
            get { return _endDate; }
            set { _endDate = value; }
        }

        #endregion

        public TimeLineVisualizer()
        {
            InitializeComponent();
            MouseWheel += PlotCanvasMouseWheelMoved;
            StartDate = DateTime.Now.AddDays(-14);
            EndDate = DateTime.Now.AddDays(14);
            _mouseHighlight = new Rectangle { Fill = new SolidColorBrush { Color = Colors.White } };
        }


        #region Event Raisers
        private void OnDetailRequested(DetailRequestedEventArgs e)
        {
            if (DetailRequested != null)
                DetailRequested(this, e);
        }

        void OnTimeLineChanged()
        {
            if (TimeLineChanged != null)
            {
                TimeLineChanged(this, new TimeLineChangedEventArgs { StartTime = _startDate.Value, EndTime = _endDate.Value });
            }
        }
        #endregion
        
        #region UI Event Handlers
        private void PlotCanvasMouseWheelMoved(object sender, MouseWheelEventArgs e)
        {
            if (_widthOfADay > 0)
            {
                bool shouldTimeLineChange = true;
                //get the current date represented by mouse position
                DateTime currentDate = _startDate.Value.AddDays(_mousePosition.X / _widthOfADay);
                //get the ratio that says how much closer the current date is to start of timeline, than it is to
                //end of timeline. We will use this ratio later to decide what the start/end dates should be.
                double ratio = (currentDate - _startDate.Value).TotalDays / (_endDate.Value - _startDate.Value).TotalDays;
                if (e.Delta < 0) //scroll out
                {

                    _startDate = _startDate.Value.AddDays(e.Delta/100);
                    _endDate = _endDate.Value.AddDays(Math.Abs(e.Delta/100));
                }
                else if (e.Delta > 0) //zoom in
                {
                    DateTime start = _startDate.Value.AddDays(e.Delta/100);
                    DateTime end = _endDate.Value.AddDays(e.Delta/100 * -1);
                    if (start >= end)
                        shouldTimeLineChange = false;
                    else
                    {
                        _startDate = start;
                        _endDate = end;
                    }
                }
                if (shouldTimeLineChange)
                {
                    //take the date range, and distribute it around the current date according to
                    //the ratio we calculated earlier.
                    double rangeInDays = (_endDate.Value - _startDate.Value).TotalDays;
                    _startDate = currentDate.AddDays(-ratio * rangeInDays);
                    _endDate = currentDate.AddDays((1 - ratio) * rangeInDays);
                    OnTimeLineChanged();
                }
            }
        }

        private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PlotCanvas.Width = PlotScrollViewer.Width;
            PlotCanvas.Height = PlotScrollViewer.Height;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isMouseCaptured = true;
            PlotCanvas.CaptureMouse();
            Cursor = Cursors.Hand;
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PlotCanvas.ReleaseMouseCapture();
            _isMouseCaptured = false;
            Cursor = Cursors.Arrow;
            _mousePosition = e.GetPosition(PlotCanvas);
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_widthOfADay > 0)
            {
                if (_isMouseCaptured)
                {
                    double deltaDays = (_mousePosition.X - e.GetPosition(PlotCanvas).X) / _widthOfADay;
                    _startDate = _startDate.Value.AddDays(deltaDays);
                    _endDate = _endDate.Value.AddDays(deltaDays);
                    OnTimeLineChanged();
                    //                PlotScrollViewer.ScrollToVerticalOffset(PlotScrollViewer.VerticalOffset + _mousePosition.Y - e.GetPosition(PlotCanvas).Y);
                }
                else
                {
                    HighlightDay(e.GetPosition(PlotCanvas));
                }
            }
            else
            {
                Canvas_SizeChanged(null, null);
            }
            _mousePosition = e.GetPosition(PlotCanvas);
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_itemsSource != null)
                VisualizeItemsSource();
        }

        void TlDetailRequested(object sender, DetailRequestedEventArgs e)
        {
            if (IsSingleSelect)
            {
                OnDetailRequested(new DetailRequestedEventArgs
                {
                    Data = ((TimeLine)sender).ItemsSource.DataContext
                });
            }
            else
            {
                OnDetailRequested(new DetailRequestedEventArgs
                {
                    Data = from tl in PlotCanvas.Children
                           where tl is TimeLine && ((TimeLine)tl).ItemsSource.IsHighlighted
                           select ((TimeLine)tl).ItemsSource.DataContext
                });
            }
        }

        void TlSelected(object sender, EventArgs e)
        {
            var tls = from tl in PlotCanvas.Children
                      where tl is TimeLine
                          && ((TimeLine)tl).ItemsSource.IsHighlighted
                          && !tl.Equals(sender)
                      select (TimeLine)tl;
            foreach (TimeLine tl in tls)
                tl.Deselect();
        }

        void TlGlobalDeselectRequested(object sender, EventArgs e)
        {
            var hts = from tl in PlotCanvas.Children
                      where tl is TimeLine && ((TimeLine)tl).ItemsSource.IsHighlighted
                      select tl;
            foreach (TimeLine ht in hts)
                ht.ItemsSource.IsHighlighted = false;
        }
        #endregion

        #region Helper Methods
        private void VisualizeItemsSource()
        {
            //we may change the size of the plot depending on number of elements we have to draw.
            //Dont want to fire the size-changed event and redraw stuff, so unhook event handler
            PlotCanvas.SizeChanged -= Canvas_SizeChanged;
            String[] currHighlight = null;
            if (PlotCanvas.Children.Count > 0)
            {
                currHighlight = (from tl in PlotCanvas.Children
                                    where
                                        tl is TimeLine && ((TimeLine)tl).ItemsSource.IsHighlighted
                                    select ((TimeLine)tl).ItemsSource.Label).ToArray();
            }

            //remove all existing stuff on the plot canvas
            PlotCanvas.Children.Clear();
            //if the elements can be more than the minimum specified height and still fit into the
            //viewport of scrollviewer, do that. Else grow the plotcanvas height to (elementCount * minimumHeight)
            PlotCanvas.Height = PlotScrollViewer.ActualHeight;
            double totalHeight = PlotCanvas.Height;
            double tlHeight = 0;
            if (_itemsSource != null)
            {
                tlHeight = totalHeight/_itemsSource.Count();
            }
            if (tlHeight < MINIMUM_TIMELINE_HEIGHT)
            {
                tlHeight = MINIMUM_TIMELINE_HEIGHT;
                PlotCanvas.Height = _itemsSource ==null?0: MINIMUM_TIMELINE_HEIGHT * _itemsSource.Count();
            }
            //Draw Axis and the background shading inside plotcanvas for axis-related stuff (weekends, etc.)
            DrawAxis();

            //Create a TimeLine for each item in source, and position it on the plot canvas one below the other.
            double i = 0;
            if (_itemsSource != null)
                foreach (TimeLineData d in _itemsSource)
                {
                    if (currHighlight != null && currHighlight.Contains(d.Label))
                        d.IsHighlighted = true;
                    var tl = new TimeLine
                                      {
                                          TimeLineStart = _startDate,
                                          TimeLineEnd = _endDate,
                                          Height = tlHeight,
                                          Width = PlotCanvas.ActualWidth,
                                          ItemsSource = d
                                      };
                    tl.DetailRequested += TlDetailRequested;
                    if (IsSingleSelect)
                        tl.Selected += TlSelected;
                    tl.GlobalDeselectRequested += TlGlobalDeselectRequested;
                    Canvas.SetLeft(tl, 0);
                    //Position the TimeLine below the last one.
                    Canvas.SetTop(tl, i * tlHeight);
                    PlotCanvas.Children.Add(tl);
                    i++;
                }
            PlotCanvas.UpdateLayout();
            //We are done. Let the event handler play again.
            PlotCanvas.SizeChanged += Canvas_SizeChanged;
        }

        private void HighlightDay(Point point)
        {
            //Get the lower bound of the day where the point is, set it as the left position of 'highlight' rectangle
            Canvas.SetLeft(_mouseHighlight, Math.Floor((point.X - (_widthOfADay - _axisOffset))/_widthOfADay)*_widthOfADay + _widthOfADay - _axisOffset);
        }

        private void DrawAxis()
        {
            //Clear all existing elements in axis canvas
            AxisCanvas.Children.Clear();
            //number of days in the range. Includes partial days.
            double numDays = (_endDate.Value - _startDate.Value).TotalDays;
            double totalWidth = PlotCanvas.ActualWidth;
            //Set the axis to span the whole plot canvas
            AxisLine.X2 = totalWidth;
            AxisCanvas.Children.Add(AxisLine);
            //Get the Width of a day as represented in the axis. This value wil be used to compute
            //the date/time info represented by a point on the plot canvas until the next time axis changes
            _widthOfADay = totalWidth / numDays;
            _mouseHighlight.Width = _widthOfADay;
            _mouseHighlight.Height = PlotCanvas.Height;
            Canvas.SetLeft(_mouseHighlight, 0 - _widthOfADay);
            PlotCanvas.Children.Add(_mouseHighlight);
            //The first day may be a partial day. If it is, account for this on the axis.
            //axisOffset represents the part of the day before the axis starts
            _axisOffset = (_startDate.Value.TimeOfDay - _startDate.Value.Date.TimeOfDay).TotalDays * _widthOfADay;
            for (double i = 0; i <= numDays; i++)
            {
                //day we are dealing with
                DateTime currentDate = _startDate.Value.AddDays(i);

                //'left' is the point on the axis that represents the start of the day
                double left = _widthOfADay * i - _axisOffset;

                //draw the vertical hash on axis
                var l = new Line { Y2 = 10, StrokeThickness = 0.1,
                    Stroke = new SolidColorBrush { Color = Colors.Black } };
                Canvas.SetLeft( l, left);
                Canvas.SetTop(l, 40);
                AxisCanvas.Children.Add(l);

                //Date and DayOfWeek are written on the axis if the width of a day is more than 20
                if (_widthOfADay > 20)
                {
                    //Write date on the axis, starting 3 units from center of the day width
                    var t = new TextBlock { Text = currentDate.Day.ToString(), FontSize = 9, FontWeight = FontWeights.Thin, Foreground = new SolidColorBrush { Color = Colors.Gray } };
                    Canvas.SetLeft(t, left + _widthOfADay / 2 - 3);
                    Canvas.SetTop(t, 30);
                    AxisCanvas.Children.Add(t);

                    //Write dayOfWeek on the axis, starting 3 units from center of the day width
                    var d = new TextBlock { Text = currentDate.DayOfWeek.ToString().Substring(0,3), FontSize = 9, FontWeight = FontWeights.Medium, Foreground = new SolidColorBrush{Color=Colors.LightGray } };
                    Canvas.SetLeft(d, left + _widthOfADay / 2 - 8);
                    Canvas.SetTop(d, 15);
                    AxisCanvas.Children.Add(d);
                }

                //if it is the first day of the month or the second day on the axis (because the first day may be
                //a partial and hence we won't see the value), write the month on the axis
                if (i == 1 || currentDate.Day == 1)
                {
                    var tmonth = new TextBlock { Text = Enum.GetName(typeof(Months), currentDate.Month - 1), FontSize = 10, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush { Color = Colors.Gray } };
                    if (currentDate.Month == 1)
                        tmonth.Text += " " + currentDate.Year;
                    Canvas.SetLeft(tmonth, left);
                    Canvas.SetTop(tmonth, 2);
                    AxisCanvas.Children.Add(tmonth);
                }
                //Draw the vertical gridline on the plot canvas for the start of the day.
                DrawGridLine(left);

                //If the day is today, do some special stuff
                if (currentDate.Date == DateTime.Now.Date)
                {
                    //color the day in plot canvas
                    ColorColumn(left, _widthOfADay, (Brush)Application.Current.Resources["TimeLineTodayColumn"]);
                    //Draw a colored rectangle on the axis that spans the entire day
                    var r = new Rectangle
                    {
                        Height = AxisCanvas.ActualHeight,
                        Width = _widthOfADay,
                        Fill = new SolidColorBrush { Color = Colors.Gray },
                        Opacity = .1
                    };
                    Canvas.SetLeft(r, left);
                    ToolTipService.SetToolTip(r, "Today");
                    AxisCanvas.Children.Add(r);
                }
                //if it is weekend and not today, color it differently.
                else if (currentDate.DayOfWeek == DayOfWeek.Sunday || currentDate.DayOfWeek == DayOfWeek.Saturday)
                    ColorColumn(left, _widthOfADay, (Brush)Application.Current.Resources["TimeLineWeekendColumn"]);
            }
            AxisCanvas.UpdateLayout();
        }

        void ColorColumn(double left, double width, Brush brush)
        {
            var r = new Rectangle
            {
                Fill = brush,
                Height = PlotCanvas.Height,
                Width = width
            };
            Canvas.SetLeft(r, left);
            PlotCanvas.Children.Add(r);
            return;
        }

        void DrawGridLine(double left)
        {
            var l = new Line
            {
                StrokeThickness = GRIDLINE_WIDTH,
                Stroke = _gridlineBrush,
                Y2 = PlotCanvas.Height
            };
            Canvas.SetLeft(l, left);
            PlotCanvas.Children.Add(l);
            return;
        }
        #endregion
    }
}
