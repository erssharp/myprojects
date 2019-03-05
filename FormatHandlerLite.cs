using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FormatData
{
    class FormatHandlerLite
    {
        List<char> colsep = new List<char>()
        {
            '\t',
            ';',
            ' ',
            ','
        };

        public FormatData GetFullData(string[] doc)
        {
            int sr = GetSkipRowsCount(doc);
            char colsep = GetColumnSeparator(doc, sr);
            int datetime = GetDateTimeColumn(doc, sr, colsep);

            if (datetime != -1)
                for (int i = sr; i < doc.Length; i++)
                    doc[i] = doc[i].Replace(' ', colsep);

            int date = GetDateColumn(doc, colsep, sr);
            string datef = "";
            Task task1 = Task.Factory.StartNew(() => { datef = GetDateFormat(doc, date, colsep, sr); });
            int time = GetTimeColumn(doc, sr, colsep, date);
            char decsep = '\0';
            string timef = "";
            Task task2 = Task.Factory.StartNew(() => { decsep = GetDecimalSeparator(doc, sr, date, time, colsep); });
            Task task3 = Task.Factory.StartNew(() => { timef = GetTimeFormat(doc, time, colsep, sr); });
            int vol = GetVolumeColumn(doc, sr, date, time, colsep);
            Task.WaitAll(task2);
            int high = GetHighColumn(doc, sr, date, time, vol, colsep, decsep);
            int low = GetLowColumn(doc, sr, date, time, vol, colsep, decsep);
            Dictionary<string, int> opcl = GetOpenCloseColumns(doc, sr, high, low, date, time, vol, colsep);
            int open = opcl["Open"];
            int close = opcl["Close"];

            Task.WaitAll(task1, task3);
            string timeframe = "";
            timeframe = GetTimeFrame(doc, sr, date, time, timef, datef, colsep);

            if (datetime != -1)
            {
                date = -1;
                time = -1;
            }

            FormatData data = new FormatData()
            {
                SkipRows = sr,
                ColumnSeparator = colsep,
                DecimalSeparator = decsep,
                TimeColumn = time,
                DateColumn = date,
                DateTimeColumn = datetime,
                DateFormat = datef,
                TimeFormat = timef,
                OpenColumn = open,
                HighColumn = high,
                LowColumn = low,
                VolumeColumn = vol,
                CloseColumn = close,
                TimeFrame = timeframe
            };

            return data;
        }

        public char GetColumnSeparator(string[] doc, int sr)
        {
            string row = doc[sr];
            foreach (char sep in colsep)
            {
                var arr = row.Split(sep);
                if (arr.Length >= 6)
                    return sep;
            }
            throw new Exception("Wrong column separator"); ;
        }

        public int GetSkipRowsCount(string[] doc)
        {
            for (int i = 0; i < doc.Length; i++)
            {
                string pattern = @"(((\w+)|(\d{2,4}(/|\.|-)?\d{2,4}(/|\.|-)?\d{2,4})|(\d+(\.|,)?\d{0,}))(\t|,|;| )){4,}((\w+)|(\d{2,4}(/|\.|-)?\d{2,4}(/|\.|-)?\d{2,4})|(\d+(\.|,)?\d{0,}))";
                string words = @"(\w+\s?)*";
                if (!Regex.IsMatch(doc[i], pattern) && Regex.IsMatch(doc[i], words))
                    continue;
                return i;
            }
            return -1;
        }

        public int GetDateColumn(string[] doc, char colsep, int sr)
        {
            string[] arr = doc[sr].Split(colsep);
            foreach (string column in arr)
            {
                foreach (KeyValuePair<string, string> pair in Patterns.Date)
                {
                    if (Regex.IsMatch(column, pair.Value))
                        return arr.ToList().IndexOf(column);
                }
            }
            return -1;
        }

        public string GetDateFormat(string[] doc, int d, char colsep, int sr)
        {
            Dictionary<string, string> patterns = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> pair in Patterns.Date)
                patterns.Add(pair.Key, pair.Value);

            List<string> dates = new List<string>();

            int dayind = Day(doc, sr, d, colsep);

            if (d == -1)
                return "Date is missing";

            string prdate = string.Empty;
            int ind = d;

            for (int i = 0; i < doc.Length - sr; i++)
            {
                string[] arr = doc[sr + i].Split(colsep);
                string date = arr[ind];
                if (date == prdate)
                    continue;
                else prdate = date;

                List<string> keys = new List<string>();

                foreach (KeyValuePair<string, string> pair in patterns)
                {
                    if (!Regex.IsMatch(date, pair.Value) || pair.Key.ToCharArray()[dayind] != 'D')
                        keys.Add(pair.Key);
                }

                foreach (string key in keys)
                    patterns.Remove(key);

                if (patterns.Count == 1)
                    return patterns.Keys.ToArray()[0];
            }

            return "Format can't be determined";
        }

        int Day(string[] doc, int sr, int d, char cs)
        {
            for (int i = sr; i < doc.Length - 1; i++)
            {
                string date1 = doc[i].Split(cs)[d];
                string date2 = doc[i + 1].Split(cs)[d];
                if (date1 != date2)
                {
                    char[] arr1 = date1.ToCharArray();
                    char[] arr2 = date2.ToCharArray();
                    for (int j = 0; j < date1.Length; j++)
                    {
                        if (arr1[j] != arr2[j])
                            return j;
                    }
                }
            }
            return -1;
        }

        public int GetTimeColumn(string[] doc, int sr, char cs, int dc)
        {
            Dictionary<int, int> indcount = new Dictionary<int, int>();
            for (int i = sr; i < sr + 100; i++)
            {
                string[] arr = doc[sr + i].Split(cs);
                foreach (string column in arr)
                {
                    int j = arr.ToList().IndexOf(column);
                    if (column.Contains(':'))
                        return arr.ToList().IndexOf(column);
                    if (j == dc)
                        continue;

                    foreach (KeyValuePair<string, string> pair in Patterns.Time)
                    {
                        if (Regex.IsMatch(column, pair.Value))
                            try
                            {
                                indcount[arr.ToList().IndexOf(column)]++;
                            }
                            catch (Exception)
                            {
                                indcount.Add(arr.ToList().IndexOf(column), 1);
                            }
                    }
                }
            }

            List<KeyValuePair<int, int>> dict = indcount.ToList();
            dict.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
            try
            {
                if (dict.Last().Value == 100)
                    return dict.Last().Key;
                else return -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public string GetTimeFormat(string[] doc, int tc, char cs, int sr)
        {
            Dictionary<string, string> patterns = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> pair in Patterns.Time)
                patterns.Add(pair.Key, pair.Value);

            if (tc == -1)
                return "Time is missing";

            for (int i = 0; i < doc.Length - sr; i++)
            {
                string[] arr = doc[sr + i].Split(cs);
                string time = arr[tc];

                foreach (KeyValuePair<string, string> pair in Patterns.Time)
                {
                    if (!Regex.IsMatch(time, pair.Value))
                        patterns.Remove(pair.Key);
                }
                if (patterns.Count == 1)
                    return patterns.Keys.ToArray()[0];
            }
            return "Format can't be determined";
        }

        public char GetDecimalSeparator(string[] doc, int sr, int dc, int tc, char cs)
        {
            string row = doc[GetSkipRowsCount(doc)];
            string[] arr = row.Split(cs);
            foreach (string word in arr)
                foreach (KeyValuePair<string, string> pair in Patterns.Decimal)
                    if (Regex.IsMatch(word, pair.Value))
                    {
                        if (arr.ToList().IndexOf(word) == dc || arr.ToList().IndexOf(word) == tc)
                            continue;
                        if (pair.Key == "D.D" || pair.Key == "D")
                            return '.';
                        else if (pair.Key == "D,D")
                            return ',';
                    }
            throw new Exception("Wrong decimal separatoe");
        }

        public string GetTimeFrame(string[] doc, int sr, int dc, int tc, string tf, string df, char cs)
        {
            for (int i = 0; i < doc.Length - 1; i++)
            {
                string tframe = GetTF(doc, i, sr, dc, tc, tf, df, cs);
                if (tframe != "Wrong Timeframe")
                    return tframe;
            }
            return "Wrong Timeframe";
        }

        string GetTF(string[] doc, int ind, int sr, int dc, int tc, string tf, string df, char cs)
        {
            string[] tframes = { "M1", "H1", "D1" };

            if (dc != -1)
            {
                string[] dates =
                {
                    doc[sr + ind].Split(cs)[dc],
                    doc[sr + ind + 1].Split(cs)[dc]
                };
                string DateFormat = df;
                int i = df.IndexOf("D");
                string d1 = dates[0].Substring(i, 2);
                string d2 = dates[1].Substring(i, 2);
                int.TryParse(d1, out int i1);
                int.TryParse(d2, out int i2);
                if (Math.Abs(i1 - i2) == 1)
                    return tframes[2];
            }

            if (tc != -1)
            {
                string[] times =
                {
                    doc[sr + ind].Split(cs)[tc],
                    doc[sr + ind + 1].Split(cs)[tc]
                };

                int hi = tf.IndexOf("H");
                int mi = tf.IndexOf("M");
                string h = times[0].Substring(hi, 2);
                string m = times[0].Substring(mi, 2);
                string h1 = times[1].Substring(hi, 2);
                string m1 = times[1].Substring(mi, 2);
                int.TryParse(h, out int hn);
                int.TryParse(m, out int mn);
                int.TryParse(h1, out int hn1);
                int.TryParse(m1, out int mn1);
                if (Math.Abs(mn1 - mn) == 0 && Math.Abs(hn - hn1) == 1)
                    return tframes[1];
                if (Math.Abs(mn1 - mn) == 1 && Math.Abs(hn - hn1) == 0)
                    return tframes[0];
            }

            return "Wrong Timeframe";
        }

        public int GetVolumeColumn(string[] doc, int sr, int dc, int tc, char cs)
        {
            int volume = 0;
            List<string> row = new List<string>();
            string[] first = doc[sr].Split(cs);

            for (int j = 0; j < first.Length; j++)
            {
                if (j == dc || j == tc || first[j] == string.Empty)
                    continue;

                foreach (KeyValuePair<string, string> pair in Patterns.Decimal)
                    if (Regex.IsMatch(first[j], pair.Value))
                        row.Add(first[j]);
            }

            if (row.Count == 5)
                volume = first.ToList().LastIndexOf(row[4]);
            else
                volume = -1;

            return volume;
        }

        public int GetHighColumn(string[] doc, int sr, int dc, int tc, int v, char cs, char ds)
        {
            for (int i = sr; i < doc.Length; i++)
            {
                List<string> row = new List<string>();
                string[] first = doc[i].Split(cs);

                for (int j = 0; j < first.Length; j++)
                {
                    if (j == dc || j == tc || first[j] == string.Empty || j == v)
                        continue;

                    foreach (KeyValuePair<string, string> pair in Patterns.Decimal)
                        if (Regex.IsMatch(first[j], pair.Value))
                            row.Add(first[j]);
                }

                for (int j = 0; j < row.Count; j++)
                {
                    if (ds == '.')
                        row[j] = row[j].Replace('.', ',');
                }

                KeyValuePair<int, double> mx = MaxCount(row);

                if (mx.Key == 1)
                {
                    if (ds == '.')
                        return first.ToList().IndexOf(mx.Value.ToString().Replace(',', '.'));
                    else return first.ToList().IndexOf(mx.Value.ToString());
                }
            }
            return -1;
        }

        public int GetLowColumn(string[] doc, int sr, int dc, int tc, int v, char cs, char ds)
        {
            for (int i = sr; i < doc.Length; i++)
            {
                List<string> row = new List<string>();
                string[] first = doc[i].Split(cs);

                for (int j = 0; j < first.Length; j++)
                {
                    if (j == dc || j == tc || first[j] == string.Empty || j == v)
                        continue;

                    foreach (KeyValuePair<string, string> pair in Patterns.Decimal)
                        if (Regex.IsMatch(first[j], pair.Value))
                            row.Add(first[j]);
                }

                for (int j = 0; j < row.Count; j++)
                {
                    if (ds == '.')
                        row[j] = row[j].Replace('.', ',');
                }

                KeyValuePair<int, double> mn = MinCount(row);

                if (mn.Key == 1)
                    if (ds == '.')
                        return first.ToList().IndexOf(mn.Value.ToString().Replace(',', '.'));
                    else return first.ToList().IndexOf(mn.Value.ToString());
            }
            return -1;
        }

        public Dictionary<string, int> GetOpenCloseColumns(string[] doc, int sr, int hc, int lc, int dc, int tc, int v, char cs)
        {
            Dictionary<string, int> openclose = new Dictionary<string, int>();
            int[] voc = new int[2];
            List<string> str = new List<string>();

            for (int i = sr; i < doc.Length - 1; i++)
            {
                string[] row = doc[i].Split(cs);
                if (row.Distinct().Count() == row.Length)
                {
                    for (int j = 0; j < row.Length; j++)
                    {
                        if (j == dc || j == tc || row[j] == string.Empty || j == v || j == hc || j == lc)
                            continue;
                        foreach (KeyValuePair<string, string> pair in Patterns.Decimal)
                            if (Regex.IsMatch(row[j], pair.Value))
                                str.Add(row[j]); ;
                    }
                    voc[0] = row.ToList().IndexOf(str[1]);
                    voc[1] = row.ToList().IndexOf(str[0]);
                    Array.Sort(voc);
                    break;
                }
            }

            openclose.Add("Open", voc[0]);
            openclose.Add("Close", voc[1]);
            return openclose;
        }

        public int GetDateTimeColumn(string[] doc, int sr, char cs)
        {
            string[] row = doc[sr].Split(cs);
            foreach (string word in row)
            {
                bool date = false;
                bool time = false;
                string[] arr = word.Split(' ');

                if (arr.Length == 2)
                {
                    foreach (KeyValuePair<string, string> pair in Patterns.Date)
                        foreach (string s in arr)
                            if (Regex.IsMatch(s, pair.Value))
                                date = true;

                    foreach (KeyValuePair<string, string> pair in Patterns.Time)
                        foreach (string s in arr)
                            if (Regex.IsMatch(s, pair.Value))
                                time = true;

                    if (date && time)
                    {
                        return row.ToList().IndexOf(word);
                    }
                }
            }
            return -1;
        }

        //Подсчет минимумов 
        KeyValuePair<int, double> MinCount(List<string> row)
        {
            List<double> d = new List<double>();
            foreach (string v in row)
                d.Add(double.Parse(v));

            double min = double.MaxValue;
            int mincount = 0;

            for (int i = 0; i < d.Count; i++)
            {
                if (d[i] < min)
                {
                    min = d[i];
                    mincount = 1;
                }
                else if (d[i] == min)
                {
                    mincount++;
                }
            }
            KeyValuePair<int, double> maxcnt = new KeyValuePair<int, double>(mincount, min);

            return maxcnt;
        }

        //Подсчет максимумов
        KeyValuePair<int, double> MaxCount(List<string> row)
        {
            List<double> d = new List<double>();
            foreach (string v in row)
                d.Add(double.Parse(v));

            double max = 0;
            int maxcount = 0;

            for (int i = 0; i < d.Count; i++)
            {
                if (d[i] > max)
                {
                    max = d[i];
                    maxcount = 1;
                }
                else if (d[i] == max)
                {
                    maxcount++;
                }
            }
            KeyValuePair<int, double> maxcnt = new KeyValuePair<int, double>(maxcount, max);

            return maxcnt;
        }

        //Вспомогательная функция для сравнения значений двух строк 
        bool Compare(string s1, string s2)
        {
            return double.Parse(s1) >= double.Parse(s2);
        }
    }
}
