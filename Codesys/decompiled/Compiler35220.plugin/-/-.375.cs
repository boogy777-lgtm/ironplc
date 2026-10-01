using System;
using System.Collections.Generic;
using \u0007;
using \u0019;
using _3S.CoDeSys.Compiler35220.ImplicitCode;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0080;

namespace \u0002
{
	// Token: 0x020003C1 RID: 961
	internal static class \u0011
	{
		// Token: 0x060036C9 RID: 14025 RVA: 0x000DE898 File Offset: 0x000DCA98
		internal static bool \u0001(_ICompileContext \u0002, _ICompileContext \u0003, _ISignature \u0004, _ISignature \u0005, ErrorVisitor \u0006)
		{
			_ISignature isignature = \u0004.GetSubSignature(IdentifierConstants.VFInitMethodName) as _ISignature;
			if (isignature == null)
			{
				return false;
			}
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0002, \u0004.Id);
			scope.MethodSignature = isignature;
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.AppendLine("{implicit on}");
			lstringBuilder.Append("{nobp}");
			global::\u0002.\u0011.\u0001(\u0002, \u0003, \u0004, \u0005, lstringBuilder, scope);
			lstringBuilder.Append("{bp}");
			lstringBuilder.AppendLine("{implicit off}");
			_IStatement istatement = (_IStatement)\u0019.\u0003.Builder.ParseSTSnippet(lstringBuilder.ToString());
			_ICompiledPOU icompiledPOU = \u0019.\u0003.\u0001(isignature.Name.ToUpperInvariant());
			icompiledPOU.SetParseTree(istatement);
			\u0002.AddCompiledPOU(icompiledPOU, isignature, true, \u0003);
			_ISignature u = null;
			if (\u0003 != null)
			{
				u = \u0003[isignature.Id];
			}
			ExpressionTypifierWithSpecialTasks ivisit = new ExpressionTypifierWithSpecialTasks(scope, \u0002, false, icompiledPOU);
			istatement.Accept(ivisit);
			TypeCheckerVisitor ivisit2 = new TypeCheckerVisitor(scope, \u0002);
			istatement.Accept(ivisit2);
			istatement.Accept(\u0006);
			icompiledPOU.SetFlag(CompiledPOUFlags.Typified, true);
			icompiledPOU.SetFlagInternal(InternalCompiledPOUFlags.ImplicitInitialisationCodeAdded, true);
			if (\u0004.HasAttribute(CompileAttributes.ATTRIBUTE_CPPEXTERNAL))
			{
				isignature.SetFlag(SignatureFlag.External, true);
			}
			Locator.\u0001(\u0002.DataManager, \u0002, \u0003, isignature, u);
			Debug.\u0001(\u0006.MessageList.Count == 0, "Fehler in VFInit");
			return true;
		}

		// Token: 0x060036CA RID: 14026 RVA: 0x000DE9E4 File Offset: 0x000DCBE4
		private static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003, _ISignature \u0004, _ISignature \u0005, LStringBuilder \u0006, IScope5 \u0007)
		{
			bool flag = \u0004.GetFlag(SignatureFlag.Structure);
			if (\u0004.POUType != Operator.FunctionBlock && !flag)
			{
				return;
			}
			if (\u0004.BaseSignatureId != Helper.InvalidId)
			{
				_ISignature isignature = \u0007[\u0004.BaseSignatureId] as _ISignature;
				if (((isignature != null) ? isignature.VirtualFunctionTable : null) != null)
				{
					\u0006.AppendLine("SUPER^.__VFInit();");
				}
			}
			InterfaceHierarchy u = (InterfaceHierarchy)\u0004.InterfaceHierarchy;
			_IVirtualFunctionTable ivirtualFunctionTable = \u0004.VirtualFunctionTable as _IVirtualFunctionTable;
			if (ivirtualFunctionTable != null)
			{
				_IVariable ivariable = (_IVariable)\u0002[IdentifierConstants.VFTableName][IdentifierConstants.GetVFTableVarName(\u0004.Id)];
				IScope5 u2 = global::\u0007.\u0005.\u0001(\u0002, \u0004.Id);
				global::\u0002.\u0011.\u0001(\u0004, \u0005, ivariable);
				\u0006.AppendLine(IdentifierConstants.VFTablePointer + " := ADR(" + ivariable.VersionedName + ");");
				IDictionary<int, int> offsetInterfaceMap = ivirtualFunctionTable.GetOffsetInterfaceMap();
				IList<IVFTableEntry> entries = ivirtualFunctionTable._Entries;
				foreach (int u3 in offsetInterfaceMap.Keys)
				{
					global::\u0002.\u0011.\u0001(\u0002, \u0006, entries, u3, u, u2);
				}
			}
			global::\u0002.\u0011.\u0001(\u0004, \u0007, \u0002, \u0003, \u0006);
		}

		// Token: 0x060036CB RID: 14027 RVA: 0x000DEB24 File Offset: 0x000DCD24
		private static void \u0001(_ICompileContext \u0002, LStringBuilder \u0003, IList<IVFTableEntry> \u0004, int \u0005, InterfaceHierarchy \u0006, IScope5 \u0007)
		{
			_IInterfaceOffsetEntry iinterfaceOffsetEntry = null;
			if (\u0004.Count > \u0005)
			{
				iinterfaceOffsetEntry = (\u0004[\u0005] as _IInterfaceOffsetEntry);
			}
			IInterfaceInfo ii = \u0006[iinterfaceOffsetEntry.HierarchyOffset];
			int num = \u0005;
			string stName;
			if (iinterfaceOffsetEntry != null && iinterfaceOffsetEntry.CPP)
			{
				stName = IdentifierConstants.GetInterfacePointerName(iinterfaceOffsetEntry.Id);
				num = \u0005 + 1;
			}
			else
			{
				stName = \u0006.GetUniqueInterfaceVariableName(ii);
			}
			ISignature signature;
			_IVariable ivariable = (_IVariable)\u0007.FindVariableLocal(stName, out signature);
			if (ivariable.HasAttribute(CompileAttributes.ATTRIBUTE_NOINIT))
			{
				return;
			}
			int num2 = num * \u0002.PointerSize;
			if (global::\u0002.\u0011.\u0001(\u0002.Codegenerator, CodegeneratorProperties.WordAddressing))
			{
				num2 /= 2;
			}
			iinterfaceOffsetEntry.InstancePointerOffset = ivariable.DataLocation.Offset;
			\u0003.AppendLine(string.Format("{0} := {1} + {2};", ivariable.OrgName, IdentifierConstants.VFTablePointer, num2));
		}

		// Token: 0x060036CC RID: 14028 RVA: 0x000DEBF4 File Offset: 0x000DCDF4
		private static void \u0001(_ISignature \u0002, IScope5 \u0003, _ICompileContext \u0004, _ICompileContext \u0005, LStringBuilder \u0006)
		{
			_ISignature isignature = (_ISignature)\u0003.MethodSignature;
			_ISignature u000E = ((\u0005 != null) ? \u0005.GetSignatureById(isignature.Id) : null) as _ISignature;
			foreach (_IVariable ivariable in \u0002.AllVariables)
			{
				if (!ivariable.HasFlag(VarFlag.Inout | VarFlag.External | VarFlag.ReplacedConstant) && ivariable.Type.Class != TypeClass.Reference && !ivariable.IsProperty)
				{
					bool flag;
					_IStatement istatement = \u001A.\u0001(ivariable, true, ivariable._Type.DeRefType as _IType, \u0019.\u0003.\u0001(ivariable.VersionedName), \u0003, out flag, isignature, u000E, "bInitRetains", "bInCopyCode", 0, \u0004) as _IStatement;
					if (istatement != null)
					{
						\u0006.Append(istatement.ToString());
					}
				}
			}
		}

		// Token: 0x060036CD RID: 14029 RVA: 0x000DECD0 File Offset: 0x000DCED0
		private static void \u0001(_ISignature \u0002, _ISignature \u0003, _IVariable \u0004)
		{
			_IVariable ivariable = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001());
			ivariable.Name = IdentifierConstants.VFTablePointer;
			ivariable.DataLocation = \u0019.\u0003.\u0001(0);
			ivariable._Type = \u0019.\u0003.\u0001(\u0004._Type);
			ivariable.SetFlag(VarFlag.Local | VarFlag.IsCompiled | VarFlag.RelativeInstance | VarFlag.NoInit | VarFlag.Implicit | VarFlag.NoCopy, true);
			if (\u0003 != null && \u0003[ivariable.VersionedName] != null)
			{
				ivariable.Id = \u0003[ivariable.VersionedName].Id;
			}
			else
			{
				ivariable.Id = \u0002.NextId;
			}
			\u0002.AddVariable(ivariable);
		}

		// Token: 0x060036CE RID: 14030 RVA: 0x000DED5C File Offset: 0x000DCF5C
		private static bool \u0001(ICodegenerator \u0002, CodegeneratorProperties \u0003)
		{
			ICodegenerator3 codegenerator = \u0002 as ICodegenerator3;
			return codegenerator != null && codegenerator.GetProperty(\u0003);
		}
	}
}
