using Fleck;
using Native.Sdk.Cqp;
using Native.Sdk.Cqp.EventArgs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Code.Service
{
    public class CoolQWebSocket
    {
        private CQApi Api { get; set; }
        private CQLog Log { get; set; }
        private WebSocketServer Server { get; set; }

        public class Message
        {
            public string Action { get; set; }
            public object Params { get; set; }
        }

        public EventHandler<CQDiscussMessageEventArgs> CQDiscussMessage;
        public EventHandler<CQGroupMessageEventArgs> CQGroupMessageEventArgs;
        public EventHandler<CQPrivateMessageEventArgs> CQPrivateMessageEventArgs;

        private List<IWebSocketConnection> Clients = new List<IWebSocketConnection>();

        public CoolQWebSocket(CQApi api, CQLog log)
        {
            this.Api = api;
            this.Log = log;
            this.Server = new WebSocketServer($"ws://0.0.0.0:{Common.WebSocketPort}");
            this.Server.RestartAfterListenError = true;

            CQDiscussMessage = new EventHandler<CQDiscussMessageEventArgs>((s,e)=>
            {
                foreach (var conn in Clients)
                {
                    try
                    {
                        conn.Send(JsonConvert.SerializeObject(new
                        {
                            sub_type = e.SubType,
                            message_id = e.Id,
                            user_id = e.FromQQ.Id,
                            discuss_id = e.FromDiscuss.Id,
                            message = e.Message.ToSendString(),
                            raw_message = e.Message.Text
                        }));
                    }
                    catch (Exception ex) { log.Debug("WebSocket", ex); }
                }
            });
            CQGroupMessageEventArgs = new EventHandler<CQGroupMessageEventArgs>((s, e) =>
            {
                foreach (var conn in Clients)
                {
                    try
                    {
                        conn.Send(JsonConvert.SerializeObject(new {
                            sub_type = e.SubType,
                            message_id = e.Id,
                            user_id = e.FromQQ.Id,
                            group_id = e.FromGroup.Id,
                            anonymous = e.IsFromAnonymous ? e.FromAnonymous.Token : null,
                            message = e.Message.ToSendString(),
                            raw_message = e.Message.Text
                        }));
                    }
                    catch (Exception ex) { log.Debug("WebSocket", ex); }
                }
            });
            CQPrivateMessageEventArgs = new EventHandler<CQPrivateMessageEventArgs>((s, e) =>
            {
                foreach (var conn in Clients)
                {
                    try
                    {
                        conn.Send(JsonConvert.SerializeObject(new
                        {
                            sub_type = e.SubType,
                            message_id = e.Id,
                            user_id = e.FromQQ.Id,
                            message = e.Message.ToSendString(),
                            raw_message = e.Message.Text
                        }));
                    }
                    catch (Exception ex) { log.Debug("WebSocket", ex); }
                }
            });

            this.Server.Start(conn =>
            {
                if (conn.ConnectionInfo.Headers.ContainsKey("Authorization"))
                {
                    if (conn.ConnectionInfo.Headers["Authorization"] != $"Bearer {Common.WebSocketSecret}")
                    {
                        conn.Close();
                    }
                }
                else if (conn.ConnectionInfo.Path.Contains($"/api?access_token={Common.WebSocketSecret}") == false)
                {
                    conn.Close();
                }

                conn.OnOpen += ()=> Clients.Add(conn);
                conn.OnClose += () => Clients.Remove(conn);

                Log.Debug("WebSocket", $"新连接-{conn.ConnectionInfo.ClientIpAddress}");

                conn.OnMessage += (msg) =>
                {
                    try
                    {
                        JObject obj = JObject.Parse(msg, new JsonLoadSettings()
                        {
                            DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Replace,
                            LineInfoHandling = LineInfoHandling.Ignore
                        });

                        if (String.IsNullOrEmpty(obj.SelectToken("action")?.ToObject<string>()) == false)
                        {
                            switch (obj.SelectToken("action")?.ToObject<string>())
                            {
                                case "SendGroupMessage":
                                    SendGroupMessage(obj, conn);
                                    break;
                                case "SendPrivateMessage":
                                    SendPrivateMessage(obj, conn);
                                    break;
                                case "SendDiscussMessage":
                                    SendDiscussMessage(obj, conn);
                                    break;
                                case "GetStrangerInfo":
                                    GetStrangerInfo(obj, conn);
                                    break;
                                case "GetGroupMemberInfo":
                                    GetGroupMemberInfo(obj, conn);
                                    break;
                                case "GetGroupMemberList":
                                    GetGroupMemberList(obj, conn);
                                    break;
                                case "GetLoginQQ":
                                    conn.Send(Api.GetLoginQQ().Id.ToString());
                                    break;
                                case "GetLoginNick":
                                    conn.Send(Api.GetLoginNick());
                                    break;
                                case "GetFriendList":
                                    conn.Send(JsonConvert.SerializeObject(Api.GetFriendList()));
                                    break;
                                case "GetGroupList":
                                    conn.Send(JsonConvert.SerializeObject(Api.GetGroupList()));
                                    break;
                            }
                        }
                    }
                    catch { }
                };
            });
        }

        private void GetGroupMemberInfo(JObject obj, IWebSocketConnection conn)
        {
            var para = obj.SelectToken("params");
            if (para.HasValues)
            {
                if (para.SelectToken("user_id") != null && para.SelectToken("group_id") != null)
                {
                    var gminfo = Api.GetGroupMemberInfo(obj.SelectToken("group_id").ToObject<long>(), obj.SelectToken("user_id").ToObject<long>());
                    conn.Send(JsonConvert.SerializeObject(new { gminfo.Age, gminfo.Area, gminfo.Card, gminfo.IsAllowEditorCard, gminfo.IsBadRecord, gminfo.JoinGroupDateTime, gminfo.LastSpeakDateTime, gminfo.Level, gminfo.MemberType, gminfo.Nick, gminfo.Sex }));
                }
            }
        }

        private void GetGroupMemberList(JObject obj, IWebSocketConnection conn)
        {
            var para = obj.SelectToken("params");
            if (para.HasValues)
            {
                if (para.SelectToken("group_id") != null)
                {
                    var gml = Api.GetGroupMemberList(para.SelectToken("group_id").ToObject<long>());
                    conn.Send(JsonConvert.SerializeObject(gml));
                }
            }
        }

        private void GetStrangerInfo(JObject obj, IWebSocketConnection conn)
        {
            var para = obj.SelectToken("params");
            if (para.HasValues)
            {
                if (para.SelectToken("user_id") != null)
                {
                    var sinfo = Api.GetStrangerInfo(para.SelectToken("user_id").ToObject<long>(), para.SelectToken("no_cache")?.ToObject<bool>() ?? false);
                    conn.Send(JsonConvert.SerializeObject(new { sinfo.Age, sinfo.Nick, sinfo.QQ.Id, sinfo.Sex }));
                }
            }
        }

        private void SendGroupMessage(JObject obj, IWebSocketConnection conn)
        {
            var para = obj.SelectToken("params");
            if (para.HasValues)
            {
                if (para.SelectToken("group_id") != null)
                {
                    if (para.SelectToken("message") != null)
                    {
                        conn.Send(Api.SendGroupMessage(para.SelectToken("group_id").ToObject<long>(), para.SelectToken("message").ToObject<string>()).ToString());
                    }
                }
            }
        }

        private void SendPrivateMessage(JObject obj, IWebSocketConnection conn)
        {
            var para = obj.SelectToken("params");
            if (para.HasValues)
            {
                if (para.SelectToken("user_id")!=null)
                {
                    if (para.SelectToken("message") != null)
                    {
                        conn.Send(Api.SendPrivateMessage(para.SelectToken("user_id").ToObject<long>(), para.SelectToken("message").ToObject<string>()).ToString());
                    }
                }
            }
        }

        private void SendDiscussMessage(JObject obj, IWebSocketConnection conn)
        {
            var para = obj.SelectToken("params");
            if (para.HasValues)
            {
                if (para.SelectToken("discuss_id") != null)
                {
                    if (para.SelectToken("message") != null)
                    {
                        conn.Send(Api.SendDiscussMessage(para.SelectToken("discuss_id").ToObject<long>(), para.SelectToken("message").ToObject<string>()).ToString());
                    }
                }
            }
        }
    }
}
