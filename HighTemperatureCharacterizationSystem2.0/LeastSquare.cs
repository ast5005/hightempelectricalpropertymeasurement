using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HighTemperatureCharacterizationSystem2._0
{
    class LeastSquare
    {
        public double goodfit { get; set; }
        public double slope { get; set; }
        public double yintercept { get; set; }
        public LeastSquare()
        {
            goodfit = 0;
            slope = 0;
            yintercept = 0;
        }
        public void LeastSquareFit(double[] xVals, double[] yVals,int inclusiveStart,int exclusiveEnd)
        {
            double sumOfX = 0;
            double sumOfY = 0;
            double sumOfXSq = 0;
            double sumOfYSq = 0;
            double ssX = 0;
            double ssY = 0;
            double sumCodeviates = 0;
            double sCo = 0;
            double count = exclusiveEnd - inclusiveStart;
 
            for (int ctr = inclusiveStart; ctr < exclusiveEnd; ctr++)
            {
              
                double x = xVals[ctr];
                double y = yVals[ctr];
                sumCodeviates += x * y;
                sumOfX += x;
                sumOfY += y;
                sumOfXSq += x * x;
                sumOfYSq += y * y;
            }
            ssX = sumOfXSq - ((sumOfX * sumOfX) / count);
            ssY = sumOfYSq - ((sumOfY * sumOfY) / count);
            double RNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
            double RDenom = (count * sumOfXSq - (sumOfX * sumOfX))
             * (count * sumOfYSq - (sumOfY * sumOfY));
            sCo = sumCodeviates - ((sumOfX * sumOfY) / count);
 
            double meanX = sumOfX / count;
            double meanY = sumOfY / count;
            double dblR = RNumerator / Math.Sqrt(RDenom);
            goodfit = dblR * dblR;
            yintercept = meanY - ((sCo / ssX) * meanX);
            slope = sCo / ssX;
        }

    }
}
