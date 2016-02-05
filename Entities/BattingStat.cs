
using FluentNHibernate.Mapping;

namespace ivNet.Club.Entities
{
    public class BattingStat : BaseEntity
    {
        public virtual int Position { get; set; }
        public virtual int Runs { get; set; }
        public virtual decimal Overs { get; set; }

        public virtual Player Player { get; set; }
        public virtual Fixture Fixture { get; set; }
        public virtual HowOut HowOut { get; set; }

    }

    public class BattingStatMap : ClassMap<BattingStat>
    {
        public BattingStatMap()
        {
            Id(x => x.Id);

            Map(x => x.Position);
            Map(x => x.Runs);
            Map(x => x.Overs);

            References(x => x.Player);
            References(x => x.Fixture);
            References(x => x.HowOut);

            Map(x => x.IsActive);
            Map(x => x.CreatedBy).Not.Nullable().Length(50);
            Map(x => x.CreateDate).Not.Nullable();
            Map(x => x.ModifiedBy).Not.Nullable().Length(50);
            Map(x => x.ModifiedDate).Not.Nullable();
        }
    }
}
