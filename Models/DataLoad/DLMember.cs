
using System;

namespace ivNet.Club.Models.DataLoad
{
    public class DLMember
    {
        public int LegacyId { get; set; }
        public string Lastname { get; set; }
        public string Firstname { get; set; }
        public DateTime? Dob { get; set; }
    }
}