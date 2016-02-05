
using FluentNHibernate.Mapping;

namespace ivNet.Club.Entities
{
    public class TeamName : BaseEntity
    {
        public virtual string Name { get; set; }
        public virtual int Type { get; set; }
    }

    public class TeamNameMap : ClassMap<TeamName>
    {
        public TeamNameMap()
        {
            Id(x => x.Id);

            Map(x => x.Name);
            Map(x => x.Type);

            Map(x => x.IsActive);
            Map(x => x.CreatedBy).Not.Nullable().Length(50);
            Map(x => x.CreateDate).Not.Nullable();
            Map(x => x.ModifiedBy).Not.Nullable().Length(50);
            Map(x => x.ModifiedDate).Not.Nullable();
        }
    }
}