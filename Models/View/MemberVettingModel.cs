
using System;

namespace ivNet.Club.Models.View
{
    public class MemberVettingModel
    {
        public int MemberId { get; set; }   

        public string Name { get; set; }

        public virtual DateTime? CreateDate { get; set; }
    }
}