using System;
using System.Drawing;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000152 RID: 338
	[TypeGuid("{217bc73e-759b-4a3c-bfa1-991c938a6541}")]
	public class PreCompileMessageCategory : IMessageCategory
	{
		// Token: 0x1700076D RID: 1901
		// (get) Token: 0x06001B90 RID: 7056 RVA: 0x00005F0F File Offset: 0x00004F0F
		public Icon Icon
		{
			get
			{
				return null;
			}
		}

		// Token: 0x1700076E RID: 1902
		// (get) Token: 0x06001B91 RID: 7057 RVA: 0x0004E21E File Offset: 0x0004D21E
		public string Text
		{
			get
			{
				return Strings.Precompile;
			}
		}

		// Token: 0x040005D0 RID: 1488
		public static readonly PreCompileMessageCategory Singleton = new PreCompileMessageCategory();
	}
}
