using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Native.Sdk.Cqp.Core
{
	internal static class LibImport
	{
		#region --CqpApi--
		[DllImport(Common.DllName, EntryPoint = "CQ_sendPrivateMsg")]
		internal static extern int CQ_sendPrivateMsg(int authCode, long qqId, string msg);

		[DllImport(Common.DllName, EntryPoint = "CQ_sendGroupMsg")]
		internal static extern int CQ_sendGroupMsg(int authCode, long groupId, string msg);

		[DllImport(Common.DllName, EntryPoint = "CQ_sendDiscussMsg")]
		internal static extern int CQ_sendDiscussMsg(int authCode, long discussId, string msg);

		[DllImport(Common.DllName, EntryPoint = "CQ_deleteMsg")]
		internal static extern int CQ_deleteMsg(int authCode, long msgId);

		[Obsolete("请调用 CQ_sendLikeV2")]
		[DllImport(Common.DllName, EntryPoint = "CQ_sendLike")]
		internal static extern int CQ_sendLike(int authCode, long qqId);

		[DllImport(Common.DllName, EntryPoint = "CQ_sendLikeV2")]
		internal static extern int CQ_sendLikeV2(int authCode, long qqId, int times);

		[DllImport(Common.DllName, EntryPoint = "CQ_getCookies")]
		internal static extern string CQ_getCookies(int authCode);

		[DllImport(Common.DllName, EntryPoint = "CQ_getRecord")]
		internal static extern string CQ_getRecord(int authCode, string file, string format);

		[DllImport(Common.DllName, EntryPoint = "CQ_getCsrfToken")]
		internal static extern int CQ_getCsrfToken(int authCode);

		[DllImport(Common.DllName, EntryPoint = "CQ_getAppDirectory")]
		internal static extern string CQ_getAppDirectory(int authCode);

		[DllImport(Common.DllName, EntryPoint = "CQ_getLoginQQ")]
		internal static extern long CQ_getLoginQQ(int authCode);

		[DllImport(Common.DllName, EntryPoint = "CQ_getLoginNick")]
		internal static extern string CQ_getLoginNick(int authCode);

		[DllImport(Common.DllName, EntryPoint = "CQ_setGroupKick")]
		internal static extern int CQ_setGroupKick(int authCode, long groupId, long qqId, bool refuses);

		[DllImport(Common.DllName, EntryPoint = "CQ_setGroupBan")]
		internal static extern int CQ_setGroupBan(int authCode, long groupId, long qqId, long time);

		[DllImport(Common.DllName, EntryPoint = "CQ_setGroupAdmin")]
		internal static extern int CQ_setGroupAdmin(int authCode, long groupId, long qqId, bool isSet);

		[DllImport(Common.DllName, EntryPoint = "CQ_setGroupSpecialTitle")]
		internal static extern int CQ_setGroupSpecialTitle(int authCode, long groupId, long qqId, string title, long durationTime);

		[DllImport(Common.DllName, EntryPoint = "CQ_setGroupWholeBan")]
		internal static extern int CQ_setGroupWholeBan(int authCode, long groupId, bool isOpen);

		[DllImport(Common.DllName, EntryPoint = "CQ_setGroupAnonymousBan")]
		internal static extern int CQ_setGroupAnonymousBan(int authCode, long groupId, string anonymous, long banTime);

		[DllImport(Common.DllName, EntryPoint = "CQ_setGroupAnonymous")]
		internal static extern int CQ_setGroupAnonymous(int authCode, long groupId, bool isOpen);

		[DllImport(Common.DllName, EntryPoint = "CQ_setGroupCard")]
		internal static extern int CQ_setGroupCard(int authCode, long groupId, long qqId, string newCard);

		[DllImport(Common.DllName, EntryPoint = "CQ_setGroupLeave")]
		internal static extern int CQ_setGroupLeave(int authCode, long groupId, bool isDisband);

		[DllImport(Common.DllName, EntryPoint = "CQ_setDiscussLeave")]
		internal static extern int CQ_setDiscussLeave(int authCode, long disscussId);

		[DllImport(Common.DllName, EntryPoint = "CQ_setFriendAddRequest")]
		internal static extern int CQ_setFriendAddRequest(int authCode, string identifying, int requestType, string appendMsg);

		[Obsolete("请使用: CQ_setGroupAddRequestV2")]
		[DllImport(Common.DllName, EntryPoint = "CQ_setGroupAddRequest")]
		internal static extern int CQ_setGroupAddRequest(int authCode, string identifying, int requestType, int responseType);

		[DllImport(Common.DllName, EntryPoint = "CQ_setGroupAddRequestV2")]
		internal static extern int CQ_setGroupAddRequestV2(int authCode, string identifying, int requestType, int responseType, string appendMsg);

		[DllImport(Common.DllName, EntryPoint = "CQ_addLog")]
		internal static extern int CQ_addLog(int authCode, int priority, string type, string Msg);

		[DllImport(Common.DllName, EntryPoint = "CQ_setFatal")]
		internal static extern int CQ_setFatal(int authCode, string errorMsg);

		[DllImport(Common.DllName, EntryPoint = "CQ_getGroupMemberInfoV2")]
		internal static extern string CQ_getGroupMemberInfoV2(int authCode, long groudId, long qqId, bool isCache);

		[DllImport(Common.DllName, EntryPoint = "CQ_getGroupMemberList")]
		internal static extern string CQ_getGroupMemberList(int authCode, long groupId);

		[DllImport(Common.DllName, EntryPoint = "CQ_getGroupList")]
		internal static extern string CQ_getGroupList(int authCode);

		[DllImport(Common.DllName, EntryPoint = "CQ_getStrangerInfo")]
		internal static extern string CQ_getStrangerInfo(int authCode, long qqId, bool notCache);
		#endregion

		#region --SysApi--
		[DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
		internal static extern void RtlMoveMemory(IntPtr lpDestination, IntPtr lpSource, int length);

		[DllImport("kernel32.dll", EntryPoint = "GlobalSize")]
		internal static extern int GlobalSize(int hMem);

		#region --Ini文件所使用的Api--
		//[DllImport("kernel32.dll")]
		//private extern static int GetPrivateProfileIntA(string segName, string keyName, int iDefault, string fileName);
		[DllImport("kernel32.dll")]
		internal extern static int GetPrivateProfileStringA(string segName, string keyName, string sDefault, StringBuilder buffer, int nSize, string fileName);
		[DllImport("kernel32.dll")]
		internal extern static int GetPrivateProfileSectionA(string segName, StringBuilder buffer, int nSize, string fileName);
		[DllImport("kernel32.dll")]
		internal extern static int GetPrivateProfileSectionNamesA(byte[] buffer, int iLen, string fileName);
		[DllImport("kernel32.dll")]
		internal extern static int WritePrivateProfileSectionA(string segName, string sValue, string fileName);
		[DllImport("kernel32.dll")]
		internal extern static int WritePrivateProfileStringA(string segName, string keyName, string sValue, string fileName);
		#endregion
		#endregion
	}
}
