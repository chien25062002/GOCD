using CodeSketch.Mono;
using UnityEngine;

namespace CodeSketch.Modules.HealthSystem
{
    public abstract class HealthBarBase : MonoBase
    {
        public abstract void Init(float maxHp, float hp);
        public abstract void TakeDamage(float damage, bool immediately = false);
        public abstract void Hide();
    }
}
