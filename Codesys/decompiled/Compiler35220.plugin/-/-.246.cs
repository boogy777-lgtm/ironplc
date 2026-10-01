using System;
using \u000E;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u000F
{
	// Token: 0x020002AA RID: 682
	internal static class \u0013
	{
		// Token: 0x06002A97 RID: 10903 RVA: 0x000951BC File Offset: 0x000933BC
		public static _IExpression \u0001(_IOperatorExpression \u0002, global::\u000E.\u0011 \u0003, _ICompiledPOU \u0004)
		{
			_IOperatorExpression ioperatorExpression = \u0019.\u0003.\u0001(Operator.Ne);
			ioperatorExpression.AddOperand(\u0002._OperandsList[0]);
			ioperatorExpression.AddOperand(\u0019.\u0003.\u0001(0L));
			return \u0003.Generator.\u0001<_IOperatorExpression>(ioperatorExpression, \u0003._Scope, \u0004);
		}
	}
}
