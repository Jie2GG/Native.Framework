using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;
using Code.Action;
using Code.Helper;
using Code.Request;
using Native.Sdk.Cqp;
using Native.Sdk.Cqp.EventArgs;
using Native.Sdk.Cqp.Interface;
using Native.Sdk.Extension;
using UI.Model;

namespace Code
{
    public class Event_Message : IGroupMessage, IPrivateMessage, IDiscussMessage
    {
        public void DiscussMessage(object sender, CQDiscussMessageEventArgs e)
        {
            Common.CoolQWebSocket.CQDiscussMessage(sender, e);
        }
        public void PrivateMessage(object sender, CQPrivateMessageEventArgs e)
        {
            Common.CoolQWebSocket.CQPrivateMessageEventArgs(sender, e);

            if (e.SubType == Native.Sdk.Cqp.Enum.CQPrviateMessageType.Friend)
            {
                if (e.Message.Text.Contains("/重启"))
                {
                    User32.SendMessage(Process.GetCurrentProcess().ParentProcess().MainWindowHandle, 273, (int)Model.CQMenu.Reset , null);
                }
            }
        }

        /// <summary>
        /// 社区上反馈的问题
        /// </summary>
        private void Issue(CQGroupMessageEventArgs e)
        {
            /// 2020-01-14 群匿名禁言失败(-14) 
            if (e.IsFromAnonymous)
            {
                if (e.CQApi.SetGroupAnonymousMemberBanSpeak(e.FromGroup.Id, e.FromAnonymous, TimeSpan.FromMinutes(1)))
                {
                    e.CQLog.Debug("匿名禁言", "禁言失败");
                }
            }

            ///2020-01-11 取群成员性别错乱
            if (e.IsFromAnonymous == false)
            {
                e.CQLog.Debug("发言成员性别", e.CQApi.GetGroupMemberInfo(e.FromGroup, e.FromQQ).Sex.ToString());
            }

            ///2020-01-08 下载语音失败
            if (e.Message.CQCodes.Any(a => a.Function == Native.Sdk.Cqp.Enum.CQFunction.Record))
            {
                //下载消息中的语音
                e.CQLog.Debug("下载语音", ReceiveRecord.ReceiveRecordAsAMR(e.Message) ?? "下载失败");
            }

            ///2020-01-16 好友列表失败
            try
            {
                var friends = e.CQApi.GetFriendList();
                e.CQLog.Debug("Friends", String.Join(",", friends.Select(s => $"{s.Nick}({s.QQ})").ToList()) ?? "N/A");
            }
            catch { }
        }

        public void GroupMessage(object sender, CQGroupMessageEventArgs e)
        {
            Common.CoolQWebSocket.CQGroupMessageEventArgs(sender, e);

            //绑定数据同步
            BindingOperations.EnableCollectionSynchronization(ViewModel.MainInstance.GroupMessages, ViewModel.MainInstance.SyncLock);

            ViewModel.MainInstance.GroupMessages.Add(new Message()
            {
                Qq = e.FromQQ.Id,
                GroupId = e.FromGroup.Id,
                Content = e.Message.ToSendString(),
                DisplayName = e.IsFromAnonymous ? e.FromAnonymous.Name : e.CQApi.GetGroupMemberInfo(e.FromGroup, e.FromQQ).Nick,
                GroupName = e.CQApi.GetGroupInfo(e.FromGroup).Name,
            });

            //没有启用插件时,不处理其他服务
            if (Common.IsRunning == false) { return; }

            //社区上反馈的问题
            Issue(e);

            //判断收到的消息中是否有被艾特
            if (e.Message.CQCodes.Any(a => a.Function == Native.Sdk.Cqp.Enum.CQFunction.At && a.Items["qq"] == e.CQApi.GetLoginQQ().Id.ToString()) == false) { return; }

            //价值100亿AI的核心
            if (e.Message.Text.Contains("?") || e.Message.Text.Contains("？"))
            {
                //阻断事件，不再将事件传递给优先级更低的插件
                e.Handler = true;
                //将消息发送到群内
                e.CQApi.SendGroupMessage(e.FromGroup, e.Message.Text.Replace("?", "。").Replace("？", "。"));
            }

            //使用 "/撤回 {信息倒数条数}"
            if (e.Message.Text.Contains("/撤回"))
            {
                string digit = Regex.Match(e.Message.Text, @"\d+").Value;
                if (int.TryParse(digit, out int index))
                {
                    RemoveMessage.RemoveByGroupIdWithIndex(e.FromGroup.Id, index);
                }
            }

            //被艾特时,下载消息中的图片
            ReceiveImage.ReceiveAllImageFromMessage(e.Message);

            if (e.Message.Text.Contains("/好友列表"))
            {
                var list = Common.Friends.GetFrientList();
                if (list != null)
                {
                    foreach (var f in list)
                    {
                        e.CQLog.Debug("好友", $"[{f.GroupName}] - {f.NickName}(VIP{f.VipLevel})");
                    }
                    e.CQApi.SendGroupMessage(e.FromGroup.Id, String.Join(Environment.NewLine, list.Select(f => $"[{f.GroupName}] - {f.NickName}(VIP{f.VipLevel})")));
                }
            }

            if (e.Message.Text.Contains("/会员等级"))
            {
                var vip = Common.VipInfo.GetVipInfo(e.FromQQ.Id);
                if (vip != null)
                {
                    e.CQApi.SendGroupMessage(e.FromGroup.Id, $"{vip.VipLevel}({vip.GrowupTotal},{vip.GrowSpeed})");
                }
            }

            if (e.Message.Text.Contains("/头像"))
            {
                string iconFilePath = Common.Icon.SaveQqIcon(e.FromQQ.Id);
                if (String.IsNullOrEmpty(iconFilePath) == false)
                {
                    e.CQApi.SendGroupMessage(e.FromGroup.Id, CQApi.CQCode_Image(iconFilePath).ToSendString());
                }
            }

            if (e.Message.Text.Contains("/群公告"))
            {
                Common.Group.GetGroupNotice(e.FromGroup.Id);
            }

        }

    }
}
