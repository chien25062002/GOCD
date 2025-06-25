using UnityEngine;

namespace CFramework
{
    public class WorldCanvasRootSetter : MonoBehaviour
    {
        void OnEnable()
        {
            WorldCanvasManager.Root = transform;
        }
    }
}
