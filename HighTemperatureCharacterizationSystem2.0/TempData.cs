using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HighTemperatureCharacterizationSystem2._0
{
    public class TempData
    {
        public double temp1{get; set;}
        public double temp2{get; set;}       
        public double deltaT()
        {
            return temp2 - temp1;
        }
        
    }
}
