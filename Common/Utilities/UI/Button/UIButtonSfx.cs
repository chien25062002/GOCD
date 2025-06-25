using GOCD.Framework.Audio;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GOCD.Framework
{
    public class UIButtonSfx : MonoBehaviour, IPointerDownHandler
    {
        [Title("Config")]
        [SerializeField] AudioConfig _sfxOnClick;

        int _tryCount;
        Button _button;

        Button button
        {
            get
            {
                if (_tryCount == 0)
                {
                    _button = GetComponent<Button>();
                    _tryCount++;
                }

                return _button;
            }
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (button != null && !button.interactable) return;
            
            AudioManager.Play(_sfxOnClick != null ? _sfxOnClick : GOCDFactory.SfxUIButtonClick);
        }
    }
}
