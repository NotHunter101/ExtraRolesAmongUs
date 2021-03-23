﻿namespace ExtraRoles2.Classes
{
    public static class PlayerControlExtensions
    {
        public static Player GetModdedPlayer(this PlayerControl player)
        {
            return Main.Instance.Players.Find(x => x.Owner == player);
        }
    }
}