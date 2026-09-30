using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using \u000F;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0082;

namespace _3S.CoDeSys.Compiler35220.Serialization
{
	// Token: 0x02000087 RID: 135
	public class CompileContextSerializer
	{
		// Token: 0x06000BAC RID: 2988 RVA: 0x0001A914 File Offset: 0x00018B14
		public static LList<ISignature> Sort(ISignature[] signs)
		{
			LList<ISignature> llist = Enumerable.ToLList<ISignature>(signs);
			llist.Sort(new Comparison<ISignature>(CompileContextSerializer.<>c.<>9.\u0001));
			return llist;
		}

		// Token: 0x06000BAD RID: 2989 RVA: 0x0001A944 File Offset: 0x00018B44
		public static LList<Guid> Sort(Guid[] guids)
		{
			LList<Guid> llist = Enumerable.ToLList<Guid>(guids);
			llist.Sort(new Comparison<Guid>(CompileContextSerializer.<>c.<>9.\u0001));
			return llist;
		}

		// Token: 0x06000BAE RID: 2990 RVA: 0x0001A974 File Offset: 0x00018B74
		public static LList<KeyValuePair<string, string>> Sort(IEnumerable<KeyValuePair<string, string>> list)
		{
			LList<KeyValuePair<string, string>> llist = Enumerable.ToLList<KeyValuePair<string, string>>(list);
			llist.Sort(new Comparison<KeyValuePair<string, string>>(CompileContextSerializer.<>c.<>9.\u0001));
			return llist;
		}

		// Token: 0x06000BAF RID: 2991 RVA: 0x0001A9A4 File Offset: 0x00018BA4
		public static LList<ILibInfoSerializable> Sort(IEnumerable<ILibInfoSerializable> list)
		{
			LList<ILibInfoSerializable> llist = Enumerable.ToLList<ILibInfoSerializable>(list);
			llist.Sort(new Comparison<ILibInfoSerializable>(CompileContextSerializer.<>c.<>9.\u0001));
			return llist;
		}

		// Token: 0x06000BB0 RID: 2992 RVA: 0x0001A9D4 File Offset: 0x00018BD4
		public static LList<int> Sort(int[] values)
		{
			LList<int> llist = Enumerable.ToLList<int>(values);
			llist.Sort();
			return llist;
		}

		// Token: 0x06000BB1 RID: 2993 RVA: 0x0001A9E4 File Offset: 0x00018BE4
		internal static void \u0001(BinaryWriter \u0002, _IExpression \u0003)
		{
			global::\u000F.\u0004.\u0002(\u0002, \u0003);
		}

		// Token: 0x06000BB2 RID: 2994 RVA: 0x0001A9F0 File Offset: 0x00018BF0
		public void SerializeCompileContext(BinaryWriter bw, ICompileContextSerializable comcon)
		{
			bw.Write((int)comcon.KindOf);
			bw.Write(comcon.ApplicationGuid.ToByteArray());
			bw.Write(comcon.SerializableSignatureIdManager);
			bw.Write(comcon.SerializableLibraryIdManager);
			bw.Write(comcon.SupportDynamicMemory);
			bw.Write(comcon.GenerateContent);
			bw.Write(comcon.TimeStampContext);
			bw.Write(comcon.TimeStampPool);
			bw.Write(comcon.ContainsOnlineChangeCode);
			bw.Write(comcon.CodeId.ToByteArray());
			bw.Write(comcon.DataId.ToByteArray());
			bw.Write(comcon.LastCodeId.ToByteArray());
			bw.Write(comcon.LastDataId.ToByteArray());
			bw.Write(comcon.CheckSumCode);
			bw.Write(comcon.CheckSumData);
			bw.Write(comcon.CheckSumCodeLast);
			bw.Write(comcon.CheckSumDataLast);
			CommonSerializer.SerializeList<ISignatureSerializable2>(bw, comcon.SignaturesSerializable.Cast<ISignatureSerializable2>(), new Action<BinaryWriter, ISignatureSerializable2>(this.SerializeSignature));
			CommonSerializer.SerializeList<ISignatureSerializable2>(bw, comcon.GlobalSignaturesSerializable.Cast<ISignatureSerializable2>(), new Action<BinaryWriter, ISignatureSerializable2>(this.SerializeSignature));
			CommonSerializer.SerializeList<ICompiledPOUSerializable>(bw, comcon.CompiledPOUsSerializable, new Action<BinaryWriter, ICompiledPOUSerializable>(this.SerializeCompiledPOU));
			this.SerializeTaskList(bw, comcon.TaskList);
			bw.Write(comcon.MemorySettingsChecksum);
			if (!CommonSerializer.SerializeNullabe(bw, comcon.StaticMemorySegments == null))
			{
				CommonSerializer.SerializeList<IStaticMemorySegment>(bw, comcon.StaticMemorySegments, new Action<BinaryWriter, IStaticMemorySegment>(CommonSerializer.SerializeStaticMemorySegment));
			}
			this.SerializeDataManager(bw, (IDataManagerSerializable)comcon.DataManager);
			CommonSerializer.SerializeDefines(bw, comcon.DefineTable);
			CommonSerializer.SerializeDefines(bw, comcon.PrecompileDefineTable);
			bw.Write(comcon.bFastOnlineChange);
			if (!CommonSerializer.SerializeNullabe(bw, comcon.CompileOptionsSerializable == null))
			{
				this.SerializeCompileOptions(bw, comcon.CompileOptionsSerializable);
			}
			ILibraryTableSerializable libraryTableSerializable = (ILibraryTableSerializable)comcon._LibraryTable;
			IEnumerable<ILibInfoSerializable> enumerable = (libraryTableSerializable != null) ? libraryTableSerializable.LibInfoSerializable : null;
			if (!CommonSerializer.SerializeNullabe(bw, enumerable == null))
			{
				enumerable = CompileContextSerializer.Sort(enumerable);
				CommonSerializer.SerializeList<ILibInfoSerializable>(bw, enumerable, new Action<BinaryWriter, ILibInfoSerializable>(this.SerializeLibInfo));
			}
			bw.Write(comcon.PrecompileContextNamesChecksum);
			bw.Write(comcon.ParameterTableChecksum);
			bw.Write(comcon.LibCheckSum);
			bw.Write(comcon.PoolLibCheckSum);
			CommonSerializer.SerializeSlotPOUList(bw, comcon.SlotPOUs as _ISlotPOUList2);
			this.SerializeDirVarCrossRefTable(bw, comcon.GetDirectVariableTable() as _IDirectVariableCrossRefTable);
			bw.Write(comcon.LoadWithoutTargetsettings);
			bw.Write(comcon.CodegeneratorGuid.ToByteArray());
			bw.Write(comcon.SimulationMode);
			bw.Write(comcon.ContainsCode);
			this.SerializeDeviceIdentification(bw, comcon.DeviceId);
			CommonSerializer.SerializeList<_IImplicitReferenceVariable>(bw, comcon.ImplicitReferenceVariables, new Action<BinaryWriter, _IImplicitReferenceVariable>(this.SerializeImplicitReferenceVariable));
			bw.Write(comcon.ProjectChecksum);
		}

		// Token: 0x06000BB3 RID: 2995 RVA: 0x0001ACD4 File Offset: 0x00018ED4
		public void SerializeImplicitReferenceVariable(BinaryWriter bw, _IImplicitReferenceVariable var)
		{
			this.SerializeVariable(bw, (IVariableSerializable)var.Var);
			bw.Write(var.SignatureId);
		}

		// Token: 0x06000BB4 RID: 2996 RVA: 0x0001ACF4 File Offset: 0x00018EF4
		public void SerializeDeviceIdentification(BinaryWriter bw, IDeviceIdentification deviceid)
		{
			bw.Write(deviceid.Type);
			bw.Write(deviceid.Id);
			bw.Write(deviceid.Version);
		}

		// Token: 0x06000BB5 RID: 2997 RVA: 0x0001AD1C File Offset: 0x00018F1C
		public void SerializeDirVarCrossRefTable(BinaryWriter bw, _IDirectVariableCrossRefTable dirVarCrossRefs)
		{
			IDirectVariable[] allDirectVariables = dirVarCrossRefs.AllDirectVariables;
			bw.Write(allDirectVariables.Length);
			foreach (IDirectVariable directVariable in allDirectVariables)
			{
				CommonSerializer.SerializeDirectVariable_Nullable(bw, directVariable);
				IAddressCrossReference[] crossReferencesOfDirectVariable = dirVarCrossRefs.GetCrossReferencesOfDirectVariable(directVariable);
				CommonSerializer.SerializeList<IAddressCrossReference>(bw, crossReferencesOfDirectVariable, new Action<BinaryWriter, IAddressCrossReference>(this.SerializeDirVarCrossRef));
			}
		}

		// Token: 0x06000BB6 RID: 2998 RVA: 0x0001AD74 File Offset: 0x00018F74
		public void SerializeDirVarCrossRef(BinaryWriter bw, IAddressCrossReference crossRef)
		{
			bw.Write(crossRef.CodeId);
			CommonSerializer.SerializeList<IAddressCodePositionSerializable>(bw, crossRef.Positions.Cast<IAddressCodePositionSerializable>(), new Action<BinaryWriter, IAddressCodePositionSerializable>(this.SerializeAddressCodePosition));
		}

		// Token: 0x06000BB7 RID: 2999 RVA: 0x0001ADA0 File Offset: 0x00018FA0
		public void SerializeAddressCodePosition(BinaryWriter bw, IAddressCodePositionSerializable position)
		{
			bw.Write((byte)position.Access);
			bw.Write(position.TypeSize);
			bw.Write(position.PositionToSave);
		}

		// Token: 0x06000BB8 RID: 3000 RVA: 0x0001ADC8 File Offset: 0x00018FC8
		public void SerializeLibInfo(BinaryWriter bw, ILibInfoSerializable libInfo)
		{
			bw.Write(libInfo.LibraryId);
			bw.Write(libInfo.Namespace);
			bw.Write(libInfo.ReferencingLibrary);
			bw.Write(libInfo.OutOfPool);
			bw.Write(libInfo.Id);
			bw.Write(libInfo.QualifiedOnly);
			bw.Write(libInfo.PublishSymbols);
		}

		// Token: 0x06000BB9 RID: 3001 RVA: 0x0001AE2C File Offset: 0x0001902C
		public void SerializeCompileOptions(BinaryWriter bw, ICompileOptionsSerializable options)
		{
			bw.Write(options.LoggingInBreakpointsToSave);
			bw.Write(options.ReplaceConstantsToSave);
			bw.Write(options.UnicodeToSave);
			CommonSerializer.SerializeList<int>(bw, options.CompilerVersionToSave, new Action<BinaryWriter, int>(CommonSerializer.WriteInt));
		}

		// Token: 0x06000BBA RID: 3002 RVA: 0x0001AE6C File Offset: 0x0001906C
		public void SerializeCompiledPOU(BinaryWriter bw, ICompiledPOUSerializable cpou)
		{
			bw.Write(cpou.OriginalName);
			bw.Write(cpou.ObjectGuid.ToByteArray());
			bw.Write(cpou.MessageGuid.ToByteArray());
			bw.Write(cpou.ParentObjectGuid.ToByteArray());
			bw.Write(cpou.SignatureId);
			bw.Write((int)cpou.Flags);
			if (!CommonSerializer.SerializeNullabe(bw, cpou.CompiledCode == null || cpou.CompiledCode is ICompiledCodeEmptyPlaceholder))
			{
				this.\u0001(bw, cpou.CompiledCode);
			}
			bw.Write(cpou.ScratchSize);
			bw.Write(cpou.TimeStamp);
			bw.Write(cpou.Checksum);
			bw.Write(cpou.LibraryPath);
			bw.Write(cpou.MaxParamSize);
			if (!CommonSerializer.SerializeNullabe(bw, cpou.BreakpointList == null))
			{
				this.SerializeBreakpointList(bw, cpou.BreakpointList as _IBreakpointList);
			}
			if (!CommonSerializer.SerializeNullabe(bw, cpou.Messages == null))
			{
				CommonSerializer.SerializeList<_ICompilerMessage>(bw, cpou.Messages, new Action<BinaryWriter, _ICompilerMessage>(CommonSerializer.SerializeMessage));
			}
			bw.Write(cpou.CodeGeneratorStackSize);
			if (!CommonSerializer.SerializeNullabe(bw, cpou.BitWriteAccesses == null))
			{
				CommonSerializer.SerializeList<IBitWriteAccessSerializable>(bw, cpou.BitWriteAccesses.Cast<IBitWriteAccessSerializable>(), new Action<BinaryWriter, IBitWriteAccessSerializable>(this.SerializeBitWriteAccess));
			}
			if (!CommonSerializer.SerializeNullabe(bw, cpou.TryCatchCodeAddresses == null))
			{
				CommonSerializer.SerializeList<_IDataLocation>(bw, cpou.TryCatchFPAddresses.Cast<_IDataLocation>(), new Action<BinaryWriter, _IDataLocation>(this.SerializeDataLocation));
			}
			if (!CommonSerializer.SerializeNullabe(bw, cpou.TryCatchCodeAddresses == null))
			{
				CommonSerializer.SerializeList<int>(bw, cpou.TryCatchCodeAddresses, new Action<BinaryWriter, int>(CommonSerializer.WriteInt));
			}
			bw.Write((int)cpou.InternalFlags);
			if (!CommonSerializer.SerializeNullabe(bw, cpou.PrecompileMessages == null))
			{
				CommonSerializer.SerializeList<_ICompilerMessage>(bw, cpou.PrecompileMessages, new Action<BinaryWriter, _ICompilerMessage>(CommonSerializer.SerializeMessage));
			}
		}

		// Token: 0x06000BBB RID: 3003 RVA: 0x0001B058 File Offset: 0x00019258
		private void \u0001(BinaryWriter \u0002, ICompiledCode \u0003)
		{
			ICompiledCodeSerializable compiledCodeSerializable = \u0003 as ICompiledCodeSerializable;
			if (compiledCodeSerializable == null)
			{
				\u0002.Write(3);
				CommonSerializer.\u0001(\u0002, (IArchivable)\u0003);
				return;
			}
			this.\u0001(\u0002, compiledCodeSerializable);
		}

		// Token: 0x06000BBC RID: 3004 RVA: 0x0001B08C File Offset: 0x0001928C
		private void \u0001(BinaryWriter \u0002, ICompiledCodeSerializable \u0003)
		{
			if (\u0003 is ICompiledCodeDataRelocSerializable)
			{
				\u0002.Write(2);
				\u0002.Write(((ICompiledCodeDataRelocSerializable)\u0003).MotorolaByteOrder);
			}
			else if (\u0003 is ICompiledCodeDataSerializable)
			{
				\u0002.Write(1);
				\u0002.Write(((ICompiledCodeDataSerializable)\u0003).RelatedId);
			}
			else
			{
				\u0002.Write(0);
			}
			ICompiledCodeSerializable2 compiledCodeSerializable = (ICompiledCodeSerializable2)\u0003;
			\u0002.Write((ushort)compiledCodeSerializable.Flags);
			this.SerializeDataLocation(\u0002, (_IDataLocation)compiledCodeSerializable.Location);
			CommonSerializer.\u0001(\u0002, (IArchivable)compiledCodeSerializable.RelocationList);
			\u0002.Write(compiledCodeSerializable.NumCodeBytes);
			compiledCodeSerializable.WriteCodeBytesToWriter(\u0002);
		}

		// Token: 0x06000BBD RID: 3005 RVA: 0x0001B130 File Offset: 0x00019330
		public void SerializeBreakpointList(BinaryWriter bw, _IBreakpointList bpList)
		{
			bw.Write(bpList.FirstIndex);
			IBreakpointSerializable[] array = new IBreakpointSerializable[bpList.Count];
			bpList.CopyTo(array, 0);
			CommonSerializer.SerializeList<IBreakpointSerializable>(bw, array, new Action<BinaryWriter, IBreakpointSerializable>(this.SerializeBreakpoint));
		}

		// Token: 0x06000BBE RID: 3006 RVA: 0x0001B170 File Offset: 0x00019370
		public void SerializeTaskList(BinaryWriter bw, _ITaskList tasklist)
		{
			bw.Write(tasklist.Count);
			for (int i = 0; i < tasklist.Count; i++)
			{
				CommonSerializer.SerializeTaskInfo(bw, tasklist[i] as ITaskInfo2);
			}
		}

		// Token: 0x06000BBF RID: 3007 RVA: 0x0001B1AC File Offset: 0x000193AC
		public void SerializeBitWriteAccess(BinaryWriter bw, IBitWriteAccessSerializable access)
		{
			bw.Write(access.SignatureId);
			bw.Write(access.Area);
			bw.Write(access.Offset);
			bw.Write(access.BitNr);
			bw.Write(access.Symbol);
			bw.Write(access.PositionCombination);
		}

		// Token: 0x06000BC0 RID: 3008 RVA: 0x0001B204 File Offset: 0x00019404
		public void SerializeBreakpoint(BinaryWriter bw, IBreakpointSerializable bp)
		{
			byte b = 0;
			if (bp.TryCatchId >= 0)
			{
				b = 1;
			}
			bw.Write(b);
			if (b == 1)
			{
				bw.Write(bp.TryCatchId);
			}
			bw.Write(bp.Len);
			bw.Write(bp.Offset);
			if (!CommonSerializer.SerializeNullabe(bw, bp.Successors == null))
			{
				CommonSerializer.SerializeList<int>(bw, bp.Successors, new Action<BinaryWriter, int>(CommonSerializer.WriteInt));
			}
			if (!CommonSerializer.SerializeNullabe(bw, bp.StepInSuccessors == null))
			{
				CommonSerializer.SerializeList<_IStepInPosition>(bw, bp.StepInSuccessors.Select(new Func<IStepInPosition, _IStepInPosition>(CompileContextSerializer.<>c.<>9.\u0001)), new Action<BinaryWriter, _IStepInPosition>(this.SerializeStepInSuccessor));
			}
			if (!CommonSerializer.SerializeNullabe(bw, bp.AssemblySuccessors == null))
			{
				CommonSerializer.SerializeList<int>(bw, bp.AssemblySuccessors, new Action<BinaryWriter, int>(CommonSerializer.WriteInt));
			}
			bw.Write(bp.AreaGPRegister);
			bw.Write(bp.OffsetGPRegister);
			bw.Write(bp.PositionCombination);
			bw.Write(bp.ExceptionHandlingSuccessor);
		}

		// Token: 0x06000BC1 RID: 3009 RVA: 0x0001B31C File Offset: 0x0001951C
		public void SerializeStepInSuccessor(BinaryWriter bw, _IStepInPosition stepInPosition)
		{
			bw.Write(stepInPosition.SignatureId);
			bw.Write((byte)stepInPosition.KindOfCall);
			if (!CommonSerializer.SerializeNullabe(bw, stepInPosition.StepOutBreakpoint == null))
			{
				this.SerializeBreakpoint(bw, stepInPosition.StepOutBreakpoint as IBreakpointSerializable);
			}
			if (!CommonSerializer.SerializeNullabe(bw, stepInPosition.StepInBreakpoint == null))
			{
				this.SerializeBreakpoint(bw, stepInPosition.StepInBreakpoint as IBreakpointSerializable);
			}
		}

		// Token: 0x06000BC2 RID: 3010 RVA: 0x0001B388 File Offset: 0x00019588
		public void SerializeSignature(BinaryWriter bw, ISignatureSerializable2 signature)
		{
			CompileContextSerializer.\u0001 u = new CompileContextSerializer.\u0001();
			u.\u0001 = signature;
			CompileContextSerializer.\u0001(bw, u.\u0001._NameExpression);
			bw.Write(u.\u0001.TimeStamp);
			bw.Write((long)u.\u0001.Flags);
			bw.Write((ulong)u.\u0001.InternalFlags);
			bw.Write(u.\u0001.Id);
			bw.Write(u.\u0001.ParentSignatureId);
			bw.Write(u.\u0001.ObjectGuid.ToByteArray());
			bw.Write(u.\u0001.MessageGuid.ToByteArray());
			bw.Write(u.\u0001.ParentObjectGuid.ToByteArray());
			bw.Write(u.\u0001.LibraryPath);
			this.SerializeDataLocation(bw, u.\u0001.FPDataLocation as _IDataLocation);
			bw.Write(u.\u0001.SerializableVariableIdManagement);
			bw.Write(u.\u0001.TaskReferenceList.Length);
			bw.Write(u.\u0001.TaskReferenceList);
			IVirtualFunctionTableSerializable virtualFunctionTableSerializable = (IVirtualFunctionTableSerializable)u.\u0001._VirtualFunctionTable;
			if (!CommonSerializer.SerializeNullabe(bw, virtualFunctionTableSerializable == null))
			{
				this.SerializeVFTable(bw, virtualFunctionTableSerializable);
			}
			bw.Write(u.\u0001.BaseSignatureId);
			CommonSerializer.SerializeList<int>(bw, u.\u0001.InterfaceIds, new Action<BinaryWriter, int>(CommonSerializer.WriteInt));
			IEnumerable<KeyValuePair<string, string>> pairs = u.\u0001.Attributes.Select(new Func<string, KeyValuePair<string, string>>(u.\u0001));
			CommonSerializer.SerializeKeyValuePairs<string, string>(bw, pairs, new Action<BinaryWriter, string>(CommonSerializer.WriteString), new Action<BinaryWriter, string>(CommonSerializer.WriteString));
			bw.Write(u.\u0001.Size);
			bw.Write(u.\u0001.CalleeSize);
			bw.Write((int)u.\u0001.POUType);
			CommonSerializer.SerializeList<IVariableSerializable>(bw, u.\u0001.AllVariables.Cast<IVariableSerializable>(), new Action<BinaryWriter, IVariableSerializable>(this.SerializeVariable));
			LList<ISignature> source = CompileContextSerializer.Sort(u.\u0001.SubSignatures);
			CommonSerializer.SerializeList<ISignatureSerializable2>(bw, source.Cast<ISignatureSerializable2>(), new Action<BinaryWriter, ISignatureSerializable2>(this.SerializeSignature));
			_ISignature4 isignature = u.\u0001 as _ISignature4;
			if (isignature != null)
			{
				List<string> list = isignature.GetOverloadedNames().ToList<string>();
				CommonSerializer.SerializeList<string>(bw, list, new Action<BinaryWriter, string>(CommonSerializer.WriteString));
				foreach (string stName in list)
				{
					IList<_ISignature> overloadedSignatures = isignature.GetOverloadedSignatures(stName);
					CommonSerializer.SerializeList<ISignatureSerializable2>(bw, overloadedSignatures.Cast<ISignatureSerializable2>(), new Action<BinaryWriter, ISignatureSerializable2>(this.SerializeSignature));
				}
			}
			bw.Write(u.\u0001.Checksum);
			bw.Write(u.\u0001.ChecksumNoInit);
			LList<int> list2 = CompileContextSerializer.Sort(u.\u0001.DeclarerIds);
			CommonSerializer.SerializeList<int>(bw, list2, new Action<BinaryWriter, int>(CommonSerializer.WriteInt));
			LList<int> list3 = CompileContextSerializer.Sort(u.\u0001.ReferencerIds);
			CommonSerializer.SerializeList<int>(bw, list3, new Action<BinaryWriter, int>(CommonSerializer.WriteInt));
			CommonSerializer.SerializeList<int>(bw, u.\u0001.CallerIds, new Action<BinaryWriter, int>(CommonSerializer.WriteInt));
			if (!CommonSerializer.SerializeNullabe(bw, u.\u0001.CalleeIdList == null))
			{
				CommonSerializer.SerializeList<uint>(bw, u.\u0001.CalleeIdList, new Action<BinaryWriter, uint>(CommonSerializer.WriteUInt));
			}
			bw.Write(u.\u0001.HighestUsedOffset);
			bw.Write(u.\u0001.ChecksumOptionalInputs);
		}

		// Token: 0x06000BC3 RID: 3011 RVA: 0x0001B734 File Offset: 0x00019934
		public void SerializeVariable(BinaryWriter bw, IVariableSerializable variable)
		{
			CompileContextSerializer.\u0002 u = new CompileContextSerializer.\u0002();
			u.\u0001 = variable;
			bw.Write(u.\u0001.OrgName);
			_IType u2 = u.\u0001.OriginalType as _IType;
			\u0082.\u0001.Instance.\u0001(bw, u2);
			bw.Write((long)u.\u0001.Flags);
			bw.Write(u.\u0001.PositionToSave);
			bw.Write(u.\u0001.Id);
			CommonSerializer.SerializeList<ICrossReference>(bw, u.\u0001.CrossReferences, new Action<BinaryWriter, ICrossReference>(this.SerializeCrossRef));
			IVariableWithModifyingAccesses variableWithModifyingAccesses = u.\u0001 as IVariableWithModifyingAccesses;
			int[] list;
			if (variableWithModifyingAccesses != null)
			{
				list = variableWithModifyingAccesses.GetModifyingCrossReferences();
			}
			else
			{
				list = new int[0];
			}
			CommonSerializer.SerializeList<int>(bw, list, new Action<BinaryWriter, int>(CommonSerializer.WriteInt));
			this.SerializeDataLocation(bw, u.\u0001.DataLocation as _IDataLocation);
			IEnumerable<KeyValuePair<string, string>> pairs = u.\u0001.Attributes.Select(new Func<string, KeyValuePair<string, string>>(u.\u0001));
			CommonSerializer.SerializeKeyValuePairs<string, string>(bw, pairs, new Action<BinaryWriter, string>(CommonSerializer.WriteString), new Action<BinaryWriter, string>(CommonSerializer.WriteString));
			CompileContextSerializer.\u0001(bw, u.\u0001._Initial);
			CommonSerializer.SerializeDirectVariable_Nullable(bw, u.\u0001.Address);
			if (!CommonSerializer.SerializeNullabe(bw, u.\u0001.InputAssignments == null))
			{
				CommonSerializer.SerializeList<_IExpression>(bw, u.\u0001.InputAssignments.Cast<_IExpression>(), new Action<BinaryWriter, _IExpression>(CompileContextSerializer.\u0001));
			}
		}

		// Token: 0x06000BC4 RID: 3012 RVA: 0x0001B8B0 File Offset: 0x00019AB0
		public void SerializeCrossRef(BinaryWriter bw, ICrossReference crossRef)
		{
			bw.Write(crossRef.CodeId);
		}

		// Token: 0x06000BC5 RID: 3013 RVA: 0x0001B8C0 File Offset: 0x00019AC0
		public void SerializeVFTable(BinaryWriter bw, IVirtualFunctionTableSerializable vfTable)
		{
			CommonSerializer.SerializeList<IVFTableEntry2>(bw, vfTable._Entries.Select(new Func<IVFTableEntry, IVFTableEntry2>(CompileContextSerializer.<>c.<>9.\u0001)), new Action<BinaryWriter, IVFTableEntry2>(this.SerializeVFTableEntry));
			this.SerializeDataLocation(bw, vfTable.DataLocation as _IDataLocation);
			CommonSerializer.SerializeKeyValuePairs<int, int>(bw, vfTable.InterfaceIdToOffset, new Action<BinaryWriter, int>(CommonSerializer.WriteInt), new Action<BinaryWriter, int>(CommonSerializer.WriteInt));
			CommonSerializer.SerializeKeyValuePairs<int, int>(bw, vfTable.OffsetToInterfaceId, new Action<BinaryWriter, int>(CommonSerializer.WriteInt), new Action<BinaryWriter, int>(CommonSerializer.WriteInt));
			bw.Write(vfTable.PointerSize);
		}

		// Token: 0x06000BC6 RID: 3014 RVA: 0x0001B970 File Offset: 0x00019B70
		public void SerializeDataLocation(BinaryWriter bw, _IDataLocation dataLocation)
		{
			byte b;
			if (dataLocation == null)
			{
				b = 0;
			}
			else if (dataLocation.IsRelativ)
			{
				if (dataLocation.IsBitLocation)
				{
					b = 1;
				}
				else
				{
					b = 2;
				}
			}
			else if (dataLocation.IsBitLocation)
			{
				b = 3;
			}
			else
			{
				b = 4;
			}
			bw.Write(b);
			if (b == 0)
			{
				return;
			}
			if (dataLocation.IsRelativ)
			{
				_IRelativeDataLocation irelativeDataLocation = dataLocation as _IRelativeDataLocation;
				bw.Write(dataLocation.Offset);
				bw.Write((int)irelativeDataLocation.Flags);
			}
			else
			{
				bw.Write(dataLocation.Area);
				bw.Write(dataLocation.Offset);
			}
			if (dataLocation.IsBitLocation)
			{
				bw.Write(dataLocation.BitNr);
			}
		}

		// Token: 0x06000BC7 RID: 3015 RVA: 0x0001BA0C File Offset: 0x00019C0C
		public void SerializeVFTableEntry(BinaryWriter bw, IVFTableEntry2 entry)
		{
			if (entry.IsFunctionPointerEntry)
			{
				bw.Write(0);
				this.SerializeFunctionPointerEntry(bw, entry as _IFunctionPointerEntry);
				return;
			}
			bw.Write(1);
			this.SerializeInterfaceOffsetEntry(bw, entry as _IInterfaceOffsetEntry);
		}

		// Token: 0x06000BC8 RID: 3016 RVA: 0x0001BA40 File Offset: 0x00019C40
		public void SerializeFunctionPointerEntry(BinaryWriter bw, _IFunctionPointerEntry entry)
		{
			bw.Write(entry.Name);
			bw.Write(entry.Id);
		}

		// Token: 0x06000BC9 RID: 3017 RVA: 0x0001BA5C File Offset: 0x00019C5C
		public void SerializeInterfaceOffsetEntry(BinaryWriter bw, _IInterfaceOffsetEntry entry)
		{
			bw.Write(entry.Name);
			bw.Write(entry.Id);
			bw.Write(entry.CPP);
			bw.Write(entry.HierarchyOffset);
			bw.Write(entry.InstancePointerOffset);
		}

		// Token: 0x06000BCA RID: 3018 RVA: 0x0001BA9C File Offset: 0x00019C9C
		public void SerializeDataManager(BinaryWriter bw, IDataManagerSerializable dataManager)
		{
			CommonSerializer.SerializeList<_IDataSegment>(bw, dataManager._DataSegments, new Action<BinaryWriter, _IDataSegment>(this.SerializeDataSegment));
			CommonSerializer.SerializeList<_IArea>(bw, dataManager._Areas.Cast<_IArea>(), new Action<BinaryWriter, _IArea>(this.SerializeArea));
			bw.Write(dataManager.FirstArea);
		}

		// Token: 0x06000BCB RID: 3019 RVA: 0x0001BAEC File Offset: 0x00019CEC
		public void SerializeArea(BinaryWriter bw, _IArea area)
		{
			bw.Write((ushort)area.Flags);
			bw.Write((uint)area.AreaFlags);
			bw.Write(area.StartAddress);
			bw.Write(area.Size);
			bw.Write(area.Index);
			bw.Write(area.MinimalAreaSize);
			bw.Write(area.AllocationPlusInPercent);
			bw.Write(area.Checksum);
			bw.Write(area.AvailableSize);
			bw.Write(area.MaximalAreaSize);
		}

		// Token: 0x06000BCC RID: 3020 RVA: 0x0001BB74 File Offset: 0x00019D74
		public void SerializeDataSegment(BinaryWriter bw, _IDataSegment dataSegment)
		{
			bw.Write(dataSegment.Area);
			bw.Write(dataSegment.Address);
			bw.Write(dataSegment.DPTableOffset);
			bw.Write(dataSegment.Size);
			bw.Write((ushort)dataSegment.Flags);
			bw.Write(dataSegment.LogByFlag);
			CommonSerializer.SerializeMemoryManager(bw, (IMemoryManagerSerializable)dataSegment.MemMan);
			IDictionary<DataSegmentFlags, _IMemoryManager> memMansToFlags = dataSegment.MemMansToFlags;
			if (!CommonSerializer.SerializeNullabe(bw, memMansToFlags == null))
			{
				CommonSerializer.SerializeKeyValuePairs<DataSegmentFlags, _IMemoryManager>(bw, memMansToFlags, new Action<BinaryWriter, DataSegmentFlags>(CommonSerializer.SerializeDataSegmentFlags), new Action<BinaryWriter, _IMemoryManager>(CommonSerializer.SerializeMemoryManager));
			}
		}

		// Token: 0x02000089 RID: 137
		[CompilerGenerated]
		private sealed class \u0001
		{
			// Token: 0x06000BD7 RID: 3031 RVA: 0x0001BCE0 File Offset: 0x00019EE0
			internal KeyValuePair<string, string> \u0001(string \u0002)
			{
				return new KeyValuePair<string, string>(\u0002, this.\u0001.GetAttributeValue(\u0002));
			}

			// Token: 0x04000168 RID: 360
			public ISignatureSerializable2 \u0001;
		}

		// Token: 0x0200008A RID: 138
		[CompilerGenerated]
		private sealed class \u0002
		{
			// Token: 0x06000BD9 RID: 3033 RVA: 0x0001BCFC File Offset: 0x00019EFC
			internal KeyValuePair<string, string> \u0001(string \u0002)
			{
				return new KeyValuePair<string, string>(\u0002, this.\u0001.GetAttributeValue(\u0002));
			}

			// Token: 0x04000169 RID: 361
			public IVariableSerializable \u0001;
		}
	}
}
