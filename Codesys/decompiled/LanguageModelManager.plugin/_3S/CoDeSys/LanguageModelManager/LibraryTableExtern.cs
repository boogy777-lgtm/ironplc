using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000CF RID: 207
	internal class LibraryTableExtern : ILibraryTable4, ILibraryTable3, ILibraryTable2, ILibraryTable
	{
		// Token: 0x06000E9B RID: 3739 RVA: 0x00026CCE File Offset: 0x00025CCE
		public LibraryTableExtern(_ILibraryTable libtab)
		{
			this._libtab = libtab;
		}

		// Token: 0x06000E9C RID: 3740 RVA: 0x00026CDD File Offset: 0x00025CDD
		public IPreCompileContext9 GetLibraryContextByNamespace(string stNamespace, IPreCompileContext9 precomlocal)
		{
			return this._libtab.GetLibraryContextByNamespace(stNamespace, precomlocal as _IPreCompileContext);
		}

		// Token: 0x06000E9D RID: 3741 RVA: 0x00026CF1 File Offset: 0x00025CF1
		public IList<IPreCompileContext9> GetVisibleLibraries(IPreCompileContext9 precomlocal)
		{
			return new LList<IPreCompileContext9>(this._libtab.GetVisibleLibraries(precomlocal as _IPreCompileContext));
		}

		// Token: 0x06000E9E RID: 3742 RVA: 0x00026D09 File Offset: 0x00025D09
		public string GetNamespaceOfLibrary(string stLibraryId, IPreCompileContext9 precomlocal)
		{
			return this._libtab.GetNamespaceOfLibrary(precomlocal as _IPreCompileContext, stLibraryId);
		}

		// Token: 0x06000E9F RID: 3743 RVA: 0x00026D1D File Offset: 0x00025D1D
		public IEnumerable<string> AllReferencedLibraries()
		{
			return this._libtab.AllReferencedLibraries();
		}

		// Token: 0x06000EA0 RID: 3744 RVA: 0x00026D2A File Offset: 0x00025D2A
		public bool GetQualifiedOnly(IPreCompileContext precomLocal, string stLibraryIdToLookup)
		{
			return this._libtab.GetQualifiedOnly(precomLocal as _IPreCompileContext, stLibraryIdToLookup);
		}

		// Token: 0x06000EA1 RID: 3745 RVA: 0x00026D3E File Offset: 0x00025D3E
		public bool? GetQualifiedOnlyRecursive(IPreCompileContext precomLocal, string stLibraryIdToLookup)
		{
			return this._libtab.GetQualifiedOnlyRecursive(precomLocal as _IPreCompileContext, stLibraryIdToLookup);
		}

		// Token: 0x06000EA2 RID: 3746 RVA: 0x00026D52 File Offset: 0x00025D52
		public string GetLocalLibraryNamespaceRecursive(IPreCompileContext precomLocal, string stLibraryToFind)
		{
			return this._libtab.GetLocalLibraryNamespaceRecursiveExt(precomLocal as _IPreCompileContext, stLibraryToFind);
		}

		// Token: 0x0400029C RID: 668
		private readonly _ILibraryTable _libtab;
	}
}
