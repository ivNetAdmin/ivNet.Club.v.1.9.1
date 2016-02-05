
using System.Collections.Generic;
using FluentNHibernate.Mapping;
using ivNet.Club.Entities;

namespace ivNet.Club.Entities
{
    public class MemberType : BaseEntity
    {
        public virtual string Name { get; set; }
        public virtual IList<Member> Members { get; set; }

        public virtual void Init()
        {
            Members = new List<Member>();
        }
    }
}

public class MemberTypeMap : ClassMap<MemberType>
{
    public MemberTypeMap()
    {
        Id(x => x.Id);

        Map(x => x.Name);

        HasManyToMany(x => x.Members)
            .Inverse()
            .Cascade.SaveUpdate()
            .Table("ivNetMemberTypeMember");

        Map(x => x.IsActive);
        Map(x => x.CreatedBy).Not.Nullable().Length(50);
        Map(x => x.CreateDate).Not.Nullable();
        Map(x => x.ModifiedBy).Not.Nullable().Length(50);
        Map(x => x.ModifiedDate).Not.Nullable();
    }
}

