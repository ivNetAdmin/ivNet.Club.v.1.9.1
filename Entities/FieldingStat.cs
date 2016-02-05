
using FluentNHibernate.Mapping;

namespace ivNet.Club.Entities
{
    public class FieldingStat : BaseEntity
    {
        public virtual int Catches { get; set; }
        public virtual int Stumpings { get; set; }

        public virtual Player Player { get; set; }
        public virtual Fixture Fixture { get; set; }
    }

    public class FieldingStatMap : ClassMap<FieldingStat>
    {
        public FieldingStatMap()
        {
            Id(x => x.Id);

            Map(x => x.Catches);
            Map(x => x.Stumpings);

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

