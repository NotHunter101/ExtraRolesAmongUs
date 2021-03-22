using ExtraRolesMod.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExtraRolesMod.Rpc
{
    [Serializable]
    public class InitializeRoundData
    {
        public InitializeRoundData()
        {
        }

        public Dictionary<byte, Role> Roles { get; set; } = new Dictionary<byte, Role>();
    }
}
