using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormatData
{
    struct FormatData
    {
        public int SkipRows { get; set; }                //Count of rows to skip
        public char ColumnSeparator { get; set; }        //'\t', ';', ' ', ','
        public char DecimalSeparator { get; set; }       //',' or '.'
        public int DateColumn { get; set; }
        public int TimeColumn { get; set; }
        public int DateTimeColumn { get; set; }
        public string DateFormat { get; set; }           
        public string TimeFormat { get; set; }
        public int OpenColumn { get; set; }             
        public int HighColumn { get; set; }
        public int LowColumn { get; set; }
        public int CloseColumn { get; set; }
        public string TimeFrame { get; set; }
        public int VolumeColumn { get; set; }


        public void Show()
        {
            Console.WriteLine("SkipRows = {0}", SkipRows);
            Console.WriteLine("ColumnSeparator = {0}", ColumnSeparator);
            Console.WriteLine("DecimalSeparator = {0}", DecimalSeparator);
            Console.WriteLine("DateFormat = {0}", DateFormat);
            Console.WriteLine("TimeFormat = {0}", TimeFormat);
            Console.WriteLine("TimeFrame = {0}", TimeFrame);
            Console.WriteLine("DateColumn = {0}", DateColumn);
            Console.WriteLine("TimeColumn = {0}", TimeColumn);
            Console.WriteLine("OpenColumn = {0}", OpenColumn);
            Console.WriteLine("HighColumn = {0}", HighColumn);
            Console.WriteLine("LowColumn = {0}", LowColumn);
            Console.WriteLine("CloseColumn = {0}", CloseColumn);
            Console.WriteLine("VolumeColumn = {0}", VolumeColumn);
            Console.WriteLine("DateTimeColumn = {0}", DateTimeColumn);
        }
    }

}
