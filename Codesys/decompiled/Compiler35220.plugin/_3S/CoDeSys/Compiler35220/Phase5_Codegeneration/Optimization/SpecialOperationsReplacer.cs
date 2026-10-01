using System;
using System.Collections.Generic;
using \u000E;
using \u0010;
using \u0011;
using \u0013;
using \u0019;
using \u001B;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0084;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x020002C7 RID: 711
	public class SpecialOperationsReplacer : IReplacer, IExpressionStatementReplacer
	{
		// Token: 0x06002B23 RID: 11043 RVA: 0x00098094 File Offset: 0x00096294
		private SpecialOperationsReplacer(global::\u000E.\u0011 context)
		{
			this.\u0001 = context;
			this.\u0001 = new ExpressionStatementReplacerVisitor(this, context);
		}

		// Token: 0x06002B24 RID: 11044 RVA: 0x000980B0 File Offset: 0x000962B0
		internal static bool \u0001(Operator \u0002)
		{
			return SpecialOperationsReplacer.\u0001.ContainsKey(\u0002);
		}

		// Token: 0x06002B25 RID: 11045 RVA: 0x000980C0 File Offset: 0x000962C0
		internal static ReplacerController \u0001(global::\u000E.\u0011 \u0002)
		{
			return new ReplacerController(new SpecialOperationsReplacer(\u0002), new global::\u0013.\u0008());
		}

		// Token: 0x06002B26 RID: 11046 RVA: 0x000980D4 File Offset: 0x000962D4
		public void ReplaceCode(_ICompiledPOU cpou)
		{
			this.\u0001.ReplaceCode(cpou);
		}

		// Token: 0x06002B27 RID: 11047 RVA: 0x000980E4 File Offset: 0x000962E4
		public _IStatement ReplaceExpressionStatement(_IExpressionStatement expressionStatement, _ICompiledPOU cpou)
		{
			_IOperatorExpression ioperatorExpression = expressionStatement._Expr as _IOperatorExpression;
			if (ioperatorExpression != null)
			{
				return this.\u0001(ioperatorExpression, cpou);
			}
			return null;
		}

		// Token: 0x06002B28 RID: 11048 RVA: 0x0009810C File Offset: 0x0009630C
		private _IStatement \u0001(_IOperatorExpression \u0002, _ICompiledPOU \u0003)
		{
			this.\u0001.CompiledPOU = \u0003;
			if (!SpecialOperationsReplacer.\u0001.ContainsKey(\u0002.Code))
			{
				return null;
			}
			return SpecialOperationsReplacer.\u0001[\u0002.Code](\u0002, this.\u0001);
		}

		// Token: 0x0400082F RID: 2095
		private global::\u000E.\u0011 \u0001;

		// Token: 0x04000830 RID: 2096
		private readonly ExpressionStatementReplacerVisitor \u0001;

		// Token: 0x04000831 RID: 2097
		private static readonly Dictionary<Operator, Func<_IOperatorExpression, global::\u000E.\u0011, _IStatement>> \u0001 = new Dictionary<Operator, Func<_IOperatorExpression, global::\u000E.\u0011, _IStatement>>
		{
			{
				Operator.__Init,
				new Func<_IOperatorExpression, global::\u000E.\u0011, _IStatement>(global::\u001B.\u0008.\u0001)
			},
			{
				Operator.__CallInitFunction,
				new Func<_IOperatorExpression, global::\u000E.\u0011, _IStatement>(global::\u0019.\u000E.\u0001)
			},
			{
				Operator.__Delete,
				new Func<_IOperatorExpression, global::\u000E.\u0011, _IStatement>(\u0084.\u0017.\u0001)
			},
			{
				Operator.__MemoryBarrier,
				new Func<_IOperatorExpression, global::\u000E.\u0011, _IStatement>(global::\u0011.\u0011.\u0001)
			},
			{
				Operator.__PropertyInfo,
				new Func<_IOperatorExpression, global::\u000E.\u0011, _IStatement>(global::\u0010.\u0006.\u0001)
			}
		};
	}
}
