using System;
using System.Collections;
using UnityEngine;

namespace GOCD.Framework
{
    public class CoroutineGlobal : MonoSingleton<CoroutineGlobal>
    {
        public void StartInstruction(YieldInstruction instruction, Action action)
        {
            StartCoroutine(RunActionAfterInstruction(instruction, action));
        }

        IEnumerator RunActionAfterInstruction(YieldInstruction instruction, Action action)
        {
            yield return instruction;
            action.Invoke();
        }

        public void StartInstruction(CustomYieldInstruction instruction, Action action)
        {
            StartCoroutine(RunActionAfterInstruction(instruction, action));
        }

        IEnumerator RunActionAfterInstruction(CustomYieldInstruction instruction, Action action)
        {
            yield return instruction;
            action.Invoke();
        }
        
        // Start a coroutine and call an action when it's complete
        public Coroutine StartCoroutineAction(IEnumerator coroutine, Action onComplete = null)
        {
            return StartCoroutine(RunCoroutineWithAction(coroutine, onComplete));
        }

        // Stop a running coroutine
        public void StopCoroutineAction(Coroutine coroutine)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        
        // Coroutine wrapper to call action after completion
        IEnumerator RunCoroutineWithAction(IEnumerator coroutine, Action onComplete)
        {
            yield return StartCoroutine(coroutine);
            onComplete?.Invoke();
        }
    }
}
