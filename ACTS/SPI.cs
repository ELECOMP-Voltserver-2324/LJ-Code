//-----------------------------------------------------------------------------
// SPI.cs
// 
// Demonstrates SPI communication.
//
// You can short MOSI to MISO for testing.
//
// T4 Encoder Wiring:
//     MOSI    DIO8     (Breakout board pin 4)
//     MISO    DIO10    (Breakout board pin 5)
//     CLK     DIO9     (Breakout board pin 12)
//     CS      DIO11    (Breakout board pin 13)
//  
//     VS               (Breakout board pin 1)
//     GND              (Breakout board pin 8)
//
// If you short MISO to MOSI, then you will read back the same bytes that you
// write.  If you short MISO to GND, then you will read back zeros.  If you
// short MISO to VS or leave it unconnected, you will read back 255s.
//
// support@labjack.com
//-----------------------------------------------------------------------------
using System;
using System.Runtime.InteropServices;
using LabJack;


namespace ACTS
{
    class SPI
    {
        private int spiHandle; //Labjack device identifier

        //Set up SPI connection and send no_ops to prep buffer
        public SPI(int handle)
        {
            spiHandle = handle;
            //Configure FIO4 to FIO7 as digital I/O.
            LJM.eWriteName(spiHandle, "DIO_INHIBIT", 0xFFF0F);
            LJM.eWriteName(spiHandle, "DIO_ANALOG_ENABLE", 0x00000);

            //Setting CS, CLK, MISO, and MOSI lines for the T4. FIO0
            //to FIO3 are reserved for analog inputs, and SPI requires
            //digital lines.
            LJM.eWriteName(spiHandle, "SPI_CS_DIONUM", 11);  //CS is DIO11
            LJM.eWriteName(spiHandle, "SPI_CLK_DIONUM", 9);  //CLK is DIO9
            LJM.eWriteName(spiHandle, "SPI_MISO_DIONUM", 10);  //MISO is DIO10
            LJM.eWriteName(spiHandle, "SPI_MOSI_DIONUM", 8);  //MOSI is DIO8

            //Selecting Mode CPHA=1 (bit 0), CPOL=1 (bit 1)
            LJM.eWriteName(spiHandle, "SPI_MODE", 0);

            //Speed Throttle:
            //Valid speed throttle values are 1 to 65536 where 0 = 65536.
            //Configuring Max. Speed (~800 kHz) = 0
            LJM.eWriteName(spiHandle, "SPI_SPEED_THROTTLE", 61100);

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
            LJM.eWriteName(spiHandle, "SPI_OPTIONS", 0);

            //as encoder just powered up, start by running no_ops to make the buffer behave
            spiComm("clear_buffer"); 
        }

        public void showErrorMessage(LJM.LJMException e)
        {
            Console.Out.WriteLine("LJMException: " + e.ToString());
            Console.Out.WriteLine(e.StackTrace);
        }

        //spiComm
        //sends command to encoder over SPI, returns position if read, if other commands return 1 on success (to be added)
        //string command: operation to be performed ("clear_buffer","read_pos","set_zero")
        //0x00: no_op, recieve 0xA5(165) in response
        //0x10: read_pos, see AMT20 data sheet for process description
        //0x70: set_zero, see AMT20 data sheet for process description
        //for read_pos, function returns position as integer, for other operations it returns 0 upon completion
        public int spiComm(string command)
        {
            int errAddr = -1;
            int numBytes = 1;
            byte[] aBytes = new byte[1];//length of byte array being written and read
            int commSets = 5;  //how many times to do seperate write/read requests (temporay variable for testing purpose)

            try 
            {
                LJM.eWriteName(spiHandle, "SPI_NUM_BYTES", numBytes);
        
                switch (command)
                {
                    case "clear_buffer":
                        int count = 0;
                        while (true)
                        {
                            //Write the bytes
                            aBytes[0] = Convert.ToByte(0x00);
                            LJM.eWriteNameByteArray(spiHandle, "SPI_DATA_TX", numBytes, aBytes, ref errAddr);
                            LJM.eWriteName(spiHandle, "SPI_GO", 1);  //Do the SPI communications

                            //Read the bytes
                            aBytes[0] = 0; //Initialize byte array values to zero
                            LJM.eReadNameByteArray(spiHandle, "SPI_DATA_RX", numBytes, aBytes, ref errAddr);
                            LJM.eWriteName(spiHandle, "SPI_GO", 1);//Do the SPI communications
        
                            //checks to make sure encoder is responding reliably before breaking
                            if (aBytes[0] == 0xA5)
                            {
                                count++;
                                if (count >= 2) //wait until 2 consecutive proper responses
                                {
                                    Console.Out.WriteLine("SPI: buffer cleared!"); 
                                    break;
                                }           
                            }  
                            else
                            {
                                count = 0;
                            }      
                        }
                        return 0;
                    case "read_pos":
                        //Send initial command
                        aBytes[0] = Convert.ToByte(0x10);
                        LJM.eWriteNameByteArray(spiHandle, "SPI_DATA_TX", numBytes, aBytes, ref errAddr);
                        LJM.eWriteName(spiHandle, "SPI_GO", 1);  //Do the SPI communications
                        
                        //await response
                        while(true)
                        {
                            //Read the bytes
                            aBytes[0] = 0; //Initialize byte array values to zero
                            LJM.eReadNameByteArray(spiHandle, "SPI_DATA_RX", numBytes, aBytes, ref errAddr);
                            LJM.eWriteName(spiHandle, "SPI_GO", 1);//Do the SPI communications

                            if (aBytes[0]==0x10) break; //break after getting right response
                            
                            aBytes[0] = Convert.ToByte(0x00);//send no_op until response
                            LJM.eWriteNameByteArray(spiHandle, "SPI_DATA_TX", numBytes, aBytes, ref errAddr);
                            LJM.eWriteName(spiHandle, "SPI_GO", 1);  //Do the SPI communications
                        }
                        //Send no_op
                        aBytes[0] = Convert.ToByte(0x00);
                        LJM.eWriteNameByteArray(spiHandle, "SPI_DATA_TX", numBytes, aBytes, ref errAddr);
                        LJM.eWriteName(spiHandle, "SPI_GO", 1);  //Do the SPI communications

                        //get MSB
                        aBytes[0] = Convert.ToByte(0x00);//send no_op until response
                        LJM.eWriteNameByteArray(spiHandle, "SPI_DATA_TX", numBytes, aBytes, ref errAddr);
                        LJM.eWriteName(spiHandle, "SPI_GO", 1);  //Do the SPI communications

                        int msb = aBytes[0] & 0b00001111; //mask out all but lower 4 bits

                        //Send no_op
                        aBytes[0] = Convert.ToByte(0x00);
                        LJM.eWriteNameByteArray(spiHandle, "SPI_DATA_TX", numBytes, aBytes, ref errAddr);
                        LJM.eWriteName(spiHandle, "SPI_GO", 1);  //Do the SPI communications

                        //get LSB
                        aBytes[0] = Convert.ToByte(0x00);//send no_op until response
                        LJM.eWriteNameByteArray(spiHandle, "SPI_DATA_TX", numBytes, aBytes, ref errAddr);
                        LJM.eWriteName(spiHandle, "SPI_GO", 1);  //Do the SPI communications

                        int lsb = aBytes[0];
                        int position = (msb << 8) | lsb;

                        Console.Out.WriteLine("SPI: position is " + position);
                        return postion;

                    case "set_zero":
                        //Send initial command
                        aBytes[0] = Convert.ToByte(0x70);
                        LJM.eWriteNameByteArray(spiHandle, "SPI_DATA_TX", numBytes, aBytes, ref errAddr);
                        LJM.eWriteName(spiHandle, "SPI_GO", 1);  //Do the SPI communications
                        
                        //await response
                        while(true)
                        {
                            //Read the bytes
                            aBytes[0] = 0; //Initialize byte array values to zero
                            LJM.eReadNameByteArray(spiHandle, "SPI_DATA_RX", numBytes, aBytes, ref errAddr);
                            LJM.eWriteName(spiHandle, "SPI_GO", 1);//Do the SPI communications

                            if (aBytes[0]==0x80) break; //break after getting right response
                            
                            aBytes[0] = Convert.ToByte(0x00);//send no_op until response
                            LJM.eWriteNameByteArray(spiHandle, "SPI_DATA_TX", numBytes, aBytes, ref errAddr);
                            LJM.eWriteName(spiHandle, "SPI_GO", 1);  //Do the SPI communications
                        }
                        Console.Out.WriteLine("SPI: Zero set. Encoder MUST be powered cycled for new zero to take effect!");
                        return 0;
                    default:
                        Console.Out.WriteLine("SPI: Invalid command given");
                        return 0;
                }
            }
            catch (LJM.LJMException ljme)
            {
                showErrorMessage(ljme);
            }


            //OLD CODE, DELETE AFTER TESTING NEW
            /*
            try
            {
                //Write(TX)/Read(RX) 1 bytes
                LJM.eWriteName(spiHandle, "SPI_NUM_BYTES", numBytes);

                for (int y = 0; y < commSets; y++)
                {
                    //Write the bytes
                    aBytes[0] = Convert.ToByte(command);
                    LJM.eWriteNameByteArray(spiHandle, "SPI_DATA_TX", numBytes, aBytes, ref errAddr);
                    LJM.eWriteName(spiHandle, "SPI_GO", 1);  //Do the SPI communications
                    //Display the bytes written
                    Console.WriteLine("");
                    Console.Out.WriteLine("dataWrite[" + y + "] = " + aBytes[0]);


                    //Read the bytes
                    //Initialize byte array values to zero
                    aBytes[0] = 0;
                    LJM.eReadNameByteArray(spiHandle, "SPI_DATA_RX", numBytes, aBytes, ref errAddr);
                    LJM.eWriteName(spiHandle, "SPI_GO", 1);  //Do the SPI communications
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
            }*/
        }
    }
}
