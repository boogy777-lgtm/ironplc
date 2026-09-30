using System;
using \u0019;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0010
{
	// Token: 0x020001C3 RID: 451
	internal static class \u0004
	{
		// Token: 0x06002092 RID: 8338 RVA: 0x0006ED88 File Offset: 0x0006CF88
		internal static string \u0001(_ILiteralValue \u0002)
		{
			KindOfLiteral kindOf = \u0002.KindOf;
			if (kindOf == KindOfLiteral.SignedInteger)
			{
				return \u0002.SignedLong.ToString();
			}
			if (kindOf != KindOfLiteral.UnsignedInteger)
			{
				return "Reached Unexpected Code line";
			}
			return \u0002.UnsignedLong.ToString();
		}

		// Token: 0x06002093 RID: 8339 RVA: 0x0006EDC8 File Offset: 0x0006CFC8
		internal static _IExpression \u0001(_ILiteralValue \u0002, IVariable \u0003)
		{
			_ILiteralExpression iliteralExpression = null;
			KindOfLiteral kindOf = \u0002.KindOf;
			if (kindOf != KindOfLiteral.SignedInteger)
			{
				if (kindOf != KindOfLiteral.UnsignedInteger)
				{
					if (kindOf != KindOfLiteral.None)
					{
					}
				}
				else
				{
					iliteralExpression = \u0003.\u0001(\u0002.UnsignedLong);
				}
			}
			else
			{
				iliteralExpression = \u0003.\u0001(\u0002.SignedLong);
			}
			Debug.\u0001(iliteralExpression != null);
			if (iliteralExpression != null)
			{
				iliteralExpression.ConstantType = \u0003.Type.Class;
				iliteralExpression._CompiledType = \u0003.CompiledType;
			}
			return iliteralExpression;
		}
	}
}
