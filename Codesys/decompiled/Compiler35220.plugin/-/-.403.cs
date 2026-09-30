using System;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0003;
using \u000E;
using \u000F;
using \u0019;
using \u001E;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.ImplicitCode;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace \u0010
{
	// Token: 0x02000403 RID: 1027
	internal sealed class \u0013
	{
		// Token: 0x17000982 RID: 2434
		// (get) Token: 0x060038E9 RID: 14569 RVA: 0x000EB33C File Offset: 0x000E953C
		// (set) Token: 0x060038EA RID: 14570 RVA: 0x000EB344 File Offset: 0x000E9544
		private global::\u000E.\u001B CompileInformation { get; set; }

		// Token: 0x17000983 RID: 2435
		// (get) Token: 0x060038EB RID: 14571 RVA: 0x000EB350 File Offset: 0x000E9550
		public Guid ApplicationGuid
		{
			get
			{
				return this.CompileInformation.ApplicationGuid;
			}
		}

		// Token: 0x17000984 RID: 2436
		// (get) Token: 0x060038EC RID: 14572 RVA: 0x000EB360 File Offset: 0x000E9560
		public Guid DeviceGuid
		{
			get
			{
				return this.CompileInformation.DeviceGuid;
			}
		}

		// Token: 0x17000985 RID: 2437
		// (get) Token: 0x060038ED RID: 14573 RVA: 0x000EB370 File Offset: 0x000E9570
		// (set) Token: 0x060038EE RID: 14574 RVA: 0x000EB380 File Offset: 0x000E9580
		public _ICompileContext ComconNew
		{
			get
			{
				return this.CompileInformation.ComconNew;
			}
			set
			{
				this.CompileInformation.ComconNew = value;
			}
		}

		// Token: 0x17000986 RID: 2438
		// (get) Token: 0x060038EF RID: 14575 RVA: 0x000EB390 File Offset: 0x000E9590
		// (set) Token: 0x060038F0 RID: 14576 RVA: 0x000EB3A0 File Offset: 0x000E95A0
		public _ICompileContext ComconParent
		{
			get
			{
				return this.CompileInformation.ComconParent;
			}
			set
			{
				this.CompileInformation.ComconParent = value;
			}
		}

		// Token: 0x17000987 RID: 2439
		// (get) Token: 0x060038F1 RID: 14577 RVA: 0x000EB3B0 File Offset: 0x000E95B0
		public _ICompileContext ComconOld
		{
			get
			{
				return this.CompileInformation.ComconOld;
			}
		}

		// Token: 0x17000988 RID: 2440
		// (get) Token: 0x060038F2 RID: 14578 RVA: 0x000EB3C0 File Offset: 0x000E95C0
		// (set) Token: 0x060038F3 RID: 14579 RVA: 0x000EB3C8 File Offset: 0x000E95C8
		public _ICompileContext ComconLastCompile { get; set; }

		// Token: 0x17000989 RID: 2441
		// (get) Token: 0x060038F4 RID: 14580 RVA: 0x000EB3D4 File Offset: 0x000E95D4
		public bool OnlineChange
		{
			get
			{
				return this.CompileInformation.OnlineChange;
			}
		}

		// Token: 0x1700098A RID: 2442
		// (get) Token: 0x060038F5 RID: 14581 RVA: 0x000EB3E4 File Offset: 0x000E95E4
		public bool BootProject
		{
			get
			{
				return this.CompileInformation.BootProject;
			}
		}

		// Token: 0x1700098B RID: 2443
		// (get) Token: 0x060038F6 RID: 14582 RVA: 0x000EB3F4 File Offset: 0x000E95F4
		public bool KeepCompileInformation
		{
			get
			{
				return this.CompileInformation.KeepCompileInformation;
			}
		}

		// Token: 0x1700098C RID: 2444
		// (get) Token: 0x060038F7 RID: 14583 RVA: 0x000EB404 File Offset: 0x000E9604
		public _IPreCompileContext Precomp
		{
			get
			{
				return this.CompileInformation.Precomp;
			}
		}

		// Token: 0x1700098D RID: 2445
		// (get) Token: 0x060038F8 RID: 14584 RVA: 0x000EB414 File Offset: 0x000E9614
		public _IPreCompileContext PrecompPool
		{
			get
			{
				return this.CompileInformation.PrecompPool;
			}
		}

		// Token: 0x1700098E RID: 2446
		// (get) Token: 0x060038F9 RID: 14585 RVA: 0x000EB424 File Offset: 0x000E9624
		// (set) Token: 0x060038FA RID: 14586 RVA: 0x000EB42C File Offset: 0x000E962C
		public bool ErrorsOccured { get; set; }

		// Token: 0x1700098F RID: 2447
		// (get) Token: 0x060038FB RID: 14587 RVA: 0x000EB438 File Offset: 0x000E9638
		public IProgressCallback Callback
		{
			get
			{
				return this.CompileInformation.Callback;
			}
		}

		// Token: 0x060038FC RID: 14588 RVA: 0x000EB448 File Offset: 0x000E9648
		private \u0013(Guid \u0084\u0002, bool \u0014\u0004)
		{
			this.CompileInformation = \u001E.\u001A.\u0001(\u0084\u0002, false, true, \u0014\u0004);
		}

		// Token: 0x060038FD RID: 14589 RVA: 0x000EB460 File Offset: 0x000E9660
		public static _ICompileContext \u0001(Guid \u0002, _ICompileContext \u0003, IMessageCategory \u0004, IProgressCallback \u0005, out bool \u0006, bool \u0007)
		{
			\u0006 = false;
			_ICompileContext icompileContext;
			try
			{
				icompileContext = new global::\u0010.\u0013(\u0002, \u0007).\u0001(\u0004, \u0005, out \u0006);
			}
			catch
			{
				return null;
			}
			if (icompileContext == null)
			{
				return null;
			}
			icompileContext.DataId = \u0003.DataId;
			icompileContext.LastDataId = \u0003.DataId;
			icompileContext.CodeId = \u0003.CodeId;
			icompileContext.LastCodeId = \u0003.CodeId;
			return icompileContext;
		}

		// Token: 0x060038FE RID: 14590 RVA: 0x000EB4D4 File Offset: 0x000E96D4
		private _ICompileContext \u0001(IMessageCategory \u0002, IProgressCallback \u0003, out bool \u0004)
		{
			\u0004 = false;
			this.CompileInformation.\u0001();
			this.ComconNew = this.ComconOld.Duplicate();
			foreach (_ICompiledPOU icompiledPOU in this.ComconNew.CompiledPOUList.ToArray<_ICompiledPOU>())
			{
				if (icompiledPOU.Name.EndsWith("__TOREMOVE"))
				{
					this.ComconNew.RemoveCompiledPOU(icompiledPOU);
				}
				else
				{
					_ISignature isignature = this.ComconNew.GetSignatureById(icompiledPOU.SignatureId) as _ISignature;
					if (icompiledPOU.Name == "GLOBAL__EXIT__COPY" || icompiledPOU.GetFlagInternal(InternalCompiledPOUFlags.RemovedFromMemory) || (isignature != null && isignature.HasAttribute("RemovedAfterDownload")))
					{
						this.ComconNew.RemoveCompiledPOU(icompiledPOU);
					}
				}
			}
			ICodegenerator codegen = CompilerServicesInternal.\u0001(this.DeviceGuid, this.ApplicationGuid, this.ComconNew.SimulationMode, this.KeepCompileInformation);
			Codegeneration codegeneration = new Codegeneration(this.ComconNew, this.ComconNew, true, true, codegen);
			_ISignature isignature2 = this.ComconNew.GetSignature(IdentifierConstants.GlobalImplicitSignature) as _ISignature;
			_ICompiledPOU icompiledPOU2 = this.ComconNew.GetCompiledPOUById(isignature2.Id) as _ICompiledPOU;
			this.ComconNew.RemoveSignature(isignature2);
			if (icompiledPOU2 != null)
			{
				this.ComconNew.RemoveCompiledPOU(icompiledPOU2);
			}
			_ISignature isignature3 = this.ComconNew.GetSignature(IdentifierConstants.RelocateCodeName) as _ISignature;
			_ICompiledPOU icompiledPOU3 = this.ComconNew.GetCompiledPOUById(isignature3.Id) as _ICompiledPOU;
			this.ComconNew.RemoveSignature(isignature3);
			if (icompiledPOU3 != null)
			{
				this.ComconNew.RemoveCompiledPOU(icompiledPOU3);
			}
			_ISignature isignature4 = this.ComconNew.GetSignature("__GLOBAL_RELOC_DEFINITIONS") as _ISignature;
			_ICompiledPOU icompiledPOU4 = this.ComconNew.GetCompiledPOUById(isignature4.Id) as _ICompiledPOU;
			this.ComconNew.RemoveSignature(isignature4);
			if (icompiledPOU4 != null)
			{
				this.ComconNew.RemoveCompiledPOU(icompiledPOU4);
			}
			string u = \u0081.\u0001.GenerateCodeInit;
			_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, u, Severity.Text, MessageId.None);
			APEnvironmentFacade.Instance.AddMessage(\u0002, message);
			_ISignature isignature5 = null;
			if (this.ComconNew.ConcurrentOnlineChange)
			{
				isignature5 = CodeInitGenerator.\u0001(this.ComconNew, false);
			}
			_ICompiledPOU icompiledPOU5 = CodeInitGenerator.\u0001(this.ComconNew, false, isignature5 != null, this.ComconOld, out isignature5, true);
			codegeneration.\u0003(icompiledPOU5);
			ushort maxValue = ushort.MaxValue;
			int u2 = -1;
			if (!MemoryCompiler.\u0003(this.ComconNew.DataManager, ref maxValue, ref u2, this.ComconNew.DataManager.PackMode, icompiledPOU5.CompiledCode.CodeSize, this.ComconNew.DataManager._MemorySettings.CodeSegmentSize, DataSegmentFlags.Code))
			{
				u = global::\u0003.\u0006.\u0001(MessageId.Err_OutOfCodeMemory, new object[]
				{
					icompiledPOU5.Name,
					icompiledPOU5.CompiledCode.CodeSize
				});
				message = global::\u0019.\u0003.\u0001(null, u, Severity.Error, MessageId.Err_OutOfCodeMemory);
				APEnvironmentFacade.Instance.AddMessage(\u0002, message);
			}
			else
			{
				icompiledPOU5.CompiledCode.Location = global::\u0019.\u0003.\u0001(maxValue, u2);
			}
			icompiledPOU5.SetFlag(CompiledPOUFlags.ToCompile | CompiledPOUFlags.ToRemoveAfterDownload, true);
			icompiledPOU5.SetFlag(CompiledPOUFlags.BootProjectRelevant, true);
			this.ComconNew.AddCompiledPOU(icompiledPOU5, isignature5, true, null);
			global::\u000F.\u001A u001A = new global::\u000F.\u001A(this.ComconNew, false, true, codegeneration);
			u001A.\u0001(\u0003);
			\u0004 = u001A.AllocationError;
			this.CompileInformation.\u0002();
			return this.ComconNew;
		}

		// Token: 0x04000B58 RID: 2904
		[CompilerGenerated]
		private global::\u000E.\u001B \u0001;

		// Token: 0x04000B59 RID: 2905
		[CompilerGenerated]
		private _ICompileContext \u0001;

		// Token: 0x04000B5A RID: 2906
		[CompilerGenerated]
		private bool \u0001;
	}
}
