using System;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Utilities;

namespace \u0008
{
	// Token: 0x020000EF RID: 239
	internal static class \u0005
	{
		// Token: 0x06001047 RID: 4167 RVA: 0x0002E080 File Offset: 0x0002C280
		internal static void \u0001(string \u0002, LDictionary<string, string> \u0003)
		{
			IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner();
			scanner.Initialize(\u0002);
			IToken token;
			while (scanner.GetNext(out token) == TokenType.Identifier)
			{
				string identifier = scanner.GetIdentifier(token);
				if (scanner.GetNext(out token) == TokenType.End)
				{
					\u0003[identifier] = null;
					return;
				}
				if (token.Type != TokenType.Operator)
				{
					break;
				}
				if (scanner.GetOperator(token) == Operator.Comma)
				{
					\u0003[identifier] = null;
				}
				else
				{
					if (scanner.GetOperator(token) != Operator.Assign || scanner.GetNext(out token) != TokenType.SingleByteString)
					{
						break;
					}
					string singleByteString = scanner.GetSingleByteString(token);
					\u0003[identifier] = singleByteString;
					if (scanner.GetNext(out token) != TokenType.Operator || scanner.GetOperator(token) != Operator.Comma)
					{
						break;
					}
				}
			}
		}
	}
}
