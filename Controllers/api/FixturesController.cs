
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ivNet.Club.Services;
using Orchard.Logging;

namespace ivNet.Club.Controllers.api
{
    public class FixturesController : ApiController
    {
        private readonly IFixtureServices _fixtureServices;
        private readonly IReportServices _reportServices;
        private readonly IPlayerServices _playerServices;

        public FixturesController(IFixtureServices fixtureServices, 
            IReportServices reportServices,
            IPlayerServices playerServices)
        {
            _fixtureServices = fixtureServices;
            _playerServices = playerServices;
            _reportServices = reportServices;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        [HttpGet]
        public HttpResponseMessage Get()
        {
            var paramCollection = Helpers.HttpRequestMessageExtensions.GetQueryStrings(Request);
            // call this now and put it in a session variable if called from admin page
            if (paramCollection.ContainsKey("admin"))
            {
                _playerServices.GetAllActivePlayers();
            }
            return Request.CreateResponse(HttpStatusCode.OK, _fixtureServices.GetFixtures(paramCollection));
        }

        [HttpGet]
        public HttpResponseMessage Get(int id)
        {
            return Request.CreateResponse(HttpStatusCode.OK, _fixtureServices.GetFixture(id));
        }

        [HttpDelete]
        public HttpResponseMessage Delete(int id)
        {
            try
            {
                _fixtureServices.DeleteFixture(id);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            
        }


        [HttpPut]
        public HttpResponseMessage Put(int id, dynamic fixtureStats)
        {
            //if (!_orchardServices.Authorizer.Authorize(Permissions.ivAdminTab))
            //    return Request.CreateResponse(HttpStatusCode.Forbidden);

            if ((string) fixtureStats["Type"] == "stat")
            {
                _fixtureServices.UpdateFixtureStats(id, fixtureStats);
            }
            else
            {
                _reportServices.UpdateReport(id, (string)fixtureStats["Report"]);
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}