
using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using ivNet.Club.Enums;
using ivNet.Club.Models.View;
using ivNet.Club.Services;
using Orchard.Logging;

namespace ivNet.Club.Controllers.api
{
    public class RegistrationController : ApiController
    {
       private readonly IMemberServices _memberServices;
       public RegistrationController(IMemberServices memberServices)
        {
            _memberServices = memberServices;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }


        [HttpGet]
        public HttpResponseMessage Get()
        {           
            return Request.CreateResponse(HttpStatusCode.OK, _memberServices.GetNewRegistrations());
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
                    case "vet":                        
                        _memberServices.CreateWebAccount(id);
                        break;
                
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage Post(Registration dataModel)
        {
            //if (!_orchardServices.Authorizer.Authorize(Permissions.ivAdminTab))
            //    return Request.CreateResponse(HttpStatusCode.Forbidden);

            if (CheckMemberExits(dataModel.Firstname, dataModel.Lastname))
            {
                return Request.CreateResponse(
                    HttpStatusCode.Conflict,
                    string.Format("Member {0} {1} is already registered", dataModel.Firstname, dataModel.Lastname));
            }

            var member = Mapper.Map(dataModel, new ClubMemberModel());

            if (dataModel.Roles.Guardian)
            {
                member.MemberTypList.Add("Guardian");
            }
            if (dataModel.Roles.Player)
            {
                member.MemberTypList.Add("Player");
                member.PlayerType = PlayerType.Senior.ToString();
            }
            if (dataModel.Roles.Officer)
            {
                member.MemberTypList.Add("Officer");
            }

            foreach (var ward in dataModel.Wards)
            {
                if (CheckMemberExits(ward.Firstname, ward.Lastname))
                {
                    return Request.CreateResponse(
                        HttpStatusCode.Conflict,
                        string.Format("Junior {0} {1} is already registered", ward.Firstname, ward.Lastname));
                }              
            }
         
            return Request.CreateResponse(HttpStatusCode.OK, _memberServices.SaveMember(member));
        }

        private bool CheckMemberExits(string firstname, string lastname)
        {
            return _memberServices.GetMemberByName(firstname, lastname) != null;
        }
    }
}