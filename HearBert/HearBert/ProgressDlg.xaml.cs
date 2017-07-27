using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HearBert
{
    /// <summary>
    /// Interaktionslogik für ProgressDlg.xaml
    /// </summary>
    public partial class ProgressDlg : Window
    {
        public bool canClose { get; set; }
        public bool reqClose { get; set; }

        public ProgressDlg()
        {
            InitializeComponent();
            canClose = false;
            reqClose = false;
            Closing += ProgressDlg_Closing; ;
        }

        private void ProgressDlg_Closing(object sender, CancelEventArgs e)
        {
            reqClose = true; // in case user pressed Alt-F4 or clicked on close box
            if (canClose) e.Cancel = false;
            else e.Cancel = true;
        }

        private void Abort_Click(object sender, RoutedEventArgs e)
        {
            AbortBtn.IsEnabled = false;
            reqClose = true;
        }

        public void SetValueAnimated(int val)
        {
            DoubleAnimation da = new DoubleAnimation();
            da.To = val;
            da.Duration = new System.Windows.Duration(new System.TimeSpan(0, 0, 0, 0, 500));
            FileProgress.BeginAnimation(System.Windows.Controls.ProgressBar.ValueProperty, da);
        }

    }
}
