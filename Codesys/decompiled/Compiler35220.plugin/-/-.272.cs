using System;
using \u0012;
using \u0019;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0080;

namespace \u000E
{
	// Token: 0x020002D5 RID: 725
	internal static class \u0013
	{
		// Token: 0x06002BCC RID: 11212 RVA: 0x000996D8 File Offset: 0x000978D8
		internal static _IExpression \u0001(_ISignature \u0002, _ICompiledPOU \u0003, _ICallExpression \u0004, global::\u000E.\u0011 \u0005)
		{
			bool flag = \u0080.\u0013.\u0001(\u0004._Callee, \u0005._Scope);
			bool flag2 = \u0002.GetFlag(SignatureFlag.Action);
			global::\u0012.\u0011 u = global::\u0012.\u0011.\u0001();
			u.ComplexCall = flag;
			\u0004.CallInfo = u;
			if (!flag)
			{
				_IExpression iexpression = \u0004._Callee;
				_ICompoAccessExpression icompoAccessExpression = iexpression as _ICompoAccessExpression;
				if (icompoAccessExpression != null && flag2)
				{
					iexpression = icompoAccessExpression._Left;
				}
				bool flag3 = !(\u0004._Callee is ICompoAccessExpression) && flag2;
				if (\u0002.POUType == Operator.FunctionBlock || (flag2 && !flag3))
				{
					for (int i = 0; i < \u0004.Outputs.Count; i++)
					{
						_ICompoAccessExpression icompoAccessExpression2 = \u0019.\u0003.\u0001(iexpression.Duplicate() as _IExpression, Token.Empty);
						icompoAccessExpression2._Right = (\u0004.Outputs[i].Duplicate() as _IExpression);
						_IExpression exp = \u0005.Generator.\u0001<_ICompoAccessExpression>(icompoAccessExpression2, \u0005._Scope, \u0003);
						\u0004.SetFormalOutput(exp, i);
					}
				}
			}
			return \u0004;
		}

		// Token: 0x06002BCD RID: 11213 RVA: 0x000997D8 File Offset: 0x000979D8
		internal static bool \u0001(_ISignature \u0002, _ICallExpression \u0003)
		{
			bool flag = \u0002.GetFlag(SignatureFlag.Action);
			bool flag2 = !(\u0003._Callee is ICompoAccessExpression) && flag;
			return (\u0002.POUType == Operator.FunctionBlock || flag) && !flag2 && \u0003.Outputs.Count > 0;
		}
	}
}
