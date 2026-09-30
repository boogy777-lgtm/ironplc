using System;
using System.Reflection;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200002C RID: 44
	[TypeGuid("{762f79d8-237e-4aec-8576-b032ad1bb941}")]
	[StorageVersion("3.3.0.0")]
	public class StepInPosition : GenericObject2, _IStepInPosition, IStepInPosition2, IStepInPosition, IStepInPositionSerializable
	{
		// Token: 0x060001E8 RID: 488 RVA: 0x00006BE6 File Offset: 0x00005BE6
		public StepInPosition()
		{
		}

		// Token: 0x060001E9 RID: 489 RVA: 0x00006C00 File Offset: 0x00005C00
		public StepInPosition(int nSignId, IBreakpoint bpStepOutBreakpoint)
		{
			this.m_iCalledSignatureId = nSignId;
			this.m_bpStepOutBreakpoint = bpStepOutBreakpoint;
		}

		// Token: 0x060001EA RID: 490 RVA: 0x00006C28 File Offset: 0x00005C28
		public StepInPosition(_IStepInPosition pos)
		{
			this.m_iCalledSignatureId = pos.SignatureId;
			this.m_kindofcall = pos.KindOfCall;
			this.m_bpStepOutBreakpoint = pos.StepOutBreakpoint;
			this.m_bpStepInBreakpoint = pos.StepInBreakpoint;
		}

		// Token: 0x1700005F RID: 95
		// (get) Token: 0x060001EB RID: 491 RVA: 0x00006C7D File Offset: 0x00005C7D
		// (set) Token: 0x060001EC RID: 492 RVA: 0x00006C85 File Offset: 0x00005C85
		public int SignatureId
		{
			get
			{
				return this.m_iCalledSignatureId;
			}
			set
			{
				this.m_iCalledSignatureId = value;
			}
		}

		// Token: 0x17000060 RID: 96
		// (get) Token: 0x060001ED RID: 493 RVA: 0x00006C8E File Offset: 0x00005C8E
		// (set) Token: 0x060001EE RID: 494 RVA: 0x00006C96 File Offset: 0x00005C96
		public KindOfCall KindOfCall
		{
			get
			{
				return this.m_kindofcall;
			}
			set
			{
				this.m_kindofcall = value;
			}
		}

		// Token: 0x17000061 RID: 97
		// (get) Token: 0x060001EF RID: 495 RVA: 0x00006C9F File Offset: 0x00005C9F
		// (set) Token: 0x060001F0 RID: 496 RVA: 0x00006CA7 File Offset: 0x00005CA7
		public IBreakpoint StepOutBreakpoint
		{
			get
			{
				return this.m_bpStepOutBreakpoint;
			}
			set
			{
				this.m_bpStepOutBreakpoint = value;
			}
		}

		// Token: 0x17000062 RID: 98
		// (get) Token: 0x060001F1 RID: 497 RVA: 0x00006CB0 File Offset: 0x00005CB0
		// (set) Token: 0x060001F2 RID: 498 RVA: 0x00006CB8 File Offset: 0x00005CB8
		public IBreakpoint StepInBreakpoint
		{
			get
			{
				return this.m_bpStepInBreakpoint;
			}
			set
			{
				this.m_bpStepInBreakpoint = value;
			}
		}

		// Token: 0x04000050 RID: 80
		[DefaultSerialization("SignId")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_iCalledSignatureId = Common.InvalidID;

		// Token: 0x04000051 RID: 81
		[DefaultSerialization("Flags")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private KindOfCall m_kindofcall = KindOfCall.None;

		// Token: 0x04000052 RID: 82
		[DefaultSerialization("StepOutBp")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private IBreakpoint m_bpStepOutBreakpoint;

		// Token: 0x04000053 RID: 83
		[DefaultSerialization("StepInBp")]
		[StorageVersion("3.5.13.0")]
		[StorageDefaultValue(null)]
		[Obfuscation(Feature = "rename")]
		private IBreakpoint m_bpStepInBreakpoint;
	}
}
