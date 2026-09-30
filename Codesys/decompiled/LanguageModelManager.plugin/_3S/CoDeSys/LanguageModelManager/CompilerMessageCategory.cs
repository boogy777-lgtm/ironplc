using System;
using System.Drawing;
using System.Reflection;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000151 RID: 337
	[TypeGuid("{97F48D64-A2A3-4856-B640-75C046E37EA9}")]
	public class CompilerMessageCategory : IMessageCategory
	{
		// Token: 0x1700076B RID: 1899
		// (get) Token: 0x06001B8C RID: 7052 RVA: 0x00005F0F File Offset: 0x00004F0F
		public Icon Icon
		{
			get
			{
				return null;
			}
		}

		// Token: 0x1700076C RID: 1900
		// (get) Token: 0x06001B8D RID: 7053 RVA: 0x0004E20B File Offset: 0x0004D20B
		public string Text
		{
			get
			{
				return Strings.Build;
			}
		}

		// Token: 0x040005CF RID: 1487
		[Obfuscation(Feature = "rename")]
		public static readonly CompilerMessageCategory Singleton = new CompilerMessageCategory();
	}
}
