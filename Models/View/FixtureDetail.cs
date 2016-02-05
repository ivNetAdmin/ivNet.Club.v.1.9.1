
using System;
using System.Collections.Generic;
using ivNet.Club.Entities;
using ivNet.Club.Models.View.Stats;

namespace ivNet.Club.Models.View
{
    public class FixtureDetail
    {
        public int FixtureId { get; set; }
        public string Player { get; set; }
        public DateTime DatePlayed { get; set; }
        public string Result { get; set; }
        public string ResultType { get; set; }
        public string HomeTeam { get; set; }
        public string Opposition { get; set; }

        public FixtureStats FixtureStats { get; set; }

        public List<string> TeamPlayerList { get; set; }
        public List<ConfigItem> HowOutList { get; set; }
        public List<ConfigItem> ResultTypeList { get; set; }
        public List<PlayerDetail> PlayerList { get; set; }
        public List<ConfigItem> OppositionList { get; set; }
    }
}