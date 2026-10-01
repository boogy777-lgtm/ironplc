using System;

namespace SmartAssembly.Attributes
{
	// Token: 0x0200000A RID: 10
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface)]
	public sealed class DoNotPruneTypeAttribute : Attribute
	{
	}
}
