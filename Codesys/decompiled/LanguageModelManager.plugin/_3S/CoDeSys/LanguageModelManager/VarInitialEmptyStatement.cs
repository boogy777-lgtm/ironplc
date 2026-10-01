using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000A7 RID: 167
	[TypeGuid("{1B1F427C-9D38-4148-B696-265F0B30F41F}")]
	[StorageVersion("3.3.0.0")]
	public class VarInitialEmptyStatement : EmptyStatement, _IVarInitialEmptyStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement
	{
		// Token: 0x06000A23 RID: 2595 RVA: 0x0001718D File Offset: 0x0001618D
		public VarInitialEmptyStatement()
		{
		}

		// Token: 0x06000A24 RID: 2596 RVA: 0x00017195 File Offset: 0x00016195
		internal VarInitialEmptyStatement(_IVariable varInitial, _ISignature signInitial)
		{
			this._varInitial = varInitial;
			this._signInitial = signInitial;
		}

		// Token: 0x06000A25 RID: 2597 RVA: 0x000171AB File Offset: 0x000161AB
		public _IVariable GetVariable()
		{
			return this._varInitial;
		}

		// Token: 0x06000A26 RID: 2598 RVA: 0x000171B3 File Offset: 0x000161B3
		public _ISignature GetSignature()
		{
			return this._signInitial;
		}

		// Token: 0x04000175 RID: 373
		private readonly _IVariable _varInitial;

		// Token: 0x04000176 RID: 374
		private readonly _ISignature _signInitial;
	}
}
