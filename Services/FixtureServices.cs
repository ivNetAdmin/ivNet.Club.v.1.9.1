
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ivNet.Club.Controllers.api;
using ivNet.Club.Entities;
using ivNet.Club.Enums;
using ivNet.Club.Helpers;
using ivNet.Club.Models.View;
using NHibernate.Criterion;
using Orchard;
using Orchard.Security;
using Orchard.Logging;

namespace ivNet.Club.Services
{

    public interface IFixtureServices : IDependency
    {
        Fixture SaveFixture(int legacyFixtureId, DateTime fixtureDate, TeamName homeTeam, TeamName opposition);
        List<FixtureDetail> GetFixtures(Dictionary<string, string> paramCollection);
        FixtureDetail GetFixture(int id);
        void UpdateFixtureStats(int id, dynamic fixture);
        void AddPlayerAvailabilty(AvailabilityModel availability);
        void UpdatePlayerAvailabilty(int playerId, string month, string date, bool available);
        void DeleteFixture(int id);
    }

    public class FixtureServices : BaseService, IFixtureServices
    {
        private IStatsServices _statsServices;
        private IConfigServices _configServices;
        private IPlayerServices _playerServices;
        private IMemberServices _memberServices;
        
        private string[] _months = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        public FixtureServices(
            IAuthenticationService authenticationService,
            IStatsServices statsServices,
            IConfigServices configServices,
            IPlayerServices playerServices,
            IMemberServices memberServices)
            : base(authenticationService)
        {
            _statsServices = statsServices;
            _configServices = configServices;
            _playerServices = playerServices;
            _memberServices = memberServices;
        }

        public Fixture SaveFixture(int legacyFixtureId, DateTime fixtureDate, TeamName homeTeam, TeamName opposition)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var fixture = session.CreateCriteria(typeof (Fixture))
                            .List<Fixture>()
                            .FirstOrDefault(x => x.LegacyId.Equals(legacyFixtureId)) ??
                                      new Fixture
                                      {
                                          FixtureKey =
                                              CustomStringHelper.BuildKey(new[]
                                              {homeTeam.Name, fixtureDate.ToShortDateString()}),
                                          LegacyId = legacyFixtureId,
                                          DatePlayed = fixtureDate,
                                          HomeTeam = homeTeam,
                                          Opposition = opposition
                                      };

                        if (fixture.Id != 0)
                        {
                            transaction.Rollback();
                            return fixture;
                        }

                        SetAudit(fixture);
                        session.SaveOrUpdate(fixture);
                        transaction.Commit();

                        return fixture;

                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, ex, string.Empty, null);
                        transaction.Rollback();
                        return null;
                    }
                }
            }
        }

        public List<FixtureDetail> GetFixtures(Dictionary<string, string> paramCollection)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var fixtureCriteria = session.CreateCriteria(typeof (Fixture));

                var fixtureList = new List<FixtureDetail>();
                var playerName = string.Empty;

                if (paramCollection.ContainsKey("player") && !string.IsNullOrEmpty(paramCollection["player"]))
                {
                    var playerCriteria = session.CreateCriteria(typeof (Player));
                    playerCriteria.CreateCriteria("Member", "m");
                    playerCriteria.Add(Expression.Like("m.MemberKey", paramCollection["player"], MatchMode.Anywhere));

                    var players = playerCriteria.List<Player>();

                    foreach (var player in players)
                    {
                        playerName = string.Format("{0}, {1}", player.Member.Lastname, player.Member.Firstname);
                        foreach (var team in player.Teams)
                        {
                            Fixture fixture = null;
                            if (paramCollection.ContainsKey("year") && !string.IsNullOrEmpty(paramCollection["year"]))
                            {
                                if (team.Fixture.DatePlayed >=
                                    DateTime.Parse(string.Format("31/12/{0}", paramCollection["year"])).AddYears(-1)
                                    &&
                                    team.Fixture.DatePlayed <=
                                    DateTime.Parse(string.Format("01/01/{0}", paramCollection["year"])).AddYears(1))
                                {
                                    fixture = team.Fixture;
                                }
                            }
                            else
                            {
                                fixture = team.Fixture;
                            }

                            if (fixture == null) continue;

                            if (paramCollection.ContainsKey("team") && !paramCollection.ContainsKey("year"))
                            {
                                if (fixture.HomeTeam.Name == paramCollection["team"])
                                {
                                    if (fixture != null)
                                        fixtureList.Add(Mapper.Map(fixture, new FixtureDetail {Player = playerName}));
                                }
                            }
                            else
                            {
                                if (fixture != null)
                                    fixtureList.Add(Mapper.Map(fixture, new FixtureDetail {Player = playerName}));
                            }
                        }
                    }
                }
                else
                {
                    if (paramCollection.ContainsKey("year") || paramCollection.ContainsKey("team"))
                    {
                        if (paramCollection.ContainsKey("year"))
                        {
                            fixtureCriteria.Add(Expression.Ge("DatePlayed",
                                DateTime.Parse(string.Format("31/12/{0}", paramCollection["year"])).AddYears(-1)))
                                .Add(Expression.Lt("DatePlayed",
                                    DateTime.Parse(string.Format("01/01/{0}", paramCollection["year"])).AddYears(1)));
                        }

                        if (paramCollection.ContainsKey("team"))
                        {
                            fixtureCriteria.CreateCriteria("HomeTeam", "tn");

                            fixtureCriteria.Add(Expression.Eq("tn.Name", paramCollection["team"]));
                        }
                    }
                    var list = fixtureCriteria.List<Fixture>().OrderBy(x => x.DatePlayed).ToList();

                    fixtureList.AddRange(list.Select(fixture => Mapper.Map(fixture, new FixtureDetail())));

                    //foreach (var fixture in list)
                    //{
                    //    fixtureList.Add(Mapper.Map(fixture, new FixtureDetail()));
                    //}

                }

                return fixtureList;
            }
        }

        public FixtureDetail GetFixture(int id)
        {
            using (var session = NHibernateHelper.OpenSession())
            {

                if (id == 0)
                {
                    return new FixtureDetail
                    {
                        FixtureId = 0,
                        FixtureStats = _statsServices.GetStats(0),
                        HowOutList = _configServices.GetHowOutList(),
                        PlayerList = _playerServices.GetAllActivePlayers(),
                        ResultTypeList = _configServices.GetResultTypeList(),
                        OppositionList = _configServices.GetTeamNames(TeamType.Opposition)
                    };
                }

                var fixture = session.CreateCriteria(typeof (Fixture))
                    .List<Fixture>()
                    .FirstOrDefault(x => x.Id.Equals(id));

                if (fixture == null) return new FixtureDetail();

                var team = session.CreateCriteria(typeof (Team))
                    .List<Team>()
                    .FirstOrDefault(x => x.Fixture.Id.Equals(id));

                var players = team == null
                    ? (IEnumerable<Player>) new List<Player>()
                    : team.Players.OrderBy(x => x.Member.Lastname).ThenBy(x => x.Member.Firstname);

                var fixtureDetail = new FixtureDetail
                {
                    FixtureId = fixture.Id,
                    DatePlayed = fixture.DatePlayed,
                    HomeTeam = fixture.HomeTeam == null ? "unknown" : fixture.HomeTeam.Name,
                    Result = fixture.Result,
                    ResultType = fixture.ResultType == null ? "unknown" : fixture.ResultType.Name,
                    Opposition = fixture.Opposition == null ? "unknown" : fixture.Opposition.Name,
                    FixtureStats = _statsServices.GetStats(fixture.Id),
                    TeamPlayerList = SetTeam(players),
                    HowOutList = _configServices.GetHowOutList(),
                    PlayerList = _playerServices.GetAllActivePlayers(),
                    ResultTypeList = _configServices.GetResultTypeList()
                };

                return fixtureDetail;
            }
        }

        public void UpdateFixtureStats(int id, dynamic fixtureStats)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var sqlQuery = string.Empty;

                        // fixture details
                        var fixture = id > 0
                            ? session.CreateCriteria(typeof (Fixture))
                                .List<Fixture>()
                                .FirstOrDefault(x => x.Id.Equals(id))
                            : new Fixture();

                        if (fixture == null) return;

                        if (id == 0)
                        {

                            fixture.HomeTeam = session.CreateCriteria(typeof (TeamName))
                                .List<TeamName>().FirstOrDefault(x => x.Name.Equals((string) fixtureStats["Team"]));

                            fixture.Opposition = session.CreateCriteria(typeof (TeamName))
                                .List<TeamName>()
                                .FirstOrDefault(x => x.Name.Equals((string) fixtureStats["Opposition"]));

                            fixture.DatePlayed = DateTime.Parse((string) fixtureStats["Date"]);

                            fixture.FixtureKey = CustomStringHelper.BuildKey(new[]
                            {
                                fixture.HomeTeam.Name,
                                fixture.DatePlayed.ToShortDateString()
                            });
                        }

                        var strResultType = (string) fixtureStats["ResultType"];
                        var strResult = (string) fixtureStats["Result"];

                        var resultType = session.CreateCriteria(typeof (ResultType))
                            .List<ResultType>()
                            .FirstOrDefault(x => x.Name.Equals(strResultType));

                        fixture.ResultType = resultType;
                        fixture.Result = strResult;

                        SetAudit(fixture);
                        session.SaveOrUpdate(fixture);

                        // batting stats
                        var battingStats = (IEnumerable) fixtureStats["Batting"];

                        if (battingStats != null)
                        {
                            sqlQuery = string.Format("DELETE FROM ivNetBattingStat WHERE FixtureID = {0}", id);
                            session.CreateSQLQuery(sqlQuery)
                                .ExecuteUpdate();

                            foreach (dynamic battingStat in battingStats)
                            {
                                var memberKey = CustomStringHelper.BuildKey(new[] {(string) battingStat["Name"]});
                                var player = session.CreateCriteria(typeof (Player))
                                    .List<Player>()
                                    .FirstOrDefault(x => x.Member.MemberKey.Equals(memberKey));

                                var howOut = session.CreateCriteria(typeof (HowOut))
                                    .List<HowOut>()
                                    .FirstOrDefault(x => x.Name.Equals((string) battingStat["HowOut"]));

                                var newBattingStat = new BattingStat
                                {
                                    Player = player,
                                    Fixture = fixture,
                                    Position = Convert.ToInt32((string) battingStat["Position"]),
                                    Runs = Convert.ToInt32((string) battingStat["Runs"]),
                                    Overs = Convert.ToDecimal((string) battingStat["Overs"]),
                                    HowOut = howOut
                                };

                                SetAudit(newBattingStat);
                                session.SaveOrUpdate(newBattingStat);
                            }
                        }

                        // bowling stats
                        var bowlingStats = (IEnumerable) fixtureStats["Bowling"];

                        if (bowlingStats != null)
                        {
                            sqlQuery = string.Format("DELETE FROM ivNetBowlingStat WHERE FixtureID = {0}", id);
                            session.CreateSQLQuery(sqlQuery)
                                .ExecuteUpdate();

                            foreach (dynamic bowlingStat in bowlingStats)
                            {
                                var memberKey = CustomStringHelper.BuildKey(new[] {(string) bowlingStat["Name"]});
                                var player = session.CreateCriteria(typeof (Player))
                                    .List<Player>()
                                    .FirstOrDefault(x => x.Member.MemberKey.Equals(memberKey));

                                var newBowlingStat = new BowlingStat
                                {
                                    Player = player,
                                    Fixture = fixture,
                                    Wickets = Convert.ToInt32((string) bowlingStat["Wickets"]),
                                    Runs = Convert.ToInt32((string) bowlingStat["Runs"]),
                                    Overs = Convert.ToDecimal((string) bowlingStat["Overs"]),
                                    Maidens = Convert.ToInt32((string) bowlingStat["Maidens"])
                                };

                                SetAudit(newBowlingStat);
                                session.SaveOrUpdate(newBowlingStat);
                            }
                        }

                        // fielding stats
                        var fieldingStats = (IEnumerable) fixtureStats["Fielding"];

                        if (fieldingStats != null)
                        {
                            sqlQuery = string.Format("DELETE FROM ivNetFieldingStat WHERE FixtureID = {0}", id);
                            session.CreateSQLQuery(sqlQuery)
                                .ExecuteUpdate();

                            foreach (dynamic fieldingStat in fieldingStats)
                            {
                                var memberKey = CustomStringHelper.BuildKey(new[] {(string) fieldingStat["Name"]});
                                var player = session.CreateCriteria(typeof (Player))
                                    .List<Player>()
                                    .FirstOrDefault(x => x.Member.MemberKey.Equals(memberKey));

                                var newFieldingStat = new FieldingStat
                                {
                                    Player = player,
                                    Fixture = fixture,
                                    Catches = Convert.ToInt32((string) fieldingStat["Catches"]),
                                    Stumpings = Convert.ToInt32((string) fieldingStat["Stumpings"])
                                };

                                SetAudit(newFieldingStat);
                                session.SaveOrUpdate(newFieldingStat);
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, ex, string.Empty, null);
                        transaction.Rollback();
                    }
                }
            }
        }

        public void AddPlayerAvailabilty(AvailabilityModel availabilityModel)
        {
            var member = _memberServices.GetMemberByUserId(CurrentUser.Id);
            if (member == null) return;
            var player = _playerServices.GetPlayerByName(member.Lastname, member.Firstname);
            if (player == null) return;

            availabilityModel.PlayerId = player.Id;

            var availabilityList = GetPlayerAvailability(player.Id);
            foreach (var availability in availabilityList)
            {
                foreach (var month in availabilityModel.Months)
                {
                    foreach (var day in month.Days)
                    {
                        if (string.IsNullOrEmpty(day.Date)) continue;

                        var availableDate =
                            new DateTime(DateTime.Now.Year,
                                Array.IndexOf(_months, month.Name) + 1,
                                Convert.ToInt32(day.Date));

                        if (availability.Date == availableDate)
                            day.Available = true;
                    }
                }
            }

        }

        public void UpdatePlayerAvailabilty(int playerId, string month, string date, bool available)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var availableDate =
                            new DateTime(DateTime.Now.Year,
                                Array.IndexOf(_months, month) + 1,
                                Convert.ToInt32(date));

                        if (available)
                        {
                            var player = session.CreateCriteria(typeof (Player))
                                .List<Player>()
                                .FirstOrDefault(x => x.Id.Equals(playerId));

                            var availability = new Availability()
                            {
                                Player = player,
                                Date = availableDate
                            };

                            SetAudit(availability);
                            session.SaveOrUpdate(availability);
                        }
                        else
                        {
                            var availability = session.CreateCriteria(typeof(Availability))
                                .List<Availability>()
                                .FirstOrDefault(x => x.Player.Id == playerId && x.Date == availableDate);

                            session.Delete(availability);
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, ex, string.Empty, null);
                        transaction.Rollback();
                    }
                }
            }
        }

        public void DeleteFixture(int id)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {

                        var sqlQuery = string.Format("DELETE FROM ivNetBattingStat WHERE FixtureID = {0}", id);
                        session.CreateSQLQuery(sqlQuery)
                            .ExecuteUpdate();

                        sqlQuery = string.Format("DELETE FROM ivNetBowlingStat WHERE FixtureID = {0}", id);
                        session.CreateSQLQuery(sqlQuery)
                            .ExecuteUpdate();

                        sqlQuery = string.Format("DELETE FROM ivNetFieldingStat WHERE FixtureID = {0}", id);
                        session.CreateSQLQuery(sqlQuery)
                            .ExecuteUpdate();

                        var fixture = session.CreateCriteria(typeof (Fixture))
                            .List<Fixture>()
                            .FirstOrDefault(x => x.Id.Equals(id));

                        session.Delete(fixture);
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, ex, string.Empty, null);
                        transaction.Rollback();
                    }
                }
            }
        }

        //private Dictionary<int, string> SetTeam(IEnumerable<Player> players)
                //{
                //    return players == null
                //        ? new Dictionary<int, string>()
                //        : players.ToDictionary(player => player.Id,
                //            player => string.Format("{0}, {1}", player.Member.Lastname, player.Member.Firstname.Substring(0, 1)));
                //}

            private
            List<string> SetTeam(IEnumerable<Player> players)
        {
            return
                players.Select(
                    player => string.Format("{0}, {1}", player.Member.Lastname, player.Member.Firstname.Substring(0, 1)))
                    .ToList();
        }

        private List<Availability> GetPlayerAvailability(int playerId)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                return session.CreateCriteria(typeof (Availability))
                    .List<Availability>().Where(x => x.Player.Id.Equals(playerId)).ToList();
            }
        }
    }
}
    
