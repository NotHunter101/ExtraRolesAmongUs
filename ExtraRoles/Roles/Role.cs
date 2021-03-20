using System;
using System.Collections.Generic;
using System.Text;

namespace ExtraRoles.Roles
{
    public enum Role : byte
    {
        Joker = 0,
        Officer = 1,
        Engineer = 2,
        Medic = 3,

        // non rpc roles
        Crewmate,
        Impostor
    }
}
