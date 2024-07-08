using Microsoft.AspNetCore.Hosting.Server;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AVAPI;
using System.Windows.Threading;

namespace AV_Data_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool running;
        private HostedWebServer Server;
        private DispatcherTimer Timer;

        public MainWindow()
        {
            this.ShowInTaskbar = false;
            InitializeComponent();
            this.running = false;
            this.Timer = new DispatcherTimer();
            this.Timer.Tick += new EventHandler(Timer_Tick);
            this.Timer.Interval = new TimeSpan(hours: 0, minutes: 0, seconds: 5);
            this.Timer.Start();

            this.InitializeWebServer();
            this.Button_Start_Click();
        }
        private void InitializeWebServer()
        {
            Server = new API();

#if USE_ALL_FEATURES_OF_SAMPLE_APPLICATION
            // Optionally Intercept Request Start/Completed for logging or UI
            Server.OnRequestCompleted = (ctx, ts) =>
            {
                // Request comes in on non-ui thread!
                Dispatcher.Invoke(() =>
                {
                    var method = ctx.Request.Method.PadRight(8);
                    var path = ctx.Request.Path.ToString();
                    var query = ctx.Request.QueryString.ToString();
                    if (!string.IsNullOrEmpty(query))
                        path += query;
                    var status = ctx.Response.StatusCode;


                    var text = method + path.PadRight(94) +
                               " (" + status + ") " +
                               ts.TotalMilliseconds.ToString("n3") + "ms";
                    double lines = Math.Floor((RequestMessages.ActualHeight) / (RequestMessages.LineHeight));
                    Model.AddRequestLine(text, Convert.ToInt32(lines - 1));
                    Model.RequestCount++;
                });
            };
#endif
        }
        public static void FireAndForget(Task task)
        {
            task.ContinueWith(tsk => tsk.Exception,
                TaskContinuationOptions.OnlyOnFaulted);
        }

        private async void Button_Start_Click()
        {
#if USE_ALL_FEATURES_OF_SAMPLE_APPLICATION
            Statusbar.ShowStatusSuccess("Server started.");
#endif
            var server = this.Server.LaunchAsync();
            MainWindow.FireAndForget(server);
            this.running = true;
            this.StartStop.Content = "Stop";

#if USE_ALL_FEATURES_OF_SAMPLE_APPLICATION
            Model.RequestText = "*** Web Server started.";
            Model.ServerStatus = "server is running";
#endif
        }

        private async void Button_Stop_Click()
        {
            await Server.Stop();
            this.running = false;
            this.StartStop.Content = "Start";

#if USE_ALL_FEATURES_OF_SAMPLE_APPLICATION
            Statusbar.ShowStatusSuccess("Server stopped.");
            Model.ServerStatus = "server is stopped";
            Model.RequestText = "*** Web Server is stopped. Click Start Server to run.";
#endif
        }

        private void StartStop_Click(object sender, RoutedEventArgs e)
        {
            if (this.running)
            {
                Button_Stop_Click();
            }
            else
            {
                Button_Start_Click();
            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            this.Timer.Interval = new TimeSpan(hours: 24, minutes: 0, seconds: 0);
            this.Hide();
        }
    }
}