using System.Linq;
using Hazel;
using Reactor.Extensions;
using UnityEngine;

namespace ExtraRoles2.Classes.Roles
{
    public class Engineer : Role
    {
        public Button SabotageButton { get; set; }
        
        public Engineer(Player owner)
        {
            Owner = owner;
            Id = RoleId.Engineer;
            Name = "Engineer";
            Color = new Color(252f / 255f, 164f / 255f, 0f);

            if (!owner.Owner.AmOwner) return;
            SabotageButton = new Button(Vector2.zero, null, 10f, Owner);
        }
        
        public override void Update()
        {
            if (SabotageButton == null)
                return;
            
            var bottomLeft = Camera.main.ScreenToWorldPoint(new Vector2(0, 0));
            bottomLeft.x += 0.75f; bottomLeft.y += 0.75f;
            
            SabotageButton.Position = bottomLeft;
            SabotageButton.IsActive = HudManager.Instance.UseButton.isActiveAndEnabled;
            SabotageButton.IsLit = Owner.Owner.myTasks.ToArray().Any(PlayerTask.TaskIsEmergency);
            SabotageButton.Update();
        }

        public override void PerformKill(KillButtonManager killButtonManager)
        {
            if (SabotageButton == null) return;
            if (killButtonManager != SabotageButton.ButtonManager) return;
            if (killButtonManager.isCoolingDown) return;
            if (!killButtonManager.isActiveAndEnabled) return;
            if (!SabotageButton.IsLit) return;

            ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 16);
            
            ShipStatus.Instance.RpcRepairSystem(SystemTypes.Laboratory, 16);

            ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 0 | 64);
            ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 1 | 64);
            
            ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 0);
            ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 1);
            
            SwitchSystem switchSystem = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
            switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
            
            MessageWriter writer = RpcHelper.Instance.GeneratePacket(CustomRPC.FixLights);
            writer.EndMessage();
            
            SabotageButton.ButtonManager.gameObject.Destroy();
            SabotageButton = null;
        }

        public override void ResetButtons()
        {
            SabotageButton.ResetCooldown();
        }
    }
}