using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.Compiler35220.OnlineChange
{
	// Token: 0x02000364 RID: 868
	public sealed class VarInfoComparer_NoFlag : IComparer<IVariableInfo>, IEqualityComparer<IVariableInfo>
	{
		// Token: 0x060033E8 RID: 13288 RVA: 0x000CBF58 File Offset: 0x000CA158
		public int Compare(IVariableInfo x, IVariableInfo y)
		{
			if (x == y)
			{
				return 0;
			}
			if (y == null)
			{
				return 1;
			}
			if (x == null)
			{
				return -1;
			}
			int num = x.SignatureId.CompareTo(y.SignatureId);
			if (num != 0)
			{
				return num;
			}
			return x.VariableId.CompareTo(y.VariableId);
		}

		// Token: 0x060033E9 RID: 13289 RVA: 0x000CBFA4 File Offset: 0x000CA1A4
		public bool Equals(IVariableInfo x, IVariableInfo y)
		{
			return this.Compare(x, y) == 0;
		}

		// Token: 0x060033EA RID: 13290 RVA: 0x000CBFB4 File Offset: 0x000CA1B4
		public int GetHashCode(IVariableInfo obj)
		{
			return obj.VariableId * 397 ^ obj.SignatureId;
		}

		// Token: 0x04000A08 RID: 2568
		public static readonly VarInfoComparer_NoFlag Instance = new VarInfoComparer_NoFlag();
	}
}
