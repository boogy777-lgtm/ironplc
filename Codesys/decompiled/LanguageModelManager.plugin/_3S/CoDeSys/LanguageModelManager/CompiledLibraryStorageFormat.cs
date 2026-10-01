using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000B0 RID: 176
	internal class CompiledLibraryStorageFormat : ArchiveStorageFormat
	{
		// Token: 0x06000A58 RID: 2648 RVA: 0x000179E0 File Offset: 0x000169E0
		internal CompiledLibraryStorageFormat()
		{
			this.Create(PreCompileSetArchiveStorageFormat.ClassicalFormat);
		}

		// Token: 0x06000A59 RID: 2649 RVA: 0x000179EF File Offset: 0x000169EF
		internal CompiledLibraryStorageFormat(PreCompileSetArchiveStorageFormat compiledLibStorageFormat)
		{
			this.Create(compiledLibStorageFormat);
		}

		// Token: 0x06000A5A RID: 2650 RVA: 0x000179FE File Offset: 0x000169FE
		private void Create(PreCompileSetArchiveStorageFormat compiledLibStorageFormat)
		{
			this.PreserveCompiledLibComments = APEnvironmentFacade.Instance.IsProjectInfoObjectBoolFlagSet(APEnvironmentFacade.Instance.PrimaryProjectHandle, "PreserveCompiledLibComments");
			this.ExcludeParseTreeInPOU = (compiledLibStorageFormat == PreCompileSetArchiveStorageFormat.MemoryOptimizedFormat);
		}

		// Token: 0x04000180 RID: 384
		internal bool PreserveCompiledLibComments;

		// Token: 0x04000181 RID: 385
		internal bool ExcludeParseTreeInPOU;
	}
}
