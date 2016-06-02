using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxpPAApiLib
{
    public class ErrorCodes
    {

        static Dictionary<int, string> err = new Dictionary<int, string>();
        static ErrorCodes()
        {
             err.Add(0  , "No Error");
             err.Add(1, "Illegal Command – the command byte is not recognized");
             err.Add(2 , "Illegal Command Format – an I2C read was attempted without having set the            command using an I2C write");
             err.Add(3, "Incorrect Byte Count – the number of data bytes written does not match the command");
             err.Add(4 ,"Invalid State – the requested command cannot be executed in the current state / mode");
             err.Add(5 ,"Illegal Mode – the mode requested by the Set Mode command is not supported");
             err.Add(6 ,"Invalid I2C Address – the I2C address supplied in the Assign ID command is less than 2 or is more than 127.");
             err.Add(7, "Invalid DAC Bit – the Write DAC command attempted to write to an unsupported reserved DAC channel");
             err.Add(8 ,"Invalid Parameter Bit – the Write Param command attempted to write to anunsupported / reserved parameter");
             err.Add(9 ,"Invalid Parameter Value – the Write Param command attempted to write a parameter value that was outside the supported range");
             err.Add(10 ,"CRC Error – the checksum for the EEPROM data in the Write EEPROM command data did not match");
             err.Add(11 ,"EEPROM Locked – the Write EEPROM data command attempted to write to the EEPROM but it is locked");
             err.Add(12 ,"Pulse Test, SW Check – failure detected during the Pulse Test");
             err.Add(13 ,"Pulse Test, PLL Lock – failure detected during the Pulse Test");
             err.Add(14 ,"Reserved");
             err.Add(15 ,"Shutdown – a command was received to modify the RF output but the module is in Shutdown state so the command was not performed");
             err.Add(16, "Incorrect Read Byte Count – the number of bytes read exceeds the requested data size");
             err.Add(17 ,"NVM Size – attempted to read EEPROM Data from beyond the EEPROM size");
             err.Add(18 ,"NVM Erase – An internal error occurred while erasing the EEPROM");
             err.Add(19 ,"NVM Write – An internal error occurred while writing EEPROM data to the NVM");
             err.Add(20 ,"NVM Invalid – Attempted to perform Pulse Test but the EEPROM contents are invalid");
        }
        public static string GetError(int id)
        {
            try {
                return err[id];
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }

        public static string getAlertReasonBit(byte alert)
        {
            string msg = string.Empty;
                
            if ((alert & 0x1) == 0x0)
            {
                msg += @"TEMP / VOLT \nPA exceeds the PA temperature" + Environment.NewLine +
                        "limit, or the PA voltage minimum and maximum limits, at" + Environment.NewLine +
                        "which time the PA is also powered down. This bit is cleared" + Environment.NewLine;
            }

            if ((alert & 0x2) == 0x0)
            {
                msg += @"\nIDD" + Environment.NewLine +
                "PA exceeds the current limit, at" + Environment.NewLine +
                "which time the PA is also powered down. This bit is cleared" + Environment.NewLine +
                "when the Alert register is read" + Environment.NewLine;
            }

            if ((alert & 0x4) == 0x0)
            {
                msg += @"\nVSWR" + Environment.NewLine +
                "VSWR limit, at" + Environment.NewLine +
                "which time the PA is also powered down. This bit is cleared" + Environment.NewLine +
                "when the Alert register is read." + Environment.NewLine;
            }

            if ((alert & 0x8) == 0x0)
            {
                msg += @"\nREF_PWR\n
                        reflected power\n
                        limit, at which time the PA is also powered down. This bit is\n
                         cleared when the Alert register is read.";
            }

            if ((alert & 0x10) == 0x0)
            {
                msg += @"\nHW_PWR" + Environment.NewLine +
                       "This bit is set active when the PA exceeds the hardware" + Environment.NewLine +
                       "reflected power limit, at which time the PA is also powered" + Environment.NewLine +
                       "down and the SHUTDOWN_B signal is asserted low to shut" + Environment.NewLine +
                       "down all other modules. This bit is cleared when the Alert" + Environment.NewLine +
                       "register is read" + Environment.NewLine;
            }


            if ((alert & 0x20) == 0)
            {
                msg += @"PULSE_FAIL" + Environment.NewLine  + 
                          "Failure is detected during the pulse"+ Environment.NewLine  + 
                          "test. This bit is cleared when the Alert register is read.";
            }

            if ((alert & 0x40) == 0)
            {
                msg += @"CMD_ERR"+ Environment.NewLine  + 
                "When a command error is detected by the slave module this"+ Environment.NewLine  + 
                "bit is set active and the command is ignored. Possible causes"+ Environment.NewLine  + 
                "of a command error include:"+ Environment.NewLine  + 
                "- Invalid command byte"+ Environment.NewLine  + 
                "- Broadcast of a command that does not support it"+ Environment.NewLine  + 
                "- Too few or too many data bytes for a command"+ Environment.NewLine  + 
                "- I2C read is not preceded by an I2C write"+ Environment.NewLine  + 
                "This bit is cleared when the Alert register is read"+ Environment.NewLine;
            }
            if ((alert & 0x80) == 0)
            {
                msg += "NEED_ID This bit is set active when the module completes its startup"+ Environment.NewLine;
                msg += "sequence and is ready for an ID to be assigned. The bit is" + Environment.NewLine;
                msg += "cleared when the module ID is successfully assigned with the" + Environment.NewLine;
                msg += "Assign ID command.";
                msg += Environment.NewLine;
            }

            return msg;
        }
    }
}
