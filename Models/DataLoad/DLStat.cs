
using System;

namespace ivNet.Club.Models.DataLoad
{
    public class DLStat
    {
        public int LegacyFixtureId { get; set; }
        public int LegacyMemberId { get; set; }

        public string MemberKey{ get; set; }
        public string FixtureKey{ get; set; }

        public DateTime DatePlayed { get; set; }

        public int Position { get; set; }
        public int RunsScored { get; set; }
        public decimal OversFaced { get; set; }
        public string HowOut { get; set; }
        
        public decimal OversBowled { get; set; }
        public int Maidens { get; set; }
        public int RunsConceeded { get; set; }
        public int Wickets { get; set; }

        public int Catches { get; set; }
        public int Stumpings { get; set; }
    }
}