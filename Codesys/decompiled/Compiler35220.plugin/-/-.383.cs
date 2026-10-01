using System;
using System.Linq;
using \u0007;
using \u0014;
using \u0019;
using _3S.CoDeSys.Compiler35220.ImplicitCode;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0017
{
	// Token: 0x020003CE RID: 974
	internal static class \u0018
	{
		// Token: 0x06003705 RID: 14085 RVA: 0x000E0ADC File Offset: 0x000DECDC
		internal static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003)
		{
			LList<_ISignature> llist = new LList<_ISignature>();
			foreach (_ISignature isignature in \u0002.AllSignatures.OfType<_ISignature>())
			{
				if (isignature.POUType == Operator.Interface)
				{
					llist.Add(isignature);
				}
			}
			LList<_ISignature> llist2 = new LList<_ISignature>();
			LDictionary<int, int> u = new LDictionary<int, int>();
			IScope5 u2 = \u0002.CreateGlobalIScope() as IScope5;
			foreach (_ISignature u3 in llist)
			{
				\u0018.\u0001(u3, u2, llist2, u);
			}
			foreach (_ISignature isignature2 in llist2)
			{
				string str = IdentifierConstants.InterfaceStructure(isignature2);
				LStringBuilder lstringBuilder = new LStringBuilder();
				lstringBuilder.AppendLine("TYPE " + str + ":");
				lstringBuilder.AppendLine("STRUCT");
				lstringBuilder.AppendLine("END_STRUCT");
				lstringBuilder.AppendLine("END_TYPE");
				_ISignature isignature3 = ParserHelper.\u0001(lstringBuilder.ToString(), true);
				\u0018.\u0001(isignature2, u2);
				\u0018.\u0001(isignature3, isignature2.InterfaceHierarchy as InterfaceHierarchy);
				isignature3.LibraryPath = isignature2.LibraryPath;
				isignature3.SetFlag(SignatureFlag.InterfaceLibraryObject, isignature2.GetFlag(SignatureFlag.InterfaceLibraryObject));
				isignature3.SetFlag(SignatureFlag.PoolSignature, isignature2.GetFlag(SignatureFlag.PoolSignature));
				isignature3.SetFlagInternal(SignatureFlagInternal.VersionFreeLibrary, isignature2.GetFlagInternal(SignatureFlagInternal.VersionFreeLibrary));
				_ISignature isignature4 = null;
				if (\u0003 != null)
				{
					isignature4 = \u0003[isignature3.GetSearchName(\u0002)];
				}
				isignature3 = isignature3.CreateCompiledSignature(isignature3, \u0002.HasByteSupport());
				isignature3.ObjectGuid = isignature2.ObjectGuid;
				isignature3.SetFlag(SignatureFlag.Generated, true);
				isignature3.SetFlag(SignatureFlag.InterfaceLibraryObject, isignature2.GetFlag(SignatureFlag.InterfaceLibraryObject));
				isignature3.SetFlagInternal(SignatureFlagInternal.VersionFreeLibrary, isignature2.GetFlagInternal(SignatureFlagInternal.VersionFreeLibrary));
				\u0002.AddSignature(isignature3, isignature4, \u0003, true);
				IScope5 u4 = global::\u0007.\u0005.\u0001(\u0002, isignature3.Id);
				global::\u0014.\u0013.\u0002(isignature3, u4, \u0002);
				Locator.\u0001(isignature3, isignature4, \u0002, \u0003);
			}
			foreach (_ISignature isignature5 in \u0002.AllSignatures.OfType<_ISignature>())
			{
				if (isignature5.POUType == Operator.FunctionBlock)
				{
					int num = 0;
					_ISignature u5 = null;
					if (\u0003 != null)
					{
						u5 = \u0003[isignature5.Id];
					}
					\u0018.\u0001(isignature5, u2);
					\u0018.\u0001(isignature5, u5, ref num);
				}
			}
		}

		// Token: 0x06003706 RID: 14086 RVA: 0x000E0DF8 File Offset: 0x000DEFF8
		private static void \u0001(_ISignature \u0002, IScope5 \u0003, LList<_ISignature> \u0004, LDictionary<int, int> \u0005)
		{
			if (\u0005.ContainsKey(\u0002.Id))
			{
				return;
			}
			\u0005.Add(\u0002.Id, \u0002.Id);
			if (\u0002.BaseSignatureId != Helper.InvalidId)
			{
				\u0018.\u0001(\u0003[\u0002.BaseSignatureId] as _ISignature, \u0003, \u0004, \u0005);
			}
			if (\u0002.InterfaceIds.Length != 0)
			{
				foreach (int nId in \u0002.InterfaceIds)
				{
					\u0018.\u0001(\u0003[nId] as _ISignature, \u0003, \u0004, \u0005);
				}
			}
			\u0004.Add(\u0002);
		}

		// Token: 0x06003707 RID: 14087 RVA: 0x000E0E8C File Offset: 0x000DF08C
		private static void \u0001(_ISignature \u0002, InterfaceHierarchy \u0003)
		{
			foreach (InterfaceInfo u in \u0003.Interfaces.OfType<InterfaceInfo>())
			{
				_IVariable ivariable = \u0018.\u0001(u, \u0003);
				ivariable.Id = \u0002.NextId;
				\u0002.AddVariable(ivariable);
			}
		}

		// Token: 0x06003708 RID: 14088 RVA: 0x000E0EF4 File Offset: 0x000DF0F4
		private static _IVariable \u0001(InterfaceInfo \u0002, InterfaceHierarchy \u0003)
		{
			string uniqueInterfaceVariableName = \u0003.GetUniqueInterfaceVariableName(\u0002);
			_IVariable ivariable = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001());
			ivariable.Name = uniqueInterfaceVariableName;
			if (\u0002.NoInit)
			{
				ivariable.AddAttribute(CompileAttributes.ATTRIBUTE_NOINIT, null);
			}
			if (\u0002.IsEqualParent)
			{
				IInterfaceInfo ii = \u0003[\u0002.ParentInterfaceIndex];
				string uniqueInterfaceVariableName2 = \u0003.GetUniqueInterfaceVariableName(ii);
				ivariable.AddAttribute(CompileAttributes.ATTRIBUTE_USELOCATION, uniqueInterfaceVariableName2);
			}
			_IArrayType iarrayType = \u0019.\u0003.\u0001(TypeTable.DWord);
			iarrayType.AddDimension(\u0019.\u0003.\u0001(0L), \u0019.\u0003.\u0001(5L));
			ivariable._Type = \u0019.\u0003.\u0001(iarrayType);
			ivariable.SetFlag(VarFlag.Local | VarFlag.IsCompiled | VarFlag.RelativeInstance | VarFlag.NoInit | VarFlag.Implicit, true);
			ivariable.SetFlag(VarFlag.NoCopy, true);
			ivariable.Comment = \u0002.OrgName;
			return ivariable;
		}

		// Token: 0x06003709 RID: 14089 RVA: 0x000E0FAC File Offset: 0x000DF1AC
		private static void \u0001(_ISignature \u0002, _ISignature \u0003, ref int \u0004)
		{
			InterfaceHierarchy interfaceHierarchy = \u0002.InterfaceHierarchy as InterfaceHierarchy;
			foreach (InterfaceInfo interfaceInfo in interfaceHierarchy.Interfaces.OfType<InterfaceInfo>())
			{
				if (!interfaceInfo.IsBaseInterfaceInfo)
				{
					_IVariable ivariable = \u0018.\u0001(interfaceInfo, interfaceHierarchy);
					if (\u0004 == 0 && \u0002.BaseSignatureId == Helper.InvalidId && \u0002.HasAttribute(CompileAttributes.ATTRIBUTE_CPPEXTERNAL))
					{
						ivariable.AddAttribute(CompileAttributes.ATTRIBUTE_RELATIVE_OFFSET, "0");
					}
					if (\u0003 != null && \u0003[ivariable.OrgName] != null)
					{
						ivariable.Id = \u0003[ivariable.OrgName].Id;
					}
					else
					{
						ivariable.Id = \u0002.NextId;
					}
					\u0002.InsertVariable(ivariable, \u0004);
					if (\u0003 != null && (\u0004 >= \u0003.AllVariables.Count || \u0003.AllVariables[\u0004] == null || \u0003.AllVariables[\u0004].Id != ivariable.Id))
					{
						\u0002.SetFlag(SignatureFlag.InitializeVirtualFunctionTable, true);
					}
					\u0004++;
				}
			}
		}

		// Token: 0x0600370A RID: 14090 RVA: 0x000E10D8 File Offset: 0x000DF2D8
		internal static void \u0001(_ISignature \u0002, IScope5 \u0003)
		{
			if (\u0002.POUType == Operator.Interface || \u0002.POUType == Operator.FunctionBlock)
			{
				InterfaceHierarchy interfaceHierarchy = new InterfaceHierarchy();
				interfaceHierarchy.Initialize(\u0003, \u0002);
				\u0002.InterfaceHierarchy = interfaceHierarchy;
			}
		}
	}
}
