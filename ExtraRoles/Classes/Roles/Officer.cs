using Hazel;
using UnityEngine;

namespace ExtraRoles2.Classes.Roles
{
    public class Officer : Role
    {
        public Button KillButton { get; set; }
        
        public Officer(Player owner)
        {
            Owner = owner;
            Id = RoleId.Officer;
            Name = "Officer";
            Color = new Color(0f, 97f / 255f, 252f / 255f);
            
            if (!owner.Owner.AmOwner) return;
            KillButton = new Button(Vector2.zero, null, 45f, Owner);
        }

        public override void Update()
        {
            if (KillButton == null) return;
            
            var bottomLeft = Camera.main.ScreenToWorldPoint(new Vector2(0, 0));
            bottomLeft.x += 0.75f; bottomLeft.y += 0.75f;
            
            KillButton.Position = bottomLeft;
            KillButton.IsActive = HudManager.Instance.UseButton.isActiveAndEnabled;
            KillButton.CurrentTarget = Owner.Owner.FindClosestTarget();
            KillButton.Update();
        }

        public override void PerformKill(KillButtonManager killButtonManager)
        {
            if (KillButton == null) return;
            if (KillButton.CurrentTarget == null) return;
            if (killButtonManager != KillButton.ButtonManager) return;
            if (killButtonManager.isCoolingDown) return;
            if (!killButtonManager.isActiveAndEnabled) return;


            if (KillButton.CurrentTarget.Data.IsImpostor)
                Owner.Owner.MurderPlayer(KillButton.CurrentTarget);
            else
                Owner.Owner.MurderPlayer(Owner.Owner);
            
            ResetButtons();
        }

        public override void ResetButtons()
        {
            KillButton.ResetCooldown();
        }
    }
}