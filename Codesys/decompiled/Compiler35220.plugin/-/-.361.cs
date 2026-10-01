using System;
using System.Runtime.CompilerServices;
using \u0019;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001C
{
	// Token: 0x020003AC RID: 940
	internal sealed class \u0013 : EmptyVisitor351900
	{
		// Token: 0x170008D2 RID: 2258
		// (get) Token: 0x0600363F RID: 13887 RVA: 0x000D9FF0 File Offset: 0x000D81F0
		private IScope5 Scope { get; }

		// Token: 0x06003640 RID: 13888 RVA: 0x000D9FF8 File Offset: 0x000D81F8
		private \u0013(IScope5 \u009B\u0002)
		{
			this.Scope = \u009B\u0002;
		}

		// Token: 0x06003641 RID: 13889 RVA: 0x000DA008 File Offset: 0x000D8208
		internal static void \u0001(_IExpression \u0002, IScope5 \u0003)
		{
			StandardTraverser ivisit = new StandardTraverser(new \u0013(\u0003));
			\u0002.Accept(ivisit);
		}

		// Token: 0x06003642 RID: 13890 RVA: 0x000DA028 File Offset: 0x000D8228
		private static bool \u0001(_IExpression \u0002, IScope5 \u0003)
		{
			if (\u0002.Type is IReferenceType)
			{
				return true;
			}
			IUserdefType userdefType = \u0002.Type as IUserdefType;
			if (userdefType != null)
			{
				ISignature signature = \u0003[userdefType.SignatureId];
				if (signature != null && signature.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06003643 RID: 13891 RVA: 0x000DA074 File Offset: 0x000D8274
		internal static void \u0001(_IAssignmentExpression \u0002, IScope5 \u0003)
		{
			if (\u0013.\u0001(\u0002._LValue, \u0003))
			{
				\u0002._RValue = \u0003.\u0001(Operator.Adr, \u0002._RValue);
			}
		}

		// Token: 0x06003644 RID: 13892 RVA: 0x000DA098 File Offset: 0x000D8298
		public override void visit(_IStructureInitialization errorst)
		{
			foreach (_IAssignmentExpression u in errorst._CompoInits)
			{
				\u0013.\u0001(u, this.Scope);
			}
		}

		// Token: 0x04000A92 RID: 2706
		[CompilerGenerated]
		private readonly IScope5 \u0001;
	}
}
