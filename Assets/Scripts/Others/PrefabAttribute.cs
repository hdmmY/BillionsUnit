using System;

[AttributeUsage (AttributeTargets.Class)]
public class PrefabAttribute : Attribute
{
	public readonly string Name;

	public PrefabAttribute (string name) { Name = name; }

	public PrefabAttribute () { Name = null; }
}