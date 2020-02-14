using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Database
{

	[Table("log")]
	public class CQDBLog
	{
		[PrimaryKey, AutoIncrement, Column("ID")]
		public int Id { get; set; }
		[Column("time")]
		public long Time { get; set; }
		[Column("priority")]
		public long Priority { get; set; }
		[Column("source")]
		public string Source { get; set; }
		[Column("status")]
		public string Status { get; set; }
		[Column("name")]
		public string Name { get; set; }
		[Column("detail")]
		public string Detail { get; set; }
	}
}
