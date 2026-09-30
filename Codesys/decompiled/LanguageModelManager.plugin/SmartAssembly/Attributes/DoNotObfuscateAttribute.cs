using System;

namespace SmartAssembly.Attributes
{
	// Token: 0x02000007 RID: 7
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Interface | AttributeTargets.Delegate)]
	public sealed class DoNotObfuscateAttribute : Attribute
	{
	}
}
