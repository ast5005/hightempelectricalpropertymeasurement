using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighTemperatureCharacterizationSystem2._0
{
    public static class CRCModbus
    {
        public static ushort[] getcrc(ushort[] message)
        {
               //return the CRC values:  
            ushort crc=0xffff;
            ushort[] crcarray = new ushort[2];
            for (int i = 0; i < (message.Length); i++)
            {
              
                Console.Write("message["+i+"]= "+message[i]+"\n");
                crc = (ushort)(crc ^ message[i]);
                Console.Write(crc + "  " + (i+1) + ". byte iteration\n");

                for (int j = 0; j < 8; j++)
                {                    
                    if ((crc & 0x0001) == 1)
                    {                        
                        crc = (ushort)((crc >> 1));
                        Console.Write(crc + "  " + (j+1) + ". shift if==1\n");
                        crc = (ushort)(crc ^ 0xA001);
                        Console.Write(crc + "  " + (j + 1) + ". shift if==1 after ^\n");

                    }
                    else
                    {
                        crc = (ushort)((crc >> 1));
                        Console.Write(crc + "  " + (j + 1) + ". shift if!=1\n");
                    }
                    
                }
                
            }
            Console.Write(crc+" final result\n");
            crcarray[1] = (ushort)(crc & 0x00ff);
            crcarray[0] = (ushort)((ushort)(crc & 0xff00)>>8);
            return crcarray;            
        }
    }
}
