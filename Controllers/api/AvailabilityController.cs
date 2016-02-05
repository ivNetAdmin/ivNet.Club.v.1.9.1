
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using ivNet.Club.Models.View;
using ivNet.Club.Services;
using Orchard;
using Orchard.Logging;

namespace ivNet.Club.Controllers.api
{
    public class AvailabilityController : ApiController
    {
        private IFixtureServices _fixtureServices;
        private IOrchardServices _orchardServices;
        
        public AvailabilityController(IFixtureServices fixtureServices, IOrchardServices orchardServices)
        {
            _fixtureServices = fixtureServices;
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }


        [HttpGet]       
        public HttpResponseMessage Get()
        {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ivMemberTab))
                return Request.CreateResponse(HttpStatusCode.Forbidden);

            var availability = new AvailabilityModel();
            tidyCalendar(availability);
            _fixtureServices.AddPlayerAvailabilty(availability);
            return Request.CreateResponse(HttpStatusCode.OK, availability, new MediaTypeHeaderValue("application/json"));
        }

        [HttpGet]
        public HttpResponseMessage Get(int id)
        {
           
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPut]
        public HttpResponseMessage Put(int id, dynamic item)
        {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ivMemberTab))
                return Request.CreateResponse(HttpStatusCode.Forbidden);

            try
            {
                _fixtureServices.UpdatePlayerAvailabilty(id, 
                    (string) item.Month, 
                    (string) item.Date,
                    (bool) item.Available);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private void tidyCalendar(AvailabilityModel availability)
        {
            foreach (var month in availability.Months)
            {
                for (var i = 0; i < maxDays(availability); i++)
                {                    
                    if (month.Days.Count <= i)
                        month.Days.Add(new AvailabilityModel.Day{Date = string.Empty});
                }
            }
        }

        private int maxDays(AvailabilityModel availability)
        {
            return availability.Months.Select(month => month.Days.Count).Concat(new[] {0}).Max();
        }
    }
}