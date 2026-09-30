using System;
using \u000E;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0081
{
	// Token: 0x020002D6 RID: 726
	internal static class \u0014
	{
		// Token: 0x06002BCE RID: 11214 RVA: 0x00099830 File Offset: 0x00097A30
		internal static _IExpression \u0001(_ISignature \u0002, _ICompiledPOU \u0003, _ICallExpression \u0004, \u0011 \u0005)
		{
			if (\u0002.Name == "_SYSTEM_CALL" && \u0002.HasAttribute("__SYSTEM__CALL") && \u0004.ParamExpressions.Count > 0)
			{
				_ILiteralExpression iliteralExpression = \u0004.ParamExpressions[0] as _ILiteralExpression;
				if (iliteralExpression != null && iliteralExpression.StringValue != null)
				{
					string stringValue = iliteralExpression.StringValue;
					return \u0005.Generator.GenerateExpression(stringValue, \u0005._Scope, \u0003);
				}
			}
			return \u0004;
		}

		// Token: 0x06002BCF RID: 11215 RVA: 0x000998A8 File Offset: 0x00097AA8
		internal static bool \u0001(_ISignature \u0002, _ICallExpression \u0003)
		{
			return \u0002.Name == "_SYSTEM_CALL" && \u0002.HasAttribute("__SYSTEM__CALL");
		}
	}
}
