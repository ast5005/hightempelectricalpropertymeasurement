using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HighTemperatureCharacterizationSystem2._0
{
    class TempCommandCenter
    {
        
        serialCom sc1;
        string baudrate = "9600";
        string parity = "Odd";
        string stopbit = "One";
        string length = "7";
        string name = "COM4";
        int address=0;
        double setvalueInf = 0;
        double procvalueInf = 0;
        public TempCommandCenter()
        {
            address = 1;
        }
        public double getSetValueI()
        {
            sc1 = new serialCom(baudrate, parity, stopbit, length, name);
            sc1.OpenPort();
            //ushort[] mesg = { 0x01, 0x03, 0x00, 0x01, 0x00, 0x01 };
            //string mesg = "*W01200069";
            string mesg = "*R01";
            bool success = sc1.portWrite(mesg);
            //MessageBox.Show("Success of fail "+success);
           System.Threading.Thread.Sleep(2000);
            List<byte> reading = sc1.portRead();
            //List<int> offset = sc1.gettheoffset();
            string hexstring = "";
            //if(reading.Count!=0)
           //{
               for (int i = 0; i < 5; i++)
               {
                   //hexArray[i] = Convert.ToChar(buffer[bytes - 5 + i]);
                   // s+=hexArray[i];
                   hexstring += Convert.ToChar(reading[reading.Count - 5 + i]);

               }
            //double readvalue = double.Parse(s);
            //MessageBox.Show(" Hex array "+s);
            int readvalue = Int32.Parse(hexstring, System.Globalization.NumberStyles.HexNumber);
            setvalueInf = (readvalue % 10) * 0.1 + readvalue / 10;
           // MessageBox.Show("Set Value "+setvalueInf);
            //}
            sc1.ClosePort();
            //this.reset();
            /*if (sc1.checkmsg())
            {               
                string w = "";
                for (int m = 0; m < offset.Count(); m++)
                {
                    w += " " + offset[m];
                }
                int count = offset.Count() - 1;
                w +="\n" +count + ". reading :\n";
                int k = offset.Sum() - offset[0];
                int l = reading.Count();
                // MessageBox.Show("Starting= "+k+"  Finish="+l);
                for (int i = k; i < l; i++)
                {
                    w += reading[i] + " ";
                }
                float result = (float)((reading[k + 3] * 0x100 + reading[k + 4]) / 10.0);
                //float result = (float)((reading[k + 4] * 0x100 + reading[k + 5]) / 10.0);
                //w += "\n " + result + "\npart1 " + reading[k + 3] * 0x100 + "\npart2 " + reading[k + 4];
                // w += "\n";
                //MessageBox.Show("offset= " + w);

                return result;
            }
            else return -1;*/
            return setvalueInf;

        }
        public double getProcValueI()
        {
            sc1 = new serialCom(baudrate, parity, stopbit, length, name);
            sc1.OpenPort();
            string mesg = "*X01";
            bool success = sc1.portWrite(mesg);
            List<byte> reading = sc1.portRead();
            System.Threading.Thread.Sleep(1000);
            string hexstring = "";
           
            for (int i = reading.Count-7; i < reading.Count - 2; i++)
            {
                //hexArray[i] = Convert.ToChar(buffer[bytes - 5 + i]);
                // s+=hexArray[i];
                hexstring += Convert.ToChar(reading[i]);
            }
            //double readvalue = double.Parse(s);
            //MessageBox.Show(" Hex array "+s);
            //MessageBox.Show("hexstring string in getProcValue "+hexstring);
            procvalueInf =Convert.ToDouble(hexstring);            
            //MessageBox.Show(" getProcValue " + procvalueInf);
              sc1.ClosePort();
            return procvalueInf;
        }
        public float getProcValue()
        {
            sc1 = new serialCom(baudrate, parity, stopbit, length, name);
            sc1.OpenPort();
            ushort[] mesg = { 0x01, 0x03, 0x00, 0x27, 0x00, 0x01 };
            bool success = sc1.portWrite(mesg);
            List<byte> reading = sc1.portRead();
            List<int> offset = sc1.gettheoffset();
            sc1.ClosePort();
            //MessageBox.Show("check message is coming" + sc1.checkmsg());
            if (sc1.checkmsg())
            {
                
                //string w = "";
                /*for (int m = 0; m < offset.Count(); m++)
                {
                    w += " " + offset[m];
                }*/
                int count = offset.Count() - 1;
                // w +="\n" +count + ". reading :\n";
                int k = offset.Sum() - offset[0];
                int l = reading.Count();
                // MessageBox.Show("Starting= "+k+"  Finish="+l);
                /*for (int i = k; i < l; i++)
                {
                    w += reading[i] + " ";
                }*/
                float result = (float)((reading[k + 3] * 0x100 + reading[k + 4]) / 10.0);
                //float result = (float)((reading[k + 4] * 0x100 + reading[k + 5]) / 10.0);
                //w += "\n " + result + "\npart1 " + reading[k + 3] * 0x100 + "\npart2 " + reading[k + 4];
                // w += "\n";
                // MessageBox.Show("offset= " + w);

                return result;
            }
            else return -1;

        }
        public int setsetvalueI(double setpoint)
        {
            string mesg = "*W0120";//069";
            int setpointint = (int)(setpoint * 10);
            byte[] setpointmsg = BitConverter.GetBytes(setpointint);
            string h = setpointint + "";
            ushort[] temp=new ushort[2];
            for (int i = 0; i < setpointmsg.Length; i++)
            {
                h += " " + setpointmsg[i];
            }    
            mesg+=BitConverter.ToString(setpointmsg, 1, 1) + BitConverter.ToString(setpointmsg, 0, 1);
            //MessageBox.Show(h + "  " +mesg);
            sc1 = new serialCom(baudrate, parity, stopbit, length, name);
            sc1.OpenPort();            
            bool success = sc1.portWrite(mesg);
            System.Threading.Thread.Sleep(2000);
            sc1.ClosePort();
            //MessageBox.Show("written");
            return 1;
        }
        public int reset()
        {
            string mesg = "*Z02";//069";       
            sc1 = new serialCom(baudrate, parity, stopbit, length, name);
            sc1.OpenPort();
            bool success = sc1.portWrite(mesg);
            System.Threading.Thread.Sleep(2000);
            sc1.ClosePort();
            //MessageBox.Show("written");
            return 1;
        }
        public float setsetvalue(float setpoint)           
        {
            sc1 = new serialCom(baudrate, parity, stopbit, length, name);
            sc1.OpenPort();
            //MessageBox.Show("Port Opened");
            int setpointint = (int)(setpoint*10);
            
            byte[] setpointmsg=BitConverter.GetBytes(setpointint);
            string h = setpointint+"";
            for (int i = 0; i < setpointmsg.Length; i++)
            {
                h += " "+setpointmsg[i];
            }
            //MessageBox.Show(h);
            ushort[] mesg = { 0x01, 0x06, 0x00, 0x01, setpointmsg[1], setpointmsg[0] };
            bool success = sc1.portWrite(mesg);
            List<byte> reading = sc1.portRead();
            List<int> offset = sc1.gettheoffset();
            sc1.ClosePort();
            if (sc1.checkmsg())
            {
                
                string w = "";
                /*for (int m = 0; m < offset.Count(); m++)
                {
                    w += " " + offset[m];
                }*/
                int count = offset.Count() - 1;
                // w +="\n" +count + ". reading :\n";
                int k = offset.Sum() - offset[0];
                int l = reading.Count();
                 MessageBox.Show("Starting= "+k+"  Finish="+l);
                for (int i = k; i < l; i++)
                {
                    w += reading[i] + " ";
                }
                float result = (float)((reading[k + 4] * 0x100 + reading[k + 5]) / 10.0);
                w += "\n " + result + "\npart1 " + reading[k + 3] * 0x100 + "\npart2 " + reading[k + 4];
                 w += "\n";
                MessageBox.Show("offset= " + w);

                return result;
            }
            else return -1;

        }
    }
}
