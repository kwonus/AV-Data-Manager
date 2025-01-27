using System.Text;
using System.Windows;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.IO;
using AVXFramework;
using Pinshot.Blue;
using AVAPI;

namespace AV_Data_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        private bool running;
        private HostedWebServer? Server;
        private DispatcherTimer Timer;

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint wMsg, UIntPtr wParam, IntPtr lParam);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        const int WM_QUIT  = 0x0012;
        const int WM_CLOSE = 0x0010;

        private string GetVersionDigitalAV()
        {
            StringBuilder version = new(8);
            string omega = AVEngine.Data;
            if (System.IO.File.Exists(omega))
            {
                try
                {
                    using (FileStream fs = new FileStream(omega, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (BinaryReader br = new BinaryReader(fs))
                        {
                            byte c;
                            UInt32 i;
                            UInt64 value = 0;
                            for (int x = 0; x < 16; x++)
                                c = br.ReadByte();
                            for (int x = 0; x < 4; x++)
                                i = br.ReadUInt32();
                            for (int x = 0; x < 2; x++)
                                value = br.ReadUInt64();

                            version.Append((value >> 12).ToString("X"));
                            version.Append(".");
                            version.Append(((value & 0xF00) >> 8).ToString("X"));
                            version.Append(".");
                            version.Append((value & 0xFF).ToString("X"));
                            version.Append("  Ω");
                        }
                    }
                }
                catch (Exception ex)
                {
                    version.Clear();
                    version.Append("Unknown");
                }
            }
            return version.ToString();
        }
        public SplashScreen()
        {
            var existing = FindWindow(null, "AV Data Manager"); // Force a single instance
            if (existing > 0)
            {
                SendMessage(existing, WM_CLOSE, UIntPtr.Zero, IntPtr.Zero);
            }
            this.ShowInTaskbar = false;
            InitializeComponent();
            this.Revision_S4T.Text = "S4T Grammar Version: " + Pinshot_RustFFI.VERSION;
            this.Revision_DAV.Text = "Digital-AV SDK: " + GetVersionDigitalAV();
            this.running = false;
            this.Timer = new DispatcherTimer();
            this.Timer.Tick += new EventHandler(Timer_Tick);
            this.Timer.Interval = new TimeSpan(hours: 0, minutes: 0, seconds: 11);
            this.Timer.Start();

            this.InitializeWebServer();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.running)
            {
                var task = this.Server.Stop();
                task.Wait(2500);
            }
        }

        private void InitializeWebServer()
        {
            Server = new API();
            var server = this.Server.LaunchAsync();
            SplashScreen.FireAndForget(server);
            this.running = true;
        }

        public static void FireAndForget(Task task)
        {
            task.ContinueWith(tsk => tsk.Exception,
                TaskContinuationOptions.OnlyOnFaulted);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            this.Timer.Interval = new TimeSpan(hours: 24, minutes: 0, seconds: 0);
            this.Hide();
        }
    }
}