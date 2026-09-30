using System;
using \u0007;
using \u0018;
using \u0019;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0084;

namespace \u0001
{
	// Token: 0x02000372 RID: 882
	internal sealed class \u0011 : \u001E
	{
		// Token: 0x0600343F RID: 13375 RVA: 0x000CD6D4 File Offset: 0x000CB8D4
		public bool \u0001(\u0018.\u0010 \u0002)
		{
			foreach (int num in \u0002.changedpous.Keys)
			{
				_ICompiledPOU icompiledPOU = \u0002.changedpous[num];
				_ICompiledPOU icompiledPOU2 = \u0019.\u0003.\u0001(\u0002.ComconNew[num].OrgName + "__TOREMOVE");
				_ISignature isignature = \u0002.ComconNew.GetSignature(icompiledPOU2.Name) as _ISignature;
				if (isignature != null)
				{
					_ICompiledPOU cpou = \u0002.ComconNew.GetCompiledPOUById(isignature.Id) as _ICompiledPOU;
					\u0002.ComconNew.RemoveSignature(isignature);
					\u0002.ComconNew.RemoveCompiledPOU(cpou);
				}
				icompiledPOU2.SetFlag(CompiledPOUFlags.ToRemoveAfterDownload | CompiledPOUFlags.IgnoreForChecksum, true);
				icompiledPOU2.CompiledCode = \u0019.\u0003.\u0001(icompiledPOU.CompiledCode.CodeSize, icompiledPOU.CompiledCode.Location);
				icompiledPOU2.SetParseTree(\u0019.\u0003.\u0001());
				LStringBuilder lstringBuilder = new LStringBuilder();
				lstringBuilder.AppendLine("VAR_GLOBAL");
				lstringBuilder.AppendLine("END_VAR");
				_ISignature isignature2 = ParserHelper.\u0001(icompiledPOU2.Name, lstringBuilder.ToString(), true);
				isignature2 = isignature2.CreateCompiledSignature(null, \u0002.ComconNew.HasByteSupport());
				isignature2.SetFlag(SignatureFlag.Located, true);
				IScope5 scope = global::\u0007.\u0005.\u0001(\u0002.ComconNew, isignature2.Id);
				scope.LocalSignature = isignature2;
				\u0002.CompileInformation.InterfaceCompiler.\u0003(isignature2, scope);
				\u0002.ComconNew.AddSignature(isignature2, null, null, true);
				\u0002.ComconNew.AddCompiledPOU(icompiledPOU2, isignature2, null);
			}
			return true;
		}
	}
}
