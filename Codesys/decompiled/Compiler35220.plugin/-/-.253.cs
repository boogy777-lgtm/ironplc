using System;
using System.Collections.Generic;
using \u000E;
using \u0019;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u000F
{
	// Token: 0x020002B5 RID: 693
	internal static class \u0014
	{
		// Token: 0x06002ABC RID: 10940 RVA: 0x000963B4 File Offset: 0x000945B4
		public static _IExpression \u0001(_IOperatorExpression \u0002, global::\u000E.\u0011 \u0003)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			_ICallExpression icallExpression = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(\u0019.\u0003.\u0001("__CheckedPointerCast"), Token.Empty), Token.Empty);
			_IOperatorExpression ioperatorExpression = \u0019.\u0003.\u0001(Operator.Adr);
			ioperatorExpression.AddOperand(operandsList[1]);
			icallExpression.AddParam(ioperatorExpression);
			icallExpression.AddParam(operandsList[0]);
			return \u0003.Generator.\u0001<_ICallExpression>(icallExpression, \u0003._Scope, \u0003.CompiledPOU);
		}
	}
}
