
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ivNet.Club.Services;
using Orchard.Logging;

namespace ivNet.Club.Controllers.api
{
    public class ReportsController : ApiController
    {
        private readonly IReportServices _reportServices;

        public ReportsController(IReportServices reportServices)
        {
            _reportServices = reportServices;           
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }


        [HttpGet]
        public HttpResponseMessage Get(int id)
        {
            return Request.CreateResponse(HttpStatusCode.OK, _reportServices.GetReport(id));
        }

        [HttpPut]
        public HttpResponseMessage Put(int id, dynamic report)
        {
            //if (!_orchardServices.Authorizer.Authorize(Permissions.ivAdminTab))
            //    return Request.CreateResponse(HttpStatusCode.Forbidden);

            _reportServices.UpdateReport(id, report);

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}