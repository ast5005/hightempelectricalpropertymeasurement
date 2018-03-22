using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Globalization;

namespace HighTemperatureCharacterizationSystem2._0
{
    class serialCom 
    {
        #region Manager Variables
        private string baudrate = "";
        private string parity = "";
        private string stopbits = "";
        private string databits = "";
        private string portname ="";
        private double existingReading = 0.0; 
        private SerialPort port = new SerialPort();
        #endregion
        List<byte> readBuffer=new List<byte>();
        List<int> messageOffset=new List<int>();        
        int messagepassed = 0;


        public serialCom(string ibaudrate,string iparity,string istopbits,string idatabits,string iportname)
        {
            baudrate = ibaudrate;
            parity = iparity;
            stopbits = istopbits;
            databits = idatabits;
            portname = iportname;
            port.DataReceived += new SerialDataReceivedEventHandler(portReceive);
            messageOffset.Add(0);
           //messageOffset.Add(0);
        }
        public bool OpenPort()
        {
            try
            {
                
                if (port.IsOpen == true) port.Close();
                port.PortName = portname;
                port.BaudRate = int.Parse(baudrate);
                port.DataBits = int.Parse(databits);
                port.StopBits = (StopBits)Enum.Parse(typeof(StopBits), stopbits);    
                port.Parity = (Parity)Enum.Parse(typeof(Parity), parity);
                //MessageBox.Show(port.PortName+" "+port.BaudRate+" "+port.DataBits+" "+port.StopBits+" "+port.Parity);
                System.Threading.Thread.Sleep(2000);
                port.Open();
                return true;

            }
            catch (Exception e)
            {

                MessageBox.Show("Error in openning the COM Port\n"+e.Message+"\n"+e.StackTrace+"\nException is:  \n"+e.InnerException);
                return false;
            }

        }
        public bool ClosePort()
        {
            try
            {
                port.Close();
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show("Error in closing the COM Port\n"+e.Message);
                return false;
            }
        }
        public void portReceive(object sender, SerialDataReceivedEventArgs e)
        {
            //MessageBox.Show("portReceivr Invoked ");
            int bytes = port.BytesToRead;
            messageOffset[messagepassed]+= (int)bytes;
           // MessageBox.Show("received " + bytes + " bytes");
           byte[] buffer = new byte[bytes];                      
            port.Read(buffer,0,bytes);
            //System.Threading.Thread.Sleep(10000);
            //string s = ""; //port.ReadExisting();
            string c = "";
            for (int i = 0; i < bytes; i++)
            {
                //s+="  "+buffer[i];
                readBuffer.Add(buffer[i]);
                c += "  " + readBuffer[i];
            }
            //MessageBox.Show("readbuffer " + c + " bytes " +bytes );
            //char[] hexArray = new char[5];            
           //MessageBox.Show(" Hex string " + hexstring+" Read Temp "+readTemp);
            //readBuffer=buffer;*/                
        }
        public List<int> gettheoffset()
        {
            return messageOffset;
        }
        public List<byte> portRead()
        {
            System.Threading.Thread.Sleep(2000);
           string s = "";
            for (int i = 0; i < readBuffer.Count(); i++)
            {
                s += "  " + readBuffer[i];
            }
            //MessageBox.Show("readbuffer in portread "+s);
            messagepassed++;           
            messageOffset.Add(0);
            //checkmsg();
            return readBuffer;
        }
        public double portReadER()
        {
            return existingReading;
        }
        public bool checkmsg()
        {
             int count=messageOffset.Count()-1;             
             int k=messageOffset.Sum()-messageOffset[0];
             int l=readBuffer.Count();
             ushort[] msg=new ushort[l-k];
             for (int i=k; i < l; i++)
             {
                 msg[i-k]=readBuffer[i];

             }
            string s="";
            string p="";
            ushort[] checkedmsg = checkedmessage(msg);            
           for(int i=0;i<msg.Length;i++)
           {
               s+=" "+msg[i];
               p+="  "+checkedmsg[i];
           }
            MessageBox.Show("Reading Message for checking= "+s+"\nChecked message = "+p);
            
                if (checkedmsg[checkedmsg.Length - 4] == msg[msg.Length - 2] && checkedmsg[checkedmsg.Length - 3] == msg[msg.Length - 1])
                {
                    //MessageBox.Show("true icinde");
                    return true;
                }
                else return false;
            
           
        }

        public ushort[] checkedmessage(ushort[] msg)
        {
            ushort[] crc = CRCModbus.getcrc(msg);
            ushort[] checkedmsg = new ushort[msg.Length + 2];
            for (int i = 0; i < msg.Length; i++)
            {
                checkedmsg[i] = msg[i];
            }
            checkedmsg[msg.Length] = crc[1];
            checkedmsg[msg.Length + 1] = crc[0];
            return checkedmsg;
        }
        public bool portWrite(string msg)
        {
            if (!(port.IsOpen == true)) port.Open();
            //send the message to the port
            msg += "\r\n";
            //MessageBox.Show("Message is "+msg + "\n");
            port.Write(msg);
            return true;
        }
        public bool portWrite(ushort[] msg)
        {

            ushort[] checkedmsg = checkedmessage(msg);            
                try
                {
                    if (!(port.IsOpen == true)) port.Open();
                    string q = "";
                    for (int i = 0; i < checkedmsg.Length;i++)
                    {
                        q += " "+checkedmsg[i];
                    }
                    //MessageBox.Show("Checked message is = "+q);
                    port.Write(convertUshorttoByte(checkedmsg), 0, checkedmsg.Length);

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("COMP Port command sending Error\n" + e.Message);
                    return false;
                }

        }
        private byte[] convertUshorttoByte(ushort[] mesg)
        {
            byte[] bytemessage=new byte[mesg.Length];
            //string s = "";
            for(int i=0;i<mesg.Length;i++)
            {

                bytemessage[i] = (byte)(mesg[i]);
                //s += " " + bytemessage[i];

            }
            //MessageBox.Show("Send message= "+s);
                return bytemessage;
        }
       /* private ushort[] convertBytetoUshort()
        {
            //if (readBuffer.Length == 0) MessageBox.Show("Reading Buffer is empty");

            try
            {

                ushort[] hexmessage = new ushort[readBuffer.Count];
                foreach (int i in readBuffer)
                {
                    hexmessage[i] = (ushort)(readBuffer[i]);

                }
                return hexmessage;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                ushort[] hexmessage=new ushort[1];
                hexmessage[0] = 0xffff;
                return hexmessage;
            }
            
        }*/

    }
}
