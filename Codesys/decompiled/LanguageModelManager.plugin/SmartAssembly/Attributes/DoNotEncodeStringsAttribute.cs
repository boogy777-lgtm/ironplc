using System;

namespace SmartAssembly.Attributes
{
	// Token: 0x02000012 RID: 18
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method)]
	public sealed class DoNotEncodeStringsAttribute : Attribute
	{
	}
}
