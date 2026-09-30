using System;
using \u0010;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Scopes;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u007F
{
	// Token: 0x02000154 RID: 340
	internal sealed class \u0004 : ISymbolTables
	{
		// Token: 0x060017AE RID: 6062 RVA: 0x00049514 File Offset: 0x00047714
		public \u0004(_ICompileContext \u0001\u0002)
		{
			this.\u0001 = \u0001\u0002;
			this.\u0001 = new \u0002(APEnvironmentFacade.Instance.LanguageModelMgr._GetPrecompileContext(this.\u0001.ApplicationGuid));
			this.\u0001 = new LDictionary<_IPreCompileContext, SymbolTable>();
		}

		// Token: 0x060017AF RID: 6063 RVA: 0x00049554 File Offset: 0x00047754
		public void \u0001(_IPreCompileContext \u0002, _IPreCompileContext \u0003)
		{
			this.\u0001(\u0002, APEnvironmentFacade.Instance.LanguageModelMgr.Pool).\u0001();
		}

		// Token: 0x060017B0 RID: 6064 RVA: 0x00049574 File Offset: 0x00047774
		internal SymbolTable \u0001(_IPreCompileContext \u0002, _IPreCompileContext \u0003)
		{
			if (!this.\u0001.ContainsKey(\u0002))
			{
				this.\u0001.Add(\u0002, new SymbolTable(\u0002, \u0003, this.\u0001, this.\u0001));
			}
			return this.\u0001[\u0002];
		}

		// Token: 0x04000435 RID: 1077
		private readonly LDictionary<_IPreCompileContext, SymbolTable> \u0001;

		// Token: 0x04000436 RID: 1078
		private readonly \u0002 \u0001;

		// Token: 0x04000437 RID: 1079
		private readonly _ICompileContext \u0001;
	}
}
