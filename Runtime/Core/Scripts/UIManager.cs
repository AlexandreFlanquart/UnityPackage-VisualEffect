using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using MyUnityPackage.Toolkit;

namespace MyUnityPackage.VisualEffect
{
    public static class UIManager
    {
        private const string TransitionsResourcesPath = "Transitions";

        // Maps a UI component type to its registered instance (e.g. Canvas, custom UI scripts)
        private static readonly Dictionary<Type, UnityEngine.Object> canvasUI = new Dictionary<Type, UnityEngine.Object>();

        // Transitions by name, loaded on first use from Resources/Transitions
        private static Dictionary<string, TransitionSO> transitions;

        // Static state survives Play Mode sessions when Domain Reload is disabled: reset it explicitly
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            canvasUI.Clear();
            transitions = null;
        }

        private static Dictionary<string, TransitionSO> Transitions
        {
            get
            {
                if (transitions != null) return transitions;

                transitions = new Dictionary<string, TransitionSO>();

                foreach (TransitionSO transition in Resources.LoadAll<TransitionSO>(TransitionsResourcesPath))
                {
                    if (transition == null) continue;

                    if (string.IsNullOrEmpty(transition.transitionName))
                    {
                        MUPLogger.Warning("Transition asset '" + transition.name + "' has an empty transition name and was ignored");
                        continue;
                    }

                    if (transitions.ContainsKey(transition.transitionName))
                    {
                        MUPLogger.Warning("Duplicate transition name '" + transition.transitionName + "': asset '" + transition.name + "' was ignored");
                        continue;
                    }

                    transitions.Add(transition.transitionName, transition);
                }

                return transitions;
            }
        }

        // Register a UI component of type T from the given GameObject
        public static void AddCanvasUI<T>(GameObject go) where T : UnityEngine.Object
        {
            if (go == null) throw new ArgumentNullException(nameof(go));

            // Already registered and still alive: keep the existing one
            if (canvasUI.TryGetValue(typeof(T), out UnityEngine.Object existing) && existing != null)
            {
                MUPLogger.Warning("Canvas UI " + typeof(T).Name + " is already registered");
                return;
            }

            T component = go.GetComponent<T>();
            if (component == null)
            {
                MUPLogger.Warning("Can't register canvas UI: no " + typeof(T).Name + " component on " + go.name);
                return;
            }

            // Indexer also replaces an entry whose object has been destroyed
            canvasUI[typeof(T)] = component;
        }

        // Retrieve a registered UI component of type T (null if not registered or destroyed)
        public static T GetCanvasUI<T>() where T : UnityEngine.Object
        {
            if (!canvasUI.TryGetValue(typeof(T), out UnityEngine.Object registered))
            {
                MUPLogger.Warning("Can't find requested canvas UI " + typeof(T).Name);
                return null;
            }

            if (registered == null)
            {
                // The key exists but the object it references has been destroyed
                canvasUI.Remove(typeof(T));
                return null;
            }

            return (T)registered;
        }

        // Play a transition by its name on the given canvas (fire and forget)
        public static void PlayTransitionByName(GameObject canvas, string transitionName)
        {
            _ = LogExceptions(PlayTransitionByNameAsync(canvas, transitionName));
        }

        // Play a transition by its name on the given canvas. The task completes when the transition is finished.
        public static Task PlayTransitionByNameAsync(GameObject canvas, string transitionName)
        {
            if (canvas == null) throw new ArgumentNullException(nameof(canvas));

            if (string.IsNullOrEmpty(transitionName) || !Transitions.TryGetValue(transitionName, out TransitionSO transition))
            {
                MUPLogger.Warning("Transition not found: " + transitionName + " check if this transition is in a Resources/" + TransitionsResourcesPath + " folder");
                return Task.CompletedTask;
            }

            MUPLogger.Info("PlayTransition : " + transition.transitionName + " at canvas : " + canvas.name);
            return transition.PlayTransition(canvas);
        }

        // Play a transition by triggering an Animator parameter on the given canvas
        public static void PlayTransitionByTrigger(GameObject canvas, Animator animator, string triggerName)
        {
            if (animator != null)
            {
                animator.SetTrigger(triggerName);
            }
            else
            {
                MUPLogger.Warning("Animator not found: " + (canvas != null ? canvas.name : "null"));
            }
        }

        // Surfaces exceptions of fire-and-forget transitions instead of losing them in an unobserved Task
        private static async Task LogExceptions(Task task)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
