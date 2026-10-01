using System;
using \u000E;
using \u0019;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0006
{
	// Token: 0x02000265 RID: 613
	internal static class \u0006
	{
		// Token: 0x06002792 RID: 10130 RVA: 0x00088E50 File Offset: 0x00087050
		internal static _IStatement \u0001(_IAssignmentExpression \u0002, global::\u000E.\u0011 \u0003)
		{
			_IUserdefType iuserdefType = \u0002._RValue.Type.DeRefType as _IUserdefType;
			if (iuserdefType != null)
			{
				_IUserdefType iuserdefType2 = \u0002._LValue.Type.DeRefType as _IUserdefType;
				if (iuserdefType2 != null && \u0002.KindOf != Operator.RefAssign)
				{
					_ISignature isignature = (_ISignature)iuserdefType2.GetSignature(\u0003._Scope);
					_ISignature isignature2 = (_ISignature)iuserdefType.GetSignature(\u0003._Scope);
					bool flag = \u0002._RValue.Type is _IReferenceType || \u0002._RValue is IDeRefAccessExpression;
					bool flag2 = isignature.Id == isignature2.Id && !flag;
					if (isignature.POUType == Operator.FunctionBlock && isignature2.POUType == Operator.FunctionBlock && !flag2)
					{
						_ICompoAccessExpression icompoAccessExpression = \u0019.\u0003.\u0001(\u0002._LValue.Duplicate() as _IExpression, Token.Empty);
						icompoAccessExpression._Right = \u0019.\u0003.\u0001(IdentifierConstants.VFInitMethodName);
						_IExpressionStatement iexpressionStatement = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(icompoAccessExpression, Token.Empty), Token.Empty);
						iexpressionStatement.SetFlag(StatementFlag.Implicit, true);
						_IStatement state = \u0003.Generator.\u0001<_IExpressionStatement>(iexpressionStatement, \u0003._Scope, \u0003.CompiledPOU);
						_IExpressionStatement iexpressionStatement2 = \u0019.\u0003.\u0001(\u0002);
						iexpressionStatement2.SetFlag(StatementFlag.GenerateBP, true);
						_ISequenceStatement isequenceStatement = \u0019.\u0003.\u0001();
						isequenceStatement.SetFlag(StatementFlag.GenerateBP, true);
						isequenceStatement.AddStatement(iexpressionStatement2);
						isequenceStatement.AddStatement(state);
						return isequenceStatement;
					}
				}
			}
			return null;
		}

		// Token: 0x06002793 RID: 10131 RVA: 0x00088FC0 File Offset: 0x000871C0
		internal static bool \u0001(_IAssignmentExpression \u0002, global::\u000E.\u0011 \u0003)
		{
			return \u0002._RValue.Type.DeRefType is _IUserdefType && \u0002._LValue.Type.DeRefType is _IUserdefType && \u0002.KindOf != Operator.RefAssign;
		}
	}
}
