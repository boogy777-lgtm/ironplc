using System;

namespace SmartAssembly.Attributes
{
	// Token: 0x02000008 RID: 8
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface)]
	public sealed class DoNotObfuscateTypeAttribute : Attribute
	{
	}
}
