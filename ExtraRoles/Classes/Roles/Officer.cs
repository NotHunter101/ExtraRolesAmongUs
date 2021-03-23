using UnityEngine;

namespace ExtraRoles2.Classes.Roles
{
    public class Officer : Role
    {
        public Officer(Player owner)
        {
            Owner = owner;
            Id = RoleId.Officer;
            Name = "Officer";
            Color = new Color(0f, 97f / 255f, 252f / 255f);
        }
        
        public override void Update()
        {
            PlayerControl currentTarget = Owner.Owner.FindClosestTarget();
            
            if (HudManager.Instance.KillButton.isActiveAndEnabled && Owner.Owner.CanMove)
                Owner.Owner.killTimer -= Time.fixedDeltaTime;
            
            if (Owner.ShouldLockButtons())
            {
                Owner.Owner.killTimer = PlayerControl.GameOptions.KillCooldown;
                currentTarget = null;
            }

            HudManager.Instance.KillButton.gameObject.SetActive(HudManager.Instance.UseButton.isActiveAndEnabled);
            HudManager.Instance.KillButton.SetCoolDown(Owner.Owner.killTimer, PlayerControl.GameOptions.KillCooldown);
            HudManager.Instance.KillButton.SetTarget(currentTarget);
        }
    }
}