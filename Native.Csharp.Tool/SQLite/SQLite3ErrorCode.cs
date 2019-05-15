using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Native.Csharp.Tool.SQLite
{
	/// <summary>
	/// SQLite3 错误列表枚举
	/// </summary>
	[DefaultValue (SQLITE_ERROR_OK)]
	public enum SQLite3ErrorCode
	{
		/// <summary>
		/// 操作成功，没有出现任何错误"
		/// </summary>
		[Description ("操作成功，没有出现任何错误")]
		SQLITE_ERROR_OK = 0,
		/// <summary>
		/// SQL数据库错误或丢失
		/// </summary>
		[Description ("SQL数据库错误或丢失")]
		SQLITE_ERROR_ERROR = 1,
		/// <summary>
		/// 一个SQLite内部的逻辑错误
		/// </summary>
		[Description ("一个SQLite内部的逻辑错误")]
		SQLITE_ERROR_INTERNAL = 2,
		/// <summary>
		/// 访问权限被拒绝
		/// </summary>
		[Description ("访问权限被拒绝")]
		SQLITE_ERROR_PERM = 3,
		/// <summary>
		/// 需要一个中断的Callback指令
		/// </summary>
		[Description ("需要一个中断的Callback指令")]
		SQLITE_ERROR_ABORT = 4,
		/// <summary>
		/// 数据库文件被锁定
		/// </summary>
		[Description ("数据库文件被锁定")]
		SQLITE_ERROR_BUSY = 5,
		/// <summary>
		/// 表被锁定
		/// </summary>
		[Description ("表被锁定")]
		SQLITE_ERROR_LOCKED = 6,
		/// <summary>
		/// 申请内存失败
		/// </summary>
		[Description ("申请内存失败")]
		SQLITE_ERROR_NOMEM = 7,
		/// <summary>
		/// 试图写一个只读的数据库
		/// </summary>
		[Description ("试图写一个只读的数据库")]
		SQLITE_ERROR_READONLY = 8,
		/// <summary>
		/// 操作被中断结束
		/// </summary>
		[Description ("操作被中断结束")]
		SQLITE_ERROR_INTERRUPT = 9,
		/// <summary>
		/// 某种磁盘I/O错误发生
		/// </summary>
		[Description ("某种磁盘I/O错误发生")]
		SQLITE_ERROR_IOERR = 10,
		/// <summary>
		/// 数据库磁盘镜像异常
		/// </summary>
		[Description ("数据库磁盘镜像异常")]
		SQLITE_ERROR_CORRUPT = 11,
		/// <summary>
		/// 表或记录不存在
		/// </summary>
		[Description ("表或记录不存在")]
		SQLITE_ERROR_NOTFOUND = 12,
		/// <summary>
		/// 数据库满，插入失败
		/// </summary>
		[Description ("数据库满，插入失败")]
		SQLITE_ERROR_FULL = 13,
		/// <summary>
		/// 不能打开数据库文件
		/// </summary>
		[Description ("不能打开数据库文件")]
		SQLITE_ERROR_CANTOPEN = 14,
		/// <summary>
		/// 公数据库错定协议错
		/// </summary>
		[Description ("公数据库错定协议错")]
		SQLITE_ERROR_PROTOCOL = 15,
		/// <summary>
		/// 数据库表为空
		/// </summary>
		[Description ("数据库表为空")]
		SQLITE_ERROR_EMPTY = 16,
		/// <summary>
		/// 数据库结构被改变
		/// </summary>
		[Description ("数据库结构被改变")]
		SQLITE_ERROR_SCHEMA = 17,
		/// <summary>
		/// 一个表的行数据过多
		/// </summary>
		[Description ("一个表的行数据过多")]
		SQLITE_ERROR_TOOBIG = 18,
		/// <summary>
		/// 由于约束冲突而中止
		/// </summary>
		[Description ("由于约束冲突而中止")]
		SQLITE_ERROR_CONSTRAINT = 19,
		/// <summary>
		/// 数据类型不匹配
		/// </summary>
		[Description ("数据类型不匹配")]
		SQLITE_ERROR_MISMATCH = 20,
		/// <summary>
		/// 库被不正确使用
		/// </summary>
		[Description ("库被不正确使用")]
		SQLITE_ERROR_MISUSE = 21,
		/// <summary>
		/// 主机不支持的OS特性
		/// </summary>
		[Description ("主机不支持的OS特性")]
		SQLITE_ERROR_NOLFS = 22,
		/// <summary>
		/// 授权被否定
		/// </summary>
		[Description ("授权被否定")]
		SQLITE_ERROR_AUTH = 23,
		/// <summary>
		/// 辅助数据库格式错误
		/// </summary>
		[Description ("辅助数据库格式错误")]
		SQLITE_ERROR_FORMAT = 24,
		/// <summary>
		/// 绑定参数时索引超出范围
		/// </summary>
		[Description ("绑定参数时索引超出范围")]
		SQLITE_ERROR_RANGE = 25,
		/// <summary>
		/// 文件已打开但没有数据库
		/// </summary>
		[Description ("文件已打开但没有数据库")]
		SQLITE_ERROR_NOTADB = 26,
		/// <summary>
		/// 有另一行就绪
		/// </summary>
		[Description ("有另一行就绪")]
		SQLITE_ERROR_ROW = 100,
		/// <summary>
		/// 已经完成执行
		/// </summary>
		[Description ("已经完成执行")]
		SQLITE_ERROR_DONE = 101
	}
}
