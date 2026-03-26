using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public class AddTypeMenuAttribute : Attribute
{
    public string MenuPath { get; }
    public AddTypeMenuAttribute(string menuPath) => MenuPath = menuPath;
}