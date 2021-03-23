namespace ExtraRoles2.Classes
{
    public class RpcRole
    {
        public RoleId RoleId { get; set; }
        public PlayerControl RolePlayer { get; set; }

        public RpcRole(RoleId roleId, PlayerControl rolePlayer)
        {
            RoleId = roleId;
            RolePlayer = rolePlayer;
        }
    }
}