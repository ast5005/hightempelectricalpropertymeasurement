using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HighTemperatureCharacterizationSystem2._0
{
    class ElectricalCondMeasurement
    {

        public string samplename { get; set; }
        public double mincurr{ get; set;}
        public double maxcurr{get; set;}
        public double numberofsteps { get; set; }
        public double temp { get; set; }
        public double compliance { get; set; }
        public ElectricalCondData data {get; set;}
        public ElectricalCondMeasurement(double imincurr,double imaxcurr,double inumberofsteps,double itemp,double icompliance,string isamplename)
        {
            mincurr = imincurr;
            maxcurr = imaxcurr;
            numberofsteps = inumberofsteps;
            temp = itemp;
            compliance = icompliance;
            samplename = isamplename;
            data = new ElectricalCondData(temp,samplename,System.DateTime.Now);
        }
        public void measure()
        {
            
            KE2400CommandCenter currsource = new KE2400CommandCenter();
            KE2000CommandCenter voltreader = new KE2000CommandCenter();
            currsource.OpenPort();
            voltreader.OpenPort();
            double step = (maxcurr - mincurr) / numberofsteps;
            for (int i = 0; i < numberofsteps; i++)
            {
                data.curr.Add(i*step+mincurr);
                currsource.setcurr(i*step+mincurr);
                currsource.StartOutput();
                data.volt.Add(voltreader.readVoltage());
            }
            currsource.StopOutput();
            LeastSquare ls = new LeastSquare();
            ls.LeastSquareFit(data.curr.ToArray<double>(),data.volt.ToArray<double>(),0,data.curr.Count<double>());
            data.resistance = ls.slope;

        }
        private void switchIVconfig()
        {
            SwitchCommandCenter visacom1 = new SwitchCommandCenter();            
            visacom1.OpenPort();  
            byte[] openSwitches = { 201, 203, 209 };
            byte[] closedSwitches = { 207, 208, 206 };
            visacom1.setSwitches(openSwitches, closedSwitches);            
        }
    }
}
