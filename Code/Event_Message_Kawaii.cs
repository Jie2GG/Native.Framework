using Native.Sdk.Cqp;
using Native.Sdk.Cqp.EventArgs;
using Native.Sdk.Cqp.Interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code
{
    public class Event_Message_Kawaii : IGroupMessage
    {
        private List<string> messages = new List<string>()
        {
            "举高高",
            "我也很可爱呀",
        };

        private List<string> messages_luckyWithOjisan = new List<string>()
        {
            "{target}大叔抱了你一下,流下了青春的眼泪并放下¥{money}后就跑步离开了",
            "你从{target}大叔的钱包偷走¥{money}"
        };

        private List<string> messages_unluckyWithOjisan = new List<string>()
        {
            "大概这个世界已经没有大叔这种生物吧(損失了¥{money})",
            "{target}大叔发动(哔)[END](損失了¥{money})"
        };

        private List<string> messages_lucky = new List<string>()
        {
            "你拿出¥{money}向{target}买到一只O",
            "{target}听到$$的声音,被你捕获了"
        };

        private List<string> messages_unlucky = new List<string>()
        {
            "可惜了,你失去$${money}",
            "你失去$${money}"
        };

        private ConcurrentDictionary<GroupQq, long> Wallets = new ConcurrentDictionary<GroupQq, long>();

        public void GroupMessage(object sender, CQGroupMessageEventArgs e)
        {
            if (e.Message.IsRegexMessage)
            {
                string msg = string.Empty;
                string at = string.Empty;
                string money = string.Empty;
                string type = string.Empty;
                int _money = 0;

                e.Message.RegexKeyValuePairs.TryGetValue("msg", out msg);
                e.Message.RegexKeyValuePairs.TryGetValue("at", out at);
                e.Message.RegexKeyValuePairs.TryGetValue("money", out money);
                e.Message.RegexKeyValuePairs.TryGetValue("type", out type);
                int.TryParse(money, out _money);
                _money = Math.Max(_money, 0);

                if ((at ?? money ?? type) == "")
                {
                    //随机一个回覆并延时
                    e.CQApi.SendGroupMessage(e.FromGroup , RandomAndDelayWithWalletHandler(messages, e, null));
                    return;
                }
                else if (at == null)
                {
                    //拿出你的钱钱
                    if (new Random().Next(1000) % Enumerable.Range(1, 20).OrderBy(x => Guid.NewGuid()).Take(1).Single() == 1)
                    {
                        e.CQApi.SendGroupMessage(e.FromGroup.Id, RandomAndDelayWithWalletHandler(messages_lucky, e, false, _money));
                    }
                    else
                    {
                        e.CQApi.SendGroupMessage(e.FromGroup.Id, RandomAndDelayWithWalletHandler(messages_unlucky, e, true, _money));
                    }
                    return;
                }
                else
                {
                    //主控查询
                    if (at.Trim() == CQApi.CQCode_At(e.CQApi.GetLoginQQId()).ToSendString())
                    {
                        if (type.Contains("$"))
                        {
                            //我的钱钱数量
                            long myWallet = 0;
                            Wallets.TryGetValue(new GroupQq { groupId = e.FromGroup.Id, qqId = e.FromQQ.Id }, out myWallet);
                            e.CQApi.SendGroupMessage(e.FromGroup.Id, $"¥{myWallet}");
                            return;
                        }
                        if (type.Contains("top"))
                        {
                            //取出首十名
                            StringBuilder sb = new StringBuilder();
                            int no = 1;

                            Wallets?.Where(w => (w.Key.groupId == e.FromGroup.Id) && (w.Key.qqId == e.FromQQ.Id))
                                ?.OrderByDescending(o => o.Value)
                                ?.Take(10)
                                ?.ToList()
                                ?.ForEach(
                                    f =>
                                    {
                                        sb.Append($"{no}.{f.Key.qqId}\t{f.Value}");
                                        no++;
                                    });

                            if (String.IsNullOrEmpty(sb.ToString()) == true) { return; }
                            e.CQApi.SendGroupMessage(e.FromGroup.Id, sb.ToString());
                        }
                        return;
                    }
                    //向大叔拿钱钱
                    if (new Random().Next(Math.Max(1000000, 1000 * _money)) % Enumerable.Range(1, 20).OrderBy(x => Guid.NewGuid()).Take(1).Single() == 1)
                    {
                        e.CQApi.SendGroupMessage(e.FromGroup.Id, RandomAndDelayWithWalletHandler(messages_luckyWithOjisan, e, false, _money));
                    }
                    else
                    {
                        e.CQApi.SendGroupMessage(e.FromGroup.Id, RandomAndDelayWithWalletHandler(messages_unluckyWithOjisan, e, true, _money));
                    }
                }
            }
        }


        private string RandomAndDelayWithWalletHandler(List<string> msg, CQGroupMessageEventArgs e, Nullable<bool> byebyeMoney, long money = 0)
        {
            string outMessage = msg?.OrderBy(x => Guid.NewGuid()).Take(1).Single();
            Task.Delay(TimeSpan.FromMilliseconds(Enumerable.Range(10, 1000).OrderBy(x => Guid.NewGuid()).Take(1).Single() * outMessage.Length)).Wait();
            string strTarget = string.Empty;

            if (byebyeMoney != null)
            {
                if (money == 0)
                {
                    money = new Random().Next(10000);
                }

                var members = e.CQApi.GetGroupMemberList(e.FromGroup);

                if (members != null && members.Count > 0)
                {
                    strTarget = CQApi.CQCode_At(members.OrderBy(x => Guid.NewGuid()).Take(1).Single().QQ.Id).ToSendString();
                }

                if (byebyeMoney.Value == true)
                {
                    money *= -1;
                }
                long current = 0;
                Wallets.TryGetValue(new GroupQq { groupId = e.FromGroup.Id, qqId = e.FromQQ.Id }, out current);
                current += money;
                Wallets.AddOrUpdate(new GroupQq { groupId = e.FromGroup.Id, qqId = e.FromQQ.Id }, Math.Max(0, current), (a, b) => Math.Max(0, current));
            }

            return outMessage.Replace("{target}", strTarget).Replace("{money}", Math.Abs(money).ToString());
        }

        public class GroupQq
        {
            public long groupId { get; set; }
            public long qqId { get; set; }
        }

    }
}
