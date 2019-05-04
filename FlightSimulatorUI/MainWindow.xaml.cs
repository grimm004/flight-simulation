using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using FSUIPCSDK;

namespace FlightSimulatorUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        protected FSUIPC fsuipc;
        protected PropertyManager propertyManager;
        protected SimulatorPaused paused;
        protected Altitude altitude;
        protected Heading heading;
        protected IndicatedAirspeed iAirspeed;

        private const int dwFSReq = 0;
        private int dwResult = 0;
        protected const int UPDATESPERSECOND = 30;

        protected bool connected;

        public MainWindow()
        {
            InitializeComponent();

            fsuipc = new FSUIPC();
            fsuipc.FSUIPC_Initialization();
            connected = false;

            propertyManager = new PropertyManager();
            iAirspeed = new IndicatedAirspeed();
            propertyManager.AddProperty(iAirspeed);
            heading = new Heading();
            propertyManager.AddProperty(heading);
            altitude = new Altitude();
            propertyManager.AddProperty(altitude);
            paused = new SimulatorPaused();
            propertyManager.AddProperty(paused);

            Task.Run((Action)ServeUpdates);
        }

        protected void ServeUpdates()
        {
            while (true)
            {
                if (connected) propertyManager.Update(fsuipc);
                Dispatcher.Invoke(Update);
                Thread.Sleep(1000 / UPDATESPERSECOND);
            }
        }

        protected void Update()
        {
            airspeedLabel.Content = iAirspeed.GetValue().ToString();
            headingLabel.Content = heading.GetValue().ToString("000");
            altitudeLabel.Content = altitude.GetValue().ToString();
            pauseButton.Content = paused.GetValue() ? "Paused" : "Pause";
            pauseButton.IsEnabled = !paused.GetValue() && connected;
        }
        
        private void PauseButtonClick(object sender, RoutedEventArgs e)
        {
            paused.SetValue(fsuipc, true);
        }

        private void ConnectButtonClick(object sender, RoutedEventArgs e)
        {
            if (connected)
            {
                fsuipc.FSUIPC_Close();
                fsuipc.FSUIPC_Initialization();
                connectButton.Content = "Connect";
                connected = false;
            } else if (!(connected = fsuipc.FSUIPC_Open(dwFSReq, ref dwResult))) MessageBox.Show("Could not connect to FSUIPC.", "FSUIPC Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else connectButton.Content = "Disconnect";
        }
    }
}
