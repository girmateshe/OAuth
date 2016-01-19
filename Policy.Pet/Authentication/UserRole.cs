using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Policy.Pets.Authentication
{
    public enum UserRole
    {
        None = -1,
        ReadOnly = 10,
        Operator = 20,
        Editor = 30,
        Administrator = 40,
    }
}