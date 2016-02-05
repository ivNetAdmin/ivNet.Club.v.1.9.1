
using System.Collections.Generic;

namespace ivNet.Club.Models.View.Stats
{
    public class FixtureStats
    {
        public List<Batting> Batting { get; set; }
        public List<Bowling> Bowling { get; set; }
        public List<Fielding> Fielding { get; set; }

        public FixtureStats()
        {
            Batting = new List<Batting>();
            Bowling = new List<Bowling>();
            Fielding = new List<Fielding>();
        }
    }
}