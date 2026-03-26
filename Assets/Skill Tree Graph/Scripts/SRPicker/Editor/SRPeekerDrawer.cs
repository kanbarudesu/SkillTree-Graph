using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SRPeek.Editor
{
    [CustomPropertyDrawer(typeof(SRPeekerAttribute))]
    public class SRPeekerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                EditorGUI.LabelField(position, label.text, "Use [SRPeeker] only with [SerializeReference]");
                return;
            }

            Event e = Event.current;
            if (e.type == EventType.ContextClick && position.Contains(e.mousePosition))
            {
                HandleContextMenu(property);
                e.Use();
            }

            Rect foldoutRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            Rect buttonRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);

            string fullType = property.managedReferenceFullTypename.Split(' ').Last();
            string typeName = string.IsNullOrEmpty(property.managedReferenceFullTypename)
                ? "None (Null)"
                : (fullType.Contains('.') ? fullType.Split('.').Last() : fullType);

            if (GUI.Button(buttonRect, new GUIContent(typeName, "Right-click label to Copy/Paste"), EditorStyles.popup))
            {
                Type baseType = GetFieldType();
                var dropdown = new SRTypeDropdown(new AdvancedDropdownState(), baseType, (selectedType) =>
                {
                    property.managedReferenceValue = selectedType == null ? null : Activator.CreateInstance(selectedType);
                    property.serializedObject.ApplyModifiedProperties();
                });
                dropdown.Show(buttonRect);
            }

            if (property.isExpanded && !string.IsNullOrEmpty(property.managedReferenceFullTypename))
            {
                EditorGUI.indentLevel++;
                SerializedProperty endProperty = property.GetEndProperty();
                SerializedProperty child = property.Copy();
                bool enterChildren = true;
                float currentY = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                while (child.NextVisible(enterChildren) && !SerializedProperty.EqualContents(child, endProperty))
                {
                    float childHeight = EditorGUI.GetPropertyHeight(child, true);
                    EditorGUI.PropertyField(new Rect(position.x, currentY, position.width, childHeight), child, true);
                    currentY += childHeight + EditorGUIUtility.standardVerticalSpacing;
                    enterChildren = false;
                }
                EditorGUI.indentLevel--;
            }
        }

        private void HandleContextMenu(SerializedProperty property)
        {
            GenericMenu menu = new GenericMenu();

            if (!string.IsNullOrEmpty(property.managedReferenceFullTypename))
            {
                menu.AddItem(new GUIContent("Copy Reference"), false, () =>
                {
                    object val = property.managedReferenceValue;
                    if (val != null)
                    {
                        string json = EditorJsonUtility.ToJson(val);
                        EditorGUIUtility.systemCopyBuffer = $"{property.managedReferenceFullTypename}|{json}";
                    }
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Copy Reference"));
            }

            string buffer = EditorGUIUtility.systemCopyBuffer;
            bool canPaste = !string.IsNullOrEmpty(buffer) && buffer.Contains("|");

            if (canPaste)
            {
                menu.AddItem(new GUIContent("Paste Reference"), false, () =>
                {
                    try
                    {
                        string[] parts = buffer.Split('|');
                        string fullTypeName = parts[0];
                        string json = parts[1];
                        string[] typeSplit = fullTypeName.Split(' ');
                        string assemblyName = typeSplit[0];
                        string className = typeSplit[1];

                        Type targetType = Type.GetType($"{className}, {assemblyName}");

                        if (targetType != null)
                        {
                            object newInstance = Activator.CreateInstance(targetType);
                            EditorJsonUtility.FromJsonOverwrite(json, newInstance);

                            property.managedReferenceValue = newInstance;
                            property.serializedObject.ApplyModifiedProperties();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[SRPeek] Paste failed: {ex.Message}");
                    }
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Paste Reference"));
            }

            menu.ShowAsContext();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;

            if (property.isExpanded)
            {
                SerializedProperty endProperty = property.GetEndProperty();
                SerializedProperty child = property.Copy();
                bool enterChildren = true;

                while (child.NextVisible(enterChildren) && !SerializedProperty.EqualContents(child, endProperty))
                {
                    height += EditorGUI.GetPropertyHeight(child, true) + EditorGUIUtility.standardVerticalSpacing;
                    enterChildren = false;
                }
            }

            return height;
        }

        private Type GetFieldType()
        {
            Type type = fieldInfo.FieldType;
            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(List<>)))
                return type.GetGenericArguments()[0];
            if (type.IsArray)
                return type.GetElementType();
            return type;
        }
    }
}