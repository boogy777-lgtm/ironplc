using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.CommonCompilerData
{
	// Token: 0x020001C9 RID: 457
	[TypeGuid("{03a1a217-49ef-48e0-93ec-2b5037e8d799}")]
	[StorageVersion("3.3.0.0")]
	public class CompilerAttribute : GenericObject2, _ICompilerAttribute
	{
		// Token: 0x06002040 RID: 8256 RVA: 0x000596D4 File Offset: 0x000586D4
		public CompilerAttribute()
		{
		}

		// Token: 0x06002041 RID: 8257 RVA: 0x000596F2 File Offset: 0x000586F2
		public CompilerAttribute(string stName, string stValue)
		{
			this.Name = stName;
			this.Value = stValue;
		}

		// Token: 0x1700086E RID: 2158
		// (get) Token: 0x06002042 RID: 8258 RVA: 0x0005971E File Offset: 0x0005871E
		public string Value { get; } = string.Empty;

		// Token: 0x1700086F RID: 2159
		// (get) Token: 0x06002043 RID: 8259 RVA: 0x00059726 File Offset: 0x00058726
		public string Name { get; } = string.Empty;
	}
}
