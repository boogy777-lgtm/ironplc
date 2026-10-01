using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using \u000F;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0081;

namespace \u001D
{
	// Token: 0x020000E6 RID: 230
	internal sealed class \u0002
	{
		// Token: 0x1700049A RID: 1178
		// (get) Token: 0x06001017 RID: 4119 RVA: 0x0002CEB0 File Offset: 0x0002B0B0
		private _ICompileContext CompileContext { get; }

		// Token: 0x1700049B RID: 1179
		// (get) Token: 0x06001018 RID: 4120 RVA: 0x0002CEB8 File Offset: 0x0002B0B8
		private DownloadInfoFlags DownloadInfoFlags { get; }

		// Token: 0x1700049C RID: 1180
		// (get) Token: 0x06001019 RID: 4121 RVA: 0x0002CEC0 File Offset: 0x0002B0C0
		private bool OnlineChange
		{
			get
			{
				return this.DownloadInfoFlags.OnlineChange;
			}
		}

		// Token: 0x1700049D RID: 1181
		// (get) Token: 0x0600101A RID: 4122 RVA: 0x0002CEDC File Offset: 0x0002B0DC
		private PersistentDownloadInfoFactory PersistentDownloadInfoFactory { get; }

		// Token: 0x1700049E RID: 1182
		// (get) Token: 0x0600101B RID: 4123 RVA: 0x0002CEE4 File Offset: 0x0002B0E4
		private \u0081.\u0004 StandardCodePieceFactory { get; }

		// Token: 0x0600101C RID: 4124 RVA: 0x0002CEEC File Offset: 0x0002B0EC
		private \u0002(_ICompileContext \u0001\u0002, DownloadInfoFlags \u0018\u0005, int[] \u0019\u0005)
		{
			this.CompileContext = \u0001\u0002;
			this.DownloadInfoFlags = \u0018\u0005;
			this.PersistentDownloadInfoFactory = new PersistentDownloadInfoFactory(this.CompileContext, \u0018\u0005);
			this.StandardCodePieceFactory = new \u0081.\u0004(this.CompileContext, \u0018\u0005, \u0019\u0005);
		}

		// Token: 0x0600101D RID: 4125 RVA: 0x0002CF28 File Offset: 0x0002B128
		private IDownloadInfo \u0001()
		{
			DownloadInfo downloadInfo;
			if (!this.\u0001(out downloadInfo) && !this.\u0001(downloadInfo))
			{
				return null;
			}
			return downloadInfo;
		}

		// Token: 0x0600101E RID: 4126 RVA: 0x0002CF4C File Offset: 0x0002B14C
		internal static IDownloadInfo \u0001(_ICompileContext \u0002, DownloadInfoFlags \u0003, int[] \u0004)
		{
			return new \u001D.\u0002(\u0002, \u0003, \u0004).\u0001();
		}

		// Token: 0x0600101F RID: 4127 RVA: 0x0002CF5C File Offset: 0x0002B15C
		internal bool \u0001(out DownloadInfo \u0002)
		{
			_ICompileContext2 icompileContext = this.CompileContext as _ICompileContext2;
			bool result = false;
			if (icompileContext != null && this.OnlineChange)
			{
				if (icompileContext.StoredOnlineChangeDownloadInfo != null)
				{
					\u0002 = (icompileContext.StoredOnlineChangeDownloadInfo as DownloadInfo);
					result = true;
				}
				else
				{
					\u0002 = new DownloadInfo();
					icompileContext.StoredOnlineChangeDownloadInfo = \u0002;
				}
			}
			else
			{
				\u0002 = new DownloadInfo();
			}
			return result;
		}

		// Token: 0x06001020 RID: 4128 RVA: 0x0002CFB4 File Offset: 0x0002B1B4
		private bool \u0001(DownloadInfo \u0002)
		{
			this.\u0001(\u0002);
			IList<ICompiledPOU4> compiledPOUsToCompileEx = this.CompileContext.GetCompiledPOUsToCompileEx();
			if (!MemoryChecker.\u0001(this.CompileContext, compiledPOUsToCompileEx, this.DownloadInfoFlags.OnlineChange))
			{
				return false;
			}
			LList<ICodePiece2> llist = this.StandardCodePieceFactory.\u0001(compiledPOUsToCompileEx);
			IDataLocation u = this.PersistentDownloadInfoFactory.\u0001(llist);
			this.\u0001(\u0002, llist);
			this.\u0001(\u0002, u);
			\u0002.CodeId = this.CompileContext.CodeId;
			\u0002.DataId = this.CompileContext.DataId;
			ICodePiece[] codePieces = llist.ToArray();
			\u0002.CodePieces = codePieces;
			this.PersistentDownloadInfoFactory.\u0001(\u0002);
			\u0002.ExternalReferences = global::\u000F.\u0005.\u0001(this.CompileContext, this.DownloadInfoFlags.CompactDownload);
			\u0002.SystemApplicationReferences = Array.Empty<IExternalReference>();
			return true;
		}

		// Token: 0x06001021 RID: 4129 RVA: 0x0002D088 File Offset: 0x0002B288
		internal static IExternalReference[] \u0001(_ICompileContext \u0002, bool \u0003)
		{
			return global::\u000F.\u0005.\u0001(\u0002, \u0003);
		}

		// Token: 0x06001022 RID: 4130 RVA: 0x0002D094 File Offset: 0x0002B294
		private void \u0001(DownloadInfo \u0002, IDataLocation \u0003)
		{
			ISignature signature = this.CompileContext.GetSignature("__DataSegmentInfoVariables");
			if (signature != null)
			{
				IVariable variable = signature["dataSegments"];
				if (variable != null)
				{
					\u0002.SegmentInfoLocation = variable.DataLocation;
				}
			}
			ISignature signature2 = this.CompileContext["_Implicit_Target_Info_Variables"];
			if (signature2 != null)
			{
				IVariable variable2 = signature2["__targetInfo"];
				if (variable2 != null)
				{
					\u0002.TargetInformationLocation = variable2.DataLocation;
				}
			}
			ISignature signature3 = this.CompileContext.GetSignature("__ApplicationInfoVariables");
			if (signature3 != null)
			{
				IVariable variable3 = signature3["appContent"];
				if (variable3 != null)
				{
					\u0002.ApplicationInfoLocation = variable3.DataLocation;
				}
			}
			ISignature signature4 = this.CompileContext.GetSignature("__ApplicationCodeInfoVariables");
			if (signature4 != null)
			{
				IVariable variable4 = signature4["appCodeLocations"];
				if (variable4 != null)
				{
					\u0002.CodeLocationInfo = variable4.DataLocation;
				}
			}
			if (\u0003 != null)
			{
				\u0002.PersistentInitOnlyNewVariablesLocation = \u0003;
			}
		}

		// Token: 0x06001023 RID: 4131 RVA: 0x0002D174 File Offset: 0x0002B374
		private void \u0001(DownloadInfo \u0002, LList<ICodePiece2> \u0003)
		{
			if (this.CompileContext[IdentifierConstants.GlobalInitName] != null)
			{
				\u0002.GlobalInitPointerLocation = this.CompileContext[IdentifierConstants.GlobalInitName].FPDataLocation;
			}
			if (this.CompileContext[IdentifierConstants.GlobalExitName] != null)
			{
				\u0002.GlobalExitPointerLocation = this.CompileContext[IdentifierConstants.GlobalExitName].FPDataLocation;
			}
			if (this.CompileContext[IdentifierConstants.GlobalImplicitSignature] != null)
			{
				ICompiledCode compiledCode = this.CompileContext.GetCompiledPOUById(this.CompileContext[IdentifierConstants.GlobalImplicitSignature].Id).CompiledCode;
				\u0002.CodeInitLocation = this.CompileContext.GetCompiledPOUById(this.CompileContext[IdentifierConstants.GlobalImplicitSignature].Id).CompiledCode.Location;
				if (this.DownloadInfoFlags.BootProject && \u0003.Count > 100 && compiledCode.CodeSize < 100)
				{
					Debug.\u0001(false, "Boot project check failed! Please check if boot project runs correctly!");
				}
			}
			if (this.CompileContext[IdentifierConstants.RelocateCodeName] != null)
			{
				\u0002.RelocCodeLocation = this.CompileContext.GetCompiledPOUById(this.CompileContext[IdentifierConstants.RelocateCodeName].Id).CompiledCode.Location;
			}
			if (this.CompileContext[IdentifierConstants.GetOnlineChangeConcurrentPOUName(true)] != null)
			{
				\u0002.OnlineChangeConcurrentBefore = this.CompileContext.GetCompiledPOUById(this.CompileContext[IdentifierConstants.GetOnlineChangeConcurrentPOUName(true)].Id).CompiledCode.Location;
			}
			if (this.CompileContext[IdentifierConstants.GetOnlineChangeConcurrentPOUName(false)] != null)
			{
				\u0002.OnlineChangeConcurrentAfter = this.CompileContext.GetCompiledPOUById(this.CompileContext[IdentifierConstants.GetOnlineChangeConcurrentPOUName(false)].Id).CompiledCode.Location;
			}
			if (this.CompileContext[IdentifierConstants.OnlineChange1ConcurrentPOUName] != null)
			{
				\u0002.OnlineChange1Concurrent = this.CompileContext.GetCompiledPOUById(this.CompileContext[IdentifierConstants.OnlineChange1ConcurrentPOUName].Id).CompiledCode.Location;
			}
			if (this.CompileContext[IdentifierConstants.OnlineChange2RepeatablePOUName] != null)
			{
				\u0002.OnlineChange2Repeatable = this.CompileContext.GetCompiledPOUById(this.CompileContext[IdentifierConstants.OnlineChange2RepeatablePOUName].Id).CompiledCode.Location;
			}
			_ISignature isignature;
			if (this.DownloadInfoFlags.OnlineChange)
			{
				isignature = this.CompileContext[IdentifierConstants.OnlineChangePOUName];
			}
			else
			{
				isignature = this.CompileContext[IdentifierConstants.DownloadPOUName];
			}
			if (isignature != null)
			{
				\u0002.DownloadPOUPointerLocation = isignature.FPDataLocation;
				return;
			}
			\u0002.DownloadPOUPointerLocation = null;
		}

		// Token: 0x06001024 RID: 4132 RVA: 0x0002D410 File Offset: 0x0002B610
		private void \u0001(DownloadInfo \u0002)
		{
			IArea[] array = MemoryCompiler.\u0001(this.CompileContext.DataManager);
			IArea[] array2 = array;
			if (!this.DownloadInfoFlags.OnlineChange)
			{
				\u0002.Areas = MemoryCompiler.\u0001(this.CompileContext.DataManager);
				return;
			}
			\u0002.Areas = array2;
			if (\u0002.Areas.Length == 0 && this.CompileContext.OnlineChangeAreas != null && this.CompileContext.OnlineChangeAreas.Length != 0)
			{
				\u0002.Areas = this.CompileContext.OnlineChangeAreas;
				return;
			}
			this.CompileContext.OnlineChangeAreas = array2;
		}

		// Token: 0x040002DA RID: 730
		[CompilerGenerated]
		private readonly _ICompileContext \u0001;

		// Token: 0x040002DB RID: 731
		[CompilerGenerated]
		private readonly DownloadInfoFlags \u0001;

		// Token: 0x040002DC RID: 732
		[CompilerGenerated]
		private readonly PersistentDownloadInfoFactory \u0001;

		// Token: 0x040002DD RID: 733
		[CompilerGenerated]
		private readonly \u0081.\u0004 \u0001;
	}
}
