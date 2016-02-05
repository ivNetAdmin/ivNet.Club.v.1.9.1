
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using AutoMapper;
using ivNet.Club.Entities;
using ivNet.Club.Helpers;
using ivNet.Club.Models.View;
using ivNet.Club.Models.View.Stats;
using NHibernate;
using NHibernate.Criterion;
using Orchard;
using Orchard.Security;
using LogLevel = Orchard.Logging.LogLevel;

namespace ivNet.Club.Services
{

    public interface IStatsServices : IDependency
    {
        BattingStat SaveBattingStat(Player player, Fixture fixture, HowOut howOut, int position, int runs, decimal overs);
        BowlingStat SaveBowlingStat(Player player, Fixture fixture, decimal overs, int maidens, int runs, int wickets);
        FieldingStat SaveFieldingStat(Player player, Fixture fixture, int catches, int stumpings);
        Team UpdateTeam(Fixture fixture, Player player);
        FixtureStats GetStats(Dictionary<string, string> paramCollection);
        FixtureStats GetStats(int fixtureId);
    }

    public class StatsServices : BaseService, IStatsServices
    {
        public StatsServices(IAuthenticationService authenticationService) : base(authenticationService)
        {
        }

        public BattingStat SaveBattingStat(Player player, Fixture fixture, HowOut howOut, int position, int runs,
            decimal overs)
        {
            if (howOut.Name == "DNB") return null;

            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var stat = session.CreateCriteria(typeof(BattingStat))
                           .List<BattingStat>()
                           .FirstOrDefault(x => x.Player.Id.Equals(player.Id) && x.Fixture.Id.Equals(fixture.Id)) ??
                                  new BattingStat ();

                        if (stat.Id == 0)
                        {
                            stat.Fixture = fixture;
                            stat.Player = player;
                        }

                        stat.HowOut = howOut;
                        stat.Overs = overs;
                        stat.Position = position;
                        stat.Runs = runs;

                        SetAudit(stat);
                        session.SaveOrUpdate(stat);
                        transaction.Commit();
                        return stat;
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


        public BowlingStat SaveBowlingStat(Player player, Fixture fixture, decimal overs, int maidens, int runs, int wickets)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                if (overs == 0) return null;

                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var stat = session.CreateCriteria(typeof(BowlingStat))
                           .List<BowlingStat>()
                           .FirstOrDefault(x => x.Player.Id.Equals(player.Id) && x.Fixture.Id.Equals(fixture.Id)) ??
                                  new BowlingStat();

                        if (stat.Id == 0)
                        {
                            stat.Fixture = fixture;
                            stat.Player = player;
                        }

                        stat.Overs = overs;
                        stat.Maidens = maidens;
                        stat.Wickets = wickets;
                        stat.Runs = runs;

                        SetAudit(stat);
                        session.SaveOrUpdate(stat);
                        transaction.Commit();
                        return stat;
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

        public FieldingStat SaveFieldingStat(Player player, Fixture fixture, int catches, int stumpings)
        {
            if (catches + stumpings == 0) return null;

            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var stat = session.CreateCriteria(typeof(FieldingStat))
                          .List<FieldingStat>()
                          .FirstOrDefault(x => x.Player.Id.Equals(player.Id) && x.Fixture.Id.Equals(fixture.Id)) ??
                                 new FieldingStat();

                        if (stat.Id == 0)
                        {
                            stat.Fixture = fixture;
                            stat.Player = player;
                        }

                        stat.Catches = catches;
                        stat.Stumpings = stumpings;

                        SetAudit(stat);
                        session.SaveOrUpdate(stat);
                        transaction.Commit();
                        return stat;
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

        public Team UpdateTeam(Fixture fixture, Player player)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var team = session.CreateCriteria(typeof (Team))
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

                        transaction.Commit();

                        return team;
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

        public FixtureStats GetStats(Dictionary<string, string> paramCollection)
        {
            var statsData = new FixtureStats();
            using (var session = NHibernateHelper.OpenSession())
            {  
                // get stats
                AddBattingStats(session, statsData, paramCollection);
                AddBowingStats(session, statsData, paramCollection);
                AddFieldingStats(session, statsData, paramCollection);
                return statsData;
            }
        }

        public FixtureStats GetStats(int fixtureId)
        {
            var statsData = new FixtureStats();

            if (fixtureId == 0) return statsData;

            using (var session = NHibernateHelper.OpenSession())
            {
                var battingStats = session.CreateCriteria(typeof (BattingStat))
                    .List<BattingStat>().Where(x => x.Fixture.Id.Equals(fixtureId)).OrderBy(x => x.Position);

                foreach (var battingStat in battingStats)
                {
                    statsData.Batting.Add(Mapper.Map(battingStat, new Batting()));
                }

                var bowlingStats = session.CreateCriteria(typeof(BowlingStat))
                              .List<BowlingStat>().Where(x => x.Fixture.Id.Equals(fixtureId));

                foreach (var bowlingStat in bowlingStats)
                {
                    statsData.Bowling.Add(Mapper.Map(bowlingStat, new Bowling()));
                }

                var fieldingStats = session.CreateCriteria(typeof(FieldingStat))
                             .List<FieldingStat>().Where(x => x.Fixture.Id.Equals(fixtureId));

                foreach (var fieldingStat in fieldingStats)
                {
                    statsData.Fielding.Add(Mapper.Map(fieldingStat, new Fielding()));
                }
            }

            return statsData;
        }

        private void AddFieldingStats(ISession session, FixtureStats statsData, 
            IReadOnlyDictionary<string, string> paramCollection)
        {
            var fieldingStatsCriteria = session.CreateCriteria(typeof(FieldingStat));

            if (paramCollection.ContainsKey("year") || paramCollection.ContainsKey("team"))
            {
                var fixtureCriteria = fieldingStatsCriteria.CreateCriteria("Fixture", "f");

                if (paramCollection.ContainsKey("year"))
                {
                    fieldingStatsCriteria.Add(Expression.Ge("f.DatePlayed",
                        DateTime.Parse(string.Format("31/12/{0}", paramCollection["year"])).AddYears(-1)))
                        .Add(Expression.Lt("f.DatePlayed",
                            DateTime.Parse(string.Format("01/01/{0}", paramCollection["year"])).AddYears(1)));
                }

                if (paramCollection.ContainsKey("team"))
                {
                    fixtureCriteria.CreateCriteria("HomeTeam", "t");

                    fixtureCriteria.Add(Expression.Eq("t.Name", paramCollection["team"]));
                }
            }

            if (paramCollection.ContainsKey("player"))
            {
                var playerCriteria = fieldingStatsCriteria.CreateCriteria("Player", "p");

                playerCriteria.CreateCriteria("Member", "m");

                playerCriteria.Add(Expression.Like("m.MemberKey", paramCollection["player"], MatchMode.Anywhere));

            }

            var stats = fieldingStatsCriteria.List<FieldingStat>().OrderBy(x => x.Player.Member.MemberKey);

            var playerIdList = new List<int>();
            foreach (var fieldingStat in stats)
            {
                var playerId = fieldingStat.Player.Id;
                if (playerIdList.Contains(playerId))
                {
                    CalculateFieldingStat(statsData.Fielding.Last(), fieldingStat);
                }
                else
                {
                    statsData.Fielding.Add(Mapper.Map(fieldingStat, new Fielding()));
                    SetFieldingStat(fieldingStat, statsData.Fielding.Last());
                    playerIdList.Add(playerId);
                }
            }
        }

        private void AddBowingStats(ISession session, FixtureStats statsData, 
            IReadOnlyDictionary<string, string> paramCollection)
        {
            var bowlingStatsCriteria = session.CreateCriteria(typeof(BowlingStat));

            if (paramCollection.ContainsKey("year") || paramCollection.ContainsKey("team"))
            {
                var fixtureCriteria = bowlingStatsCriteria.CreateCriteria("Fixture", "f");

                if (paramCollection.ContainsKey("year"))
                {
                    bowlingStatsCriteria.Add(Expression.Ge("f.DatePlayed",
                        DateTime.Parse(string.Format("31/12/{0}", paramCollection["year"])).AddYears(-1)))
                        .Add(Expression.Lt("f.DatePlayed",
                            DateTime.Parse(string.Format("01/01/{0}", paramCollection["year"])).AddYears(1)));
                }

                if (paramCollection.ContainsKey("team"))
                {
                    fixtureCriteria.CreateCriteria("HomeTeam", "t");

                    fixtureCriteria.Add(Expression.Eq("t.Name", paramCollection["team"]));
                }
            }

            if (paramCollection.ContainsKey("player"))
            {
                var playerCriteria = bowlingStatsCriteria.CreateCriteria("Player", "p");

                playerCriteria.CreateCriteria("Member", "m");

                playerCriteria.Add(Expression.Like("m.MemberKey", paramCollection["player"], MatchMode.Anywhere));

            }

            var stats = bowlingStatsCriteria.List<BowlingStat>().OrderBy(x => x.Player.Member.MemberKey);

            var playerIdList = new List<int>();
            foreach (var bowlingStat in stats)
            {
                var playerId = bowlingStat.Player.Id;
                if (playerIdList.Contains(playerId))
                {
                    CalculateBowlingStat(statsData.Bowling.Last(), bowlingStat);
                }
                else
                {
                    statsData.Bowling.Add(Mapper.Map(bowlingStat, new Bowling()));
                    SetBowlingStat(statsData.Bowling.Last());
                    playerIdList.Add(playerId);
                }
            }
        } 

        private void AddBattingStats(ISession session, FixtureStats statsData,
            IReadOnlyDictionary<string, string> paramCollection)
        {
            var battingStatsCriteria = session.CreateCriteria(typeof (BattingStat));

            if (paramCollection.ContainsKey("year") || paramCollection.ContainsKey("team"))
            {
                var fixtureCriteria = battingStatsCriteria.CreateCriteria("Fixture", "f");

                if (paramCollection.ContainsKey("year"))
                {
                    battingStatsCriteria.Add(Expression.Ge("f.DatePlayed",
                        DateTime.Parse(string.Format("31/12/{0}", paramCollection["year"])).AddYears(-1)))
                        .Add(Expression.Lt("f.DatePlayed",
                            DateTime.Parse(string.Format("01/01/{0}", paramCollection["year"])).AddYears(1)));
                }

                if (paramCollection.ContainsKey("team"))
                {
                    fixtureCriteria.CreateCriteria("HomeTeam", "t");

                    fixtureCriteria.Add(Expression.Eq("t.Name", paramCollection["team"]));
                }
            }

            if (paramCollection.ContainsKey("player"))
            {
                var playerCriteria = battingStatsCriteria.CreateCriteria("Player", "p");

                playerCriteria.CreateCriteria("Member", "m");

                playerCriteria.Add(Expression.Like("m.MemberKey", paramCollection["player"], MatchMode.Anywhere));

            }

            var stats = battingStatsCriteria.List<BattingStat>().OrderBy(x => x.Player.Member.MemberKey);

            var playerIdList = new List<int>();
            foreach (var battingStat in stats)
            {
                var playerId = battingStat.Player.Id;
                if (playerIdList.Contains(playerId))
                {
                    CalculateBattingStat(statsData.Batting.Last(), battingStat);
                }
                else
                {
                    statsData.Batting.Add(Mapper.Map(battingStat, new Batting()));
                    SetBattingStat(battingStat, statsData.Batting.Last());
                    playerIdList.Add(playerId);
                }
            }
        }

        private void CalculateFieldingStat(Fielding lastFieldingStat, FieldingStat fieldingStat)
        {
            SetFieldingStat(fieldingStat, lastFieldingStat);
     
            lastFieldingStat.Catches += fieldingStat.Catches;
            lastFieldingStat.Stumpings += fieldingStat.Stumpings;
        }

        private void CalculateBowlingStat(Bowling lastBowlingStat, BowlingStat bowlingStat)
        {
            lastBowlingStat.Runs += bowlingStat.Runs;
            lastBowlingStat.Wickets += bowlingStat.Wickets;
            lastBowlingStat.Maidens += bowlingStat.Maidens;
            lastBowlingStat.OversDec += bowlingStat.Overs;

            SetBowlingStat(lastBowlingStat);
         
        }   

        private void CalculateBattingStat(Batting lastBattingStat, BattingStat battingStat)
        {
            if (battingStat.Runs > lastBattingStat.Highest) lastBattingStat.Highest = battingStat.Runs;

            lastBattingStat.Runs += battingStat.Runs;

            SetBattingStat(battingStat, lastBattingStat);
        }

        private void SetFieldingStat(FieldingStat fieldingStat, Fielding lastFieldingStat)
        {
            if (fieldingStat.Catches > lastFieldingStat.MostCatches) lastFieldingStat.MostCatches = fieldingStat.Catches;
            if (fieldingStat.Stumpings > lastFieldingStat.MostStumpings) lastFieldingStat.MostStumpings = fieldingStat.Stumpings;

        }

        private void SetBowlingStat(Bowling lastBowlingStat)
        {
            if (lastBowlingStat.Wickets == 0)
            {
                lastBowlingStat.EconomyDec = Convert.ToDecimal(lastBowlingStat.Runs);
                lastBowlingStat.StrikeDec = Convert.ToDecimal(lastBowlingStat.Runs);
            }
            else
            {
                if (lastBowlingStat.OversDec > 0)
                {
                    lastBowlingStat.EconomyDec = lastBowlingStat.Runs/Convert.ToDecimal(lastBowlingStat.OversDec);
                }
                if (lastBowlingStat.Wickets > 0)
                {
                    lastBowlingStat.StrikeDec = lastBowlingStat.Runs/Convert.ToDecimal(lastBowlingStat.Wickets);
                }
            }
        }

        private void SetBattingStat(BattingStat battingStat, Batting lastBattingStat)
        {
            lastBattingStat.Innings++;
            if (lastBattingStat.Dismissals == 0)
                lastBattingStat.AverageDec = Convert.ToDecimal(lastBattingStat.Runs);

            if (battingStat.HowOut.Name != "Not Out")
            {
                lastBattingStat.Dismissals++;
                lastBattingStat.AverageDec = Convert.ToDecimal(lastBattingStat.Runs) / lastBattingStat.Dismissals;
            }
        }
    }
}   