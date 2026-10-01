using System;
using _3S.CoDeSys.LanguageModelManager.Interfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000AE RID: 174
	public class ArchiveStorageFormatFactory : ILMCompiledSetArchiveStorageFormatFactory
	{
		// Token: 0x06000A53 RID: 2643 RVA: 0x0001799D File Offset: 0x0001699D
		public ILMCompiledSetArchiveStorageFormat CreateCompiledLibraryStorageFormat()
		{
			return new CompiledLibraryStorageFormat();
		}

		// Token: 0x06000A54 RID: 2644 RVA: 0x000179A4 File Offset: 0x000169A4
		public ILMCompiledSetArchiveStorageFormat CreateCompileInfoStorageFormat()
		{
			return new CompileInfoStorageFormat();
		}
	}
}
