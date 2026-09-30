using System;
using \u0007;
using \u0014;
using \u0019;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0082;

namespace \u0012
{
	// Token: 0x02000214 RID: 532
	internal static class \u0010
	{
		// Token: 0x0600231F RID: 8991 RVA: 0x00078CA0 File Offset: 0x00076EA0
		internal static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003, ICodegenerator \u0004, bool \u0005)
		{
			if (\u0005)
			{
				_ISignature isignature = \u0002.GetSignature("__ApplicationCodeInfoVariables") as _ISignature;
				if (isignature != null)
				{
					\u0002.RemoveSignature(isignature);
					_ISignature isignature2 = \u0002.GetSignature("CODE_LOCATION") as _ISignature;
					if (isignature2 != null)
					{
						\u0002.RemoveSignature(isignature2);
					}
					_ISignature isignature3 = \u0002.GetSignature(string.Format("__BlobInit__{0}__0", isignature.Id)) as _ISignature;
					if (isignature3 != null)
					{
						isignature3.SetFlag(SignatureFlag.ToRemoveAfterDownload, true);
						_ICompiledPOU icompiledPOU = (_ICompiledPOU)\u0002.GetCompiledPOUById(isignature3.Id);
						\u0002.RemoveSignature(isignature3);
						\u0002.RemoveCompiledPOU(icompiledPOU);
						_ICompiledPOU icompiledPOU2 = \u0019.\u0003.\u0001(string.Format("__BlobInit__{0}__0__TOREMOVE", isignature.Id));
						icompiledPOU2.SetFlag(CompiledPOUFlags.ToRemoveAfterDownload, true);
						icompiledPOU2.CompiledCode = \u0019.\u0003.\u0001(icompiledPOU.CompiledCode.CodeSize, icompiledPOU.CompiledCode.Location);
						icompiledPOU2.SetParseTree(\u0019.\u0003.\u0001());
						icompiledPOU2.SetFlag(CompiledPOUFlags.ConstBlob, icompiledPOU.GetFlag(CompiledPOUFlags.ConstBlob));
						icompiledPOU2.SetFlag(CompiledPOUFlags.Blob, icompiledPOU.GetFlag(CompiledPOUFlags.Blob));
						LStringBuilder lstringBuilder = new LStringBuilder();
						lstringBuilder.AppendLine("VAR_GLOBAL");
						lstringBuilder.AppendLine("END_VAR");
						_ISignature isignature4 = ParserHelper.\u0001(string.Format("__BlobInit__{0}__0", isignature.Id), lstringBuilder.ToString(), true);
						isignature4 = isignature4.CreateCompiledSignature(null, \u0002.HasByteSupport());
						isignature4.SetFlag(SignatureFlag.Located, true);
						IScope5 scope = global::\u0007.\u0005.\u0001(\u0002, isignature4.Id);
						scope.LocalSignature = isignature4;
						global::\u0014.\u0013.\u0002(isignature4, scope, \u0002);
						\u0002.AddSignature(isignature4, null, null, true);
						\u0002.AddCompiledPOU(icompiledPOU2, isignature4, null);
					}
				}
			}
			CodeLocationInfo codeLocationInfo = new CodeLocationInfo();
			if (codeLocationInfo.\u0001(\u0002))
			{
				_ISignature isignature5 = ParserHelper.\u0001(codeLocationInfo.\u0003(), true);
				isignature5.SetFlag(SignatureFlag.Generated, true);
				isignature5 = isignature5.CreateCompiledSignature(null, \u0002.HasByteSupport());
				\u0002.AddSignature(isignature5, null, \u0003, true);
				IScope5 scope2 = global::\u0007.\u0005.\u0001(\u0002, isignature5.Id);
				global::\u0014.\u0013.\u0002(isignature5, scope2, \u0002);
				Locator.\u0001(isignature5, null, \u0002, \u0003);
				string u = codeLocationInfo.\u0002();
				_ISignature isignature6 = ParserHelper.\u0001("__ApplicationCodeInfoVariables", u, true);
				isignature6 = isignature6.CreateCompiledSignature(null, \u0002.HasByteSupport());
				\u0002.AddSignature(isignature6, null, \u0003, true);
				scope2 = global::\u0007.\u0005.\u0001(\u0002, isignature6.Id);
				global::\u0014.\u0013.\u0002(isignature6, scope2, \u0002);
				Locator.\u0001(isignature6, null, \u0002, \u0003);
				_IVariable ivariable = isignature6.AllVariables[0];
				ivariable.SetFlag(VarFlag.NoCopy, true);
				new \u0082.\u000F(\u0002, scope2, \u0004).\u0001(ivariable, isignature5, isignature6, false);
			}
		}
	}
}
