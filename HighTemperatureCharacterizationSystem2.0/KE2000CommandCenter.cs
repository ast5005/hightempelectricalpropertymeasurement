using Ivi.Visa.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace HighTemperatureCharacterizationSystem2._0
{
    class KE2000CommandCenter
    {
        string addr;
        private ResourceManager mgr;
        private FormattedIO488 data_logger;
        public string currmsg = "";
        
        public KE2000CommandCenter()
        {
            addr = "GPIB1::16::INSTR";
            try
            {
                mgr = new ResourceManager();
                data_logger = new FormattedIO488();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\nIntialization Error Occured at SwitchCommandCenter", "SwitchCommandCenter");
            }
        }
        public bool OpenPort()
        {
            string addrtype, option;
            option = "";
            try
            {
                if (addr.Length < 5)
                {
                    MessageBox.Show("Incorrect address: Check address", "SwitchCommandCenter Class Error");
                    return false;
                }
                else
                {
                    addrtype = addr.Substring(0, 4);
                }
                if (addrtype == "GPIB")
                {
                    data_logger.IO = (IMessage)mgr.Open(addr, AccessMode.NO_LOCK, 2000, option);

                }
                else
                {
                    MessageBox.Show("No Resources found");
                }
                data_logger.WriteString("*IDN?", true);
                currmsg = data_logger.ReadString();
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\nPort Couldn't be Opened at SwitchCommandCenter", "SwitchCommandCenter");
                return false;
            }
        }
        public double readVoltage()
        {
            try
            {
                //MessageBox.Show("1");
                data_logger.WriteString(":SENS:FUNC 'VOLT:DC'");
                //MessageBox.Show("2");
                data_logger.WriteString(":SENS:DATA?");
                string received=data_logger.ReadString();
                //MessageBox.Show("3 " + received);
                
                string[] data_received = received.Split(',');
                //MessageBox.Show("4"+ data_received[0]);
                string powerString = data_received[0].Remove(0,12);
                double power = Convert.ToDouble(powerString.Remove(3));
                double value=Convert.ToDouble(data_received[0].Remove(6));
               // MessageBox.Show("value "+value+"  Power "+power);
                return value * Math.Pow(10,power);
               
                //double value = Convert.ToDouble(data_received[1]);
                //return 0;// value;


            }
            catch (Exception e)
            {
                return -1;
            }

        }



    }
}
