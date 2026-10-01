using System;
using System.Collections.Generic;
using System.Text;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	public static class Common
	{
		internal static readonly IScanner SCANNER = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner("", bIncludeComments: false, bIncludeEndOfLines: false, bIncludePragmas: false, bIncludeWhitespaces: false);

		public static bool IsCompoAccess(string st)
		{
			st = st.Trim();
			if (!st.StartsWith("%") && st.Contains("."))
			{
				return true;
			}
			return false;
		}

		public static IIdentifierInfo GetIdentifierInfoAtPosition(ISourcePosition pos, string stName)
		{
			if (pos == null)
			{
				return null;
			}
			WhatToFind whattofind = WhatToFind.PartialInstancePath;
			if (IsCompoAccess(stName))
			{
				whattofind = WhatToFind.ExactInstancePath;
			}
			IIdentifierInfo[] identifierInfoAtSourcePosition = APEnvironmentFacade.Instance.LanguageModelMgr.GetIdentifierInfoAtSourcePosition(stName, pos, whattofind);
			if (identifierInfoAtSourcePosition == null || identifierInfoAtSourcePosition.Length == 0 || identifierInfoAtSourcePosition.Length > 1)
			{
				return null;
			}
			return identifierInfoAtSourcePosition[0];
		}

		public static IPreCompileContext GetPreCompileContext(Guid objectGuid, int nProjectHandle)
		{
			Guid applicationGuid = APEnvironmentFacade.Instance.GetApplicationGuid(objectGuid, nProjectHandle);
			return APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(applicationGuid);
		}

		public static IPreCompileContext GetPreCompileContextByApplicationGuid(Guid applicationGuid, int nProjectHandle)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(applicationGuid);
		}

		internal static SignatureFlag ExtractVisibilityFlags(ISignature signature)
		{
			SignatureFlag signatureFlag = SignatureFlag.None;
			if (signature != null)
			{
				if (signature.GetFlag(SignatureFlag.Final))
				{
					signatureFlag |= SignatureFlag.Final;
				}
				if (signature.GetFlag(SignatureFlag.Abstract))
				{
					signatureFlag |= SignatureFlag.Abstract;
				}
				if (signature.GetFlag(SignatureFlag.Internal))
				{
					signatureFlag |= SignatureFlag.Internal;
				}
				if (signature.GetFlag(SignatureFlag.Protected))
				{
					signatureFlag |= SignatureFlag.Protected;
				}
				if (signature.GetFlag(SignatureFlag.Private))
				{
					signatureFlag |= SignatureFlag.Private;
				}
			}
			return signatureFlag;
		}

		internal static string GetVisibilityString(ISignature signature)
		{
			return GetVisibilityString(ExtractVisibilityFlags(signature));
		}

		internal static string GetVisibilityString(SignatureFlag signatureFlag)
		{
			string text = string.Empty;
			if ((signatureFlag & SignatureFlag.Final) != SignatureFlag.None)
			{
				text += " FINAL";
			}
			if ((signatureFlag & SignatureFlag.Abstract) != SignatureFlag.None)
			{
				text += " ABSTRACT";
			}
			if ((signatureFlag & SignatureFlag.Internal) != SignatureFlag.None)
			{
				text += " INTERNAL";
			}
			if ((signatureFlag & SignatureFlag.Private) != SignatureFlag.None)
			{
				text += " PRIVATE";
			}
			if ((signatureFlag & SignatureFlag.Protected) != SignatureFlag.None)
			{
				text += " PROTECTED";
			}
			return text.Trim();
		}

		public static string[] SplitAtDot(string stStringToSplit)
		{
			IScanner scanner = APEnvironmentFacade.Instance.LMServiceProvider.CreatorService.CreateScanner(stStringToSplit, bIncludeComments: false, bIncludeEndOfLines: false, bIncludePragmas: false, bIncludeWhitespaces: false);
			List<string> list = new List<string>();
			StringBuilder stringBuilder = new StringBuilder();
			IToken token;
			TokenType next = scanner.GetNext(out token);
			while (TokenType.End != next)
			{
				bool flag = true;
				if (TokenType.Operator == next && Operator.Period == scanner.GetOperator(token))
				{
					list.Add(stringBuilder.ToString());
					stringBuilder = new StringBuilder();
					flag = false;
				}
				if (flag)
				{
					stringBuilder.Append(scanner.GetTokenText(token));
				}
				next = scanner.GetNext(out token);
			}
			list.Add(stringBuilder.ToString());
			return list.ToArray();
		}

		internal static string GetNamespaceDelimiterConsideringCompilerversion3_5_21_10()
		{
			if ((!APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionGreaterEq(3, 5, 20, 60) || APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionGreaterEq(3, 5, 21, 0)) && !APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionGreaterEq(3, 5, 21, 10))
			{
				return ".";
			}
			return "#";
		}
	}
}
