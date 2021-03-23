using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExtraRoles2.Classes
{
    public class Main
    {
        public static Main Instance { get; set; }
        public List<Player> Players { get; }

        public Main()
        {
            Players = new List<Player>();
        }

        public void Reset()
        {
            Players.Clear();
        }

        public void Update()
        {
            if (!AmongUsClient.Instance.IsGameStarted)
                Reset();
            else
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    if (Players.All(x => x.Owner != player))
                        Players.Add(new Player(player));

            if (PlayerControl.LocalPlayer.GetModdedPlayer() == null) return;
            
            Player moddedPlayer = PlayerControl.LocalPlayer.GetModdedPlayer();
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.GetModdedPlayer() == null)
                    continue;
                if (player.AmOwner || moddedPlayer.Owner.Data.IsDead)
                {
                    if (player.Data.IsImpostor && player.GetModdedPlayer().Role == null)
                        moddedPlayer.SetNameColor(player, Palette.ImpostorRed);
                    else if (player.GetModdedPlayer().Role != null)
                        moddedPlayer.SetNameColor(player, player.GetModdedPlayer().Role.Color);
                }
            }
            moddedPlayer.Role?.Update();
        }
    }
}