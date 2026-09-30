using System;
using System.Collections.Generic;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000BA RID: 186
	internal class LibraryTableCache
	{
		// Token: 0x06000AF0 RID: 2800 RVA: 0x0001C72D File Offset: 0x0001B72D
		internal void Clear()
		{
			this._libTablesForApp.Clear();
		}

		// Token: 0x06000AF1 RID: 2801 RVA: 0x0001C73A File Offset: 0x0001B73A
		internal _ILibraryTable Add(Guid appGuid, PreCompileContext preCompileContext, _ILibraryTable libnew)
		{
			if (!this._libTablesForApp.ContainsKey(appGuid))
			{
				this._libTablesForApp[appGuid] = new LDictionary<PreCompileContext, _ILibraryTable>();
			}
			this._libTablesForApp[appGuid][preCompileContext] = libnew;
			return libnew;
		}

		// Token: 0x06000AF2 RID: 2802 RVA: 0x0001C76F File Offset: 0x0001B76F
		internal bool Contains(Guid appGuid, PreCompileContext preCompileContext)
		{
			return this._libTablesForApp.ContainsKey(appGuid) && this._libTablesForApp[appGuid].ContainsKey(preCompileContext);
		}

		// Token: 0x06000AF3 RID: 2803 RVA: 0x0001C793 File Offset: 0x0001B793
		public _ILibraryTable Get(Guid appGuid, PreCompileContext preCompileContext)
		{
			return this._libTablesForApp[appGuid][preCompileContext];
		}

		// Token: 0x040001BD RID: 445
		private readonly LDictionary<Guid, IDictionary<PreCompileContext, _ILibraryTable>> _libTablesForApp = new LDictionary<Guid, IDictionary<PreCompileContext, _ILibraryTable>>();
	}
}
