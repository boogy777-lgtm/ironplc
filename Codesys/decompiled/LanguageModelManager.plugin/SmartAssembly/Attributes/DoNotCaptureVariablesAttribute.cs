using System;

namespace SmartAssembly.Attributes
{
	// Token: 0x02000005 RID: 5
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method)]
	public sealed class DoNotCaptureVariablesAttribute : Attribute
	{
	}
}
