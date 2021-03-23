using Hazel;
using Reactor.Extensions;
using UnityEngine;

namespace ExtraRoles2.Classes.Roles
{
    public class Medic : Role
    {
        public Button ShieldButton { get; set; }
        
        public Medic(Player owner)
        {
            Owner = owner;
            Id = RoleId.Medic;
            Name = "Medic";
            Color = new Color(0f, 252f / 255f, 88 / 255f);

            if (!owner.Owner.AmOwner) return;
            ShieldButton = new Button(Vector2.zero, null, 10f, Owner);
        }
        
        public override void Update()
        {
            if (ShieldButton == null) return;
            
            var bottomLeft = Camera.main.ScreenToWorldPoint(new Vector2(0, 0));
            bottomLeft.x += 0.75f; bottomLeft.y += 0.75f;
            
            ShieldButton.Position = bottomLeft;
            ShieldButton.IsActive = HudManager.Instance.UseButton.isActiveAndEnabled;
            ShieldButton.CurrentTarget = Owner.Owner.FindClosestTarget();
            ShieldButton.Update();
        }
        
        public override void PerformKill(KillButtonManager killButtonManager)
        {
            if (ShieldButton == null) return;
            if (ShieldButton.CurrentTarget == null) return;
            if (killButtonManager != ShieldButton.ButtonManager) return;
            if (killButtonManager.isCoolingDown) return;
            if (!killButtonManager.isActiveAndEnabled) return;

            ShieldButton.CurrentTarget.GetModdedPlayer().Immortal = true;
            
            MessageWriter writer = RpcHelper.Instance.GeneratePacket(CustomRPC.SetProtected);
            writer.Write(ShieldButton.CurrentTarget.PlayerId);
            writer.EndMessage();

            ShieldButton.CurrentTarget = null;
            ShieldButton.ButtonManager.gameObject.Destroy();
            ShieldButton = null;
        }

        public override void ResetButtons()
        {
            ShieldButton.ResetCooldown();
        }
    }
}