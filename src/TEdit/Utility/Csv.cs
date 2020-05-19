/* 
Copyright (c) 2011 BinaryConstruct
 
This source is subject to the Microsoft Public License.
See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
All other rights reserved.

THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TEdit.Utility
{
    public static class Csv
    {
        public static DataTable ToDataTable(string fileName)
        {
            var cols = new List<DataColumn>();
            return ToDataTable(fileName, cols);
        }

        public static DataTable ToDataTable(string csvFileName, List<DataColumn> columnTypes)
        {
            var dt = new DataTable(Path.GetFileNameWithoutExtension(csvFileName));
            //if (File.Exists(csvFileName))
            //{
            using (var fs = new FileStream(csvFileName, FileMode.Open, FileAccess.Read))
            {
                var csvStreamReader = new StreamReader(fs);
                string line = csvStreamReader.ReadLine();
                if (line == null)
                    throw new ApplicationException("First line of CSV cannot be null.");

                string[] columns = CsvLine(line.ToLower());
                foreach (string col in columns)
                {
                    var curcol = columnTypes.FirstOrDefault(c => string.Equals(c.ColumnName, col, StringComparison.InvariantCultureIgnoreCase))
                                        ?? new DataColumn(col.ToUpper(), typeof (string));
                    dt.Columns.Add(curcol);
                }

                var linebuilder = new StringBuilder();
                linebuilder.Append(csvStreamReader.ReadLine());
                while ((line = csvStreamReader.ReadLine()) != null)
                {
                    if (!line.StartsWith(" "))
                    {
                        // valid row, add to table
                        string[] splitline = CsvLine(linebuilder.ToString());

                        var dr = CsvLineToDataRow(splitline, dt);
                        dt.Rows.Add(dr);
                        linebuilder.Clear();
                    }

                    linebuilder.Append(line);
                }

                csvStreamReader.Close();
                fs.Close();
            }
            //}
            //else
            //{
            //    throw new FileNotFoundException("File Not Found", csvFileName);
            //}

            return dt;
        }

        public static DataRow CsvLineToDataRow(string[] splitline, DataTable dt)
        {
            DataRow dr = dt.NewRow();
            for (int i = 0; i < splitline.Length; i++)
            {
                if (i >= dr.Table.Columns.Count) break;

                string val = splitline[i].Trim();
                decimal dec_out;
                Int32 int_out;
                Int64 int64_out;
                float float_out;
                double double_out;
                bool bool_out;

                switch (dr.Table.Columns[i].DataType.ToString())
                {
                    case "System.String":
                        dr[i] = val;
                        break;
                    case "System.Double":
                        double.TryParse(val, out double_out);
                        dr[i] = double_out;
                        break;
                    case "System.Decimal":
                        decimal.TryParse(val, out dec_out);
                        dr[i] = dec_out;
                        break;
                    case "System.Float":
                        float.TryParse(val, out float_out);
                        dr[i] = float_out;
                        break;
                    case "System.Int32":
                        val = val.TrimEnd('.').Trim();
                        ;
                        Int32.TryParse(val, out int_out);
                        dr[i] = int_out;
                        break;
                    case "System.Int64":
                        Int64.TryParse(val, out int64_out);
                        dr[i] = int64_out;
                        break;
                    case "System.Boolean":
                        if (val == "1")
                            val = "true";
                        if (val == "0")
                            val = "false";
                        Boolean.TryParse(val, out bool_out);
                        dr[i] = bool_out;
                        break;
                }
            }

            return dr;
        }

        public static string[] CsvLine(string line)
        {
            return (from Match m in Regex.Matches(line,
                                                  @"(((?<x>(?=[,\r\n]+))|""(?<x>([^""]|"""")+)""|(?<x>[^,\r\n]+)),?)",
                                                  RegexOptions.ExplicitCapture)
                    select m.Groups[1].Value).ToArray();
        }

        public static void DataTableToCsv(this DataTable dt, string fileName)
        {
            string firstRow = dt.Columns.Cast<DataColumn>().Aggregate("", (current, col) => current + (col.ColumnName + ","));

            var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            var s = new StreamWriter(fs);

            s.WriteLine(firstRow);

            foreach (DataRow row in dt.Rows)
            {
                string outline = "";
                foreach (object item in row.ItemArray)
                {
                    if (item is string)
                        outline += "\"" + item + "\",";
                    else
                        outline += item + ",";
                }
                s.WriteLine(outline);
            }

            s.WriteLine("");
            s.Close();
            fs.Close();
        }
    }
}