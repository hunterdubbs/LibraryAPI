using System;

namespace LibraryAPI.Domain
{
    public class EmailVerificationCode
    {
        public int ID { get; set; }
        public string UserID { get; set; }
        public string Code { get; set; }
        public DateTime Sent { get; set; }
        public bool IsVerified { get; set; }
    }
}
