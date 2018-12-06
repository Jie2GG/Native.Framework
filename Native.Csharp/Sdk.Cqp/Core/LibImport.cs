using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Native.Csharp.Sdk.Cqp.Core
{
	public static class LibImport
	{
		#region --CqpApi--
		[DllImport(Common.DllName, EntryPoint = "CQ_sendPrivateMsg")]
		public static extern int CQ_sendPrivateMsg(int authCode, long qqId, string msg);

		[DllImport(Common.DllName, EntryPoint = "CQ_sendGroupMsg")]
		public static extern int CQ_sendGroupMsg(int authCode, long groupId, string msg);

		[DllImport(Common.DllName, EntryPoint = "CQ_sendDiscussMsg")]
		public static extern int CQ_sendDiscussMsg(int authCode, long discussId, string msg);

		[DllImport(Common.DllName, EntryPoint = "CQ_deleteMsg")]
		public static extern int CQ_deleteMsg(int authCode, long msgId);

		[Obsolete("请调用 CQ_sendLikeV2")]
		[DllImport(Common.DllName, EntryPoint = "CQ_sendLike")]
		public static extern int CQ_sendLike(int authCode, long qqId);

		[DllImport(Common.DllName, EntryPoint = "CQ_sendLikeV2")]
		public static extern int CQ_sendLikeV2(int authCode, long qqId, int times);

		[DllImport(Common.DllName, EntryPoint = "CQ_getCookies")]
		public static extern string CQ_getCookies(int authCode);

		[DllImport(Common.DllName, EntryPoint = "CQ_getRecord")]
		public static extern string CQ_getRecord(int authCode, string file, string format);

		[DllImport(Common.DllName, EntryPoint = "CQ_getCsrfToken")]
		public static extern int CQ_getCsrfToken(int authCode);

		[DllImport(Common.DllName, EntryPoint = "CQ_getAppDirectory")]
		public static extern string CQ_getAppDirectory(int authCode);

		[DllImport(Common.DllName, EntryPoint = "CQ_getLoginQQ")]
		public static extern long CQ_getLoginQQ(int authCode);

		[DllImport(Common.DllName, EntryPoint = "CQ_getLoginNick")]
		public static extern string CQ_getLoginNick(int authCode);

		[DllImport(Common.DllName, EntryPoint = "CQ_setGroupKick")]
		public static extern int CQ_setGroupKick(int authCode, long groupId, long qqId, bool refuses);

		[DllImport(Common.DllName, EntryPoint = "CQ_setGroupBan")]
		public static extern int CQ_setGroupBan(int authCode, long groupId, long qqId, long time);

		[DllImport(Common.DllName, EntryPoint = "CQ_setGroupAdmin")]
		public static extern int CQ_setGroupAdmin(int authCode, long groupId, long qqId, bool isSet);

		[DllImport(Common.DllName, EntryPoint = "CQ_setGroupSpecialTitle")]
		public static extern int CQ_setGroupSpecialTitle(int authCode, long groupId, long qqId, string title, long durationTime);

		[DllImport(Common.DllName, EntryPoint = "CQ_setGroupWholeBan")]
		public static extern int CQ_setGroupWholeBan(int authCode, long groupId, bool isOpen);

		[DllImport(Common.DllName, EntryPoint = "CQ_setGroupAnonymousBan")]
		public static extern int CQ_setGroupAnonymousBan(int authCode, long groupId, string anonymous, long banTime);

		[DllImport(Common.DllName, EntryPoint = "CQ_setGroupAnonymous")]
		public static extern int CQ_setGroupAnonymous(int authCode, long groupId, bool isOpen);

		[DllImport(Common.DllName, EntryPoint = "CQ_setGroupCard")]
		public static extern int CQ_setGroupCard(int authCode, long groupId, long qqId, string newCard);

		[DllImport(Common.DllName, EntryPoint = "CQ_setGroupLeave")]
		public static extern int CQ_setGroupLeave(int authCode, long groupId, bool isDisband);

		[DllImport(Common.DllName, EntryPoint = "CQ_setDiscussLeave")]
		public static extern int CQ_setDiscussLeave(int authCode, long disscussId);

		[DllImport(Common.DllName, EntryPoint = "CQ_setFriendAddRequest")]
		public static extern int CQ_setFriendAddRequest(int authCode, string identifying, int requestType, string appendMsg);

		[Obsolete("请使用: CQ_setGroupAddRequestV2")]
		[DllImport(Common.DllName, EntryPoint = "CQ_setGroupAddRequest")]
		public static extern int CQ_setGroupAddRequest(int authCode, string identifying, int requestType, int responseType);

		[DllImport(Common.DllName, EntryPoint = "CQ_setGroupAddRequestV2")]
		public static extern int CQ_setGroupAddRequestV2(int authCode, string identifying, int requestType, int responseType, string appendMsg);

		[DllImport(Common.DllName, EntryPoint = "CQ_addLog")]
		public static extern int CQ_addLog(int authCode, int priority, string type, string Msg);

		[DllImport(Common.DllName, EntryPoint = "CQ_setFatal")]
		public static extern int CQ_setFatal(int authCode, string errorMsg);

		[DllImport(Common.DllName, EntryPoint = "CQ_getGroupMemberInfoV2")]
		public static extern string CQ_getGroupMemberInfoV2(int authCode, long groudId, long qqId, bool isCache);

		[DllImport(Common.DllName, EntryPoint = "CQ_getGroupMemberList")]
		public static extern string CQ_getGroupMemberList(int authCode, long groupId);

		[DllImport(Common.DllName, EntryPoint = "CQ_getGroupList")]
		public static extern string CQ_getGroupList(int authCode);

		[DllImport(Common.DllName, EntryPoint = "CQ_getStrangerInfo")]
		public static extern string CQ_getStrangerInfo(int authCode, long qqId, bool notCache);
		#endregion

		#region --SysApi--
		[DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
		public static extern void RtlMoveMemory(IntPtr lpDestination, IntPtr lpSource, int length);

		[DllImport("kernel32.dll", EntryPoint = "GlobalSize")]
		public static extern int GlobalSize(int hMem);

		//[DllImport("kernel32.dll")]
		//private extern static int GetPrivateProfileIntA(string segName, string keyName, int iDefault, string fileName);

		[DllImport("kernel32.dll")]
		public extern static int GetPrivateProfileStringA(string segName, string keyName, string sDefault, StringBuilder buffer, int nSize, string fileName);

		[DllImport("kernel32.dll")]
		public extern static int GetPrivateProfileSectionA(string segName, StringBuilder buffer, int nSize, string fileName);

		[DllImport("kernel32.dll")]
		public extern static int GetPrivateProfileSectionNamesA(byte[] buffer, int iLen, string fileName);

		[DllImport("kernel32.dll")]
		public extern static int WritePrivateProfileSectionA(string segName, string sValue, string fileName);

		[DllImport("kernel32.dll")]
		public extern static int WritePrivateProfileStringA(string segName, string keyName, string sValue, string fileName);
		#endregion
	}
}
