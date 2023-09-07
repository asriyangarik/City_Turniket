using RelayControll;
using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;

namespace MyTurniketControll
{
    public partial class CITY_TURNIKET_SCANER : Form
    {
        private string _ScanerQR = "";
        RelayControllCL MyTurniketRelay;
        SerialPort mySerialPort;
        string OpenEAN;
        string ComPort;
        string Device;


        public CITY_TURNIKET_SCANER()
        {
            InitializeComponent();
            MyTurniketRelay = new RelayControllCL();

            string path = "C:/turniket.txt";
            StreamReader reader = new StreamReader(path);

            string line;
            string[] AllLines = new string[] { };
            int i = 0;
            while ((line = reader.ReadLine()) != null)
            {
                i++;
                Array.Resize(ref AllLines, i);
                AllLines[i - 1] = line;
            }
            Device = AllLines[0];
            OpenEAN = AllLines[1];
            ComPort = AllLines[2];

            DeviceTB.Text = Device;
            ScCOMportTB.Text = ComPort;


            string isConnected = MyTurniketRelay.MyDeviceConnect();

            if (isConnected == "Cannot Connect To devise" || isConnected == "Cannot Connect To devise chek the file")
            {
                MessageBox.Show(isConnected + " Turniket");
                CloseAplication();
            }

            MyTurniketRelay.AllReleOff();

            mySerialPort = new SerialPort(ComPort);
            mySerialPort.BaudRate = 9600;
            mySerialPort.Parity = Parity.None;
            mySerialPort.StopBits = StopBits.One;
            mySerialPort.DataBits = 8;
            mySerialPort.Handshake = Handshake.None;

            mySerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

            try
            {
                mySerialPort.Open();
            }
            catch (System.Exception e)
            {

                MessageBox.Show(e.Message);
                CloseAplication();
            }


            this.WindowState = FormWindowState.Minimized;


        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
           // SerialPort sp = (SerialPort)sender;
            _ScanerQR = mySerialPort.ReadLine();
            string Prefix = "SCQR";

            if (_ScanerQR.IndexOf(Prefix) == 0)
            {
                string numberChek = "";
                string dateTimeChek = "";


                string[] words = _ScanerQR.Split(new char[] { ';' });
                dateTimeChek = words[1];
                numberChek = words[2];



                DateTime chekDate;
                bool isReslut = DateTime.TryParse(dateTimeChek, out chekDate);

                if (!isReslut)
                {
                    return;
                }

                DateTime chekedDateTim = DateTime.Now;


                chekedDateTim = chekedDateTim.AddMinutes(-5);
                if (chekDate > chekedDateTim)
                {
                    MyTurniketRelay.ReleOn(1);
                    Thread.Sleep(250);
                    MyTurniketRelay.ReleOff(1);
                }

            }
            else if (_ScanerQR == OpenEAN)
            {
                MyTurniketRelay.ReleOn(1);
                Thread.Sleep(250);
                MyTurniketRelay.ReleOff(1);
            }
         
           // thisForm.WindowState = FormWindowState.Minimized;


        }

        private void CITY_TURNIKET_SCANER_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (mySerialPort != null)
            {
                mySerialPort.Close();
                MyTurniketRelay.MyDeviceDisConnect();
                MyTurniketRelay.AllReleOff();
            }

        }

        private void CloseAplication()
        {
            if (mySerialPort != null)
            {
                mySerialPort.Close();
                MyTurniketRelay.MyDeviceDisConnect();
                MyTurniketRelay.AllReleOff();
            }

            System.Environment.Exit(0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
    }
}
