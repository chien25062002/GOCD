namespace GOCD.Framework
{
    public class DebugButton : UIButtonBase
    {
        protected override void Awake()
        {
            base.Awake();

#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            button.interactable = false;
#endif
        }
    }
}
