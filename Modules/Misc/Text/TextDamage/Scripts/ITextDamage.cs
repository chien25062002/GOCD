using UnityEngine;

namespace CodeSketch.Modules.TextDamageSystem
{
    public interface ITextDamage
    {
        void Show(int damage, Vector3 position, bool isCrit);
    }
}
