using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using \u0004;
using \u0007;
using \u0008;
using \u000E;
using \u000F;
using \u0011;
using \u0013;
using \u0017;
using \u001A;
using \u001E;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;
using \u0084;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x020002AE RID: 686
	public class OperationsReplacer : AbstractReplacer, IReplacer
	{
		// Token: 0x17000760 RID: 1888
		// (get) Token: 0x06002A9C RID: 10908 RVA: 0x000953C8 File Offset: 0x000935C8
		private \u0081.\u0010 ReplacerVisitor { get; }

		// Token: 0x17000761 RID: 1889
		// (get) Token: 0x06002A9D RID: 10909 RVA: 0x000953D0 File Offset: 0x000935D0
		private ExternalFunctionCallsHandler ExternalFunctionCallsHandler { get; }

		// Token: 0x17000762 RID: 1890
		// (get) Token: 0x06002A9E RID: 10910 RVA: 0x000953D8 File Offset: 0x000935D8
		private IScope5 Scope
		{
			get
			{
				return this.\u0001._Scope;
			}
		}

		// Token: 0x06002A9F RID: 10911 RVA: 0x000953F4 File Offset: 0x000935F4
		internal OperationsReplacer(ExternalFunctionCallsHandler externalFunctionCallsHandler, global::\u000E.\u0011 context)
		{
			this.\u0001 = context;
			this.ReplacerVisitor = \u0081.\u0010.\u0001(this, context);
			this.ExternalFunctionCallsHandler = externalFunctionCallsHandler;
		}

		// Token: 0x06002AA0 RID: 10912 RVA: 0x00095418 File Offset: 0x00093618
		public void ReplaceCode(_ICompiledPOU cpou)
		{
			this.\u0001 = cpou;
			this.ReplacerVisitor.ReplaceCode(cpou);
		}

		// Token: 0x06002AA1 RID: 10913 RVA: 0x00095430 File Offset: 0x00093630
		public void ReplaceCodeInExprement(_ICompiledPOU cpou, _IExprement exprement)
		{
			this.\u0001 = cpou;
			this.ReplacerVisitor.\u0001(exprement);
		}

		// Token: 0x06002AA2 RID: 10914 RVA: 0x00095448 File Offset: 0x00093648
		public override _IExpression ReplaceOperatorExpression(_IOperatorExpression operatorExpression)
		{
			_ILiteralExpression iliteralExpression = LateOperationReplacer.\u0001(operatorExpression, this.Scope) as _ILiteralExpression;
			if (iliteralExpression != null)
			{
				return iliteralExpression;
			}
			if (operatorExpression.Code == Operator.TruncInt)
			{
				operatorExpression.Code = Operator.Trunc;
			}
			_IExpression result;
			if (this.ExternalFunctionCallsHandler.HandleExternalFunctionCalls(operatorExpression, this.\u0001, out result))
			{
				return result;
			}
			Func<_IOperatorExpression, global::\u000E.\u0011, _IExpression> func;
			if (OperationsReplacer.\u0001.TryGetValue(operatorExpression.Code, out func))
			{
				return func(operatorExpression, this.\u0001);
			}
			return operatorExpression;
		}

		// Token: 0x04000801 RID: 2049
		private _ICompiledPOU \u0001;

		// Token: 0x04000802 RID: 2050
		[CompilerGenerated]
		private readonly \u0081.\u0010 \u0001;

		// Token: 0x04000803 RID: 2051
		[CompilerGenerated]
		private readonly ExternalFunctionCallsHandler \u0001;

		// Token: 0x04000804 RID: 2052
		private readonly global::\u000E.\u0011 \u0001;

		// Token: 0x04000805 RID: 2053
		private static readonly Dictionary<Operator, Func<_IOperatorExpression, global::\u000E.\u0011, _IExpression>> \u0001 = new Dictionary<Operator, Func<_IOperatorExpression, global::\u000E.\u0011, _IExpression>>
		{
			{
				Operator.Ini,
				new Func<_IOperatorExpression, global::\u000E.\u0011, _IExpression>(global::\u0008.\u0011.\u0001)
			},
			{
				Operator.__LateCompiledExpr,
				new Func<_IOperatorExpression, global::\u000E.\u0011, _IExpression>(\u0084.\u0018.\u0001)
			},
			{
				Operator.UpperBound,
				new Func<_IOperatorExpression, global::\u000E.\u0011, _IExpression>(global::\u001A.\u0011.\u0001)
			},
			{
				Operator.LowerBound,
				new Func<_IOperatorExpression, global::\u000E.\u0011, _IExpression>(global::\u001A.\u0011.\u0001)
			},
			{
				Operator.__QueryInterface,
				new Func<_IOperatorExpression, global::\u000E.\u0011, _IExpression>(global::\u0007.\u0010.\u0001)
			},
			{
				Operator.__QueryPointer,
				new Func<_IOperatorExpression, global::\u000E.\u0011, _IExpression>(global::\u000F.\u0014.\u0001)
			},
			{
				Operator.BitAdr,
				new Func<_IOperatorExpression, global::\u000E.\u0011, _IExpression>(\u001E.\u0010.\u0001)
			},
			{
				Operator.Adr,
				new Func<_IOperatorExpression, global::\u000E.\u0011, _IExpression>(global::\u0013.\u0006.\u0001)
			},
			{
				Operator.Add,
				new Func<_IOperatorExpression, global::\u000E.\u0011, _IExpression>(global::\u0004.\u000F.\u0001)
			},
			{
				Operator.Plus,
				new Func<_IOperatorExpression, global::\u000E.\u0011, _IExpression>(global::\u0004.\u000F.\u0001)
			},
			{
				Operator.Sub,
				new Func<_IOperatorExpression, global::\u000E.\u0011, _IExpression>(global::\u0017.\u0012.\u0001)
			},
			{
				Operator.Minus,
				new Func<_IOperatorExpression, global::\u000E.\u0011, _IExpression>(global::\u0017.\u0012.\u0001)
			},
			{
				Operator.__BitOffset,
				new Func<_IOperatorExpression, global::\u000E.\u0011, _IExpression>(BitOffsetHandler.\u0001)
			},
			{
				Operator.__AdrInst,
				new Func<_IOperatorExpression, global::\u000E.\u0011, _IExpression>(global::\u0011.\u0010.\u0001)
			}
		};
	}
}
