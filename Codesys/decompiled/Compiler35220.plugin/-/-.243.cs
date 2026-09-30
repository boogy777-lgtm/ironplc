using System;
using \u000E;
using _3S.CoDeSys.Compiler35220.InitialisationCode;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0019
{
	// Token: 0x020002A0 RID: 672
	internal static class \u000E
	{
		// Token: 0x06002A40 RID: 10816 RVA: 0x0009396C File Offset: 0x00091B6C
		public static _IStatement \u0001(_IOperatorExpression \u0002, \u0011 \u0003)
		{
			_ISignature isignature = (_ISignature)\u0002[0].GetSignature(\u0003._Scope);
			if (isignature == null)
			{
				return null;
			}
			_ICallExpression icallExpression = \u0003.\u0001(\u0003.\u0001(GVLInitialisationFunctionCreator.\u0002(isignature)), Token.Empty);
			icallExpression.AddParam(\u0002[1].Duplicate() as _IExpression, \u0003.\u0001("__bInitRetains"));
			_IExpressionStatement u = \u0003.\u0001(icallExpression, Token.Empty);
			return \u0003.Generator.\u0001<_IExpressionStatement>(u, \u0003._Scope, \u0003.CompiledPOU);
		}
	}
}
