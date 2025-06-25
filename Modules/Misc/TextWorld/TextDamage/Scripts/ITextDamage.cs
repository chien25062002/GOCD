using UnityEngine;

namespace GOCD.Framework.Module.TextDamage
{
    public interface ITextDamage
    {
        void Show(int damage, Vector3 position, bool isCrit);
    }
}
