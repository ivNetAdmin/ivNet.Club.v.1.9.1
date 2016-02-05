
using FluentNHibernate.Mapping;

namespace ivNet.Club.Entities
{
    public class Address : BaseEntity
    {
        public virtual string AddressKey { get; set; }

        public virtual string AddressLine { get; set; }
        public virtual string Postcode { get; set; }
    }

    public class AddressMap : ClassMap<Address>
    {
        public AddressMap()
        {
            Id(x => x.Id);

            Map(x => x.AddressKey).Not.Nullable().Length(120).UniqueKey("ix_Address_Unique");

            Map(x => x.AddressLine);
            Map(x => x.Postcode);

            Map(x => x.IsActive);
            Map(x => x.CreatedBy).Not.Nullable().Length(50);
            Map(x => x.CreateDate).Not.Nullable();
            Map(x => x.ModifiedBy).Not.Nullable().Length(50);
            Map(x => x.ModifiedDate).Not.Nullable();
        }
    }
}