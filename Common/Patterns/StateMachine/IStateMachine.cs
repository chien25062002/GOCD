namespace GOCD.Framework
{
    public interface IStateMachine
    {
        void Init();
        void OnStart();
        void OnUpdate();
        void OnFixedUpdate();
        void OnStop();
        void OnDestroy();
    }
}
