
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace ivNet.Club.Entities
{
    public class Player : BaseEntity
    {
        public virtual int PlayerType { get; set; }
        public virtual string Nickname { get; set; }

        public virtual Member Member { get; set; }

        public virtual IList<Guardian> Guardians { get; set; }
        public virtual IList<Team> Teams { get; set; }

        public virtual void Init()
        {
            Guardians = new List<Guardian>();
            Teams = new List<Team>();
        }
    }

    public class PlayerMap : ClassMap<Player>
    {
        public PlayerMap()
        {
            Id(x => x.Id);

            Map(x => x.PlayerType);
            Map(x => x.Nickname);

            References(x => x.Member);

            HasManyToMany(x => x.Guardians)
                .Inverse()
                .Cascade.SaveUpdate()
                .Table("ivNetGuardianPlayer");

            HasManyToMany(x => x.Teams)
                .Inverse()
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