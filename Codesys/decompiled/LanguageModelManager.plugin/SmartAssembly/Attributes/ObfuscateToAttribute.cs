using System;

namespace SmartAssembly.Attributes
{
	// Token: 0x02000010 RID: 16
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Interface)]
	public sealed class ObfuscateToAttribute : Attribute
	{
		// Token: 0x06000016 RID: 22 RVA: 0x00002050 File Offset: 0x00001050
		public ObfuscateToAttribute(string newName)
		{
		}
	}
}
