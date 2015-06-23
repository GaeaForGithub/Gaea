using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gaea.Net.UI
{
    public class RunTime
    {
        private static DateTime startTime = DateTime.Now;

        public static string GetRunTimeInfo()
        {
            DateTime now = DateTime.Now;

            TimeSpan ts = now.Subtract(startTime);
            string rvalue = "";
            if (ts.Days > 0)
            {
                rvalue = rvalue + ts.Days.ToString() + " d ";
            }

            if (ts.Hours > 0)
            {
                rvalue = rvalue + ts.Hours.ToString() + " h ";
            }

            if (ts.Minutes > 0)
            {
                rvalue = rvalue + ts.Minutes.ToString() + " m ";
            }

            if (ts.Seconds > 0)
            {
                rvalue = rvalue + ts.Seconds.ToString() + " s ";
            }

            return rvalue;

        }
    }
}
