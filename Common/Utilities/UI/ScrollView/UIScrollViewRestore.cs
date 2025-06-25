using UnityEngine;
using UnityEngine.UI;

namespace CFramework
{
    public class UIScrollViewRestore : MonoBehaviour
    {
        ScrollRect _scrollRect;

        ScrollRect scrollRect
        {
            get
            {
                if (_scrollRect == null)
                    _scrollRect = GetComponent<ScrollRect>();
                if (_scrollRect == null)
                    _scrollRect = GetComponentInChildren<ScrollRect>();
                return _scrollRect;
            }
        }

        void OnEnable()
        {
            Restore();
        }
        
        void Restore()
        {
            if (scrollRect.vertical)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }
            else
            {
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }
    }
}
