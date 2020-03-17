using System;
using System.IO;
using System.Threading;

namespace VocsAutoTest.Algorithm
{
    class FileControl
    {
        public static void SaveMatrix(double[,] data, string fileName, int d, string format, string head, string tail)
        {
            TextWriter textWriter = null;
            try
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
                textWriter = File.CreateText(fileName);
                if (head != null)
                    textWriter.WriteLine(head);

                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-us");

                for (int i = 0; i < data.GetLength(0); i++)
                {
                    string line = "";
                    for (int j = 0; j < data.GetLength(1); j++)
                    {
                        line = line + (data[i, j] * d).ToString(format) + "\t";
                    }
                    textWriter.WriteLine(line);
                }
                if (tail != null)
                    textWriter.WriteLine(tail);
            }
            catch (Exception)
            {
            }
            finally
            {
                if (textWriter != null)
                    textWriter.Close();
            }
        }
    }
}
