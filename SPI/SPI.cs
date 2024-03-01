//-----------------------------------------------------------------------------
// SPI.cs
// 
// Demonstrates SPI communication.
//
// You can short MOSI to MISO for testing.
//
// T4:
//     MOSI    FIO6
//     MISO    FIO7
//     CLK     FIO4
//     CS      FIO5
//
// If you short MISO to MOSI, then you will read back the same bytes that you
// write.  If you short MISO to GND, then you will read back zeros.  If you
// short MISO to VS or leave it unconnected, you will read back 255s.
//
// support@labjack.com
//-----------------------------------------------------------------------------
using System;
using LabJack;


namespace SPI
{
    class SPI
    {
        static void Main(string[] args)
        {
            SPI spi = new SPI();
            spi.performActions();
        }

        public void showErrorMessage(LJM.LJMException e)
        {
            Console.Out.WriteLine("LJMException: " + e.ToString());
            Console.Out.WriteLine(e.StackTrace);
        }

        public void performActions()
        {
            int handle = 0;
            int errAddr = -1;
            int devType = 0;
            int conType = 0;
            int serNum = 0;
            int ipAddr = 0;
            int port = 0;
            int maxBytesPerMB = 0;
            string ipAddrStr = "";
            int numBytes = 1;
            byte[] aBytes = new byte[1];
            int numOBytes = 16;
            byte[] oBytes = new byte[16];
            Random rand = new Random();

            try
            {
                
                LJM.OpenS("T4", "ANY", "ANY", ref handle);  // T4 device, Any connection, Any identifier
               

                LJM.GetHandleInfo(handle, ref devType, ref conType, ref serNum, ref ipAddr, ref port, ref maxBytesPerMB);
                LJM.NumberToIP(ipAddr, ref ipAddrStr);
                Console.WriteLine("Opened a LabJack with Device type: " + devType + ", Connection type: " + conType + ",");
                Console.WriteLine("Serial number: " + serNum + ", IP address: " + ipAddrStr + ", Port: " + port + ",");
                Console.WriteLine("Max bytes per MB: " + maxBytesPerMB);
                Console.Out.WriteLine("");

               //Configure FIO4 to FIO7 as digital I/O.
                LJM.eWriteName(handle, "DIO_INHIBIT", 0xFFF0F);
                LJM.eWriteName(handle, "DIO_ANALOG_ENABLE", 0x00000);

                //Setting CS, CLK, MISO, and MOSI lines for the T4. FIO0
                //to FIO3 are reserved for analog inputs, and SPI requires
                //digital lines.
                LJM.eWriteName(handle, "SPI_CS_DIONUM", 5);  //CS is FIO5
                LJM.eWriteName(handle, "SPI_CLK_DIONUM", 4);  //CLK is FIO4
                LJM.eWriteName(handle, "SPI_MISO_DIONUM", 7);  //MISO is FIO7
                LJM.eWriteName(handle, "SPI_MOSI_DIONUM", 6);  //MOSI is FIO6
                
             

                //Selecting Mode CPHA=1 (bit 0), CPOL=1 (bit 1)
                LJM.eWriteName(handle, "SPI_MODE", 0);

                //Speed Throttle:
                //Valid speed throttle values are 1 to 65536 where 0 = 65536.
                //Configuring Max. Speed (~800 kHz) = 0
                LJM.eWriteName(handle, "SPI_SPEED_THROTTLE", 61100);

                //Options
                //bit 0:
                //    0 = Active low clock select enabled
                //    1 = Active low clock select disabled.
                //bit 1:
                //    0 = DIO directions are automatically changed
                //    1 = DIO directions are not automatically changed.
                //bits 2-3: Reserved
                //bits 4-7: Number of bits in the last byte. 0 = 8.
                //bits 8-15: Reserved

                //Enabling active low clock select pin
                LJM.eWriteName(handle, "SPI_OPTIONS", 0);

                //Read back and display the SPI settings
                string[] aNames = {"SPI_CS_DIONUM", "SPI_CLK_DIONUM",
                                   "SPI_MISO_DIONUM", "SPI_MOSI_DIONUM",
                                   "SPI_MODE", "SPI_SPEED_THROTTLE",
                                   "SPI_OPTIONS" };
                double[] aValues = new double[aNames.Length];
                LJM.eReadNames(handle, aNames.Length, aNames, aValues, ref errAddr);

                Console.WriteLine("SPI Configuration:");
                for (int i = 0; i < aNames.Length; i++)
                {
                    Console.WriteLine("  " + aNames[i] + " = " + aValues[i]);
                }

        

                //Write(TX)/Read(RX) 1 bytes
                
                LJM.eWriteName(handle, "SPI_NUM_BYTES", numBytes);

                //Set command to be sent
                //0x00: no_op, recieve 0xA5(165) in response; send this if getting strange response to clear out buffer
                //0x10: read_pos, see AMT20 data sheet for process description
                //0x70: set_zero
                int command = 0x10; //starting command (if encoder just powerd up start by running no_ops to make the buffer behave, change command to what you want on second run)
                int commSets = 5;  //how many times to do seperate write/read requests (temporay variable for testing purpose)

                for (int y = 0; y < commSets; y++)
                {
                    //Write the bytes
                    aBytes[0] = Convert.ToByte(command);
                    LJM.eWriteNameByteArray(handle, "SPI_DATA_TX", numBytes, aBytes, ref errAddr);
                    LJM.eWriteName(handle, "SPI_GO", 1);  //Do the SPI communications
                    //Display the bytes written
                    Console.WriteLine("");
                    Console.Out.WriteLine("dataWrite[" + y + "] = " + aBytes[0]);


                    //Read the bytes
                    //Initialize byte array values to zero
                    aBytes[0] = 0;
                    LJM.eReadNameByteArray(handle, "SPI_DATA_RX", numBytes, aBytes, ref errAddr);
                    LJM.eWriteName(handle, "SPI_GO", 1);  //Do the SPI communications
                    //Display the bytes read
                    Console.Out.WriteLine("");
                    Console.Out.WriteLine("dataRead[" + y + "] = " + aBytes[0]);

                    command = 0x00; //after intial command is sent we will only want to send no_ops
                
                    //wait for user key before next set
                    Console.Out.WriteLine("");
                    Console.WriteLine("Press any key to continue on to next set...");
                    Console.ReadKey();

                }
                    
       
            }
            catch (LJM.LJMException ljme)
            {
                showErrorMessage(ljme);
            }

            LJM.CloseAll();  //Close all handles

            Console.WriteLine("\nDone.\nPress the enter key to exit.");
            Console.ReadLine();  //Pause for user
        }
    }
}
