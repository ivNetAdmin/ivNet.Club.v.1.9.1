
using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Mapping;

namespace ivNet.Club.Entities
{
    public class Team : BaseEntity
    {
        public virtual Fixture Fixture { get; set; }
        public virtual IList<Player> Players { get; set; }

        public virtual void Init()
        {
            Players = new List<Player>();
        }

        public virtual void AddPlayer(Player player)
        {
            var exists = Players.Any(teamPlayer => teamPlayer.Member.MemberKey == player.Member.MemberKey);

            if (exists) return;
            player.Teams.Add(this);
            Players.Add(player);
        }

        public virtual void RemovePlayer(Player player)
        {
            player.Teams.Remove(this);
            Players.Remove(player);
        }
    }

    public class TeamMap : ClassMap<Team>
    {   
        public TeamMap()
        {
            Id(x => x.Id);

            References(x => x.Fixture);

            HasManyToMany(x => x.Players)
                .Cascade.SaveUpdate()
                .Table("ivNetTeamPlayer");

            Map(x => x.IsActive);
            Map(x => x.CreatedBy).Not.Nullable().Length(50);
            Map(x => x.CreateDate).Not.Nullable();
            Map(x => x.ModifiedBy).Not.Nullable().Length(50);
            Map(x => x.ModifiedDate).Not.Nullable();
        }
    }
}