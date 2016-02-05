
using System;

namespace ivNet.Club.Models.View
{
    public class Ward
    {
        public int MemberId { get; set; }

        public string Lastname { get; set; }
        public string Firstname { get; set; }

        public string Nickname { get; set; }
        public DateTime? Dob { get; set; }

        public string DobStr
        {
            get { return Dob.GetValueOrDefault().ToString("yyyy-MM-dd"); }
        }

        public string GuardianList { get; set; }
    }
}