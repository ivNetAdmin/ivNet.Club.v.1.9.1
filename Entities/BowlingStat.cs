
using FluentNHibernate.Mapping;

namespace ivNet.Club.Entities
{
    public class BowlingStat : BaseEntity
    {
        public virtual decimal Overs { get; set; }
        public virtual int Maidens { get; set; }
        public virtual int Runs { get; set; }
        public virtual int Wickets { get; set; }

        public virtual Player Player { get; set; }
        public virtual Fixture Fixture { get; set; }
    }

    public class BowlingStatMap : ClassMap<BowlingStat>
    {
        public BowlingStatMap()
        {
            Id(x => x.Id);

            Map(x => x.Overs);
            Map(x => x.Maidens);
            Map(x => x.Runs);
            Map(x => x.Wickets);

            References(x => x.Player);
            References(x => x.Fixture);

            Map(x => x.IsActive);
            Map(x => x.CreatedBy).Not.Nullable().Length(50);
            Map(x => x.CreateDate).Not.Nullable();
            Map(x => x.ModifiedBy).Not.Nullable().Length(50);
            Map(x => x.ModifiedDate).Not.Nullable();

        }
    }
}

