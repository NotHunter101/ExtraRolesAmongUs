using System.Linq;
using UnityEngine;

namespace ExtraRoles2.Classes
{
    public class Player
    {
        public PlayerControl Owner { get; set; }
        public Role Role { get; set; }
        public bool Immortal { get; set; }

        public Player(PlayerControl player)
        {
            Owner = player;
            Immortal = false;
        }

        public void Update()
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.GetModdedPlayer() == null) continue;
                if (!player.AmOwner && !Owner.Data.IsDead) continue;
                if (player.Data.IsImpostor && player.GetModdedPlayer().Role == null)
                    SetNameColor(player, Palette.ImpostorRed);
                else if (player.GetModdedPlayer().Role != null)
                    SetNameColor(player, player.GetModdedPlayer().Role.Color);
            }
            
            foreach (Player player in Main.Instance.Players)
            {
                if (!player.Immortal) continue;

                player.SetOutline(new Color(0f, 252f / 255f, 88 / 255f));
                Player Medic = Main.Instance.Players.Find(x => x.Role?.Id == RoleId.Medic);
                if (Medic == null || Medic.Owner.Data.IsDead || Medic.Owner.Data.Disconnected)
                {
                    player.Immortal = false;
                    player.ClearOutline();
                }
            }

            Role?.Update();
        }

        public void SetNameColor(PlayerControl player, Color color)
        {
            player.nameText.Color = color;
            if (HudManager.Instance && HudManager.Instance.Chat)
                foreach (PoolableBehavior bubble in HudManager.Instance.Chat.chatBubPool.activeChildren)
                    if (bubble.Cast<ChatBubble>().NameText.text == player.nameText.Text)
                        bubble.Cast<ChatBubble>().NameText.color = color;
            if (MeetingHud.Instance && MeetingHud.Instance.playerStates != null)
                foreach (PlayerVoteArea voteArea in MeetingHud.Instance.playerStates)
                    if (voteArea.TargetPlayerId == player.PlayerId)
                        voteArea.NameText.Color = color;
        }

        public void SetOutline(Color color)
        {
            if (!AmongUsClient.Instance.IsGameStarted) return;
            
            Owner.GetComponent<SpriteRenderer>().material.SetFloat("_Outline", 1f);
            Owner.GetComponent<SpriteRenderer>().material.SetColor("_OutlineColor", color);
        }
        
        public void ClearOutline()
        {
            if (!AmongUsClient.Instance.IsGameStarted) return;
            
            Owner.GetComponent<SpriteRenderer>().material.SetFloat("_Outline", 0f);
            Owner.GetComponent<SpriteRenderer>().material.SetColor("_OutlineColor", new Color(0f, 0f, 0f, 0f));
        }
        
        public bool ShouldLockButtons()
        {
            return Owner.Data.IsDead || MeetingHud.Instance || ExileController.Instance;
        }
    }
}