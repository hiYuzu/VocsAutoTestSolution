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
        private readonly Random random = new Random();
        public Color ColorSelect()
        {
            int R = random.Next(255);
            int G = random.Next(255);
            int B = random.Next(255);
            B = (R + G > 400) ? R + G - 400 : B;
            B = (B > 255) ? 255 : B;
            Console.WriteLine("RGB: " + R + "-" + G + "-" + B);
            return Color.FromRgb((byte)R, (byte)G, (byte)B);
        }
    }
}
