
using FluentNHibernate.Mapping;

namespace ivNet.Club.Entities
{
    public class Contact : BaseEntity
    {
        public virtual string ContactKey { get; set; }

        public virtual string Email { get; set; }
        public virtual string Phone { get; set; }
    }

    public class ContactMap : ClassMap<Contact>
    {
        public ContactMap()
        {
            Id(x => x.Id);

            Map(x => x.ContactKey).Not.Nullable().Length(120).UniqueKey("ix_Contact_Unique");

            Map(x => x.Email);
            Map(x => x.Phone);

            Map(x => x.IsActive);
            Map(x => x.CreatedBy).Not.Nullable().Length(50);
            Map(x => x.CreateDate).Not.Nullable();
            Map(x => x.ModifiedBy).Not.Nullable().Length(50);
            Map(x => x.ModifiedDate).Not.Nullable();
        }
    }
}