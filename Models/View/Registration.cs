
using System.Collections.Generic;

namespace ivNet.Club.Models.View
{
    public class Registration
    {
        public Registration()
        {
            Roles=new Roles();
            Wards = new List<Ward>();
        }
        public string Lastname { get; set; }
        public string Firstname { get; set; }
        public string Nickname { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Postcode { get; set; }

        public Roles Roles { get; set; }
        public List<Ward> Wards { get; set; }
    }
}
