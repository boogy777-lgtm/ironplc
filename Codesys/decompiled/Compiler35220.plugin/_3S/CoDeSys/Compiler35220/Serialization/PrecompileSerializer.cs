using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using \u000F;
using \u0012;
using \u001E;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Serialization
{
	// Token: 0x0200009C RID: 156
	public class PrecompileSerializer
	{
		// Token: 0x06000CF7 RID: 3319 RVA: 0x00021310 File Offset: 0x0001F510
		public void SerializeVariable(BinaryWriter bw, _IVariable variable)
		{
			PrecompileSerializer.\u0001 u = new PrecompileSerializer.\u0001();
			u.\u0001 = variable;
			bw.Write(u.\u0001.OrgName);
			_IType u2 = u.\u0001.OriginalType as _IType;
			global::\u0012.\u0005.Instance.\u0001(bw, u2);
			bw.Write((long)u.\u0001.Flags);
			_ISourcePosition sourcePosition = u.\u0001._SourcePosition;
			bw.Write(sourcePosition.PositionCombination);
			IEnumerable<KeyValuePair<string, string>> pairs = u.\u0001.Attributes.Select(new Func<string, KeyValuePair<string, string>>(u.\u0001));
			CommonSerializer.SerializeKeyValuePairs<string, string>(bw, pairs, new Action<BinaryWriter, string>(CommonSerializer.WriteString), new Action<BinaryWriter, string>(CommonSerializer.WriteString));
			CommonSerializer.SerializeDirectVariable_Nullable(bw, u.\u0001.Address);
			global::\u000F.\u0004.\u0001(bw, u.\u0001._Initial);
			IAssignmentExpression[] inputAssignments = u.\u0001.InputAssignments;
			if (inputAssignments == null)
			{
				bw.Write(0);
				return;
			}
			bw.Write(inputAssignments.Length);
			foreach (IAssignmentExpression assignmentExpression in inputAssignments)
			{
				global::\u000F.\u0004.\u0001(bw, (_IExprement)assignmentExpression);
			}
		}

		// Token: 0x06000CF8 RID: 3320 RVA: 0x00021430 File Offset: 0x0001F630
		public void SerializeSignature(BinaryWriter bw, _ISignature3 signature)
		{
			PrecompileSerializer.\u0002 u = new PrecompileSerializer.\u0002();
			u.\u0001 = signature;
			global::\u000F.\u0004.\u0001(bw, u.\u0001._NameExpression);
			global::\u000F.\u0004.\u0001(bw, u.\u0001._BaseSignature);
			IExpression[] interfaceExpressions = u.\u0001.InterfaceExpressions;
			bw.Write(interfaceExpressions.Length);
			foreach (IExpression expression in interfaceExpressions)
			{
				global::\u000F.\u0004.\u0001(bw, (_IExprement)expression);
			}
			bw.Write((uint)u.\u0001.POUType);
			IEnumerable<KeyValuePair<string, string>> pairs = u.\u0001.Attributes.Select(new Func<string, KeyValuePair<string, string>>(u.\u0001));
			CommonSerializer.SerializeKeyValuePairs<string, string>(bw, pairs, new Action<BinaryWriter, string>(CommonSerializer.WriteString), new Action<BinaryWriter, string>(CommonSerializer.WriteString));
			bw.Write((ulong)u.\u0001.Flags);
			bw.Write((ulong)u.\u0001.InternalFlags);
			bw.Write(u.\u0001.AllVariables.Count);
			foreach (_IVariable variable in u.\u0001.AllVariables)
			{
				this.SerializeVariable(bw, variable);
			}
			bw.Write(u.\u0001.LibraryPath);
			bw.Write(u.\u0001.ObjectGuid.ToByteArray());
			bw.Write(u.\u0001.ParentObjectGuid.ToByteArray());
			bw.Write(u.\u0001.MessageGuid.ToByteArray());
			bw.Write(u.\u0001.Checksum);
			bw.Write(u.\u0001.ChecksumNoInit);
			bw.Write(u.\u0001.TimeStamp);
			int num = 0;
			if (u.\u0001.UnusedDeclarationPositions != null)
			{
				num = u.\u0001.UnusedDeclarationPositions.Count<ISourcePosition>();
			}
			bw.Write(num);
			if (num > 0)
			{
				foreach (ISourcePosition pos in u.\u0001.UnusedDeclarationPositions)
				{
					this.SerializeSourcePosition(bw, pos);
				}
			}
		}

		// Token: 0x06000CF9 RID: 3321 RVA: 0x00021684 File Offset: 0x0001F884
		public void SerializeSourcePosition(BinaryWriter bw, ISourcePosition pos)
		{
			bw.Write(pos.ProjectHandle);
			bw.Write(pos.ObjectGuid.ToByteArray());
			bw.Write(pos.Position);
			bw.Write(pos.PositionOffset);
			bw.Write(pos.Length);
		}

		// Token: 0x06000CFA RID: 3322 RVA: 0x000216D8 File Offset: 0x0001F8D8
		public void SerializeCompiledPou_Green(BinaryWriter bw, _ICompiledPOU2 cpou, IDictionary<_IExprement, int> hashedExpressions)
		{
			bw.Write(cpou.OriginalName);
			bw.Write(cpou.ObjectGuid.ToByteArray());
			bw.Write(cpou.ParentObjectGuid.ToByteArray());
			bw.Write(cpou.MessageGuid.ToByteArray());
			bw.Write((uint)cpou.Flags);
			bw.Write((uint)cpou.InternalFlags);
			bw.Write(cpou.LibraryPath);
			bw.Write(cpou.Checksum);
			bw.Write(cpou.TimeStamp);
			_IStatement originalParseTree = (cpou as ICompiledPOUWithCompactedParseTree).OriginalParseTree;
			if (originalParseTree is IGreenTreeExprement)
			{
				bw.Write(true);
				ICompactedParseTreeInformation compactedParseTreeInformation = (cpou as ICompiledPOUWithCompactedParseTree).CompactedParseTreeInformation;
				\u001E.\u0002.\u0001(bw, hashedExpressions, originalParseTree, compactedParseTreeInformation);
				return;
			}
			bw.Write(false);
			global::\u000F.\u0004.\u0001(bw, originalParseTree);
		}

		// Token: 0x06000CFB RID: 3323 RVA: 0x000217A8 File Offset: 0x0001F9A8
		public void SerializePlaceholder(BinaryWriter bw, _ILibraryPlaceholder placeholder)
		{
			bw.Write(placeholder.Name);
			bw.Write(placeholder.Namespace ?? string.Empty);
			bw.Write(placeholder.DefaultLibraryId ?? string.Empty);
			bw.Write(placeholder.Resolver.ToByteArray());
			bw.Write(placeholder.LibManGuid.ToByteArray());
			bw.Write(placeholder.PublishSymbols);
			bw.Write(placeholder.LinkAllContent);
			bw.Write(placeholder.QualifiedOnlyLocal);
			bw.Write(placeholder.LinkInSimulation);
			bw.Write(placeholder.Optional);
		}

		// Token: 0x06000CFC RID: 3324 RVA: 0x00021850 File Offset: 0x0001FA50
		public void SerializePrecompileContext(BinaryWriter bw, _IPreCompileContext2 precom, IDictionary<_IExprement, int> hashedExpressions)
		{
			bw.Write((uint)precom.KindOf);
			bw.Write(precom.LibraryPath);
			bw.Write(precom.OrgNamespace ?? string.Empty);
			bw.Write(precom.ApplicationGuid.ToByteArray());
			bw.Write(precom.LinkAll);
			bw.Write(precom.LinkInSimulation);
			bw.Write(precom.IsInterfaceLibrary);
			bw.Write(APEnvironmentFacade.Instance.CompileOptions.UnicodeIdentifiers);
			bw.Write(precom.OnlineChangeable);
			bw.Write(precom.IgnoreLinkAll);
			bw.Write(precom.QualifiedAccessOnly);
			bw.Write(precom.SystemApplication);
			bw.Write(precom.SupportDynamicMemory);
			bw.Write(precom.GenerateContent);
			bw.Write(precom.DeviceApplication);
			bw.Write(precom.PrecompiledLibrary);
			bw.Write(precom.Support32BitOnly);
			bw.Write(precom.TimeStamp);
			bw.Write(precom.TargetOutputSize);
			bw.Write(precom.TargetInputSize);
			bw.Write(precom.TargetMemorySize);
			bw.Write(precom.TargetStaticSize);
			_ITaskList taskList = precom.TaskList;
			bw.Write(taskList.Count);
			for (int i = 0; i < taskList.Count; i++)
			{
				CommonSerializer.SerializeTaskInfo(bw, (ITaskInfo2)taskList[i]);
			}
			CommonSerializer.SerializeSlotPOUList(bw, (_ISlotPOUList2)precom.SlotPOUs);
			IList<IStaticMemorySegment> staticMemorySegments = precom.StaticMemorySegments;
			bw.Write(staticMemorySegments.Count);
			for (int j = 0; j < staticMemorySegments.Count; j++)
			{
				CommonSerializer.SerializeStaticMemorySegment(bw, staticMemorySegments[j]);
			}
			_ILibraryPlaceholder[] placeholders = precom.Placeholders;
			bw.Write(placeholders.Length);
			foreach (_ILibraryPlaceholder placeholder in placeholders)
			{
				this.SerializePlaceholder(bw, placeholder);
			}
			bw.Write(precom.UnitTestingDefine);
			CommonSerializer.SerializeDefines(bw, precom.DefineTable);
			CommonSerializer.SerializeDefines(bw, precom.TargetDefineTable);
			IList<ISignature4> allSignaturesFlat = precom.GetAllSignaturesFlat();
			int count = allSignaturesFlat.Count;
			bw.Write(count);
			foreach (ISignature4 signature in allSignaturesFlat)
			{
				this.SerializeSignature(bw, (_ISignature3)signature);
			}
			IEnumerable<_ICompiledPOU> allCompiledPOUs = precom.AllCompiledPOUs;
			int value = allCompiledPOUs.Count<_ICompiledPOU>();
			bw.Write(value);
			foreach (_ICompiledPOU icompiledPOU in allCompiledPOUs)
			{
				this.SerializeCompiledPou_Green(bw, (_ICompiledPOU2)icompiledPOU, hashedExpressions);
			}
		}

		// Token: 0x06000CFD RID: 3325 RVA: 0x00021B14 File Offset: 0x0001FD14
		public void SerializeLibraryParams(BinaryWriter bw, ILibParameterTable paramTable)
		{
			IDictionary dictionary = (paramTable != null) ? paramTable.ParameterTable : null;
			if (dictionary == null)
			{
				bw.Write(0);
				return;
			}
			bw.Write(dictionary.Count);
			foreach (DictionaryEntry dictionaryEntry in dictionary.Cast<DictionaryEntry>())
			{
				bw.Write((string)dictionaryEntry.Key);
				global::\u000F.\u0004.\u0001(bw, (_IExprement)dictionaryEntry.Value);
			}
		}

		// Token: 0x06000CFE RID: 3326 RVA: 0x00021BA4 File Offset: 0x0001FDA4
		public void SerializeLibInfo(BinaryWriter bw, ILMLibraryInfo5 libInfo)
		{
			bw.Write(libInfo.DefaultNamespace);
			bw.Write(libInfo.Identification);
			bw.Write(libInfo.LinkAllContent);
			bw.Write(libInfo.LinkInSimulation);
			bw.Write(libInfo.Namespace);
			this.SerializeLibraryParams(bw, libInfo.ParamTable);
			bw.Write(libInfo.PublishSymbols);
			bw.Write(libInfo.QualifiedOnly);
			bw.Write(libInfo.SystemApplication);
			bw.Write(libInfo.SystemLibrary);
			bw.Write(libInfo.QualifiedOnlyLocal);
			bw.Write(libInfo.OnlineChangeable);
			bw.Write(libInfo.Optional);
			bw.Write(libInfo.PoolLibrary);
			bw.Write(libInfo.UnresolvedReference);
		}

		// Token: 0x06000CFF RID: 3327 RVA: 0x00021C68 File Offset: 0x0001FE68
		public void SerializePlaceholderInfo(BinaryWriter bw, _ILibraryPlaceholder info)
		{
			bw.Write(info.DefaultLibraryId);
			bw.Write(info.Name);
			bw.Write(info.Namespace);
			bw.Write(info.PublishSymbols);
			bw.Write(info.Resolver.ToByteArray());
			bw.Write(info.QualifiedOnlyLocal);
			bw.Write(info.Optional);
			bw.Write(info.LinkAllContent);
			bw.Write(info.LinkInSimulation);
			bw.Write(info.LibManGuid.ToByteArray());
		}

		// Token: 0x06000D00 RID: 3328 RVA: 0x00021D00 File Offset: 0x0001FF00
		public void SerializeLibraryList(BinaryWriter bw, ILMLibraryList2 libList, Guid appGuid, string precomAppGuidLibraryPath)
		{
			bw.Write(appGuid.ToByteArray());
			bw.Write(precomAppGuidLibraryPath);
			bw.Write(libList.LibManGuid.ToByteArray());
			bw.Write(libList.ObjectGuid.ToByteArray());
			bw.Write(libList.LibraryId);
			ILMLibraryInfo[] libraries = libList.Libraries;
			bw.Write(libraries.Count<ILMLibraryInfo>());
			foreach (ILMLibraryInfo ilmlibraryInfo in libraries)
			{
				this.SerializeLibInfo(bw, (ILMLibraryInfo5)ilmlibraryInfo);
			}
			ILMPlaceholderInfo[] placeholders = libList.Placeholders;
			bw.Write(placeholders.Count<ILMPlaceholderInfo>());
			foreach (ILMPlaceholderInfo ilmplaceholderInfo in placeholders)
			{
				this.SerializeLibraryParams(bw, ilmplaceholderInfo.ParamTable);
				this.SerializePlaceholderInfo(bw, (_ILibraryPlaceholder)ilmplaceholderInfo.PlaceholderInfo);
			}
		}

		// Token: 0x06000D01 RID: 3329 RVA: 0x00021DE4 File Offset: 0x0001FFE4
		public void SerializeApplicationDeviceTable(BinaryWriter bw, _IApplicationDeviceTable2 appDevTable)
		{
			bw.Write(appDevTable.DeviceOfApplication.Count);
			foreach (KeyValuePair<Guid, Guid> keyValuePair in appDevTable.DeviceOfApplication)
			{
				bw.Write(keyValuePair.Key.ToByteArray());
				bw.Write(keyValuePair.Value.ToByteArray());
			}
			bw.Write(appDevTable.ApplicationNameTable.Count);
			foreach (KeyValuePair<Guid, string> keyValuePair2 in appDevTable.ApplicationNameTable)
			{
				bw.Write(keyValuePair2.Key.ToByteArray());
				bw.Write(keyValuePair2.Value);
			}
			bw.Write(appDevTable.SimulationApplicationNameTable.Count);
			foreach (KeyValuePair<Guid, string> keyValuePair3 in appDevTable.SimulationApplicationNameTable)
			{
				bw.Write(keyValuePair3.Key.ToByteArray());
				bw.Write(keyValuePair3.Value);
			}
			bw.Write(appDevTable.DeviceNameTable.Count);
			foreach (KeyValuePair<Guid, string> keyValuePair4 in appDevTable.DeviceNameTable)
			{
				bw.Write(keyValuePair4.Key.ToByteArray());
				bw.Write(keyValuePair4.Value);
			}
			bw.Write(appDevTable.TargetIdOfDevice.Count);
			foreach (KeyValuePair<Guid, IDeviceIdentification> keyValuePair5 in appDevTable.TargetIdOfDevice)
			{
				bw.Write(keyValuePair5.Key.ToByteArray());
				bw.Write(keyValuePair5.Value.Id);
				bw.Write(keyValuePair5.Value.Type);
				bw.Write(keyValuePair5.Value.Version);
			}
			bw.Write(appDevTable.ClonesOfApplication.Count);
			foreach (KeyValuePair<Guid, ICollection> keyValuePair6 in appDevTable.ClonesOfApplication)
			{
				bw.Write(keyValuePair6.Key.ToByteArray());
				bw.Write(keyValuePair6.Value.Count);
				foreach (object obj in keyValuePair6.Value)
				{
					bw.Write(((Guid)obj).ToByteArray());
				}
			}
			bw.Write(appDevTable.SubApplicationTable.Count);
			foreach (KeyValuePair<Guid, Guid> keyValuePair7 in appDevTable.SubApplicationTable)
			{
				bw.Write(keyValuePair7.Key.ToByteArray());
				bw.Write(keyValuePair7.Value.ToByteArray());
			}
			bw.Write(appDevTable.MemorySettingsProviderTable.Count);
			foreach (KeyValuePair<Guid, Guid> keyValuePair8 in appDevTable.MemorySettingsProviderTable)
			{
				bw.Write(keyValuePair8.Key.ToByteArray());
				bw.Write(keyValuePair8.Value.ToByteArray());
			}
		}

		// Token: 0x06000D02 RID: 3330 RVA: 0x000221E8 File Offset: 0x000203E8
		public void SerializeRelatedObjectTable(BinaryWriter bw, ILMRelatedObjectTable relatedObjects)
		{
			bw.Write(relatedObjects.Keys.Count<Guid>());
			foreach (Guid key in relatedObjects.Keys)
			{
				bw.Write(key.ToByteArray());
				IEnumerable<Guid> values = relatedObjects.GetValues(key);
				bw.Write(values.Count<Guid>());
				foreach (Guid guid in values)
				{
					bw.Write(guid.ToByteArray());
				}
			}
		}

		// Token: 0x06000D03 RID: 3331 RVA: 0x000222A0 File Offset: 0x000204A0
		internal void \u0001(BinaryWriter \u0002, ITextualPreCompCrossReferencesSerializable \u0003)
		{
			IEnumerable<ITextualPreCompCrossReferenceSerializable> crossReferences = \u0003.CrossReferences;
			IList<Guid> sharedGuidTable = \u0003.SharedGuidTable;
			CommonSerializer.SerializeList<Guid>(\u0002, sharedGuidTable, new Action<BinaryWriter, Guid>(CommonSerializer.WriteGuid));
			\u0002.Write(crossReferences.Count<ITextualPreCompCrossReferenceSerializable>());
			foreach (ITextualPreCompCrossReferenceSerializable textualPreCompCrossReferenceSerializable in crossReferences)
			{
				\u0002.Write(textualPreCompCrossReferenceSerializable.Name);
				\u0002.Write(textualPreCompCrossReferenceSerializable.AccessingObjectsByMessageGuid.Count);
				foreach (KeyValuePair<int, ICollection<int>> keyValuePair in textualPreCompCrossReferenceSerializable.AccessingObjectsByMessageGuid)
				{
					\u0002.Write(keyValuePair.Key);
					CommonSerializer.SerializeList<int>(\u0002, keyValuePair.Value, new Action<BinaryWriter, int>(CommonSerializer.WriteInt));
				}
			}
		}

		// Token: 0x0200009D RID: 157
		[CompilerGenerated]
		private sealed class \u0001
		{
			// Token: 0x06000D06 RID: 3334 RVA: 0x000223A0 File Offset: 0x000205A0
			internal KeyValuePair<string, string> \u0001(string \u0002)
			{
				return new KeyValuePair<string, string>(\u0002, this.\u0001.GetAttributeValue(\u0002));
			}

			// Token: 0x04000232 RID: 562
			public _IVariable \u0001;
		}

		// Token: 0x0200009E RID: 158
		[CompilerGenerated]
		private sealed class \u0002
		{
			// Token: 0x06000D08 RID: 3336 RVA: 0x000223BC File Offset: 0x000205BC
			internal KeyValuePair<string, string> \u0001(string \u0002)
			{
				return new KeyValuePair<string, string>(\u0002, this.\u0001.GetAttributeValue(\u0002));
			}

			// Token: 0x04000233 RID: 563
			public _ISignature3 \u0001;
		}
	}
}
