using System;
using \u0007;
using \u0008;
using \u000E;
using \u0011;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace \u0005
{
	// Token: 0x02000252 RID: 594
	internal sealed class \u0006 : global::\u0008.\u0010, IReplacer
	{
		// Token: 0x060026DF RID: 9951 RVA: 0x00086928 File Offset: 0x00084B28
		public \u0006(\u0081.\u0010 \u0096\u0007, global::\u0011.\u000F \u0012\u0006, global::\u000E.\u0011 \u0083\u0005) : base(\u0096\u0007, new global::\u0007.\u000E(), \u0083\u0005)
		{
			this.\u0001 = null;
			this.\u0001 = \u0012\u0006;
		}

		// Token: 0x060026E0 RID: 9952 RVA: 0x00086948 File Offset: 0x00084B48
		protected override _IExpression \u0001(_IExpression \u0002, bool \u0003 = true)
		{
			\u0002.Accept(this);
			return \u0002;
		}

		// Token: 0x060026E1 RID: 9953 RVA: 0x00086954 File Offset: 0x00084B54
		public override void \u0001(_IExpressionStatement \u0002)
		{
			_IAssignmentExpression iassignmentExpression = \u0002._Expr as _IAssignmentExpression;
			if (iassignmentExpression != null)
			{
				iassignmentExpression._LValue.Accept(this);
				iassignmentExpression._RValue.Accept(this);
				this.\u0001.\u0001(iassignmentExpression, this.\u0001);
				return;
			}
			\u0002._Expr.Accept(this);
		}

		// Token: 0x060026E2 RID: 9954 RVA: 0x000869A8 File Offset: 0x00084BA8
		public override void \u0001(_ICallExpression \u0002)
		{
			base.\u0001(\u0002);
			this.\u0001.\u0001(\u0002, this.\u0001);
		}

		// Token: 0x060026E3 RID: 9955 RVA: 0x000869C4 File Offset: 0x00084BC4
		public override void \u0001(_IAssignmentExpression \u0002)
		{
			base.\u0001(\u0002);
			this.\u0001.\u0002(\u0002, this.\u0001);
		}

		// Token: 0x060026E4 RID: 9956 RVA: 0x000869E0 File Offset: 0x00084BE0
		public void ReplaceCode(_ICompiledPOU cpou)
		{
			this.\u0001 = cpou;
			cpou.GetParseTree().Accept(this);
		}

		// Token: 0x04000714 RID: 1812
		private new readonly global::\u0011.\u000F \u0001;

		// Token: 0x04000715 RID: 1813
		private new _ICompiledPOU \u0001;
	}
}
