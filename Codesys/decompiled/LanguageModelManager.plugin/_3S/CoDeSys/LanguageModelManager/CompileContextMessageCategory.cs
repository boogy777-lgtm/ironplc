using System;
using System.Drawing;
using System.Reflection;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000031 RID: 49
	[TypeGuid("{879A1ADE-CA5F-4780-807B-79A0346051FE}")]
	public class CompileContextMessageCategory : IMessageCategory
	{
		// Token: 0x17000065 RID: 101
		// (get) Token: 0x06000251 RID: 593 RVA: 0x00005F0F File Offset: 0x00004F0F
		public Icon Icon
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000066 RID: 102
		// (get) Token: 0x06000252 RID: 594 RVA: 0x00007F12 File Offset: 0x00006F12
		public string Text
		{
			get
			{
				return Strings.CompileContext;
			}
		}

		// Token: 0x04000056 RID: 86
		[Obfuscation(Feature = "rename")]
		public static readonly CompileContextMessageCategory Singleton = new CompileContextMessageCategory();
	}
}
