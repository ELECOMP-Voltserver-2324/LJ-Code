//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
using System;
//using System.Runtime.InteropServices;
using LabJack;
using static ACTS.StepControl;
using static ACTS.SPI;

namespace ACTS
{
    class ACTS
    {
        static void Main(string[] args)
        {
            int handle = openLJ();
            StepControl sc = new StepControl(handle);
            SPI spi = new SPI(handle); //initalizes SPI parameters on Labjack

            sc.makeSteps(300, true); //do 300 steps
            spi.spiComm("read_pos"); //print position

            LJM.CloseAll();  //Close all handles
            Console.WriteLine("\nDone.\nPress the enter key to exit.");
            Console.ReadLine();  //Pause for user
        }

        //openLJ()
        //connects to t4 labjack device and returns interger identifier
        static int openLJ()
        {
            int handle = 0;
            int devType = 0;
            int conType = 0;
            int serNum = 0;
            int ipAddr = 0;
            int port = 0;
            int maxBytesPerMB = 0;
            string ipAddrStr = "";

            LJM.OpenS("T4", "ANY", "ANY", ref handle);  // T4 device, Any connection, Any identifier
            LJM.GetHandleInfo(handle, ref devType, ref conType, ref serNum, ref ipAddr, ref port, ref maxBytesPerMB);
            LJM.NumberToIP(ipAddr, ref ipAddrStr);
            Console.WriteLine("Opened a LabJack with Device type: " + devType + ", Connection type: " + conType + ",");
            Console.WriteLine("Serial number: " + serNum + ", IP address: " + ipAddrStr + ", Port: " + port + ",");
            Console.WriteLine("Max bytes per MB: " + maxBytesPerMB);
            Console.Out.WriteLine("");

            return handle;
        }

    }
}
