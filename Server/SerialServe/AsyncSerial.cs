using System;
using System.Threading;
using System.IO.Ports;

namespace SerialServe
{
    public delegate void IncomingLineHandler(string line, string port);
    public class AsyncSerial
    {
        public event IncomingLineHandler OnIncomingLine;

        private SerialPort p;

        private Thread readThread;
        private bool continueReading = true;

        public int Baud
        {
            get
            {
                return p.BaudRate;
            }
        }

        public Parity Parity
        {
            get
            {
                return p.Parity;
            }
        }

        public int DataBits
        {
            get
            {
                return p.DataBits;
            }
        }

        public StopBits StopBits
        {
            get
            {
                return p.StopBits;
            }
        }

        public string NewLine
        {
            get
            {
                return p.NewLine;
            }
        }

        public AsyncSerial(string port, int baud = 2400, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One, string newLine = "\r")
        {
            p = new SerialPort(port, baud, parity, dataBits, stopBits);
            p.NewLine = newLine;
            p.Open();

            readThread = new Thread(Read);
            readThread.Start();
            readThread.IsBackground = true;
        }

        public void Close()
        {
            p.Close();
        }

        public void Write(string s)
        {
            p.Write(s);
        }

        public static AsyncSerial SearchPort(string write, char search, int baud = 9600)
        {
            foreach (string port in SerialPort.GetPortNames())
            {
                SerialPort temp;
                try
                {
                    temp = new SerialPort(port, baud);
                    try
                    {
                        temp.Open();
                        temp.ReadTimeout = 2500;
                        temp.Write(write);
                        var v = temp.ReadChar();
                        if (v == search)
                        {
                            temp.Close();
                            return new AsyncSerial(port);
                        }
                    }
                    catch
                    { }
                    finally
                    {
                        temp.Close();
                    }
                }
                catch
                { }
            }

            throw new Exception("Port not found.");
        }

        public void DtsDisable()
        {
            p.DtrEnable = false;
        }

        public void DtsEnable()
        {
            p.DtrEnable = true;
        }

        private void Read()
        {
            while (continueReading)
            {
                try
                {
                    string result = p.ReadLine();

                    // Strip out meta-text
                    result = result.Replace("\u0003", "");
                    result = result.Replace("\u0002", "");
                    result = result.Replace("\r", "");
                    result = result.Replace("\n", "");

                    if (OnIncomingLine != null && result != "")
                    {
                        OnIncomingLine(result, p.PortName);
                    }
                }
                catch { }
            }

            p.Close();
        }
    }
}
