
using FluentNHibernate.Mapping;

namespace ivNet.Club.Entities
{
    public class Venue : BaseEntity
    {
        public virtual string Name { get; set; }
    }

    public class VenueMap : ClassMap<Venue>
    {
        public VenueMap()
        {
            Id(x => x.Id);

            Map(x => x.Name);

            Map(x => x.IsActive);
            Map(x => x.CreatedBy).Not.Nullable().Length(50);
            Map(x => x.CreateDate).Not.Nullable();
            Map(x => x.ModifiedBy).Not.Nullable().Length(50);
            Map(x => x.ModifiedDate).Not.Nullable();
        }
    }
}