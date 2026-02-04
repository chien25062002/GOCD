using UnityEngine;

namespace GOCD.Framework.ChainOfResponsibility
{
    public abstract class HandlerBase<T> : IHandler<T>
    {
        protected IHandler<T> _nextHandler;

        public IHandler<T> NextHandler => _nextHandler;

        public void SetNext(IHandler<T> nextHandler)
        {
            _nextHandler = nextHandler;
        }

        public abstract bool CanHandle(T request);

        public void Handle(T request)
        {
            if (CanHandle(request))
            {
                ProcessRequest(request);
            }
            else
            {
                _nextHandler?.Handle(request);
            }
        }

        public void PostProcess(T request)
        {
            if (CanHandle(request))
            {
                PostProcessRequest(request);
            }
            else
            {
                _nextHandler?.PostProcess(request);
            }
        }

        protected abstract void ProcessRequest(T request);
        protected abstract void PostProcessRequest(T request);
    }
}
