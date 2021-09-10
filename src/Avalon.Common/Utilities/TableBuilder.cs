/*
 * TableBuilder
 *
 * @original code by  : Khalid Abuhakmeh
 * @based on          : ConsoleTable => https://github.com/khalidabuhakmeh/ConsoleTables
 * @license           : MIT
 *
 * This is a modified version of the ConsoleTable project by Khalid Abuhakmeh.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Avalon.Common
{
    /// <summary>
    /// https://github.com/khalidabuhakmeh/ConsoleTables
    /// </summary>
    public class TableBuilder
    {
        /// <summary>
        /// The columns in the current table.
        /// </summary>
        public IList<object> Columns { get; set; }

        /// <summary>
        /// The rows in the current table.
        /// </summary>
        public IList<object[]> Rows { get; protected set; }

        /// <summary>
        /// The options for rendering the table.
        /// </summary>
        public ConsoleTableOptions Options { get; protected set; }

        /// <summary>
        /// The types of each column in the table.
        /// </summary>
        public Type[] ColumnTypes { get; private set; }

        /// <summary>
        /// The supported numeric types used with formatting alignments.
        /// </summary>
        public static HashSet<Type> NumericTypes = new()
        {
            typeof(int),
            typeof(double),
            typeof(decimal),
            typeof(long),
            typeof(short),
            typeof(sbyte),
            typeof(byte),
            typeof(ulong),
            typeof(ushort),
            typeof(uint),
            typeof(float)
        };

        public TableBuilder(params string[] columns) : this(new ConsoleTableOptions { Columns = new List<string>(columns) })
        {
        }

        public TableBuilder(ConsoleTableOptions options)
        {
            this.Options = options ?? throw new ArgumentNullException(nameof(options));
            this.Rows = new List<object[]>();
            this.Columns = new List<object>(options.Columns);
        }

        /// <summary>
        /// Adds a single column into the <see cref="Columns"/> list.
        /// </summary>
        /// <param name="name"></param>
        public TableBuilder AddColumn(string name)
        {
            this.Columns.Add(name);
            return this;
        }

        /// <summary>
        /// Adds a list of columns names into the <see cref="Columns"/> list.
        /// </summary>
        /// <param name="names"></param>
        /// <returns></returns>
        public TableBuilder AddColumn(IEnumerable<string> names)
        {
            foreach (var name in names)
            {
                this.Columns.Add(name);
            }

            return this;
        }

        public TableBuilder AddRow(params object[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (!this.Columns.Any())
            {
                throw new Exception("Please set the columns first");
            }

            if (this.Columns.Count != values.Length)
            {
                throw new Exception($"The number columns in the row ({this.Columns.Count}) does not match the values ({values.Length})");
            }

            this.Rows.Add(values);
            return this;
        }

        public TableBuilder Configure(Action<ConsoleTableOptions> action)
        {
            action(this.Options);
            return this;
        }

        public static TableBuilder From<T>(IEnumerable<T> values)
        {
            var table = new TableBuilder
            {
                ColumnTypes = GetColumnsType<T>().ToArray()
            };

            var columns = GetColumns<T>();

            table.AddColumn(columns);

            foreach (var propertyValues in values.Select(value => columns.Select(column => GetColumnValue<T>(value, column)))
            )
            {
                table.AddRow(propertyValues.ToArray());
            }

            return table;
        }
        public string ToMarkDownString()
        {
            var sb = new StringBuilder();

            // Find the longest column by searching each row
            var columnLengths = this.ColumnLengths();

            // Create the string tableFormat with padding
            var format = this.Format(columnLengths);

            // Find the longest formatted line
            var columnHeaders = string.Format(format, this.Columns.ToArray());

            // Add each row
            var results = this.Rows.Select(row => string.Format(format, row)).ToList();

            // Create the divider
            var divider = Regex.Replace(columnHeaders, @"[^|]", "-");

            sb.AppendLine(columnHeaders);
            sb.AppendLine(divider);
            results.ForEach(row => sb.AppendLine(row));

            return sb.ToString();
        }

        /// <summary>
        /// Returns an text table based on the rows and columns from this class.
        /// </summary>
        public override string ToString()
        {
            var builder = new StringBuilder();

            // find the longest column by searching each row
            var columnLengths = this.ColumnLengths();

            // create the string tableFormat with padding
            var format = this.Format(columnLengths);

            // find the longest formatted line
            var columnHeaders = string.Format(format, this.Columns.ToArray());

            // add each row
            var results = this.Rows.Select(row => string.Format(format, row)).ToList();

            // create the divider
            var divider = Regex.Replace(columnHeaders, @"[^|]", "-");
            var dividerPlus = divider.Replace("|", "+");

            builder.AppendLine(dividerPlus);
            builder.AppendLine(columnHeaders);
            builder.AppendLine(dividerPlus);

            foreach (var row in results)
            {
                builder.AppendLine(row);

                if (this.Options.RowDivider)
                {
                    builder.AppendLine(dividerPlus);
                }
            }

            if (!this.Options.RowDivider)
            {
                builder.AppendLine(dividerPlus);
            }

            return builder.ToString();
        }

        private string Format(List<int> columnLengths, char delimiter = '|')
        {
            // Set right alignment if is a number
            var columnAlignment = Enumerable.Range(0, this.Columns.Count)
                .Select(this.GetNumberAlignment)
                .ToList();

            var delimiterStr = delimiter == char.MinValue ? string.Empty : delimiter.ToString();
            var format = (Enumerable.Range(0, this.Columns.Count)
                .Select(i => " " + delimiterStr + " {" + i + "," + columnAlignment[i] + columnLengths[i] + "}")
                .Aggregate((s, a) => s + a) + " " + delimiterStr).Trim();

            return format;
        }

        /// <summary>
        /// If a number type returns the alignment character based on the NumberAlignment option.
        /// </summary>
        /// <param name="i"></param>
        private string GetNumberAlignment(int i)
        {
            return this.Options.NumberAlignment == Alignment.Right
                   && this.ColumnTypes != null
                   && NumericTypes.Contains(this.ColumnTypes[i])
                ? ""
                : "-";
        }

        /// <summary>
        /// Calculates the maximum column length size for each column.
        /// </summary>
        private List<int> ColumnLengths()
        {
            var columnLengths = this.Columns
                                    .Select((t, i) => this.Rows.Select(x => x[i])
                                                          .Union(new[] { this.Columns[i] })
                                                          .Where(x => x != null)
                                                          .Select(x => x.ToString().Length).Max())
                                    .ToList();
            return columnLengths;
        }

        /// <summary>
        /// Writes the table to the OutputTable property which is
        /// set to be the Console by default.
        /// </summary>
        /// <param name="tableFormat"></param>
        public void Write(TableFormat tableFormat = TableFormat.Default)
        {
            switch (tableFormat)
            {
                case TableFormat.Default:
                    this.Options.OutputTo.WriteLine(this.ToString());
                    break;
                case TableFormat.MarkDown:
                    this.Options.OutputTo.WriteLine(this.ToMarkDownString());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tableFormat), tableFormat, null);
            }
        }

        /// <summary>
        /// Returns an enumerable of the name in each column.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private static IEnumerable<string> GetColumns<T>()
        {
            return typeof(T).GetProperties().Select(x => x.Name).ToArray();
        }

        /// <summary>
        /// Returns the value of a cell.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="column"></param>
        private static object GetColumnValue<T>(object target, string column)
        {
            return typeof(T).GetProperty(column).GetValue(target, null);
        }

        /// <summary>
        /// Returns an enumerable of types that each column represents.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private static IEnumerable<Type> GetColumnsType<T>()
        {
            return typeof(T).GetProperties().Select(x => x.PropertyType).ToArray();
        }
    }

    /// <summary>
    /// Options for the table processing.
    /// </summary>
    public class ConsoleTableOptions
    {
        public IEnumerable<string> Columns { get; set; } = new List<string>();

        /// <summary>
        /// Enable only from a list of objects
        /// </summary>
        public Alignment NumberAlignment { get; set; } = Alignment.Left;

        /// <summary>
        /// The <see cref="TextWriter"/> to write to. Defaults to <see cref="Console.Out"/>.
        /// </summary>
        public TextWriter OutputTo { get; set; } = Console.Out;

        /// <summary>
        /// Whether the divider line between rows should be shown or not.
        /// </summary>
        public bool RowDivider { get; set; } = false;
    }

    /// <summary>
    /// Supported table formats.
    /// </summary>
    public enum TableFormat
    {
        Default = 0,
        MarkDown = 1,
    }

    /// <summary>
    /// Supported horizontal alignments.
    /// </summary>
    public enum Alignment
    {
        Left,
        Right
    }
}