using Reactor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnhollowerBaseLib.Attributes;
using UnityEngine;


namespace ExtraRolesMod.Roles.Medic
{
    [RegisterInIl2Cpp]
    public class PlayerShield : MonoBehaviour
    {
        float strength = 1f;
        IEnumerator glow = null;
        public PlayerShield(IntPtr value) : base(value)
        {
        }

        public void OnDestroy()
        {
            if (glow != null)
                Coroutines.Stop(glow);
            
            strength = 0f;
            SetShieldColor(Palette.VisorColor);
        }

        public void Update()
        {
            SetShieldColor(Colors.protectedColor);
        }

        [HideFromIl2Cpp]
        private void SetShieldColor(Color color)
        {
            var myRend = gameObject.GetComponent<SpriteRenderer>();
            myRend.material.SetColor("_VisorColor", color);
            myRend.material.SetFloat("_Outline", strength);
            myRend.material.SetColor("_OutlineColor", color);
        }

        internal void GlowShield()
        {

            SoundManager.Instance.PlaySound(ExtraRoles.Assets.breakClip, false, 100f);
            glow = Coroutines.Start(MakeGlow());
        }

        [HideFromIl2Cpp]
        public IEnumerator MakeGlow()
        {
            for (int i = 0; i < 10; i++)
            {
                strength -= .1f;
                yield return new WaitForSeconds(.05f);
            }
            for (int i = 0; i < 10; i++)
            {
                strength += .1f;
                yield return new WaitForSeconds(.05f);
            }
            glow = null;
        }
    }
}
