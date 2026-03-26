using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SRPeek.Editor
{
    public class SRTypeDropdown : AdvancedDropdown
    {
        private readonly Type _baseType;
        private readonly Action<Type> _onSelected;

        public SRTypeDropdown(AdvancedDropdownState state, Type baseType, Action<Type> onSelected) : base(state)
        {
            _baseType = baseType;
            _onSelected = onSelected;
            minimumSize = new Vector2(300, 350);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem($"{_baseType.Name} Types");

            root.AddChild(new TypeDropdownItem(null, "None (Null)"));
            root.AddSeparator();

            var types = TypeCache.GetTypesDerivedFrom(_baseType)
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .OrderBy(t => t.Name);

            foreach (var type in types)
            {
                var menuAttr = type.GetCustomAttribute<AddTypeMenuAttribute>();
                if (menuAttr != null && !string.IsNullOrEmpty(menuAttr.MenuPath))
                    AddChildByPath(root, type, menuAttr.MenuPath);
                else
                    root.AddChild(new TypeDropdownItem(type, type.Name));
            }
            return root;
        }

        private void AddChildByPath(AdvancedDropdownItem root, Type type, string path)
        {
            string[] parts = path.Split('/');
            AdvancedDropdownItem parent = root;

            for (int i = 0; i < parts.Length; i++)
            {
                string partName = parts[i];
                var existingChild = parent.children.FirstOrDefault(c => c.name == partName);

                if (existingChild == null)
                {
                    var newFolder = new AdvancedDropdownItem(partName);
                    parent.AddChild(newFolder);
                    parent = newFolder;
                }
                else
                {
                    parent = existingChild;
                }
            }
            parent.AddChild(new TypeDropdownItem(type, type.Name));
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item is TypeDropdownItem typeItem)
                _onSelected?.Invoke(typeItem.Type);
        }
    }

    internal class TypeDropdownItem : AdvancedDropdownItem
    {
        public Type Type { get; }
        public TypeDropdownItem(Type type, string name) : base(name) => Type = type;
    }
}