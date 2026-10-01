using System;
using System.Runtime.CompilerServices;
using \u0002;
using _3S.CoDeSys.Compiler35220.Compile.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0010
{
	// Token: 0x020002FE RID: 766
	internal sealed class \u0007 : \u0006
	{
		// Token: 0x170007D3 RID: 2003
		// (get) Token: 0x06002EC9 RID: 11977 RVA: 0x000B0280 File Offset: 0x000AE480
		private ConstantFolder Folder { get; }

		// Token: 0x06002ECA RID: 11978 RVA: 0x000B0288 File Offset: 0x000AE488
		public \u0007(ConstantFolder \u0081\u0005) : base(\u0081\u0005)
		{
			this.Folder = \u0081\u0005;
		}

		// Token: 0x06002ECB RID: 11979 RVA: 0x000B0298 File Offset: 0x000AE498
		private new static TypeClass \u0001(_IExpression \u0002)
		{
			TypeClass result = TypeClass.None;
			if (\u0002.Type != null)
			{
				result = \u0002.Type.Class;
			}
			return result;
		}

		// Token: 0x06002ECC RID: 11980 RVA: 0x000B02C0 File Offset: 0x000AE4C0
		private new static bool \u0001(TypeClass \u0002, TypeClass \u0003)
		{
			return \u0003 != \u0002 && !TypeTable.IsEquivalent(\u0002, \u0003) && \u0002 != TypeClass.None && \u0003 != TypeClass.None;
		}

		// Token: 0x06002ECD RID: 11981 RVA: 0x000B02E0 File Offset: 0x000AE4E0
		public override void \u0001(_IOperatorExpression \u0002)
		{
			bool flag = false;
			for (int i = 0; i < \u0002._OperandsList.Count; i++)
			{
				TypeClass u = \u0007.\u0001(\u0002[i]);
				\u0002[i] = base.\u0001(\u0002[i]);
				base.\u0002(\u0002[i]);
				TypeClass u2 = \u0007.\u0001(\u0002[i]);
				flag = (flag || \u0007.\u0001(u, u2));
			}
			if (flag)
			{
				\u0002._CompiledType = null;
				this.Folder.\u0001(\u0002);
			}
		}

		// Token: 0x040008E8 RID: 2280
		[CompilerGenerated]
		private new readonly ConstantFolder \u0001;
	}
}
