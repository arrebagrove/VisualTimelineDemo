using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace VisualTimelineDemo
{
    public static class DataAccess
    {
        static SolidColorBrush _Background = new SolidColorBrush(Color.FromArgb(0x33, 0x26, 0x7f, 0x00));
        static List<TimeLineData> _TimeLines;
        public static List<TimeLineData> TimeLines
        {
            get
            {
                if (_TimeLines == null)
                {
                    _TimeLines = new List<TimeLineData>();
                    AddTimeLine(DateTime.Now, DateTime.Now.AddDays(2), "Code Timecard Module");
                    AddTimeLine(DateTime.Now.AddDays(-10), DateTime.Now.AddDays(-2), "Design Timecard Module");
                    AddTimeLine(DateTime.Now.AddDays(-2), DateTime.Now, "Review Timecard Module Design");
                    AddTimeLine(DateTime.Now.AddDays(-15), DateTime.Now.AddDays(-8), "Gather requirements for Timecard Module");
                    AddTimeLine(DateTime.Now.AddDays(1), DateTime.Now.AddDays(3), "Late requests for Timecard Module");
                    AddTimeLine(DateTime.Now.AddDays(-8), DateTime.Now.AddDays(-1), "Code Expenses Module");
                    AddTimeLine(DateTime.Now.AddDays(-1), DateTime.Now.AddDays(2), "Review Expenses Module Design");
                    AddTimeLine(DateTime.Now.AddDays(-15), DateTime.Now.AddDays(-4), "Gather requirements for Expenses Module");
                    AddTimeLine(DateTime.Now.AddDays(5), DateTime.Now.AddDays(15), "System Integration Test");
                    AddTimeLine(DateTime.Now.AddDays(10), DateTime.Now.AddDays(20), "Bug Fixes");
                    AddTimeLine(DateTime.Now.AddDays(20), DateTime.Now.AddDays(25), "Second Integration Test");
                    AddTimeLine(DateTime.Now.AddDays(25), DateTime.Now.AddDays(30), "User Acceptance Test");
                    AddTimeLine(DateTime.Now.AddDays(32), DateTime.Now.AddDays(33), "Production Install");

                }
                return _TimeLines;
            }
            set { }
        }

        public static void AddTimeLine(DateTime start, DateTime end, string label)
        {
            _TimeLines.Add(new TimeLineData{BackGround = _Background,
                Start = start,
                End = end,
                Label = label,
                ToolTip = label + Environment.NewLine + "Start: " + start.ToString()
                + Environment.NewLine + "End: " + end.ToString()});

        }
    }
}
