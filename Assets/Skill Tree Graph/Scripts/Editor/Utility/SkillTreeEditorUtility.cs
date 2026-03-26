using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using SkillTreeGraph.Core;
using System.Collections.Generic;

namespace SkillTreeGraph.Editor
{
    public static class SkillTreeEditorUtility
    {
        public static SkillNode CreateSkillNodeAsset(string folderPath)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError("Invalid folder path: " + folderPath);
                return null;
            }

            SkillNode asset = ScriptableObject.CreateInstance<SkillNode>();

            asset.Id = GUID.Generate().ToString();

            string path = AssetDatabase.GenerateUniqueAssetPath(
                Path.Combine(folderPath, "NewSkillNode.asset")
            );

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return asset;
        }

        public static void BuildInspectorElement(VisualElement panel, SerializedObject serializedObject, string labelText = null)
        {
            panel.Clear();

            var label = new Label(labelText);
            label.style.fontSize = 25;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            panel.Add(label);

            var iterator = serializedObject.GetIterator();
            if (iterator.NextVisible(true))
            {
                do
                {
                    if (iterator.propertyPath == "m_Script")
                        continue;

                    var field = new PropertyField(iterator.Copy());
                    panel.Add(field);

                } while (iterator.NextVisible(false));
            }
            panel.Bind(serializedObject);
        }

        public static Sprite LoadSprite(string guid, string spriteName)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (string.IsNullOrEmpty(path))
                return null;

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();

            foreach (Object obj in assets)
            {
                if (obj is Sprite s && s.name == spriteName)
                    return s;
            }

            return null;
        }

        public static List<T> LoadScriptableAssets<T>() where T : ScriptableObject
        {
            List<T> result = new List<T>();

            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                result.Add(asset);
            }

            return result;
        }
    }
}