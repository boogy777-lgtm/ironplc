using System;
using \u0001;
using \u0006;
using \u000E;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x02000255 RID: 597
	public class AssignmentStatementReplacer : IReplacer, IExpressionStatementReplacer
	{
		// Token: 0x06002735 RID: 10037 RVA: 0x0008722C File Offset: 0x0008542C
		private AssignmentStatementReplacer(global::\u000E.\u0011 context, Func<_IAssignmentExpression, global::\u000E.\u0011, _IStatement>[] replacers)
		{
			this.\u0001 = new ExpressionStatementReplacerVisitor(this, context);
			this.\u0001 = context;
			this.\u0001 = replacers;
		}

		// Token: 0x06002736 RID: 10038 RVA: 0x00087250 File Offset: 0x00085450
		public void ReplaceCode(_ICompiledPOU cpou)
		{
			this.\u0001.CompiledPOU = cpou;
			cpou.GetParseTree().\u0001(this.\u0001);
		}

		// Token: 0x06002737 RID: 10039 RVA: 0x00087270 File Offset: 0x00085470
		public _IStatement ReplaceExpressionStatement(_IExpressionStatement expressionStatement, _ICompiledPOU cpou)
		{
			_IAssignmentExpression iassignmentExpression = expressionStatement._Expr as _IAssignmentExpression;
			if (iassignmentExpression != null)
			{
				Func<_IAssignmentExpression, global::\u000E.\u0011, _IStatement>[] u = this.\u0001;
				for (int i = 0; i < u.Length; i++)
				{
					_IStatement istatement = u[i](iassignmentExpression, this.\u0001);
					if (istatement != null)
					{
						return istatement;
					}
				}
			}
			return expressionStatement;
		}

		// Token: 0x06002738 RID: 10040 RVA: 0x000872B8 File Offset: 0x000854B8
		internal static ReplacerController \u0001(global::\u000E.\u0011 \u0002, Func<_IAssignmentExpression, global::\u000E.\u0011, _IStatement>[] \u0003, Func<_IAssignmentExpression, global::\u000E.\u0011, bool>[] \u0004)
		{
			return new ReplacerController(new AssignmentStatementReplacer(\u0002, \u0003), new global::\u0001.\u0008(\u0002, \u0004));
		}

		// Token: 0x04000719 RID: 1817
		private readonly ExpressionStatementReplacerVisitor \u0001;

		// Token: 0x0400071A RID: 1818
		private global::\u000E.\u0011 \u0001;

		// Token: 0x0400071B RID: 1819
		private readonly Func<_IAssignmentExpression, global::\u000E.\u0011, _IStatement>[] \u0001;
	}
}
