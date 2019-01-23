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
using System.Windows.Shapes;

namespace BubblePoke
{
    /// <summary>
    /// Interaction logic for ResultDialog.xaml
    /// </summary>
    public partial class ResultDialog : Window
    {
        private UserAction UAction = UserAction.NotDefined;

        public ResultDialog()
        {
            InitializeComponent();
        }

        // 0 - Score, 1 - To Win, 2 - Stars
        public ResultDialog(List<int> ires)
        {
            InitializeComponent();

            DisplayResult(ires);
        }

        private void DisplayResult(List<int> r)
        {
            if (r[0] >= r[1])
            {
                FailedGrid.Visibility = Visibility.Collapsed;
                ComplitedGrid.Visibility = Visibility.Visible;

                Star1.Visibility = Visibility.Visible;
                Star2.Visibility = r[2] > 1 ? Visibility.Visible : Visibility.Hidden;
                Star3.Visibility = r[2] > 2 ? Visibility.Visible : Visibility.Hidden;
            }
            else
            {
                FailedGrid.Visibility = Visibility.Visible;
                ComplitedGrid.Visibility = Visibility.Collapsed;

                ScoreTBL.Text = $"Score: {r[0]}";
                ToWinTBL.Text = $"To Win: {r[1]}";
            }
        }

        public UserAction Answer
        {
            set
            {
                UAction = value;
                this.DialogResult = true;
                this.Close();
            }
            get { return UAction; }
        }

        private void TryBT_Click(object sender, RoutedEventArgs e)
        {
            Answer = UserAction.TryAgain;
        }

        private void QuiteBT_Click(object sender, RoutedEventArgs e)
        {
            Answer = UserAction.Quit;
        }

        private void PlayNextBT_Click(object sender, RoutedEventArgs e)
        {
            Answer = UserAction.PlayNext;
        }
    }
}
