using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Native.Csharp.Sdk.Cqp.Core
{
	public static class CQP
	{
		/* 
		 * 官方SDK更新日志:
		 * 
		 *	V9.20 (2019-3-3)
		 *	----------------
		 *
		 *	修改 接收语音 Api
		 *	 * 新版 SDK 中，该 Api 将返回本地**绝对**路径（旧版 SDK 是返回相对路径）。
		 *	   若此前用到了该 Api，需要对代码做相应调整。
		 *
		 *	新增 接收图片 Api
		 *	 * 可以使用该 Api，让酷Q直接下载收到的图片，并返回本地**绝对**路径。
		 *	   更新应用时，需要切换至该 Api，在未来获得更好的兼容性。
		 *
		 *	新增 是否支持发送图片() 及 是否支持发送语音() Api
		 *	 * 在开发图片及语音相关的应用时，可用该 Api 处理不支持对应功能的情况（如酷Q Air）。
		 */

		#region --常量--
		private const string DllName = "CQP.dll";
		#endregion

		#region --CqpApi--
		[DllImport (DllName, EntryPoint = "CQ_sendPrivateMsg")]
		public static extern int CQ_sendPrivateMsg (int authCode, long qqId, IntPtr msg);

		[DllImport (DllName, EntryPoint = "CQ_sendGroupMsg")]
		public static extern int CQ_sendGroupMsg (int authCode, long groupId, IntPtr msg);

		[DllImport (DllName, EntryPoint = "CQ_sendDiscussMsg")]
		public static extern int CQ_sendDiscussMsg (int authCode, long discussId, IntPtr msg);

		[DllImport (DllName, EntryPoint = "CQ_deleteMsg")]
		public static extern int CQ_deleteMsg (int authCode, long msgId);

		[DllImport (DllName, EntryPoint = "CQ_sendLikeV2")]
		public static extern int CQ_sendLikeV2 (int authCode, long qqId, int times);

		[DllImport (DllName, EntryPoint = "CQ_getCookies")]
		public static extern string CQ_getCookies (int authCode);

		[Obsolete ("请使用 CQ_getRecordV2 版本 API")]
		[DllImport (DllName, EntryPoint = "CQ_getRecord")]
		public static extern string CQ_getRecord (int authCode, string file, string format);

		[DllImport (DllName, EntryPoint = "CQ_getRecordV2")]
		public static extern string CQ_getRecordV2 (int authCode, string file, string format);

		[DllImport (DllName, EntryPoint = "CQ_getCsrfToken")]
		public static extern int CQ_getCsrfToken (int authCode);

		[DllImport (DllName, EntryPoint = "CQ_getAppDirectory")]
		public static extern string CQ_getAppDirectory (int authCode);

		[DllImport (DllName, EntryPoint = "CQ_getLoginQQ")]
		public static extern long CQ_getLoginQQ (int authCode);

		[DllImport (DllName, EntryPoint = "CQ_getLoginNick")]
		public static extern IntPtr CQ_getLoginNick (int authCode);

		[DllImport (DllName, EntryPoint = "CQ_setGroupKick")]
		public static extern int CQ_setGroupKick (int authCode, long groupId, long qqId, bool refuses);

		[DllImport (DllName, EntryPoint = "CQ_setGroupBan")]
		public static extern int CQ_setGroupBan (int authCode, long groupId, long qqId, long time);

		[DllImport (DllName, EntryPoint = "CQ_setGroupAdmin")]
		public static extern int CQ_setGroupAdmin (int authCode, long groupId, long qqId, bool isSet);

		[DllImport (DllName, EntryPoint = "CQ_setGroupSpecialTitle")]
		public static extern int CQ_setGroupSpecialTitle (int authCode, long groupId, long qqId, IntPtr title, long durationTime);

		[DllImport (DllName, EntryPoint = "CQ_setGroupWholeBan")]
		public static extern int CQ_setGroupWholeBan (int authCode, long groupId, bool isOpen);

		[DllImport (DllName, EntryPoint = "CQ_setGroupAnonymousBan")]
		public static extern int CQ_setGroupAnonymousBan (int authCode, long groupId, IntPtr anonymous, long banTime);

		[DllImport (DllName, EntryPoint = "CQ_setGroupAnonymous")]
		public static extern int CQ_setGroupAnonymous (int authCode, long groupId, bool isOpen);

		[DllImport (DllName, EntryPoint = "CQ_setGroupCard")]
		public static extern int CQ_setGroupCard (int authCode, long groupId, long qqId, IntPtr newCard);

		[DllImport (DllName, EntryPoint = "CQ_setGroupLeave")]
		public static extern int CQ_setGroupLeave (int authCode, long groupId, bool isDisband);

		[DllImport (DllName, EntryPoint = "CQ_setDiscussLeave")]
		public static extern int CQ_setDiscussLeave (int authCode, long disscussId);

		[DllImport (DllName, EntryPoint = "CQ_setFriendAddRequest")]
		public static extern int CQ_setFriendAddRequest (int authCode, string identifying, int requestType, IntPtr appendMsg);

		[DllImport (DllName, EntryPoint = "CQ_setGroupAddRequestV2")]
		public static extern int CQ_setGroupAddRequestV2 (int authCode, string identifying, int requestType, int responseType, IntPtr appendMsg);

		[DllImport (DllName, EntryPoint = "CQ_addLog")]
		public static extern int CQ_addLog (int authCode, int priority, string type, IntPtr Msg);

		[DllImport (DllName, EntryPoint = "CQ_setFatal")]
		public static extern int CQ_setFatal (int authCode, IntPtr errorMsg);

		[DllImport (DllName, EntryPoint = "CQ_getGroupMemberInfoV2")]
		public static extern string CQ_getGroupMemberInfoV2 (int authCode, long groudId, long qqId, bool isCache);

		[DllImport (DllName, EntryPoint = "CQ_getGroupMemberList")]
		public static extern string CQ_getGroupMemberList (int authCode, long groupId);

		[DllImport (DllName, EntryPoint = "CQ_getGroupList")]
		public static extern string CQ_getGroupList (int authCode);

		[DllImport (DllName, EntryPoint = "CQ_getStrangerInfo")]
		public static extern string CQ_getStrangerInfo (int authCode, long qqId, bool notCache);

		[DllImport (DllName, EntryPoint = "CQ_canSendImage")]
		public static extern int CQ_canSendImage (int authCode);

		[DllImport (DllName, EntryPoint = "CQ_canSendRecord")]
		public static extern int CQ_canSendRecord (int authCode);

		[DllImport(DllName, EntryPoint = "CQ_getImage")]
		public static extern string CQ_getImage (int authCode, string file);
		#endregion
	}
}
