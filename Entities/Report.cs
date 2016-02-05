
using FluentNHibernate.Mapping;

namespace ivNet.Club.Entities
{
     public class Report : BaseEntity
    {
        public virtual string Html { get; set; }
        public virtual Fixture Fixture { get; set; }

    }

    public class ReportMap : ClassMap<Report>
    {
        public ReportMap()
        {
            Id(x => x.Id);

            Map(x => x.Html).Nullable().Length(4500);

            References(x => x.Fixture);

            Map(x => x.IsActive);
            Map(x => x.CreatedBy).Not.Nullable().Length(50);
            Map(x => x.CreateDate).Not.Nullable();
            Map(x => x.ModifiedBy).Not.Nullable().Length(50);
            Map(x => x.ModifiedDate).Not.Nullable();
        }
    }
}
