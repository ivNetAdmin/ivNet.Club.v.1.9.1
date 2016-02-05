
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using ivNet.Club.Models.View;
using Orchard;
using Orchard.Logging;

namespace ivNet.Club.Controllers.api
{
    public class PlayerController : ApiController
    {
        private IPlayerServices _playerServices;
        private IOrchardServices _orchardServices;

        public PlayerController(IPlayerServices playerServices,IOrchardServices orchardServices)
        {
            _orchardServices = orchardServices;
            _playerServices = playerServices;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        [HttpGet]
        public HttpResponseMessage Get()
        {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ivMemberTab))
                return Request.CreateResponse(HttpStatusCode.Forbidden);

            var paramCollection = Helpers.HttpRequestMessageExtensions.GetQueryStrings(Request);
            // call this now and put it in a session variable if called from admin page
            if (paramCollection.ContainsKey("type"))
            {
                return Request.CreateResponse(HttpStatusCode.OK, _playerServices.GetAvailablePlayers(paramCollection),
                    new MediaTypeHeaderValue("application/json"));
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, _playerServices.GetAllActivePlayers(),
                    new MediaTypeHeaderValue("application/json"));
            }
        }
    }
}