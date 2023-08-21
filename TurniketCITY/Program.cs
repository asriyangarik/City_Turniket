using RelayControll;
using System.Runtime.InteropServices;

namespace TurniketCity
{

    class Program
    {
       
        static void Main(string[] args)
        {

           
            RelayControllCL MyRelay = new RelayControllCL();
            Console.WriteLine( MyRelay.MyDeviceConnect());




        }
    }
}
