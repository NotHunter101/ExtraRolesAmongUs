using System;
using UnityEngine;

namespace ExtraRoles2.Classes
{
    public class Button
    {
        public KillButtonManager ButtonManager { get; }
        public Player Owner { get; }
        public Vector2 Position { get; set; }
        public Sprite Sprite { get; }
        public float Cooldown { get; set; }
        public PlayerControl CurrentTarget { get; set; }
        public float MaxCooldown { get; set; }
        public bool IsActive { get; set; }
        public bool IsLit { get; set; }

        public Button(Vector2 position, Sprite sprite, float maxCooldown, Player owner)
        {
            ButtonManager = UnityEngine.Object.Instantiate(HudManager.Instance.KillButton);
            ButtonManager.renderer.enabled = true;
            
            Position = position;
            Sprite = sprite;
            MaxCooldown = maxCooldown;
            Cooldown = maxCooldown;
            Owner = owner;
        }

        public void Update()
        {
            if (IsActive && Owner.Owner.CanMove)
                Cooldown -= Time.fixedDeltaTime;

            if (Owner.ShouldLockButtons())
            {
                ResetCooldown();
                CurrentTarget = null;
            }

            ButtonManager.gameObject.SetActive(IsActive);
            ButtonManager.transform.position = Position;
            ButtonManager.SetCoolDown(Cooldown, MaxCooldown);
            ButtonManager.SetTarget(CurrentTarget);

            if (IsLit &&
                ButtonManager.isActiveAndEnabled && 
                !ButtonManager.isCoolingDown)
            {
                ButtonManager.renderer.material.SetFloat("_Desat", 0f);
                ButtonManager.renderer.color = Palette.EnabledColor;
            }
        }

        public void ResetCooldown()
        {
            Cooldown = MaxCooldown;
        }
    }
}