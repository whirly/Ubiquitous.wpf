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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Ubiquitous
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class UbiquitousWindow : Window
    {
        // Variable
        String server;
        String password;

        String inPoint;
        String outPoint;

        IcecastEmitter emitter;
        IcecastReceiver receiver;

        DispatcherTimer dispatcherTimer;

        public UbiquitousWindow()
        {
            InitializeComponent();

            emitter = new IcecastEmitter();
            receiver = new IcecastReceiver();
        }

        public void SetFromCommandLine(String server, String inPoint, String outPoint, String password)
        {
            this.server = server;
            this.password = password;

            this.inPoint = inPoint;
            this.outPoint = outPoint;
        }

        internal void Start()
        {
            if (server != null)
            {
                emitter = new IcecastEmitter();
                emitter.OpenStream( server, outPoint, password );
                labelEmitter.Content += "Emitter connected...\n";

                receiver = new IcecastReceiver();
                if (!receiver.OpenStream(server, inPoint))
                {
                    dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                    dispatcherTimer.Tick += new EventHandler(dispatcherTick);
                    dispatcherTimer.Interval = new TimeSpan(0, 0, 10);
                    dispatcherTimer.Start();

                    this.labelReceiver.Content += "Server not responding, trying again in 10s\n";
                }
                else
                {
                    this.labelReceiver.Content += "Connected...\n";
                }
            }
        }

        private void dispatcherTick( Object sender, EventArgs e )
        {

            if (receiver.OpenStream(server, inPoint))
            {
                dispatcherTimer.Stop();
                this.labelReceiver.Content += "Connected...\n";
            }
            else
            {
                this.labelReceiver.Content += "Server not responding, trying again in 10s\n";
            }

        }
    }
}
