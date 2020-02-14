using Nancy;
using Nancy.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Module
{
    public class CoolQModule : NancyModule
    {
        public CoolQModule() : base("/cqapi")
        {
            Post["/SendGroupMessage"] = x =>
            {
                var para = this.Bind<ApiSendMessage>();

                int flag = Common.Api.SendGroupMessage(para.Id, para.Message);
                return this.Response.AsJson(flag, flag > 0 ? HttpStatusCode.OK : HttpStatusCode.NotFound);
            };

            Post["/SendPrivateMessage"] = x =>
            {
                var para = this.Bind<ApiSendMessage>();

                int flag = Common.Api.SendPrivateMessage(para.Id, para.Message);
                return this.Response.AsJson(flag, flag > 0 ? HttpStatusCode.OK : HttpStatusCode.NotFound);
            };

            Post["/SendDiscussMessage"] = x =>
            {
                var para = this.Bind<ApiSendMessage>();

                int flag = Common.Api.SendDiscussMessage(para.Id, para.Message);
                return this.Response.AsJson(flag, flag > 0 ? HttpStatusCode.OK : HttpStatusCode.NotFound);
            };

            Post["/GetStrangerInfo"] = x =>
            {
                var para = this.Bind<ApiGetStrangerInfo>();
                return this.Response.AsJson(Common.Api.GetStrangerInfo(para.QqId));
            };
            Post["/GetGroupMemberInfo"] = x =>
            {
                var para = this.Bind<ApiGetGroupMemberInfo>();
                return this.Response.AsJson(Common.Api.GetGroupMemberInfo(para.GroupId, para.QqId));
            };
            Post["/GetGroupMemberList"] = x =>
            {
                var para = this.Bind<ApiGetGroupMemberList>();
                return this.Response.AsJson(Common.Api.GetGroupMemberList(para.GroupId));
            };

            Get["/GetLoginQQ"] = x =>
            {
                return this.Response.AsJson(Common.Api.GetLoginQQ());
            };
            Get["/GetLoginQQId"] = x =>
            {
                return this.Response.AsJson(Common.Api.GetLoginQQId());
            };
            Get["/GetLoginNick"] = x =>
            {
                return this.Response.AsJson(Common.Api.GetLoginNick());
            };
            Get["/GetFriendList"] = x =>
            {
                return this.Response.AsJson(Common.Api.GetFriendList().Select(s=> new { s.Nick, s.QQ.Id , s.Postscript}));
            };
            Get["/GetGroupList"] = x =>
            {
                return this.Response.AsJson(Common.Api.GetGroupList().Select(s => new { s.Group.Id, s.Name, s.CurrentMemberCount, s.MaxMemberCount}));
            };

        }
    }

    public class ApiSendMessage
    {
        public long Id { get; set; }
        public string Message { get; set; }
    }

    public class ApiGetGroupMemberList
    {
        public long GroupId { get; set; }
    }

    public class ApiGetGroupMemberInfo
    {
        public long GroupId { get; set; }
        public long QqId { get; set; }
    }

    public class ApiGetStrangerInfo
    {
        public long QqId { get; set; }
    }

}
