using System;

namespace Policy.Pets.Models
{
    public class User : Model
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}