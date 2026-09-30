using System;
using System.Collections.Generic;
using \u000E;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0013
{
	// Token: 0x0200029D RID: 669
	internal static class \u0006
	{
		// Token: 0x06002A39 RID: 10809 RVA: 0x00093678 File Offset: 0x00091878
		public static _IExpression \u0001(_IOperatorExpression \u0002, \u0011 \u0003)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			Debug.\u0001(operandsList.Count == 1);
			if (operandsList[0].IsPOUReference)
			{
				ISignature signature = operandsList[0].GetSignature(\u0003._Scope);
				if (signature != null && (signature.POUType == Operator.Program || signature.POUType == Operator.FunctionBlock || signature.POUType == Operator.Function || signature.POUType == Operator.Method))
				{
					if (signature.POUType == Operator.FunctionBlock)
					{
						signature = signature.GetSubSignature(IdentifierConstants.MainSignatureName);
					}
					_IExpression u = LateCodeGenerator.\u0001(signature.Id);
					_IExpression value = \u0003.Generator.\u0001<_IExpression>(u, \u0003._Scope, \u0003.CompiledPOU);
					\u0002[0] = value;
				}
			}
			return \u0002;
		}
	}
}
