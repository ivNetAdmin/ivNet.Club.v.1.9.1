
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using AutoMapper;
using ivNet.Club.Models.View;
using ivNet.Club.Services;
using NHibernate.Mapping;
using Orchard;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Themes;

namespace ivNet.Club.Controllers
{
    public class ClubAdminController : BaseController
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IMemberServices _memberServices;

        public ClubAdminController(IOrchardServices orchardServices, IMemberServices memberServices)
        {
            _orchardServices = orchardServices;
            _memberServices = memberServices;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        [Themed]
        public ActionResult Configure()
        {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ivSiteAdmin, T("You are not authorized")))
                Response.Redirect("/Users/Account/AccessDenied?ReturnUrl=/club/admin/configure");

            return View("Admin/Configure/Index");
        }

        [Themed]
        public ActionResult UserStories()
        {

            if (!_orchardServices.Authorizer.Authorize(Permissions.ivSiteAdmin, T("You are not authorized")))
                Response.Redirect("/Users/Account/AccessDenied?ReturnUrl=/club/admin/user-stories");


            return View("Admin/UserStories/Index");
        }

        [Themed]
        public ActionResult Membership()
        {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ivMembershipAdmin, T("You are not authorized")))
                Response.Redirect("/Users/Account/AccessDenied?ReturnUrl=/club/admin/membership");

            return View("Admin/Membership/Index");
        }

        [Themed]
        public ActionResult Fixture()
        {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ivMembershipAdmin, T("You are not authorized")))
                Response.Redirect("/Users/Account/AccessDenied?ReturnUrl=/club/admin/fixture");

            return View("Admin/Fixture/Index");
        }

        [Themed]
        public ActionResult TeamSelection()
        {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ivMembershipAdmin, T("You are not authorized")))
                Response.Redirect("/Users/Account/AccessDenied?ReturnUrl=/club/admin/team-selection");

            return View("Admin/TeamSelection/Index");
        }

        [Themed]
        public ActionResult Registration()
        {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ivMembershipAdmin, T("You are not authorized")))
                Response.Redirect("/Users/Account/AccessDenied?ReturnUrl=/club/admin/registration");

            return View("Admin/Registration/Index");
        }


        public ActionResult SaveMember(ClubMemberModel model, FormCollection form)
        {

            model.Wards = new List<Ward>();
            foreach (var key in form.AllKeys)
            {
                if (key.IndexOf("ward", StringComparison.CurrentCultureIgnoreCase) != -1)
                {
                    UpdateWardList(model.Wards, form, key);
                }
            }

            _memberServices.SaveMember(model);

            return Redirect("/club/admin/membership");
        }

        private void UpdateWardList(List<Ward> wardList, FormCollection model, string key)
        {
            var wardCounter = Convert.ToInt32(key.Substring(key.IndexOf("_", StringComparison.CurrentCultureIgnoreCase) + 1));
            if (wardList.Count < wardCounter)
            {
                wardList.Add(new Ward());
            }

            var ward = wardList[wardCounter - 1];
            switch(key.Substring(0,key.IndexOf("_", StringComparison.CurrentCultureIgnoreCase)))
            {
                case "wardMemberId":
                    ward.MemberId = Convert.ToInt32(model[key]);
                    break;
                    case "wardLastname":
                    ward.Lastname = model[key];
                    break;
                    case "wardFirstname":
                    ward.Firstname = model[key];
                    break;
                    case "wardNickname":
                    ward.Nickname = model[key];
                    break;
                    case "wardDob":
                    ward.Dob = DateTime.Parse(model[key]);
                    break;
            }
        }
    }
}