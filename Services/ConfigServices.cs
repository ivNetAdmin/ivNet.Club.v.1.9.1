
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using ivNet.Club.Entities;
using ivNet.Club.Enums;
using ivNet.Club.Helpers;
using ivNet.Club.Models.View;
using NHibernate.Criterion;
using Orchard;
using Orchard.Security;
using LogLevel = Orchard.Logging.LogLevel;

namespace ivNet.Club.Services
{

    public interface IConfigServices : IDependency
    {
        List<ConfigItem> GetMemberTypes();
        List<ConfigItem> GetTeamNames(TeamType teamType);
        List<ConfigItem> GetSeasons();
        List<ConfigItem> GetHowOutList();
        List<ConfigItem> StatsSelectorItems();
        List<ConfigItem> GetResultTypeList();
        
        ConfigItem SaveMemberType(ConfigItem item);
        ConfigItem SaveTeamName(ConfigItem item, out TeamName team);
        ConfigItem SaveHowOut(ConfigItem item, out HowOut howOut);

        bool DeleteMemberType(int id);
        bool DeleteTeam(int id);

        
    }

    public class ConfigServices : BaseService, IConfigServices
    {
        public ConfigServices(IAuthenticationService authenticationService) : base(authenticationService)
        {
        }

        public List<ConfigItem> GetMemberTypes()
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var itemCriteria = session.CreateCriteria(typeof(MemberType));
                itemCriteria.Add(Expression.Eq("IsActive", (byte)1));

                var items = itemCriteria.List<MemberType>();
             
                return items.Select(item =>
                    new ConfigItem { Id = item.Id, Name = item.Name }).OrderBy(x => x.Name).ToList();
            }
        }

        public List<ConfigItem> GetHowOutList()
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var items = session.CreateCriteria(typeof (HowOut))
                    .List<HowOut>().OrderBy(x => x.Name);

                return items.Select(item =>
                    new ConfigItem { Id = item.Id, Name = item.Name }).ToList();
            }
        }

        public List<ConfigItem> GetResultTypeList()
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var items = session.CreateCriteria(typeof(ResultType))
                    .List<ResultType>().OrderBy(x => x.Name);

                return items.Select(item =>
                    new ConfigItem { Id = item.Id, Name = item.Name }).ToList();
            }
        }

        public List<ConfigItem> GetTeamNames(TeamType teamType)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var itemCriteria = session.CreateCriteria(typeof(TeamName));
                var teamList = new List<TeamName>();
                switch (teamType)
                {
                    case TeamType.Home:
                    {

                        itemCriteria.Add(Expression.Like("Type", 1));
                        itemCriteria.Add(Expression.Eq("IsActive", (byte)1));
                        teamList = itemCriteria.List<TeamName>().OrderBy(x => x.Name).ToList();
                        break;
                    }
                    case TeamType.Opposition:
                    {
                        itemCriteria.Add(Expression.Like("Type", 2));
                        itemCriteria.Add(Expression.Eq("IsActive", (byte)1));
                        teamList = itemCriteria.List<TeamName>().OrderBy(x => x.Name).ToList();
                        break;
                    }
                    default:
                    itemCriteria.Add(Expression.Eq("IsActive", (byte)1));
                    teamList = itemCriteria.List<TeamName>().OrderBy(x=>x.Name).ToList();
                        break;
                }

                return (from item in teamList where !string.IsNullOrEmpty(item.Name) select new ConfigItem {Id = item.Id, Name = item.Name}).ToList();
            }
        }

        public List<ConfigItem> GetSeasons()
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var seasonList = new List<ConfigItem>();
                // get oldest fixture
                var fixture = session.CreateCriteria(typeof (Fixture))
                    .List<Fixture>().OrderBy(x => x.DatePlayed).FirstOrDefault();

                if (fixture != null)
                {
                    for (var i = DateTime.Now.Year; i >= fixture.DatePlayed.Year; i--)
                    {
                        seasonList.Add(new ConfigItem { Id = i,Name=i.ToString(CultureInfo.InvariantCulture) });
                    }
                }
                else
                {
                    seasonList.Add(new ConfigItem { Id = DateTime.Now.Year, Name = DateTime.Now.Year.ToString(CultureInfo.InvariantCulture) });
                }
                return seasonList;
            }            
        }

        public List<ConfigItem> StatsSelectorItems()
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var items = session.CreateCriteria(typeof (TeamName))
                    .List<TeamName>().Where(x => x.Type == (int) TeamType.Home);

                var statsSelectorItems = items.Select(team =>
                    new ConfigItem {Id = team.Id, Name = team.Name, Type = "team"}).OrderBy(x => x.Name).ToList();

                for (var i = 2000; i <= DateTime.Now.Year; i++)
                {
                    statsSelectorItems.Add(new ConfigItem
                    {
                        Id=i,
                        Name = i.ToString(CultureInfo.InvariantCulture),
                        Type = "year"
                    });
                }

                return statsSelectorItems;
            }
        }

        public ConfigItem SaveMemberType(ConfigItem newItem)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var item = session.CreateCriteria(typeof (MemberType))
                            .List<MemberType>()
                            .FirstOrDefault(x => x.Name.ToLowerInvariant() == newItem.Name.ToLowerInvariant()) ??
                                   new MemberType {Name = newItem.Name};

                        if (item.Id != 0)
                        {
                            transaction.Rollback();
                            return null;
                        }

                        SetAudit(item);
                        session.SaveOrUpdate(item);
                        transaction.Commit();

                        newItem.Id = item.Id;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, ex, string.Empty, null);
                        transaction.Rollback();
                    }
                    return newItem;
                }
            }
        }

        public ConfigItem SaveTeamName(ConfigItem newItem, out TeamName team)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var item = session.CreateCriteria(typeof(TeamName))
                            .List<TeamName>()
                            .FirstOrDefault(x => x.Name.ToLowerInvariant() == newItem.Name.ToLowerInvariant()) ??
                                   new TeamName { Name = newItem.Name };

                        if (item.Id != 0)
                        {
                            transaction.Rollback();
                            team = item;
                            return null;
                        }

                        item.Type = newItem.Type == "Opposition" ? (int)TeamType.Opposition : (int)TeamType.Home;
                      
                        SetAudit(item);
                        session.SaveOrUpdate(item);
                        transaction.Commit();

                        team = item;
                        newItem.Id = item.Id;
                    }
                    catch (Exception ex)
                    {
                        team = null;
                        Logger.Log(LogLevel.Error, ex, string.Empty, null);
                        transaction.Rollback();
                    }
                    return newItem;
                }
            }
        }

        public ConfigItem SaveHowOut(ConfigItem newItem, out HowOut howOut)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var item = session.CreateCriteria(typeof(HowOut))
                            .List<HowOut>()
                            .FirstOrDefault(x => x.Name.ToLowerInvariant() == newItem.Name.ToLowerInvariant()) ??
                                   new HowOut { Name = newItem.Name };

                        if (item.Id != 0)
                        {
                            transaction.Rollback();
                            howOut = item;
                            return null;
                        }

                        SetAudit(item);
                        session.SaveOrUpdate(item);
                        transaction.Commit();

                        howOut = item;
                        newItem.Id = item.Id;
                    }
                    catch (Exception ex)
                    {
                        howOut = null;
                        Logger.Log(LogLevel.Error, ex, string.Empty, null);
                        transaction.Rollback();
                    }
                    return newItem;
                }
            }
        }

        public bool DeleteMemberType(int id)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try { 
                    var item = session.CreateCriteria(typeof (MemberType))
                        .List<MemberType>()
                        .FirstOrDefault(x => x.Id == id);

                    //session.Delete(item);

                    if (item != null)
                    {
                        SetAudit(item);
                        item.IsActive = 0;
                        session.SaveOrUpdate(item);
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

        public bool DeleteTeam(int id)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var item = session.CreateCriteria(typeof(Team))
                            .List<Team>()
                            .FirstOrDefault(x => x.Id == id);
                                               
                        if (item != null)
                        {
                            SetAudit(item);
                            item.IsActive = 0;
                            session.SaveOrUpdate(item);
                        }

                        //session.Delete(item);
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
    }
}