using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace SeemoPredictor
{
    public class ColorGenerator
    {
        public static double Remap(double value, double from1, double to1, double from2, double to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public static Color UDRED = Color.FromArgb(255, 255, 0, 55);
        public static Color UDBLUE = Color.FromArgb(255, 0, 109, 255);
        public static Color UDYELLOW = Color.FromArgb(255, 255, 255, 0);

        public static void WriteGradianImg ( string path, Color left, Color centre, Color right)
        {
            Bitmap bmp = new Bitmap(100, 20, PixelFormat.Format24bppRgb);
            double w = (double)bmp.Width;
            for (int x = 0; x<bmp.Width; x++)
            {
                Color c = GetTriColour(x / w, left, centre, right);
                for (int y = 0; y < bmp.Height; y++)
                    bmp.SetPixel(x, y, c);

            }
            bmp.Save(path, ImageFormat.Png);
        }
        
        public static Color GetTriColour(double percent, Color left, Color centre, Color right)
        {
            if (percent < 0 || percent > 1)
                throw new Exception("Percent must be between 0 and 1");

            //double weight = Math.Sin(percent * Math.PI);
            double weight = (Math.Cos((percent * 2 - 1) * Math.PI) + 1) / 2;

            return GetColourFromLinearGradient(weight,
                percent < 0.5 ? left : right, centre);
        }

        //public static Eto.Drawing.Color GetTriColour(double percent, Color left, Color centre, Color right)
        //{
        //    if (percent < 0 || percent > 1)
        //        throw new Exception("Percent must be between 0 and 1");

        //    //double weight = Math.Sin(percent * Math.PI);
        //    double weight = (Math.Cos((percent * 2 - 1) * Math.PI) + 1) / 2;

        //    return GetColourFromLinearGradient(weight,
        //        percent < 0.5 ? left : right, centre);
        //}

        public static Color GetColourFromLinearGradient(double percent, Color start, Color end)
        {
            double a, r, g, b;

            if (percent < 0 || percent > 1)
                throw new Exception("Percent must be between 0 and 1");

            double npercent = 1.0 - percent;

            a = Math.Min(start.A, end.A) + Math.Abs(start.A - end.A) * (start.A > end.A ? npercent : percent);
            r = Math.Min(start.R, end.R) + Math.Abs(start.R - end.R) * (start.R > end.R ? npercent : percent);
            g = Math.Min(start.G, end.G) + Math.Abs(start.G - end.G) * (start.G > end.G ? npercent : percent);
            b = Math.Min(start.B, end.B) + Math.Abs(start.B - end.B) * (start.B > end.B ? npercent : percent);

            return Color.FromArgb((int)a, (int)r, (int)g, (int)b);
        }

        //public static Eto.Drawing.Color GetColourFromLineaerGradient(double percent, Eto.Drawing.Color start, Eto.Drawing.Color end)
        //{
        //    double a, r, g, b;

        //    if (percent < 0 || percent > 1)
        //        throw new Exception("Percent must be btween 0 and 1");

        //    double npercent = 1.0 - percent;

        //    a = Math.Min(start.A, end.A) + Math.Abs(start.A - end.A) * (start.A > end.A ? npercent : percent);
        //    r = Math.Min(start.R, end.R) + Math.Abs(start.R - end.R) * (start.R > end.R ? npercent : percent);
        //    g = Math.Min(start.G, end.G) + Math.Abs(start.G - end.G) * (start.G > end.G ? npercent : percent);
        //    b = Math.Min(start.B, end.B) + Math.Abs(start.B - end.B) * (start.B > end.B ? npercent : percent);

        //    return Eto.Drawing.Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255), (int)(a * 255));

        //}

    }
}
