
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ivNet.Club.Models.View;
using ivNet.Club.Services;
using Microsoft.SqlServer.Server;
using Orchard.Logging;
using Orchard.Security;

namespace ivNet.Club.Controllers.api
{
    public class MembersController : ApiController
    {
        private IMemberServices _memberServices;
        public MembersController(IMemberServices memberServices)
        {
            _memberServices = memberServices;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }


        [HttpGet]
        public HttpResponseMessage Get()
        {
            var paramCollection = Helpers.HttpRequestMessageExtensions.GetQueryStrings(Request);
            return Request.CreateResponse(HttpStatusCode.OK, _memberServices.GetMembers(paramCollection));
        }

        [HttpGet]
        //public HttpResponseMessage Get(int id, dynamic ward)
        public HttpResponseMessage Get(int id)
        {            
            return Request.CreateResponse(HttpStatusCode.OK, _memberServices.GetMember(id));
        }

        [HttpPost]
        public HttpResponseMessage Post(dynamic data)
        {
            var model = data.data;
            switch ((string) model.Type)
            {
                case "Ward":
                    return Request.CreateResponse(HttpStatusCode.OK,
                        _memberServices.GetWard((string) model.Lastname, (string) model.Firstname));
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        [HttpPut]
        public HttpResponseMessage Put(int id, dynamic item)
        {
            //if (!_orchardServices.Authorizer.Authorize(Permissions.ivAdminTab))
            //    return Request.CreateResponse(HttpStatusCode.Forbidden);

            try
            {
                switch ((string)item.Action)
                {
                    case "edit":
                        var model = MapClubMemberModel(item);
                        //_memberServices.SaveMember(model);
                        break;
                    case "activate":
                        _memberServices.Activate(id, (string)item.Type, true);
                        break;
                    case "deactivate":
                        _memberServices.Activate(id, (string)item.Type, false);
                        if (item.GuardianId != null)
                        {
                            _memberServices.RemoveWard((int) item.GuardianId, id);
                        }
                        break;
                    //case "delete":
                    //    _listingServices.DeleteListing(id);
                    //    break;
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private ClubMemberModel MapClubMemberModel(dynamic item)
        {
            if (item != null && item.Member != null)
            {

                var model = new ClubMemberModel
                {
                    MemberId = (int) item.Member.MemberId
                };
                return model;
            }
          
                return null;
        }
    }
}

