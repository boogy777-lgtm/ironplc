using System;
using System.Runtime.CompilerServices;
using \u0002;
using \u0003;
using \u0006;
using \u000E;
using \u001B;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x020002CB RID: 715
	public class StructAndArrayInitReplacer : IReplacer, IExpressionStatementReplacer
	{
		// Token: 0x17000785 RID: 1925
		// (get) Token: 0x06002B44 RID: 11076 RVA: 0x000985A8 File Offset: 0x000967A8
		// (set) Token: 0x06002B45 RID: 11077 RVA: 0x000985B0 File Offset: 0x000967B0
		public bool DoCallAfterInitAttribute { get; set; }

		// Token: 0x17000786 RID: 1926
		// (get) Token: 0x06002B46 RID: 11078 RVA: 0x000985BC File Offset: 0x000967BC
		// (set) Token: 0x06002B47 RID: 11079 RVA: 0x000985C4 File Offset: 0x000967C4
		internal int CurrentArrayIndex { get; set; }

		// Token: 0x06002B48 RID: 11080 RVA: 0x000985D0 File Offset: 0x000967D0
		internal StructAndArrayInitReplacer(global::\u000E.\u0011 context)
		{
			this.\u0001 = context;
			this.\u0001 = new ExpressionStatementReplacerVisitor(this, this.\u0001);
			this.\u0001 = new global::\u0002.\u0007(context, this);
			this.\u0001 = new global::\u0003.\u0012(context, this);
			this.DoCallAfterInitAttribute = true;
		}

		// Token: 0x06002B49 RID: 11081 RVA: 0x00098620 File Offset: 0x00096820
		internal static ReplacerController \u0001(global::\u000E.\u0011 \u0002)
		{
			return new ReplacerController(new StructAndArrayInitReplacer(\u0002), new \u001B.\u000E());
		}

		// Token: 0x06002B4A RID: 11082 RVA: 0x00098634 File Offset: 0x00096834
		public _IStatement ReplaceExpressionStatement(_IExpressionStatement expressionStatement, _ICompiledPOU cpou)
		{
			_IAssignmentExpression iassignmentExpression = expressionStatement._Expr as _IAssignmentExpression;
			if (iassignmentExpression == null)
			{
				return expressionStatement;
			}
			_IStatement istatement = this.\u0001.\u0001(iassignmentExpression, this.DoCallAfterInitAttribute);
			if (istatement != null)
			{
				return istatement;
			}
			return this.\u0001.\u0001(iassignmentExpression);
		}

		// Token: 0x06002B4B RID: 11083 RVA: 0x00098678 File Offset: 0x00096878
		public void ReplaceInCode(_ISequenceStatement statement)
		{
			statement.\u0001(this.\u0001);
		}

		// Token: 0x06002B4C RID: 11084 RVA: 0x00098688 File Offset: 0x00096888
		public void ReplaceCode(_ICompiledPOU cpou)
		{
			this.\u0001.CompiledPOU = cpou;
			cpou.GetParseTree().\u0001(this.\u0001);
		}

		// Token: 0x0400083D RID: 2109
		private readonly ExpressionStatementReplacerVisitor \u0001;

		// Token: 0x0400083E RID: 2110
		private readonly global::\u0002.\u0007 \u0001;

		// Token: 0x0400083F RID: 2111
		private readonly global::\u0003.\u0012 \u0001;

		// Token: 0x04000840 RID: 2112
		private global::\u000E.\u0011 \u0001;

		// Token: 0x04000841 RID: 2113
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x04000842 RID: 2114
		[CompilerGenerated]
		private int \u0001;
	}
}
