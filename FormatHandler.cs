using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace FormatData
{
    class FormatHandler
    {
        List<char> colsep = new List<char>()
        {
            '\t',
            ';',
            ' ',
            ','
        };

        List<char> decsep = new List<char>()
        {
            '.',
            ','
        };

        public FormatData GetFullData(string[] doc)
        {
            Dictionary<string, int> oc = GetOpenCloseColumns(doc);
            int dt = -1;

            if (GetDateTimeColumn(doc) != -1)
            {
                dt = GetDateTimeColumn(doc);
                for (int i = GetSkipRowsCount(doc); i < doc.Length; i++)
                {
                    doc[i] = doc[i].Replace(' ', GetColumnSeparator(doc));
                }
            }

            FormatData data = new FormatData()
            {
                SkipRows = GetSkipRowsCount(doc),
                ColumnSeparator = GetColumnSeparator(doc),
                DecimalSeparator = GetDecimalSeparator(doc),
                DateFormat = GetDateFormat(doc),
                TimeFormat = GetTimeFormat(doc),
                TimeFrame = GetTimeFrame(doc),
                OpenColumn = oc["Open"],
                HighColumn = GetHighColumn(doc),
                LowColumn = GetLowColumn(doc),
                CloseColumn = oc["Close"],
                VolumeColumn = GetVolumeColumn(doc)
            };

            if (dt != -1)
            {
                data.DateTimeColumn = dt;
                data.DateColumn = -1;
                data.TimeColumn = -1;
            }
            else
            {
                data.DateTimeColumn = -1;
                data.DateColumn = GetDateColumn(doc);
                data.TimeColumn = GetTimeColumn(doc);
            }
            return data;
        }

        //Пробуем разделить колонки различными разделителями, если кол-во колонок больше 6, то возвращаем разделитель
        //
        public char GetColumnSeparator(string[] doc)
        {
            string row = doc[GetSkipRowsCount(doc)];
            foreach (char sep in colsep)
            {
                var arr = row.Split(sep);
                if (arr.Length >= 6)
                    return sep;
            }
            throw new Exception("Wrong column separator");
        }

        //Проверяем строки на соответствие шаблону, если не подходит - пропускаем
        //
        public int GetSkipRowsCount(string[] doc)
        {
            for (int i = 0; i < doc.Length; i++)
            {
                string pattern = @"(((\w+)|(\d{2,4}(/|\.|-)?\d{2,4}(/|\.|-)?\d{2,4})|(\d+(\.|,)?\d{0,}))(\t|,|;| )){5,}((\w+)|(\d{2,4}(/|\.|-)?\d{2,4}(/|\.|-)?\d{2,4})|(\d+(\.|,)?\d{0,}))";
                string words = @"(\w+\s?)*";
                if (!Regex.IsMatch(doc[i], pattern) && Regex.IsMatch(doc[i], words))
                    continue;
                return i;
            }
            return -1;
        }

        //Проверяем, присутствует ли колонка, разделенная пробелом
        //Если да - разделяем эту колонку и проверяем, соответствуют ли подстроки дате и времени
        //Если да - возвращаем индекс этой колонке
        //Если нет - возвращаем -1

        public int GetDateTimeColumn(string[] doc)
        {
            string[] row = doc[GetSkipRowsCount(doc)].Split(GetColumnSeparator(doc));
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

        //Проверяем колонки на соответствие шаблонам даты
        //Если такая колонка есть - возвращаем ее индекс
        //Если нет - возвращаем -1
        //
        public int GetDateColumn(string[] doc)
        {
            string[] arr = doc[GetSkipRowsCount(doc)].Split(GetColumnSeparator(doc));
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

        //Добавляем все шаблоны дат в отдельный словарь
        //Проверяем, присутсвтует ли дата в строках
        //Если нет - возвращаем сообщение о том, что дата отсутствует
        //Исключаем все неподходящие шаблоны
        //Когда остается один шаблон - возвращаем его
        //В противном случае возвращаем сообщение о том, что формат невозможно определить
        //
        public string GetDateFormat(string[] doc)
        {
            Dictionary<string, string> patterns = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> pair in Patterns.Date)
                patterns.Add(pair.Key, pair.Value);

            List<string> dates = new List<string>();

            if (GetDateColumn(doc) == -1)
                return "Date is missing";

            string prdate = string.Empty;
            int ind = GetDateColumn(doc.ToArray());

            for (int i = 0; i < doc.Length - GetSkipRowsCount(doc); i++)
            {
                string[] arr = doc[GetSkipRowsCount(doc) + i].Split(GetColumnSeparator(doc));
                string date = arr[ind];
                if (date == prdate)
                    continue;
                else prdate = date;
                int dayind = Day(doc, GetSkipRowsCount(doc), GetDateColumn(doc), GetColumnSeparator(doc));
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
            foreach (var pair in patterns)
                Console.WriteLine(pair.Key);
            return "Format can't be determined";
        }

        //Аналогично определнию номера колонки с датой
        //
        public int GetTimeColumn(string[] doc)
        {
            string[] arr = doc[GetSkipRowsCount(doc)].Split(GetColumnSeparator(doc));
            foreach (string column in arr)
            {
                foreach (KeyValuePair<string, string> pair in Patterns.Time)
                {
                    if (Regex.IsMatch(column, pair.Value))
                        return arr.ToList().IndexOf(column);
                }
            }
            return -1;
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
                    for (int j = 0; j < arr1.Length; j++)
                    {
                        if (arr1[j] != arr2[j])
                            return j;
                    }
                }
            }
            return -1;
        }

        //Аналогично определению формата даты
        //
        public string GetTimeFormat(string[] doc)
        {
            Dictionary<string, string> patterns = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> pair in Patterns.Time)
                patterns.Add(pair.Key, pair.Value);

            if (GetTimeColumn(doc) == -1)
                return "Time is missing";

            for (int i = 0; i < doc.Length - GetSkipRowsCount(doc); i++)
            {
                string[] arr = doc[GetSkipRowsCount(doc) + i].Split(GetColumnSeparator(doc));
                string time = arr[GetTimeColumn(doc)];

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

        //Разделяем строку на колонки
        //Проверяем на соответствие шаблонам чисел, пропуская колонки с датой и временем
        //Возвращаем подходящий разделитель
        //
        public char GetDecimalSeparator(string[] doc)
        {
            string row = doc[GetSkipRowsCount(doc)];
            string[] arr = row.Split(GetColumnSeparator(doc));
            foreach (string word in arr)
                foreach (KeyValuePair<string, string> pair in Patterns.Decimal)
                    if (Regex.IsMatch(word, pair.Value))
                    {
                        if (arr.ToList().IndexOf(word) == GetDateColumn(doc) || arr.ToList().IndexOf(word) == GetTimeColumn(doc))
                            continue;
                        if (pair.Key == "D.D" || pair.Key == "D")
                            return '.';
                        else if (pair.Key == "D,D")
                            return ',';
                    }
            return '0';
        }

        //Берем две подряд идущие строки
        //Проверяем разницу в датах и времени
        //В случае, если разница дат - 1 день, возвращаем D1
        //В случае, если разница времени - 1 час, возвращаем H1
        //В случае, если разница времени - 1 минута, возвращаем M1
        //В противном случае возвращаем сообщение о неподходящем таймфрейме
        //
        public string GetTimeFrame(string[] doc)
        {
            for (int i = 0; i <= doc.Length - 2; i++)
            {
                if (GetTF(doc, i) != "Wrong Timeframe")
                    return GetTF(doc, i);
            }
            return "Wrong Timeframe";
        }

        string GetTF(string[] doc, int ind)
        {
            string[] tframes = { "M1", "H1", "D1" };

            if (GetDateColumn(doc) != -1)
            {
                string[] dates =
                {
                    doc[GetSkipRowsCount(doc) + ind].Split(GetColumnSeparator(doc))[GetDateColumn(doc)],
                    doc[GetSkipRowsCount(doc) + ind + 1].Split(GetColumnSeparator(doc))[GetDateColumn(doc)]
                };
                string DateFormat = GetDateFormat(doc);
                int i = GetDateFormat(doc).IndexOf("D");
                string d1 = dates[0].Substring(i, 2);
                string d2 = dates[1].Substring(i, 2);
                int.TryParse(d1, out int i1);
                int.TryParse(d2, out int i2);
                if (Math.Abs(i1 - i2) == 1)
                    return tframes[2];
            }

            if (GetTimeColumn(doc) != -1)
            {
                string[] times =
                {
                    doc[GetSkipRowsCount(doc) + ind].Split(GetColumnSeparator(doc))[GetTimeColumn(doc)],
                    doc[GetSkipRowsCount(doc) + ind + 1].Split(GetColumnSeparator(doc))[GetTimeColumn(doc)]
                };

                int hi = GetTimeFormat(doc).IndexOf("H");
                int mi = GetTimeFormat(doc).IndexOf("M");
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

        //Разделяем строку на колонки
        //Пропускаем колонки с датой и временем, извлекаем все колонки, соответствующие шаблонам чисел
        //Если кол-во полученных чисел - 5, то возвращаем индекс последнего числа в строке
        //В противном случае - обхем отсутствует и возвращаем -1
        //
        public int GetVolumeColumn(string[] doc)
        {
            int volume = 0;
            int i = GetSkipRowsCount(doc);
            List<string> row = new List<string>();
            string[] first = doc[i].Split(GetColumnSeparator(doc));

            for (int j = 0; j < first.Length; j++)
            {
                if (j == GetDateColumn(doc) || j == GetTimeColumn(doc) || first[j] == string.Empty)
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

        //Разделяем строку на колонки, пропуская колонки с датой, временем и объемом
        //Проверяем количество максимумов в полученной подстроке
        //Если кол-во максимумов - 1, то возвращаем его индекс в строке
        //В противном случае переходим на следующую строку
        //
        public int GetHighColumn(string[] doc)
        {
            for (int i = GetSkipRowsCount(doc); i < doc.Length; i++)
            {
                List<string> row = new List<string>();
                string[] first = doc[i].Split(GetColumnSeparator(doc));

                for (int j = 0; j < first.Length; j++)
                {
                    if (j == GetDateColumn(doc) || j == GetTimeColumn(doc) || first[j] == string.Empty || j == GetVolumeColumn(doc))
                        continue;

                    foreach (KeyValuePair<string, string> pair in Patterns.Decimal)
                        if (Regex.IsMatch(first[j], pair.Value))
                            row.Add(first[j]);
                }

                for (int j = 0; j < row.Count; j++)
                {
                    if (GetDecimalSeparator(doc) == '.')
                        row[j] = row[j].Replace('.', ',');
                }

                if (MaxCount(row).Key == 1)
                {
                    if (GetDecimalSeparator(doc) == '.')
                        return first.ToList().IndexOf(MaxCount(row).Value.ToString().Replace(',', '.'));
                    else return first.ToList().IndexOf(MaxCount(row).Value.ToString());
                }
            }
            return -1;
        }

        //Аналогично поиску максимума за исключением того, что ищем количество минимумов
        //
        public int GetLowColumn(string[] doc)
        {
            for (int i = GetSkipRowsCount(doc); i < doc.Length; i++)
            {
                List<string> row = new List<string>();
                string[] first = doc[i].Split(GetColumnSeparator(doc));

                for (int j = 0; j < first.Length; j++)
                {
                    if (j == GetDateColumn(doc) || j == GetTimeColumn(doc) || first[j] == string.Empty || j == GetVolumeColumn(doc))
                        continue;

                    foreach (KeyValuePair<string, string> pair in Patterns.Decimal)
                        if (Regex.IsMatch(first[j], pair.Value))
                            row.Add(first[j]);
                }

                for (int j = 0; j < row.Count; j++)
                {
                    if (GetDecimalSeparator(doc) == '.')
                        row[j] = row[j].Replace('.', ',');
                }

                if (MinCount(row).Key == 1)
                    if (GetDecimalSeparator(doc) == '.')
                        return first.ToList().IndexOf(MinCount(row).Value.ToString().Replace(',', '.'));
                    else return first.ToList().IndexOf(MinCount(row).Value.ToString());
            }
            return -1;
        }

        //Берем две подряд идущие строки
        //Разделяем строки на колонки, пропуская колонки с датой, временем, объемом, максимумом и минимумом
        //Проверяем оставшиеся колонки на соответствие шаблонам чисел
        //Остается две колонки в каждой строке
        //Добавляем их индексы в массив voc в порядке возрастания
        //Проверяем, есть ли две такие колонки в обеих подстроках с разными индексами, чьи значения равны
        //Если да, то
        //Делаем предположение, что в первой подстроке - это закрытие, а во второй - открытие
        //Прибавляем значение соответствующей переменной, отвечающей за порядок (oc - OpenClose, co - CloseOpen)
        //Если одна из переменных больше, чем 80% длины массива doc, то возвращаем словарь с ключами "Open" и "Close" и их индексами
        //Если условие не выполнено, то предполагаем, что индекс колонки открытия меньше индекса колонки закрытия и возвращаем соответствующий словарь
        public Dictionary<string, int> GetOpenCloseColumns(string[] doc)
        {
            Dictionary<string, int> openclose = new Dictionary<string, int>();
            int date = GetDateColumn(doc);
            int time = GetTimeColumn(doc);
            int volume = GetVolumeColumn(doc);
            int max = GetHighColumn(doc);
            int min = GetLowColumn(doc);
            int oc = 0;
            int co = 0;
            int[] voc = new int[2];
            List<string> str = new List<string>();

            for (int i = GetSkipRowsCount(doc); i < doc.Length - 1; i++)
            {
                string[] row = doc[i].Split(GetColumnSeparator(doc));
                if (row.Distinct().Count() == row.Length)
                {
                    for (int j = 0; j < row.Length; j++)
                    {
                        if (j == date || j == time || row[j] == string.Empty || j == volume || j == max || j == min)
                            continue;
                        foreach (KeyValuePair<string, string> pair in Patterns.Decimal)
                            if (Regex.IsMatch(row[j], pair.Value))
                                str.Add(row[j]); ;
                    }
                    voc[0] = row.ToList().IndexOf(str[1]);
                    voc[1] = row.ToList().IndexOf(str[0]);
                    Array.Sort(voc);
#if DEBUG
                    Console.WriteLine(voc[0] + " " + voc[1]);
#endif

                    break;
                }
            }

            for (int i = GetSkipRowsCount(doc); i < doc.Length - 1; i++)
            {
                List<string> row1 = new List<string>();
                string[] first = doc[i].Split(GetColumnSeparator(doc));
                List<string> row2 = new List<string>();
                string[] second = doc[i + 1].Split(GetColumnSeparator(doc));


                for (int j = 0; j < first.Length; j++)
                {
                    if (j == date || j == time || first[j] == string.Empty || j == volume || j == max || j == min)
                        continue;

                    foreach (KeyValuePair<string, string> pair in Patterns.Decimal)
                        if (Regex.IsMatch(first[j], pair.Value))
                            row1.Add(first[j]);

                    foreach (KeyValuePair<string, string> pair in Patterns.Decimal)
                        if (Regex.IsMatch(second[j], pair.Value))
                            row2.Add(second[j]);
                }

                foreach (string sub1 in row1)
                    foreach (string sub2 in row2)
                    {
                        if (row1.IndexOf(sub1) == row2.LastIndexOf(sub2) || row1.LastIndexOf(sub1) == row2.IndexOf(sub2))
                            continue;
                        if (sub1 == sub2)
                        {
                            int open = 0;
                            int close = 0;

                            for (int j = 0; j < first.Length; j++)
                            {
                                if (j == date || j == time || first[j] == string.Empty || j == volume || j == max || j == min)
                                    continue;
                                if (first[j] == sub1)
                                    close = j;
                            }

                            for (int j = 0; j < second.Length; j++)
                            {
                                if (j == date || j == time || second[j] == string.Empty || j == volume || j == max || j == min)
                                    continue;
                                if (second[j] == sub2)
                                    open = j;
                            }

                            if (open > close)
                                co++;
                            else if (close > open)
                                oc++;
                        }
                    }

            }

            if (oc >= (int)(((double)doc.Length) * 0.8))
            {
#if DEBUG
                Console.WriteLine("OpenClose");
#endif
                openclose.Add("Open", voc[0]);
                openclose.Add("Close", voc[1]);
                return openclose;
            }

            if (co >= (int)(((double)doc.Length) * 0.8))
            {
#if DEBUG
                Console.WriteLine("CloseOpen");
#endif
                openclose.Add("Open", voc[1]);
                openclose.Add("Close", voc[0]);
                return openclose;
            }
#if DEBUG
            Console.WriteLine("OpenClose <80%");
#endif
            openclose.Add("Open", voc[0]);
            openclose.Add("Close", voc[1]);
            return openclose;
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
