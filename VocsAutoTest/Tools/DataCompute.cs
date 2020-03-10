using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VocsAutoTest.Tools
{
    public class DataCompute
    {
        long computeTimes = 1;
        double maxConc = double.MinValue;
        double minConc = double.MaxValue;
        double subConc = 0;
        double subPowConc = 0;
        double aver = 0;
        double std = 0;
        double curConc = 0;
        public void Reset()
        {
            computeTimes = 1;
            maxConc = double.MinValue;
            minConc = double.MaxValue;
            subConc = 0;
            subPowConc = 0;
        }
        public long GetCount()
        {
            return computeTimes - 1;
        }

        public void AddData(double conc)
        {
            try
            {
                curConc = conc;
                if (conc >= maxConc)
                {
                    maxConc = conc;
                }
                if (conc <= minConc)
                {
                    minConc = conc;
                }
                if (0 != computeTimes)
                {
                    subConc += conc;
                    subPowConc += (conc * conc);
                    aver = subConc / computeTimes;
                    std = subPowConc / computeTimes - aver * aver;
                }
                else
                {
                    subConc = conc;
                    subPowConc = (conc * conc);
                    aver = subConc;
                    std = 0;
                }
                if (std > 0)
                {
                    std = Math.Sqrt(std);
                }
                else
                {
                    std = 0;
                }
                computeTimes++;
            }
            catch
            {
                subConc = 0;
                subPowConc = 0;
                computeTimes = 1;
            }
        }

        public float GetCurValue()
        {
            return (float)curConc;
        }
        public float GetMaxValue()
        {
            return (float)maxConc;
        }

        public float GetMinValue()
        {
            return (float)minConc;
        }

        public float GetAvgValue()
        {
            return (float)aver;
        }

        public float GetCorValue()
        {
            return (float)std;

        }
    }
}
