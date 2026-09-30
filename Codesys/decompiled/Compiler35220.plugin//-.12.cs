using System;
using \u0007;
using \u0014;
using \u0019;
using \u001B;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0082;

namespace \u0084
{
	// Token: 0x020001EA RID: 490
	internal static class \u0011
	{
		// Token: 0x06002194 RID: 8596 RVA: 0x00073894 File Offset: 0x00071A94
		internal static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003, ICodegenerator \u0004, bool \u0005)
		{
			if (\u0005)
			{
				_ISignature isignature = \u0002.GetSignature("__ApplicationInfoVariables") as _ISignature;
				if (isignature != null)
				{
					_ISignature isignature2 = \u0002.GetSignature(string.Format("__BlobInit__{0}__0", isignature.Id)) as _ISignature;
					_ICompiledPOU icompiledPOU = \u0002.GetCompiledPOUById(isignature2.Id) as _ICompiledPOU;
					icompiledPOU.SetFlag(CompiledPOUFlags.ToRemoveAfterDownload, true);
					_ICompiledPOU icompiledPOU2 = icompiledPOU;
					icompiledPOU2.Name += "__TOREMOVE";
					icompiledPOU.CompiledCode = \u0019.\u0003.\u0001(icompiledPOU.CompiledCode.CodeSize, icompiledPOU.CompiledCode.Location);
					isignature2.SetFlag(SignatureFlag.ToRemoveAfterDownload, true);
				}
			}
			\u001B.\u0005 u = new \u001B.\u0005();
			_ISignature isignature3 = ParserHelper.\u0001(u.\u0001(\u0002), true);
			isignature3.SetFlag(SignatureFlag.Generated, true);
			_ISignature isignature4 = null;
			isignature3 = isignature3.CreateCompiledSignature(isignature4, \u0002.HasByteSupport());
			\u0002.AddSignature(isignature3, isignature4, \u0003, true);
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0002, isignature3.Id);
			global::\u0014.\u0013.\u0002(isignature3, scope, \u0002);
			Locator.\u0001(isignature3, isignature4, \u0002, \u0003);
			if (isignature4 != null && isignature3.ChecksumNoInit != isignature4.ChecksumNoInit)
			{
				isignature3.SetFlag(SignatureFlag.OnlineChanged, true);
			}
			string u2 = u.\u0002(\u0002);
			_ISignature isignature5 = ParserHelper.\u0001("__ApplicationInfoVariables", u2, true);
			isignature4 = null;
			isignature5 = isignature5.CreateCompiledSignature(isignature4, \u0002.HasByteSupport());
			\u0002.AddSignature(isignature5, isignature4, \u0003, true);
			scope = global::\u0007.\u0005.\u0001(\u0002, isignature5.Id);
			global::\u0014.\u0013.\u0002(isignature5, scope, \u0002);
			Locator.\u0001(isignature5, isignature4, \u0002, \u0003);
			_IVariable ivariable = isignature5.AllVariables[0];
			ivariable.SetFlag(VarFlag.NoCopy, true);
			new \u0082.\u000F(\u0002, scope, \u0004).\u0001(ivariable, isignature3, isignature5, false);
		}
	}
}
