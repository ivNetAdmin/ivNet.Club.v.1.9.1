
using System.Web.Mvc;
using Orchard;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Themes;

namespace ivNet.Club.Controllers
{
    public class ClubSiteController : BaseController
    {
        private readonly IOrchardServices _orchardServices;

        public ClubSiteController(IOrchardServices orchardServices)
        {
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        [Themed]
        public ActionResult Stats()
        {
            return View("Site/Stats/Index");
        }

        [Themed]
        public ActionResult Fixtures()
        {
            return View("Site/Fixtures/Index");
        }

        [Themed]
        public ActionResult Registration()
        {
            return View("Site/Registration/Index");
        }

        [Themed]
        public ActionResult MyDetails()
        {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ivMemberTab, T("You are not authorized")))
                Response.Redirect("/Users/Account/AccessDenied?ReturnUrl=/");

            return View("Site/Members/MyDetails/Index");
        }

        [Themed]
        public ActionResult MyAvailability()
        {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ivMemberTab, T("You are not authorized")))
                Response.Redirect("/Users/Account/AccessDenied?ReturnUrl=/");


            return View("Site/Members/MyAvailability/Index");
        }
    }
}