
using FluentNHibernate.Mapping;

namespace ivNet.Club.Entities
{
    public class ResultType : BaseEntity
    {
        public virtual string Name { get; set; }
    }

    public class ResultTypeMap : ClassMap<ResultType>
    {
        public ResultTypeMap()
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