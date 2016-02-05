
using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Mapping;

namespace ivNet.Club.Entities
{
    public class Guardian : BaseEntity
    {
      
        public virtual Member Member { get; set; }

        public virtual IList<Player> Wards { get; protected set; }

        public virtual void Init()
        {
            Wards = new List<Player>();
        }

        public virtual void AddWard(Player player)
        {   
            var exists = Wards.Any(ward => ward.Member.MemberKey == player.Member.MemberKey);

            if (exists) return;

            player.Guardians.Add(this);
            Wards.Add(player);
            
        }

        public virtual void RemoveWard(Player player)
        {
            player.Guardians.Remove(this);
            Wards.Remove(player);
        }
    }

    public class GuardianMap : ClassMap<Guardian>
    {
        public GuardianMap()
        {
            Id(x => x.Id);

            References(x => x.Member);

            HasManyToMany(x => x.Wards)
                .Cascade.SaveUpdate()
                .Table("ivNetGuardianPlayer");

            Map(x => x.IsActive);
            Map(x => x.CreatedBy).Not.Nullable().Length(50);
            Map(x => x.CreateDate).Not.Nullable();
            Map(x => x.ModifiedBy).Not.Nullable().Length(50);
            Map(x => x.ModifiedDate).Not.Nullable();
        }
    }
}

