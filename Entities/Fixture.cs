
using System;
using FluentNHibernate.Mapping;

namespace ivNet.Club.Entities
{
    public class Fixture : BaseEntity
    {
        public virtual string FixtureKey { get; set; }

        public virtual int LegacyId { get; set; }
        public virtual DateTime DatePlayed { get; set; }
        public virtual string Result { get; set; }
        
        public virtual Team Team { get; set; }
        public virtual TeamName Opposition { get; set; }
        public virtual TeamName HomeTeam { get; set; }
        public virtual ResultType ResultType { get; set; }
        public virtual Venue Venue { get; set; }
        public virtual Competition Competition { get; set; }        
        
    }

    public class FixtureMap : ClassMap<Fixture>
    {
        public FixtureMap()
        {
            Id(x => x.Id);

            Map(x => x.LegacyId);
            Map(x => x.FixtureKey).Not.Nullable().Length(120).UniqueKey("ix_Fixture_Unique");

            Map(x => x.DatePlayed);
            Map(x => x.Result);

            References(x => x.Team);
            References(x => x.HomeTeam);
            References(x => x.Opposition);
            References(x => x.ResultType);
            References(x => x.Venue);
            References(x => x.Competition);

            Map(x => x.IsActive);
            Map(x => x.CreatedBy).Not.Nullable().Length(50);
            Map(x => x.CreateDate).Not.Nullable();
            Map(x => x.ModifiedBy).Not.Nullable().Length(50);
            Map(x => x.ModifiedDate).Not.Nullable();
        }
    }
}