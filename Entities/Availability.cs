
using System;
using FluentNHibernate.Mapping;

namespace ivNet.Club.Entities
{
    public class Availability : BaseEntity
    {
        public virtual DateTime Date { get; set; }
        public virtual Player Player { get; set; }
    }

    public class AvailabilityMap : ClassMap<Availability>
    {
        public AvailabilityMap()
        {
            Id(x => x.Id);

            Map(x => x.Date);

            References(x => x.Player);

            Map(x => x.IsActive);
            Map(x => x.CreatedBy).Not.Nullable().Length(50);
            Map(x => x.CreateDate).Not.Nullable();
            Map(x => x.ModifiedBy).Not.Nullable().Length(50);
            Map(x => x.ModifiedDate).Not.Nullable();
        }
    }
}