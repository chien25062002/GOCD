using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GOCD.Framework
{
    public class Optimize_UILayoutGroup : MonoBehaviour
    {
        [SerializeField, HideInInspector] LayoutGroup _layoutGroup;
        [SerializeField] int _waitForFrameCount = 1;
        
        LayoutGroup LayoutGroup
        {
            get
            {
                if (_layoutGroup == null)
                    _layoutGroup = GetComponent<LayoutGroup>();
                return _layoutGroup;
            }
        }

        void OnEnable()
        {
            HandleOptimize();
        }

        void OnDisable()
        {
            LayoutGroup.enabled = false;
        }

        public void HandleOptimize()
        {
            StopAllCoroutines();
            StartCoroutine(IEHandleOptimize());
        }

        IEnumerator IEHandleOptimize()
        {
            LayoutGroup.enabled = true;

            for (int i = 0; i < _waitForFrameCount; i++)
            {
                yield return new WaitForEndOfFrame();
            }

            LayoutGroup.enabled = false;
        }

        void OnValidate()
        {
            if (_layoutGroup == null) _layoutGroup = LayoutGroup;
        }
    }
}
