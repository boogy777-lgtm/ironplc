using System;
using System.Collections.Generic;
using System.IO;
using \u0003;
using \u0008;
using \u001B;
using \u001E;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0016
{
	// Token: 0x02000094 RID: 148
	internal sealed class \u0001
	{
		// Token: 0x06000C5C RID: 3164 RVA: 0x0001DDCC File Offset: 0x0001BFCC
		public \u0001(BinaryReader \u009E\u0002, ITreeFactory \u009F\u0002, ITreeFactory \u0001\u0003, _ILanguageModelBuilder \u0002\u0003)
		{
			this.\u0001 = \u009E\u0002;
			this.\u0001 = \u009F\u0002;
			this.\u0001 = (_ILanguageModelBuilder2)\u0002\u0003;
			this.\u0001 = new \u001B.\u0001();
			this.\u0001 = new \u001E.\u0003(\u009E\u0002, \u0001\u0003);
			this.\u0001 = new global::\u0008.\u0003(\u009E\u0002, (ILMSerializableTypeFactory2)\u0002\u0003, new \u001E.\u0003(\u009E\u0002, \u0001\u0003));
		}

		// Token: 0x06000C5D RID: 3165 RVA: 0x0001DE2C File Offset: 0x0001C02C
		private IEnumerable<KeyValuePair<string, string>> \u0001()
		{
			int num = this.\u0001.ReadInt32();
			int num2 = this.\u0001.ReadInt32();
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>(num + num2);
			for (int i = 0; i < num; i++)
			{
				string key = this.\u0001.ReadString();
				list.Add(new KeyValuePair<string, string>(key, null));
			}
			for (int j = 0; j < num2; j++)
			{
				string key2 = this.\u0001.ReadString();
				string value = this.\u0001.ReadString();
				list.Add(new KeyValuePair<string, string>(key2, value));
			}
			return list;
		}

		// Token: 0x06000C5E RID: 3166 RVA: 0x0001DEBC File Offset: 0x0001C0BC
		private IEnumerable<KeyValuePair<string, string>> \u0002()
		{
			return this.\u0001();
		}

		// Token: 0x06000C5F RID: 3167 RVA: 0x0001DEC4 File Offset: 0x0001C0C4
		public _IVariable \u0001()
		{
			string u = this.\u0001.ReadString();
			_IType u2 = this.\u0001.\u0001();
			VarFlag u3 = (VarFlag)this.\u0001.ReadUInt64();
			long u4 = this.\u0001.ReadInt64();
			IEnumerable<KeyValuePair<string, string>> u5 = this.\u0001();
			IDirectVariable u6 = \u001B.\u0001.\u0001(this.\u0001, this.\u0001);
			IExpression u7 = this.\u0001.\u0001<IExpression>(this.\u0001);
			int num = this.\u0001.ReadInt32();
			_IAssignmentExpression[] array = (num > 0) ? new _IAssignmentExpression[num] : null;
			for (int i = 0; i < num; i++)
			{
				_IAssignmentExpression iassignmentExpression = this.\u0001.\u0001<_IAssignmentExpression>(this.\u0001);
				array[i] = iassignmentExpression;
			}
			return this.\u0001(u4, u, u2, u3, u5, u6, u7, array);
		}

		// Token: 0x06000C60 RID: 3168 RVA: 0x0001DF8C File Offset: 0x0001C18C
		private _IVariable \u0001(long \u0002, string \u0003, _IType \u0004, VarFlag \u0005, IEnumerable<KeyValuePair<string, string>> \u0006, IDirectVariable \u0007, IExpression \u0008, ICollection<_IAssignmentExpression> \u000E)
		{
			long nPosition;
			short sPositionOffset;
			PositionHelper.SplitPosition(\u0002, ref nPosition, ref sPositionOffset);
			ISourcePosition sp = this.\u0001.CreateSourcePosition(-1, Guid.Empty, nPosition, sPositionOffset, 0);
			_IVariable ivariable = this.\u0001.CreateVariable(sp);
			ivariable.Name = \u0003;
			ivariable._Type = \u0004;
			ivariable.SetFlag(\u0005, true);
			if (\u0006 != null)
			{
				foreach (KeyValuePair<string, string> keyValuePair in \u0006)
				{
					ivariable.SetAttributeValue(keyValuePair.Key, keyValuePair.Value);
				}
			}
			ivariable.Address = \u0007;
			ivariable.Initial = \u0008;
			if (\u000E != null)
			{
				ivariable.SetInputAssignments(\u000E);
			}
			return ivariable;
		}

		// Token: 0x06000C61 RID: 3169 RVA: 0x0001E04C File Offset: 0x0001C24C
		public _ISignature2 \u0001()
		{
			_IExpression nameExpression = this.\u0001.\u0001<_IExpression>(this.\u0001);
			_IExpression baseSignature = this.\u0001.\u0001<_IExpression>(this.\u0001);
			int num = this.\u0001.ReadInt32();
			_IExpression[] array = new _IExpression[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = this.\u0001.\u0001<_IExpression>(this.\u0001);
			}
			Operator poutype = (Operator)this.\u0001.ReadUInt32();
			IEnumerable<KeyValuePair<string, string>> enumerable = this.\u0001();
			SignatureFlag flags = (SignatureFlag)this.\u0001.ReadUInt64();
			SignatureFlagInternal internalFlags = (SignatureFlagInternal)this.\u0001.ReadUInt64();
			int num2 = this.\u0001.ReadInt32();
			List<_IVariable> list = new List<_IVariable>(num2);
			for (int j = 0; j < num2; j++)
			{
				list.Add(this.\u0001());
			}
			string libraryPath = this.\u0001.ReadString();
			Guid objectGuid = this.\u0001.\u0001(this.\u0001);
			Guid parentObjectGuid = this.\u0001.\u0001(this.\u0001);
			Guid messageGuid = this.\u0001.\u0001(this.\u0001);
			uint checksum = this.\u0001.ReadUInt32();
			uint checksumNoInit = this.\u0001.ReadUInt32();
			long timeStamp = this.\u0001.ReadInt64();
			_ISignature3 isignature = (_ISignature3)this.\u0001.CreateSignature();
			isignature._NameExpression = nameExpression;
			isignature._BaseSignature = baseSignature;
			foreach (_IExpression expInterface in array)
			{
				isignature.AddInterface(expInterface);
			}
			isignature.POUType = poutype;
			foreach (KeyValuePair<string, string> keyValuePair in enumerable)
			{
				isignature.AddAttribute(keyValuePair.Key, keyValuePair.Value);
			}
			isignature.Flags = flags;
			isignature.InternalFlags = internalFlags;
			foreach (_IVariable var in list)
			{
				isignature.AddVariable(var);
			}
			isignature.LibraryPath = libraryPath;
			isignature.ObjectGuid = objectGuid;
			isignature.ParentObjectGuid = parentObjectGuid;
			isignature.MessageGuid = messageGuid;
			isignature.Checksum = checksum;
			isignature.ChecksumNoInit = checksumNoInit;
			isignature.TimeStamp = timeStamp;
			int num3 = this.\u0001.ReadInt32();
			if (num3 > 0)
			{
				List<ISourcePosition> list2 = new List<ISourcePosition>(num3);
				for (int l = 0; l < num3; l++)
				{
					ISourcePosition item = this.\u0001();
					list2.Add(item);
				}
				isignature.UnusedDeclarationPositions = list2;
			}
			return isignature;
		}

		// Token: 0x06000C62 RID: 3170 RVA: 0x0001E304 File Offset: 0x0001C504
		public ISourcePosition \u0001()
		{
			int nProjectHandle = this.\u0001.ReadInt32();
			Guid objectGuid = this.\u0001.\u0001(this.\u0001);
			long nPosition = this.\u0001.ReadInt64();
			short sPositionOffset = this.\u0001.ReadInt16();
			short nLength = this.\u0001.ReadInt16();
			return this.\u0001.CreateSourcePosition(nProjectHandle, objectGuid, nPosition, sPositionOffset, nLength);
		}

		// Token: 0x06000C63 RID: 3171 RVA: 0x0001E368 File Offset: 0x0001C568
		public _ICompiledPOU2 \u0001(IList<_IExprement> \u0002)
		{
			string stName = this.\u0001.ReadString();
			Guid objectGuid = this.\u0001.\u0001(this.\u0001);
			Guid parentObjectGuid = this.\u0001.\u0001(this.\u0001);
			Guid messageGuid = this.\u0001.\u0001(this.\u0001);
			CompiledPOUFlags flags = (CompiledPOUFlags)this.\u0001.ReadUInt32();
			InternalCompiledPOUFlags internalFlags = (InternalCompiledPOUFlags)this.\u0001.ReadUInt32();
			string libraryPath = this.\u0001.ReadString();
			uint checksum = this.\u0001.ReadUInt32();
			long timeStamp = this.\u0001.ReadInt64();
			ICompiledPOUSerializable compiledPOUSerializable = (ICompiledPOUSerializable)this.\u0001.CreateCompiledPOU(stName);
			if (this.\u0001.ReadBoolean())
			{
				_IExprement iexprement;
				ICompactedParseTreeInformation compactedParseTreeInformation;
				global::\u0003.\u0001.\u0001(this.\u0001, this.\u0001, \u0002, out iexprement, out compactedParseTreeInformation);
				compiledPOUSerializable.SetParseTree((_IStatement)iexprement);
				((ICompiledPOUWithCompactedParseTree)compiledPOUSerializable).CompactedParseTreeInformation = compactedParseTreeInformation;
			}
			else
			{
				_IStatement parseTree = this.\u0001.\u0001<_IStatement>(this.\u0001);
				compiledPOUSerializable.SetParseTree(parseTree);
			}
			compiledPOUSerializable.ObjectGuid = objectGuid;
			compiledPOUSerializable.ParentObjectGuid = parentObjectGuid;
			compiledPOUSerializable.MessageGuid = messageGuid;
			compiledPOUSerializable.Flags = flags;
			compiledPOUSerializable.InternalFlags = internalFlags;
			compiledPOUSerializable.LibraryPath = libraryPath;
			compiledPOUSerializable.Checksum = checksum;
			compiledPOUSerializable.TimeStamp = timeStamp;
			return (_ICompiledPOU2)compiledPOUSerializable;
		}

		// Token: 0x06000C64 RID: 3172 RVA: 0x0001E4B4 File Offset: 0x0001C6B4
		private global::\u0016.\u0001.\u0001 \u0001()
		{
			return new global::\u0016.\u0001.\u0001
			{
				\u0001 = this.\u0001.ReadString(),
				\u0002 = this.\u0001.ReadString(),
				\u0003 = this.\u0001.ReadString(),
				\u0001 = this.\u0001.\u0001(this.\u0001),
				\u0002 = this.\u0001.\u0001(this.\u0001),
				\u0001 = this.\u0001.ReadBoolean(),
				\u0002 = this.\u0001.ReadBoolean(),
				\u0003 = this.\u0001.ReadBoolean(),
				\u0004 = this.\u0001.ReadBoolean(),
				\u0005 = this.\u0001.ReadBoolean()
			};
		}

		// Token: 0x06000C65 RID: 3173 RVA: 0x0001E57C File Offset: 0x0001C77C
		public _IPreCompileContext2 \u0001(IList<_IExprement> \u0002)
		{
			_IPreCompileContext2 ipreCompileContext = this.\u0001();
			int num = this.\u0001.ReadInt32();
			\u001B.\u0001.\u0001[] array = new \u001B.\u0001.\u0001[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = this.\u0001.\u0001(this.\u0001);
			}
			this.\u0001.\u0001(this.\u0001, ipreCompileContext.SlotPOUs);
			int num2 = this.\u0001.ReadInt32();
			IStaticMemorySegment[] array2 = new IStaticMemorySegment[num2];
			for (int j = 0; j < num2; j++)
			{
				array2[j] = this.\u0001.\u0001(this.\u0001, this.\u0001);
			}
			int num3 = this.\u0001.ReadInt32();
			global::\u0016.\u0001.\u0001[] array3 = new global::\u0016.\u0001.\u0001[num3];
			for (int k = 0; k < num3; k++)
			{
				array3[k] = this.\u0001();
			}
			string unitTestingDefine = this.\u0001.ReadString();
			IEnumerable<KeyValuePair<string, string>> enumerable = this.\u0002();
			IEnumerable<KeyValuePair<string, string>> enumerable2 = this.\u0002();
			int num4 = this.\u0001.ReadInt32();
			_ISignature2[] array4 = new _ISignature2[num4];
			for (int l = 0; l < num4; l++)
			{
				array4[l] = this.\u0001();
			}
			int num5 = this.\u0001.ReadInt32();
			_ICompiledPOU2[] array5 = new _ICompiledPOU2[num5];
			for (int m = 0; m < num5; m++)
			{
				array5[m] = this.\u0001(\u0002);
			}
			foreach (\u001B.\u0001.\u0001 u in array)
			{
				ipreCompileContext.TaskList.AddTaskInfo(u.\u0002, u.\u0001, u.\u0001, u.\u0002);
			}
			ipreCompileContext.AddStaticMemorySegments(array2);
			foreach (global::\u0016.\u0001.\u0001 u2 in array3)
			{
				ipreCompileContext.AddLibraryPlaceholder(u2.\u0001, null, u2.\u0003, u2.\u0002, u2.\u0001, u2.\u0002, u2.\u0004, u2.\u0001, u2.\u0003, u2.\u0002, u2.\u0005);
			}
			ipreCompileContext.UnitTestingDefine = unitTestingDefine;
			foreach (KeyValuePair<string, string> keyValuePair in enumerable)
			{
				ipreCompileContext.DefineTable[keyValuePair.Key] = keyValuePair.Value;
			}
			foreach (KeyValuePair<string, string> keyValuePair2 in enumerable2)
			{
				ipreCompileContext.TargetDefineTable[keyValuePair2.Key] = keyValuePair2.Value;
			}
			foreach (_ISignature2 sign in array4)
			{
				ipreCompileContext.AddGreenSignature(sign, false);
			}
			foreach (_ICompiledPOU2 cpou in array5)
			{
				ipreCompileContext.AddGreenCompiledPOU(cpou, false);
			}
			return ipreCompileContext;
		}

		// Token: 0x06000C66 RID: 3174 RVA: 0x0001E888 File Offset: 0x0001CA88
		private _IPreCompileContext2 \u0001()
		{
			KindOfContext kindof = (KindOfContext)this.\u0001.ReadUInt32();
			string stLibraryPath = this.\u0001.ReadString();
			string text = this.\u0001.ReadString();
			if (text.Length == 0)
			{
				text = null;
			}
			Guid applicationGuid = this.\u0001.\u0001(this.\u0001);
			bool linkAll = this.\u0001.ReadBoolean();
			bool linkInSimulation = this.\u0001.ReadBoolean();
			bool isInterfaceLibrary = this.\u0001.ReadBoolean();
			bool savedWithUnicodeIdentifiers = this.\u0001.ReadBoolean();
			bool onlineChangeable = this.\u0001.ReadBoolean();
			bool ignoreLinkAll = this.\u0001.ReadBoolean();
			bool qualifiedAccessOnly = this.\u0001.ReadBoolean();
			bool systemApplication = this.\u0001.ReadBoolean();
			bool supportDynamicMemory = this.\u0001.ReadBoolean();
			bool generateContent = this.\u0001.ReadBoolean();
			bool deviceApplication = this.\u0001.ReadBoolean();
			bool precompiledLibrary = this.\u0001.ReadBoolean();
			bool support32BitOnly = this.\u0001.ReadBoolean();
			long timeStamp = this.\u0001.ReadInt64();
			int targetOutputSize = this.\u0001.ReadInt32();
			int targetInputSize = this.\u0001.ReadInt32();
			int targetMemorySize = this.\u0001.ReadInt32();
			int targetStaticSize = this.\u0001.ReadInt32();
			_IPreCompileContext2 ipreCompileContext = (_IPreCompileContext2)this.\u0001.CreatePrecompileContext(stLibraryPath, applicationGuid, kindof);
			ipreCompileContext.Namespace = text;
			ipreCompileContext.ApplicationGuid = applicationGuid;
			ipreCompileContext.LinkAll = linkAll;
			ipreCompileContext.LinkInSimulation = linkInSimulation;
			ipreCompileContext.IsInterfaceLibrary = isInterfaceLibrary;
			ipreCompileContext.SavedWithUnicodeIdentifiers = savedWithUnicodeIdentifiers;
			ipreCompileContext.OnlineChangeable = onlineChangeable;
			ipreCompileContext.IgnoreLinkAll = ignoreLinkAll;
			ipreCompileContext.QualifiedAccessOnly = qualifiedAccessOnly;
			ipreCompileContext.SystemApplication = systemApplication;
			ipreCompileContext.SupportDynamicMemory = supportDynamicMemory;
			ipreCompileContext.GenerateContent = generateContent;
			ipreCompileContext.DeviceApplication = deviceApplication;
			ipreCompileContext.PrecompiledLibrary = precompiledLibrary;
			ipreCompileContext.Support32BitOnly = support32BitOnly;
			ipreCompileContext.TimeStamp = timeStamp;
			ipreCompileContext.TargetOutputSize = targetOutputSize;
			ipreCompileContext.TargetInputSize = targetInputSize;
			ipreCompileContext.TargetMemorySize = targetMemorySize;
			ipreCompileContext.TargetStaticSize = targetStaticSize;
			return ipreCompileContext;
		}

		// Token: 0x06000C67 RID: 3175 RVA: 0x0001EA70 File Offset: 0x0001CC70
		public ILibParameterTable \u0001()
		{
			ILibParameterTable2 libParameterTable = (ILibParameterTable2)this.\u0001.CreateLibraryParameterTable();
			int num = this.\u0001.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				string stName = this.\u0001.ReadString();
				IExpression expression = this.\u0001.\u0001<IExpression>(this.\u0001);
				libParameterTable.AddParameter(stName, expression);
			}
			return libParameterTable;
		}

		// Token: 0x06000C68 RID: 3176 RVA: 0x0001EAD0 File Offset: 0x0001CCD0
		public ILMLibraryInfo5 \u0001()
		{
			string stDefaultNamespace = this.\u0001.ReadString();
			string stId = this.\u0001.ReadString();
			bool bLinkAllContent = this.\u0001.ReadBoolean();
			bool bLinkInSimulation = this.\u0001.ReadBoolean();
			string stNamespace = this.\u0001.ReadString();
			ILibParameterTable paramTable = this.\u0001();
			bool bPublishSymbols = this.\u0001.ReadBoolean();
			bool bQualifiedOnly = this.\u0001.ReadBoolean();
			this.\u0001.ReadBoolean();
			bool bSystemLibrary = this.\u0001.ReadBoolean();
			bool qualifiedOnlyLocal = this.\u0001.ReadBoolean();
			bool onlineChangeable = this.\u0001.ReadBoolean();
			bool optional = this.\u0001.ReadBoolean();
			bool poolLibrary = this.\u0001.ReadBoolean();
			bool unresolvedReference = this.\u0001.ReadBoolean();
			ILMLibraryInfo5 ilmlibraryInfo = (ILMLibraryInfo5)this.\u0001.CreateLibInfo(stId, stDefaultNamespace, stNamespace, bSystemLibrary, bPublishSymbols, bLinkAllContent, bLinkInSimulation, bQualifiedOnly);
			ilmlibraryInfo.QualifiedOnlyLocal = qualifiedOnlyLocal;
			ilmlibraryInfo.OnlineChangeable = onlineChangeable;
			ilmlibraryInfo.Optional = optional;
			ilmlibraryInfo.PoolLibrary = poolLibrary;
			ilmlibraryInfo.UnresolvedReference = unresolvedReference;
			ilmlibraryInfo.ParamTable = paramTable;
			return ilmlibraryInfo;
		}

		// Token: 0x06000C69 RID: 3177 RVA: 0x0001EBE4 File Offset: 0x0001CDE4
		public ILMPlaceholderInfo \u0001()
		{
			string stDefaultLibraryId = this.\u0001.ReadString();
			string stName = this.\u0001.ReadString();
			string stNamespace = this.\u0001.ReadString();
			bool bPublishSymbols = this.\u0001.ReadBoolean();
			Guid guidResolver = this.\u0001.\u0001(this.\u0001);
			this.\u0001.ReadBoolean();
			this.\u0001.ReadBoolean();
			bool bLinkAllContent = this.\u0001.ReadBoolean();
			bool bLinkInSimulation = this.\u0001.ReadBoolean();
			Guid libManGuid = this.\u0001.\u0001(this.\u0001);
			return this.\u0001.CreateLibraryPlaceholder(stName, stDefaultLibraryId, stNamespace, bPublishSymbols, bLinkAllContent, bLinkInSimulation, guidResolver, libManGuid);
		}

		// Token: 0x06000C6A RID: 3178 RVA: 0x0001EC90 File Offset: 0x0001CE90
		public ILMLibraryList2 \u0001(out Guid \u0002, out string \u0003)
		{
			\u0002 = this.\u0001.\u0001(this.\u0001);
			\u0003 = this.\u0001.ReadString();
			Guid guidLibMan = this.\u0001.\u0001(this.\u0001);
			Guid objectGuid = this.\u0001.\u0001(this.\u0001);
			string stLibraryId = this.\u0001.ReadString();
			ILMLibraryList2 ilmlibraryList = (ILMLibraryList2)this.\u0001.CreateLibraryList(guidLibMan, stLibraryId);
			ilmlibraryList.ObjectGuid = objectGuid;
			int num = this.\u0001.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				ILMLibraryInfo5 libinfo = this.\u0001();
				ilmlibraryList.AddLibraryInfo(libinfo);
			}
			int num2 = this.\u0001.ReadInt32();
			for (int j = 0; j < num2; j++)
			{
				ILibParameterTable paramTable = this.\u0001();
				ILMPlaceholderInfo ilmplaceholderInfo = this.\u0001();
				ilmplaceholderInfo.ParamTable = paramTable;
				ilmlibraryList.AddPlaceholderInfo(ilmplaceholderInfo);
			}
			return ilmlibraryList;
		}

		// Token: 0x06000C6B RID: 3179 RVA: 0x0001ED7C File Offset: 0x0001CF7C
		public void \u0001(_IApplicationDeviceTable2 \u0002)
		{
			int num = this.\u0001.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				Guid guidApplication = this.\u0001.\u0001(this.\u0001);
				Guid guidDevice = this.\u0001.\u0001(this.\u0001);
				\u0002.AddApplicationDevice(guidApplication, guidDevice);
			}
			int num2 = this.\u0001.ReadInt32();
			for (int j = 0; j < num2; j++)
			{
				Guid guidApplication2 = this.\u0001.\u0001(this.\u0001);
				string stName = this.\u0001.ReadString();
				\u0002.SetApplicationName(guidApplication2, stName, false);
			}
			int num3 = this.\u0001.ReadInt32();
			for (int k = 0; k < num3; k++)
			{
				Guid guidApplication3 = this.\u0001.\u0001(this.\u0001);
				string stName2 = this.\u0001.ReadString();
				\u0002.SetApplicationName(guidApplication3, stName2, true);
			}
			int num4 = this.\u0001.ReadInt32();
			for (int l = 0; l < num4; l++)
			{
				Guid guidDevice2 = this.\u0001.\u0001(this.\u0001);
				string stName3 = this.\u0001.ReadString();
				\u0002.SetDeviceName(guidDevice2, stName3);
			}
			int num5 = this.\u0001.ReadInt32();
			for (int m = 0; m < num5; m++)
			{
				Guid guidDevice3 = this.\u0001.\u0001(this.\u0001);
				string stId = this.\u0001.ReadString();
				int nType = this.\u0001.ReadInt32();
				string stVersion = this.\u0001.ReadString();
				\u0002.SetTargetIdOfDevice(guidDevice3, this.\u0001.CreateDeviceIdentification(nType, stId, stVersion));
			}
			int num6 = this.\u0001.ReadInt32();
			for (int n = 0; n < num6; n++)
			{
				Guid guidApplication4 = this.\u0001.\u0001(this.\u0001);
				int num7 = this.\u0001.ReadInt32();
				for (int num8 = 0; num8 < num7; num8++)
				{
					Guid guidClone = this.\u0001.\u0001(this.\u0001);
					\u0002.AddCloneOfApplication(guidApplication4, guidClone);
				}
			}
			int num9 = this.\u0001.ReadInt32();
			for (int num10 = 0; num10 < num9; num10++)
			{
				Guid guidSubApplication = this.\u0001.\u0001(this.\u0001);
				Guid guidParentApplication = this.\u0001.\u0001(this.\u0001);
				\u0002.SetParentApplication(guidParentApplication, guidSubApplication);
			}
			int num11 = this.\u0001.ReadInt32();
			for (int num12 = 0; num12 < num11; num12++)
			{
				Guid guidApplication5 = this.\u0001.\u0001(this.\u0001);
				Guid guidMemorySettingsProvider = this.\u0001.\u0001(this.\u0001);
				\u0002.SetMemorySettingsProvider(guidApplication5, guidMemorySettingsProvider);
			}
		}

		// Token: 0x06000C6C RID: 3180 RVA: 0x0001F024 File Offset: 0x0001D224
		public void \u0001(ILMRelatedObjectTable \u0002)
		{
			int num = this.\u0001.ReadInt32();
			List<Guid> list = new List<Guid>();
			for (int i = 0; i < num; i++)
			{
				Guid key = this.\u0001.\u0001(this.\u0001);
				list.Clear();
				int num2 = this.\u0001.ReadInt32();
				for (int j = 0; j < num2; j++)
				{
					list.Add(this.\u0001.\u0001(this.\u0001));
				}
				\u0002.Add(key, list);
			}
		}

		// Token: 0x06000C6D RID: 3181 RVA: 0x0001F0A4 File Offset: 0x0001D2A4
		internal void \u0001(_IPreCompCrossReferences \u0002)
		{
			IList<Guid> list = \u001B.\u0001.\u0001<Guid>(this.\u0001, new Func<BinaryReader, Guid>(this.\u0001.\u0001));
			int num = this.\u0001.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				string stName = this.\u0001.ReadString();
				int num2 = this.\u0001.ReadInt32();
				for (int j = 0; j < num2; j++)
				{
					int index = this.\u0001.ReadInt32();
					foreach (int index2 in \u001B.\u0001.\u0001<int>(this.\u0001, new Func<BinaryReader, int>(\u001B.\u0001.\u0001)))
					{
						\u0002.Add(stName, list[index2], list[index]);
					}
				}
			}
		}

		// Token: 0x04000216 RID: 534
		private readonly ITreeFactory \u0001;

		// Token: 0x04000217 RID: 535
		private readonly _ILanguageModelBuilder2 \u0001;

		// Token: 0x04000218 RID: 536
		private readonly \u001B.\u0001 \u0001;

		// Token: 0x04000219 RID: 537
		private readonly \u001E.\u0003 \u0001;

		// Token: 0x0400021A RID: 538
		private readonly global::\u0008.\u0003 \u0001;

		// Token: 0x0400021B RID: 539
		private readonly BinaryReader \u0001;

		// Token: 0x02000095 RID: 149
		private sealed class \u0001
		{
			// Token: 0x0400021C RID: 540
			public string \u0001;

			// Token: 0x0400021D RID: 541
			public string \u0002;

			// Token: 0x0400021E RID: 542
			public string \u0003;

			// Token: 0x0400021F RID: 543
			public Guid \u0001;

			// Token: 0x04000220 RID: 544
			public Guid \u0002;

			// Token: 0x04000221 RID: 545
			public bool \u0001;

			// Token: 0x04000222 RID: 546
			public bool \u0002;

			// Token: 0x04000223 RID: 547
			public bool \u0003;

			// Token: 0x04000224 RID: 548
			public bool \u0004;

			// Token: 0x04000225 RID: 549
			public bool \u0005;
		}
	}
}
