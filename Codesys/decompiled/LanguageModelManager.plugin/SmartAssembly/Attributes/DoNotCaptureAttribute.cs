using System;

namespace SmartAssembly.Attributes
{
	// Token: 0x02000006 RID: 6
	[DoNotPrune]
	[DoNotObfuscate]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field, Inherited = true)]
	public sealed class DoNotCaptureAttribute : Attribute
	{
	}
}
