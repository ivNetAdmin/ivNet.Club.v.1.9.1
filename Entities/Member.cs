
using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace ivNet.Club.Entities
{
    public class Member : BaseEntity
    {
        public virtual string MemberKey { get; set; }

        public virtual string Lastname { get; set; }
        public virtual string Firstname { get; set; }
       
        public virtual int LegacyId { get; set; }
        public virtual int UserId { get; set; }

        public virtual DateTime? Dob { get; set; }

        public virtual Contact Contact { get; set; }
        public virtual Address Address { get; set; }

        public virtual IList<MemberType> Types { get; set; }

        public virtual void AddMemberType(MemberType memberType)
        {
            if (Types.Contains(memberType)) return;
            memberType.Members.Add(this);
            Types.Add(memberType);
        }

        public virtual void RemoveMemberType(MemberType memberType)
        {
            memberType.Members.Remove(this);
            Types.Remove(memberType);
        }
    }

    public class MemberMap : ClassMap<Member>
    {
        public MemberMap()
        {
            Id(x => x.Id);

            Map(x => x.LegacyId);
            Map(x => x.MemberKey).Not.Nullable().Length(120).UniqueKey("ix_Member_Unique");
            Map(x => x.Lastname);
            Map(x => x.Firstname);
            Map(x => x.Dob);
            Map(x => x.UserId);

            References(x => x.Contact);
            References(x => x.Address);

            HasManyToMany(x => x.Types)
                 .Cascade.SaveUpdate()
                 .Table("ivNetMemberTypeMember");

            Map(x => x.IsVetted);

            Map(x => x.IsActive);
            Map(x => x.CreatedBy).Not.Nullable().Length(50);
            Map(x => x.CreateDate).Not.Nullable();
            Map(x => x.ModifiedBy).Not.Nullable().Length(50);
            Map(x => x.ModifiedDate).Not.Nullable();
        }
    }
}
