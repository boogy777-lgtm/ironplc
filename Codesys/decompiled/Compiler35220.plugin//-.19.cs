using System;
using \u000E;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0084
{
	// Token: 0x020002AB RID: 683
	internal static class \u0018
	{
		// Token: 0x06002A98 RID: 10904 RVA: 0x00095208 File Offset: 0x00093408
		public static _IExpression \u0001(_IOperatorExpression \u0002, \u0011 \u0003)
		{
			string stringValue = ((ILiteralExpression)\u0002._OperandsList[0]).StringValue;
			return \u0003.Generator.GenerateExpression(stringValue, \u0003._Scope, \u0003.CompiledPOU);
		}
	}
}
