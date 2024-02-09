//-----------------------------------------------------------------------------
// SingleDIO.cs
//
// Demonstrates how to set and read a single digital I/O.
//
// support@labjack.com
//-----------------------------------------------------------------------------
using System;
using LabJack;


namespace SingleDIO
{
    class SingleDIO
    {
        static void Main(string[] args)
        {
            SingleDIO sd = new SingleDIO();
            sd.performActions();
        }

        public void showErrorMessage(LJM.LJMException e)
        {
            Console.Out.WriteLine("LJMException: " + e.ToString());
            Console.Out.WriteLine(e.StackTrace);
        }

        public void performActions()
        {
            int handle = 0;
            int devType = 0;
            int conType = 0;
            int serNum = 0;
            int ipAddr = 0;
            int port = 0;
            int maxBytesPerMB = 0;
            string ipAddrStr = "";
            string name = "";
            double state = 0;

            try
            {
                //Open first found LabJack
                LJM.OpenS("T4", "USB", "ANY", ref handle);  // Any device, Any connection, Any identifier
                //LJM.OpenS("T7", "ANY", "ANY", ref handle);  // T7 device, Any connection, Any identifier
                //LJM.OpenS("T4", "ANY", "ANY", ref handle);  // T4 device, Any connection, Any identifier
                //LJM.Open(LJM.CONSTANTS.dtANY, LJM.CONSTANTS.ctANY, "ANY", ref handle);  // Any device, Any connection, Any identifier

                LJM.GetHandleInfo(handle, ref devType, ref conType, ref serNum, ref ipAddr, ref port, ref maxBytesPerMB);
                LJM.NumberToIP(ipAddr, ref ipAddrStr);
                Console.WriteLine("Opened a LabJack with Device type: " + devType + ", Connection type: " + conType + ",");
                Console.WriteLine("Serial number: " + serNum + ", IP address: " + ipAddrStr + ", Port: " + port + ",");
                Console.WriteLine("Max bytes per MB: " + maxBytesPerMB);

                //Setup and call eWriteName to set the DIO state.
                if(devType == LJM.CONSTANTS.dtT4)
                {
                    //Setting FIO4 on the LabJack T4. FIO0-FIO3 are reserved for
                    //AIN0-AIN3.
                    name = "FIO4";
                    LJM.eReadName(handle, name, ref state);
                }
                else
                {
                    //Setting FIO0 on the LabJack T7 and other devices.
                    name = "FIO0";
                }
                
                while(true)
                {
                    LJM.eWriteName(handle, name, 0);
                    System.Threading.Thread.Sleep(3);
                    LJM.eWriteName(handle, name, 1);
                    System.Threading.Thread.Sleep(3);
                }

            }
            catch (LJM.LJMException e)
            {
                showErrorMessage(e);
            }

            LJM.CloseAll();  //Close all handles

            Console.WriteLine("\nDone.\nPress the enter key to exit.");
            Console.ReadLine();  // Pause for user
        }
    }
}
