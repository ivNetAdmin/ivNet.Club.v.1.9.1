
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ivNet.Club.Services;
using Orchard.Logging;

namespace ivNet.Club.Controllers.api
{
    public class CarouselController : ApiController
    {
        private readonly IImageServices _imageServices;

        public CarouselController(IImageServices imageServices)
        {
            _imageServices = imageServices;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        [System.Web.Mvc.HttpGet]
        public HttpResponseMessage Get()
        {
            return Request.CreateResponse(HttpStatusCode.OK,
                _imageServices.GetCarouselImages());
        }
    }
}