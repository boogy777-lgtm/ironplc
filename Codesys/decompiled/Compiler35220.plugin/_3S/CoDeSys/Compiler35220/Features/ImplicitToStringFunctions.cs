using System;
using System.Collections.Generic;
using System.Linq;
using \u0007;
using \u0011;
using \u0014;
using \u0019;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.Features
{
	// Token: 0x020001CC RID: 460
	internal static class ImplicitToStringFunctions
	{
		// Token: 0x060020B2 RID: 8370 RVA: 0x0006F6BC File Offset: 0x0006D8BC
		internal static void \u0001(_ICompileContext \u0002, _ISignature \u0003, _ICompileContext \u0004)
		{
			if (\u0003.POUType != Operator.VarGlobal)
			{
				return;
			}
			if (!\u0003.GetFlag(SignatureFlag.Enum))
			{
				return;
			}
			if (!\u0003.HasAttribute(CompileAttributes.ATTRIBUTE_TO_STRING))
			{
				return;
			}
			if (!\u0003.AllVariables.Any<_IVariable>())
			{
				return;
			}
			string text = string.Empty;
			if (\u0003.IsLibraryObject)
			{
				text = Helper.\u0001(\u0002, \u0003.LibraryPath);
			}
			bool flag = false;
			ISignature[] array = global::\u0007.\u0005.\u0001(\u0002, -1).SystemScope.FindSignature(\u0003.OrgName);
			if (array != null && array.Length == 1 && array[0].ObjectGuid == \u0003.ObjectGuid)
			{
				flag = true;
			}
			int u = \u0003.AllVariables.Max(new Func<_IVariable, int>(ImplicitToStringFunctions.<>c.<>9.\u0001));
			foreach (object obj in Enum.GetValues(typeof(ImplicitToStringFunctions.StringType)))
			{
				ImplicitToStringFunctions.StringType stringType = (ImplicitToStringFunctions.StringType)obj;
				string text2 = string.Format("__{0}__{1}", ImplicitToStringFunctions.\u0002(stringType), \u0003.Id);
				string u0087_u = ImplicitToStringFunctions.\u0001(\u0003, stringType, text2, u, text);
				string u0087_u2 = ImplicitToStringFunctions.\u0001(\u0003, stringType, text2, text);
				_ISignature isignature = new global::\u0011.\u0006(u0087_u).\u0001() as _ISignature;
				if (\u0003.IsLibraryObject)
				{
					isignature.SetFlag(SignatureFlag.SuperGlobal, true);
				}
				if (flag)
				{
					isignature.SetFlag(SignatureFlag.SystemNamespaceForced, true);
				}
				_ISignature isignature2 = null;
				if (\u0004 != null)
				{
					isignature2 = \u0004[isignature.GetSearchName(\u0004)];
				}
				isignature = isignature.CreateCompiledSignature(isignature2, \u0002.HasByteSupport());
				\u0002.AddSignature(isignature, isignature2, \u0004, true);
				_ICompiledPOU icompiledPOU = \u0019.\u0003.\u0001(text2);
				_IStatement parseTree = new global::\u0011.\u0006(u0087_u2).\u0001();
				icompiledPOU.SetParseTree(parseTree);
				\u0002.AddCompiledPOU(icompiledPOU, isignature, true, \u0004);
				IScope5 u2 = global::\u0007.\u0005.\u0001(\u0002, isignature.Id);
				global::\u0014.\u0013.\u0002(isignature, u2, \u0002);
				Locator.\u0001(isignature, isignature2, \u0002, \u0004);
				string stAttribute = string.Format("to_{0}_function", ImplicitToStringFunctions.\u0001(stringType).ToLower());
				string stValue = text2;
				\u0003.AddAttribute(stAttribute, stValue);
			}
		}

		// Token: 0x060020B3 RID: 8371 RVA: 0x0006F900 File Offset: 0x0006DB00
		private static string \u0001(string \u0002, ImplicitToStringFunctions.StringType \u0003)
		{
			if (\u0003 == ImplicitToStringFunctions.StringType.STRING)
			{
				return string.Format("'{0}'", \u0002);
			}
			if (\u0003 != ImplicitToStringFunctions.StringType.WSTRING)
			{
				throw new ArgumentException(\u0003.ToString());
			}
			return string.Format("\"{0}\"", \u0002);
		}

		// Token: 0x060020B4 RID: 8372 RVA: 0x0006F938 File Offset: 0x0006DB38
		private static string \u0001(ImplicitToStringFunctions.StringType \u0002)
		{
			return Enum.GetName(typeof(ImplicitToStringFunctions.StringType), \u0002);
		}

		// Token: 0x060020B5 RID: 8373 RVA: 0x0006F950 File Offset: 0x0006DB50
		private static string \u0002(ImplicitToStringFunctions.StringType \u0002)
		{
			return "TO_" + ImplicitToStringFunctions.\u0001(\u0002);
		}

		// Token: 0x060020B6 RID: 8374 RVA: 0x0006F964 File Offset: 0x0006DB64
		private static string \u0001(_ISignature \u0002, ImplicitToStringFunctions.StringType \u0003, string \u0004, int \u0005, string \u0006)
		{
			Debug.\u0001(\u0002.AllVariables.Any<_IVariable>());
			ICompiledType compiledType = \u0002.AllVariables[0].Type as ICompiledType;
			if (compiledType == null)
			{
				return string.Empty;
			}
			LStringBuilder lstringBuilder = new LStringBuilder();
			string arg = ImplicitToStringFunctions.\u0001(\u0003);
			lstringBuilder.AppendLine("{implicit on}");
			lstringBuilder.AppendLine(string.Format("FUNCTION {0} : {1}({2})", \u0004, arg, \u0005));
			lstringBuilder.AppendLine("VAR_INPUT");
			string text = string.IsNullOrEmpty(\u0006) ? string.Empty : (\u0006 + "#");
			text += \u0002.Name;
			lstringBuilder.AppendLine("\tvalue : " + text + ";");
			lstringBuilder.AppendLine("END_VAR");
			lstringBuilder.AppendLine("VAR");
			lstringBuilder.AppendLine(string.Format("\tintval : {0};", compiledType.DeRefType));
			lstringBuilder.AppendLine("END_VAR");
			lstringBuilder.AppendLine("{implicit off}");
			return lstringBuilder.ToString();
		}

		// Token: 0x060020B7 RID: 8375 RVA: 0x0006FA6C File Offset: 0x0006DC6C
		private static string \u0001(_ISignature \u0002, ImplicitToStringFunctions.StringType \u0003, string \u0004, string \u0005)
		{
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.AppendLine("{implicit on}");
			lstringBuilder.AppendLine();
			IList<_IVariable> allVariables = \u0002.AllVariables;
			for (int i = 0; i < allVariables.Count; i++)
			{
				_IVariable ivariable = allVariables[i];
				string text = string.IsNullOrEmpty(\u0005) ? string.Empty : (\u0005 + "#");
				text += string.Format("{0}.{1}", \u0002.OrgName, ivariable.OrgName);
				lstringBuilder.AppendLine("IF value = " + text + " THEN");
				string text2 = ImplicitToStringFunctions.\u0001(ivariable.OrgName, \u0003);
				lstringBuilder.AppendLine(string.Concat(new string[]
				{
					"\t",
					\u0004,
					" := ",
					text2,
					";"
				}));
				lstringBuilder.AppendLine("\tRETURN;");
				lstringBuilder.AppendLine("END_IF");
			}
			lstringBuilder.AppendLine("\tintval := value;");
			string text3 = ImplicitToStringFunctions.\u0002(\u0003);
			lstringBuilder.AppendLine(string.Concat(new string[]
			{
				"\t",
				\u0004,
				" := ",
				text3,
				"(intval);"
			}));
			lstringBuilder.AppendLine("{implicit off}");
			return lstringBuilder.ToString();
		}

		// Token: 0x020001CD RID: 461
		private enum StringType
		{
			// Token: 0x04000560 RID: 1376
			STRING,
			// Token: 0x04000561 RID: 1377
			WSTRING
		}
	}
}
