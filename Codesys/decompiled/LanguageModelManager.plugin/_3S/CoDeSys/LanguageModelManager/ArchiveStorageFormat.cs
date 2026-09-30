using System;
using _3S.CoDeSys.LanguageModelManager.Interfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000AF RID: 175
	internal abstract class ArchiveStorageFormat : ILMCompiledSetArchiveStorageFormat, IDisposable
	{
		// Token: 0x06000A56 RID: 2646 RVA: 0x000179AB File Offset: 0x000169AB
		protected ArchiveStorageFormat()
		{
			this._oldValue = ArchiveStorageConfig.Singleton.StorageFormat;
			ArchiveStorageConfig.Singleton.StorageFormat = this;
		}

		// Token: 0x06000A57 RID: 2647 RVA: 0x000179CE File Offset: 0x000169CE
		public virtual void Dispose()
		{
			ArchiveStorageConfig.Singleton.StorageFormat = this._oldValue;
		}

		// Token: 0x0400017F RID: 383
		protected ArchiveStorageFormat _oldValue;
	}
}
