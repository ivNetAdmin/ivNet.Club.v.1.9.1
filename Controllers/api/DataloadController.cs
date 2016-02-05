using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Xml;
using ivNet.Club.Entities;
using ivNet.Club.Enums;
using ivNet.Club.Helpers;
using ivNet.Club.Models.View;
using ivNet.Club.Services;
using NHibernate.Criterion;
using Orchard.Logging;

namespace ivNet.Club.Controllers.api
{
    public class DataloadController : ApiController
    {
        private readonly IMemberServices _memberServices;
        private readonly IConfigServices _configServices;
        private readonly IFixtureServices _fixtureServices;
        private readonly IPlayerServices _playerServices;
        private readonly IStatsServices _statsServices;

        public DataloadController(
            IMemberServices memberServices,
            IConfigServices configServices,
            IFixtureServices fixtureServices,
            IPlayerServices playerServices,
            IStatsServices statsServices)
        {
            _memberServices = memberServices;
            _configServices = configServices;
            _fixtureServices = fixtureServices;
            _playerServices = playerServices;
            _statsServices = statsServices;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        [HttpGet]
        public HttpResponseMessage Get(string id)
        {
            try
            {
                var bwccXmlDoc = new XmlDocument();

                switch (id)
                {
                    case "members":
                        bwccXmlDoc.Load(
                            HttpContext.Current.Server.MapPath(
                                "~/Modules/ivNet.Club/App_Data/MySql/member.contact.address.xml"));

                        var memberNodes = bwccXmlDoc.DocumentElement.SelectNodes("ROW");

                        foreach (XmlNode memberNode in memberNodes)
                        {

                            var ageGroupNode = memberNode.SelectSingleNode("agegroup");
                            var ageGroup = ageGroupNode == null ? string.Empty : ageGroupNode.InnerText;

                            var model = GenerateMemberDetailModel(memberNode);
                            if (ageGroup == "J") model = ApplyJuniorRules(model);

                            model = ApplyNullRules(model);

                            _memberServices.Register(model);

                        }

                        break;

                    case "guardians":

                        var bwccMemberRef = new XmlDocument();
                        bwccMemberRef.Load(
                            HttpContext.Current.Server.MapPath(
                                "~/Modules/ivNet.Club/App_Data/MySql/members.xml"));

                        bwccXmlDoc.Load(
                            HttpContext.Current.Server.MapPath(
                                "~/Modules/ivNet.Club/App_Data/MySql/member.contact.xml"));

                        var memberContactsNodes = bwccXmlDoc.DocumentElement.SelectNodes("ROW");

                        // 1-mother, 2-father, 6-alternative

                        foreach (XmlNode memberContactsNode in memberContactsNodes)
                        {
                            var juniorId = Convert.ToInt32(memberContactsNode.SelectSingleNode("memberid").InnerText);
                            var guardianId =
                                Convert.ToInt32(memberContactsNode.SelectSingleNode("contactid").InnerText);

                            var juniorNode =
                                bwccMemberRef.DocumentElement.SelectSingleNode(
                                    string.Format("ROW[memberid/text()='{0}']", juniorId));

                            var guardianNode =
                                bwccMemberRef.DocumentElement.SelectSingleNode(
                                    string.Format("ROW[memberid/text()='{0}']", guardianId));

                            if (juniorNode == null || guardianNode == null) continue;

                            var juniorKey =
                                CustomStringHelper.BuildKey(new[]
                                {
                                    juniorNode.SelectSingleNode("surname").InnerText,
                                    juniorNode.SelectSingleNode("firstname").InnerText
                                });

                            var guardianKey =
                                CustomStringHelper.BuildKey(new[]
                                {
                                    guardianNode.SelectSingleNode("surname").InnerText,
                                    guardianNode.SelectSingleNode("firstname").InnerText
                                });

                            _memberServices.DataLoadAddGuardian(juniorKey, guardianKey);
                        }

                        break;

                    case "stats":
                        bwccXmlDoc.Load(
                            HttpContext.Current.Server.MapPath(
                                "~/Modules/ivNet.Club/App_Data/MySql/player.stats.xml"));

                        var statsNodes = bwccXmlDoc.DocumentElement.SelectNodes("ROW");
                        var statCounter = 0;
                        foreach (XmlNode stat in statsNodes)
                        {
                            var legacyFixtureId = Convert.ToInt32(stat.SelectSingleNode("fixtureid").InnerText);
                            // create team

                            // get player
                            var lastname = stat.SelectSingleNode("lastname").InnerText;
                            var firstname = stat.SelectSingleNode("firstname").InnerText;
                            var player = _playerServices.GetPlayerByName(lastname, firstname);

                            if (player == null) continue;

                            var teamName = stat.SelectSingleNode("teamname").InnerText;
                            TeamName homeTeam;
                            _configServices.SaveTeamName(
                                new ConfigItem {Name = teamName, Type = TeamType.Home.ToString()}, out homeTeam);

                            // create opposition
                            TeamName opposition;
                            teamName = stat.SelectSingleNode("opposition").InnerText;
                            _configServices.SaveTeamName(
                                new ConfigItem {Name = teamName, Type = TeamType.Opposition.ToString()},
                                out opposition);

                            // create fixture                          
                            var fixtureDate = DateTime.Parse(stat.SelectSingleNode("dateplayed").InnerText);
                            var fixture = _fixtureServices.SaveFixture(legacyFixtureId, fixtureDate, homeTeam,
                                opposition);

                            // create how out
                            var howOutText = stat.SelectSingleNode("howout").InnerText;
                            HowOut howOut;
                            _configServices.SaveHowOut(new ConfigItem {Name = howOutText}, out howOut);

                            // create batting stat
                            var position = Convert.ToInt32(stat.SelectSingleNode("battingposition").InnerText);
                            var runs = Convert.ToInt32(stat.SelectSingleNode("runsscored").InnerText);
                            var overs = 0;
                            _statsServices.SaveBattingStat(player, fixture, howOut, position, runs, overs);

                            // create bowling stat
                            var oversbowled = Convert.ToDecimal(stat.SelectSingleNode("oversbowled").InnerText);
                            var maidenovers = Convert.ToInt32(stat.SelectSingleNode("maidenovers").InnerText);
                            var runsconceeded = Convert.ToInt32(stat.SelectSingleNode("runsconceeded").InnerText);
                            var wicketstaken = Convert.ToInt32(stat.SelectSingleNode("wicketstaken").InnerText);
                            _statsServices.SaveBowlingStat(player, fixture, oversbowled, maidenovers, runsconceeded,
                                wicketstaken);

                            // create fielding stat
                            var catches = Convert.ToInt32(stat.SelectSingleNode("catches").InnerText);
                            var stumpings = Convert.ToInt32(stat.SelectSingleNode("stumpings").InnerText);
                            _statsServices.SaveFieldingStat(player, fixture, catches, stumpings);

                            // updsate team
                            _statsServices.UpdateTeam(fixture, player);

                            statCounter++;

                            if (statCounter == 200) break;

                        }

                        break;
                }
                return Request.CreateResponse(HttpStatusCode.OK, id);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, string.Empty, null);
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        private MemberDetailModel ApplyJuniorRules(MemberDetailModel model)
        {
            model.Address = null;
            model.Email = null;
            model.Phone = null;
            model.Postcode = null;

            return model;
        }

        private MemberDetailModel ApplyNullRules(MemberDetailModel model)
        {
            if (model.Postcode == "unknown" || string.IsNullOrEmpty(model.Postcode))
            {
                model.Address = null;
                model.Postcode = null;
            }

            if (model.Email == "unknown" || string.IsNullOrEmpty(model.Email))
            {
                model.Email = null;
            }

            return model;
        }

        private MemberDetailModel GenerateMemberDetailModel(XmlNode memberNode)
        {
            var firstname = memberNode.SelectSingleNode("firstname") == null
                ? "unknown"
                : memberNode.SelectSingleNode("firstname").InnerText;

            return new MemberDetailModel
            {
                LegacyId = Convert.ToInt32(memberNode.SelectSingleNode("legacyid").InnerText),

                IsActive =
                    Convert.ToInt32(memberNode.SelectSingleNode("notactive").InnerText) == 0 ? (byte) 1 : (byte) 0,

                Address = string.Format("{0}{1}{2}",
                    memberNode.SelectSingleNode("address1") == null
                        ? "unknown"
                        : memberNode.SelectSingleNode("address1").InnerText,
                    memberNode.SelectSingleNode("address2") == null
                        ? string.Empty
                        : string.Format(" {0}", memberNode.SelectSingleNode("address2").InnerText),
                    memberNode.SelectSingleNode("towncity") == null
                        ? string.Empty
                        : string.Format(", {0}", memberNode.SelectSingleNode("towncity").InnerText)
                    ),

                Email = memberNode.SelectSingleNode("email") == null
                    ? "unknown"
                    : memberNode.SelectSingleNode("email").InnerText,

                Firstname = firstname,

                Lastname = memberNode.SelectSingleNode("surname").InnerText,

                Password =
                    CustomStringHelper.GenerateInitialPassword(
                        firstname),

                Phone = memberNode.SelectSingleNode("mobiletelephone") == null
                    ? memberNode.SelectSingleNode("hometelephone") == null
                        ? memberNode.SelectSingleNode("worktelephone") == null
                            ? string.Empty
                            : memberNode.SelectSingleNode("worktelephone").InnerText
                        : memberNode.SelectSingleNode("hometelephone").InnerText
                    : memberNode.SelectSingleNode("mobiletelephone").InnerText,

                Postcode = memberNode.SelectSingleNode("postcode") == null
                    ? "unknown"
                    : memberNode.SelectSingleNode("postcode").InnerText
            };

        }
    }
}