using System;

namespace _3S.CoDeSys.LanguageModelManager.Interfaces
{
	// Token: 0x020001D1 RID: 465
	public interface ILMCompiledSetArchiveStorageFormatFactory
	{
		// Token: 0x0600211D RID: 8477
		ILMCompiledSetArchiveStorageFormat CreateCompiledLibraryStorageFormat();

		// Token: 0x0600211E RID: 8478
		ILMCompiledSetArchiveStorageFormat CreateCompileInfoStorageFormat();
	}
}
