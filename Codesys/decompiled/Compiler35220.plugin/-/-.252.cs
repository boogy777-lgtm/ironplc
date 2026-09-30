using System;
using System.Collections.Generic;
using \u000E;
using \u0019;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0007
{
	// Token: 0x020002B4 RID: 692
	internal static class \u0010
	{
		// Token: 0x06002ABA RID: 10938 RVA: 0x00096280 File Offset: 0x00094480
		public static _IExpression \u0001(_IOperatorExpression \u0002, global::\u000E.\u0011 \u0003)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			_ICallExpression icallExpression = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(\u0019.\u0003.\u0001("__CheckedInterfaceCast"), Token.Empty), Token.Empty);
			int num = global::\u0007.\u0010.\u0001(\u0003, operandsList);
			icallExpression.AddParam(\u0019.\u0003.\u0001((long)num));
			_IOperatorExpression ioperatorExpression = \u0019.\u0003.\u0001(Operator.Adr);
			ioperatorExpression.AddOperand(operandsList[1]);
			icallExpression.AddParam(ioperatorExpression);
			icallExpression.AddParam(operandsList[0]);
			return \u0003.Generator.\u0001<_ICallExpression>(icallExpression, \u0003._Scope, \u0003.CompiledPOU);
		}

		// Token: 0x06002ABB RID: 10939 RVA: 0x0009630C File Offset: 0x0009450C
		private static int \u0001(global::\u000E.\u0011 \u0002, IList<_IExpression> \u0003)
		{
			_IUserdefType iuserdefType = \u0003[1]._CompiledType as _IUserdefType;
			if (iuserdefType == null)
			{
				_IReferenceType ireferenceType = (_IReferenceType)\u0003[1]._CompiledType;
				iuserdefType = (_IUserdefType)((ireferenceType != null) ? ireferenceType.BaseType : null);
			}
			if (iuserdefType == null)
			{
				throw new LateCompileErrorException("unexpected operand in QueryInterface");
			}
			ISignature signature = iuserdefType.GetSignature(\u0002._Scope);
			int id = signature.Id;
			if (signature.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
			{
				_IUserdefType iuserdefType2 = signature["__Interface"].CompiledType.BaseType as _IUserdefType;
				if (iuserdefType2 != null)
				{
					id = iuserdefType2.GetSignature(\u0002._Scope).Id;
				}
			}
			return id;
		}
	}
}
