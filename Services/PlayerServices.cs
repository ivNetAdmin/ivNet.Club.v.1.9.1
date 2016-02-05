using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.ModelBinding;
using AutoMapper;
using ivNet.Club.Entities;
using ivNet.Club.Enums;
using ivNet.Club.Helpers;
using ivNet.Club.Models.View;
using Orchard;
using Orchard.Caching;
using Orchard.Security;
using Orchard.Logging;

public interface IPlayerServices : IDependency
{
    Player GetPlayerByName(string lastname, string firstname);
    List<PlayerDetail> GetAllActivePlayers();
    List<PlayerDetail> GetAvailablePlayers(Dictionary<string, string> paramCollection);
}

namespace ivNet.Club.Services
{
    public class PlayerServices : BaseService, IPlayerServices
    {
        private readonly ICacheManager _cacheManager;

        public PlayerServices(IAuthenticationService authenticationService, ICacheManager cacheManager)
            : base(authenticationService)
        {
            _cacheManager = cacheManager;
        }

        public Player GetPlayerByName(string lastname, string firstname)
        {

            var playerKey =
                CustomStringHelper.BuildKey(new[]
                {
                    lastname,
                    firstname
                });

            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var player = CheckPlayerExists(session, playerKey);

                        if (player == null)
                        {
                            transaction.Rollback();
                            return null;
                        }
                        if (player.PlayerType == 0) player.PlayerType = (int) PlayerType.Senior;

                        if (player.Id != 0) return player;

                        SetAudit(player);
                        session.SaveOrUpdate(player);

                        transaction.Commit();
                        return player;
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

        public List<PlayerDetail> GetAllActivePlayers()
        {
            if (HttpContext.Current.Session["PlayerList"] == null)
            {
                using (var session = NHibernateHelper.OpenSession())
                {
                    var players = session.CreateCriteria(typeof (Player))
                        .List<Player>()
                        .Where(x => x.Member.IsActive.Equals(1))
                        .OrderBy(x => x.Member.Lastname)
                        .ThenBy(x => x.Member.Firstname);

                    HttpContext.Current.Session["PlayerList"] =
                        players.Select(player => Mapper.Map(player, new PlayerDetail())).ToList();
                    
                }
            }
            return (List<PlayerDetail>)HttpContext.Current.Session["PlayerList"];
        }

        public List<PlayerDetail> GetAvailablePlayers(Dictionary<string, string> paramCollection)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                DateTime date;
                DateTime.TryParse(paramCollection["date"], out date);

                DateTime.TryParse("2016-05-08T00:00:00", out date);       

                var players = session.CreateCriteria(typeof(Availability))
                    .List<Availability>()
                    .Where(x => x.Player.Member.IsActive.Equals(1) && x.Date.Equals(date))
                    .OrderBy(x => x.Player.Member.Lastname)
                    .ThenBy(x => x.Player.Member.Firstname);

                return players.Select(availability => Mapper.Map(availability.Player, new PlayerDetail())).ToList();

            }
        }
    }
}