using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
	[Table("event")]
	public class CQDBEvent
	{
		[PrimaryKey, AutoIncrement, Column("id")]
		public int Id { get; set; }
		[Column("type")]
		public long Type { get; set; }
		[Column("tag")]
		public string Tag { get; set; }
		[Column("group")]
		public string Group { get; set; }
		[Column("account")]
		public string Account { get; set; }
		[Column("operator")]
		public bool @Operator { get; set; }
		[Column("content")]
		public string Content { get; set; }
		[Column("time")]
		public long Time { get; set; }
		[Column("extra")]
		public bool Extra { get; set; }
		[Column("proto_extra")]
		public bool Proto_extra { get; set; }
		[Column("mark")]
		public string Mark { get; set; }
	}

}