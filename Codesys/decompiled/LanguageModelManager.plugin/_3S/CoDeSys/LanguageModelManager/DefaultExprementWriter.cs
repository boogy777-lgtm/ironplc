using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000C6 RID: 198
	internal class DefaultExprementWriter : ICompilerServiceExprementWriter
	{
		// Token: 0x06000C31 RID: 3121 RVA: 0x0001F51F File Offset: 0x0001E51F
		public DefaultExprementWriter(ICompiler compiler)
		{
			this.Compiler = compiler;
		}

		// Token: 0x06000C32 RID: 3122 RVA: 0x0001F52E File Offset: 0x0001E52E
		public string WriteExprement(_IExprement exprement, WriteExprementFlags flags)
		{
			return this.Compiler.DumpExprement(exprement);
		}

		// Token: 0x0400022D RID: 557
		private readonly ICompiler Compiler;
	}
}
