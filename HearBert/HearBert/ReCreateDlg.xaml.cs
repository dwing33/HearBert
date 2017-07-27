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

namespace HearBert
{
    /// <summary>
    /// Interaktionslogik für ReCreateDlg.xaml
    /// </summary>
    public partial class ReCreateDlg : Window
    {
        public enum DlgSelection { Drive, AllNew, Abort }
        public DlgSelection Selection;

        public ReCreateDlg()
        {
            InitializeComponent();
        }

        private void Drive_Click(object sender, RoutedEventArgs e)
        {
            Selection = DlgSelection.Drive;
            this.DialogResult = true;
        }

        private void AllNew_Click(object sender, RoutedEventArgs e)
        {
            Selection = DlgSelection.AllNew;
            this.DialogResult = true;
        }
    }
}
