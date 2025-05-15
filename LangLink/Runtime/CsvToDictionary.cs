
using System.Collections.Generic;
using System.Text;

namespace Studio.Daily.LangLink
{
    public class CsvToDictionary : ITableTxtToDictionary
    {
        public Dictionary<string, string> ParseTableTxt(string csv)
        {
            var dict = new Dictionary<string, string>();
            int i = 0, length = csv.Length;

            var inQuotes = false;
            var field = new StringBuilder();
            var fields = new List<string>();

            while (i < length)
            {
                var c = csv[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < length && csv[i + 1] == '"')
                        {
                            field.Append('"'); // escaped quote
                            i++;
                        }
                        else
                        {
                            inQuotes = false; // quote closed
                        }
                    }
                    else
                    {
                        field.Append(c);
                    }
                }
                else
                {
                    switch (c)
                    {
                        case '"':
                            inQuotes = true;
                            break;
                        case ',':
                            fields.Add(field.ToString());
                            field.Clear();
                            break;
                        case '\r':
                        case '\n':
                        {
                            // Handle \r\n as a single newline
                            if (c == '\r' && i + 1 < length && csv[i + 1] == '\n') i++;

                            fields.Add(field.ToString());
                            field.Clear();

                            if (fields.Count >= 2)
                            {
                                var key = fields[0].Trim();
                                var value = fields[1].Trim();
                                if (!string.IsNullOrEmpty(key))
                                {
                                    dict[key] = value;
                                }
                            }

                            fields.Clear();
                            break;
                        }
                        default:
                            field.Append(c);
                            break;
                    }
                }

                i++;
            }

            // Handle final line (if file does not end with newline)
            if (field.Length > 0 || fields.Count > 0)
            {
                fields.Add(field.ToString());
                if (fields.Count >= 2)
                {
                    var key = fields[0].Trim();
                    var value = fields[1].Trim();
                    if (!string.IsNullOrEmpty(key))
                    {
                        dict[key] = value;
                    }
                }
            }

            return dict;
        }
    }
}