using System;

namespace LibraryAPI.Domain
{
    public class PasswordResetCode
    {
        public int ID { get; set; }
        public string UserID { get; set; }
        public string Hash { get; set; }
        public string Salt { get; set; }
        public DateTime Expires { get; set; }
    }
}
