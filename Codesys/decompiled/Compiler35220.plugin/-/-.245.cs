using System;
using \u0004;
using \u000E;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0080;

namespace \u001B
{
	// Token: 0x020002A7 RID: 679
	internal static class \u0008
	{
		// Token: 0x06002A8C RID: 10892 RVA: 0x00094DEC File Offset: 0x00092FEC
		public static _IStatement \u0001(_IOperatorExpression \u0002, global::\u000E.\u0011 \u0003)
		{
			_IVariable ivariable = \u0002[0].GetVariable(\u0003._Scope) as _IVariable;
			int signatureId = \u0002[0].SignatureId;
			_ISignature isignature = \u0003._Scope[signatureId] as _ISignature;
			if (ivariable == null || isignature == null)
			{
				return null;
			}
			bool flag = global::\u0004.\u0018.\u0001(ivariable, \u0003._Scope);
			bool flag2 = true;
			_IExprement iexprement;
			if (ivariable.Initial == null || flag)
			{
				_IExpression u000F = global::\u0019.\u0003.\u0001(true);
				_IExpression u = global::\u0019.\u0003.\u0001(false);
				iexprement = \u0080.\u001A.\u0001(ivariable, false, ivariable._Type, \u0002[0], \u0003._Scope, out flag2, null, null, u000F, u, 0, \u0003.Comcon);
				if (ivariable.Initial != null && iexprement is _IStatement)
				{
					iexprement = global::\u0004.\u0018.\u0001(ivariable, isignature, ivariable.Initial as _IExpression, iexprement);
				}
			}
			else
			{
				iexprement = (ivariable.Initial as _IExpression);
			}
			if (flag2)
			{
				iexprement = global::\u0019.\u0003.\u0001(\u0002[0].Duplicate() as IExpression, iexprement as _IExpression);
			}
			return \u0003.Generator.\u0001<_IStatement>(iexprement as _IStatement, \u0003._Scope, \u0003.CompiledPOU);
		}
	}
}
