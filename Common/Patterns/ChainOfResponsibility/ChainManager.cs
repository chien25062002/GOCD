using UnityEngine;

namespace GOCD.Framework.ChainOfResponsibility
{
    public class ChainManager<T>
    {
        IHandler<T> _firstHandler;

        public void AddHandler(IHandler<T> handler)
        {
            if (_firstHandler == null)
            {
                _firstHandler = handler;
            }
            else
            {
                var current = _firstHandler;
                while (current is HandlerBase<T> baseHandler && baseHandler.NextHandler != null)
                {
                    current = baseHandler.NextHandler;
                }
                
                current.SetNext(handler);
            }
        }
        
        public void Process(T request)
        {
            _firstHandler?.Handle(request); // Bắt đầu xử lý từ handler đầu tiên
        }

        public void PostProcess(T request)
        {
            _firstHandler?.PostProcess(request);
        }
    }
}
