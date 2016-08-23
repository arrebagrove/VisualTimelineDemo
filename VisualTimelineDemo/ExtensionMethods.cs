using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace VisualTimelineDemo
{
    public static class ExtensionMethods
    {

        #region Mouse Double Click
        //initialize to 2 secs back, so event doesnt fire very first time user clicks on control
        static DateTime _lastClicked = DateTime.Now.AddSeconds(-2);
        static Point _lastMousePosition = new Point(1, 1);
        private const double MousePositionTolerance = 20;
        public static bool IsDoubleClick(this MouseButtonEventArgs e)
        {
            bool ret = false;
            if ((DateTime.Now.Subtract(_lastClicked) < TimeSpan.FromMilliseconds(500))
                && (e.GetPosition(null).GetDistance(_lastMousePosition) < MousePositionTolerance))
            {
                ret = true;
            }
            _lastClicked = DateTime.Now;
            _lastMousePosition = e.GetPosition(null);

            return ret;
        }
        #endregion

        public static double GetDistance(this Point current, Point other)
        {
            double x = current.X - other.X;
            double y = current.Y - other.Y;
            return Math.Sqrt(x * x + y * y);
        }

        //not really an extension method
        public static bool Intersects(DateTime r1Start, DateTime r1End, DateTime r2Start, DateTime r2End)
        {
            return (r1Start == r2Start) || (r1Start > r2Start ? r1Start <= r2End : r2Start <= r1End);
        }


    }
}
