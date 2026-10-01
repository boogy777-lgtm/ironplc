using System;
using System.Collections.Generic;
using \u0007;
using \u0014;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0010
{
	// Token: 0x020003C5 RID: 965
	internal static class \u0012
	{
		// Token: 0x060036D6 RID: 14038 RVA: 0x000DF440 File Offset: 0x000DD640
		internal static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003)
		{
			IList<_ISignature> allSignatureList = \u0002.AllSignatureList;
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.AppendLine("VAR_GLOBAL");
			foreach (_ISignature isignature in allSignatureList)
			{
				if (isignature.POUType == Operator.FunctionBlock || isignature.GetFlag(SignatureFlag.Structure))
				{
					if (isignature.VirtualFunctionTable == null)
					{
						isignature.SetFlag(SignatureFlag.NonVirtual, true);
					}
					else
					{
						lstringBuilder.AppendFormat(IdentifierConstants.VFTableTemplate + ": ARRAY[0..{1}] OF POINTER TO POINTER TO __XWORD;", new object[]
						{
							isignature.Id,
							isignature.VirtualFunctionTable.Size / \u0002.PointerSize
						});
					}
				}
			}
			lstringBuilder.AppendLine("END_VAR");
			_ISignature isignature2 = ParserHelper.\u0001(IdentifierConstants.VFTableName, lstringBuilder.ToString(), true);
			_ISignature isignature3 = null;
			if (\u0003 != null)
			{
				isignature3 = \u0003[isignature2.Name];
			}
			isignature2 = isignature2.CreateCompiledSignature(isignature3, \u0002.HasByteSupport());
			isignature2.SetFlag(SignatureFlag.NoInit | SignatureFlag.NoCopy | SignatureFlag.SuperGlobal, true);
			IScope5 u = global::\u0007.\u0005.\u0001(\u0002, isignature2.Id);
			global::\u0014.\u0013.\u0002(isignature2, u, \u0002);
			\u0002.AddSignature(isignature2, isignature3, \u0003, true);
			foreach (_ISignature isignature4 in allSignatureList)
			{
				if ((isignature4.POUType == Operator.FunctionBlock || isignature4.GetFlag(SignatureFlag.Structure)) && isignature4.VirtualFunctionTable != null)
				{
					string stName = string.Format(IdentifierConstants.VFTableTemplate, isignature4.Id);
					_IVariable ivariable = isignature2[stName] as _IVariable;
					ivariable.DataLocation = isignature4.VirtualFunctionTable.DataLocation;
					ivariable.SetFlag(VarFlag.Absolut, true);
				}
			}
		}
	}
}
