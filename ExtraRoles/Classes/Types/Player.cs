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
        
        public bool ShouldLockButtons()
        {
            return Owner.Data.IsDead || MeetingHud.Instance || ExileController.Instance;
        }
    }
}