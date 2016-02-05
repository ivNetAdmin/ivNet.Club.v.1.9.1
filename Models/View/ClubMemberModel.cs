using System.Collections.Generic;

namespace ivNet.Club.Models.View
{
    public class ClubMemberModel
    {
        public int MemberId { get; set; }

        public string Lastname { get; set; }
        public string Firstname { get; set; }

        public string Email { get; set; }
        public string Phone { get; set; }

        public string AddressLine { get; set; }
        public string Postcode { get; set; }

        public string Nickname { get; set; }
        public string PlayerType { get; set; }

        public List<string> MemberTypList { get; set; }

        public List<Ward> Wards { get; set; }
       
        public ClubMemberModel()
        {
            MemberTypList=new List<string>();
            Wards = new List<Ward>();
        }
    }
}