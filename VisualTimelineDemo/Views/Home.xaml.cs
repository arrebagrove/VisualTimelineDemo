using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VisualTimelineDemo
{
    public partial class Home : Page
    {
        public Home()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Home_Loaded);
            tl.TimeLineChanged += (s, e) => SetTimeLineItemSource();
            tl.DetailRequested += new EventHandler<DetailRequestedEventArgs>(tl_DetailRequested);
        }

        void tl_DetailRequested(object sender, DetailRequestedEventArgs e)
        {
            MessageBox.Show("'DetailRequested' event raised by TimeLine");
        }

        void Home_Loaded(object sender, RoutedEventArgs e)
        {
            SetTimeLineItemSource();
        }

        private void SetTimeLineItemSource()
        {
            tl.ItemsSource = from t in DataAccess.TimeLines
                             where ExtensionMethods.Intersects(t.Start, t.End, tl.StartDate.Value, tl.EndDate.Value)
                             select t;
        }

        // Executes when the user navigates to this page.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DateTime st = (StartDatePicker.SelectedDate.HasValue)?StartDatePicker.SelectedDate.Value:DateTime.Now;
            DateTime en = (EndDatePicker.SelectedDate.HasValue)?EndDatePicker.SelectedDate.Value:st.AddDays(1);
            string lbl = (string.IsNullOrEmpty(ItemNameTextBox.Text))?"Random item":ItemNameTextBox.Text;
            DataAccess.AddTimeLine(st, en, lbl);
            SetTimeLineItemSource();
        }
    }
}