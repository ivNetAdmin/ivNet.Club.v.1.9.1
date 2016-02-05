
using System;
using System.Collections.Generic;

namespace ivNet.Club.Models.View
{
    public class ReportDetail
    {
        public int FixtureId { get; set; }
        public string Html { get; set; }
        public string Result { get; set; }
        public string ResultType { get; set; }
        public List<ConfigItem> ResultTypeList { get; set; }
        public DateTime DatePlayed { get; set; }
        public string HomeTeam { get; set; }
        public string Opposition { get; set; }
    }
}