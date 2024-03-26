//-----------------------------------------------------------------------------
// StepControl.cs
//
// Pulses for Motor Control
//
// support@labjack.com
//-----------------------------------------------------------------------------
using System;
using LabJack;



namespace ACTS
{
    class StepControl
    {
        private int scHandle;

        public StepControl (int handle)
        {
            scHandle = handle;
        }
        public void showErrorMessage(LJM.LJMException e)
        {
            Console.Out.WriteLine("LJMException: " + e.ToString());
            Console.Out.WriteLine(e.StackTrace);
        }

        //makeSteps()
        //int handle: labjack device identifier,
        //int steps: how many steps to take
        //bool forward: which direction to take (note: change to clockwise after verifying directions)
        public void makeSteps(int steps, bool forward)
        {
            string dir_pin = "DAC1";    //pin indicating step direction
            string pulse_pin = "DAC0";  //pin giving step pulse

            int uptime = 3;             //time pule_pin is high
            int downtime = 3;           //time pulse_pin is low              
            int dir = forward ? 5 : 0;  //step direction (5 = Foward, 0 = Back) 

            try
            {
                 
                LJM.eWriteName(scHandle, dir_pin, dir); //Set direction signal
                LJM.eWriteName(scHandle, pulse_pin, 0);//Set initial pulse low

                for (int i = 0; i < steps; i++) 
                {
                    System.Threading.Thread.Sleep(downtime);   //Toggles pulse_pin on and off
                    LJM.eWriteName(scHandle, pulse_pin, 5);
                    System.Threading.Thread.Sleep(uptime);
                    LJM.eWriteName(scHandle, pulse_pin, 0);
                }
                Console.WriteLine("\n StepControl: Finished " + steps.ToString() + " steps");



            }
            catch (LJM.LJMException e)
            {
                showErrorMessage(e);
            }
        }
    }
}
