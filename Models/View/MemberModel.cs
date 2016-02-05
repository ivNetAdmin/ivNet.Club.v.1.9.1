
using System;

namespace ivNet.Club.Models.View
{
    public class MemberModel
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public string Name { get; set; }
        public string Dob { get; set; }
        public string PlayerType { get; set; }
        public byte IsActive { get; set; }
        public string WardList { get; set; }
        public string MemberTypeList { get; set; }
    }
}