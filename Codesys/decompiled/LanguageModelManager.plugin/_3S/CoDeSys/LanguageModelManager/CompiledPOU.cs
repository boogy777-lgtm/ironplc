using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using SmartAssembly.Attributes;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.GreenTrees;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000C1 RID: 193
	[TypeGuid("{b19c1570-9d63-448c-b1a8-77c564f4bf41}")]
	[StorageVersion("3.3.0.0")]
	[DebuggerDisplay("{Name} Signature Id: {SignatureId}")]
	public class CompiledPOU : GenericObject2, _ICompiledPOU2, _ICompiledPOU, ICompiledPOU10, ICompiledPOU9, ICompiledPOU8, ICompiledPOU6, ICompiledPOU5, ICompiledPOU4, ICompiledPOU3, ICompiledPOU, ICompiledPOU7, ICompiledPOUWithCompactedParseTree, ICompiledPOUWithParseTreeProvider, ICompiledPOUSerializable
	{
		// Token: 0x170002CB RID: 715
		// (get) Token: 0x06000B88 RID: 2952 RVA: 0x0001D36B File Offset: 0x0001C36B
		// (set) Token: 0x06000B89 RID: 2953 RVA: 0x0001D376 File Offset: 0x0001C376
		[DefaultSerialization("ParseTreeEmpty")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private bool ParseTreeEmpty
		{
			get
			{
				return this.m_parseTree == null;
			}
			set
			{
				this.m_parseTreeEmpty = value;
			}
		}

		// Token: 0x170002CC RID: 716
		// (get) Token: 0x06000B8A RID: 2954 RVA: 0x0001D37F File Offset: 0x0001C37F
		// (set) Token: 0x06000B8B RID: 2955 RVA: 0x0001D38C File Offset: 0x0001C38C
		public ICompactedParseTreeInformation CompactedParseTreeInformation
		{
			get
			{
				return this.ParseTreeProvider.CompactedParseTreeInformation;
			}
			set
			{
				this.ParseTreeProvider.CompactedParseTreeInformation = value;
			}
		}

		// Token: 0x170002CD RID: 717
		// (get) Token: 0x06000B8C RID: 2956 RVA: 0x0001D39A File Offset: 0x0001C39A
		// (set) Token: 0x06000B8D RID: 2957 RVA: 0x0001D39E File Offset: 0x0001C39E
		[DefaultSerialization("PreCompiledPOUFlags")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private CompiledPOUFlags PreCompiledPOUFlags
		{
			get
			{
				return CompiledPOUFlags.TimeStampOnly;
			}
			set
			{
				this.m_cpFlags = CompiledPOUFlags.TimeStampOnly;
			}
		}

		// Token: 0x06000B8E RID: 2958 RVA: 0x0001D3A8 File Offset: 0x0001C3A8
		public CompiledPOU()
		{
			this.ParseTreeProvider = CompilerProxy.ParseTreeService.CreateParseTreeProvider(this, CompilerProxy.GreenTreeConverter_OrNull, RedTreeFactory.Singleton);
		}

		// Token: 0x06000B8F RID: 2959 RVA: 0x0001D410 File Offset: 0x0001C410
		public CompiledPOU(string stName)
		{
			this.ParseTreeProvider = CompilerProxy.ParseTreeService.CreateParseTreeProvider(this, CompilerProxy.GreenTreeConverter_OrNull, RedTreeFactory.Singleton);
			this.Name = stName;
		}

		// Token: 0x170002CE RID: 718
		// (get) Token: 0x06000B90 RID: 2960 RVA: 0x0001D47C File Offset: 0x0001C47C
		// (set) Token: 0x06000B91 RID: 2961 RVA: 0x0001D484 File Offset: 0x0001C484
		public IParseTreeProvider ParseTreeProvider { get; set; }

		// Token: 0x170002CF RID: 719
		// (get) Token: 0x06000B92 RID: 2962 RVA: 0x0001D48D File Offset: 0x0001C48D
		public IStatement ParseTree
		{
			[ObfuscateControlFlow]
			get
			{
				if (this.NoAccess)
				{
					CodeAccessSecurity.AssertCallerHasKeyFlag("Decompile", 2);
				}
				return this.GetParseTree();
			}
		}

		// Token: 0x170002D0 RID: 720
		// (get) Token: 0x06000B93 RID: 2963 RVA: 0x0001D4A8 File Offset: 0x0001C4A8
		public _IStatement OriginalParseTree
		{
			get
			{
				return this.m_parseTree;
			}
		}

		// Token: 0x170002D1 RID: 721
		// (get) Token: 0x06000B94 RID: 2964 RVA: 0x0001D4A8 File Offset: 0x0001C4A8
		// (set) Token: 0x06000B95 RID: 2965 RVA: 0x0001D4B0 File Offset: 0x0001C4B0
		public _IStatement ParseTreeRaw
		{
			get
			{
				return this.m_parseTree;
			}
			set
			{
				this.m_parseTree = value;
			}
		}

		// Token: 0x170002D2 RID: 722
		// (get) Token: 0x06000B96 RID: 2966 RVA: 0x0001D4B9 File Offset: 0x0001C4B9
		// (set) Token: 0x06000B97 RID: 2967 RVA: 0x0001D4C1 File Offset: 0x0001C4C1
		[Obsolete("Nimm CRC")]
		public long TimeStamp
		{
			get
			{
				return this.m_lTimeStamp;
			}
			set
			{
				this.m_lTimeStamp = value;
			}
		}

		// Token: 0x170002D3 RID: 723
		// (get) Token: 0x06000B98 RID: 2968 RVA: 0x0001D4CA File Offset: 0x0001C4CA
		// (set) Token: 0x06000B99 RID: 2969 RVA: 0x0001D4D2 File Offset: 0x0001C4D2
		public uint Checksum
		{
			get
			{
				return this.m_crc;
			}
			set
			{
				this.m_crc = value;
			}
		}

		// Token: 0x170002D4 RID: 724
		// (get) Token: 0x06000B9A RID: 2970 RVA: 0x0001D4DB File Offset: 0x0001C4DB
		// (set) Token: 0x06000B9B RID: 2971 RVA: 0x0001D511 File Offset: 0x0001C511
		[DefaultSerialization("messages")]
		[StorageVersion("3.3.0.0")]
		public CompilerMessage[] MessagesCompatibility
		{
			get
			{
				if (this.m_messages == null)
				{
					return null;
				}
				return Array.ConvertAll<_ICompilerMessage, CompilerMessage>(this.m_messages, (_ICompilerMessage input) => input as CompilerMessage);
			}
			set
			{
				if (value != null)
				{
					this.m_messages = Array.ConvertAll<CompilerMessage, _ICompilerMessage>(value, (CompilerMessage input) => input);
				}
			}
		}

		// Token: 0x170002D5 RID: 725
		// (get) Token: 0x06000B9C RID: 2972 RVA: 0x0001D541 File Offset: 0x0001C541
		public _ICompilerMessage[] Messages
		{
			get
			{
				return this.m_messages;
			}
		}

		// Token: 0x06000B9D RID: 2973 RVA: 0x0001D549 File Offset: 0x0001C549
		public void SetMessages(IList<_ICompilerMessage> messages)
		{
			this.m_messages = null;
			if (messages != null)
			{
				this.m_messages = messages.ToArray<_ICompilerMessage>();
			}
		}

		// Token: 0x170002D6 RID: 726
		// (get) Token: 0x06000B9E RID: 2974 RVA: 0x0001D561 File Offset: 0x0001C561
		// (set) Token: 0x06000B9F RID: 2975 RVA: 0x0001D569 File Offset: 0x0001C569
		public _ICompilerMessage[] PrecompileMessages
		{
			get
			{
				return this.m_pmessages;
			}
			set
			{
				this.m_pmessages = value;
			}
		}

		// Token: 0x06000BA0 RID: 2976 RVA: 0x0001D572 File Offset: 0x0001C572
		public void SetPrecompileMessages(IList<_ICompilerMessage> messages)
		{
			if (messages != null)
			{
				this.m_pmessages = messages.ToArray<_ICompilerMessage>();
			}
		}

		// Token: 0x170002D7 RID: 727
		// (get) Token: 0x06000BA1 RID: 2977 RVA: 0x0001D583 File Offset: 0x0001C583
		// (set) Token: 0x06000BA2 RID: 2978 RVA: 0x0001D58B File Offset: 0x0001C58B
		public IEnumerable<IBitWriteAccess> BitWriteAccesses
		{
			get
			{
				return this.m_BitWriteAccesses;
			}
			set
			{
				this.m_BitWriteAccesses = new LList<IBitWriteAccess>(value);
			}
		}

		// Token: 0x06000BA3 RID: 2979 RVA: 0x0001D599 File Offset: 0x0001C599
		public void ClearBitWriteAccesses()
		{
			this.m_BitWriteAccesses = null;
		}

		// Token: 0x06000BA4 RID: 2980 RVA: 0x0001D5A2 File Offset: 0x0001C5A2
		public void AddBitWriteAccess(IBitWriteAccess bwa)
		{
			if (this.m_BitWriteAccesses == null)
			{
				this.m_BitWriteAccesses = new LList<IBitWriteAccess>();
			}
			this.m_BitWriteAccesses.Add(bwa);
		}

		// Token: 0x170002D8 RID: 728
		// (get) Token: 0x06000BA5 RID: 2981 RVA: 0x0001D5C3 File Offset: 0x0001C5C3
		public bool NoAccess
		{
			get
			{
				return !string.IsNullOrEmpty(this.LibraryPath) && !this.GetFlagInternal(InternalCompiledPOUFlags.LibraryAccessPermitted);
			}
		}

		// Token: 0x170002D9 RID: 729
		// (get) Token: 0x06000BA6 RID: 2982 RVA: 0x0001D5DE File Offset: 0x0001C5DE
		// (set) Token: 0x06000BA7 RID: 2983 RVA: 0x0001D5E6 File Offset: 0x0001C5E6
		public IList<IDataLocation> TryCatchFPAddresses
		{
			get
			{
				return this.m_lTryCatchFPAddresses;
			}
			set
			{
				this.m_lTryCatchFPAddresses = new LList<IDataLocation>(value);
			}
		}

		// Token: 0x06000BA8 RID: 2984 RVA: 0x0001D5F4 File Offset: 0x0001C5F4
		public int AddTryCatchFPAddress(IDataLocation datloc)
		{
			if (this.m_lTryCatchFPAddresses == null)
			{
				this.m_lTryCatchFPAddresses = new LList<IDataLocation>();
			}
			this.m_lTryCatchFPAddresses.Add(datloc);
			return this.m_lTryCatchFPAddresses.Count - 1;
		}

		// Token: 0x170002DA RID: 730
		// (get) Token: 0x06000BA9 RID: 2985 RVA: 0x0001D622 File Offset: 0x0001C622
		// (set) Token: 0x06000BAA RID: 2986 RVA: 0x0001D62A File Offset: 0x0001C62A
		public IList<int> TryCatchCodeAddresses
		{
			get
			{
				return this.m_lTryCatchCodeAddresses;
			}
			set
			{
				this.m_lTryCatchCodeAddresses = new LList<int>(value);
			}
		}

		// Token: 0x06000BAB RID: 2987 RVA: 0x0001D638 File Offset: 0x0001C638
		[Obsolete("use AddTryCatchCodeAddressIndex")]
		public int AddTryCatchCodeAddress(int iOffset)
		{
			if (this.m_lTryCatchCodeAddresses == null)
			{
				this.m_lTryCatchCodeAddresses = new LList<int>();
			}
			this.m_lTryCatchCodeAddresses.Add(iOffset);
			return this.m_lTryCatchCodeAddresses.Count - 1;
		}

		// Token: 0x06000BAC RID: 2988 RVA: 0x0001D668 File Offset: 0x0001C668
		public void AddTryCatchCodeAddressIndex(int iOffset, int iIndex)
		{
			if (this.m_lTryCatchCodeAddresses == null)
			{
				this.m_lTryCatchCodeAddresses = new LList<int>(this.m_lTryCatchFPAddresses.Count);
				for (int i = 0; i < this.m_lTryCatchFPAddresses.Count; i++)
				{
					this.m_lTryCatchCodeAddresses.Add(-1);
				}
			}
			this.m_lTryCatchCodeAddresses[iIndex] = iOffset;
		}

		// Token: 0x06000BAD RID: 2989 RVA: 0x0001D6C4 File Offset: 0x0001C6C4
		private _ICompilerMessage[] GetMessagesX()
		{
			bool flag = this.GetFlag(CompiledPOUFlags.Typified);
			if (this.GetFlag(CompiledPOUFlags.ContainsNoParseTree) || this.GetFlag(CompiledPOUFlags.ContainsNoCode) || this.GetFlagInternal(InternalCompiledPOUFlags.Checked) || this.m_parseTree == null)
			{
				if (this.m_messages == null)
				{
					return Array.Empty<_ICompilerMessage>();
				}
				return this.m_messages;
			}
			else
			{
				IMessage[] poumessages = CompilerProxy.GetPOUMessages(this, flag);
				if (poumessages == null || poumessages.Length == 0)
				{
					return Array.Empty<_ICompilerMessage>();
				}
				_ICompilerMessage[] array = new _ICompilerMessage[poumessages.Length];
				for (int i = 0; i < poumessages.Length; i++)
				{
					array[i] = (poumessages[i] as _ICompilerMessage);
				}
				return array;
			}
		}

		// Token: 0x06000BAE RID: 2990 RVA: 0x0001D754 File Offset: 0x0001C754
		public IMessage4[] GetMessages()
		{
			return this.GetMessages(true);
		}

		// Token: 0x06000BAF RID: 2991 RVA: 0x0001D76C File Offset: 0x0001C76C
		public _ICompilerMessage[] GetMessages(bool bWithPrecompiledErrors)
		{
			_ICompilerMessage[] messagesX = this.GetMessagesX();
			if (!bWithPrecompiledErrors || this.m_pmessages == null)
			{
				return messagesX;
			}
			_ICompilerMessage[] array = new _ICompilerMessage[messagesX.Length + this.m_pmessages.Length];
			messagesX.CopyTo(array, 0);
			this.m_pmessages.CopyTo(array, messagesX.Length);
			return array;
		}

		// Token: 0x06000BB0 RID: 2992 RVA: 0x0001D7B8 File Offset: 0x0001C7B8
		public void UpdateTimeStamp()
		{
			this.m_lTimeStamp = DateTime.Now.Ticks;
		}

		// Token: 0x06000BB1 RID: 2993 RVA: 0x0001D7D8 File Offset: 0x0001C7D8
		public void UpdateChecksum()
		{
			ICheckSumVisitor checkSumVisitor = CompilerProxy.CreateChecksumVisitor(false);
			checkSumVisitor.Traverser.visit(this);
			this.Checksum = checkSumVisitor.Checksum;
		}

		// Token: 0x06000BB2 RID: 2994 RVA: 0x0001D804 File Offset: 0x0001C804
		public void SetParseTree(_IStatement parseTree)
		{
			this.ParseTreeProvider.SetParseTreeWithSideEffects(parseTree);
		}

		// Token: 0x06000BB3 RID: 2995 RVA: 0x0001D4B0 File Offset: 0x0001C4B0
		public void SetParseTreeDirectly(_IStatement parseTree)
		{
			this.m_parseTree = parseTree;
		}

		// Token: 0x06000BB4 RID: 2996 RVA: 0x0001D812 File Offset: 0x0001C812
		public _IStatement GetParseTree()
		{
			return this.ParseTreeProvider.GetParseTree();
		}

		// Token: 0x06000BB5 RID: 2997 RVA: 0x0001D81F File Offset: 0x0001C81F
		public _IStatement CreateTemporaryRedTree()
		{
			return this.ParseTreeProvider.CreateTemporaryRedTree();
		}

		// Token: 0x06000BB6 RID: 2998 RVA: 0x00005F0F File Offset: 0x00004F0F
		[ObfuscateControlFlow]
		[Obsolete("returns null!")]
		public string GetCode()
		{
			return null;
		}

		// Token: 0x06000BB7 RID: 2999 RVA: 0x0001D82C File Offset: 0x0001C82C
		public void SetBreakpointList(IBreakpointList bpl)
		{
			if (bpl == null || bpl.Count == 0)
			{
				this.m_breakpointlist = null;
				return;
			}
			if (bpl is BreakpointList && (bpl as BreakpointList).Count < 10)
			{
				this.m_breakpointlist = (bpl as BreakpointList).CreateLittleList();
			}
			this.m_breakpointlist = bpl;
		}

		// Token: 0x170002DB RID: 731
		// (get) Token: 0x06000BB8 RID: 3000 RVA: 0x0001D87B File Offset: 0x0001C87B
		public IBreakpointList BreakpointList
		{
			get
			{
				if (this.m_breakpointlist == null)
				{
					return new BreakpointList();
				}
				return this.m_breakpointlist;
			}
		}

		// Token: 0x170002DC RID: 732
		// (get) Token: 0x06000BB9 RID: 3001 RVA: 0x0001D891 File Offset: 0x0001C891
		// (set) Token: 0x06000BBA RID: 3002 RVA: 0x0001D89E File Offset: 0x0001C89E
		public string Name
		{
			get
			{
				return this.m_stName.ToUpperInvariant();
			}
			set
			{
				this.m_stName = value;
			}
		}

		// Token: 0x06000BBB RID: 3003 RVA: 0x0001D8A8 File Offset: 0x0001C8A8
		public string GetFullName(_ICompileContext comcon)
		{
			string text = this.Name;
			_ISignature isignature = comcon[this.SignatureId];
			if (isignature != null && isignature.Name != IdentifierConstants.MainSignatureName)
			{
				_ISignature isignature2 = comcon[isignature.ParentSignatureId];
				if (isignature2 != null)
				{
					text = isignature2.Name + "." + text;
				}
			}
			return text;
		}

		// Token: 0x170002DD RID: 733
		// (get) Token: 0x06000BBC RID: 3004 RVA: 0x0001D901 File Offset: 0x0001C901
		// (set) Token: 0x06000BBD RID: 3005 RVA: 0x0001D909 File Offset: 0x0001C909
		public Guid ObjectGuid
		{
			get
			{
				return this.m_objectGuid;
			}
			set
			{
				this.m_objectGuid = value;
			}
		}

		// Token: 0x170002DE RID: 734
		// (get) Token: 0x06000BBE RID: 3006 RVA: 0x0001D912 File Offset: 0x0001C912
		// (set) Token: 0x06000BBF RID: 3007 RVA: 0x0001D933 File Offset: 0x0001C933
		public Guid MessageGuid
		{
			get
			{
				if (this.m_messageGuid == Guid.Empty)
				{
					return this.ObjectGuid;
				}
				return this.m_messageGuid;
			}
			set
			{
				this.m_messageGuid = value;
			}
		}

		// Token: 0x170002DF RID: 735
		// (get) Token: 0x06000BC0 RID: 3008 RVA: 0x0001D93C File Offset: 0x0001C93C
		// (set) Token: 0x06000BC1 RID: 3009 RVA: 0x0001D944 File Offset: 0x0001C944
		public Guid ParentObjectGuid
		{
			get
			{
				return this.m_parentObjectGuid;
			}
			set
			{
				this.m_parentObjectGuid = value;
			}
		}

		// Token: 0x170002E0 RID: 736
		// (get) Token: 0x06000BC2 RID: 3010 RVA: 0x0001D94D File Offset: 0x0001C94D
		// (set) Token: 0x06000BC3 RID: 3011 RVA: 0x0001D955 File Offset: 0x0001C955
		public int SignatureId
		{
			get
			{
				return this.m_nSignId;
			}
			set
			{
				this.m_nSignId = value;
			}
		}

		// Token: 0x170002E1 RID: 737
		// (get) Token: 0x06000BC4 RID: 3012 RVA: 0x0001D95E File Offset: 0x0001C95E
		// (set) Token: 0x06000BC5 RID: 3013 RVA: 0x0001D966 File Offset: 0x0001C966
		public ICompiledCode CompiledCode
		{
			get
			{
				return this.m_code;
			}
			set
			{
				this.m_code = value;
			}
		}

		// Token: 0x170002E2 RID: 738
		// (get) Token: 0x06000BC6 RID: 3014 RVA: 0x0001D96F File Offset: 0x0001C96F
		// (set) Token: 0x06000BC7 RID: 3015 RVA: 0x0001D977 File Offset: 0x0001C977
		public int ScratchSize
		{
			get
			{
				return this.m_nScratchSize;
			}
			set
			{
				this.m_nScratchSize = value;
			}
		}

		// Token: 0x170002E3 RID: 739
		// (get) Token: 0x06000BC8 RID: 3016 RVA: 0x0001D980 File Offset: 0x0001C980
		// (set) Token: 0x06000BC9 RID: 3017 RVA: 0x0001D988 File Offset: 0x0001C988
		public int MaxParamSize
		{
			get
			{
				return this.m_nMaxParamSize;
			}
			set
			{
				this.m_nMaxParamSize = value;
			}
		}

		// Token: 0x170002E4 RID: 740
		// (get) Token: 0x06000BCA RID: 3018 RVA: 0x0001D991 File Offset: 0x0001C991
		// (set) Token: 0x06000BCB RID: 3019 RVA: 0x0001D999 File Offset: 0x0001C999
		public int CodeGeneratorStackSize
		{
			get
			{
				return this.m_nCodeGeneratorStackSize;
			}
			set
			{
				this.m_nCodeGeneratorStackSize = value;
			}
		}

		// Token: 0x170002E5 RID: 741
		// (get) Token: 0x06000BCC RID: 3020 RVA: 0x0001D9A2 File Offset: 0x0001C9A2
		public string OriginalName
		{
			get
			{
				return this.m_stName;
			}
		}

		// Token: 0x170002E6 RID: 742
		// (get) Token: 0x06000BCD RID: 3021 RVA: 0x0001D9AA File Offset: 0x0001C9AA
		// (set) Token: 0x06000BCE RID: 3022 RVA: 0x0001D9B2 File Offset: 0x0001C9B2
		public CompiledPOUFlags Flags
		{
			get
			{
				return this.m_cpFlags;
			}
			set
			{
				this.m_cpFlags = value;
			}
		}

		// Token: 0x170002E7 RID: 743
		// (get) Token: 0x06000BCF RID: 3023 RVA: 0x0001D9BB File Offset: 0x0001C9BB
		// (set) Token: 0x06000BD0 RID: 3024 RVA: 0x0001D9C3 File Offset: 0x0001C9C3
		public InternalCompiledPOUFlags InternalFlags
		{
			get
			{
				return this.m_cpFlagsInternal;
			}
			set
			{
				this.m_cpFlagsInternal = value;
			}
		}

		// Token: 0x06000BD1 RID: 3025 RVA: 0x0001D9CC File Offset: 0x0001C9CC
		public bool GetFlagInternal(InternalCompiledPOUFlags cpFlag)
		{
			return (this.m_cpFlagsInternal & cpFlag) == cpFlag;
		}

		// Token: 0x06000BD2 RID: 3026 RVA: 0x0001D9D9 File Offset: 0x0001C9D9
		public void SetFlagInternal(InternalCompiledPOUFlags cpFlag, bool bSetTrue)
		{
			if (bSetTrue)
			{
				this.m_cpFlagsInternal |= cpFlag;
				return;
			}
			this.m_cpFlagsInternal &= ~cpFlag;
		}

		// Token: 0x06000BD3 RID: 3027 RVA: 0x0001D9FC File Offset: 0x0001C9FC
		public bool GetFlag(CompiledPOUFlags cpFlag)
		{
			return (this.m_cpFlags & cpFlag) == cpFlag;
		}

		// Token: 0x06000BD4 RID: 3028 RVA: 0x0001DA09 File Offset: 0x0001CA09
		public void SetFlag(CompiledPOUFlags cpFlag, bool bSetTrue)
		{
			if (bSetTrue)
			{
				this.m_cpFlags |= cpFlag;
				return;
			}
			this.m_cpFlags &= ~cpFlag;
		}

		// Token: 0x170002E8 RID: 744
		// (get) Token: 0x06000BD5 RID: 3029 RVA: 0x0001DA2C File Offset: 0x0001CA2C
		// (set) Token: 0x06000BD6 RID: 3030 RVA: 0x0001DA42 File Offset: 0x0001CA42
		public string LibraryPath
		{
			get
			{
				if (this.m_stLibraryPath != null)
				{
					return this.m_stLibraryPath;
				}
				return string.Empty;
			}
			set
			{
				this.m_stLibraryPath = ((value == string.Empty) ? null : value);
			}
		}

		// Token: 0x170002E9 RID: 745
		// (get) Token: 0x06000BD7 RID: 3031 RVA: 0x0001DA5B File Offset: 0x0001CA5B
		public long ImplicitReturnPositionPos
		{
			get
			{
				return 281474976710655L;
			}
		}

		// Token: 0x170002EA RID: 746
		// (get) Token: 0x06000BD8 RID: 3032 RVA: 0x0001DA66 File Offset: 0x0001CA66
		public ISourcePosition ImplicitReturnPosition
		{
			get
			{
				return new SourcePosition(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(this.LibraryPath), this.MessageGuid, this.ImplicitReturnPositionPos, 0, 0);
			}
		}

		// Token: 0x06000BD9 RID: 3033 RVA: 0x0001DA98 File Offset: 0x0001CA98
		public ISourcePosition GetSourcePositionOfBreakpoint(IBreakpoint bp)
		{
			ISourcePosition position = bp.Position;
			SourcePosition sourcePosition = position as SourcePosition;
			if (sourcePosition == null)
			{
				return position;
			}
			sourcePosition.SetObjectIdentification(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(this.LibraryPath), this.MessageGuid);
			return sourcePosition;
		}

		// Token: 0x06000BDA RID: 3034 RVA: 0x0001DAE0 File Offset: 0x0001CAE0
		public _ICompiledPOU CreateCompiledPOU()
		{
			CompiledPOU compiledPOU = new CompiledPOU(this.m_stName);
			compiledPOU.m_cpFlags = this.m_cpFlags;
			compiledPOU.m_cpFlagsInternal = this.m_cpFlagsInternal;
			compiledPOU.m_objectGuid = this.m_objectGuid;
			compiledPOU.m_messageGuid = this.m_messageGuid;
			compiledPOU.m_parentObjectGuid = this.m_parentObjectGuid;
			compiledPOU.m_lTimeStamp = this.m_lTimeStamp;
			compiledPOU.Checksum = this.Checksum;
			compiledPOU._nStatements = this._nStatements;
			compiledPOU.m_stLibraryPath = this.m_stLibraryPath;
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351700)
			{
				compiledPOU.SetMessages(this.Messages);
			}
			compiledPOU.SetFlag(CompiledPOUFlags.Compiled, true);
			compiledPOU.SetFlag(CompiledPOUFlags.ContainsNoParseTree, false);
			compiledPOU.SetFlagInternal(InternalCompiledPOUFlags.Checked, false);
			this.ParseTreeProvider.CreateParseTreeForCompiledPOU(compiledPOU);
			return compiledPOU;
		}

		// Token: 0x06000BDB RID: 3035 RVA: 0x0001DBB0 File Offset: 0x0001CBB0
		public _ICompiledPOU Duplicate()
		{
			CompiledPOU compiledPOU = new CompiledPOU(this.m_stName);
			compiledPOU.m_cpFlags = this.m_cpFlags;
			compiledPOU.m_objectGuid = this.m_objectGuid;
			compiledPOU.m_messageGuid = this.m_messageGuid;
			compiledPOU.m_parentObjectGuid = this.m_parentObjectGuid;
			compiledPOU.m_lTimeStamp = this.m_lTimeStamp;
			if (this.m_parseTree == null)
			{
				compiledPOU.m_parseTree = this.GetParseTree();
				this.m_parseTree = null;
			}
			else
			{
				compiledPOU.m_parseTree = (this.m_parseTree.Duplicate() as _IStatement);
			}
			compiledPOU.m_stLibraryPath = this.m_stLibraryPath;
			compiledPOU.SetMessages(this.Messages);
			if (this.m_lTryCatchCodeAddresses != null)
			{
				compiledPOU.m_lTryCatchCodeAddresses = new LList<int>(this.m_lTryCatchCodeAddresses);
			}
			if (this.m_lTryCatchFPAddresses != null)
			{
				compiledPOU.m_lTryCatchFPAddresses = new LList<IDataLocation>(this.m_lTryCatchFPAddresses);
			}
			compiledPOU._nStatements = this._nStatements;
			return compiledPOU;
		}

		// Token: 0x06000BDC RID: 3036 RVA: 0x0001DC8F File Offset: 0x0001CC8F
		public void DuplicateParseTreeForCompilation()
		{
			this.ParseTreeProvider.DuplicateParseTreeForCompilation();
		}

		// Token: 0x06000BDD RID: 3037 RVA: 0x0001DC9C File Offset: 0x0001CC9C
		public void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000BDE RID: 3038 RVA: 0x0001DCA8 File Offset: 0x0001CCA8
		public override void AfterDeserialize()
		{
			base.AfterDeserialize();
			if (this.m_stCode == null && this.m_parseTree == null)
			{
				this.m_parseTree = CompiledPOU.s_EmptyStatement;
			}
			if (this.m_stCode != null && this.m_parseTreeEmpty)
			{
				this.m_parseTree = null;
			}
			if (this.m_crc == 0U)
			{
				_IStatement istatement = this.m_parseTree;
				if (this.m_parseTree == null && this.m_stCode != null)
				{
					istatement = CompilerProxy.CreateParser(this.m_stCode).ParseST(this.NoAccess);
					this.m_parseTree = istatement;
					this.m_stCode = null;
				}
				if (istatement != null)
				{
					ICheckSumVisitor checkSumVisitor = CompilerProxy.CreateChecksumVisitor(false);
					istatement.Accept(checkSumVisitor.Traverser);
					this.m_crc = checkSumVisitor.Checksum;
				}
			}
			this.SetFlag(CompiledPOUFlags.SavePrecompile, false);
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35900)
			{
				this.SetFlag(CompiledPOUFlags.ToTypify, false);
			}
		}

		// Token: 0x06000BDF RID: 3039 RVA: 0x0001DD7C File Offset: 0x0001CD7C
		public override string[] GetSerializableValueNames(IArchiveVersionInfo info, IArchiveReporter reporter)
		{
			CompiledLibraryStorageFormat compiledLibraryStorageFormat = ArchiveStorageConfig.Singleton.StorageFormat as CompiledLibraryStorageFormat;
			CompileInfoStorageFormat compileInfoStorageFormat = ArchiveStorageConfig.Singleton.StorageFormat as CompileInfoStorageFormat;
			if (compiledLibraryStorageFormat == null && compileInfoStorageFormat == null && this.m_code == null)
			{
				return CompiledPOU.s_stPreCompileArchiveTags;
			}
			string[] array;
			if (info != null && info.GetTargetVersion(this) >= new Version(3, 5, 5, 30))
			{
				array = CompiledPOU.s_stArchiveTags_V3530.Concat(CompiledPOU.s_stArchiveTags_V35530).ToArray<string>();
			}
			else if (info != null && info.GetTargetVersion(this) >= new Version(3, 5, 3, 0))
			{
				array = CompiledPOU.s_stArchiveTags_V3530;
			}
			else
			{
				array = CompiledPOU.s_stArchiveTags;
			}
			if (compiledLibraryStorageFormat != null && compiledLibraryStorageFormat.ExcludeParseTreeInPOU)
			{
				array = array.Except(new string[]
				{
					"ParseTree"
				}).ToArray<string>();
			}
			return array;
		}

		// Token: 0x06000BE0 RID: 3040 RVA: 0x0001DE40 File Offset: 0x0001CE40
		public override object GetSerializableValue(string stValueName)
		{
			CompiledLibraryStorageFormat compiledLibraryStorageFormat = ArchiveStorageConfig.Singleton.StorageFormat as CompiledLibraryStorageFormat;
			if (stValueName == "ParseTree" && this.m_parseTree != null)
			{
				bool bDeleteComments = compiledLibraryStorageFormat != null && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35600 && !compiledLibraryStorageFormat.PreserveCompiledLibComments;
				return this.ParseTreeProvider.GetParseTreeForSerialization(bDeleteComments);
			}
			return base.GetSerializableValue(stValueName);
		}

		// Token: 0x170002EB RID: 747
		// (get) Token: 0x06000BE1 RID: 3041 RVA: 0x0001DEA7 File Offset: 0x0001CEA7
		public override string[] SerializableValueNames
		{
			get
			{
				return this.GetSerializableValueNames(null, null);
			}
		}

		// Token: 0x170002EC RID: 748
		// (get) Token: 0x06000BE2 RID: 3042 RVA: 0x0001DEB4 File Offset: 0x0001CEB4
		internal string[] ArchiveTags
		{
			get
			{
				if (!this.GetFlag(CompiledPOUFlags.Compiled) && !this.GetFlag(CompiledPOUFlags.SavePrecompile) && this.m_code == null)
				{
					return CompiledPOU.s_stPreCompileArchiveTags;
				}
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35530)
				{
					return CompiledPOU.s_stArchiveTags_V3530.Concat(CompiledPOU.s_stArchiveTags_V35530).ToArray<string>();
				}
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300)
				{
					return CompiledPOU.s_stArchiveTags_V3530;
				}
				return CompiledPOU.s_stArchiveTags;
			}
		}

		// Token: 0x170002ED RID: 749
		// (get) Token: 0x06000BE3 RID: 3043 RVA: 0x0001DF2B File Offset: 0x0001CF2B
		// (set) Token: 0x06000BE4 RID: 3044 RVA: 0x0001DF33 File Offset: 0x0001CF33
		public int NumberOfStatements
		{
			get
			{
				return this._nStatements;
			}
			set
			{
				this._nStatements = value;
			}
		}

		// Token: 0x170002EE RID: 750
		// (get) Token: 0x06000BE5 RID: 3045 RVA: 0x0001DF3C File Offset: 0x0001CF3C
		// (set) Token: 0x06000BE6 RID: 3046 RVA: 0x0001DF44 File Offset: 0x0001CF44
		public ILMCompiledParseTreeService CompiledParseTreeService { get; internal set; }

		// Token: 0x06000BE7 RID: 3047 RVA: 0x0001DF4D File Offset: 0x0001CF4D
		public override object CreateSerializableValue(string valueName, byte[] nesting, IArchiveReporter reporter)
		{
			if (valueName == "BitWriteAccesses")
			{
				return new LList<IBitWriteAccess>();
			}
			return base.CreateSerializableValue(valueName, nesting, reporter);
		}

		// Token: 0x040001EC RID: 492
		[Obfuscation(Feature = "rename")]
		private static readonly string[] s_stPreCompileArchiveTags = new string[]
		{
			"Name",
			"ObjectGuid",
			"SignatureId",
			"PreCompiledPOUFlags",
			"TimeStamp",
			"Breakpointlist"
		};

		// Token: 0x040001ED RID: 493
		[Obfuscation(Feature = "rename")]
		private static readonly string[] s_stArchiveTags = new string[]
		{
			"Name",
			"Code",
			"ParseTree",
			"ParseTreeEmpty",
			"ObjectGuid",
			"MessageGuid",
			"ParentObjectGuid",
			"SignatureId",
			"CompiledPOUFlags",
			"CompiledCode",
			"ScratchSize",
			"TimeStamp",
			"Checksum",
			"LibraryPath",
			"MaxParamSize",
			"Breakpointlist",
			"messages"
		};

		// Token: 0x040001EE RID: 494
		[Obfuscation(Feature = "rename")]
		private static readonly string[] s_stArchiveTags_V3530 = new string[]
		{
			"Name",
			"Code",
			"ParseTree",
			"ParseTreeEmpty",
			"ObjectGuid",
			"MessageGuid",
			"ParentObjectGuid",
			"SignatureId",
			"CompiledPOUFlags",
			"CompiledCode",
			"ScratchSize",
			"TimeStamp",
			"Checksum",
			"LibraryPath",
			"MaxParamSize",
			"Breakpointlist",
			"messages",
			"CodeGeneratorStackSize",
			"BitWriteAccesses",
			"trycatchfpadr",
			"trycatchcodeadr"
		};

		// Token: 0x040001EF RID: 495
		[Obfuscation(Feature = "rename")]
		private static readonly string[] s_stArchiveTags_V35530 = new string[]
		{
			"InternalCompiledPOUFlags",
			"pmessages"
		};

		// Token: 0x040001F0 RID: 496
		[DefaultSerialization("Name")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string m_stName = string.Empty;

		// Token: 0x040001F1 RID: 497
		private bool m_parseTreeEmpty;

		// Token: 0x040001F2 RID: 498
		[DefaultSerialization("ParseTree")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IStatement m_parseTree;

		// Token: 0x040001F3 RID: 499
		[DefaultSerialization("Code")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string m_stCode;

		// Token: 0x040001F4 RID: 500
		[DefaultSerialization("ObjectGuid")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private Guid m_objectGuid = Guid.Empty;

		// Token: 0x040001F5 RID: 501
		[DefaultSerialization("MessageGuid")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private Guid m_messageGuid = Guid.Empty;

		// Token: 0x040001F6 RID: 502
		[DefaultSerialization("ParentObjectGuid")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private Guid m_parentObjectGuid = Guid.Empty;

		// Token: 0x040001F7 RID: 503
		[DefaultSerialization("SignatureId")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_nSignId = Common.InvalidID;

		// Token: 0x040001F8 RID: 504
		[DefaultSerialization("CompiledPOUFlags")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private CompiledPOUFlags m_cpFlags;

		// Token: 0x040001F9 RID: 505
		[Obfuscation(Feature = "rename")]
		internal int _nStatements;

		// Token: 0x040001FA RID: 506
		[DefaultSerialization("InternalCompiledPOUFlags")]
		[StorageVersion("3.5.5.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private InternalCompiledPOUFlags m_cpFlagsInternal;

		// Token: 0x040001FB RID: 507
		[DefaultSerialization("CompiledCode")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private ICompiledCode m_code;

		// Token: 0x040001FC RID: 508
		[DefaultSerialization("ScratchSize")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_nScratchSize;

		// Token: 0x040001FD RID: 509
		[DefaultSerialization("TimeStamp")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private long m_lTimeStamp;

		// Token: 0x040001FE RID: 510
		[DefaultSerialization("Checksum")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private uint m_crc;

		// Token: 0x040001FF RID: 511
		[DefaultSerialization("LibraryPath")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string m_stLibraryPath;

		// Token: 0x04000200 RID: 512
		[DefaultSerialization("MaxParamSize")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_nMaxParamSize;

		// Token: 0x04000201 RID: 513
		[DefaultSerialization("Breakpointlist")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private IBreakpointList m_breakpointlist;

		// Token: 0x04000202 RID: 514
		[Obfuscation(Feature = "rename")]
		private _ICompilerMessage[] m_messages;

		// Token: 0x04000203 RID: 515
		[DefaultSerialization("pmessages")]
		[StorageVersion("3.5.5.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private _ICompilerMessage[] m_pmessages;

		// Token: 0x04000204 RID: 516
		[DefaultSerialization("CodeGeneratorStackSize")]
		[StorageVersion("3.4.4.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private int m_nCodeGeneratorStackSize;

		// Token: 0x04000205 RID: 517
		[DefaultSerialization("BitWriteAccesses")]
		[StorageVersion("3.5.3.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private LList<IBitWriteAccess> m_BitWriteAccesses;

		// Token: 0x04000206 RID: 518
		[DefaultSerialization("trycatchfpadr")]
		[StorageVersion("3.5.3.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private LList<IDataLocation> m_lTryCatchFPAddresses;

		// Token: 0x04000207 RID: 519
		[DefaultSerialization("trycatchcodeadr")]
		[StorageVersion("3.5.3.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private LList<int> m_lTryCatchCodeAddresses;

		// Token: 0x04000209 RID: 521
		private static readonly _IStatement s_EmptyStatement = LanguageModelBuilder.Singleton.CreateEmptyStatement();
	}
}
