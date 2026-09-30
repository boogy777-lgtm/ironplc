using System;
using System.IO;
using System.Reflection;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000022 RID: 34
	[TypeGuid("{9acb4711-ae23-42e3-a6bc-58b75ab4215e}")]
	[StorageVersion("3.3.0.0")]
	public class Breakpoint : GenericObject2, _IBreakpoint, IBreakpoint3, IBreakpoint2, IBreakpoint, IBreakpoint4, IBreakpointSerializable
	{
		// Token: 0x17000030 RID: 48
		// (get) Token: 0x06000141 RID: 321 RVA: 0x00003F5D File Offset: 0x00002F5D
		// (set) Token: 0x06000142 RID: 322 RVA: 0x00003F65 File Offset: 0x00002F65
		public short Len
		{
			get
			{
				return this.m_sLen;
			}
			set
			{
				this.m_sLen = value;
			}
		}

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x06000143 RID: 323 RVA: 0x00003F70 File Offset: 0x00002F70
		// (set) Token: 0x06000144 RID: 324 RVA: 0x00003FBC File Offset: 0x00002FBC
		[DefaultSerialization("PositionToSave")]
		[StorageVersion("3.3.0.0")]
		public long PositionCombination
		{
			get
			{
				if (this.m_position == null)
				{
					return -1L;
				}
				return new IntegerUnion
				{
					m_long = this.m_position.EditorPosition,
					m_short3 = this.m_position.PositionOffset
				}.m_long;
			}
			set
			{
				if (value == -1L)
				{
					this.m_position = null;
					return;
				}
				long nPosition;
				short sPositionOffset;
				PositionHelper.SplitPosition(value, ref nPosition, ref sPositionOffset);
				this.m_position = MinimalPosition.CreateMinimalPosition(nPosition, sPositionOffset);
			}
		}

		// Token: 0x06000145 RID: 325 RVA: 0x00003FED File Offset: 0x00002FED
		public Breakpoint()
		{
		}

		// Token: 0x06000146 RID: 326 RVA: 0x00004003 File Offset: 0x00003003
		internal Breakpoint(int iOffset, IMinimalPosition sourcepos, short sLen)
		{
			this.m_iCodeOffset = iOffset;
			this.m_position = sourcepos;
			this.m_sLen = sLen;
			this.ExceptionHandlingSuccessor = -1;
		}

		// Token: 0x17000032 RID: 50
		// (get) Token: 0x06000147 RID: 327 RVA: 0x00004035 File Offset: 0x00003035
		// (set) Token: 0x06000148 RID: 328 RVA: 0x0000403D File Offset: 0x0000303D
		public int Offset
		{
			get
			{
				return this.m_iCodeOffset;
			}
			set
			{
				this.m_iCodeOffset = value;
			}
		}

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x06000149 RID: 329 RVA: 0x00004046 File Offset: 0x00003046
		// (set) Token: 0x0600014A RID: 330 RVA: 0x0000404E File Offset: 0x0000304E
		public int[] Successors
		{
			get
			{
				return this.m_iSuccessors;
			}
			set
			{
				this.m_iSuccessors = value;
			}
		}

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x0600014B RID: 331 RVA: 0x00004057 File Offset: 0x00003057
		// (set) Token: 0x0600014C RID: 332 RVA: 0x0000405F File Offset: 0x0000305F
		[DefaultSerialization("ExceptionHandlingSuccessor")]
		[StorageVersion("3.5.14.0")]
		[StorageDefaultValue(-1)]
		public int ExceptionHandlingSuccessor { get; set; } = -1;

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x0600014D RID: 333 RVA: 0x00004068 File Offset: 0x00003068
		// (set) Token: 0x0600014E RID: 334 RVA: 0x00004080 File Offset: 0x00003080
		public IStepInPosition[] StepInSuccessors
		{
			get
			{
				return this.m_StepInSuccessors;
			}
			set
			{
				if (value == null)
				{
					this.m_StepInSuccessors = null;
					return;
				}
				this.m_StepInSuccessors = new StepInPosition[value.Length];
				for (int i = 0; i < this.m_StepInSuccessors.Length; i++)
				{
					StepInPosition stepInPosition = new StepInPosition(value[i] as _IStepInPosition);
					this.m_StepInSuccessors[i] = stepInPosition;
				}
			}
		}

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x0600014F RID: 335 RVA: 0x000040D0 File Offset: 0x000030D0
		public ISourcePosition Position
		{
			get
			{
				if (this.m_position == null)
				{
					return new SourcePosition(-1, Guid.Empty, -1L, -1, 0);
				}
				return new SourcePosition(-1, Guid.Empty, this.m_position.EditorPosition, this.m_position.PositionOffset, this.m_sLen);
			}
		}

		// Token: 0x17000037 RID: 55
		// (get) Token: 0x06000150 RID: 336 RVA: 0x0000411C File Offset: 0x0000311C
		// (set) Token: 0x06000151 RID: 337 RVA: 0x00004124 File Offset: 0x00003124
		public IMinimalPosition _Position
		{
			get
			{
				return this.m_position;
			}
			set
			{
				this.m_position = value;
			}
		}

		// Token: 0x06000152 RID: 338 RVA: 0x00004130 File Offset: 0x00003130
		public void AddSuccessor(int nSucc)
		{
			if (nSucc < 0)
			{
				return;
			}
			int num = 0;
			if (this.m_iSuccessors != null)
			{
				num = this.m_iSuccessors.Length;
			}
			int[] array = new int[num + 1];
			if (this.m_iSuccessors != null)
			{
				this.m_iSuccessors.CopyTo(array, 0);
			}
			array[num] = nSucc;
			this.m_iSuccessors = array;
		}

		// Token: 0x06000153 RID: 339 RVA: 0x00004180 File Offset: 0x00003180
		public void AddSuccessors(params int[] nSucc)
		{
			int num = 0;
			if (this.m_iSuccessors != null)
			{
				num = this.m_iSuccessors.Length;
			}
			int[] array = new int[num + nSucc.Length];
			if (this.m_iSuccessors != null)
			{
				this.m_iSuccessors.CopyTo(array, 0);
			}
			nSucc.CopyTo(array, num);
			this.m_iSuccessors = array;
		}

		// Token: 0x06000154 RID: 340 RVA: 0x000041D0 File Offset: 0x000031D0
		public void AddStepInSuccessor(IStepInPosition sip)
		{
			if (this.m_StepInSuccessors != null)
			{
				foreach (StepInPosition stepInPosition in this.m_StepInSuccessors)
				{
					if (stepInPosition.SignatureId == sip.SignatureId && stepInPosition.KindOfCall == sip.KindOfCall && stepInPosition.StepOutBreakpoint == sip.StepOutBreakpoint)
					{
						return;
					}
				}
			}
			int num = 0;
			if (this.m_StepInSuccessors != null)
			{
				num = this.m_StepInSuccessors.Length;
			}
			StepInPosition[] array = new StepInPosition[num + 1];
			if (this.m_StepInSuccessors != null)
			{
				this.m_StepInSuccessors.CopyTo(array, 0);
			}
			array[num] = (sip as StepInPosition);
			this.m_StepInSuccessors = array;
		}

		// Token: 0x17000038 RID: 56
		// (get) Token: 0x06000155 RID: 341 RVA: 0x0000426E File Offset: 0x0000326E
		// (set) Token: 0x06000156 RID: 342 RVA: 0x00004276 File Offset: 0x00003276
		public int[] AssemblySuccessors
		{
			get
			{
				return this.m_iAssemblySuccessors;
			}
			set
			{
				this.m_iAssemblySuccessors = value;
			}
		}

		// Token: 0x06000157 RID: 343 RVA: 0x00004280 File Offset: 0x00003280
		public void AddAssemblySuccessor(int nSuccessorOffset)
		{
			if (nSuccessorOffset < 0)
			{
				return;
			}
			int num = 0;
			if (this.m_iAssemblySuccessors != null)
			{
				num = this.m_iAssemblySuccessors.Length;
			}
			int[] array = new int[num + 1];
			if (this.m_iAssemblySuccessors != null)
			{
				this.m_iAssemblySuccessors.CopyTo(array, 0);
			}
			array[num] = nSuccessorOffset;
			this.m_iAssemblySuccessors = array;
		}

		// Token: 0x17000039 RID: 57
		// (get) Token: 0x06000158 RID: 344 RVA: 0x000042CE File Offset: 0x000032CE
		// (set) Token: 0x06000159 RID: 345 RVA: 0x000042D6 File Offset: 0x000032D6
		public int AreaGPRegister
		{
			get
			{
				return this.m_nAreaGPRegister;
			}
			set
			{
				this.m_nAreaGPRegister = value;
			}
		}

		// Token: 0x1700003A RID: 58
		// (get) Token: 0x0600015A RID: 346 RVA: 0x000042DF File Offset: 0x000032DF
		// (set) Token: 0x0600015B RID: 347 RVA: 0x000042E7 File Offset: 0x000032E7
		public int OffsetGPRegister
		{
			get
			{
				return this.m_nOffsetGPRegister;
			}
			set
			{
				this.m_nOffsetGPRegister = value;
			}
		}

		// Token: 0x1700003B RID: 59
		// (get) Token: 0x0600015C RID: 348 RVA: 0x000042F0 File Offset: 0x000032F0
		// (set) Token: 0x0600015D RID: 349 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual short TryCatchId
		{
			get
			{
				return -1;
			}
			set
			{
			}
		}

		// Token: 0x0600015E RID: 350 RVA: 0x000042F4 File Offset: 0x000032F4
		public void Dump(StringWriter writer, IScope scope, int nIndex, int nCount)
		{
			if (this.Position == null)
			{
				writer.WriteLine("BP {0:D4}: CodeOffset: {1:X4}, SP-Pos: {2:D4}, SP-Offset: {3:D4}, Length: {4:D4}", new object[]
				{
					nIndex,
					this.m_iCodeOffset,
					0,
					0,
					0
				});
			}
			else
			{
				writer.WriteLine("BP {0:D4}: CodeOffset: {1:X4}, SP-Pos: {2:D4}, SP-Offset: {3:D4}, Length: {4:D4}", new object[]
				{
					nIndex,
					this.m_iCodeOffset,
					this.Position.Position,
					this.Position.PositionOffset,
					this.Position.Length
				});
			}
			if (this.m_iAssemblySuccessors == null)
			{
				writer.WriteLine("\tNo Assembly Successors");
			}
			else
			{
				writer.Write("\tAssembly Successors: ");
				for (int i = 0; i < this.m_iAssemblySuccessors.Length; i++)
				{
					if (i > 0)
					{
						writer.Write(", {0:X}", this.m_iAssemblySuccessors[i]);
					}
					else
					{
						writer.Write("{0:X}", this.m_iAssemblySuccessors[i]);
					}
				}
				writer.WriteLine();
			}
			if (this.m_iSuccessors == null)
			{
				writer.WriteLine("\tNo Successors");
			}
			else
			{
				writer.Write("\tSuccessors: ");
				for (int j = 0; j < this.m_iSuccessors.Length; j++)
				{
					if (j > 0)
					{
						writer.Write(", {0}", nCount - this.m_iSuccessors[j]);
					}
					else
					{
						writer.Write("{0}", nCount - this.m_iSuccessors[j]);
					}
				}
				writer.WriteLine();
			}
			if (this.m_StepInSuccessors == null)
			{
				writer.WriteLine("\tNo Step In");
				return;
			}
			writer.Write("\tStepInSuccessors: ");
			for (int k = 0; k < this.m_StepInSuccessors.Length; k++)
			{
				ISignature signature = scope[this.m_StepInSuccessors[k].SignatureId];
				string arg;
				if (signature == null)
				{
					arg = "Invalid Step In Id!";
				}
				else
				{
					arg = signature.OrgName;
					ISignature signature2 = scope[signature.ParentSignatureId];
					if (signature2 != null)
					{
						arg = signature2.OrgName + "." + signature.OrgName;
					}
				}
				if (k > 0)
				{
					writer.Write(", {0} ({1})", arg, this.m_StepInSuccessors[k].KindOfCall);
				}
				else
				{
					writer.Write("{0} ({1})", arg, this.m_StepInSuccessors[k].KindOfCall);
				}
			}
			writer.WriteLine();
		}

		// Token: 0x0400001D RID: 29
		[Obfuscation(Feature = "rename")]
		private IMinimalPosition m_position;

		// Token: 0x0400001E RID: 30
		[DefaultSerialization("Len")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private short m_sLen;

		// Token: 0x0400001F RID: 31
		[DefaultSerialization("CodeOffset")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_iCodeOffset;

		// Token: 0x04000020 RID: 32
		[DefaultSerialization("Successors")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int[] m_iSuccessors;

		// Token: 0x04000021 RID: 33
		[DefaultSerialization("StepIns")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private StepInPosition[] m_StepInSuccessors;

		// Token: 0x04000022 RID: 34
		[DefaultSerialization("AssSucc")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int[] m_iAssemblySuccessors;

		// Token: 0x04000023 RID: 35
		[DefaultSerialization("AreaGPRegister")]
		[StorageVersion("3.5.12.0")]
		[StorageDefaultValue(-1)]
		[Obfuscation(Feature = "rename")]
		private int m_nAreaGPRegister = -1;

		// Token: 0x04000024 RID: 36
		[DefaultSerialization("OffsetGPRegister")]
		[StorageVersion("3.5.12.0")]
		[StorageDefaultValue(0)]
		[Obfuscation(Feature = "rename")]
		private int m_nOffsetGPRegister;
	}
}
