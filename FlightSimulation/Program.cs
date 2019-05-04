using System;
using System.Threading;
using FSUIPCSDK;

namespace FlightSimulation
{
    class Program
    {
        private const int dwFSReq = 0;
        private static int dwResult = 0;
        
        static void Main(string[] args)
        {
            FSUIPC fsuipc = new FSUIPC();
            fsuipc.FSUIPC_Initialization();

            PropertyManager pm = new PropertyManager();

            IndicatedAirspeed indicatedAirSpeed = new IndicatedAirspeed();
            pm.AddProperty(indicatedAirSpeed);
            Heading heading = new Heading();
            pm.AddProperty(heading);
            Altitude altitude = new Altitude();
            pm.AddProperty(altitude);
            SimulatorPaused paused = new SimulatorPaused();
            pm.AddProperty(paused);

            bool result = fsuipc.FSUIPC_Open(dwFSReq, ref dwResult);
            if (result)
            {
                while (true)
                {
                    pm.Update(fsuipc);

                    Console.Clear();
                    if (!paused.GetValue()) Console.WriteLine("IAS: {0:000} HDG: {1:000} ALT : {2:0}", indicatedAirSpeed.GetValue(), heading.GetValue(), altitude.GetValue());
                    else Console.WriteLine("Simulation Paused"); ;

                    Thread.Sleep(1000 / 100);
                }
            }
            else Console.WriteLine("Open Failed");

            Console.ReadKey();
        }
    }

    public class IndicatedAirspeed : SimulatorProperty
    {
        public IndicatedAirspeed() : base(0x02BC, 4) { }

        public new int GetValue()
        {
            return BitConverter.ToInt32(Value, 0) / 128;
        }
    }

    public class Heading : SimulatorProperty
    {
        public Heading() : base(0x0580, 4) { }

        public new int GetValue()
        {
            return (int)(((long)BitConverter.ToInt32(Value, 0) * 365) / 0xFFFFFFFF + 360) % 360;
        }
    }

    public class Altitude : SimulatorProperty
    {
        public Altitude() : base(0x3324, 4) { }

        public new double GetValue()
        {
            return BitConverter.ToInt32(Value, 0);
        }
    }

    public class SimulatorPaused : SimulatorProperty
    {
        public SimulatorPaused() : base(0x0264, 2) { }

        public new bool GetValue()
        {
            return BitConverter.ToBoolean(Value, 0);
        }
    }
}
