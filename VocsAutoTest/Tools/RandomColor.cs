using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace VocsAutoTest.Tools
{
    class RandomColor
    {
        public static Color ColorSelect()
        {
            long tick = DateTime.Now.Ticks;
            Random random = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));

            int R = random.Next(255);
            int G = random.Next(255);
            int B = random.Next(255);
            B = (R + G > 400) ? R + G - 400 : B;
            B = (B > 255) ? 255 : B;
            return Color.FromRgb((byte)R, (byte)G, (byte)B);
        }
    }
}
