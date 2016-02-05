
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ivNet.Club.Entities;
using ivNet.Club.Enums;
using ivNet.Club.Models.View;
using ivNet.Club.Services;
using Orchard.Logging;

namespace ivNet.Club.Controllers.api
{
    public class ConfigurationController : ApiController
    {
        private readonly IConfigServices _configServices;

        public ConfigurationController(IConfigServices configServices)
        {
            _configServices = configServices;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        [HttpGet]
        public HttpResponseMessage Get(string id)
        {
            switch (id)
            {
                case "member_type":
                    return Request.CreateResponse(HttpStatusCode.OK, _configServices.GetMemberTypes());
                case "team":
                    return Request.CreateResponse(HttpStatusCode.OK, _configServices.GetTeamNames(0));
                case "hometeam":
                    return Request.CreateResponse(HttpStatusCode.OK, _configServices.GetTeamNames(TeamType.Home));
                case "opposition":
                    return Request.CreateResponse(HttpStatusCode.OK, _configServices.GetTeamNames(TeamType.Opposition));
                case "season":
                    return Request.CreateResponse(HttpStatusCode.OK, _configServices.GetSeasons());
                case "stats_selector":
                    return Request.CreateResponse(HttpStatusCode.OK, _configServices.StatsSelectorItems());
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, "Configuration item does not exist");
        }

        [HttpPost]
        public HttpResponseMessage Post(ConfigItem item)
        {
            //  if (!_orchardServices.Authorizer.Authorize(Permissions.ivAdminTab))
            //      return Request.CreateResponse(HttpStatusCode.Forbidden);

            try
            {
                switch ((string) item.Type)
                {
                    case "member_type":
                        return Request.CreateResponse(HttpStatusCode.OK, _configServices.SaveMemberType(item));
                    case "team":
                        TeamName team = null;
                        return Request.CreateResponse(HttpStatusCode.OK, _configServices.SaveTeamName(item, out team));
                }

                return Request.CreateResponse(HttpStatusCode.BadRequest, "Configuration item does not exist");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, string.Empty, null);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [HttpDelete]
        public HttpResponseMessage Delete(int id, string type)
        {
            // if (!_orchardServices.Authorizer.Authorize(Permissions.ivAdminTab))
            //     return Request.CreateResponse(HttpStatusCode.Forbidden);

            try
            {
                switch (type)
                {
                    case "member_type":
                        return Request.CreateResponse(HttpStatusCode.OK, _configServices.DeleteMemberType(id));
                    case "team":
                        return Request.CreateResponse(HttpStatusCode.OK, _configServices.DeleteTeam(id));
                    case "hometeam":
                        return Request.CreateResponse(HttpStatusCode.OK, _configServices.DeleteTeam(id));
                    case "opposition":
                        return Request.CreateResponse(HttpStatusCode.OK, _configServices.DeleteTeam(id));
                }

                return Request.CreateResponse(HttpStatusCode.BadRequest, "Configuration item does not exist");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, string.Empty, null);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}



