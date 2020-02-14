using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
	[Table("group_member_list")]
	public class CQDBGroup_member_list
	{
		[PrimaryKey, AutoIncrement, Column("id")]
		public int Id { get; set; }
		[Column("gid")]
		public long GroupId { get; set; }
		[Column("qq")]
		public long Qq { get; set; }
		[Column("info")]
		public byte[] Info { get; set; }
		[Column("full")]
		public int Full { get; set; }
		[Column("addtime")]
		public int Addtime { get; set; }
	}

	[Table("group_member_list_addtime")]
	public class CQDBGroup_member_list_addtime
	{
		[PrimaryKey, AutoIncrement, Column("id")]
		public int Id { get; set; }
		[Column("gid")]
		public long GroupId { get; set; }
		[Column("addtime")]
		public int Addtime { get; set; }
	}

	[Table("msg_group")]
	public class CQDBMsg_group
	{
		[PrimaryKey, AutoIncrement, Column("id")]
		public int Id { get; set; }
		[Column("qq")]
		public long Qq { get; set; }
		[Column("type")]
		public int Type { get; set; }
		[Column("groupid")]
		public long GroupId { get; set; }
		[Column("addtime")]
		public int Addtime { get; set; }
		[Column("inf")]
		public int Inf { get; set; }
	}

	[Table("msg_temp_key")]
	public class CQDBMsg_temp_key
	{
		[PrimaryKey, AutoIncrement, Column("id")]
		public int Id { get; set; }
		[Column("qq")]
		public long Qq { get; set; }
		[Column("chatkey")]
		public bool Chatkey { get; set; }
		[Column("addtime")]
		public int Addtime { get; set; }
	}

	[Table("stranger")]
	public class CQDBStranger
	{
		[PrimaryKey, AutoIncrement, Column("id")]
		public int Id { get; set; }
		[Column("qq")]
		public long Qq { get; set; }
		[Column("nick")]
		public string Nick { get; set; }
		[Column("gender")]
		public int Gender { get; set; }
		[Column("age")]
		public int Age { get; set; }
		[Column("addtime")]
		public int Addtime { get; set; }
	}

	[Table("group_member")]
	public class CQDBGroup_member
	{
		[PrimaryKey, AutoIncrement, Column("id")]
		public int Id { get; set; }
		[Column("gid")]
		public long GroupId { get; set; }
		[Column("qq")]
		public long Qq { get; set; }
		[Column("info")]
		public bool Info { get; set; }
		[Column("full")]
		public int Full { get; set; }
		[Column("addtime")]
		public int Addtime { get; set; }
	}

	[Table("version")]
	public class CQDBVersion
	{
		[Column("v")]
		public int V { get; set; }
	}

}