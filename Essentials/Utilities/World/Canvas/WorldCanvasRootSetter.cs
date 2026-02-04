using UnityEngine;

namespace CodeSketch.Utilities.CanvasWorld
{
    public class WorldCanvasRootSetter : MonoBehaviour
    {
        void OnEnable()
        {
            WorldCanvasManager.Root = transform;
        }
    }
}
