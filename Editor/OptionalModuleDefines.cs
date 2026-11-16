using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Compilation;
using UnityEngine;

namespace MyUnityPackage.VisualEffect.Editor
{
    /// <summary>
    /// Enables the optional modules of the package when their third-party asset is present.
    /// DOTween and Feel come from the Asset Store, not from the Package Manager, so an asmdef
    /// "Version Define" cannot detect them: this script looks for their assemblies instead and
    /// adds or removes the matching scripting define for the active build target.
    /// </summary>
    [InitializeOnLoad]
    internal static class OptionalModuleDefines
    {
        private const string MenuRoot = "Tools/MyUnityPackage/Visual Effect/";
        private const string AutoSyncMenu = MenuRoot + "Auto Sync Optional Modules";
        private const string AutoSyncPref = "MyUnityPackage.VisualEffect.AutoSyncOptionalModules";

        // define -> assembly whose presence enables it
        private static readonly KeyValuePair<string, string>[] Modules =
        {
            new KeyValuePair<string, string>("MUP_DOTWEEN", "DOTween.Modules"),
            new KeyValuePair<string, string>("MUP_FEEL", "MoreMountains.Feedbacks")
        };

        private static bool AutoSync
        {
            get => EditorPrefs.GetBool(AutoSyncPref, true);
            set => EditorPrefs.SetBool(AutoSyncPref, value);
        }

        static OptionalModuleDefines()
        {
            // Delayed: PlayerSettings must not be changed while the editor is still loading assemblies
            EditorApplication.delayCall += () =>
            {
                if (AutoSync) Sync(false);
            };
        }

        [MenuItem(MenuRoot + "Sync Optional Modules Now")]
        private static void SyncFromMenu()
        {
            Sync(true);
        }

        [MenuItem(AutoSyncMenu)]
        private static void ToggleAutoSync()
        {
            AutoSync = !AutoSync;
        }

        [MenuItem(AutoSyncMenu, true)]
        private static bool ToggleAutoSyncValidate()
        {
            Menu.SetChecked(AutoSyncMenu, AutoSync);
            return true;
        }

        private static void Sync(bool logWhenUnchanged)
        {
            var assemblies = new HashSet<string>();
            foreach (Assembly assembly in CompilationPipeline.GetAssemblies())
            {
                assemblies.Add(assembly.name);
            }

            NamedBuildTarget target = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            PlayerSettings.GetScriptingDefineSymbols(target, out string[] current);
            var defines = new List<string>(current);
            bool changed = false;

            foreach (KeyValuePair<string, string> module in Modules)
            {
                bool present = assemblies.Contains(module.Value);
                bool defined = defines.Contains(module.Key);

                if (present && !defined)
                {
                    defines.Add(module.Key);
                    changed = true;
                    Debug.Log($"[VisualEffect] {module.Value} found: enabling {module.Key} for {target.TargetName}.");
                }
                else if (!present && defined)
                {
                    defines.Remove(module.Key);
                    changed = true;
                    Debug.Log($"[VisualEffect] {module.Value} not found: removing {module.Key} for {target.TargetName}.");
                }
            }

            if (changed)
            {
                PlayerSettings.SetScriptingDefineSymbols(target, defines.ToArray());
            }
            else if (logWhenUnchanged)
            {
                Debug.Log("[VisualEffect] Optional modules are already in sync.");
            }
        }
    }
}
