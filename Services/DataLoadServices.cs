
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using AutoMapper;
using ivNet.Club.Entities;
using ivNet.Club.Enums;
using ivNet.Club.Helpers;
using ivNet.Club.Models.DataLoad;
using Orchard;
using Orchard.Logging;
using Orchard.Security;

namespace ivNet.Club.Services
{
    public interface IDataLoadServices : IDependency
    {
        bool AddMembers(List<DLMember> memberList);
        bool AddPlayers(List<DLPlayer> playerList);
        bool AddGuardians(List<DLGuardian> guardianList);
        bool AddGuardiansJuniors(List<DLGuardianJunior> guardianList);
        bool AddAddresses(List<DLAddress> addressList);
        bool AddContactDetails(List<DLContactDetail> contactDetailsList);
        bool AddFixtures(List<DLFixture> fixtureList);
        bool AddStats(List<DLStat> statsList);
        bool AddReports(List<DLReport> reportList);
    }

    public class DataLoadServices : BaseService, IDataLoadServices
    {
        public DataLoadServices(IAuthenticationService authenticationService) : base(authenticationService)
        {
        }

        public bool AddMembers(List<DLMember> memberList)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        foreach (var member in memberList.Select(dlMember => Mapper.Map(dlMember, new Member())))
                        {
                            SetAudit(member);
                            member.IsVetted = 1;
                            session.SaveOrUpdate(member);
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, ex, string.Empty, null);
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public bool AddPlayers(List<DLPlayer> playerList)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var memberType = new MemberType
                        {
                            Name = "Player"
                        };

                        memberType.Init();
                        SetAudit(memberType);
                        session.SaveOrUpdate(memberType);

                        foreach (var dlPlayer in playerList)
                        {
                            var member = session.CreateCriteria(typeof (Member))
                                .List<Member>().FirstOrDefault(x => x.LegacyId.Equals(dlPlayer.LegacyMemberId));

                            member.AddMemberType(memberType);
                            SetAudit(member);
                            session.SaveOrUpdate(member);

                            var player = new Player
                            {
                                PlayerType =
                                    dlPlayer.AgeGroup == "J" ? (int) PlayerType.Junior : (int) PlayerType.Senior,
                                Nickname = dlPlayer.Nickname,
                                Member = member
                            };

                            SetAudit(player);
                            session.SaveOrUpdate(player);
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, ex, string.Empty, null);
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public bool AddGuardians(List<DLGuardian> guardianList)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var memberType = new MemberType
                        {
                            Name = "Guardian"
                        };

                        memberType.Init();
                        SetAudit(memberType);
                        session.SaveOrUpdate(memberType);

                        foreach (var dlGuardian in guardianList)
                        {
                            var member = session.CreateCriteria(typeof (Member))
                                .List<Member>().FirstOrDefault(x => x.LegacyId.Equals(dlGuardian.LegacyMemberId));

                            member.AddMemberType(memberType);
                            SetAudit(member);
                            session.SaveOrUpdate(member);

                            var guardian = new Guardian
                            {
                                Member = member
                            };

                            guardian.Init();
                            SetAudit(guardian);
                            session.SaveOrUpdate(guardian);
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, ex, string.Empty, null);
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public bool AddGuardiansJuniors(List<DLGuardianJunior> guardianJuniorList)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        foreach (var dlGuardianJunior in guardianJuniorList)
                        {
                            var juniorPlayer = session.CreateCriteria(typeof (Player))
                                .List<Player>()
                                .FirstOrDefault(x => x.Member.LegacyId.Equals(dlGuardianJunior.LegacyJuniorMemberId));

                            var guardian = session.CreateCriteria(typeof (Guardian))
                                .List<Guardian>()
                                .FirstOrDefault(x => x.Member.LegacyId.Equals(dlGuardianJunior.LegacyGuardianMemberId));

                            guardian.AddWard(juniorPlayer);

                            SetAudit(guardian);
                            session.SaveOrUpdate(guardian);

                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, ex, string.Empty, null);
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public bool AddAddresses(List<DLAddress> addressList)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        foreach (var dlAddress in addressList)
                        {
                            var addressKey = CustomStringHelper.BuildKey(new[] { dlAddress.AddressLine, dlAddress.Postcode });
                            var member = session.CreateCriteria(typeof (Member))
                                .List<Member>().FirstOrDefault(x => x.LegacyId.Equals(dlAddress.LegacyMemberId));

                            if (member == null) continue;

                            var address = session.CreateCriteria(typeof (Address))
                                .List<Address>().FirstOrDefault(x => x.AddressKey.Equals(addressKey)) ??
                                          new Address();

                            address.AddressKey = addressKey;                                
                            address.AddressLine = dlAddress.AddressLine;
                            address.Postcode = dlAddress.Postcode;

                            SetAudit(address);
                            session.SaveOrUpdate(address);

                            member.Address = address;
                            SetAudit(member);
                            session.SaveOrUpdate(member);
                        }
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, ex, string.Empty, null);
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public bool AddContactDetails(List<DLContactDetail> contactDetailsList)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        foreach (var dlContactDetail in contactDetailsList)
                        {                            
                            if (string.IsNullOrEmpty(dlContactDetail.Telephone) &&
                                string.IsNullOrEmpty(dlContactDetail.Email)) continue;
                           
                            var member = session.CreateCriteria(typeof (Member))
                                .List<Member>().FirstOrDefault(x => x.LegacyId.Equals(dlContactDetail.LegacyMemberId));

                            if (member == null) continue;

                            var contactKey = CustomStringHelper.BuildKey(new[] { dlContactDetail.Email });

                            var contact = session.CreateCriteria(typeof (Contact))
                                .List<Contact>().FirstOrDefault(x => x.ContactKey.Equals(contactKey)) ??
                                          new Contact();

                            contact.ContactKey = contactKey;
                            contact.Email = dlContactDetail.Email;
                            contact.Phone = dlContactDetail.Telephone;

                            SetAudit(contact);
                            session.SaveOrUpdate(contact);

                            member.Contact = contact;
                            SetAudit(member);
                            session.SaveOrUpdate(member);
                        }
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, ex, string.Empty, null);
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public bool AddFixtures(List<DLFixture> fixtureList)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var key = "";
                    try
                    {
                        foreach (var dlFixture in fixtureList)
                        {
                            // ResultType                      
                            var resultType = session.CreateCriteria(typeof (ResultType))
                                .List<ResultType>().FirstOrDefault(x => x.Name.Equals(dlFixture.ResultType)) ??
                                             new ResultType();

                            if (resultType.Id == 0)
                            {
                                resultType.Name = dlFixture.ResultType;

                                SetAudit(resultType);
                                session.Save(resultType);
                            }

                            // Team 
                            var homeTeam = session.CreateCriteria(typeof (TeamName))
                                .List<TeamName>().FirstOrDefault(x => x.Name.Equals(dlFixture.Team)) ??
                                           new TeamName();

                            if (homeTeam.Id == 0)
                            {
                                homeTeam.Name = dlFixture.Team;
                                homeTeam.Type = (int) TeamType.Home;

                                SetAudit(homeTeam);
                                session.Save(homeTeam);
                            }

                            // Opposition 
                            var opposition = session.CreateCriteria(typeof (TeamName))
                                .List<TeamName>().FirstOrDefault(x => x.Name.Equals(dlFixture.Opposition)) ??
                                             new TeamName();

                            if (opposition.Id == 0)
                            {
                                opposition.Name = dlFixture.Opposition;
                                opposition.Type = (int) TeamType.Opposition;

                                SetAudit(opposition);
                                session.Save(opposition);
                            }

                            // Venue 
                            var venue = session.CreateCriteria(typeof (Venue))
                                .List<Venue>().FirstOrDefault(x => x.Name.Equals(dlFixture.Venue)) ??
                                        new Venue();

                            if (venue.Id == 0)
                            {
                                venue.Name = dlFixture.Venue;

                                SetAudit(venue);
                                session.Save(venue);
                            }

                            // Competition 
                            var competition = session.CreateCriteria(typeof (Competition))
                                .List<Competition>().FirstOrDefault(x => x.Name.Equals(dlFixture.Competition)) ??
                                              new Competition();

                            if (competition.Id == 0)
                            {
                                competition.Name = dlFixture.Competition;

                                SetAudit(competition);
                                session.Save(competition);
                            }

                            // Fixture
                            var fixtureKey = CustomStringHelper.BuildKey(new[]
                            {homeTeam.Name, dlFixture.DatePlayed.ToShortDateString()});

                            var fixture = session.CreateCriteria(typeof (Fixture))
                                .List<Fixture>().FirstOrDefault(x => x.FixtureKey.Equals(fixtureKey)) ??
                                          new Fixture();

                            fixture.Competition = competition;
                            fixture.DatePlayed = dlFixture.DatePlayed;
                            fixture.FixtureKey = fixtureKey;
                            fixture.HomeTeam = homeTeam;
                            fixture.LegacyId = dlFixture.LegacyId;
                            fixture.Opposition = opposition;
                            fixture.Result = dlFixture.Result;
                            fixture.ResultType = resultType;
                            fixture.Venue = venue;
                            key = fixture.FixtureKey;
                            SetAudit(fixture);
                            session.SaveOrUpdate(fixture);

                        }
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        var cakes = key;
                        Logger.Log(LogLevel.Error, ex, string.Empty, null);
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public bool AddStats(List<DLStat> statsList)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var bwccXmlDoc = new XmlDocument();

                        bwccXmlDoc.Load(
                            HttpContext.Current.Server.MapPath(
                                "~/Modules/ivNet.Club/App_Data/MySql/members.xml"));

                        var memberType = session.CreateCriteria(typeof (MemberType))
                            .List<MemberType>().FirstOrDefault(x => x.Name.Equals("Player"));

                        foreach (var dlStat in statsList)
                        {
                            // HowOut 
                            var howout = session.CreateCriteria(typeof (HowOut))
                                .List<HowOut>().FirstOrDefault(x => x.Name.Equals(dlStat.HowOut)) ??
                                         new HowOut();

                            if (howout.Id == 0)
                            {
                                howout.Name = dlStat.HowOut;

                                SetAudit(howout);
                                session.Save(howout);
                            }

                            // Fixture 
                            var fixture = session.CreateCriteria(typeof (Fixture))
                                .List<Fixture>().FirstOrDefault(x => x.FixtureKey.Equals(dlStat.FixtureKey)) ??
                                          new Fixture();

                            // Player 
                            var player = session.CreateCriteria(typeof (Player))
                                .List<Player>().FirstOrDefault(x => x.Member.MemberKey.Equals(dlStat.MemberKey)) ??
                                         new Player();

                            if (player.Id == 0)
                            {
                                // Member 
                                var member = session.CreateCriteria(typeof (Member))
                                    .List<Member>().FirstOrDefault(x => x.MemberKey.Equals(dlStat.MemberKey)) ??
                                             new Member();

                                if (member.Id != 0)
                                {
                                    member.AddMemberType(memberType);
                                    SetAudit(member);
                                    session.SaveOrUpdate(member);

                                    player = new Player
                                    {
                                        Member = member
                                    };
                                    player.Init();
                                    SetTypeNickname(player, bwccXmlDoc, member.LegacyId);

                                    SetAudit(player);
                                    session.SaveOrUpdate(player);
                                }
                                //else
                                //{
                                //    var cakes = "";
                                //}                               
                            }

                            //if (fixture.Id == 0)
                            //{
                            //    var zozo = "";
                            //}


                            if (howout.Name != "DNB")
                            {

                                var battingStat = new BattingStat
                                {
                                    Position = dlStat.Position,
                                    Runs = dlStat.RunsScored,
                                    Player = player,
                                    Fixture = fixture,
                                    HowOut = howout
                                };

                                SetAudit(battingStat);
                                session.SaveOrUpdate(battingStat);
                            }

                            if (dlStat.OversBowled>0)
                            {

                                var bowlingStat = new BowlingStat
                                {
                                    Overs = dlStat.OversBowled,
                                    Maidens = dlStat.Maidens,
                                    Wickets = dlStat.Wickets,
                                    Runs = dlStat.RunsConceeded,
                                    Player = player,
                                    Fixture = fixture
                                };

                                SetAudit(bowlingStat);
                                session.SaveOrUpdate(bowlingStat);
                            }

                            if (dlStat.Catches + dlStat.Stumpings > 0)
                            {
                                var fieldingStat = new FieldingStat
                                {
                                    Catches = dlStat.Catches,
                                    Stumpings = dlStat.Stumpings,
                                    Player = player,
                                    Fixture = fixture
                                };

                                SetAudit(fieldingStat);
                                session.SaveOrUpdate(fieldingStat);
                            }

                            var team = session.CreateCriteria(typeof(Team))
                           .List<Team>().FirstOrDefault(x => x.Fixture.Id.Equals(fixture.Id));

                            if (team == null)
                            {
                                team = new Team();
                                team.Init();
                                team.Fixture = fixture;
                                SetAudit(team);
                            }

                            team.AddPlayer(player);

                            session.SaveOrUpdate(team);

                            if (fixture.Team == null)
                            {
                                fixture.Team = team;
                                session.SaveOrUpdate(fixture);
                            }
                        }
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, ex, string.Empty, null);
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public bool AddReports(List<DLReport> reportList)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        foreach (var dlReportDetail in reportList)
                        {
                            if (string.IsNullOrEmpty(dlReportDetail.Report)) continue;

                            var fixture = session.CreateCriteria(typeof(Fixture))
                                .List<Fixture>().FirstOrDefault(x => x.LegacyId.Equals(dlReportDetail.LegacyFixtureId));

                            if (fixture == null) continue;
                  
                            var report = session.CreateCriteria(typeof(Report))
                                .List<Report>().FirstOrDefault(x => x.Fixture.Id.Equals(fixture.Id)) ??
                                          new Report();

                            report.Fixture = fixture;
                            report.Html = dlReportDetail.Report;

                            if (report.Html.Length > 4000) report.Html = report.Html.Substring(0, 4000);

                            SetAudit(report);
                            if (!string.IsNullOrEmpty(dlReportDetail.CreatedBy))
                                report.CreatedBy = dlReportDetail.CreatedBy;

                            if (!string.IsNullOrEmpty(dlReportDetail.CreateDate))
                            {
                                const string expectedFormat = "yyyy-MM-dd";
                                DateTime createDate;

                                bool result = DateTime.TryParseExact(
                                    dlReportDetail.CreateDate,
                                    expectedFormat,
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    System.Globalization.DateTimeStyles.None,
                                    out createDate);

                                if( result )
                                    report.CreateDate = createDate;
                            }
                                

                            session.SaveOrUpdate(report);

                            
                        }
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, ex, string.Empty, null);
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        private void SetTypeNickname(Player newPlayer, XmlDocument bwccXmlDoc, int legacyId)
        {
            DateTime dob;

            var playerNode = bwccXmlDoc.DocumentElement.SelectSingleNode(string.Format("row[memberid='{0}']", legacyId));
                   
            var playerType = playerNode.SelectSingleNode("agegroup").InnerText;

            newPlayer.Nickname = playerNode.SelectSingleNode("nickname").InnerText;
            newPlayer.PlayerType = playerType == "J" ? (int) PlayerType.Junior : (int) PlayerType.Senior;
        }
    }
}