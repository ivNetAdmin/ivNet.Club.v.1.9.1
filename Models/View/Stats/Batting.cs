
using System;

namespace ivNet.Club.Models.View.Stats
{
    public class Batting
    {
        public int PlayerId { get; set; }
        public string Name { get; set; }

        public int Position { get; set; }
        public int Runs { get; set; }
        public decimal Overs { get; set; }

        public string HowOut { get; set; }

        public int Innings { get; set; }
        public int Dismissals { get; set; }
        public decimal AverageDec { get; set; }

        public decimal Average
        {
            get
            {
                return decimal.Round(AverageDec, 2, MidpointRounding.AwayFromZero);      
            }
        }

        public int Highest { get; set; }
    }
}