using System;
using System.Runtime.CompilerServices;
using \u0006;
using \u000E;
using \u0019;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x020002A8 RID: 680
	public class InterfaceComparisonReplacer : AbstractReplacer, IReplacer
	{
		// Token: 0x1700075E RID: 1886
		// (get) Token: 0x06002A8D RID: 10893 RVA: 0x00094F10 File Offset: 0x00093110
		private \u0081.\u0010 ReplacerVisitor { get; }

		// Token: 0x1700075F RID: 1887
		// (get) Token: 0x06002A8E RID: 10894 RVA: 0x00094F18 File Offset: 0x00093118
		private IScope5 _Scope
		{
			get
			{
				return this.\u0001._Scope;
			}
		}

		// Token: 0x06002A8F RID: 10895 RVA: 0x00094F34 File Offset: 0x00093134
		private InterfaceComparisonReplacer(global::\u000E.\u0011 context)
		{
			this.\u0001 = context;
			this.ReplacerVisitor = \u0081.\u0010.\u0001(this, context);
		}

		// Token: 0x06002A90 RID: 10896 RVA: 0x00094F50 File Offset: 0x00093150
		public void ReplaceCode(_ICompiledPOU cpou)
		{
			this.\u0001 = cpou;
			this.ReplacerVisitor.ReplaceCode(cpou);
		}

		// Token: 0x06002A91 RID: 10897 RVA: 0x00094F68 File Offset: 0x00093168
		public override _IExpression ReplaceOperatorExpression(_IOperatorExpression operatorExpression)
		{
			Operator code = operatorExpression.Code;
			if (code - Operator.Eq <= 1 || code - Operator.Equal <= 1)
			{
				return this.\u0001(operatorExpression);
			}
			return operatorExpression;
		}

		// Token: 0x06002A92 RID: 10898 RVA: 0x00094F9C File Offset: 0x0009319C
		internal _IExpression \u0001(_IExpression \u0002)
		{
			if (!global::\u0006.\u0011.\u0001(\u0002.Type.DeRefType, this._Scope))
			{
				return null;
			}
			if (\u0002 is IAssignmentExpression)
			{
				return \u0002;
			}
			_ICompoAccessExpression icompoAccessExpression = global::\u0019.\u0003.\u0001(\u0002.Duplicate() as _IExpression, Token.Empty);
			icompoAccessExpression._Right = global::\u0019.\u0003.\u0001("__Interface");
			return icompoAccessExpression;
		}

		// Token: 0x06002A93 RID: 10899 RVA: 0x00094FF4 File Offset: 0x000931F4
		private _IExpression \u0001(_IOperatorExpression \u0002)
		{
			_IExpression iexpression = this.\u0001(\u0002._OperandsList[0]);
			_IExpression iexpression2 = this.\u0001(\u0002._OperandsList[1]);
			if (iexpression != null && iexpression2 != null)
			{
				_ICallExpression icallExpression = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001("__CompareInterfaces"), Token.Empty), Token.Empty);
				_IVariableExpression expVariable = global::\u0019.\u0003.\u0001("ppi1");
				_IVariableExpression expVariable2 = global::\u0019.\u0003.\u0001("ppi2");
				icallExpression.AddParam(iexpression, expVariable);
				icallExpression.AddParam(iexpression2, expVariable2);
				_IExpression u = icallExpression;
				if (\u0002.Code == Operator.Ne || \u0002.Code == Operator.NotEqual)
				{
					_IOperatorExpression ioperatorExpression = global::\u0019.\u0003.\u0001(Operator.Not);
					ioperatorExpression.AddOperand(icallExpression);
					u = ioperatorExpression;
				}
				return this.\u0001.Generator.\u0001<_IExpression>(u, this._Scope, this.\u0001);
			}
			if (iexpression != null)
			{
				\u0002[0] = this.\u0001.Generator.\u0001<_IExpression>(iexpression, this._Scope, this.\u0001);
				return \u0002;
			}
			if (iexpression2 != null)
			{
				\u0002[1] = this.\u0001.Generator.\u0001<_IExpression>(iexpression2, this._Scope, this.\u0001);
				return \u0002;
			}
			return \u0002;
		}

		// Token: 0x06002A94 RID: 10900 RVA: 0x00095128 File Offset: 0x00093328
		internal static ReplacerController \u0001(global::\u000E.\u0011 \u0002)
		{
			InterfaceComparisonReplacer replacer = new InterfaceComparisonReplacer(\u0002);
			return new ReplacerController(replacer, new InterfaceComparisonToVisitchecker(replacer));
		}

		// Token: 0x040007FD RID: 2045
		private _ICompiledPOU \u0001;

		// Token: 0x040007FE RID: 2046
		[CompilerGenerated]
		private readonly \u0081.\u0010 \u0001;

		// Token: 0x040007FF RID: 2047
		private readonly global::\u000E.\u0011 \u0001;
	}
}
