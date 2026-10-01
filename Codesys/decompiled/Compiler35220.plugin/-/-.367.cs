using System;
using System.Collections.Generic;
using \u0007;
using \u0014;
using _3S.CoDeSys.Compiler35220.ImplicitCode;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u001E
{
	// Token: 0x020003B5 RID: 949
	internal static class \u0019
	{
		// Token: 0x06003693 RID: 13971 RVA: 0x000DD280 File Offset: 0x000DB480
		internal static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003, InitExitSignatureInfo \u0004)
		{
			foreach (KeyValuePair<string, LList<_ISignature>> keyValuePair in \u0004.ExplicitSignatures)
			{
				\u0019.\u0001(\u0002, \u0003, false, keyValuePair.Key);
				\u0019.\u0001(\u0002, \u0003, true, keyValuePair.Key);
			}
		}

		// Token: 0x06003694 RID: 13972 RVA: 0x000DD2E4 File Offset: 0x000DB4E4
		private static void \u0001(_ICompileContext \u0002, string \u0003, string \u0004, _ICompileContext \u0005)
		{
			_ISignature isignature = ParserHelper.\u0001(\u0004, true);
			isignature.SetFlag(SignatureFlag.Generated, true);
			_ISignature isignature2 = null;
			if (\u0005 != null)
			{
				isignature2 = \u0005[\u0003];
			}
			isignature = isignature.CreateCompiledSignature(isignature2, \u0002.HasByteSupport());
			\u0002.AddSignature(isignature, isignature2, null, true);
			IScope5 u = global::\u0007.\u0005.\u0001(\u0002, isignature.Id);
			global::\u0014.\u0013.\u0002(isignature, u, \u0002);
			Locator.\u0001(\u0002, isignature, isignature2);
			Locator.\u0001(isignature, isignature2, \u0002, \u0005);
		}

		// Token: 0x06003695 RID: 13973 RVA: 0x000DD354 File Offset: 0x000DB554
		private static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003, bool \u0004, string \u0005)
		{
			string text = \u0004 ? IdentifierConstants.GetExplicitExitPOUName(\u0005) : IdentifierConstants.GetExplicitInitPOUName(\u0005);
			string u = string.Format("FUNCTION {0} : BOOL\r\nVAR_INPUT\r\n\t{1}\r\n\t__bInCopyCode : BOOL;\r\nEND_VAR\r\nVAR\r\n\t__Index: DINT := 0;\r\nEND_VAR\r\n", text, \u0004 ? string.Empty : "__bInitRetains: BOOL;");
			\u0019.\u0001(\u0002, text, u, \u0003);
		}
	}
}
