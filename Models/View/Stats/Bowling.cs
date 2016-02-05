
using System;

namespace ivNet.Club.Models.View.Stats
{
    public class Bowling
    {
        public int PlayerId { get; set; }
        public string Name { get; set; }

        public decimal OversDec { get; set; }
        public int Maidens { get; set; }
        public int Runs { get; set; }
        public int Wickets { get; set; }
        public decimal EconomyDec   { get; set; }
        public decimal StrikeDec { get; set; }

        public decimal Overs
        {
            get
            {
                return decimal.Round(OversDec, 2, MidpointRounding.AwayFromZero);      
            }
        }

        public decimal Economy
        {
            get
            {
                return decimal.Round(EconomyDec, 2, MidpointRounding.AwayFromZero);      
            }
        }

        public decimal Strike
        {
            get
            {
                return decimal.Round(StrikeDec, 1, MidpointRounding.AwayFromZero);      
            }
        }
    }
}