
namespace ivNet.Club.Models.View.Stats
{
    public class Fielding
    {
        public int PlayerId { get; set; }
        public string Name { get; set; }

        public decimal MostCatches { get; set; }
        public decimal MostStumpings { get; set; }
        public int Catches { get; set; }
        public int Stumpings { get; set; }
    }
}