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
            moddedPlayer.Update();
        }
    }
}