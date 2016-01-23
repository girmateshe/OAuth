using System;

namespace Policy.Pets.Models
{
    public class TokenRequest
    {
        public string username { get; set; }
        public string password { get; set; }
        public string grant_type { get; set; }
    }
}