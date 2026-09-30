using System;
using System.Collections.Generic;
using System.Linq;
using \u0002;
using \u0007;
using \u0018;
using \u0019;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0011
{
	// Token: 0x020003B8 RID: 952
	internal static class \u0015
	{
		// Token: 0x060036AD RID: 13997 RVA: 0x000DD96C File Offset: 0x000DBB6C
		internal static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003)
		{
			IEnumerable<_ISignature> allSignatureList = \u0002.AllSignatureList;
			ErrorVisitor u = new ErrorVisitor();
			foreach (_ISignature isignature in allSignatureList)
			{
				if (isignature.POUType == Operator.FunctionBlock || isignature.GetFlag(SignatureFlag.Structure))
				{
					_ISignature u2 = null;
					if (\u0003 != null)
					{
						u2 = \u0003[isignature.Id];
					}
					\u0015.\u0001(isignature);
					if (!\u0002.MinimalSystem && global::\u0002.\u0011.\u0001(\u0002, \u0003, isignature, u2, u))
					{
						\u0015.\u0002(\u0002, \u0003, isignature);
						if (isignature.POUType == Operator.FunctionBlock)
						{
							\u0015.\u0001(\u0002, \u0003, isignature);
						}
					}
				}
			}
		}

		// Token: 0x060036AE RID: 13998 RVA: 0x000DDA14 File Offset: 0x000DBC14
		private static _ISignature \u0001(_ISignature \u0002)
		{
			_ISignature result = null;
			foreach (_ISignature isignature in \u0002.SubSignatures.Cast<_ISignature>())
			{
				if (isignature.POUType == Operator.Method && !(isignature.Name != IdentifierConstants.InitMethodName))
				{
					IVariable[] allInputs = isignature.AllInputs;
					if (allInputs.Length >= 3 && allInputs[0].Type.Class == TypeClass.Bool && allInputs[0].Name == "BINITRETAINS" && allInputs[1].Type.Class == TypeClass.Bool && allInputs[1].Name == "BINCOPYCODE")
					{
						result = isignature;
					}
				}
			}
			return result;
		}

		// Token: 0x060036AF RID: 13999 RVA: 0x000DDAD8 File Offset: 0x000DBCD8
		private static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003, _ISignature \u0004)
		{
			_ISignature isignature = \u0004.GetSubSignature("__GetInterfaceReference") as _ISignature;
			if (isignature != null)
			{
				LStringBuilder lstringBuilder = new LStringBuilder();
				\u0015.\u0001(\u0002, \u0004, lstringBuilder);
				\u0015.\u0001(\u0002, isignature, \u0004, lstringBuilder, \u0003);
			}
			_ISignature isignature2 = \u0004.GetSubSignature("__GetInterfacePointer") as _ISignature;
			if (isignature2 != null)
			{
				LStringBuilder lstringBuilder = new LStringBuilder();
				\u0015.\u0001(lstringBuilder);
				\u0015.\u0001(\u0002, isignature2, \u0004, lstringBuilder, \u0003);
			}
		}

		// Token: 0x060036B0 RID: 14000 RVA: 0x000DDB40 File Offset: 0x000DBD40
		private static void \u0002(_ICompileContext \u0002, _ICompileContext \u0003, _ISignature \u0004)
		{
			_ISignature isignature = \u0004.GetSubSignature(IdentifierConstants.PartialInitMethodName) as _ISignature;
			if (isignature != null)
			{
				LStringBuilder u = new LStringBuilder();
				\u0015.\u0001(\u0002, isignature, \u0004, u, \u0003).SetFlag(CompiledPOUFlags.Generated | CompiledPOUFlags.ToGenerate, true);
			}
		}

		// Token: 0x060036B1 RID: 14001 RVA: 0x000DDB7C File Offset: 0x000DBD7C
		private static void \u0001(_ISignature \u0002)
		{
			_ISignature isignature = \u0015.\u0001(\u0002);
			if (\u0002.GetFlag(SignatureFlag.Structure) && \u0002.GetFlag(SignatureFlag.External))
			{
				isignature.SetFlag(SignatureFlag.External, true);
			}
			if (\u0002.GetFlag(SignatureFlag.Structure))
			{
				isignature.SetFlag(SignatureFlag.NonVirtual, true);
				return;
			}
			if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_CPPEXTERNAL))
			{
				isignature.AddAttribute(CompileAttributes.ATTRIBUTE_CPPEXTERNAL, null);
				isignature.AddAttribute(CompileAttributes.ATTRIBUTE_CPPCOMPATIBLE, null);
				_ISignature isignature2 = \u0002.GetSubSignature(IdentifierConstants.MainSignatureName) as _ISignature;
				if (isignature2 != null)
				{
					isignature2.AddAttribute(CompileAttributes.ATTRIBUTE_CPPEXTERNAL, null);
					isignature2.AddAttribute(CompileAttributes.ATTRIBUTE_CPPCOMPATIBLE, null);
				}
			}
		}

		// Token: 0x060036B2 RID: 14002 RVA: 0x000DDC18 File Offset: 0x000DBE18
		private static void \u0001(_ICompileContext \u0002, _ISignature \u0003, LStringBuilder \u0004)
		{
			\u0004.AppendLine("{implicit on}");
			\u0004.Append("{nobp}");
			IScope5 u = global::\u0007.\u0005.\u0001(\u0002);
			if (\u0003.BaseSignatureId != Helper.InvalidId)
			{
				\u0004.Append("IF SUPER^.__GetInterfaceReference(nInterfaceId, pRef) THEN __GetInterfaceReference := TRUE; RETURN; END_IF;");
			}
			if (\u0003.InterfaceIds.Length != 0)
			{
				LDictionary<int, int> ldictionary = new LDictionary<int, int>();
				Helper.\u0001(u, \u0003, ldictionary, true);
				LDictionary<int, LList<int>> ldictionary2 = \u0015.\u0001(\u0003, ldictionary);
				\u0004.AppendLine("CASE nInterfaceId OF");
				foreach (KeyValuePair<int, LList<int>> keyValuePair in ldictionary2)
				{
					bool flag = true;
					foreach (int num in keyValuePair.Value)
					{
						if (!flag)
						{
							\u0004.Append(",");
						}
						\u0004.AppendFormat("{0}", new object[]
						{
							num
						});
						flag = false;
					}
					\u0004.AppendLine(": pRef_help := THIS;");
					\u0004.AppendLine(string.Format("pRef_help := pRef_help + {0};", keyValuePair.Key));
					\u0004.Append("pRef^ := pRef_help;");
					\u0004.AppendLine("__GetInterfaceReference := TRUE;");
				}
				\u0004.Append("ELSE pRef^ := 0; __GetInterfaceReference := FALSE; END_CASE");
			}
			else
			{
				\u0004.Append("pRef^ := 0; __GetInterfaceReference := FALSE;");
			}
			\u0004.Append("{bp}");
			\u0004.AppendLine("{implicit off}");
		}

		// Token: 0x060036B3 RID: 14003 RVA: 0x000DDDAC File Offset: 0x000DBFAC
		private static LDictionary<int, LList<int>> \u0001(ISignature \u0002, LDictionary<int, int> \u0003)
		{
			LDictionary<int, LList<int>> ldictionary = new LDictionary<int, LList<int>>();
			foreach (KeyValuePair<int, int> keyValuePair in \u0003)
			{
				IVariable variable = \u0002[IdentifierConstants.GetInterfacePointerName(keyValuePair.Value)];
				if (variable != null)
				{
					if (!ldictionary.ContainsKey(variable.DataLocation.Offset))
					{
						ldictionary[variable.DataLocation.Offset] = new LList<int>();
					}
					ldictionary[variable.DataLocation.Offset].Add(keyValuePair.Value);
				}
			}
			return ldictionary;
		}

		// Token: 0x060036B4 RID: 14004 RVA: 0x000DDE58 File Offset: 0x000DC058
		private static void \u0001(LStringBuilder \u0002)
		{
			\u0002.AppendLine("{implicit on}");
			\u0002.Append("{nobp}");
			\u0002.Append("pRef^ := THIS; __GetInterfacePointer := TRUE;");
			\u0002.Append("{bp}");
			\u0002.AppendLine("{implicit off}");
		}

		// Token: 0x060036B5 RID: 14005 RVA: 0x000DDE98 File Offset: 0x000DC098
		private static _ICompiledPOU \u0001(_ICompileContext \u0002, _ISignature \u0003, _ISignature \u0004, LStringBuilder \u0005, _ICompileContext \u0006)
		{
			if (\u0003 != null)
			{
				IScope5 scope = global::\u0007.\u0005.\u0001(\u0002, \u0004.Id);
				scope.MethodSignature = \u0003;
				_IStatement istatement = new global::\u0011.\u0006(\u0005.ToString()).\u0001();
				_ICompiledPOU icompiledPOU = \u0019.\u0003.\u0001(\u0003.Name.ToUpperInvariant());
				icompiledPOU.SetParseTree(istatement);
				\u0002.AddCompiledPOU(icompiledPOU, \u0003, true, \u0006);
				ExpressionTypifierWithSpecialTasks ivisit = new ExpressionTypifierWithSpecialTasks(scope, \u0002, false, icompiledPOU);
				istatement.Accept(ivisit);
				\u0018.\u000E.\u0001(istatement, \u0002);
				TypeCheckerVisitor ivisit2 = new TypeCheckerVisitor(scope, \u0002);
				istatement.Accept(ivisit2);
				ErrorVisitor errorVisitor = new ErrorVisitor();
				istatement.Accept(errorVisitor);
				icompiledPOU.SetFlag(CompiledPOUFlags.Typified, true);
				icompiledPOU.SetFlagInternal(InternalCompiledPOUFlags.ImplicitInitialisationCodeAdded, true);
				Debug.\u0001(errorVisitor.MessageList.Count == 0, "Fehler in " + \u0003.Name);
				return icompiledPOU;
			}
			return null;
		}
	}
}
