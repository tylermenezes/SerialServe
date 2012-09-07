using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace SerialServe
{
    public delegate void PortAddedHandler(int port);
    public delegate void PortRemovedHandler(int port);
    public delegate void IncomingSerialLineHandler(int port, string line);

    public class SerialBus
    {
        private Dictionary<int, AsyncSerial> ports = new Dictionary<int, AsyncSerial>();
        private List<int> portsFound = new List<int>();

        public event PortAddedHandler OnPortAdd;
        public event PortRemovedHandler OnPortRemove;
        public event IncomingSerialLineHandler OnIncomingSerialLine;

        private Thread portScannerThread;

        public SerialBus()
        {
            portScannerThread = new Thread(new ThreadStart(PortScanner));
            portScannerThread.IsBackground = true;
            portScannerThread.Start();
        }

        public AsyncSerial Connect(int port, int baud = 2400, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One, string newLine = "\r")
        {
            if (ports.ContainsKey(port))
            {
                AsyncSerial serial = ports[port];
                if (!(serial.Baud == baud && serial.Parity == parity && serial.DataBits == dataBits && serial.StopBits == stopBits && serial.NewLine == newLine))
                {
                    serial.Close();
                    serial = new AsyncSerial("COM" + port, baud, parity, dataBits, stopBits, newLine);
                    ports[port] = serial;
                }
            }
            else
            {
                var serial = new AsyncSerial("COM" + port, baud, parity, dataBits, stopBits, newLine);
                serial.OnIncomingLine += new IncomingLineHandler(serial_OnIncomingLine);
                ports.Add(port, serial);
            }

            return ports[port];
        }

        void serial_OnIncomingLine(string line, string port)
        {
            if (OnIncomingSerialLine != null)
            {
                OnIncomingSerialLine(int.Parse(port.Substring(3)), line);
            }
        }

        public IEnumerable<int> Ports
        {
            get
            {
                foreach (string port in SerialPort.GetPortNames())
                {
                    if (port.Length < 4 || port.Substring(0, 3) != "COM")
                    {
                        continue;
                    }
                    yield return int.Parse(port.Substring(3));
                }
            }
        }

        private void PortScanner()
        {
            while (true)
            {
                List<int> currentPorts = new List<int>();
                foreach (string port in SerialPort.GetPortNames())
                {
                    try
                    {
                        if (port.Substring(0, 3) != "COM")
                        {
                            continue;
                        }
                        int id = int.Parse(port.Substring(3));

                        currentPorts.Add(id);
                        if (!portsFound.Contains(id) && OnPortAdd != null)
                        {
                            OnPortAdd(id);
                        }
                    }
                    catch { }
                }

                foreach (int existingPort in portsFound)
                {
                    if (!currentPorts.Contains(existingPort))
                    {
                        if (ports.ContainsKey(existingPort))
                        {
                            ports[existingPort].Close();
                        }

                        if (OnPortRemove != null)
                        {
                            OnPortRemove(existingPort);
                        }
                    }
                }
                Thread.Sleep(1000);
            }
        }
    }
}
