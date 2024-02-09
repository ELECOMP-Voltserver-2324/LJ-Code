//-----------------------------------------------------------------------------
// StepControl.cs
//
// Pulses for Motor Control
//
// support@labjack.com
//-----------------------------------------------------------------------------
using System;
using LabJack;



namespace StepControl
{
    class StepControl
    {
        static void Main(string[] args)
        {
            StepControl sd = new StepControl();
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
                LJM.OpenS("ANY", "ANY", "ANY", ref handle);  // Any device, Any connection, Any identifier

                LJM.GetHandleInfo(handle, ref devType, ref conType, ref serNum, ref ipAddr, ref port, ref maxBytesPerMB);
                LJM.NumberToIP(ipAddr, ref ipAddrStr);
                Console.WriteLine("Opened a LabJack with Device type: " + devType + ", Connection type: " + conType + ",");

              
                string dir_pin = "DAC1";    //pin indicating step direction
                string pulse_pin = "DAC0";  //pin giving step pulse

                int uptime = 3;             //time pule_pin is high
                int downtime = 3;           //time pulse_pin is low
                int step_count = 2000;      //how many steps to take
                int dir = 5;                //step direction (5 = Foward, 0 = Back) 
                
                LJM.eWriteName(handle, dir_pin, dir); //Direction signal
                LJM.eWriteName(handle, pulse_pin, 0);//Set initial pulse low

                
                for (int i = 0; i < disp; i++) 
                {
                    System.Threading.Thread.Sleep(downtime);   //Toggles pulse_pin on and off
                    LJM.eWriteName(handle, pulse_pin, 5);
                    System.Threading.Thread.Sleep(uptime);
                    LJM.eWriteName(handle, pulse_pin, 0);
                }

                Console.WriteLine("\nDone! " + disp +" pulses outputed.");

               
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
