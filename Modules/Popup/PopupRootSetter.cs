using UnityEngine;

namespace GOCD.Framework
{
    public class PopupRootSetter : MonoBehaviour
    {
        [SerializeField] bool _setupOnEnable = true;
        
        void Awake()
        {
            PopupManager.SetRoot(transform);
            
            PopupManager.eventRootUndefine += OnRootUndefine;
        }

        void OnDestroy()
        {
            PopupManager.eventRootUndefine -= OnRootUndefine;
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
