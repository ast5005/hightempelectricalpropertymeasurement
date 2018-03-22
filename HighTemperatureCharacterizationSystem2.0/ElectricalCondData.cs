using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HighTemperatureCharacterizationSystem2._0
{
    class ElectricalCondData
    {
        public List<double> curr { get; set; }
        public List<double> volt { get; set; }
        public double temp { get; set; }
        public DateTime recordtime { get; set; }
        public string samplename { get; set; }
        public double resistance { get; set; }
        public ElectricalCondData(double itemp,string isamplename,DateTime irecordtime)
        {
            curr=new List<double>();
            volt=new List<double>();
            temp = itemp;
            samplename = isamplename;
            recordtime = irecordtime;
        }

    }
}
