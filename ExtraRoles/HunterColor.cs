using Reactor;
using System;
using UnityEngine;

namespace ExtraRolesMod
{
    [RegisterInIl2Cpp]
    internal class HunterColor : MonoBehaviour
    {
        private bool defaultSet = false;
        private int currentColor = 0;
        private Color newColor;
        private Color nextColor;

        private Color[] colors =
        {
            Color.red, new Color(255f / 255f, 94f / 255f, 19f / 255f), Color.yellow, Color.green, Color.blue,
            new Color(120f / 255f, 7f / 255f, 188f / 255f)
        };

        public HunterColor(IntPtr value) : base(value)
        {
        }

        public void Awake()
        {

        }

        public void Update()
        {
            var player = PlayerControl.LocalPlayer;
            if (!defaultSet)
            {
                defaultSet = true;
                player.myRend.material.SetColor("_BackColor", colors[currentColor]);
                player.myRend.material.SetColor("_BodyColor", colors[currentColor]);
                newColor = colors[currentColor];
                if (currentColor + 1 >= colors.Length)
                    currentColor = -1;
                nextColor = colors[currentColor + 1];
            }

            newColor = Vector3.MoveTowards(newColor.ToVector(), nextColor.ToVector(), 0.02f).ToColor();
            player.myRend.material.SetColor("_BackColor", newColor);
            player.myRend.material.SetColor("_BodyColor", newColor);

            if (newColor != nextColor)
                return;

            currentColor++;
            defaultSet = false;
        }
    }
}
