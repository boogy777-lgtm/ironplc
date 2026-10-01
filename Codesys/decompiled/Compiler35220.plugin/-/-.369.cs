using System;
using System.Linq;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0015
{
	// Token: 0x020003BB RID: 955
	internal static class \u0008
	{
		// Token: 0x060036C3 RID: 14019 RVA: 0x000DE4E8 File Offset: 0x000DC6E8
		internal static void \u0001(_ISignature \u0002, _ISignature \u0003, _ICompileContext \u0004, _ICompileContext \u0005)
		{
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.AppendLine("METHOD __MAIN: BOOL");
			lstringBuilder.AppendLine("VAR");
			foreach (_IVariable ivariable in \u0002.Temps.Cast<_IVariable>())
			{
				foreach (string str in ivariable.Attributes)
				{
					lstringBuilder.AppendLine("{attribute '" + str + "'}");
				}
				if (ivariable.Initial == null)
				{
					lstringBuilder.AppendLine(string.Format("{0} : {1};", ivariable.VersionedName, ivariable._Type));
				}
				else
				{
					lstringBuilder.AppendLine(string.Format("{0} : {1} := {2};", ivariable.VersionedName, ivariable._Type, ivariable.Initial));
				}
				\u0002.RemoveVariable(ivariable);
			}
			lstringBuilder.AppendLine("END_VAR");
			_ISignature isignature = ParserHelper.\u0001(lstringBuilder.ToString(), true);
			_IVariable var = isignature.Outputs[0] as _IVariable;
			isignature.RemoveVariable(var);
			isignature.SetFlag(SignatureFlag.Generated, true);
			_ISignature isignature2 = null;
			if (\u0003 != null)
			{
				isignature2 = (\u0003.GetSubSignature(isignature.Name) as _ISignature);
			}
			_ISignature isignature3 = isignature.CreateCompiledSignature(isignature2, \u0004.HasByteSupport());
			isignature3.ParentObjectGuid = \u0002.ObjectGuid;
			isignature3.ParentSignatureId = \u0002.Id;
			isignature3.SetFlag(SignatureFlag.External, \u0002.GetFlag(SignatureFlag.External));
			if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_C_CALLING_CONVENTION))
			{
				isignature3.AddAttribute(CompileAttributes.ATTRIBUTE_C_CALLING_CONVENTION, null);
			}
			if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_REDUCED_BP_SET))
			{
				isignature3.AddAttribute(CompileAttributes.ATTRIBUTE_REDUCED_BP_SET, null);
			}
			\u0004.AddSignature(isignature3, isignature2, \u0005, true);
			\u0002.AddSubSignature(isignature3);
		}
	}
}
