/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Joe Mistachkin (joe@mistachkin.com)
 *
 * Released to the public domain, use at your own risk!
 ********************************************************/

using System;

namespace System.Data.SQLite
{
    /// <summary>
    /// Defines a source code time-stamp custom attribute for an assembly
    /// manifest.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
    public sealed class AssemblySourceTimeStampAttribute : Attribute
    {
        /// <summary>
        /// Constructs an instance of this attribute class using the specified
        /// source code time-stamp value.
        /// </summary>
        /// <param name="value">
        /// The source code time-stamp value to use.
        /// </param>
        public AssemblySourceTimeStampAttribute(string value)
        {
            sourceTimeStamp = value;
        }

        ///////////////////////////////////////////////////////////////////////

        private string sourceTimeStamp;
        /// <summary>
        /// Gets the source code time-stamp value.
        /// </summary>
        public string SourceTimeStamp
        {
            get { return sourceTimeStamp; }
        }
    }
}
