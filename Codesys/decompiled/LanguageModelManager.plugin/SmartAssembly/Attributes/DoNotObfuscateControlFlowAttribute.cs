using System;

namespace SmartAssembly.Attributes
{
	// Token: 0x0200000F RID: 15
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method)]
	public sealed class DoNotObfuscateControlFlowAttribute : Attribute
	{
	}
}
