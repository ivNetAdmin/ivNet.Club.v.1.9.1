
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ivNet.Club.Services;
using Orchard.Logging;

namespace ivNet.Club.Controllers.api
{
    public class StatsController : ApiController
    {
        private readonly IStatsServices _statsServices;

        public StatsController(IStatsServices statsServices)
        {
            _statsServices = statsServices;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        [HttpGet]
        public HttpResponseMessage Get()
        {
            var paramCollection = Helpers.HttpRequestMessageExtensions.GetQueryStrings(Request);
            return Request.CreateResponse(HttpStatusCode.OK, _statsServices.GetStats(paramCollection));
        }
    }
}