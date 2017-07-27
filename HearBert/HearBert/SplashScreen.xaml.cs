using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
    /// Interaktionslogik für SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        public bool ShowLong { get; set; }

        public SplashScreen()
        {
            InitializeComponent();
            ShowLong = false;
            Loaded += SplashScreen_Loaded;
        }

        private void SplashScreen_Loaded(object sender, RoutedEventArgs e)
        {
            // set assembly information
            var Assembly = Application.ResourceAssembly.GetName();
            var Version = Assembly.Version;
            var attr = Application.ResourceAssembly.GetCustomAttribute(typeof(AssemblyCopyrightAttribute));
            AssemblyInfo.Text = "Version " + Version.Major + "." + Version.Minor + " (" + Assembly.ProcessorArchitecture + ")\n";
            AssemblyInfo.Text += (attr as AssemblyCopyrightAttribute).Copyright;
            // run background thread to close window
            var bg = new Thread(() => AutoClose());
            bg.Start();
            DoubleAnimation da = new DoubleAnimation();
            da.From = 0.0;
            da.To = 1.0;
            da.Duration = new System.Windows.Duration(new System.TimeSpan(0, 0, 0, 0, 1000));
            Logo.BeginAnimation(System.Windows.Controls.Image.OpacityProperty, da);
        }

        private void AutoClose()
        {
            if (ShowLong)
            {
                Task.Delay(10000).Wait();
            } else
            {
                Task.Delay(2000).Wait();
            }

            this.Dispatcher.Invoke(() => { this.Close(); });
        }
    }
}
