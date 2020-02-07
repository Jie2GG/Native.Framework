/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 *
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace System.Data.SQLite
{
  using System;

#if !NET_COMPACT_20 && TRACE_WARNING
  using System.Diagnostics;
#endif

  using System.Runtime.InteropServices;
  using System.Collections.Generic;
  using System.Globalization;
  using System.Text;

  /// <summary>
  /// This base class provides datatype conversion services for the SQLite provider.
  /// </summary>
  public abstract class SQLiteConvert
  {
    /// <summary>
    /// This character is used to escape other characters, including itself, in
    /// connection string property names and values.
    /// </summary>
    internal const char EscapeChar = '\\';

    /// <summary>
    /// This character can be used to wrap connection string property names and
    /// values.  Normally, it is optional; however, when used, it must be the
    /// first -AND- last character of that connection string property name -OR-
    /// value.
    /// </summary>
    internal const char QuoteChar = '"';

    /// <summary>
    /// This character can be used to wrap connection string property names and
    /// values.  Normally, it is optional; however, when used, it must be the
    /// first -AND- last character of that connection string property name -OR-
    /// value.
    /// </summary>
    internal const char AltQuoteChar = '\'';

    /// <summary>
    /// The character is used to separate the name and value for a connection
    /// string property.  This character cannot be present in any connection
    /// string property name.  This character can be present in a connection
    /// string property value; however, this should be avoided unless deemed
    /// absolutely necessary.
    /// </summary>
    internal const char ValueChar = '=';

    /// <summary>
    /// This character is used to separate connection string properties.  When
    /// the "No_SQLiteConnectionNewParser" setting is enabled, this character
    /// may not appear in connection string property names -OR- values.
    /// </summary>
    internal const char PairChar = ';';

    /// <summary>
    /// These are the characters that are special to the connection string
    /// parser.
    /// </summary>
    internal static readonly char[] SpecialChars = {
        QuoteChar, AltQuoteChar, PairChar, ValueChar, EscapeChar
    };

    /// <summary>
    /// The fallback default database type when one cannot be obtained from an
    /// existing connection instance.
    /// </summary>
    private const DbType FallbackDefaultDbType = DbType.Object;

    /// <summary>
    /// The fallback default database type name when one cannot be obtained from
    /// an existing connection instance.
    /// </summary>
    private static readonly string FallbackDefaultTypeName = String.Empty;

    /// <summary>
    /// The value for the Unix epoch (e.g. January 1, 1970 at midnight, in UTC).
    /// </summary>
    protected static readonly DateTime UnixEpoch =
        new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    #pragma warning disable 414
    /// <summary>
    /// The value of the OLE Automation epoch represented as a Julian day.  This
    /// field cannot be removed as the test suite relies upon it.
    /// </summary>
    private static readonly double OleAutomationEpochAsJulianDay = 2415018.5;
    #pragma warning restore 414

    /// <summary>
    /// The format string for DateTime values when using the InvariantCulture or CurrentCulture formats.
    /// </summary>
    private const string FullFormat = "yyyy-MM-ddTHH:mm:ss.fffffffK";

    /// <summary>
    /// This is the minimum Julian Day value supported by this library
    /// (148731163200000).
    /// </summary>
    private static readonly long MinimumJd = computeJD(DateTime.MinValue);

    /// <summary>
    /// This is the maximum Julian Day value supported by this library
    /// (464269060799000).
    /// </summary>
    private static readonly long MaximumJd = computeJD(DateTime.MaxValue);

    /// <summary>
    /// An array of ISO-8601 DateTime formats that we support parsing.
    /// </summary>
    private static string[] _datetimeFormats = new string[] {
      "THHmmssK",
      "THHmmK",
      "HH:mm:ss.FFFFFFFK",
      "HH:mm:ssK",
      "HH:mmK",
      "yyyy-MM-dd HH:mm:ss.FFFFFFFK", /* NOTE: UTC default (5). */
      "yyyy-MM-dd HH:mm:ssK",
      "yyyy-MM-dd HH:mmK",
      "yyyy-MM-ddTHH:mm:ss.FFFFFFFK",
      "yyyy-MM-ddTHH:mmK",
      "yyyy-MM-ddTHH:mm:ssK",
      "yyyyMMddHHmmssK",
      "yyyyMMddHHmmK",
      "yyyyMMddTHHmmssFFFFFFFK",
      "THHmmss",
      "THHmm",
      "HH:mm:ss.FFFFFFF",
      "HH:mm:ss",
      "HH:mm",
      "yyyy-MM-dd HH:mm:ss.FFFFFFF", /* NOTE: Non-UTC default (19). */
      "yyyy-MM-dd HH:mm:ss",
      "yyyy-MM-dd HH:mm",
      "yyyy-MM-ddTHH:mm:ss.FFFFFFF",
      "yyyy-MM-ddTHH:mm",
      "yyyy-MM-ddTHH:mm:ss",
      "yyyyMMddHHmmss",
      "yyyyMMddHHmm",
      "yyyyMMddTHHmmssFFFFFFF",
      "yyyy-MM-dd",
      "yyyyMMdd",
      "yy-MM-dd"
    };

    /// <summary>
    /// The internal default format for UTC DateTime values when converting
    /// to a string.
    /// </summary>
    private static readonly string _datetimeFormatUtc = _datetimeFormats[5];

    /// <summary>
    /// The internal default format for local DateTime values when converting
    /// to a string.
    /// </summary>
    private static readonly string _datetimeFormatLocal = _datetimeFormats[19];

    /// <summary>
    /// An UTF-8 Encoding instance, so we can convert strings to and from UTF-8
    /// </summary>
    private static Encoding _utf8 = new UTF8Encoding();
    /// <summary>
    /// The default DateTime format for this instance.
    /// </summary>
    internal SQLiteDateFormats _datetimeFormat;
    /// <summary>
    /// The default DateTimeKind for this instance.
    /// </summary>
    internal DateTimeKind _datetimeKind;
    /// <summary>
    /// The default DateTime format string for this instance.
    /// </summary>
    internal string _datetimeFormatString = null;
    /// <summary>
    /// Initializes the conversion class
    /// </summary>
    /// <param name="fmt">The default date/time format to use for this instance</param>
    /// <param name="kind">The DateTimeKind to use.</param>
    /// <param name="fmtString">The DateTime format string to use.</param>
    internal SQLiteConvert(
        SQLiteDateFormats fmt,
        DateTimeKind kind,
        string fmtString
        )
    {
      _datetimeFormat = fmt;
      _datetimeKind = kind;
      _datetimeFormatString = fmtString;
    }

    #region UTF-8 Conversion Functions
    /// <summary>
    /// Converts a string to a UTF-8 encoded byte array sized to include a null-terminating character.
    /// </summary>
    /// <param name="sourceText">The string to convert to UTF-8</param>
    /// <returns>A byte array containing the converted string plus an extra 0 terminating byte at the end of the array.</returns>
    public static byte[] ToUTF8(string sourceText)
    {
      if (sourceText == null) return null;
      Byte[] byteArray;
      int nlen = _utf8.GetByteCount(sourceText) + 1;

      byteArray = new byte[nlen];
      nlen = _utf8.GetBytes(sourceText, 0, sourceText.Length, byteArray, 0);
      byteArray[nlen] = 0;

      return byteArray;
    }

    /// <summary>
    /// Convert a DateTime to a UTF-8 encoded, zero-terminated byte array.
    /// </summary>
    /// <remarks>
    /// This function is a convenience function, which first calls ToString() on the DateTime, and then calls ToUTF8() with the
    /// string result.
    /// </remarks>
    /// <param name="dateTimeValue">The DateTime to convert.</param>
    /// <returns>The UTF-8 encoded string, including a 0 terminating byte at the end of the array.</returns>
    public byte[] ToUTF8(DateTime dateTimeValue)
    {
      return ToUTF8(ToString(dateTimeValue));
    }

    /// <summary>
    /// Converts a UTF-8 encoded IntPtr of the specified length into a .NET string
    /// </summary>
    /// <param name="nativestring">The pointer to the memory where the UTF-8 string is encoded</param>
    /// <param name="nativestringlen">The number of bytes to decode</param>
    /// <returns>A string containing the translated character(s)</returns>
    public virtual string ToString(IntPtr nativestring, int nativestringlen)
    {
      return UTF8ToString(nativestring, nativestringlen);
    }

    /// <summary>
    /// Converts a UTF-8 encoded IntPtr of the specified length into a .NET string
    /// </summary>
    /// <param name="nativestring">The pointer to the memory where the UTF-8 string is encoded</param>
    /// <param name="nativestringlen">The number of bytes to decode</param>
    /// <returns>A string containing the translated character(s)</returns>
    public static string UTF8ToString(IntPtr nativestring, int nativestringlen)
    {
      if (nativestring == IntPtr.Zero || nativestringlen == 0) return String.Empty;
      if (nativestringlen < 0)
      {
        nativestringlen = 0;

        while (Marshal.ReadByte(nativestring, nativestringlen) != 0)
          nativestringlen++;

        if (nativestringlen == 0) return String.Empty;
      }

      byte[] byteArray = new byte[nativestringlen];

      Marshal.Copy(nativestring, byteArray, 0, nativestringlen);

      return _utf8.GetString(byteArray, 0, nativestringlen);
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region DateTime Conversion Functions
    #region New Julian Day Conversion Methods
    /// <summary>
    /// Checks if the specified <see cref="Int64" /> is within the
    /// supported range for a Julian Day value.
    /// </summary>
    /// <param name="jd">
    /// The Julian Day value to check.
    /// </param>
    /// <returns>
    /// Non-zero if the specified Julian Day value is in the supported
    /// range; otherwise, zero.
    /// </returns>
    private static bool isValidJd(
        long jd
        )
    {
        return ((jd >= MinimumJd) && (jd <= MaximumJd));
    }

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Converts a Julian Day value from a <see cref="Double" /> to an
    /// <see cref="Int64" />.
    /// </summary>
    /// <param name="julianDay">
    /// The Julian Day <see cref="Double" /> value to convert.
    /// </param>
    /// <returns>
    /// The resulting Julian Day <see cref="Int64" /> value.
    /// </returns>
    private static long DoubleToJd(
        double julianDay
        )
    {
        return (long)Math.Round(julianDay * 86400000.0);
    }

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Converts a Julian Day value from an <see cref="Int64" /> to a
    /// <see cref="Double" />.
    /// </summary>
    /// <param name="jd">
    /// The Julian Day <see cref="Int64" /> value to convert.
    /// </param>
    /// <returns>
    /// The resulting Julian Day <see cref="Double" /> value.
    /// </returns>
    private static double JdToDouble(
        long jd
        )
    {
        return (double)(jd / 86400000.0);
    }

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Converts a Julian Day value to a <see cref="DateTime" />.
    /// This method was translated from the "computeYMD" function in the
    /// "date.c" file belonging to the SQLite core library.
    /// </summary>
    /// <param name="jd">
    /// The Julian Day value to convert.
    /// </param>
    /// <param name="badValue">
    /// The <see cref="DateTime" /> value to return in the event that the
    /// Julian Day is out of the supported range.  If this value is null,
    /// an exception will be thrown instead.
    /// </param>
    /// <returns>
    /// A <see cref="DateTime" /> value that contains the year, month, and
    /// day values that are closest to the specified Julian Day value.
    /// </returns>
    private static DateTime computeYMD(
        long jd,
        DateTime? badValue
        )
    {
        if (!isValidJd(jd))
        {
            if (badValue == null)
            {
                throw new ArgumentException(
                    "Not a supported Julian Day value.");
            }

            return (DateTime)badValue;
        }

        int Z, A, B, C, D, E, X1;

        Z = (int)((jd + 43200000) / 86400000);
        A = (int)((Z - 1867216.25) / 36524.25);
        A = Z + 1 + A - (A / 4);
        B = A + 1524;
        C = (int)((B - 122.1) / 365.25);
        D = (36525 * C) / 100;
        E = (int)((B - D) / 30.6001);
        X1 = (int)(30.6001 * E);

        int day, month, year;

        day = B - D - X1;
        month = E < 14 ? E - 1 : E - 13;
        year = month > 2 ? C - 4716 : C - 4715;

        try
        {
            return new DateTime(year, month, day);
        }
        catch
        {
            if (badValue == null)
                throw;

            return (DateTime)badValue;
        }
    }

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Converts a Julian Day value to a <see cref="DateTime" />.
    /// This method was translated from the "computeHMS" function in the
    /// "date.c" file belonging to the SQLite core library.
    /// </summary>
    /// <param name="jd">
    /// The Julian Day value to convert.
    /// </param>
    /// <param name="badValue">
    /// The <see cref="DateTime" /> value to return in the event that the
    /// Julian Day value is out of the supported range.  If this value is
    /// null, an exception will be thrown instead.
    /// </param>
    /// <returns>
    /// A <see cref="DateTime" /> value that contains the hour, minute, and
    /// second, and millisecond values that are closest to the specified
    /// Julian Day value.
    /// </returns>
    private static DateTime computeHMS(
        long jd,
        DateTime? badValue
        )
    {
        if (!isValidJd(jd))
        {
            if (badValue == null)
            {
                throw new ArgumentException(
                    "Not a supported Julian Day value.");
            }

            return (DateTime)badValue;
        }

        int si;

        si = (int)((jd + 43200000) % 86400000);

        decimal sd;

        sd = si / 1000.0M;
        si = (int)sd;

        int millisecond = (int)((sd - si) * 1000.0M);

        sd -= si;

        int hour;

        hour = si / 3600;
        si -= hour * 3600;

        int minute;

        minute = si / 60;
        sd += si - minute * 60;

        int second = (int)sd;

        try
        {
            DateTime minValue = DateTime.MinValue;

            return new DateTime(
                minValue.Year, minValue.Month, minValue.Day,
                hour, minute, second, millisecond);
        }
        catch
        {
            if (badValue == null)
                throw;

            return (DateTime)badValue;
        }
    }

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Converts a <see cref="DateTime" /> to a Julian Day value.
    /// This method was translated from the "computeJD" function in
    /// the "date.c" file belonging to the SQLite core library.
    /// Since the range of Julian Day values supported by this method
    /// includes all possible (valid) values of a <see cref="DateTime" />
    /// value, it should be extremely difficult for this method to
    /// raise an exception or return an undefined result.
    /// </summary>
    /// <param name="dateTime">
    /// The <see cref="DateTime" /> value to convert.  This value
    /// will be within the range of <see cref="DateTime.MinValue" />
    /// (00:00:00.0000000, January 1, 0001) to
    /// <see cref="DateTime.MaxValue" /> (23:59:59.9999999, December
    /// 31, 9999).
    /// </param>
    /// <returns>
    /// The nearest Julian Day value corresponding to the specified
    /// <see cref="DateTime" /> value.
    /// </returns>
    private static long computeJD(
        DateTime dateTime
        )
    {
        int Y, M, D;

        Y = dateTime.Year;
        M = dateTime.Month;
        D = dateTime.Day;

        if (M <= 2)
        {
            Y--;
            M += 12;
        }

        int A, B, X1, X2;

        A = Y / 100;
        B = 2 - A + (A / 4);
        X1 = 36525 * (Y + 4716) / 100;
        X2 = 306001 * (M + 1) / 10000;

        long jd;

        jd = (long)((X1 + X2 + D + B - 1524.5) * 86400000);

        jd += (dateTime.Hour * 3600000) + (dateTime.Minute * 60000) +
            (dateTime.Second * 1000) + dateTime.Millisecond;

        return jd;
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Converts a string into a DateTime, using the DateTimeFormat, DateTimeKind,
    /// and DateTimeFormatString specified for the connection when it was opened.
    /// </summary>
    /// <remarks>
    /// Acceptable ISO8601 DateTime formats are:
    /// <list type="bullet">
    /// <item><description>THHmmssK</description></item>
    /// <item><description>THHmmK</description></item>
    /// <item><description>HH:mm:ss.FFFFFFFK</description></item>
    /// <item><description>HH:mm:ssK</description></item>
    /// <item><description>HH:mmK</description></item>
    /// <item><description>yyyy-MM-dd HH:mm:ss.FFFFFFFK</description></item>
    /// <item><description>yyyy-MM-dd HH:mm:ssK</description></item>
    /// <item><description>yyyy-MM-dd HH:mmK</description></item>
    /// <item><description>yyyy-MM-ddTHH:mm:ss.FFFFFFFK</description></item>
    /// <item><description>yyyy-MM-ddTHH:mmK</description></item>
    /// <item><description>yyyy-MM-ddTHH:mm:ssK</description></item>
    /// <item><description>yyyyMMddHHmmssK</description></item>
    /// <item><description>yyyyMMddHHmmK</description></item>
    /// <item><description>yyyyMMddTHHmmssFFFFFFFK</description></item>
    /// <item><description>THHmmss</description></item>
    /// <item><description>THHmm</description></item>
    /// <item><description>HH:mm:ss.FFFFFFF</description></item>
    /// <item><description>HH:mm:ss</description></item>
    /// <item><description>HH:mm</description></item>
    /// <item><description>yyyy-MM-dd HH:mm:ss.FFFFFFF</description></item>
    /// <item><description>yyyy-MM-dd HH:mm:ss</description></item>
    /// <item><description>yyyy-MM-dd HH:mm</description></item>
    /// <item><description>yyyy-MM-ddTHH:mm:ss.FFFFFFF</description></item>
    /// <item><description>yyyy-MM-ddTHH:mm</description></item>
    /// <item><description>yyyy-MM-ddTHH:mm:ss</description></item>
    /// <item><description>yyyyMMddHHmmss</description></item>
    /// <item><description>yyyyMMddHHmm</description></item>
    /// <item><description>yyyyMMddTHHmmssFFFFFFF</description></item>
    /// <item><description>yyyy-MM-dd</description></item>
    /// <item><description>yyyyMMdd</description></item>
    /// <item><description>yy-MM-dd</description></item>
    /// </list>
    /// If the string cannot be matched to one of the above formats -OR-
    /// the DateTimeFormatString if one was provided, an exception will
    /// be thrown.
    /// </remarks>
    /// <param name="dateText">The string containing either a long integer number of 100-nanosecond units since
    /// System.DateTime.MinValue, a Julian day double, an integer number of seconds since the Unix epoch, a
    /// culture-independent formatted date and time string, a formatted date and time string in the current
    /// culture, or an ISO8601-format string.</param>
    /// <returns>A DateTime value</returns>
    public DateTime ToDateTime(string dateText)
    {
      return ToDateTime(dateText, _datetimeFormat, _datetimeKind, _datetimeFormatString);
    }

    /// <summary>
    /// Converts a string into a DateTime, using the specified DateTimeFormat,
    /// DateTimeKind and DateTimeFormatString.
    /// </summary>
    /// <remarks>
    /// Acceptable ISO8601 DateTime formats are:
    /// <list type="bullet">
    /// <item><description>THHmmssK</description></item>
    /// <item><description>THHmmK</description></item>
    /// <item><description>HH:mm:ss.FFFFFFFK</description></item>
    /// <item><description>HH:mm:ssK</description></item>
    /// <item><description>HH:mmK</description></item>
    /// <item><description>yyyy-MM-dd HH:mm:ss.FFFFFFFK</description></item>
    /// <item><description>yyyy-MM-dd HH:mm:ssK</description></item>
    /// <item><description>yyyy-MM-dd HH:mmK</description></item>
    /// <item><description>yyyy-MM-ddTHH:mm:ss.FFFFFFFK</description></item>
    /// <item><description>yyyy-MM-ddTHH:mmK</description></item>
    /// <item><description>yyyy-MM-ddTHH:mm:ssK</description></item>
    /// <item><description>yyyyMMddHHmmssK</description></item>
    /// <item><description>yyyyMMddHHmmK</description></item>
    /// <item><description>yyyyMMddTHHmmssFFFFFFFK</description></item>
    /// <item><description>THHmmss</description></item>
    /// <item><description>THHmm</description></item>
    /// <item><description>HH:mm:ss.FFFFFFF</description></item>
    /// <item><description>HH:mm:ss</description></item>
    /// <item><description>HH:mm</description></item>
    /// <item><description>yyyy-MM-dd HH:mm:ss.FFFFFFF</description></item>
    /// <item><description>yyyy-MM-dd HH:mm:ss</description></item>
    /// <item><description>yyyy-MM-dd HH:mm</description></item>
    /// <item><description>yyyy-MM-ddTHH:mm:ss.FFFFFFF</description></item>
    /// <item><description>yyyy-MM-ddTHH:mm</description></item>
    /// <item><description>yyyy-MM-ddTHH:mm:ss</description></item>
    /// <item><description>yyyyMMddHHmmss</description></item>
    /// <item><description>yyyyMMddHHmm</description></item>
    /// <item><description>yyyyMMddTHHmmssFFFFFFF</description></item>
    /// <item><description>yyyy-MM-dd</description></item>
    /// <item><description>yyyyMMdd</description></item>
    /// <item><description>yy-MM-dd</description></item>
    /// </list>
    /// If the string cannot be matched to one of the above formats -OR-
    /// the DateTimeFormatString if one was provided, an exception will
    /// be thrown.
    /// </remarks>
    /// <param name="dateText">The string containing either a long integer number of 100-nanosecond units since
    /// System.DateTime.MinValue, a Julian day double, an integer number of seconds since the Unix epoch, a
    /// culture-independent formatted date and time string, a formatted date and time string in the current
    /// culture, or an ISO8601-format string.</param>
    /// <param name="format">The SQLiteDateFormats to use.</param>
    /// <param name="kind">The DateTimeKind to use.</param>
    /// <param name="formatString">The DateTime format string to use.</param>
    /// <returns>A DateTime value</returns>
    public static DateTime ToDateTime(
        string dateText,
        SQLiteDateFormats format,
        DateTimeKind kind,
        string formatString
        )
    {
        switch (format)
        {
            case SQLiteDateFormats.Ticks:
                {
                    return TicksToDateTime(Convert.ToInt64(
                        dateText, CultureInfo.InvariantCulture), kind);
                }
            case SQLiteDateFormats.JulianDay:
                {
                    return ToDateTime(Convert.ToDouble(
                        dateText, CultureInfo.InvariantCulture), kind);
                }
            case SQLiteDateFormats.UnixEpoch:
                {
                    return UnixEpochToDateTime(Convert.ToInt64(
                        dateText, CultureInfo.InvariantCulture), kind);
                }
            case SQLiteDateFormats.InvariantCulture:
                {
                    if (formatString != null)
                        return DateTime.SpecifyKind(DateTime.ParseExact(
                            dateText, formatString,
                            DateTimeFormatInfo.InvariantInfo,
                            kind == DateTimeKind.Utc ?
                                DateTimeStyles.AdjustToUniversal :
                                DateTimeStyles.None),
                            kind);
                    else
                        return DateTime.SpecifyKind(DateTime.Parse(
                            dateText, DateTimeFormatInfo.InvariantInfo,
                            kind == DateTimeKind.Utc ?
                                DateTimeStyles.AdjustToUniversal :
                                DateTimeStyles.None),
                            kind);
                }
            case SQLiteDateFormats.CurrentCulture:
                {
                    if (formatString != null)
                        return DateTime.SpecifyKind(DateTime.ParseExact(
                            dateText, formatString,
                            DateTimeFormatInfo.CurrentInfo,
                            kind == DateTimeKind.Utc ?
                                DateTimeStyles.AdjustToUniversal :
                                DateTimeStyles.None),
                            kind);
                    else
                        return DateTime.SpecifyKind(DateTime.Parse(
                            dateText, DateTimeFormatInfo.CurrentInfo,
                            kind == DateTimeKind.Utc ?
                                DateTimeStyles.AdjustToUniversal :
                                DateTimeStyles.None),
                            kind);
                }
            default: /* ISO-8601 */
                {
                    if (formatString != null)
                        return DateTime.SpecifyKind(DateTime.ParseExact(
                            dateText, formatString,
                            DateTimeFormatInfo.InvariantInfo,
                            kind == DateTimeKind.Utc ?
                                DateTimeStyles.AdjustToUniversal :
                                DateTimeStyles.None),
                            kind);
                    else
                        return DateTime.SpecifyKind(DateTime.ParseExact(
                            dateText, _datetimeFormats,
                            DateTimeFormatInfo.InvariantInfo,
                            kind == DateTimeKind.Utc ?
                                DateTimeStyles.AdjustToUniversal :
                                DateTimeStyles.None),
                            kind);
                }
        }
    }

    /// <summary>
    /// Converts a julianday value into a DateTime
    /// </summary>
    /// <param name="julianDay">The value to convert</param>
    /// <returns>A .NET DateTime</returns>
    public DateTime ToDateTime(double julianDay)
    {
      return ToDateTime(julianDay, _datetimeKind);
    }

    /// <summary>
    /// Converts a julianday value into a DateTime
    /// </summary>
    /// <param name="julianDay">The value to convert</param>
    /// <param name="kind">The DateTimeKind to use.</param>
    /// <returns>A .NET DateTime</returns>
    public static DateTime ToDateTime(
        double julianDay,
        DateTimeKind kind
        )
    {
        long jd = DoubleToJd(julianDay);
        DateTime dateTimeYMD = computeYMD(jd, null);
        DateTime dateTimeHMS = computeHMS(jd, null);

        return new DateTime(
            dateTimeYMD.Year, dateTimeYMD.Month, dateTimeYMD.Day,
            dateTimeHMS.Hour, dateTimeHMS.Minute, dateTimeHMS.Second,
            dateTimeHMS.Millisecond, kind);
    }

    /// <summary>
    /// Converts the specified number of seconds from the Unix epoch into a
    /// <see cref="DateTime" /> value.
    /// </summary>
    /// <param name="seconds">
    /// The number of whole seconds since the Unix epoch.
    /// </param>
    /// <param name="kind">
    /// Either Utc or Local time.
    /// </param>
    /// <returns>
    /// The new <see cref="DateTime" /> value.
    /// </returns>
    internal static DateTime UnixEpochToDateTime(long seconds, DateTimeKind kind)
    {
        return DateTime.SpecifyKind(UnixEpoch.AddSeconds(seconds), kind);
    }

    /// <summary>
    /// Converts the specified number of ticks since the epoch into a
    /// <see cref="DateTime" /> value.
    /// </summary>
    /// <param name="ticks">
    /// The number of whole ticks since the epoch.
    /// </param>
    /// <param name="kind">
    /// Either Utc or Local time.
    /// </param>
    /// <returns>
    /// The new <see cref="DateTime" /> value.
    /// </returns>
    internal static DateTime TicksToDateTime(long ticks, DateTimeKind kind)
    {
        return new DateTime(ticks, kind);
    }

    /// <summary>
    /// Converts a DateTime struct to a JulianDay double
    /// </summary>
    /// <param name="value">The DateTime to convert</param>
    /// <returns>The JulianDay value the Datetime represents</returns>
    public static double ToJulianDay(DateTime value)
    {
        return JdToDouble(computeJD(value));
    }

    /// <summary>
    /// Converts a DateTime struct to the whole number of seconds since the
    /// Unix epoch.
    /// </summary>
    /// <param name="value">The DateTime to convert</param>
    /// <returns>The whole number of seconds since the Unix epoch</returns>
    public static long ToUnixEpoch(DateTime value)
    {
        return (value.Subtract(UnixEpoch).Ticks / TimeSpan.TicksPerSecond);
    }

    /// <summary>
    /// Returns the DateTime format string to use for the specified DateTimeKind.
    /// If <paramref name="formatString" /> is not null, it will be returned verbatim.
    /// </summary>
    /// <param name="kind">The DateTimeKind to use.</param>
    /// <param name="formatString">The DateTime format string to use.</param>
    /// <returns>
    /// The DateTime format string to use for the specified DateTimeKind.
    /// </returns>
    private static string GetDateTimeKindFormat(
        DateTimeKind kind,
        string formatString
        )
    {
        if (formatString != null) return formatString;
        return (kind == DateTimeKind.Utc) ? _datetimeFormatUtc : _datetimeFormatLocal;
    }

    /// <summary>
    /// Converts a string into a DateTime, using the DateTimeFormat, DateTimeKind,
    /// and DateTimeFormatString specified for the connection when it was opened.
    /// </summary>
    /// <param name="dateValue">The DateTime value to convert</param>
    /// <returns>Either a string containing the long integer number of 100-nanosecond units since System.DateTime.MinValue, a
    /// Julian day double, an integer number of seconds since the Unix epoch, a culture-independent formatted date and time
    /// string, a formatted date and time string in the current culture, or an ISO8601-format date/time string.</returns>
    public string ToString(DateTime dateValue)
    {
        return ToString(dateValue, _datetimeFormat, _datetimeKind, _datetimeFormatString);
    }

    /// <summary>
    /// Converts a string into a DateTime, using the DateTimeFormat, DateTimeKind,
    /// and DateTimeFormatString specified for the connection when it was opened.
    /// </summary>
    /// <param name="dateValue">The DateTime value to convert</param>
    /// <param name="format">The SQLiteDateFormats to use.</param>
    /// <param name="kind">The DateTimeKind to use.</param>
    /// <param name="formatString">The DateTime format string to use.</param>
    /// <returns>Either a string containing the long integer number of 100-nanosecond units since System.DateTime.MinValue, a
    /// Julian day double, an integer number of seconds since the Unix epoch, a culture-independent formatted date and time
    /// string, a formatted date and time string in the current culture, or an ISO8601-format date/time string.</returns>
    public static string ToString(
        DateTime dateValue,
        SQLiteDateFormats format,
        DateTimeKind kind,
        string formatString
        )
    {
        switch (format)
        {
            case SQLiteDateFormats.Ticks:
                return dateValue.Ticks.ToString(CultureInfo.InvariantCulture);
            case SQLiteDateFormats.JulianDay:
                return ToJulianDay(dateValue).ToString(CultureInfo.InvariantCulture);
            case SQLiteDateFormats.UnixEpoch:
                return ((long)(dateValue.Subtract(UnixEpoch).Ticks / TimeSpan.TicksPerSecond)).ToString();
            case SQLiteDateFormats.InvariantCulture:
                return dateValue.ToString((formatString != null) ?
                    formatString : FullFormat, CultureInfo.InvariantCulture);
            case SQLiteDateFormats.CurrentCulture:
                return dateValue.ToString((formatString != null) ?
                    formatString : FullFormat, CultureInfo.CurrentCulture);
            default:
                return (dateValue.Kind == DateTimeKind.Unspecified) ?
                    DateTime.SpecifyKind(dateValue, kind).ToString(
                        GetDateTimeKindFormat(kind, formatString),
                            CultureInfo.InvariantCulture) : dateValue.ToString(
                        GetDateTimeKindFormat(dateValue.Kind, formatString),
                            CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// Internal function to convert a UTF-8 encoded IntPtr of the specified length to a DateTime.
    /// </summary>
    /// <remarks>
    /// This is a convenience function, which first calls ToString() on the IntPtr to convert it to a string, then calls
    /// ToDateTime() on the string to return a DateTime.
    /// </remarks>
    /// <param name="ptr">A pointer to the UTF-8 encoded string</param>
    /// <param name="len">The length in bytes of the string</param>
    /// <returns>The parsed DateTime value</returns>
    internal DateTime ToDateTime(IntPtr ptr, int len)
    {
      return ToDateTime(ToString(ptr, len));
    }
    #endregion

    /// <summary>
    /// Smart method of splitting a string.  Skips quoted elements, removes the quotes.
    /// </summary>
    /// <remarks>
    /// This split function works somewhat like the String.Split() function in that it breaks apart a string into
    /// pieces and returns the pieces as an array.  The primary differences are:
    /// <list type="bullet">
    /// <item><description>Only one character can be provided as a separator character</description></item>
    /// <item><description>Quoted text inside the string is skipped over when searching for the separator, and the quotes are removed.</description></item>
    /// </list>
    /// Thus, if splitting the following string looking for a comma:<br/>
    /// One,Two, "Three, Four", Five<br/>
    /// <br/>
    /// The resulting array would contain<br/>
    /// [0] One<br/>
    /// [1] Two<br/>
    /// [2] Three, Four<br/>
    /// [3] Five<br/>
    /// <br/>
    /// Note that the leading and trailing spaces were removed from each item during the split.
    /// </remarks>
    /// <param name="source">Source string to split apart</param>
    /// <param name="separator">Separator character</param>
    /// <returns>A string array of the split up elements</returns>
    public static string[] Split(string source, char separator)
    {
      char[] toks = new char[2] { QuoteChar, separator };
      char[] quot = new char[1] { QuoteChar };
      int n = 0;
      List<string> ls = new List<string>();
      string s;

      while (source.Length > 0)
      {
        n = source.IndexOfAny(toks, n);
        if (n == -1) break;
        if (source[n] == toks[0])
        {
          //source = source.Remove(n, 1);
          n = source.IndexOfAny(quot, n + 1);
          if (n == -1)
          {
            //source = "\"" + source;
            break;
          }
          n++;
          //source = source.Remove(n, 1);
        }
        else
        {
          s = source.Substring(0, n).Trim();
          if (s.Length > 1 && s[0] == quot[0] && s[s.Length - 1] == s[0])
            s = s.Substring(1, s.Length - 2);

          source = source.Substring(n + 1).Trim();
          if (s.Length > 0) ls.Add(s);
          n = 0;
        }
      }
      if (source.Length > 0)
      {
        s = source.Trim();
        if (s.Length > 1 && s[0] == quot[0] && s[s.Length - 1] == s[0])
          s = s.Substring(1, s.Length - 2);
        ls.Add(s);
      }

      string[] ar = new string[ls.Count];
      ls.CopyTo(ar, 0);

      return ar;
    }

    /// <summary>
    /// Splits the specified string into multiple strings based on a separator
    /// and returns the result as an array of strings.
    /// </summary>
    /// <param name="value">
    /// The string to split into pieces based on the separator character.  If
    /// this string is null, null will always be returned.  If this string is
    /// empty, an array of zero strings will always be returned.
    /// </param>
    /// <param name="separator">
    /// The character used to divide the original string into sub-strings.
    /// This character cannot be a backslash or a double-quote; otherwise, no
    /// work will be performed and null will be returned.
    /// </param>
    /// <param name="keepQuote">
    /// If this parameter is non-zero, all double-quote characters will be
    /// retained in the returned list of strings; otherwise, they will be
    /// dropped.
    /// </param>
    /// <param name="error">
    /// Upon failure, this parameter will be modified to contain an appropriate
    /// error message.
    /// </param>
    /// <returns>
    /// The new array of strings or null if the input string is null -OR- the
    /// separator character is a backslash or a double-quote -OR- the string
    /// contains an unbalanced backslash or double-quote character.
    /// </returns>
    internal static string[] NewSplit(
        string value,
        char separator,
        bool keepQuote,
        ref string error
        )
    {
        //
        // NOTE: It is illegal for the separator character to be either a
        //       backslash or a double-quote because both of those characters
        //       are used for escaping other characters (e.g. the separator
        //       character).
        //
        if ((separator == EscapeChar) || (separator == QuoteChar))
        {
            error = "separator character cannot be the escape or quote characters";
            return null;
        }

        if (value == null)
        {
            error = "string value to split cannot be null";
            return null;
        }

        int length = value.Length;

        if (length == 0)
            return new string[0];

        List<string> list = new List<string>();
        StringBuilder element = new StringBuilder();
        int index = 0;
        bool escape = false;
        bool quote = false;

        while (index < length)
        {
            char character = value[index++];

            if (escape)
            {
                //
                // HACK: Only consider the escape character to be an actual
                //       "escape" if it is followed by a reserved character;
                //       otherwise, emit the original escape character and
                //       the current character in an effort to help preserve
                //       the original string content.
                //
                if ((character != EscapeChar) &&
                    (character != QuoteChar) &&
                    (character != separator))
                {
                    element.Append(EscapeChar);
                }

                element.Append(character);
                escape = false;
            }
            else if (character == EscapeChar)
            {
                escape = true;
            }
            else if (character == QuoteChar)
            {
                if (keepQuote)
                    element.Append(character);

                quote = !quote;
            }
            else if (character == separator)
            {
                if (quote)
                {
                    element.Append(character);
                }
                else
                {
                    list.Add(element.ToString());
                    element.Length = 0;
                }
            }
            else
            {
                element.Append(character);
            }
        }

        //
        // NOTE: An unbalanced escape or quote character in the string is
        //       considered to be a fatal error; therefore, return null.
        //
        if (escape || quote)
        {
            error = "unbalanced escape or quote character found";
            return null;
        }

        if (element.Length > 0)
            list.Add(element.ToString());

        return list.ToArray();
    }

    /// <summary>
    /// Queries and returns the string representation for an object, using the
    /// specified (or current) format provider.
    /// </summary>
    /// <param name="obj">
    /// The object instance to return the string representation for.
    /// </param>
    /// <param name="provider">
    /// The format provider to use -OR- null if the current format provider for
    /// the thread should be used instead.
    /// </param>
    /// <returns>
    /// The string representation for the object instance -OR- null if the
    /// object instance is also null.
    /// </returns>
    public static string ToStringWithProvider(
        object obj,
        IFormatProvider provider
        )
    {
        if (obj == null)
            return null; /* null --> null */

        if (obj is string)
            return (string)obj; /* identity */

        IConvertible convertible = obj as IConvertible;

        if (convertible != null)
            return convertible.ToString(provider);

        return obj.ToString(); /* not IConvertible */
    }

    /// <summary>
    /// Attempts to convert an arbitrary object to the Boolean data type.
    /// Null object values are converted to false.  Throws an exception
    /// upon failure.
    /// </summary>
    /// <param name="obj">
    /// The object value to convert.
    /// </param>
    /// <param name="provider">
    /// The format provider to use.
    /// </param>
    /// <param name="viaFramework">
    /// If non-zero, a string value will be converted using the
    /// <see cref="Convert.ToBoolean(Object, IFormatProvider)" />
    /// method; otherwise, the <see cref="ToBoolean(String)" />
    /// method will be used.
    /// </param>
    /// <returns>
    /// The converted boolean value.
    /// </returns>
    internal static bool ToBoolean(
        object obj,
        IFormatProvider provider,
        bool viaFramework
        )
    {
        if (obj == null)
            return false;

        TypeCode typeCode = Type.GetTypeCode(obj.GetType());

        switch (typeCode)
        {
            case TypeCode.Empty:
            case TypeCode.DBNull:
                return false;
            case TypeCode.Boolean:
                return (bool)obj;
            case TypeCode.Char:
                return ((char)obj) != (char)0 ? true : false;
            case TypeCode.SByte:
                return ((sbyte)obj) != (sbyte)0 ? true : false;
            case TypeCode.Byte:
                return ((byte)obj) != (byte)0 ? true : false;
            case TypeCode.Int16:
                return ((short)obj) != (short)0 ? true : false;
            case TypeCode.UInt16:
                return ((ushort)obj) != (ushort)0 ? true : false;
            case TypeCode.Int32:
                return ((int)obj) != (int)0 ? true : false;
            case TypeCode.UInt32:
                return ((uint)obj) != (uint)0 ? true : false;
            case TypeCode.Int64:
                return ((long)obj) != (long)0 ? true : false;
            case TypeCode.UInt64:
                return ((ulong)obj) != (ulong)0 ? true : false;
            case TypeCode.Single:
                return ((float)obj) != (float)0.0 ? true : false;
            case TypeCode.Double:
                return ((double)obj) != (double)0.0 ? true : false;
            case TypeCode.Decimal:
                return ((decimal)obj) != Decimal.Zero ? true : false;
            case TypeCode.String:
                return viaFramework ?
                    Convert.ToBoolean(obj, provider) :
                    ToBoolean(ToStringWithProvider(obj, provider));
            default:
                throw new SQLiteException(HelperMethods.StringFormat(
                    CultureInfo.CurrentCulture,
                    "Cannot convert type {0} to boolean",
                    typeCode));
        }
    }

    /// <summary>
    /// Convert a value to true or false.
    /// </summary>
    /// <param name="source">A string or number representing true or false</param>
    /// <returns></returns>
    public static bool ToBoolean(object source)
    {
      if (source is bool) return (bool)source;

      return ToBoolean(ToStringWithProvider(
          source, CultureInfo.InvariantCulture));
    }

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Converts an integer to a string that can be round-tripped using the
    /// invariant culture.
    /// </summary>
    /// <param name="value">
    /// The integer value to return the string representation for.
    /// </param>
    /// <returns>
    /// The string representation of the specified integer value, using the
    /// invariant culture.
    /// </returns>
    internal static string ToString(int value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Attempts to convert a <see cref="String" /> into a <see cref="Boolean" />.
    /// </summary>
    /// <param name="source">
    /// The <see cref="String" /> to convert, cannot be null.
    /// </param>
    /// <returns>
    /// The converted <see cref="Boolean" /> value.
    /// </returns>
    /// <remarks>
    /// The supported strings are "yes", "no", "y", "n", "on", "off", "0", "1",
    /// as well as any prefix of the strings <see cref="Boolean.FalseString" />
    /// and <see cref="Boolean.TrueString" />.  All strings are treated in a
    /// case-insensitive manner.
    /// </remarks>
    public static bool ToBoolean(string source)
    {
        if (source == null) throw new ArgumentNullException("source");
        if (String.Compare(source, 0, bool.TrueString, 0, source.Length, StringComparison.OrdinalIgnoreCase) == 0) return true;
        else if (String.Compare(source, 0, bool.FalseString, 0, source.Length, StringComparison.OrdinalIgnoreCase) == 0) return false;

        switch (source.ToLower(CultureInfo.InvariantCulture))
        {
            case "y":
            case "yes":
            case "on":
            case "1":
                return true;
            case "n":
            case "no":
            case "off":
            case "0":
                return false;
        }

        throw new ArgumentException("source");
    }

    #region Type Conversions
    /// <summary>
    /// Converts a SQLiteType to a .NET Type object
    /// </summary>
    /// <param name="t">The SQLiteType to convert</param>
    /// <returns>Returns a .NET Type object</returns>
    internal static Type SQLiteTypeToType(SQLiteType t)
    {
      if (t.Type == DbType.Object)
        return _affinitytotype[(int)t.Affinity];
      else
        return SQLiteConvert.DbTypeToType(t.Type);
    }

    private static Type[] _affinitytotype = {
      typeof(object),   // Uninitialized (0)
      typeof(Int64),    // Int64 (1)
      typeof(Double),   // Double (2)
      typeof(string),   // Text (3)
      typeof(byte[]),   // Blob (4)
      typeof(object),   // Null (5)
      null,             // Undefined (6)
      null,             // Undefined (7)
      null,             // Undefined (8)
      null,             // Undefined (9)
      typeof(DateTime), // DateTime (10)
      typeof(object)    // None (11)
    };

    /// <summary>
    /// For a given intrinsic type, return a DbType
    /// </summary>
    /// <param name="typ">The native type to convert</param>
    /// <returns>The corresponding (closest match) DbType</returns>
    internal static DbType TypeToDbType(Type typ)
    {
      TypeCode tc = Type.GetTypeCode(typ);
      if (tc == TypeCode.Object)
      {
        if (typ == typeof(byte[])) return DbType.Binary;
        if (typ == typeof(Guid)) return DbType.Guid;
        return DbType.String;
      }
      return _typetodbtype[(int)tc];
    }

    private static DbType[] _typetodbtype = {
      DbType.Object,   // Empty (0)
      DbType.Binary,   // Object (1)
      DbType.Object,   // DBNull (2)
      DbType.Boolean,  // Boolean (3)
      DbType.SByte,    // Char (4)
      DbType.SByte,    // SByte (5)
      DbType.Byte,     // Byte (6)
      DbType.Int16,    // Int16 (7)
      DbType.UInt16,   // UInt16 (8)
      DbType.Int32,    // Int32 (9)
      DbType.UInt32,   // UInt32 (10)
      DbType.Int64,    // Int64 (11)
      DbType.UInt64,   // UInt64 (12)
      DbType.Single,   // Single (13)
      DbType.Double,   // Double (14)
      DbType.Decimal,  // Decimal (15)
      DbType.DateTime, // DateTime (16)
      DbType.Object,   // ?? (17)
      DbType.String    // String (18)
    };

    /// <summary>
    /// Returns the ColumnSize for the given DbType
    /// </summary>
    /// <param name="typ">The DbType to get the size of</param>
    /// <returns></returns>
    internal static int DbTypeToColumnSize(DbType typ)
    {
      return _dbtypetocolumnsize[(int)typ];
    }

    private static int[] _dbtypetocolumnsize = {
      int.MaxValue, // AnsiString (0)
      int.MaxValue, // Binary (1)
      1,            // Byte (2)
      1,            // Boolean (3)
      8,            // Currency (4)
      8,            // Date (5)
      8,            // DateTime (6)
      8,            // Decimal (7)
      8,            // Double (8)
      16,           // Guid (9)
      2,            // Int16 (10)
      4,            // Int32 (11)
      8,            // Int64 (12)
      int.MaxValue, // Object (13)
      1,            // SByte (14)
      4,            // Single (15)
      int.MaxValue, // String (16)
      8,            // Time (17)
      2,            // UInt16 (18)
      4,            // UInt32 (19)
      8,            // UInt64 (20)
      8,            // VarNumeric (21)
      int.MaxValue, // AnsiStringFixedLength (22)
      int.MaxValue, // StringFixedLength (23)
      int.MaxValue, // ?? (24)
      int.MaxValue, // Xml (25)
      8,            // DateTime2 (26)
      10            // DateTimeOffset (27)
    };

    internal static object DbTypeToNumericPrecision(DbType typ)
    {
      return _dbtypetonumericprecision[(int)typ];
    }

    private static object[] _dbtypetonumericprecision = {
      DBNull.Value, // AnsiString (0)
      DBNull.Value, // Binary (1)
      3,            // Byte (2)
      DBNull.Value, // Boolean (3)
      19,           // Currency (4)
      DBNull.Value, // Date (5)
      DBNull.Value, // DateTime (6)
      53,           // Decimal (7)
      53,           // Double (8)
      DBNull.Value, // Guid (9)
      5,            // Int16 (10)
      10,           // Int32 (11)
      19,           // Int64 (12)
      DBNull.Value, // Object (13)
      3,            // SByte (14)
      24,           // Single (15)
      DBNull.Value, // String (16)
      DBNull.Value, // Time (17)
      5,            // UInt16 (18)
      10,           // UInt32 (19)
      19,           // UInt64 (20)
      53,           // VarNumeric (21)
      DBNull.Value, // AnsiStringFixedLength (22)
      DBNull.Value, // StringFixedLength (23)
      DBNull.Value, // ?? (24)
      DBNull.Value, // Xml (25)
      DBNull.Value, // DateTime2 (26)
      DBNull.Value  // DateTimeOffset (27)
    };

    internal static object DbTypeToNumericScale(DbType typ)
    {
      return _dbtypetonumericscale[(int)typ];
    }

    private static object[] _dbtypetonumericscale = {
      DBNull.Value, // AnsiString (0)
      DBNull.Value, // Binary (1)
      0,            // Byte (2)
      DBNull.Value, // Boolean (3)
      4,            // Currency (4)
      DBNull.Value, // Date (5)
      DBNull.Value, // DateTime (6)
      DBNull.Value, // Decimal (7)
      DBNull.Value, // Double (8)
      DBNull.Value, // Guid (9)
      0,            // Int16 (10)
      0,            // Int32 (11)
      0,            // Int64 (12)
      DBNull.Value, // Object (13)
      0,            // SByte (14)
      DBNull.Value, // Single (15)
      DBNull.Value, // String (16)
      DBNull.Value, // Time (17)
      0,            // UInt16 (18)
      0,            // UInt32 (19)
      0,            // UInt64 (20)
      0,            // VarNumeric (21)
      DBNull.Value, // AnsiStringFixedLength (22)
      DBNull.Value, // StringFixedLength (23)
      DBNull.Value, // ?? (24)
      DBNull.Value, // Xml (25)
      DBNull.Value, // DateTime2 (26)
      DBNull.Value  // DateTimeOffset (27)
    };

    /// <summary>
    /// Determines the default database type name to be used when a
    /// per-connection value is not available.
    /// </summary>
    /// <param name="connection">
    /// The connection context for type mappings, if any.
    /// </param>
    /// <returns>
    /// The default database type name to use.
    /// </returns>
    private static string GetDefaultTypeName(
        SQLiteConnection connection
        )
    {
        SQLiteConnectionFlags flags = (connection != null) ?
            connection.Flags : SQLiteConnectionFlags.None;

        if (HelperMethods.HasFlags(
                flags, SQLiteConnectionFlags.NoConvertSettings))
        {
            return FallbackDefaultTypeName;
        }

        string name = "Use_SQLiteConvert_DefaultTypeName";
        object value = null;
        string @default = null;

        if ((connection == null) ||
            !connection.TryGetCachedSetting(name, @default, out value))
        {
            try
            {
                value = UnsafeNativeMethods.GetSettingValue(name, @default);

                if (value == null)
                    value = FallbackDefaultTypeName;
            }
            finally
            {
                if (connection != null)
                    connection.SetCachedSetting(name, value);
            }
        }

        return SettingValueToString(value);
    }

#if !NET_COMPACT_20 && TRACE_WARNING
    /// <summary>
    /// If applicable, issues a trace log message warning about falling back to
    /// the default database type name.
    /// </summary>
    /// <param name="dbType">
    /// The database value type.
    /// </param>
    /// <param name="flags">
    /// The flags associated with the parent connection object.
    /// </param>
    /// <param name="typeName">
    /// The textual name of the database type.
    /// </param>
    private static void DefaultTypeNameWarning(
        DbType dbType,
        SQLiteConnectionFlags flags,
        string typeName
        )
    {
        if (HelperMethods.HasFlags(flags, SQLiteConnectionFlags.TraceWarning))
        {
            Trace.WriteLine(HelperMethods.StringFormat(
                CultureInfo.CurrentCulture,
                "WARNING: Type mapping failed, returning default name \"{0}\" for type {1}.",
                typeName, dbType));
        }
    }

    /// <summary>
    /// If applicable, issues a trace log message warning about falling back to
    /// the default database value type.
    /// </summary>
    /// <param name="typeName">
    /// The textual name of the database type.
    /// </param>
    /// <param name="flags">
    /// The flags associated with the parent connection object.
    /// </param>
    /// <param name="dbType">
    /// The database value type.
    /// </param>
    private static void DefaultDbTypeWarning(
        string typeName,
        SQLiteConnectionFlags flags,
        DbType? dbType
        )
    {
        if (!String.IsNullOrEmpty(typeName) &&
            HelperMethods.HasFlags(flags, SQLiteConnectionFlags.TraceWarning))
        {
            Trace.WriteLine(HelperMethods.StringFormat(
                CultureInfo.CurrentCulture,
                "WARNING: Type mapping failed, returning default type {0} for name \"{1}\".",
                dbType, typeName));
        }
    }
#endif

    /// <summary>
    /// For a given database value type, return the "closest-match" textual database type name.
    /// </summary>
    /// <param name="connection">The connection context for custom type mappings, if any.</param>
    /// <param name="dbType">The database value type.</param>
    /// <param name="flags">The flags associated with the parent connection object.</param>
    /// <returns>The type name or an empty string if it cannot be determined.</returns>
    internal static string DbTypeToTypeName(
        SQLiteConnection connection,
        DbType dbType,
        SQLiteConnectionFlags flags
        )
    {
        string defaultTypeName = null;

        if (connection != null)
        {
            flags |= connection.Flags;

            if (HelperMethods.HasFlags(flags, SQLiteConnectionFlags.UseConnectionTypes))
            {
                SQLiteDbTypeMap connectionTypeNames = connection._typeNames;

                if (connectionTypeNames != null)
                {
                    SQLiteDbTypeMapping value;

                    if (connectionTypeNames.TryGetValue(dbType, out value))
                        return value.typeName;
                }
            }

            //
            // NOTE: Use the default database type name for the connection.
            //
            defaultTypeName = connection.DefaultTypeName;
        }

        if (HelperMethods.HasFlags(flags, SQLiteConnectionFlags.NoGlobalTypes))
        {
            if (defaultTypeName != null)
                return defaultTypeName;

            defaultTypeName = GetDefaultTypeName(connection);

#if !NET_COMPACT_20 && TRACE_WARNING
            DefaultTypeNameWarning(dbType, flags, defaultTypeName);
#endif

            return defaultTypeName;
        }

        {
            SQLiteDbTypeMapping value;

            if ((_typeNames != null) &&
                _typeNames.TryGetValue(dbType, out value))
            {
                return value.typeName;
            }
        }

        if (defaultTypeName != null)
            return defaultTypeName;

        defaultTypeName = GetDefaultTypeName(connection);

#if !NET_COMPACT_20 && TRACE_WARNING
        DefaultTypeNameWarning(dbType, flags, defaultTypeName);
#endif

        return defaultTypeName;
    }

    /// <summary>
    /// Convert a DbType to a Type
    /// </summary>
    /// <param name="typ">The DbType to convert from</param>
    /// <returns>The closest-match .NET type</returns>
    internal static Type DbTypeToType(DbType typ)
    {
      return _dbtypeToType[(int)typ];
    }

    private static Type[] _dbtypeToType = {
      typeof(string),        // AnsiString (0)
      typeof(byte[]),        // Binary (1)
      typeof(byte),          // Byte (2)
      typeof(bool),          // Boolean (3)
      typeof(decimal),       // Currency (4)
      typeof(DateTime),      // Date (5)
      typeof(DateTime),      // DateTime (6)
      typeof(decimal),       // Decimal (7)
      typeof(double),        // Double (8)
      typeof(Guid),          // Guid (9)
      typeof(Int16),         // Int16 (10)
      typeof(Int32),         // Int32 (11)
      typeof(Int64),         // Int64 (12)
      typeof(object),        // Object (13)
      typeof(sbyte),         // SByte (14)
      typeof(float),         // Single (15)
      typeof(string),        // String (16)
      typeof(DateTime),      // Time (17)
      typeof(UInt16),        // UInt16 (18)
      typeof(UInt32),        // UInt32 (19)
      typeof(UInt64),        // UInt64 (20)
      typeof(double),        // VarNumeric (21)
      typeof(string),        // AnsiStringFixedLength (22)
      typeof(string),        // StringFixedLength (23)
      typeof(string),        // ?? (24)
      typeof(string),        // Xml (25)
      typeof(DateTime),      // DateTime2 (26)
#if !PLATFORM_COMPACTFRAMEWORK && (NET_35 || NET_40 || NET_45 || NET_451 || NET_452 || NET_46 || NET_461 || NET_462 || NET_47 || NET_471 || NET_472 || NET_STANDARD_20 || NET_STANDARD_21)
      //
      // NOTE: This type is only available on the
      //       .NET Framework 2.0 SP1 and later.
      //
      typeof(DateTimeOffset) // DateTimeOffset (27)
#else
      typeof(DateTime)       // DateTimeOffset (27)
#endif
    };

    /// <summary>
    /// For a given type, return the closest-match SQLite TypeAffinity, which only understands a very limited subset of types.
    /// </summary>
    /// <param name="typ">The type to evaluate</param>
    /// <param name="flags">The flags associated with the connection.</param>
    /// <returns>The SQLite type affinity for that type.</returns>
    internal static TypeAffinity TypeToAffinity(
        Type typ,
        SQLiteConnectionFlags flags
        )
    {
      TypeCode tc = Type.GetTypeCode(typ);
      if (tc == TypeCode.Object)
      {
        if (typ == typeof(byte[]) || typ == typeof(Guid))
          return TypeAffinity.Blob;
        else
          return TypeAffinity.Text;
      }
      if ((tc == TypeCode.Decimal) &&
          HelperMethods.HasFlags(flags, SQLiteConnectionFlags.GetDecimalAsText))
      {
          return TypeAffinity.Text;
      }
      return _typecodeAffinities[(int)tc];
    }

    private static TypeAffinity[] _typecodeAffinities = {
      TypeAffinity.Null,     // Empty (0)
      TypeAffinity.Blob,     // Object (1)
      TypeAffinity.Null,     // DBNull (2)
      TypeAffinity.Int64,    // Boolean (3)
      TypeAffinity.Int64,    // Char (4)
      TypeAffinity.Int64,    // SByte (5)
      TypeAffinity.Int64,    // Byte (6)
      TypeAffinity.Int64,    // Int16 (7)
      TypeAffinity.Int64,    // UInt16 (8)
      TypeAffinity.Int64,    // Int32 (9)
      TypeAffinity.Int64,    // UInt32 (10)
      TypeAffinity.Int64,    // Int64 (11)
      TypeAffinity.Int64,    // UInt64 (12)
      TypeAffinity.Double,   // Single (13)
      TypeAffinity.Double,   // Double (14)
      TypeAffinity.Double,   // Decimal (15)
      TypeAffinity.DateTime, // DateTime (16)
      TypeAffinity.Null,     // ?? (17)
      TypeAffinity.Text      // String (18)
    };

    /// <summary>
    /// Builds and returns a map containing the database column types
    /// recognized by this provider.
    /// </summary>
    /// <returns>
    /// A map containing the database column types recognized by this
    /// provider.
    /// </returns>
    private static SQLiteDbTypeMap GetSQLiteDbTypeMap()
    {
        return new SQLiteDbTypeMap(new SQLiteDbTypeMapping[] {
            new SQLiteDbTypeMapping("BIGINT", DbType.Int64, false),
            new SQLiteDbTypeMapping("BIGUINT", DbType.UInt64, false),
            new SQLiteDbTypeMapping("BINARY", DbType.Binary, false),
            new SQLiteDbTypeMapping("BIT", DbType.Boolean, true),
            new SQLiteDbTypeMapping("BLOB", DbType.Binary, true),
            new SQLiteDbTypeMapping("BOOL", DbType.Boolean, false),
            new SQLiteDbTypeMapping("BOOLEAN", DbType.Boolean, false),
            new SQLiteDbTypeMapping("CHAR", DbType.AnsiStringFixedLength, true),
            new SQLiteDbTypeMapping("CLOB", DbType.String, false),
            new SQLiteDbTypeMapping("COUNTER", DbType.Int64, false),
            new SQLiteDbTypeMapping("CURRENCY", DbType.Decimal, false),
            new SQLiteDbTypeMapping("DATE", DbType.DateTime, false),
            new SQLiteDbTypeMapping("DATETIME", DbType.DateTime, true),
            new SQLiteDbTypeMapping("DECIMAL", DbType.Decimal, true),
            new SQLiteDbTypeMapping("DECIMALTEXT", DbType.Decimal, false),
            new SQLiteDbTypeMapping("DOUBLE", DbType.Double, false),
            new SQLiteDbTypeMapping("FLOAT", DbType.Double, false),
            new SQLiteDbTypeMapping("GENERAL", DbType.Binary, false),
            new SQLiteDbTypeMapping("GUID", DbType.Guid, false),
            new SQLiteDbTypeMapping("IDENTITY", DbType.Int64, false),
            new SQLiteDbTypeMapping("IMAGE", DbType.Binary, false),
            new SQLiteDbTypeMapping("INT", DbType.Int32, true),
            new SQLiteDbTypeMapping("INT8", DbType.SByte, false),
            new SQLiteDbTypeMapping("INT16", DbType.Int16, false),
            new SQLiteDbTypeMapping("INT32", DbType.Int32, false),
            new SQLiteDbTypeMapping("INT64", DbType.Int64, false),
            new SQLiteDbTypeMapping("INTEGER", DbType.Int64, true),
            new SQLiteDbTypeMapping("INTEGER8", DbType.SByte, false),
            new SQLiteDbTypeMapping("INTEGER16", DbType.Int16, false),
            new SQLiteDbTypeMapping("INTEGER32", DbType.Int32, false),
            new SQLiteDbTypeMapping("INTEGER64", DbType.Int64, false),
            new SQLiteDbTypeMapping("LOGICAL", DbType.Boolean, false),
            new SQLiteDbTypeMapping("LONG", DbType.Int64, false),
            new SQLiteDbTypeMapping("LONGCHAR", DbType.String, false),
            new SQLiteDbTypeMapping("LONGTEXT", DbType.String, false),
            new SQLiteDbTypeMapping("LONGVARCHAR", DbType.String, false),
            new SQLiteDbTypeMapping("MEMO", DbType.String, false),
            new SQLiteDbTypeMapping("MONEY", DbType.Decimal, false),
            new SQLiteDbTypeMapping("NCHAR", DbType.StringFixedLength, true),
            new SQLiteDbTypeMapping("NOTE", DbType.String, false),
            new SQLiteDbTypeMapping("NTEXT", DbType.String, false),
            new SQLiteDbTypeMapping("NUMBER", DbType.Decimal, false),
            new SQLiteDbTypeMapping("NUMERIC", DbType.Decimal, false),
            new SQLiteDbTypeMapping("NUMERICTEXT", DbType.Decimal, false),
            new SQLiteDbTypeMapping("NVARCHAR", DbType.String, true),
            new SQLiteDbTypeMapping("OLEOBJECT", DbType.Binary, false),
            new SQLiteDbTypeMapping("RAW", DbType.Binary, false),
            new SQLiteDbTypeMapping("REAL", DbType.Double, true),
            new SQLiteDbTypeMapping("SINGLE", DbType.Single, true),
            new SQLiteDbTypeMapping("SMALLDATE", DbType.DateTime, false),
            new SQLiteDbTypeMapping("SMALLINT", DbType.Int16, true),
            new SQLiteDbTypeMapping("SMALLUINT", DbType.UInt16, true),
            new SQLiteDbTypeMapping("STRING", DbType.String, false),
            new SQLiteDbTypeMapping("TEXT", DbType.String, false),
            new SQLiteDbTypeMapping("TIME", DbType.DateTime, false),
            new SQLiteDbTypeMapping("TIMESTAMP", DbType.DateTime, false),
            new SQLiteDbTypeMapping("TINYINT", DbType.Byte, true),
            new SQLiteDbTypeMapping("TINYSINT", DbType.SByte, true),
            new SQLiteDbTypeMapping("UINT", DbType.UInt32, true),
            new SQLiteDbTypeMapping("UINT8", DbType.Byte, false),
            new SQLiteDbTypeMapping("UINT16", DbType.UInt16, false),
            new SQLiteDbTypeMapping("UINT32", DbType.UInt32, false),
            new SQLiteDbTypeMapping("UINT64", DbType.UInt64, false),
            new SQLiteDbTypeMapping("ULONG", DbType.UInt64, false),
            new SQLiteDbTypeMapping("UNIQUEIDENTIFIER", DbType.Guid, true),
            new SQLiteDbTypeMapping("UNSIGNEDINTEGER", DbType.UInt64, true),
            new SQLiteDbTypeMapping("UNSIGNEDINTEGER8", DbType.Byte, false),
            new SQLiteDbTypeMapping("UNSIGNEDINTEGER16", DbType.UInt16, false),
            new SQLiteDbTypeMapping("UNSIGNEDINTEGER32", DbType.UInt32, false),
            new SQLiteDbTypeMapping("UNSIGNEDINTEGER64", DbType.UInt64, false),
            new SQLiteDbTypeMapping("VARBINARY", DbType.Binary, false),
            new SQLiteDbTypeMapping("VARCHAR", DbType.AnsiString, true),
            new SQLiteDbTypeMapping("VARCHAR2", DbType.AnsiString, false),
            new SQLiteDbTypeMapping("YESNO", DbType.Boolean, false)
        });
    }

    /// <summary>
    /// Determines if a database type is considered to be a string.
    /// </summary>
    /// <param name="type">
    /// The database type to check.
    /// </param>
    /// <returns>
    /// Non-zero if the database type is considered to be a string, zero
    /// otherwise.
    /// </returns>
    internal static bool IsStringDbType(
        DbType type
        )
    {
        switch (type)
        {
            case DbType.AnsiString:
            case DbType.String:
            case DbType.AnsiStringFixedLength:
            case DbType.StringFixedLength:
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Determines and returns the runtime configuration setting string that
    /// should be used in place of the specified object value.
    /// </summary>
    /// <param name="value">
    /// The object value to convert to a string.
    /// </param>
    /// <returns>
    /// Either the string to use in place of the object value -OR- null if it
    /// cannot be determined.
    /// </returns>
    private static string SettingValueToString(
        object value
        )
    {
        if (value is string)
            return (string)value;

        if (value != null)
            return value.ToString();

        return null;
    }

    /// <summary>
    /// Determines the default <see cref="DbType" /> value to be used when a
    /// per-connection value is not available.
    /// </summary>
    /// <param name="connection">
    /// The connection context for type mappings, if any.
    /// </param>
    /// <returns>
    /// The default <see cref="DbType" /> value to use.
    /// </returns>
    private static DbType GetDefaultDbType(
        SQLiteConnection connection
        )
    {
        SQLiteConnectionFlags flags = (connection != null) ?
            connection.Flags : SQLiteConnectionFlags.None;

        if (HelperMethods.HasFlags(
                flags, SQLiteConnectionFlags.NoConvertSettings))
        {
            return FallbackDefaultDbType;
        }

        bool found = false;
        string name = "Use_SQLiteConvert_DefaultDbType";
        object value = null;
        string @default = null;

        if ((connection == null) ||
            !connection.TryGetCachedSetting(name, @default, out value))
        {
            value = UnsafeNativeMethods.GetSettingValue(name, @default);

            if (value == null)
                value = FallbackDefaultDbType;
        }
        else
        {
            found = true;
        }

        try
        {
            if (!(value is DbType))
            {
                value = SQLiteConnection.TryParseEnum(
                    typeof(DbType), SettingValueToString(value), true);

                if (!(value is DbType))
                    value = FallbackDefaultDbType;
            }

            return (DbType)value;
        }
        finally
        {
            if (!found && (connection != null))
                connection.SetCachedSetting(name, value);
        }
    }

    /// <summary>
    /// Converts the object value, which is assumed to have originated
    /// from a <see cref="DataRow" />, to a string value.
    /// </summary>
    /// <param name="value">
    /// The value to be converted to a string.
    /// </param>
    /// <returns>
    /// A null value will be returned if the original value is null -OR-
    /// the original value is <see cref="DBNull.Value" />.  Otherwise,
    /// the original value will be converted to a string, using its
    /// (possibly overridden) <see cref="Object.ToString" /> method and
    /// then returned.
    /// </returns>
    public static string GetStringOrNull(
        object value
        )
    {
        if (value == null)
            return null;

        if (value is string)
            return (string)value;

        if (value == DBNull.Value)
            return null;

        return value.ToString();
    }

    /// <summary>
    /// Determines if the specified textual value appears to be a
    /// <see cref="DBNull" /> value.
    /// </summary>
    /// <param name="text">
    /// The textual value to inspect.
    /// </param>
    /// <returns>
    /// Non-zero if the text looks like a <see cref="DBNull" /> value,
    /// zero otherwise.
    /// </returns>
    internal static bool LooksLikeNull(
        string text
        )
    {
        return (text == null);
    }

    /// <summary>
    /// Determines if the specified textual value appears to be an
    /// <see cref="Int64" /> value.
    /// </summary>
    /// <param name="text">
    /// The textual value to inspect.
    /// </param>
    /// <returns>
    /// Non-zero if the text looks like an <see cref="Int64" /> value,
    /// zero otherwise.
    /// </returns>
    internal static bool LooksLikeInt64(
        string text
        )
    {
        long longValue;

#if !PLATFORM_COMPACTFRAMEWORK
        if (!long.TryParse(
                text, NumberStyles.Integer, CultureInfo.InvariantCulture,
                out longValue))
        {
            return false;
        }
#else
        try
        {
            longValue = long.Parse(
                text, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }
        catch
        {
            return false;
        }
#endif

        return String.Equals(
            longValue.ToString(CultureInfo.InvariantCulture), text,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Determines if the specified textual value appears to be a
    /// <see cref="Double" /> value.
    /// </summary>
    /// <param name="text">
    /// The textual value to inspect.
    /// </param>
    /// <returns>
    /// Non-zero if the text looks like a <see cref="Double" /> value,
    /// zero otherwise.
    /// </returns>
    internal static bool LooksLikeDouble(
        string text
        )
    {
        double doubleValue;

#if !PLATFORM_COMPACTFRAMEWORK
        if (!double.TryParse(
                text, NumberStyles.Float | NumberStyles.AllowThousands,
                CultureInfo.InvariantCulture, out doubleValue))
        {
            return false;
        }
#else
        try
        {
            doubleValue = double.Parse(text, CultureInfo.InvariantCulture);
        }
        catch
        {
            return false;
        }
#endif

        return String.Equals(
            doubleValue.ToString(CultureInfo.InvariantCulture), text,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Determines if the specified textual value appears to be a
    /// <see cref="DateTime" /> value.
    /// </summary>
    /// <param name="convert">
    /// The <see cref="SQLiteConvert" /> object instance configured with
    /// the chosen <see cref="DateTime" /> format.
    /// </param>
    /// <param name="text">
    /// The textual value to inspect.
    /// </param>
    /// <returns>
    /// Non-zero if the text looks like a <see cref="DateTime" /> in the
    /// configured format, zero otherwise.
    /// </returns>
    internal static bool LooksLikeDateTime(
        SQLiteConvert convert,
        string text
        )
    {
        if (convert == null)
            return false;

        try
        {
            DateTime dateTimeValue = convert.ToDateTime(text);

            if (String.Equals(
                    convert.ToString(dateTimeValue),
                    text, StringComparison.Ordinal))
            {
                return true;
            }
        }
        catch
        {
            // do nothing.
        }

        return false;
    }

    /// <summary>
    /// For a given textual database type name, return the "closest-match" database type.
    /// This method is called during query result processing; therefore, its performance
    /// is critical.
    /// </summary>
    /// <param name="connection">The connection context for custom type mappings, if any.</param>
    /// <param name="typeName">The textual name of the database type to match.</param>
    /// <param name="flags">The flags associated with the parent connection object.</param>
    /// <returns>The .NET DBType the text evaluates to.</returns>
    internal static DbType TypeNameToDbType(
        SQLiteConnection connection,
        string typeName,
        SQLiteConnectionFlags flags
        )
    {
        DbType? defaultDbType = null;

        if (connection != null)
        {
            flags |= connection.Flags;

            if (HelperMethods.HasFlags(flags, SQLiteConnectionFlags.UseConnectionTypes))
            {
                SQLiteDbTypeMap connectionTypeNames = connection._typeNames;

                if (connectionTypeNames != null)
                {
                    if (typeName != null)
                    {
                        SQLiteDbTypeMapping value;

                        if (connectionTypeNames.TryGetValue(typeName, out value))
                        {
                            return value.dataType;
                        }
                        else
                        {
                            int index = typeName.IndexOf('(');

                            if ((index > 0) &&
                                connectionTypeNames.TryGetValue(typeName.Substring(0, index).TrimEnd(), out value))
                            {
                                return value.dataType;
                            }
                        }
                    }
                }
            }

            //
            // NOTE: Use the default database type for the connection.
            //
            defaultDbType = connection.DefaultDbType;
        }

        if (HelperMethods.HasFlags(flags, SQLiteConnectionFlags.NoGlobalTypes))
        {
            if (defaultDbType != null)
                return (DbType)defaultDbType;

            defaultDbType = GetDefaultDbType(connection);

#if !NET_COMPACT_20 && TRACE_WARNING
            DefaultDbTypeWarning(typeName, flags, defaultDbType);
#endif

            return (DbType)defaultDbType;
        }

        {
            if ((_typeNames != null) && (typeName != null))
            {
                SQLiteDbTypeMapping value;

                if (_typeNames.TryGetValue(typeName, out value))
                {
                    return value.dataType;
                }
                else
                {
                    int index = typeName.IndexOf('(');

                    if ((index > 0) &&
                        _typeNames.TryGetValue(typeName.Substring(0, index).TrimEnd(), out value))
                    {
                        return value.dataType;
                    }
                }
            }
        }

        if (defaultDbType != null)
            return (DbType)defaultDbType;

        defaultDbType = GetDefaultDbType(connection);

#if !NET_COMPACT_20 && TRACE_WARNING
        DefaultDbTypeWarning(typeName, flags, defaultDbType);
#endif

        return (DbType)defaultDbType;
    }
    #endregion

    private static readonly SQLiteDbTypeMap _typeNames = GetSQLiteDbTypeMap();
  }

  /// <summary>
  /// SQLite has very limited types, and is inherently text-based.  The first 5 types below represent the sum of all types SQLite
  /// understands.  The DateTime extension to the spec is for internal use only.
  /// </summary>
  public enum TypeAffinity
  {
    /// <summary>
    /// Not used
    /// </summary>
    Uninitialized = 0,
    /// <summary>
    /// All integers in SQLite default to Int64
    /// </summary>
    Int64 = 1,
    /// <summary>
    /// All floating point numbers in SQLite default to double
    /// </summary>
    Double = 2,
    /// <summary>
    /// The default data type of SQLite is text
    /// </summary>
    Text = 3,
    /// <summary>
    /// Typically blob types are only seen when returned from a function
    /// </summary>
    Blob = 4,
    /// <summary>
    /// Null types can be returned from functions
    /// </summary>
    Null = 5,
    /// <summary>
    /// Used internally by this provider
    /// </summary>
    DateTime = 10,
    /// <summary>
    /// Used internally by this provider
    /// </summary>
    None = 11,
  }

  /// <summary>
  /// These are the event types associated with the
  /// <see cref="SQLiteConnectionEventHandler" />
  /// delegate (and its corresponding event) and the
  /// <see cref="ConnectionEventArgs" /> class.
  /// </summary>
  public enum SQLiteConnectionEventType
  {
      /// <summary>
      /// Not used.
      /// </summary>
      Invalid = -1,

      /// <summary>
      /// Not used.
      /// </summary>
      Unknown = 0,

      /// <summary>
      /// The connection is being opened.
      /// </summary>
      Opening = 1,

      /// <summary>
      /// The connection string has been parsed.
      /// </summary>
      ConnectionString = 2,

      /// <summary>
      /// The connection was opened.
      /// </summary>
      Opened = 3,

      /// <summary>
      /// The <see cref="ChangeDatabase" /> method was called on the
      /// connection.
      /// </summary>
      ChangeDatabase = 4,

      /// <summary>
      /// A transaction was created using the connection.
      /// </summary>
      NewTransaction = 5,

      /// <summary>
      /// The connection was enlisted into a transaction.
      /// </summary>
      EnlistTransaction = 6,

      /// <summary>
      /// A command was created using the connection.
      /// </summary>
      NewCommand = 7,

      /// <summary>
      /// A data reader was created using the connection.
      /// </summary>
      NewDataReader = 8,

      /// <summary>
      /// An instance of a <see cref="CriticalHandle" /> derived class has
      /// been created to wrap a native resource.
      /// </summary>
      NewCriticalHandle = 9,

      /// <summary>
      /// The connection is being closed.
      /// </summary>
      Closing = 10,

      /// <summary>
      /// The connection was closed.
      /// </summary>
      Closed = 11,

      /// <summary>
      /// A command is being disposed.
      /// </summary>
      DisposingCommand = 12,

      /// <summary>
      /// A data reader is being disposed.
      /// </summary>
      DisposingDataReader = 13,

      /// <summary>
      /// A data reader is being closed.
      /// </summary>
      ClosingDataReader = 14,

      /// <summary>
      /// A native resource was opened (i.e. obtained) from the pool.
      /// </summary>
      OpenedFromPool = 15,

      /// <summary>
      /// A native resource was closed (i.e. released) to the pool.
      /// </summary>
      ClosedToPool = 16
  }

  /// <summary>
  /// This implementation of SQLite for ADO.NET can process date/time fields in
  /// databases in one of six formats.
  /// </summary>
  /// <remarks>
  /// ISO8601 format is more compatible, readable, fully-processable, but less
  /// accurate as it does not provide time down to fractions of a second.
  /// JulianDay is the numeric format the SQLite uses internally and is arguably
  /// the most compatible with 3rd party tools.  It is not readable as text
  /// without post-processing.  Ticks less compatible with 3rd party tools that
  /// query the database, and renders the DateTime field unreadable as text
  /// without post-processing.  UnixEpoch is more compatible with Unix systems.
  /// InvariantCulture allows the configured format for the invariant culture
  /// format to be used and is human readable.  CurrentCulture allows the
  /// configured format for the current culture to be used and is also human
  /// readable.
  ///
  /// The preferred order of choosing a DateTime format is JulianDay, ISO8601,
  /// and then Ticks.  Ticks is mainly present for legacy code support.
  /// </remarks>
  public enum SQLiteDateFormats
  {
    /// <summary>
    /// Use the value of DateTime.Ticks.  This value is not recommended and is not well supported with LINQ.
    /// </summary>
    Ticks = 0,
    /// <summary>
    /// Use the ISO-8601 format.  Uses the "yyyy-MM-dd HH:mm:ss.FFFFFFFK" format for UTC DateTime values and
    /// "yyyy-MM-dd HH:mm:ss.FFFFFFF" format for local DateTime values).
    /// </summary>
    ISO8601 = 1,
    /// <summary>
    /// The interval of time in days and fractions of a day since January 1, 4713 BC.
    /// </summary>
    JulianDay = 2,
    /// <summary>
    /// The whole number of seconds since the Unix epoch (January 1, 1970).
    /// </summary>
    UnixEpoch = 3,
    /// <summary>
    /// Any culture-independent string value that the .NET Framework can interpret as a valid DateTime.
    /// </summary>
    InvariantCulture = 4,
    /// <summary>
    /// Any string value that the .NET Framework can interpret as a valid DateTime using the current culture.
    /// </summary>
    CurrentCulture = 5,
    /// <summary>
    /// The default format for this provider.
    /// </summary>
    Default = ISO8601
  }

  /// <summary>
  /// This enum determines how SQLite treats its journal file.
  /// </summary>
  /// <remarks>
  /// By default SQLite will create and delete the journal file when needed during a transaction.
  /// However, for some computers running certain filesystem monitoring tools, the rapid
  /// creation and deletion of the journal file can cause those programs to fail, or to interfere with SQLite.
  ///
  /// If a program or virus scanner is interfering with SQLite's journal file, you may receive errors like "unable to open database file"
  /// when starting a transaction.  If this is happening, you may want to change the default journal mode to Persist.
  /// </remarks>
  public enum SQLiteJournalModeEnum
  {
    /// <summary>
    /// The default mode, this causes SQLite to use the existing journaling mode for the database.
    /// </summary>
    Default = -1,
    /// <summary>
    /// SQLite will create and destroy the journal file as-needed.
    /// </summary>
    Delete = 0,
    /// <summary>
    /// When this is set, SQLite will keep the journal file even after a transaction has completed.  It's contents will be erased,
    /// and the journal re-used as often as needed.  If it is deleted, it will be recreated the next time it is needed.
    /// </summary>
    Persist = 1,
    /// <summary>
    /// This option disables the rollback journal entirely.  Interrupted transactions or a program crash can cause database
    /// corruption in this mode!
    /// </summary>
    Off = 2,
    /// <summary>
    /// SQLite will truncate the journal file to zero-length instead of deleting it.
    /// </summary>
    Truncate = 3,
    /// <summary>
    /// SQLite will store the journal in volatile RAM.  This saves disk I/O but at the expense of database safety and integrity.
    /// If the application using SQLite crashes in the middle of a transaction when the MEMORY journaling mode is set, then the
    /// database file will very likely go corrupt.
    /// </summary>
    Memory = 4,
    /// <summary>
    /// SQLite uses a write-ahead log instead of a rollback journal to implement transactions.  The WAL journaling mode is persistent;
    /// after being set it stays in effect across multiple database connections and after closing and reopening the database. A database
    /// in WAL journaling mode can only be accessed by SQLite version 3.7.0 or later.
    /// </summary>
    Wal = 5
  }

  /// <summary>
  /// Possible values for the "synchronous" database setting.  This setting determines
  /// how often the database engine calls the xSync method of the VFS.
  /// </summary>
  internal enum SQLiteSynchronousEnum
  {
      /// <summary>
      /// Use the default "synchronous" database setting.  Currently, this should be
      /// the same as using the FULL mode.
      /// </summary>
      Default = -1,

      /// <summary>
      /// The database engine continues without syncing as soon as it has handed
      /// data off to the operating system.  If the application running SQLite
      /// crashes, the data will be safe, but the database might become corrupted
      /// if the operating system crashes or the computer loses power before that
      /// data has been written to the disk surface.
      /// </summary>
      Off = 0,

      /// <summary>
      /// The database engine will still sync at the most critical moments, but
      /// less often than in FULL mode.  There is a very small (though non-zero)
      /// chance that a power failure at just the wrong time could corrupt the
      /// database in NORMAL mode.
      /// </summary>
      Normal = 1,

      /// <summary>
      /// The database engine will use the xSync method of the VFS to ensure that
      /// all content is safely written to the disk surface prior to continuing.
      /// This ensures that an operating system crash or power failure will not
      /// corrupt the database.  FULL synchronous is very safe, but it is also
      /// slower.
      /// </summary>
      Full = 2
  }

  /// <summary>
  /// The requested command execution type.  This controls which method of the
  /// <see cref="SQLiteCommand" /> object will be called.
  /// </summary>
  public enum SQLiteExecuteType
  {
      /// <summary>
      /// Do nothing.  No method will be called.
      /// </summary>
      None = 0,

      /// <summary>
      /// The command is not expected to return a result -OR- the result is not
      /// needed.  The <see cref="SQLiteCommand.ExecuteNonQuery()" /> or
      /// <see cref="SQLiteCommand.ExecuteNonQuery(CommandBehavior)" />  method
      /// will be called.
      /// </summary>
      NonQuery = 1,

      /// <summary>
      /// The command is expected to return a scalar result -OR- the result should
      /// be limited to a scalar result.  The <see cref="SQLiteCommand.ExecuteScalar()" />
      /// or <see cref="SQLiteCommand.ExecuteScalar(CommandBehavior)" /> method will
      /// be called.
      /// </summary>
      Scalar = 2,

      /// <summary>
      /// The command is expected to return <see cref="SQLiteDataReader" /> result.
      /// The <see cref="SQLiteCommand.ExecuteReader()" /> or
      /// <see cref="SQLiteCommand.ExecuteReader(CommandBehavior)" /> method will
      /// be called.
      /// </summary>
      Reader = 3,

      /// <summary>
      /// Use the default command execution type.  Using this value is the same
      /// as using the <see cref="SQLiteExecuteType.NonQuery" /> value.
      /// </summary>
      Default = NonQuery /* TODO: Good default? */
  }

  /// <summary>
  /// The action code responsible for the current call into the authorizer.
  /// </summary>
  public enum SQLiteAuthorizerActionCode
  {
      /// <summary>
      /// No action is being performed.  This value should not be used from
      /// external code.
      /// </summary>
      None = -1,

      /// <summary>
      /// No longer used.
      /// </summary>
      Copy = 0,

      /// <summary>
      /// An index will be created.  The action-specific arguments are the
      /// index name and the table name.
      ///
      /// </summary>
      CreateIndex = 1,

      /// <summary>
      /// A table will be created.  The action-specific arguments are the
      /// table name and a null value.
      /// </summary>
      CreateTable = 2,

      /// <summary>
      /// A temporary index will be created.  The action-specific arguments
      /// are the index name and the table name.
      /// </summary>
      CreateTempIndex = 3,

      /// <summary>
      /// A temporary table will be created.  The action-specific arguments
      /// are the table name and a null value.
      /// </summary>
      CreateTempTable = 4,

      /// <summary>
      /// A temporary trigger will be created.  The action-specific arguments
      /// are the trigger name and the table name.
      /// </summary>
      CreateTempTrigger = 5,

      /// <summary>
      /// A temporary view will be created.  The action-specific arguments are
      /// the view name and a null value.
      /// </summary>
      CreateTempView = 6,

      /// <summary>
      /// A trigger will be created.  The action-specific arguments are the
      /// trigger name and the table name.
      /// </summary>
      CreateTrigger = 7,

      /// <summary>
      /// A view will be created.  The action-specific arguments are the view
      /// name and a null value.
      /// </summary>
      CreateView = 8,

      /// <summary>
      /// A DELETE statement will be executed.  The action-specific arguments
      /// are the table name and a null value.
      /// </summary>
      Delete = 9,

      /// <summary>
      /// An index will be dropped.  The action-specific arguments are the
      /// index name and the table name.
      /// </summary>
      DropIndex = 10,

      /// <summary>
      /// A table will be dropped.  The action-specific arguments are the tables
      /// name and a null value.
      /// </summary>
      DropTable = 11,

      /// <summary>
      /// A temporary index will be dropped.  The action-specific arguments are
      /// the index name and the table name.
      /// </summary>
      DropTempIndex = 12,

      /// <summary>
      /// A temporary table will be dropped.  The action-specific arguments are
      /// the table name and a null value.
      /// </summary>
      DropTempTable = 13,

      /// <summary>
      /// A temporary trigger will be dropped.  The action-specific arguments
      /// are the trigger name and the table name.
      /// </summary>
      DropTempTrigger = 14,

      /// <summary>
      /// A temporary view will be dropped.  The action-specific arguments are
      /// the view name and a null value.
      /// </summary>
      DropTempView = 15,

      /// <summary>
      /// A trigger will be dropped.  The action-specific arguments are the
      /// trigger name and the table name.
      /// </summary>
      DropTrigger = 16,

      /// <summary>
      /// A view will be dropped.  The action-specific arguments are the view
      /// name and a null value.
      /// </summary>
      DropView = 17,

      /// <summary>
      /// An INSERT statement will be executed.  The action-specific arguments
      /// are the table name and a null value.
      /// </summary>
      Insert = 18,

      /// <summary>
      /// A PRAGMA statement will be executed.  The action-specific arguments
      /// are the name of the PRAGMA and the new value or a null value.
      /// </summary>
      Pragma = 19,

      /// <summary>
      /// A table column will be read.  The action-specific arguments are the
      /// table name and the column name.
      /// </summary>
      Read = 20,

      /// <summary>
      /// A SELECT statement will be executed.  The action-specific arguments
      /// are both null values.
      /// </summary>
      Select = 21,

      /// <summary>
      /// A transaction will be started, committed, or rolled back.  The
      /// action-specific arguments are the name of the operation (BEGIN,
      /// COMMIT, or ROLLBACK) and a null value.
      /// </summary>
      Transaction = 22,

      /// <summary>
      /// An UPDATE statement will be executed.  The action-specific arguments
      /// are the table name and the column name.
      /// </summary>
      Update = 23,

      /// <summary>
      /// A database will be attached to the connection.  The action-specific
      /// arguments are the database file name and a null value.
      /// </summary>
      Attach = 24,

      /// <summary>
      /// A database will be detached from the connection.  The action-specific
      /// arguments are the database name and a null value.
      /// </summary>
      Detach = 25,

      /// <summary>
      /// The schema of a table will be altered.  The action-specific arguments
      /// are the database name and the table name.
      /// </summary>
      AlterTable = 26,

      /// <summary>
      /// An index will be deleted and then recreated.  The action-specific
      /// arguments are the index name and a null value.
      /// </summary>
      Reindex = 27,

      /// <summary>
      /// A table will be analyzed to gathers statistics about it.  The
      /// action-specific arguments are the table name and a null value.
      /// </summary>
      Analyze = 28,

      /// <summary>
      /// A virtual table will be created.  The action-specific arguments are
      /// the table name and the module name.
      /// </summary>
      CreateVtable = 29,

      /// <summary>
      /// A virtual table will be dropped.  The action-specific arguments are
      /// the table name and the module name.
      /// </summary>
      DropVtable = 30,

      /// <summary>
      /// A SQL function will be called.  The action-specific arguments are a
      /// null value and the function name.
      /// </summary>
      Function = 31,

      /// <summary>
      /// A savepoint will be created, released, or rolled back.  The
      /// action-specific arguments are the name of the operation (BEGIN,
      /// RELEASE, or ROLLBACK) and the savepoint name.
      /// </summary>
      Savepoint = 32,

      /// <summary>
      /// A recursive query will be executed.  The action-specific arguments
      /// are two null values.
      /// </summary>
      Recursive = 33
  }

  /// <summary>
  /// The possible return codes for the progress callback.
  /// </summary>
  public enum SQLiteProgressReturnCode /* int */
  {
      /// <summary>
      /// The operation should continue.
      /// </summary>
      Continue = 0,

      /// <summary>
      /// The operation should be interrupted.
      /// </summary>
      Interrupt = 1
  }

  /// <summary>
  /// The return code for the current call into the authorizer.
  /// </summary>
  public enum SQLiteAuthorizerReturnCode
  {
      /// <summary>
      /// The action will be allowed.
      /// </summary>
      Ok = 0,

      /// <summary>
      /// The overall action will be disallowed and an error message will be
      /// returned from the query preparation method.
      /// </summary>
      Deny = 1,

      /// <summary>
      /// The specific action will be disallowed; however, the overall action
      /// will continue.  The exact effects of this return code vary depending
      /// on the specific action, please refer to the SQLite core library
      /// documentation for futher details.
      /// </summary>
      Ignore = 2
  }

  /// <summary>
  /// Class used internally to determine the datatype of a column in a resultset
  /// </summary>
  internal sealed class SQLiteType
  {
    /// <summary>
    /// The DbType of the column, or DbType.Object if it cannot be determined
    /// </summary>
    internal DbType Type;
    /// <summary>
    /// The affinity of a column, used for expressions or when Type is DbType.Object
    /// </summary>
    internal TypeAffinity Affinity;

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Constructs a default instance of this type.
    /// </summary>
    public SQLiteType()
    {
      // do nothing.
    }

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Constructs an instance of this type with the specified field values.
    /// </summary>
    /// <param name="affinity">
    /// The type affinity to use for the new instance.
    /// </param>
    /// <param name="type">
    /// The database type to use for the new instance.
    /// </param>
    public SQLiteType(
      TypeAffinity affinity,
      DbType type
      )
      : this()
    {
      this.Affinity = affinity;
      this.Type = type;
    }
  }

  /////////////////////////////////////////////////////////////////////////////

  internal sealed class SQLiteDbTypeMap
      : Dictionary<string, SQLiteDbTypeMapping>
  {
      #region Private Data
      private Dictionary<DbType, SQLiteDbTypeMapping> reverse;
      #endregion

      /////////////////////////////////////////////////////////////////////////

      #region Public Constructors
      public SQLiteDbTypeMap()
          : base(new TypeNameStringComparer())
      {
          reverse = new Dictionary<DbType, SQLiteDbTypeMapping>();
      }

      /////////////////////////////////////////////////////////////////////////

      public SQLiteDbTypeMap(
          IEnumerable<SQLiteDbTypeMapping> collection
          )
          : this()
      {
          Add(collection);
      }
      #endregion

      /////////////////////////////////////////////////////////////////////////

      #region System.Collections.Generic.Dictionary "Overrides"
      public new int Clear()
      {
          int result = 0;

          if (reverse != null)
          {
              result += reverse.Count;
              reverse.Clear();
          }

          result += base.Count;
          base.Clear();

          return result;
      }
      #endregion

      /////////////////////////////////////////////////////////////////////////

      #region SQLiteDbTypeMapping Helper Methods
      public void Add(
          IEnumerable<SQLiteDbTypeMapping> collection
          )
      {
          if (collection == null)
              throw new ArgumentNullException("collection");

          foreach (SQLiteDbTypeMapping item in collection)
              Add(item);
      }

      /////////////////////////////////////////////////////////////////////////

      public void Add(SQLiteDbTypeMapping item)
      {
          if (item == null)
              throw new ArgumentNullException("item");

          if (item.typeName == null)
              throw new ArgumentException("item type name cannot be null");

          base.Add(item.typeName, item);

          if (item.primary)
              reverse.Add(item.dataType, item);
      }
      #endregion

      /////////////////////////////////////////////////////////////////////////

      #region DbType Helper Methods
      public bool ContainsKey(DbType key)
      {
          if (reverse == null)
              return false;

          return reverse.ContainsKey(key);
      }

      /////////////////////////////////////////////////////////////////////////

      public bool TryGetValue(DbType key, out SQLiteDbTypeMapping value)
      {
          if (reverse == null)
          {
              value = null;
              return false;
          }

          return reverse.TryGetValue(key, out value);
      }

      /////////////////////////////////////////////////////////////////////////

      public bool Remove(DbType key)
      {
          if (reverse == null)
              return false;

          return reverse.Remove(key);
      }
      #endregion
  }

  /////////////////////////////////////////////////////////////////////////////

  internal sealed class SQLiteDbTypeMapping
  {
    internal SQLiteDbTypeMapping(
        string newTypeName,
        DbType newDataType,
        bool newPrimary
        )
    {
      typeName = newTypeName;
      dataType = newDataType;
      primary = newPrimary;
    }

    internal string typeName;
    internal DbType dataType;
    internal bool primary;
  }

  internal sealed class TypeNameStringComparer : IEqualityComparer<string>, IComparer<string>
  {
    #region IEqualityComparer<string> Members
    public bool Equals(
      string left,
      string right
      )
    {
      return String.Equals(left, right, StringComparison.OrdinalIgnoreCase);
    }

    ///////////////////////////////////////////////////////////////////////////

    public int GetHashCode(
      string value
      )
    {
      //
      // NOTE: The only thing that we must guarantee here, according
      //       to the MSDN documentation for IEqualityComparer, is
      //       that for two given strings, if Equals return true then
      //       the two strings must hash to the same value.
      //
      if (value != null)
        return StringComparer.OrdinalIgnoreCase.GetHashCode(value);
      else
        throw new ArgumentNullException("value");
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region IComparer<string> Members
    public int Compare(
      string x,
      string y
      )
    {
      if ((x == null) && (y == null))
        return 0;
      else if (x == null)
        return -1;
      else if (y == null)
        return 1;
      else
        return x.CompareTo(y);
    }
    #endregion
  }
}
