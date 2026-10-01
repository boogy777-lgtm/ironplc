using System;

namespace SmartAssembly.Attributes
{
	// Token: 0x02000011 RID: 17
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface)]
	public sealed class ObfuscateNamespaceToAttribute : Attribute
	{
		// Token: 0x06000017 RID: 23 RVA: 0x00002050 File Offset: 0x00001050
		public ObfuscateNamespaceToAttribute(string newName)
		{
		}
	}
}
