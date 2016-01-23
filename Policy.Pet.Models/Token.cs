using System;

namespace Policy.Pets.Models
{
    public class Token : Model
    {
        public string access_token { get; set; }
        public double expires_in { get; set; }
        public double expires_on { get; set; }
    }
}