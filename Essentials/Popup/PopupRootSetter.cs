using UnityEngine;

namespace CodeSketch.UIPopup
{
    public class PopupRootSetter : MonoBehaviour
    {
        [SerializeField] bool _setupOnEnable = true;
        
        void Awake()
        {
            PopupManager.SetRoot(transform);
            PopupManager.EventRootUndefine += OnRootUndefine;
        }

        void OnDestroy()
        {
            PopupManager.EventRootUndefine -= OnRootUndefine;
        }

        void OnEnable()
        {
            if (_setupOnEnable)
            {
                PopupManager.SetRoot(transform);
            }
        }

        void OnRootUndefine()
        {
            PopupManager.SetRoot(transform);
        }
    }
}
