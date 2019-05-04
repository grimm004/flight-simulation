using System.Collections.Generic;
using System;

namespace FSUIPCSDK
{
    public class PropertyManager
    {
        protected List<SimulatorProperty> Properties { get; set; }
        protected int resultOutput;

        public PropertyManager()
        {
            Properties = new List<SimulatorProperty>();
            resultOutput = 0;
        }

        public void AddProperty(SimulatorProperty property)
        {
            Properties.Add(property);
        }

        public void Update(FSUIPC fsuipc)
        {
            foreach (SimulatorProperty property in Properties)
                property.RequestRead(fsuipc);

            bool result = fsuipc.FSUIPC_Process(ref resultOutput);

            if (result) foreach (SimulatorProperty property in Properties)
                    property.LoadReadRequest(fsuipc);
        }
    }

    public class SimulatorProperty
    {
        public DataReference ReadReference { get; }
        public DataReference WriteReference { get; }
        public bool Successful { get; protected set; }
        protected byte[] Value { get { return value; } }
        public bool CanWrite { get { return WriteReference != null; } }

        private byte[] value = new byte[0];
        private int token = 0;
        private int result = 0;

        public SimulatorProperty(int offset, int size, bool canWrite = false)
        {
            ReadReference = new DataReference(offset, size);
            WriteReference = canWrite ? ReadReference : null;
            value = new byte[size];
            for (int i = 0; i < value.Length; i++) value[i] = 0x00;
        }

        public SimulatorProperty(int readOffset, int readSize, int writeOffset, int writeSize)
        {
            ReadReference = new DataReference(readOffset, readSize);
            WriteReference = new DataReference(writeOffset, writeSize);
            value = new byte[readSize];
            for (int i = 0; i < value.Length; i++) value[i] = 0x00;
        }

        public SimulatorProperty(DataReference reference, bool canWrite = false)
        {
            ReadReference = reference;
            WriteReference = canWrite ? ReadReference : null;
            value = new byte[reference.Size];
            for (int i = 0; i < value.Length; i++) value[i] = 0x00;
        }

        public SimulatorProperty(DataReference readReference, DataReference writeReference)
        {
            ReadReference = readReference;
            WriteReference = writeReference;
            value = new byte[readReference.Size];
            for (int i = 0; i < value.Length; i++) value[i] = 0x00;
        }

        public void RequestRead(FSUIPC fsuipc)
        {
            Successful = fsuipc.FSUIPC_Read(ReadReference.Offset, ReadReference.Size, ref token, ref result);
        }

        public void LoadReadRequest(FSUIPC fsuipc)
        {
            value = new byte[ReadReference.Size];
            Successful = fsuipc.FSUIPC_Get(ref token, ReadReference.Size, ref value);
        }

        public byte[] GetValue()
        {
            return Value;
        }

        public bool SetValue(FSUIPC fsuipc, byte[] value)
        {
            int wtoken = 0;
            int wresult = 0;
            if (CanWrite)
                return fsuipc.FSUIPC_Write(WriteReference.Offset, WriteReference.Size, ref wtoken, ref wresult);
            return false;
        }
    }

    public class DataReference
    {
        public int Offset { get; }
        public int Size { get; }

        public DataReference(int offset, int size)
        {
            Offset = offset;
            Size = size;
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
        public SimulatorPaused() : base(0x0264, 2, 0x0262, 2) { }

        public new bool GetValue()
        {
            return BitConverter.ToBoolean(Value, 0);
        }

        public bool SetValue(FSUIPC fsuipc, bool paused)
        {
            return SetValue(fsuipc, BitConverter.GetBytes((short)(paused ? 1 : 0)));
        }

        public bool Toggle(FSUIPC fsuipc)
        {
            return SetValue(fsuipc, !GetValue());
        }
    }
}
