using UnityEditor;
using UnityEngine;

namespace MyUnityPackage.VisualEffect.Editor
{
    /// <summary>
    /// Adds Play / Stop buttons under the effect Inspector. In Edit Mode the effect is previewed by
    /// ticking it from EditorApplication.update, and the object is restored when the preview ends.
    /// </summary>
    [CustomEditor(typeof(VisualEffectBehaviour), true)]
    [CanEditMultipleObjects]
    public class VisualEffectBehaviourEditor : UnityEditor.Editor
    {
        private bool _previewing;
        private double _lastTime;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Preview runs in the Scene view and restores the object when it ends.", MessageType.Info);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Play")) PlayAll();
                if (GUILayout.Button("Stop")) StopAll();
            }
        }

        private void OnDisable()
        {
            // Selection change, Play Mode or domain reload: never leave modified values behind
            if (_previewing) StopAll();
        }

        private void PlayAll()
        {
            foreach (Object t in targets)
            {
                VisualEffectBehaviour effect = (VisualEffectBehaviour)t;
                if (effect == null) continue;
                effect.Play();
            }

            if (Application.isPlaying) return;

            _lastTime = EditorApplication.timeSinceStartup;
            if (!_previewing)
            {
                _previewing = true;
                EditorApplication.update += PreviewUpdate;
            }
        }

        private void StopAll()
        {
            foreach (Object t in targets)
            {
                VisualEffectBehaviour effect = (VisualEffectBehaviour)t;
                if (effect == null) continue;
                effect.Stop();
            }

            Unsubscribe();
            SceneView.RepaintAll();
        }

        private void PreviewUpdate()
        {
            double now = EditorApplication.timeSinceStartup;
            float delta = (float)(now - _lastTime);
            _lastTime = now;

            bool anyPlaying = false;

            foreach (Object t in targets)
            {
                VisualEffectBehaviour effect = (VisualEffectBehaviour)t;
                if (effect == null) continue;

                effect.Tick(delta);

                if (effect.IsPlaying) anyPlaying = true;
                else effect.Stop(); // Finished: return the object to its saved state
            }

            SceneView.RepaintAll();
            EditorApplication.QueuePlayerLoopUpdate();

            if (!anyPlaying) Unsubscribe();
        }

        private void Unsubscribe()
        {
            if (!_previewing) return;
            _previewing = false;
            EditorApplication.update -= PreviewUpdate;
        }
    }
}
