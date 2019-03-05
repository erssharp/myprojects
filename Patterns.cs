﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormatData
{
    class Patterns
    {
        public static Dictionary<string, string> Date { get; } = new Dictionary<string, string>()
        {
            ["YYYY.MM.DD"] = @"\b\d{4}\.((0[1\.9])|(1[0-2]))\.(([0-2]\d)|(3[0-1]))$",
            ["MM.DD.YYYY"] = @"\b((0[1-9])|(1[0-2]))\.(([0-2]\d)|(3[0-1]))\.\d{4}$",
            ["DD.MM.YYYY"] = @"\b(([0-2]\d)|(3[0-1]))\.((0[1-9])|(1[0-2]))\.\d{4}$",
            ["YY.MM.DD"] = @"\b\d{2}\.((0[1-9])|(1[0-2]))\.(([0-2]\d)|(3[0-1]))$",
            ["MM.DD.YY"] = @"\b((0[1-9])|(1[0-2]))\.(([0-2]\d)|(3[0-1]))\.\d{2}$",
            ["DD.MM.YY"] = @"\b(([0-2]\d)|(3[0-1]))\.((0[1-9])|(1[0-2]))\.\d{2}$",

            ["YYYY-MM-DD"] = @"\b\d{4}-((0[1-9])|(1[0-2]))-(([0-2]\d)|(3[0-1]))$",
            ["MM-DD-YYYY"] = @"\b((0[1-9])|(1[0-2]))-(([0-2]\d)|(3[0-1]))-\d{4}$",
            ["DD-MM-YYYY"] = @"\b(([0-2]\d)|(3[0-1]))-((0[1-9])|(1[0-2]))-\d{4}$",
            ["YY-MM-DD"] = @"\b\d{2}-((0[1-9])|(1[0-2]))-(([0-2]\d)|(3[0-1]))$",
            ["MM-DD-YY"] = @"\b((0[1-9])|(1[0-2]))-(([0-2]\d)|(3[0-1]))-\d{2}$",
            ["DD-MM-YY"] = @"\b(([0-2]\d)|(3[0-1]))-((0[1-9])|(1[0-2]))-\d{2}$",

            ["YYYY/MM/DD"] = @"\b\d{4}/((0[1-9])|(1[0-2]))/(([0-2]\d)|(3[0-1]))$",
            ["MM/DD/YYYY"] = @"\b((0[1-9])|(1[0-2]))/(([0-2]\d)|(3[0-1]))/\d{4}$",
            ["DD/MM/YYYY"] = @"\b(([0-2]\d)|(3[0-1]))/((0[1-9])|(1[0-2]))/\d{4}$",
            ["YY/MM/DD"] = @"\b\d{2}/((0[1-9])|(1[0-2]))/(([0-2]\d)|(3[0-1]))$",
            ["MM/DD/YY"] = @"\b((0[1-9])|(1[0-2]))/(([0-2]\d)|(3[0-1]))/\d{2}$",
            ["DD/MM/YY"] = @"\b(([0-2]\d)|(3[0-1]))/((0[1-9])|(1[0-2]))/\d{2}$",

            ["YYYYMMDD"] = @"\b\d{4}((0[1-9])|(1[0-2]))(([0-2]\d)|(3[0-1]))$",
            ["MMDDYYYY"] = @"\b((0[1-9])|(1[0-2]))(([0-2]\d)|(3[0-1]))\d{4}$",
            ["DDMMYYYY"] = @"\b(([0-2]\d)|(3[0-1]))((0[1-9])|(1[0-2]))\d{4}$",
            ["YYMMDD"] = @"\b\d{2}((0[1-9])|(1[0-2]))(([0-2]\d)|(3[0-1]))$",
            ["MMDDYY"] = @"\b((0[1-9])|(1[0-2]))(([0-2]\d)|(3[0-1]))\d{2}$",
            ["DDMMYY"] = @"\b(([0-2]\d)|(3[0-1]))((0[1-9])|(1[0-2]))\d{2}$",
        };

        public static Dictionary<string, string> Time { get; } = new Dictionary<string, string>()
        {
            ["HH:MM"] = @"^\b(([0-1]{0,1}\d)|(2[0-3])):[0-5]\d$",
            ["HH:MM:SS"] = @"^\b(([0-1]{0,1}\d)|(2[0-3])):[0-5]\d:[0-5]\d$",
            ["HHMM"] = @"^\b(([0-1]\d)|(2[0-3]))[0-5]\d$",
            ["HHMMSS"] = @"^\b(([0-1]\d)|(2[0-3]))[0-5]\d[0-5]\d$"
        };

        public static Dictionary<string, string> Decimal { get; } = new Dictionary<string, string>()
        {
            ["D.D"] = @"^\d{0,}\.\d{0,}$",
            ["D"] = @"^\d{0,}$",
            ["D,D"] = @"^\d{0,},\d{0,}$" 
        };
    }
}