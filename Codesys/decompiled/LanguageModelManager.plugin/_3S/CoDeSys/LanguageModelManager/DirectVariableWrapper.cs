using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000142 RID: 322
	public class DirectVariableWrapper
	{
		// Token: 0x06001B25 RID: 6949 RVA: 0x0004D2E4 File Offset: 0x0004C2E4
		internal DirectVariableWrapper(IDirectVariable dirvar, IVariable2 var)
		{
			this._dirvar = dirvar;
			this._var = var;
			this._nHash = (this._dirvar.Location.GetHashCode() ^ this._dirvar.Size.GetHashCode() ^ this._dirvar.Incomplete.GetHashCode());
			if (this._var != null)
			{
				this._nHash ^= this._var.GetHashCode();
			}
			for (int i = 0; i < this._dirvar.Components.Length; i++)
			{
				this._nHash ^= this._dirvar.Components[i].GetHashCode();
			}
		}

		// Token: 0x06001B26 RID: 6950 RVA: 0x0004D3AD File Offset: 0x0004C3AD
		public override int GetHashCode()
		{
			return this._nHash;
		}

		// Token: 0x06001B27 RID: 6951 RVA: 0x0004D3B8 File Offset: 0x0004C3B8
		public override bool Equals(object obj)
		{
			DirectVariableWrapper directVariableWrapper = obj as DirectVariableWrapper;
			return directVariableWrapper != null && this._var == directVariableWrapper._var && (this._var == null || this._var.IsEqual(directVariableWrapper._var, false)) && this._dirvar.IsEqual(directVariableWrapper._dirvar);
		}

		// Token: 0x040005B0 RID: 1456
		internal IDirectVariable _dirvar;

		// Token: 0x040005B1 RID: 1457
		internal IVariable2 _var;

		// Token: 0x040005B2 RID: 1458
		internal int _nHash;
	}
}
