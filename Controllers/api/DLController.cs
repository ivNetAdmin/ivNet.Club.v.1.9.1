

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Xml;
using ivNet.Club.Helpers;
using ivNet.Club.Models.DataLoad;
using ivNet.Club.Services;
using NHibernate.Mapping;
using Orchard.Logging;

namespace ivNet.Club.Controllers.api
{
    public class DLController : ApiController
    {
        private readonly IDataLoadServices _dataLoadServices;

        public DLController(
            IDataLoadServices dataLoadServices)
        {
            _dataLoadServices = dataLoadServices;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        [HttpGet]
        public HttpResponseMessage Get(string id)
        {
            try
            {
                switch (id)
                {
                    case "members":
                        return LoadMembers();
                      
                    case "players":
                        return LoadPlayers();
                       
                    case "guardians":
                        return LoadGuardians();
                       
                    case "guardians-juniors":
                        return LoadGuardianJuniors();
                      
                    case "fixtures":
                        return LoadFixtures();

                    case "stats":
                        return LoadStats();

                    case "addresses":
                        return LoadAddresses();
                 
                    case "contact-details":
                        return LoadContactDetails();

                    case "reports":
                        return LoadReports();
                     
                   }

                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, string.Empty, null);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        private HttpResponseMessage LoadContactDetails()
        {
            var bwccXmlDoc = new XmlDocument();
            bwccXmlDoc.Load(
                         HttpContext.Current.Server.MapPath(
                             "~/Modules/ivNet.Club/App_Data/MySql/contact.details.xml"));

            var contactDetailsNodes = bwccXmlDoc.DocumentElement.SelectNodes("row");

            var contactDetailsList = BuildContactDetailsList(contactDetailsNodes);

            return Request.CreateResponse(HttpStatusCode.OK, _dataLoadServices.AddContactDetails(contactDetailsList));

        }

        private HttpResponseMessage LoadStats()
        {
            var bwccXmlDoc = new XmlDocument();
            bwccXmlDoc.Load(
                         HttpContext.Current.Server.MapPath(
                             "~/Modules/ivNet.Club/App_Data/MySql/stats.xml"));

            var statsNodes = bwccXmlDoc.DocumentElement.SelectNodes("row");

            var statsList = BuildStatsList(statsNodes);

            return Request.CreateResponse(HttpStatusCode.OK, _dataLoadServices.AddStats(statsList));

        }

        private HttpResponseMessage LoadAddresses()
        {
            var bwccXmlDoc = new XmlDocument();
            bwccXmlDoc.Load(
                         HttpContext.Current.Server.MapPath(
                             "~/Modules/ivNet.Club/App_Data/MySql/addresses.xml"));

            var addressNodes = bwccXmlDoc.DocumentElement.SelectNodes("row");

            var addressList = BuildAddressList(addressNodes);

            return Request.CreateResponse(HttpStatusCode.OK, _dataLoadServices.AddAddresses(addressList));

        }

        private HttpResponseMessage LoadReports()
        {
            var bwccXmlDoc = new XmlDocument();
            bwccXmlDoc.Load(
                         HttpContext.Current.Server.MapPath(
                             "~/Modules/ivNet.Club/App_Data/MySql/reports.xml"));

            var reportNodes = bwccXmlDoc.DocumentElement.SelectNodes("row");

            var reportList = BuildReportList(reportNodes);

            return Request.CreateResponse(HttpStatusCode.OK, _dataLoadServices.AddReports(reportList));

        }
        

        private HttpResponseMessage LoadFixtures()
        {
            var bwccXmlDoc = new XmlDocument();
            bwccXmlDoc.Load(
                          HttpContext.Current.Server.MapPath(
                              "~/Modules/ivNet.Club/App_Data/MySql/fixtures.xml"));

            var fixtureNodes = bwccXmlDoc.DocumentElement.SelectNodes("row");

            var fixtureList = BuildFixtureList(fixtureNodes);

            return Request.CreateResponse(HttpStatusCode.OK, _dataLoadServices.AddFixtures(fixtureList));               
                
        }

        private HttpResponseMessage LoadGuardianJuniors()
        {
            var bwccXmlDoc = new XmlDocument();
            bwccXmlDoc.Load(
                          HttpContext.Current.Server.MapPath(
                              "~/Modules/ivNet.Club/App_Data/MySql/guardian.junior.xml"));

            var guardianjuniorNodes = bwccXmlDoc.DocumentElement.SelectNodes("row");

            var guardianjuniorList = BuildGuardianJuniorList(guardianjuniorNodes);

            return Request.CreateResponse(HttpStatusCode.OK, _dataLoadServices.AddGuardiansJuniors(guardianjuniorList));

        }

        private HttpResponseMessage LoadGuardians()
        {
            var bwccXmlDoc = new XmlDocument();
            bwccXmlDoc.Load(
                           HttpContext.Current.Server.MapPath(
                               "~/Modules/ivNet.Club/App_Data/MySql/guardians.xml"));

            var guardianNodes = bwccXmlDoc.DocumentElement.SelectNodes("row");

            var guardianList = BuildGuardianList(guardianNodes);

            return Request.CreateResponse(HttpStatusCode.OK, _dataLoadServices.AddGuardians(guardianList));

        }

        private HttpResponseMessage LoadPlayers()
        {
            var bwccXmlDoc = new XmlDocument();
            bwccXmlDoc.Load(
                           HttpContext.Current.Server.MapPath(
                               "~/Modules/ivNet.Club/App_Data/MySql/players.xml"));

            var playerNodes = bwccXmlDoc.DocumentElement.SelectNodes("row");

            var playerList = BuildPlayerList(playerNodes);

            return Request.CreateResponse(HttpStatusCode.OK, _dataLoadServices.AddPlayers(playerList));

        }

        private HttpResponseMessage LoadMembers()
        {
            var bwccXmlDoc = new XmlDocument();
            bwccXmlDoc.Load(
                          HttpContext.Current.Server.MapPath(
                              "~/Modules/ivNet.Club/App_Data/MySql/members.xml"));

            var memberNodes = bwccXmlDoc.DocumentElement.SelectNodes("row");

            var memberList = BuildMemberList(memberNodes);

            return Request.CreateResponse(HttpStatusCode.OK, _dataLoadServices.AddMembers(memberList));

        }

        private List<DLStat> BuildStatsList(XmlNodeList statsNodes)
        {
            //var rtnList = new List<DLStat>();
            //foreach (XmlNode statsNode in statsNodes)
            //{
            //    try
            //    {
            //        var dlStat = new DLStat();
            //        dlStat.LegacyFixtureId =
            //            Convert.ToInt32(statsNode.SelectSingleNode("legacyfixtureid").InnerText);
            //        dlStat.LegacyMemberId =
            //            Convert.ToInt32(statsNode.SelectSingleNode("legacymemberid").InnerText);
            //        dlStat.DatePlayed =
            //            DateTime.Parse(statsNode.SelectSingleNode("dateplayed").InnerText);
            //        dlStat.RunsScored =
            //            Convert.ToInt32(statsNode.SelectSingleNode("runsscored").InnerText);
            //        dlStat.Position =
            //            Convert.ToInt32(statsNode.SelectSingleNode("battingposition").InnerText);
            //        dlStat.OversBowled =
            //            Convert.ToDecimal(statsNode.SelectSingleNode("oversbowled").InnerText);
            //        dlStat.Maidens =
            //            Convert.ToInt32(statsNode.SelectSingleNode("maidenovers").InnerText);
            //        dlStat.RunsConceeded =
            //            Convert.ToInt32(statsNode.SelectSingleNode("runsconceeded").InnerText);
            //        dlStat.Wickets =
            //            Convert.ToInt32(statsNode.SelectSingleNode("wicketstaken").InnerText);
            //        dlStat.Catches =
            //            Convert.ToInt32(statsNode.SelectSingleNode("catches").InnerText);
            //        dlStat.Stumpings =
            //            Convert.ToInt32(statsNode.SelectSingleNode("stumpings").InnerText);
            //        dlStat.MemberKey =
            //            CustomStringHelper.BuildKey(new[]
            //            {
            //                statsNode.SelectSingleNode("lastname").InnerText,
            //                statsNode.SelectSingleNode("firstname").InnerText
            //            });
            //        dlStat.FixtureKey = CustomStringHelper.BuildKey(new[]
            //        {
            //            statsNode.SelectSingleNode("teamname").InnerText,
            //            DateTime.Parse(statsNode.SelectSingleNode("dateplayed").InnerText).ToShortDateString()
            //        });

            //        rtnList.Add(dlStat);
            //    }
            //    catch (Exception ex)
            //    {
            //        var cakes = ex;
            //    }
            //}

            //return rtnList;
            return (from XmlNode statNode in statsNodes
                select new DLStat
                {
                    LegacyFixtureId =
                        Convert.ToInt32(statNode.SelectSingleNode("legacyfixtureid").InnerText),
                    LegacyMemberId =
                        Convert.ToInt32(statNode.SelectSingleNode("legacymemberid").InnerText),
                    DatePlayed =
                        DateTime.Parse(statNode.SelectSingleNode("dateplayed").InnerText),
                    RunsScored =
                        Convert.ToInt32(statNode.SelectSingleNode("runsscored").InnerText),
                    Position =
                        Convert.ToInt32(statNode.SelectSingleNode("battingposition").InnerText),
                    HowOut =
                        statNode.SelectSingleNode("howout").InnerText,
                    OversBowled =
                        Convert.ToDecimal(statNode.SelectSingleNode("oversbowled").InnerText),
                    Maidens =
                        Convert.ToInt32(statNode.SelectSingleNode("maidenovers").InnerText),
                    RunsConceeded =
                        Convert.ToInt32(statNode.SelectSingleNode("runsconceeded").InnerText),
                    Wickets =
                        Convert.ToInt32(statNode.SelectSingleNode("wicketstaken").InnerText),
                    Catches =
                        Convert.ToInt32(statNode.SelectSingleNode("catches").InnerText),
                    Stumpings =
                        Convert.ToInt32(statNode.SelectSingleNode("stumpings").InnerText),
                    MemberKey =
                        CustomStringHelper.BuildKey(new[]
                        {
                            statNode.SelectSingleNode("lastname").InnerText,
                            statNode.SelectSingleNode("firstname").InnerText
                        }),
                    FixtureKey = CustomStringHelper.BuildKey(new[]
                    {
                        statNode.SelectSingleNode("teamname").InnerText,
                        DateTime.Parse(statNode.SelectSingleNode("dateplayed").InnerText).ToShortDateString()
                    })
                }).ToList();
        }

        private List<DLFixture> BuildFixtureList(XmlNodeList fixtureNodes)
        {
            return (from XmlNode fixtureNode in fixtureNodes
                select new DLFixture
                {
                    LegacyId =
                        Convert.ToInt32(fixtureNode.SelectSingleNode("legacyid").InnerText),
                    DatePlayed =
                        DateTime.Parse(fixtureNode.SelectSingleNode("dateplayed").InnerText),
                    ResultType =
                        fixtureNode.SelectSingleNode("resulttype").InnerText == "NULL"
                            ? "Unknown"
                            : fixtureNode.SelectSingleNode("resulttype").InnerText,
                    Team =
                        fixtureNode.SelectSingleNode("team").InnerText,
                    Opposition =
                        fixtureNode.SelectSingleNode("opposition").InnerText == "NULL"
                            ? "Unknown"
                            : fixtureNode.SelectSingleNode("opposition").InnerText,
                    Result =
                        fixtureNode.SelectSingleNode("result").InnerText,
                    Venue =
                        GetVenue(fixtureNode.SelectSingleNode("venueid").InnerText),
                    Competition =
                        GetCompetition(fixtureNode.SelectSingleNode("competitionid").InnerText)
                }).ToList();
        }

        private string GetCompetition(string competitionId)
        {
            switch (competitionId)
            {
                case "1":
                    return "Friendly";
                case "2":
                    return "League";
                case "3":
                    return "Cup";
                case "4":
                    return "6-A-Side";
                default:
                    return "Unknown";
            }
        }

        private string GetVenue(string venueId)
        {
            switch (venueId)
            {
                case "1":
                    return "Home";
                case "2":
                    return "Away";
                default:
                    return "Unknown";
            }
        }

        private List<DLContactDetail> BuildContactDetailsList(XmlNodeList contactDetailsNodes)
        {
            return (from XmlNode contactDetailNode in contactDetailsNodes
                select new DLContactDetail
                {
                    LegacyMemberId =
                        Convert.ToInt32(contactDetailNode.SelectSingleNode("memberid").InnerText),
                    Email = contactDetailNode.SelectSingleNode("email") == null
                        ? "unknown@bw-cc.co.uk"
                        : contactDetailNode.SelectSingleNode("email").InnerText,
                    Telephone = string.Format("{0}{1}{2}",

                        contactDetailNode.SelectSingleNode("hometelephone") == null || string.IsNullOrEmpty(contactDetailNode.SelectSingleNode("hometelephone").InnerText)
                            ? string.Empty
                            : string.Format(" {0}", contactDetailNode.SelectSingleNode("hometelephone").InnerText),
                        contactDetailNode.SelectSingleNode("worktelephone") == null || string.IsNullOrEmpty(contactDetailNode.SelectSingleNode("worktelephone").InnerText)
                            ? string.Empty
                            : string.Format(", {0}", contactDetailNode.SelectSingleNode("worktelephone").InnerText),
                        contactDetailNode.SelectSingleNode("mobiletelephone") == null || string.IsNullOrEmpty(contactDetailNode.SelectSingleNode("mobiletelephone").InnerText)
                            ? string.Empty
                            : string.Format(", {0}", contactDetailNode.SelectSingleNode("mobiletelephone").InnerText)
                        )
                }).ToList();
        }

        private List<DLAddress> BuildAddressList(XmlNodeList addressNodes)
        {
            return (from XmlNode addressNode in addressNodes
                select new DLAddress
                {
                    LegacyMemberId = Convert.ToInt32(addressNode.SelectSingleNode("memberid").InnerText),
                    AddressLine = string.Format("{0}{1}{2}{3}",
                        addressNode.SelectSingleNode("address1") == null
                            ? "unknown"
                            : addressNode.SelectSingleNode("address1").InnerText,
                        addressNode.SelectSingleNode("address2") == null
                            ? string.Empty
                            : string.Format(" {0}", addressNode.SelectSingleNode("address2").InnerText),
                        addressNode.SelectSingleNode("towncity") == null
                            ? string.Empty
                            : string.Format(", {0}", addressNode.SelectSingleNode("towncity").InnerText),
                        addressNode.SelectSingleNode("county") == null
                            ? string.Empty
                            : string.Format(", {0}", addressNode.SelectSingleNode("county").InnerText)
                        ),
                    Postcode =
                         addressNode.SelectSingleNode("postcode") == null
                            ? "unknown"
                            : addressNode.SelectSingleNode("postcode").InnerText
                }).ToList();
        }

        private List<DLReport> BuildReportList(XmlNodeList reportNodes)
        {
            return (from XmlNode reportNode in reportNodes
                    select new DLReport
                {
                    LegacyFixtureId = Convert.ToInt32(reportNode.SelectSingleNode("fixtureid").InnerText),
                    Report = reportNode.SelectSingleNode("report").InnerText,
                    CreatedBy = reportNode.SelectSingleNode("createdby").InnerText,
                    CreateDate = reportNode.SelectSingleNode("createdate").InnerText
                }).ToList();
        }        

        private List<DLGuardianJunior> BuildGuardianJuniorList(XmlNodeList guardianjuniorNodes)
        {
            return (from XmlNode guardianjuniorNode in guardianjuniorNodes
                    select new DLGuardianJunior
                {
                    LegacyJuniorMemberId =
                        Convert.ToInt32(guardianjuniorNode.SelectSingleNode("juniorid").InnerText),
                    LegacyGuardianMemberId =
                        Convert.ToInt32(guardianjuniorNode.SelectSingleNode("guardianid").InnerText)
                }).ToList();
        }

        private List<DLGuardian> BuildGuardianList(XmlNodeList guardianNodes)
        {

            return (from XmlNode playerNode in guardianNodes
                    select new DLGuardian
                    {
                        LegacyMemberId = Convert.ToInt32(playerNode.SelectSingleNode("guardianid").InnerText)
                    }).ToList();
        }

        private List<DLPlayer> BuildPlayerList(XmlNodeList playerNodes)
        {
            return (from XmlNode playerNode in playerNodes 
                    select new DLPlayer
                    {
                        AgeGroup = playerNode.SelectSingleNode("agegroup").InnerText,
                        LegacyMemberId = Convert.ToInt32(playerNode.SelectSingleNode("memberid").InnerText),
                        Nickname = playerNode.SelectSingleNode("nickname").InnerText
                    }).ToList();
        }

        private List<DLMember> BuildMemberList(XmlNodeList memberNodes)
        {
            var rtnList = new List<DLMember>();
            foreach (XmlNode memberNode in memberNodes)
            {
                DateTime dob;
                DateTime.TryParse(memberNode.SelectSingleNode("dob").InnerText, out dob);
                var badDate = new DateTime(1970, 1, 1);
                if (dob == badDate ) dob = DateTime.MinValue;

                rtnList.Add(new DLMember
                {
                    LegacyId = Convert.ToInt32(memberNode.SelectSingleNode("memberid").InnerText),
                    Lastname = memberNode.SelectSingleNode("surname").InnerText,
                    Firstname = memberNode.SelectSingleNode("firstname").InnerText,
                    Dob = dob == DateTime.MinValue ? (DateTime?) null : dob
                });
            }

            return rtnList;
        }
    }
}