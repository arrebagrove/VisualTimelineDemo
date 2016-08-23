using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace VisualTimelineDemo
{
    public partial class TimeLine : UserControl
    {
        public event EventHandler<DetailRequestedEventArgs> DetailRequested;
        public event EventHandler GlobalDeselectRequested;
        public event EventHandler Selected;
        public event EventHandler Deselected;

        private bool _IsMouseOnTimeLineBorder;

        private TimeLineData _ItemsSource;
        public TimeLineData ItemsSource
        {
            get { return _ItemsSource; }
            set
            {
                if (_ItemsSource != null)
                {
                    _ItemsSource.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(_ItemsSource_PropertyChanged);
                }
                _ItemsSource = value;
                _ItemsSource_PropertyChanged(_ItemsSource, null);
                _ItemsSource.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_ItemsSource_PropertyChanged);
                VisualizeItemsSource();
            }
        }

        public void _ItemsSource_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (((TimeLineData)sender).IsHighlighted)
            {
                TimeLineBorder.Background = (Brush)this.Resources["SelectedTimeLineBrush"];
            }
            else
            {
                TimeLineBorder.Background = (Brush)this.Resources["TimeLineBackground"];
            }
        }

        private DateTime? _TimeLineStart;

        public DateTime? TimeLineStart
        {
            get { return _TimeLineStart; }
            set
            {
                _TimeLineStart = value;
                VisualizeItemsSource();
            }
        }

        private DateTime? _TimeLineEnd;

        public DateTime? TimeLineEnd
        {
            get { return _TimeLineEnd; }
            set
            {
                _TimeLineEnd = value;
                VisualizeItemsSource();
            }
        }

        public TimeLine()
        {
            InitializeComponent();
        }

        public void Select()
        {
            _ItemsSource.IsHighlighted = true;
            OnSelected();
        }

        public void Deselect()
        {
            _ItemsSource.IsHighlighted = false;
            OnDeselected();
        }

        void VisualizeItemsSource()
        {
            //make sure we have all the values needed, before attempting to visualize
            if (_TimeLineEnd != null && _TimeLineStart != null && _ItemsSource != null)
            {
                //Width of the event: (event-length / timeline-length) * width of root canvas
                double w = (_ItemsSource.End - _ItemsSource.Start).TotalDays / (_TimeLineEnd.Value - _TimeLineStart.Value).TotalDays * RootCanvas.ActualWidth;
                TimeLineBorder.Width = (w > 0) ? w : 0;
                TimeLineBorder.Height = (RootCanvas.ActualHeight > 3) ? RootCanvas.ActualHeight - 3 : RootCanvas.ActualHeight;
                RootCanvas.Background = _ItemsSource.BackGround;
                Canvas.SetTop(TimeLineBorder, 1.5);
                //place where event starts in the timeline: (event-start - timeline-start)/(timeline-end - event-start) * width of root canvas
                double borderLeft = (_ItemsSource.Start - _TimeLineStart.Value).TotalDays / (_TimeLineEnd.Value - _TimeLineStart.Value).TotalDays * RootCanvas.ActualWidth;
                Canvas.SetLeft(TimeLineBorder, borderLeft);

                //line at bottom of timeline, to separate it from next one.
                Canvas.SetTop(BottomBorderLine, RootCanvas.ActualHeight - 0.1);
                BottomBorderLine.X2 = RootCanvas.ActualWidth;

                //write out the label and tooltip
                TimeLineTextBlock.Text = _ItemsSource.Label;
                Canvas.SetTop(TimeLineTextBlock, 2);
                double textBlockLeft = borderLeft + 4;
                if (textBlockLeft < 0)
                    textBlockLeft = 2;
                Canvas.SetLeft(TimeLineTextBlock, textBlockLeft);
                textBlockLeft = borderLeft + 5;
                if (textBlockLeft < 0)
                    textBlockLeft = 3;
                ToolTipService.SetToolTip(TimeLineBorder, _ItemsSource.ToolTip);
                ToolTipService.SetToolTip(TimeLineTextBlock, _ItemsSource.ToolTip);
                RootCanvas.UpdateLayout();
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.IsDoubleClick())
            {
                if (!_ItemsSource.IsHighlighted)
                    Select();
                e.Handled = true;
                OnDetailRequested();
            }
            else
            {
                if (_ItemsSource.IsHighlighted)
                    Deselect();
                else
                    Select();
                _IsMouseOnTimeLineBorder = true;
            }
        }

        private void OnDetailRequested()
        {
            if (DetailRequested != null)
                DetailRequested(this, new DetailRequestedEventArgs { Data = _ItemsSource.DataContext });
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            VisualizeItemsSource();
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_IsMouseOnTimeLineBorder)
                _IsMouseOnTimeLineBorder = false;
            else if (e.IsDoubleClick())
                OnGlobalDeselectRequested();
        }

        void OnSelected()
        {
            if (Selected != null)
                Selected(this, null);
        }

        void OnDeselected()
        {
            if (Deselected != null)
                Deselected(this, null);
        }

        void OnGlobalDeselectRequested()
        {
            if (GlobalDeselectRequested != null)
                GlobalDeselectRequested(this, null);
        }
    }

    public class TimeLineData : INotifyPropertyChanged
    {
        private bool _IsHighLighted;

        public Brush BackGround { get; set; }
        public object DataContext { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Label { get; set; }
        public object ToolTip { get; set; }
        public bool IsHighlighted
        {
            get { return _IsHighLighted; }
            set
            {
                _IsHighLighted = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("IsHighLighted"));
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

}
