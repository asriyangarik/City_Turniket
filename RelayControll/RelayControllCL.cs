using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using USB_Relay_Control;

namespace RelayControll
{
    public class RelayControllCL
    {
        int _deviceHandle = 0;


        private usb_relay_device_info _mydevice;

        public string MyDeviceInfo()
        {
            try
            {
                usb_relay_device_info deviceInfo = RelayDeviceWrapper.usb_relay_device_enumerate();
                return deviceInfo.ToString();
            }
            catch (Exception ex)
            {

                return ex.Message;
            }


        }

        public string MyDeviceConnect()
        {
            try
            {
                

                string path = "C:/turniket.txt";
                StreamReader reader = new StreamReader(path);
                string myModel = reader.ReadToEnd();
                myModel = myModel.Substring(0, 7);



                List<usb_relay_device_info> devicesInfos = new List<usb_relay_device_info>();
                usb_relay_device_info deviceInfo = RelayDeviceWrapper.usb_relay_device_enumerate();
                devicesInfos.Add(deviceInfo);

                while (deviceInfo.next.ToInt32() > 0)
                {
                    deviceInfo = (usb_relay_device_info)Marshal.PtrToStructure(deviceInfo.next, typeof(usb_relay_device_info));
                    devicesInfos.Add(deviceInfo);
                }

                foreach (var Mydevice in devicesInfos)
                {
                    var chouseDev = Mydevice.ToString().Substring(Mydevice.ToString().Length - 7);

                    if (chouseDev == myModel)
                    {
                        usb_relay_device_info device = Mydevice;
                        _mydevice = device;
                        _deviceHandle = RelayDeviceWrapper.usb_relay_device_open(ref device);
                        int numberOfRelays = (int)device.type;

                        uint status = 0;
                        RelayDeviceWrapper.usb_relay_device_get_status(_deviceHandle, ref status);

                        return device.ToString() + "_Connected sucsesfull";
                    }

                }

                return "Cannot Connect To devise chek the file";

            }
            catch (Exception)
            {

                return "Cannot Connect To devise";
            }

        }


        public void MyDeviceDisConnect()
        {

            if (!_mydevice.Equals(null))
            {
                RelayDeviceWrapper.usb_relay_device_close(_deviceHandle);
            }

        }

        #region Rele On Off

        public void ReleOn(int ReleNum)
        {
            int openResult = RelayDeviceWrapper.usb_relay_device_open_one_relay_channel(_deviceHandle, ReleNum);
        }

        public void ReleOff(int ReleNum)
        {
            int Result = RelayDeviceWrapper.usb_relay_device_close_one_relay_channel(_deviceHandle, ReleNum);
        }

        public void AllReleOff()
        {
            for (int i = 0; i < Convert.ToInt16(_mydevice.type); i++)
            {
                int Result = RelayDeviceWrapper.usb_relay_device_close_one_relay_channel(_deviceHandle, i);
            }
        }

        #endregion
    }
}
