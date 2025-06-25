using UnityEngine;

namespace GOCD.Framework.Module.HealthBar
{
    public abstract class HealthBarBase : MonoCached
    {
        public abstract void Init(float maxHp, float hp);
        public abstract void TakeDamage(float damage, bool immediately = false);
        public abstract void Hide();
    }
}
