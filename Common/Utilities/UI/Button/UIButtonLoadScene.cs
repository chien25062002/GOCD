using UnityEngine;

namespace GOCD.Framework
{
    public class UIButtonLoadScene : UIButtonBase
    {
        [Header("Config")]
        [SerializeField] int _sceneIndex;

        public override void Button_OnClick()
        {
            base.Button_OnClick();

            SceneLoaderHelper.Load(_sceneIndex);
        }
    }
}