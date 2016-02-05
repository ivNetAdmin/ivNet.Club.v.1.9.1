
using System;

namespace ivNet.Club.Models.DataLoad
{
    public class DLFixture
    {
        public int LegacyId { get; set; }
        public DateTime DatePlayed { get; set; }
        public string ResultType { get; set; }
        public string Team { get; set; }
        public string Opposition { get; set; }
        public string Result { get; set; }
        public string Venue { get; set; }
        public string Competition { get; set; }
    }
}