using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace FormatData
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> doc = new List<string>();

            FormatHandlerLite fh = new FormatHandlerLite();
            Stopwatch sw = new Stopwatch();

            string[] arr1 = GetText("SP500.txt").ToArray();
            Console.WriteLine("SP500");
            sw.Start();
            FormatData data1 = fh.GetFullData(arr1);
            sw.Stop();
            Console.WriteLine($"Time: {sw.Elapsed}");
            data1.Show();

            arr1 = GetText("EURUSD.txt").ToArray();
            Console.WriteLine("EURUSD");
            sw.Restart();
            data1 = fh.GetFullData(arr1);
            sw.Stop();
            data1.Show();
            Console.WriteLine($"Time: {sw.Elapsed}");

            Console.WriteLine("AAPL");
            arr1 = GetText("AAPL.txt").ToArray();
            sw.Restart();
            data1 = fh.GetFullData(arr1);
            sw.Stop();
            data1.Show();
            Console.WriteLine($"Time: {sw.Elapsed}");

            //for (int i = 56; i <= 70; i++)
            //{
            //    //try
            //    string[] arr = GetText(@"Tests\" + i.ToString() + ".txt").ToArray();
            //    sw.Restart();
            //    FormatData data = fh.GetFullData(arr);
            //    sw.Stop();
            //    Console.WriteLine(sw.Elapsed + " " + data.TimeFrame );
            //    WriteText(data, (@"Out\" + i + ".txt"));

            //    //catch (Exception e)

            //    //FormatData data = new FormatData();
            //    //data.DateFormat = e.Message;
            //    //WriteText(data, (@"Out\" + i + ".txt"));

            //}



            Console.ReadKey();
        }

        static public void WriteText(FormatData data, string path)
        {
            //File.Create(path);
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine($"SkipRows = {data.SkipRows}");
                sw.WriteLine($"ColumnSeparator = {data.ColumnSeparator}");
                sw.WriteLine($"DecimalSeparator = {data.DecimalSeparator}");
                sw.WriteLine($"DateFormat = {data.DateFormat}");
                sw.WriteLine($"TimeFormat = {data.TimeFormat}");
                sw.WriteLine($"TimeFrame = {data.TimeFrame}");
                sw.WriteLine($"DateColumn = {data.DateColumn}");
                sw.WriteLine($"TimeColumn = {data.TimeColumn}");
                sw.WriteLine($"OpenColumn = {data.OpenColumn}");
                sw.WriteLine($"HighColumn = {data.HighColumn}");
                sw.WriteLine($"LowColumn = {data.LowColumn}");
                sw.WriteLine($"CloseColumn = {data.CloseColumn}");
                sw.WriteLine($"VolumeColumn = {data.VolumeColumn}");
                sw.WriteLine($"DateTimeColumn = {data.DateTimeColumn}");
            }
        }

        static public List<string> GetText(string path)
        {
            List<string> doc = new List<string>();
            using (StreamReader sr = new StreamReader(path))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    doc.Add(line);
                }
            }
            return doc;
        }
    }
}
