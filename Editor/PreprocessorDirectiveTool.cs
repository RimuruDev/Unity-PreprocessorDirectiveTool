#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AbyssMoth
{
    public sealed class PreprocessorDirectiveTool : EditorWindow
    {
        // TODO: Add save last changes
        private string directive = "YANDEX_GAMES";
        private string targetFolder = "Assets/YandexGame";
        private bool addDirectives = true;

        [MenuItem("RimuruDev Tools/Preprocessor Directive Tool")]
        public static void ShowWindow() =>
            GetWindow<PreprocessorDirectiveTool>("Preprocessor Directive Tool");

        private void OnGUI()
        {
            GUILayout.Label("Preprocessor Directive Tool", EditorStyles.boldLabel);

            directive = EditorGUILayout.TextField("Directive", directive);
            targetFolder = EditorGUILayout.TextField("Target Folder", targetFolder);
            addDirectives = EditorGUILayout.Toggle("Add Directives", addDirectives);

            if (GUILayout.Button("Apply"))
            {
                if (string.IsNullOrWhiteSpace(directive))
                {
                    EditorUtility.DisplayDialog("Error", "Directive cannot be empty.", "OK");
                    return;
                }

                if (!Directory.Exists(targetFolder))
                {
                    EditorUtility.DisplayDialog("Error", $"The folder '{targetFolder}' does not exist.", "OK");
                    return;
                }

                // TODO: Add in config localization settings
                var action = addDirectives ? "add" : "remove";

                if (EditorUtility.DisplayDialog(
                        "Confirm",
                        $"Are you sure you want to {action} the directive '#if {directive}' for scripts in folder '{targetFolder}'?",
                        "Yes",
                        "No"))
                {
                    if (addDirectives)
                    {
                        AddDirectivesToScripts(targetFolder, directive);
                    }
                    else
                    {
                        RemoveDirectivesFromScripts(targetFolder, directive);
                    }
                }
            }
            
            if (GUILayout.Button("Select Folder"))
            {
                var path = EditorUtility.OpenFolderPanel("Select Target Folder", Application.dataPath, "");
               
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.StartsWith(Application.dataPath))
                    {
                        targetFolder = "Assets" + path.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Error", "Please select a folder within the project Assets directory.", "OK");
                    }
                }
            }

        }

        private static void AddDirectivesToScripts(string folderPath, string directive)
        {
            var scriptFiles = Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories);

            foreach (var scriptFile in scriptFiles)
            {
                var lines = File.ReadAllLines(scriptFile);

                if (lines.Length == 0 || lines[0].Contains($"#if {directive}"))
                {
                    Debug.Log($"Skipping {scriptFile} as it already contains the directive.");
                    continue;
                }

                using (StreamWriter writer = new StreamWriter(scriptFile))
                {
                    writer.WriteLine($"#if {directive}");
                    foreach (string line in lines)
                    {
                        writer.WriteLine(line);
                    }

                    writer.WriteLine("#endif");
                }

                Debug.Log($"Processed (added directive): {scriptFile}");
            }

            AssetDatabase.Refresh();
        }

        private static void RemoveDirectivesFromScripts(string folderPath, string directive)
        {
            var scriptFiles = Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories);

            foreach (var scriptFile in scriptFiles)
            {
                var lines = File.ReadAllLines(scriptFile);
                var hasDirective = lines.Length > 0 && lines[0].Contains($"#if {directive}");
                var hasEndif = lines.Length > 0 && lines[lines.Length - 1].Trim() == "#endif";

                if (!hasDirective || !hasEndif)
                {
                    Debug.Log(
                        $"Skipping {scriptFile} as it does not contain the directive or is improperly formatted.");
                    continue;
                }

                // :D Remove the first line (#if directive) and last line (#endif)
                // NOTE: Add settings. Remove magic number)
                var newLines = lines
                    .Skip(1)
                    .Take(lines.Length - 2)
                    .ToArray();

                File.WriteAllLines(scriptFile, newLines);

                Debug.Log($"Processed (removed directive): {scriptFile}");
            }

            AssetDatabase.Refresh();
        }
    }
}
#endif