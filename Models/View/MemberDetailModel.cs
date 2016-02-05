namespace ivNet.Club.Models.View
{
    public class MemberDetailModel
    {
        public int LegacyId { get; set; }
        public byte IsActive { get; set; }

        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Address { get; set; }
        public string Postcode { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public string Captcha { get; set; }
    }
}