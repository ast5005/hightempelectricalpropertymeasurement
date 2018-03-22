using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ivi.Visa.Interop;
using System.Windows;


namespace HighTemperatureCharacterizationSystem2._0
{
    class SwitchCommandCenter
    {
        string addr;
         private ResourceManager mgr;
        private FormattedIO488 data_logger;
        public string currmsg = "";
        public SwitchCommandCenter()
        {
            addr = "GPIB1::9::INSTR";
            try
            {
                mgr = new ResourceManager();
                data_logger = new FormattedIO488();
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message + "\nIntialization Error Occured at SwitchCommandCenter", "SwitchCommandCenter");
            }

        }
        public bool OpenPort()
        {
        
            string addrtype,option;
            option="";
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
                data_logger.WriteString("*IDN?",true);
                currmsg = data_logger.ReadString();
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\nPort Couldn't be Opened at SwitchCommandCenter", "SwitchCommandCenter");
                return false;
            }
        }
        public bool setSwitches(byte[] openedSwitches, byte[] closedSwitches)
        {
            try
            {
                data_logger.WriteString("*RST;*OPC?", true);
                String temp = data_logger.ReadString();
                string open="@"+openedSwitches[0];
                string close="@"+closedSwitches[0];
                for(int i=1;i<openedSwitches.Length;i++)
                {
                    open +=","+openedSwitches[i];
                }
                for(int i=1;i<openedSwitches.Length;i++)
                {
                    close +=","+closedSwitches[i];
                }
                data_logger.WriteString("ROUT:CLOS ("+ close +")", true);
                //data_logger.WriteString("ROUT:DONE?", true);
                data_logger.WriteString("ROUT:OPEN (" + open + ")", true);
                data_logger.WriteString("ROUT:DONE?", true);
                temp = data_logger.ReadString();
                //MessageBox.Show(temp+openedSwitches[1]);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception occured during setting switches\n"+e.Message);
                return false;
            }
        }
        
    
            
    
    

    }
}
