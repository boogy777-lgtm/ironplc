using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using \u0010;
using \u0019;
using \u001B;
using \u001E;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.Serialization
{
	// Token: 0x02000085 RID: 133
	internal sealed class CompileContextDeSerializer
	{
		// Token: 0x06000B89 RID: 2953 RVA: 0x00019410 File Offset: 0x00017610
		public CompileContextDeSerializer(BinaryReader br, ITreeFactory redFactory, ILMSerializableTypeFactory2 lmb)
		{
			this.\u0001 = br;
			this.\u0001 = lmb;
			this.\u0001 = new \u001E.\u0003(br, redFactory);
			this.\u0001 = new \u001B.\u0001();
			this.\u0001 = new \u0019.\u0002(br, this.\u0001, this.\u0001);
		}

		// Token: 0x06000B8A RID: 2954 RVA: 0x00019464 File Offset: 0x00017664
		public _ICompileContext2 \u0001()
		{
			KindOfContext kindof = (KindOfContext)this.\u0001.ReadInt32();
			Guid objectGuid = this.\u0001.\u0001(this.\u0001);
			ICompileContextSerializable compileContextSerializable = (ICompileContextSerializable)this.\u0001.CreateCompileContext(kindof, null, null, objectGuid);
			compileContextSerializable.SerializableSignatureIdManager = this.\u0001.ReadInt32();
			compileContextSerializable.SerializableLibraryIdManager = this.\u0001.ReadInt32();
			compileContextSerializable.SupportDynamicMemory = this.\u0001.ReadBoolean();
			compileContextSerializable.GenerateContent = this.\u0001.ReadBoolean();
			compileContextSerializable.TimeStampContext = this.\u0001.ReadInt64();
			compileContextSerializable.TimeStampPool = this.\u0001.ReadInt64();
			compileContextSerializable.ContainsOnlineChangeCode = this.\u0001.ReadBoolean();
			compileContextSerializable.CodeId = this.\u0001.\u0001(this.\u0001);
			compileContextSerializable.DataId = this.\u0001.\u0001(this.\u0001);
			compileContextSerializable.LastCodeId = this.\u0001.\u0001(this.\u0001);
			compileContextSerializable.LastDataId = this.\u0001.\u0001(this.\u0001);
			compileContextSerializable.CheckSumCode = this.\u0001.ReadUInt32();
			compileContextSerializable.CheckSumData = this.\u0001.ReadUInt32();
			compileContextSerializable.CheckSumCodeLast = this.\u0001.ReadUInt32();
			compileContextSerializable.CheckSumDataLast = this.\u0001.ReadUInt32();
			compileContextSerializable.SetSignatures(\u001B.\u0001.\u0001<_ISignature>(this.\u0001, new Func<BinaryReader, _ISignature>(this.\u0001)));
			compileContextSerializable.SetGlobalSignatures(\u001B.\u0001.\u0001<_ISignature>(this.\u0001, new Func<BinaryReader, _ISignature>(this.\u0001)));
			compileContextSerializable.SetCompiledPOUs(\u001B.\u0001.\u0001<_ICompiledPOU2>(this.\u0001, new Func<BinaryReader, _ICompiledPOU2>(this.\u0001)));
			this.\u0002(this.\u0001, compileContextSerializable);
			compileContextSerializable.MemorySettingsChecksum = this.\u0001.ReadUInt32();
			if (!\u001B.\u0001.\u0001(this.\u0001))
			{
				compileContextSerializable.StaticMemorySegments = \u001B.\u0001.\u0001<IStaticMemorySegment>(this.\u0001, this.\u0001, new Func<BinaryReader, _ILanguageModelBuilder2, IStaticMemorySegment>(this.\u0001.\u0001));
			}
			compileContextSerializable.DataManager = this.\u0001(this.\u0001);
			foreach (KeyValuePair<string, string> keyValuePair in \u001B.\u0001.\u0001(this.\u0001))
			{
				compileContextSerializable.Define(keyValuePair.Key, keyValuePair.Value, false);
			}
			foreach (KeyValuePair<string, string> keyValuePair2 in \u001B.\u0001.\u0001(this.\u0001))
			{
				compileContextSerializable.Define(keyValuePair2.Key, keyValuePair2.Value, true);
			}
			compileContextSerializable.bFastOnlineChange = this.\u0001.ReadBoolean();
			if (!\u001B.\u0001.\u0001(this.\u0001))
			{
				compileContextSerializable.CompileOptionsSerializable = this.\u0001(this.\u0001);
			}
			if (!\u001B.\u0001.\u0001(this.\u0001))
			{
				ILibraryTableSerializable libraryTableSerializable = (ILibraryTableSerializable)this.\u0001.CreateLibraryTable();
				libraryTableSerializable.LibInfoSerializable = \u001B.\u0001.\u0001<ILibInfoSerializable>(this.\u0001, new Func<BinaryReader, ILibInfoSerializable>(this.\u0001));
				compileContextSerializable._LibraryTable = (_ILibraryTable)libraryTableSerializable;
			}
			compileContextSerializable.PrecompileContextNamesChecksum = this.\u0001.ReadUInt32();
			compileContextSerializable.ParameterTableChecksum = this.\u0001.ReadUInt32();
			compileContextSerializable.LibCheckSum = this.\u0001.ReadUInt32();
			compileContextSerializable.PoolLibCheckSum = this.\u0001.ReadUInt32();
			this.\u0001.\u0001(this.\u0001, compileContextSerializable.SlotPOUs);
			this.\u0001(this.\u0001, compileContextSerializable);
			compileContextSerializable.LoadWithoutTargetsettings = this.\u0001.ReadBoolean();
			compileContextSerializable.CodegeneratorGuid = this.\u0001.\u0001(this.\u0001);
			compileContextSerializable.SimulationMode = this.\u0001.ReadBoolean();
			compileContextSerializable.ContainsCode = this.\u0001.ReadBoolean();
			compileContextSerializable.DeviceId = this.\u0001(this.\u0001);
			IList<_IImplicitReferenceVariable> implicitReferenceVariables = \u001B.\u0001.\u0001<_IImplicitReferenceVariable>(this.\u0001, new Func<BinaryReader, _IImplicitReferenceVariable>(this.\u0001));
			compileContextSerializable.ImplicitReferenceVariables = implicitReferenceVariables;
			compileContextSerializable.ProjectChecksum = this.\u0001.ReadUInt32();
			compileContextSerializable.AfterDeserialize();
			return (_ICompileContext2)compileContextSerializable;
		}

		// Token: 0x06000B8B RID: 2955 RVA: 0x000198A0 File Offset: 0x00017AA0
		public _IImplicitReferenceVariable \u0001(BinaryReader \u0002)
		{
			_IVariable variable = this.\u0001(\u0002);
			int nSignatureId = \u0002.ReadInt32();
			return this.\u0001.CreateImplitReferenceVariable(nSignatureId, variable);
		}

		// Token: 0x06000B8C RID: 2956 RVA: 0x000198CC File Offset: 0x00017ACC
		public IDeviceIdentification \u0001(BinaryReader \u0002)
		{
			int nType = \u0002.ReadInt32();
			string stId = \u0002.ReadString();
			string stVersion = \u0002.ReadString();
			return this.\u0001.CreateDeviceIdentification(nType, stId, stVersion);
		}

		// Token: 0x06000B8D RID: 2957 RVA: 0x000198FC File Offset: 0x00017AFC
		public void \u0001(BinaryReader \u0002, ICompileContextSerializable \u0003)
		{
			int num = \u0002.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				IDirectVariable dirvar = \u001B.\u0001.\u0001(\u0002, this.\u0001);
				foreach (IAddressCrossReference addressCrossReference in \u001B.\u0001.\u0001<IAddressCrossReference>(\u0002, new Func<BinaryReader, IAddressCrossReference>(this.\u0001)))
				{
					foreach (IAddressCodePosition codepos in addressCrossReference.Positions)
					{
						\u0003.AddAddressCrossReference(dirvar, addressCrossReference.CodeId, codepos);
					}
				}
			}
		}

		// Token: 0x06000B8E RID: 2958 RVA: 0x000199A8 File Offset: 0x00017BA8
		public IAddressCrossReference \u0001(BinaryReader \u0002)
		{
			int nCodeId = \u0002.ReadInt32();
			IAddressCrossReferenceSerializable addressCrossReferenceSerializable = this.\u0001.CreateAddressCrossReference(nCodeId);
			foreach (IAddressCodePositionSerializable addressCodePositionSerializable in \u001B.\u0001.\u0001<IAddressCodePositionSerializable>(\u0002, new Func<BinaryReader, IAddressCodePositionSerializable>(this.\u0001)))
			{
				addressCrossReferenceSerializable.AddPosition((IAddressCodePosition)addressCodePositionSerializable);
			}
			return addressCrossReferenceSerializable;
		}

		// Token: 0x06000B8F RID: 2959 RVA: 0x00019A1C File Offset: 0x00017C1C
		public IAddressCodePositionSerializable \u0001(BinaryReader \u0002)
		{
			IAddressCodePositionSerializable addressCodePositionSerializable = this.\u0001.CreateAddressCodePosition();
			addressCodePositionSerializable.Access = (AccessFlag)\u0002.ReadByte();
			addressCodePositionSerializable.TypeSize = \u0002.ReadInt32();
			addressCodePositionSerializable.PositionToSave = \u0002.ReadInt64();
			return addressCodePositionSerializable;
		}

		// Token: 0x06000B90 RID: 2960 RVA: 0x00019A50 File Offset: 0x00017C50
		public ILibInfoSerializable \u0001(BinaryReader \u0002)
		{
			ILibInfoSerializable libInfoSerializable = this.\u0001.CreateLibInfo();
			libInfoSerializable.LibraryId = \u0002.ReadString();
			libInfoSerializable.Namespace = \u0002.ReadString();
			libInfoSerializable.ReferencingLibrary = \u0002.ReadString();
			libInfoSerializable.OutOfPool = \u0002.ReadBoolean();
			libInfoSerializable.Id = \u0002.ReadInt32();
			libInfoSerializable.QualifiedOnly = \u0002.ReadBoolean();
			libInfoSerializable.PublishSymbols = \u0002.ReadBoolean();
			return libInfoSerializable;
		}

		// Token: 0x06000B91 RID: 2961 RVA: 0x00019ABC File Offset: 0x00017CBC
		public ICompileOptionsSerializable \u0001(BinaryReader \u0002)
		{
			ICompileOptionsSerializable compileOptionsSerializable = this.\u0001.CreateCompileOptions();
			compileOptionsSerializable.LoggingInBreakpointsToSave = \u0002.ReadBoolean();
			compileOptionsSerializable.ReplaceConstantsToSave = \u0002.ReadBoolean();
			compileOptionsSerializable.UnicodeToSave = \u0002.ReadBoolean();
			compileOptionsSerializable.CompilerVersionToSave = \u001B.\u0001.\u0001<int>(\u0002, new Func<BinaryReader, int>(\u001B.\u0001.\u0001)).ToArray<int>();
			return compileOptionsSerializable;
		}

		// Token: 0x06000B92 RID: 2962 RVA: 0x00019B18 File Offset: 0x00017D18
		public _IDataManager \u0001(BinaryReader \u0002)
		{
			IDataManagerSerializable dataManagerSerializable = (IDataManagerSerializable)this.\u0001.CreateDataManager();
			dataManagerSerializable._DataSegments = \u001B.\u0001.\u0001<_IDataSegment>(\u0002, new Func<BinaryReader, _IDataSegment>(this.\u0001));
			dataManagerSerializable._Areas = \u001B.\u0001.\u0001<IArea>(\u0002, new Func<BinaryReader, IArea>(this.\u0001));
			dataManagerSerializable.FirstArea = \u0002.ReadInt32();
			dataManagerSerializable.AfterDeserialize();
			return (_IDataManager)dataManagerSerializable;
		}

		// Token: 0x06000B93 RID: 2963 RVA: 0x00019B7C File Offset: 0x00017D7C
		public _IArea \u0001(BinaryReader \u0002)
		{
			DataSegmentFlags dsFlag = (DataSegmentFlags)\u0002.ReadUInt16();
			AreaFlags aflag = (AreaFlags)\u0002.ReadUInt32();
			_IArea iarea = this.\u0001.CreateArea();
			iarea.SetAreaFlag(aflag, true);
			iarea.SetDataSegmentFlag(dsFlag, true);
			iarea.StartAddress = \u0002.ReadInt32();
			iarea.Size = \u0002.ReadInt32();
			iarea.Index = \u0002.ReadInt32();
			iarea.MinimalAreaSize = \u0002.ReadInt32();
			iarea.AllocationPlusInPercent = \u0002.ReadInt32();
			iarea.Checksum = \u0002.ReadUInt32();
			iarea.AvailableSize = \u0002.ReadInt32();
			iarea.MaximalAreaSize = \u0002.ReadInt32();
			return iarea;
		}

		// Token: 0x06000B94 RID: 2964 RVA: 0x00019C14 File Offset: 0x00017E14
		public _IDataSegment \u0001(BinaryReader \u0002)
		{
			ushort usArea = \u0002.ReadUInt16();
			int nAddress = \u0002.ReadInt32();
			int dptableOffset = \u0002.ReadInt32();
			int nSize = \u0002.ReadInt32();
			DataSegmentFlags flags = (DataSegmentFlags)\u0002.ReadUInt16();
			bool logByFlag = \u0002.ReadBoolean();
			_IDataSegment idataSegment = this.\u0001.CreateDataSegment(usArea, nAddress, nSize, flags);
			idataSegment.DPTableOffset = dptableOffset;
			idataSegment.LogByFlag = logByFlag;
			idataSegment.MemMan = \u001B.\u0001.\u0001(\u0002, this.\u0001);
			if (!\u001B.\u0001.\u0001(\u0002))
			{
				\u001B.\u0001.\u0001<DataSegmentFlags, _IMemoryManager>(\u0002, this.\u0001, new Func<BinaryReader, ILMSerializableTypeFactory, DataSegmentFlags>(\u001B.\u0001.\u0001), new Func<BinaryReader, ILMSerializableTypeFactory, _IMemoryManager>(\u001B.\u0001.\u0001));
			}
			return idataSegment;
		}

		// Token: 0x06000B95 RID: 2965 RVA: 0x00019CAC File Offset: 0x00017EAC
		public void \u0002(BinaryReader \u0002, ICompileContextSerializable \u0003)
		{
			int num = \u0002.ReadInt32();
			_ITaskList itaskList = this.\u0001.CreateTaskList();
			for (int i = 0; i < num; i++)
			{
				\u001B.\u0001.\u0001 u = this.\u0001.\u0001(\u0002);
				itaskList.AddTaskInfo(u.\u0002, u.\u0001, u.\u0001, u.\u0002);
			}
			\u0003.TaskList = itaskList;
		}

		// Token: 0x06000B96 RID: 2966 RVA: 0x00019D0C File Offset: 0x00017F0C
		public _ICompiledPOU2 \u0001(BinaryReader \u0002)
		{
			string stName = \u0002.ReadString();
			ICompiledPOUSerializable compiledPOUSerializable = (ICompiledPOUSerializable)this.\u0001.CreateCompiledPOU(stName);
			compiledPOUSerializable.ObjectGuid = this.\u0001.\u0001(\u0002);
			compiledPOUSerializable.MessageGuid = this.\u0001.\u0001(\u0002);
			compiledPOUSerializable.ParentObjectGuid = this.\u0001.\u0001(\u0002);
			compiledPOUSerializable.SignatureId = \u0002.ReadInt32();
			compiledPOUSerializable.Flags = (CompiledPOUFlags)\u0002.ReadInt32();
			if (!\u001B.\u0001.\u0001(\u0002))
			{
				compiledPOUSerializable.CompiledCode = this.\u0001(\u0002);
			}
			compiledPOUSerializable.ScratchSize = \u0002.ReadInt32();
			compiledPOUSerializable.TimeStamp = \u0002.ReadInt64();
			compiledPOUSerializable.Checksum = \u0002.ReadUInt32();
			compiledPOUSerializable.LibraryPath = \u0002.ReadString();
			compiledPOUSerializable.MaxParamSize = \u0002.ReadInt32();
			if (!\u001B.\u0001.\u0001(\u0002))
			{
				compiledPOUSerializable.SetBreakpointList(this.\u0001(\u0002));
			}
			if (!\u001B.\u0001.\u0001(\u0002))
			{
				compiledPOUSerializable.SetMessages(\u001B.\u0001.\u0001<_ICompilerMessage>(\u0002, this.\u0001, new Func<BinaryReader, _ILanguageModelBuilder2, _ICompilerMessage>(this.\u0001.\u0001)));
			}
			compiledPOUSerializable.CodeGeneratorStackSize = \u0002.ReadInt32();
			if (!\u001B.\u0001.\u0001(\u0002))
			{
				compiledPOUSerializable.BitWriteAccesses = \u001B.\u0001.\u0001<IBitWriteAccess>(\u0002, new Func<BinaryReader, IBitWriteAccess>(this.\u0001));
			}
			if (!\u001B.\u0001.\u0001(\u0002))
			{
				compiledPOUSerializable.TryCatchFPAddresses = \u001B.\u0001.\u0001<IDataLocation>(\u0002, new Func<BinaryReader, IDataLocation>(this.\u0001));
			}
			if (!\u001B.\u0001.\u0001(\u0002))
			{
				compiledPOUSerializable.TryCatchCodeAddresses = \u001B.\u0001.\u0001<int>(\u0002, new Func<BinaryReader, int>(\u001B.\u0001.\u0001));
			}
			compiledPOUSerializable.InternalFlags = (InternalCompiledPOUFlags)\u0002.ReadInt32();
			if (!\u001B.\u0001.\u0001(\u0002))
			{
				compiledPOUSerializable.PrecompileMessages = \u001B.\u0001.\u0001<_ICompilerMessage>(\u0002, this.\u0001, new Func<BinaryReader, _ILanguageModelBuilder2, _ICompilerMessage>(this.\u0001.\u0001)).ToArray<_ICompilerMessage>();
			}
			compiledPOUSerializable.AfterDeserialize();
			return (_ICompiledPOU2)compiledPOUSerializable;
		}

		// Token: 0x06000B97 RID: 2967 RVA: 0x00019EC8 File Offset: 0x000180C8
		private ICompiledCode \u0001(BinaryReader \u0002)
		{
			global::\u0010.\u0001 u = (global::\u0010.\u0001)\u0002.ReadByte();
			if (u == global::\u0010.\u0001.\u0004)
			{
				return \u001B.\u0001.\u0001<ICompiledCode>(\u0002);
			}
			return this.\u0001(\u0002, u);
		}

		// Token: 0x06000B98 RID: 2968 RVA: 0x00019EF0 File Offset: 0x000180F0
		private ICompiledCode \u0001(BinaryReader \u0002, global::\u0010.\u0001 \u0003)
		{
			ICompiledCodeSerializable compiledCodeSerializable;
			if (\u0003 == global::\u0010.\u0001.\u0003)
			{
				compiledCodeSerializable = this.\u0001.CreateCompiledCodeDataReloc();
				((ICompiledCodeDataRelocSerializable)compiledCodeSerializable).MotorolaByteOrder = \u0002.ReadBoolean();
			}
			else if (\u0003 == global::\u0010.\u0001.\u0002)
			{
				compiledCodeSerializable = this.\u0001.CreateCompiledCodeData();
				((ICompiledCodeDataSerializable)compiledCodeSerializable).RelatedId = \u0002.ReadInt32();
			}
			else
			{
				compiledCodeSerializable = this.\u0001.CreateCompiledCodeDataStub();
			}
			compiledCodeSerializable.Flags = (CompiledCodeFlags)\u0002.ReadUInt16();
			compiledCodeSerializable.Location = this.\u0001(\u0002);
			compiledCodeSerializable.RelocationList = \u001B.\u0001.\u0001<IRelocationList>(\u0002);
			int count = \u0002.ReadInt32();
			ChunkedMemoryStream codeBytes = new ChunkedMemoryStream(\u0002.ReadBytes(count), false);
			compiledCodeSerializable.CodeBytes = codeBytes;
			return (ICompiledCode)compiledCodeSerializable;
		}

		// Token: 0x06000B99 RID: 2969 RVA: 0x00019F98 File Offset: 0x00018198
		public _IBreakpointList \u0001(BinaryReader \u0002)
		{
			IBreakpointListSerializable breakpointListSerializable = this.\u0001.CreateBreakpointList();
			breakpointListSerializable.FirstIndex = \u0002.ReadInt32();
			IList<_IBreakpoint> list = \u001B.\u0001.\u0001<_IBreakpoint>(\u0002, new Func<BinaryReader, _IBreakpoint>(this.\u0001));
			for (int i = 0; i < list.Count; i++)
			{
				_IBreakpoint ibreakpoint = list[i];
				breakpointListSerializable.Add(ref ibreakpoint);
			}
			breakpointListSerializable.AfterDeserialize();
			return (_IBreakpointList)breakpointListSerializable;
		}

		// Token: 0x06000B9A RID: 2970 RVA: 0x0001A000 File Offset: 0x00018200
		public IArchivable \u0001(BinaryReader \u0002)
		{
			IArchiveReader archiveReader = APEnvironmentFacade.Instance.CreateNewBinaryArchiveReader();
			archiveReader.Initialize(\u0002.BaseStream);
			return archiveReader.Load();
		}

		// Token: 0x06000B9B RID: 2971 RVA: 0x0001A020 File Offset: 0x00018220
		public IBitWriteAccess \u0001(BinaryReader \u0002)
		{
			int nSignatureId = \u0002.ReadInt32();
			int nArea = \u0002.ReadInt32();
			int nOffset = \u0002.ReadInt32();
			byte byBitNr = \u0002.ReadByte();
			string stSymbol = \u0002.ReadString();
			long positionCombination = \u0002.ReadInt64();
			IBitWriteAccessSerializable bitWriteAccessSerializable = (IBitWriteAccessSerializable)this.\u0001.CreateBitWriteAccess(nSignatureId, nArea, nOffset, byBitNr, null, stSymbol);
			bitWriteAccessSerializable.PositionCombination = positionCombination;
			return (IBitWriteAccess)bitWriteAccessSerializable;
		}

		// Token: 0x06000B9C RID: 2972 RVA: 0x0001A080 File Offset: 0x00018280
		public _IBreakpoint \u0001(BinaryReader \u0002)
		{
			IBreakpointSerializable breakpointSerializable = null;
			byte b = \u0002.ReadByte();
			if (b == 0)
			{
				breakpointSerializable = this.\u0001.CreateBreakpoint();
			}
			else if (b == 1)
			{
				breakpointSerializable = this.\u0001.CreateTryCatchBreakpoint();
				breakpointSerializable.TryCatchId = \u0002.ReadInt16();
			}
			breakpointSerializable.Len = \u0002.ReadInt16();
			breakpointSerializable.Offset = \u0002.ReadInt32();
			if (!\u001B.\u0001.\u0001(\u0002))
			{
				breakpointSerializable.Successors = \u001B.\u0001.\u0001<int>(\u0002, new Func<BinaryReader, int>(\u001B.\u0001.\u0001)).ToArray<int>();
			}
			if (!\u001B.\u0001.\u0001(\u0002))
			{
				breakpointSerializable.StepInSuccessors = \u001B.\u0001.\u0001<IStepInPosition>(\u0002, new Func<BinaryReader, IStepInPosition>(this.\u0001)).ToArray<IStepInPosition>();
			}
			if (!\u001B.\u0001.\u0001(\u0002))
			{
				breakpointSerializable.AssemblySuccessors = \u001B.\u0001.\u0001<int>(\u0002, new Func<BinaryReader, int>(\u001B.\u0001.\u0001)).ToArray<int>();
			}
			breakpointSerializable.AreaGPRegister = \u0002.ReadInt32();
			breakpointSerializable.OffsetGPRegister = \u0002.ReadInt32();
			breakpointSerializable.PositionCombination = \u0002.ReadInt64();
			breakpointSerializable.ExceptionHandlingSuccessor = \u0002.ReadInt32();
			return (_IBreakpoint)breakpointSerializable;
		}

		// Token: 0x06000B9D RID: 2973 RVA: 0x0001A180 File Offset: 0x00018380
		public IStepInPosition \u0001(BinaryReader \u0002)
		{
			int num = \u0002.ReadInt32();
			KindOfCall kindOfCall = (KindOfCall)\u0002.ReadByte();
			IBreakpoint stepoutBreakpoint = null;
			if (!\u001B.\u0001.\u0001(\u0002))
			{
				stepoutBreakpoint = this.\u0001(\u0002);
			}
			IStepInPositionSerializable stepInPositionSerializable = this.\u0001.CreateStepInPosition(num, stepoutBreakpoint) as IStepInPositionSerializable;
			stepInPositionSerializable.KindOfCall = kindOfCall;
			stepInPositionSerializable.SignatureId = num;
			if (!\u001B.\u0001.\u0001(\u0002))
			{
				stepInPositionSerializable.StepInBreakpoint = this.\u0001(\u0002);
			}
			return (IStepInPosition)stepInPositionSerializable;
		}

		// Token: 0x06000B9E RID: 2974 RVA: 0x0001A1EC File Offset: 0x000183EC
		public _ISignature \u0001(BinaryReader \u0002)
		{
			ISignatureSerializable2 signatureSerializable = (ISignatureSerializable2)this.\u0001.CreateSignature();
			signatureSerializable._NameExpression = this.\u0001.\u0002<_IExpression>(\u0002);
			signatureSerializable.TimeStamp = \u0002.ReadInt64();
			signatureSerializable.Flags = (SignatureFlag)\u0002.ReadInt64();
			signatureSerializable.InternalFlags = (SignatureFlagInternal)\u0002.ReadUInt64();
			signatureSerializable.Id = \u0002.ReadInt32();
			signatureSerializable.ParentSignatureId = \u0002.ReadInt32();
			signatureSerializable.ObjectGuid = this.\u0001.\u0001(\u0002);
			signatureSerializable.MessageGuid = this.\u0001.\u0001(\u0002);
			signatureSerializable.ParentObjectGuid = this.\u0001.\u0001(\u0002);
			signatureSerializable.LibraryPath = \u0002.ReadString();
			signatureSerializable.FPDataLocation = this.\u0001(\u0002);
			signatureSerializable.SerializableVariableIdManagement = \u0002.ReadInt32();
			signatureSerializable.TaskReferenceList = \u0002.ReadBytes(\u0002.ReadInt32());
			if (!\u001B.\u0001.\u0001(\u0002))
			{
				signatureSerializable._VirtualFunctionTable = this.\u0001(\u0002, (_ISignature2)signatureSerializable);
			}
			signatureSerializable.BaseSignatureId = \u0002.ReadInt32();
			signatureSerializable.InterfaceIds = \u001B.\u0001.\u0001<int>(\u0002, new Func<BinaryReader, int>(\u001B.\u0001.\u0001)).ToArray<int>();
			signatureSerializable.SetAttributes(\u001B.\u0001.\u0001<string, string>(\u0002, new Func<BinaryReader, string>(\u001B.\u0001.\u0001), new Func<BinaryReader, string>(\u001B.\u0001.\u0001)));
			signatureSerializable.Size = \u0002.ReadInt32();
			signatureSerializable.CalleeSize = \u0002.ReadInt32();
			signatureSerializable.POUType = (Operator)\u0002.ReadInt32();
			_ISignature3 isignature = signatureSerializable as _ISignature3;
			if (isignature != null)
			{
				isignature.SetAllVariablesWithoutSideEffects(\u001B.\u0001.\u0001<_IVariable>(\u0002, new Func<BinaryReader, _IVariable>(this.\u0001)));
			}
			else
			{
				signatureSerializable.AllVariables = \u001B.\u0001.\u0001<_IVariable>(\u0002, new Func<BinaryReader, _IVariable>(this.\u0001));
			}
			signatureSerializable.SetSubSignatures(\u001B.\u0001.\u0001<_ISignature>(\u0002, new Func<BinaryReader, _ISignature>(this.\u0001)));
			_ISignature4 isignature2 = signatureSerializable as _ISignature4;
			if (isignature2 != null)
			{
				foreach (string stName in \u001B.\u0001.\u0001<string>(\u0002, new Func<BinaryReader, string>(\u001B.\u0001.\u0001)))
				{
					IList<_ISignature> signatures = \u001B.\u0001.\u0001<_ISignature>(\u0002, new Func<BinaryReader, _ISignature>(this.\u0001));
					isignature2.SetOverloadedSignatures(stName, signatures);
				}
			}
			signatureSerializable.Checksum = \u0002.ReadUInt32();
			signatureSerializable.ChecksumNoInit = \u0002.ReadUInt32();
			signatureSerializable.DeclarerIds = \u001B.\u0001.\u0001<int>(\u0002, new Func<BinaryReader, int>(\u001B.\u0001.\u0001)).ToArray<int>();
			signatureSerializable.ReferencerIds = \u001B.\u0001.\u0001<int>(\u0002, new Func<BinaryReader, int>(\u001B.\u0001.\u0001)).ToArray<int>();
			signatureSerializable.CallerIds = \u001B.\u0001.\u0001<int>(\u0002, new Func<BinaryReader, int>(\u001B.\u0001.\u0001)).ToArray<int>();
			if (!\u001B.\u0001.\u0001(\u0002))
			{
				signatureSerializable.CalleeIdList = \u001B.\u0001.\u0001<uint>(\u0002, new Func<BinaryReader, uint>(\u001B.\u0001.\u0001));
			}
			signatureSerializable.HighestUsedOffset = \u0002.ReadInt32();
			signatureSerializable.ChecksumOptionalInputs = \u0002.ReadUInt32();
			return (_ISignature)signatureSerializable;
		}

		// Token: 0x06000B9F RID: 2975 RVA: 0x0001A4C0 File Offset: 0x000186C0
		public _IVariable \u0001(BinaryReader \u0002)
		{
			IVariableSerializable variableSerializable = (IVariableSerializable)this.\u0001.CreateVariable(null);
			variableSerializable.OrgName = \u0002.ReadString();
			variableSerializable.OriginalType = this.\u0001.\u0001();
			variableSerializable.Flags = (VarFlag)\u0002.ReadInt64();
			variableSerializable.PositionToSave = \u0002.ReadInt64();
			variableSerializable.Id = \u0002.ReadInt32();
			variableSerializable.SetCrossReferences(\u001B.\u0001.\u0001<ICrossReferenceSerializable>(\u0002, new Func<BinaryReader, ICrossReferenceSerializable>(this.\u0001)));
			IList<int> list = \u001B.\u0001.\u0001<int>(\u0002, new Func<BinaryReader, int>(\u001B.\u0001.\u0001));
			IVariableWithModifyingAccesses variableWithModifyingAccesses = variableSerializable as IVariableWithModifyingAccesses;
			if (variableWithModifyingAccesses != null)
			{
				foreach (int nCodeId in list)
				{
					variableWithModifyingAccesses.AddModifyingCrossReference(nCodeId);
				}
			}
			variableSerializable.DataLocation = this.\u0001(\u0002);
			variableSerializable.SetAttributes(\u001B.\u0001.\u0001<string, string>(\u0002, new Func<BinaryReader, string>(\u001B.\u0001.\u0001), new Func<BinaryReader, string>(\u001B.\u0001.\u0001)));
			variableSerializable._Initial = this.\u0001.\u0002<_IExpression>(\u0002);
			variableSerializable.Address = \u001B.\u0001.\u0001(\u0002, this.\u0001);
			if (!\u001B.\u0001.\u0001(\u0002))
			{
				IList<IAssignmentExpression> source = \u001B.\u0001.\u0002<IAssignmentExpression>(\u0002, new Func<BinaryReader, IAssignmentExpression>(this.\u0001.\u0002<IAssignmentExpression>));
				variableSerializable.InputAssignments = source.ToArray<IAssignmentExpression>();
			}
			return (_IVariable2)variableSerializable;
		}

		// Token: 0x06000BA0 RID: 2976 RVA: 0x0001A61C File Offset: 0x0001881C
		public ICrossReferenceSerializable \u0001(BinaryReader \u0002)
		{
			ICrossReferenceSerializable crossReferenceSerializable = this.\u0001.CreateCrossReference();
			crossReferenceSerializable.CodeId = \u0002.ReadInt32();
			return crossReferenceSerializable;
		}

		// Token: 0x06000BA1 RID: 2977 RVA: 0x0001A638 File Offset: 0x00018838
		public _IVirtualFunctionTable \u0001(BinaryReader \u0002, _ISignature2 \u0003)
		{
			IVirtualFunctionTableSerializable virtualFunctionTableSerializable = this.\u0001.CreateVirtualFunctionTable();
			virtualFunctionTableSerializable.Signature = \u0003;
			virtualFunctionTableSerializable._Entries = \u001B.\u0001.\u0001<IVFTableEntry>(\u0002, new Func<BinaryReader, IVFTableEntry>(this.\u0001));
			virtualFunctionTableSerializable.DataLocation = this.\u0001(\u0002);
			virtualFunctionTableSerializable.InterfaceIdToOffset = \u001B.\u0001.\u0001<int, int>(\u0002, new Func<BinaryReader, int>(\u001B.\u0001.\u0001), new Func<BinaryReader, int>(\u001B.\u0001.\u0001)).ToDictionary(new Func<KeyValuePair<int, int>, int>(CompileContextDeSerializer.<>c.<>9.\u0001), new Func<KeyValuePair<int, int>, int>(CompileContextDeSerializer.<>c.<>9.\u0002));
			virtualFunctionTableSerializable.OffsetToInterfaceId = \u001B.\u0001.\u0001<int, int>(\u0002, new Func<BinaryReader, int>(\u001B.\u0001.\u0001), new Func<BinaryReader, int>(\u001B.\u0001.\u0001)).ToDictionary(new Func<KeyValuePair<int, int>, int>(CompileContextDeSerializer.<>c.<>9.\u0003), new Func<KeyValuePair<int, int>, int>(CompileContextDeSerializer.<>c.<>9.\u0004));
			virtualFunctionTableSerializable.PointerSize = \u0002.ReadInt32();
			return (_IVirtualFunctionTable)virtualFunctionTableSerializable;
		}

		// Token: 0x06000BA2 RID: 2978 RVA: 0x0001A75C File Offset: 0x0001895C
		public _IDataLocation \u0001(BinaryReader \u0002)
		{
			byte b = \u0002.ReadByte();
			if (b == 0)
			{
				return null;
			}
			bool flag = b == 1 || b == 3;
			if (b == 1 || b == 2)
			{
				int iOffset = \u0002.ReadInt32();
				DataLocationFlag dataLocationFlag = (DataLocationFlag)\u0002.ReadInt32();
				if (flag)
				{
					byte byBitLocation = \u0002.ReadByte();
					_IRelativeDataLocation irelativeDataLocation = (_IRelativeDataLocation)this.\u0001.CreateRelativeBitDataLocation(iOffset, byBitLocation);
					irelativeDataLocation.Flags = dataLocationFlag;
					return (_IDataLocation)irelativeDataLocation;
				}
				return (_IDataLocation)this.\u0001.CreateRelativeDataLocation(iOffset, dataLocationFlag);
			}
			else
			{
				ushort usArea = \u0002.ReadUInt16();
				int iOffset2 = \u0002.ReadInt32();
				if (flag)
				{
					byte byBitLocation2 = \u0002.ReadByte();
					return (_IDataLocation)this.\u0001.CreateBitDataLocation(usArea, iOffset2, byBitLocation2);
				}
				return (_IDataLocation)this.\u0001.CreateDataLocation(usArea, iOffset2);
			}
		}

		// Token: 0x06000BA3 RID: 2979 RVA: 0x0001A820 File Offset: 0x00018A20
		public IVFTableEntry \u0001(BinaryReader \u0002)
		{
			byte b = \u0002.ReadByte();
			if (b == 0)
			{
				return this.\u0001(\u0002);
			}
			if (b == 1)
			{
				return this.\u0001(\u0002);
			}
			return null;
		}

		// Token: 0x06000BA4 RID: 2980 RVA: 0x0001A84C File Offset: 0x00018A4C
		public _IFunctionPointerEntry \u0001(BinaryReader \u0002)
		{
			_IFunctionPointerEntry ifunctionPointerEntry = this.\u0001.CreateVirtualFunctionTableFunctionPointerEntry();
			ifunctionPointerEntry.Name = \u0002.ReadString();
			ifunctionPointerEntry.Id = \u0002.ReadInt32();
			return ifunctionPointerEntry;
		}

		// Token: 0x06000BA5 RID: 2981 RVA: 0x0001A874 File Offset: 0x00018A74
		public _IInterfaceOffsetEntry \u0001(BinaryReader \u0002)
		{
			IInterfaceOffsetEntrySerializable interfaceOffsetEntrySerializable = this.\u0001.CreateVirtualFunctionTableInterfaceOffsetEntry();
			interfaceOffsetEntrySerializable.Name = \u0002.ReadString();
			interfaceOffsetEntrySerializable.Id = \u0002.ReadInt32();
			interfaceOffsetEntrySerializable.CPP = \u0002.ReadBoolean();
			interfaceOffsetEntrySerializable.HierarchyOffset = \u0002.ReadInt32();
			interfaceOffsetEntrySerializable.InstancePointerOffset = \u0002.ReadInt32();
			return (_IInterfaceOffsetEntry)interfaceOffsetEntrySerializable;
		}

		// Token: 0x04000157 RID: 343
		private readonly ILMSerializableTypeFactory2 \u0001;

		// Token: 0x04000158 RID: 344
		private readonly BinaryReader \u0001;

		// Token: 0x04000159 RID: 345
		private readonly \u001E.\u0003 \u0001;

		// Token: 0x0400015A RID: 346
		private readonly \u001B.\u0001 \u0001;

		// Token: 0x0400015B RID: 347
		private readonly \u0019.\u0002 \u0001;
	}
}
