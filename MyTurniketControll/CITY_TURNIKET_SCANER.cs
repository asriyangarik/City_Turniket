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
        private string _OldScanerQR;
        RelayControllCL MyTurniketRelay;
        SerialPort mySerialPort;
        string OpenEAN;
        string ComPort;
        string Device;
        string Prefix = "SCQR";


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


            string isConnected = MyTurniketRelay.MyDeviceConnect(Device);

            if (isConnected == "Cannot Connect To devise" || isConnected == "Cannot Connect To devise chek the file")
            {
                MessageBox.Show(isConnected + " Turniket");
                CloseAplication();
                return;
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
                LogWriter(e.Message);
                CloseAplication();
            }


            //this.WindowState = FormWindowState.Minimized;


        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {

            SerialPort sp = (SerialPort)sender;
            _ScanerQR = sp.ReadExisting();
            _ScanerQR = _ScanerQR.Trim(new char[] { '\n', ' ', '\r' });

            if (_OldScanerQR == _ScanerQR)
            {
                return;
            }
            LogWriter(_ScanerQR);

            

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

                    _OldScanerQR = _ScanerQR;
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

            CloseAplication();

        }

        private void CloseAplication()
        {
            if (mySerialPort != null)
            {
                mySerialPort.Close();
            }
            if (MyTurniketRelay != null)
            {
                MyTurniketRelay.AllReleOff();
                MyTurniketRelay.MyDeviceDisConnect();
            }
            try
            {
                System.Environment.Exit(0);
            }
            catch (Exception e)
            {

                LogWriter(e.Message);
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void TestBT_Click(object sender, EventArgs e)
        {
            MyTurniketRelay.ReleOn(1);
            Thread.Sleep(250);
            MyTurniketRelay.ReleOff(1);
            LogWriter("TestButton" + "Serial Port is Open: " + mySerialPort.IsOpen.ToString());



        }

        private void RelayList_Click(object sender, EventArgs e)
        {
            var myReleys = RelayControllCL.MyDeviceNames();
            string Message = "";
            foreach (var item in myReleys)
            {
                Message = Message + item.ToString() + '\n';
            }
            MessageBox.Show(Message);
        }

        private void LogWriter(string text)
        {
            text = text.Trim(new char[] { '\n', ' ', '\r' });
            text = text + "---" + DateTime.Now.ToString() + '\n';

            DirectoryInfo myDir = new DirectoryInfo("turniketLogs");
            if (!myDir.Exists)
            {
                myDir.Create();
            }
            FileInfo Logfile = new FileInfo(@"turniketLogs\Turniketlog.txt");    ///stugum enq log fayli arkayutyun@

            if (!Logfile.Exists)
            {
                File.Create(@"turniketLogs\Turniketlog.txt");                        ///ete fayl@ chka
            }

            if (Logfile.CreationTime < DateTime.Now.AddDays(-3))
            {
                File.WriteAllText(@"turniketLogs\Turniketlog.txt", text);
            }
            else
            {
                File.AppendAllText(@"turniketLogs\Turniketlog.txt", text);
            }


        }
    }
}
