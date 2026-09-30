using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000CE RID: 206
	[TypeGuid("{62781875-67C3-42A1-9A2B-ADE14ADB8D7E}")]
	[StorageVersion("3.5.3.0")]
	internal class stringtuple : GenericObject2
	{
		// Token: 0x0400029A RID: 666
		[DefaultSerialization("str1")]
		[StorageVersion("3.5.3.0")]
		internal string str1;

		// Token: 0x0400029B RID: 667
		[DefaultSerialization("str2")]
		[StorageVersion("3.5.3.0")]
		internal string str2;
	}
}
