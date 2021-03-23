using UnityEngine;

namespace ExtraRoles2.Classes
{
    public class Role
    {
        public Player Owner { get; set; }
        public RoleId Id { get; set; }
        public string Name { get; set; }
        public Color Color { get; set; }
        public string IntroString { get; set; }
        public bool Teamless { get; set; }
        
        public virtual void Update() {}
        public virtual void PerformKill(KillButtonManager killButtonManager) {}
        public virtual void ResetButtons() {}
    }
}