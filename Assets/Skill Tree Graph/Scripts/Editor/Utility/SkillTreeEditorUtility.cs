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

        public static void BuildInspectorElement(VisualElement panel, SerializedObject serializedObject, string labelText = null, System.Predicate<string> isReadOnly = null, IReadOnlyDictionary<string, System.Action> onPropertyChanged = null)
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

                    if (isReadOnly != null && isReadOnly(iterator.propertyPath))
                        field.SetEnabled(false);

                    if (onPropertyChanged != null && onPropertyChanged.TryGetValue(iterator.propertyPath, out var callback))
                    {
                        field.RegisterValueChangeCallback(evt =>
                        {
                            callback();
                            serializedObject.ApplyModifiedProperties();
                        });
                    }

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

        public static T DeepClone<T>(T source) where T : ScriptableObject
        {
            T clone = CreateTransientInstance<T>();

            string json = EditorJsonUtility.ToJson(source);
            EditorJsonUtility.FromJsonOverwrite(json, clone);

            return clone;
        }

        public static T CreateTransientInstance<T>() where T : ScriptableObject
        {
            T instance = ScriptableObject.CreateInstance<T>();
            instance.hideFlags = HideFlags.DontUnloadUnusedAsset;
            return instance;
        }

        public static Vector2 ScreenToGraph(VisualElement graphContent, Vector2 screenPosition)
        {
#if UNITY_6000_3_OR_NEWER
            float zoom = graphContent.style.scale.value.value.x;
            Vector2 pan = new Vector2(graphContent.style.translate.value.x.value, graphContent.style.translate.value.y.value);
#else
            float zoom = graphContent.transform.scale.x;
            Vector2 pan = graphContent.transform.position;
#endif
            return (screenPosition - pan) / zoom;
        }
    }
}