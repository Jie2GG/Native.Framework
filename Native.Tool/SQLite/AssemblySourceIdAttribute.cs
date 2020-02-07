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
    /// Defines a source code identifier custom attribute for an assembly
    /// manifest.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
    public sealed class AssemblySourceIdAttribute : Attribute
    {
        /// <summary>
        /// Constructs an instance of this attribute class using the specified
        /// source code identifier value.
        /// </summary>
        /// <param name="value">
        /// The source code identifier value to use.
        /// </param>
        public AssemblySourceIdAttribute(string value)
        {
            sourceId = value;
        }

        ///////////////////////////////////////////////////////////////////////

        private string sourceId;
        /// <summary>
        /// Gets the source code identifier value.
        /// </summary>
        public string SourceId
        {
            get { return sourceId; }
        }
    }
}
