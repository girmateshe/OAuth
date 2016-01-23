using System;

namespace Policy.Pets.Models
{
    public class TokenContent : Model
    {
        public object Header { get; set; }
        public object Payload { get; set; }
        public double Current { get; set; }
    }
}