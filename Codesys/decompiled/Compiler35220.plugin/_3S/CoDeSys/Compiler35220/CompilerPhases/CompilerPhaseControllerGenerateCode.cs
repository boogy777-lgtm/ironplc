using System;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0003;
using \u000E;
using \u0012;
using \u0014;
using \u0016;
using \u0017;
using \u0019;
using \u001E;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u007F;
using \u0081;
using \u0084;

namespace _3S.CoDeSys.Compiler35220.CompilerPhases
{
	// Token: 0x02000405 RID: 1029
	internal sealed class CompilerPhaseControllerGenerateCode
	{
		// Token: 0x170009A3 RID: 2467
		// (get) Token: 0x0600392B RID: 14635 RVA: 0x000EC88C File Offset: 0x000EAA8C
		// (set) Token: 0x0600392C RID: 14636 RVA: 0x000EC894 File Offset: 0x000EAA94
		private global::\u000E.\u001B CompileInformation { get; set; }

		// Token: 0x170009A4 RID: 2468
		// (get) Token: 0x0600392D RID: 14637 RVA: 0x000EC8A0 File Offset: 0x000EAAA0
		// (set) Token: 0x0600392E RID: 14638 RVA: 0x000EC8A8 File Offset: 0x000EAAA8
		private _ILanguageModelManagerConsolidated LMM { get; set; }

		// Token: 0x170009A5 RID: 2469
		// (get) Token: 0x0600392F RID: 14639 RVA: 0x000EC8B4 File Offset: 0x000EAAB4
		private IOnlineChangeDetails OnlineChangeDetails
		{
			get
			{
				return this.CompileInformation.OnlineChangeDetails;
			}
		}

		// Token: 0x170009A6 RID: 2470
		// (get) Token: 0x06003930 RID: 14640 RVA: 0x000EC8C4 File Offset: 0x000EAAC4
		// (set) Token: 0x06003931 RID: 14641 RVA: 0x000EC8CC File Offset: 0x000EAACC
		private IMessage[] Errors { get; set; }

		// Token: 0x170009A7 RID: 2471
		// (get) Token: 0x06003932 RID: 14642 RVA: 0x000EC8D8 File Offset: 0x000EAAD8
		// (set) Token: 0x06003933 RID: 14643 RVA: 0x000EC8E0 File Offset: 0x000EAAE0
		private IMessage[] Warnings { get; set; }

		// Token: 0x170009A8 RID: 2472
		// (get) Token: 0x06003934 RID: 14644 RVA: 0x000EC8EC File Offset: 0x000EAAEC
		// (set) Token: 0x06003935 RID: 14645 RVA: 0x000EC8F4 File Offset: 0x000EAAF4
		private bool AllOk { get; set; }

		// Token: 0x170009A9 RID: 2473
		// (get) Token: 0x06003936 RID: 14646 RVA: 0x000EC900 File Offset: 0x000EAB00
		// (set) Token: 0x06003937 RID: 14647 RVA: 0x000EC908 File Offset: 0x000EAB08
		private bool OnlineChangePossible { get; set; }

		// Token: 0x170009AA RID: 2474
		// (get) Token: 0x06003938 RID: 14648 RVA: 0x000EC914 File Offset: 0x000EAB14
		// (set) Token: 0x06003939 RID: 14649 RVA: 0x000EC91C File Offset: 0x000EAB1C
		private bool FastOnlineChange { get; set; }

		// Token: 0x170009AB RID: 2475
		// (get) Token: 0x0600393A RID: 14650 RVA: 0x000EC928 File Offset: 0x000EAB28
		// (set) Token: 0x0600393B RID: 14651 RVA: 0x000EC930 File Offset: 0x000EAB30
		private bool UpToDate { get; set; }

		// Token: 0x170009AC RID: 2476
		// (get) Token: 0x0600393C RID: 14652 RVA: 0x000EC93C File Offset: 0x000EAB3C
		// (set) Token: 0x0600393D RID: 14653 RVA: 0x000EC944 File Offset: 0x000EAB44
		private bool CompileForFullDownload { get; set; }

		// Token: 0x170009AD RID: 2477
		// (get) Token: 0x0600393E RID: 14654 RVA: 0x000EC950 File Offset: 0x000EAB50
		internal Guid ApplicationGuid
		{
			get
			{
				return this.CompileInformation.ApplicationGuid;
			}
		}

		// Token: 0x170009AE RID: 2478
		// (get) Token: 0x0600393F RID: 14655 RVA: 0x000EC960 File Offset: 0x000EAB60
		internal bool OnlineChange
		{
			get
			{
				return this.CompileInformation.OnlineChange;
			}
		}

		// Token: 0x170009AF RID: 2479
		// (get) Token: 0x06003940 RID: 14656 RVA: 0x000EC970 File Offset: 0x000EAB70
		internal bool BootProject
		{
			get
			{
				return this.CompileInformation.BootProject;
			}
		}

		// Token: 0x170009B0 RID: 2480
		// (get) Token: 0x06003941 RID: 14657 RVA: 0x000EC980 File Offset: 0x000EAB80
		internal bool KeepCompileInformation
		{
			get
			{
				return this.CompileInformation.KeepCompileInformation;
			}
		}

		// Token: 0x170009B1 RID: 2481
		// (get) Token: 0x06003942 RID: 14658 RVA: 0x000EC990 File Offset: 0x000EAB90
		internal _IPreCompileContext Precomp
		{
			get
			{
				return this.CompileInformation.Precomp;
			}
		}

		// Token: 0x170009B2 RID: 2482
		// (get) Token: 0x06003943 RID: 14659 RVA: 0x000EC9A0 File Offset: 0x000EABA0
		internal _ICompileContext ComconNew
		{
			get
			{
				return this.CompileInformation.ComconNew;
			}
		}

		// Token: 0x170009B3 RID: 2483
		// (get) Token: 0x06003944 RID: 14660 RVA: 0x000EC9B0 File Offset: 0x000EABB0
		internal _ICompileContext ComconOld
		{
			get
			{
				return this.CompileInformation.ComconOld;
			}
		}

		// Token: 0x170009B4 RID: 2484
		// (get) Token: 0x06003945 RID: 14661 RVA: 0x000EC9C0 File Offset: 0x000EABC0
		internal IProgressCallback Callback
		{
			get
			{
				return this.CompileInformation.Callback;
			}
		}

		// Token: 0x170009B5 RID: 2485
		// (get) Token: 0x06003946 RID: 14662 RVA: 0x000EC9D0 File Offset: 0x000EABD0
		internal bool CheckAll
		{
			get
			{
				return this.CompileInformation.CheckAll;
			}
		}

		// Token: 0x170009B6 RID: 2486
		// (get) Token: 0x06003947 RID: 14663 RVA: 0x000EC9E0 File Offset: 0x000EABE0
		internal global::\u0014.\u0001 Timer
		{
			get
			{
				return this.CompileInformation.Timer;
			}
		}

		// Token: 0x170009B7 RID: 2487
		// (get) Token: 0x06003948 RID: 14664 RVA: 0x000EC9F0 File Offset: 0x000EABF0
		internal global::\u0017.\u0016 FastOnlineChanger { get; }

		// Token: 0x06003949 RID: 14665 RVA: 0x000EC9F8 File Offset: 0x000EABF8
		private CompilerPhaseControllerGenerateCode(Guid guidApplication, bool bOnlineChange, bool bBootProject, bool bKeepCompileInformation)
		{
			this.CompileInformation = \u001E.\u001A.\u0001(guidApplication, bOnlineChange, bBootProject, bKeepCompileInformation);
			this.FastOnlineChanger = new global::\u0017.\u0016(this.CompileInformation);
		}

		// Token: 0x0600394A RID: 14666 RVA: 0x000ECA24 File Offset: 0x000EAC24
		internal static bool \u0001(Guid \u0002, bool \u0003, bool \u0004, bool \u0005, out IOnlineChangeDetails \u0006, out IMessage[] \u0007, out IMessage[] \u0008)
		{
			CompilerPhaseControllerGenerateCode compilerPhaseControllerGenerateCode = new CompilerPhaseControllerGenerateCode(\u0002, \u0003, \u0004, \u0005);
			bool result = compilerPhaseControllerGenerateCode.\u000F();
			\u0006 = compilerPhaseControllerGenerateCode.OnlineChangeDetails;
			\u0007 = compilerPhaseControllerGenerateCode.Errors;
			\u0008 = compilerPhaseControllerGenerateCode.Warnings;
			return result;
		}

		// Token: 0x170009B8 RID: 2488
		// (get) Token: 0x0600394B RID: 14667 RVA: 0x000ECA5C File Offset: 0x000EAC5C
		private static IMessageCategory MessageCategory
		{
			get
			{
				return APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory;
			}
		}

		// Token: 0x0600394C RID: 14668 RVA: 0x000ECA70 File Offset: 0x000EAC70
		private bool \u000F()
		{
			this.AllOk = true;
			bool flag = false;
			try
			{
				this.Timer.\u0001("GenerateCode");
				this.CompileInformation.\u0001();
				bool flag2;
				this.\u0001(out flag, out flag2);
				bool? flag3 = this.\u0001(flag, flag2);
				if (flag3 != null)
				{
					return flag3.Value;
				}
				this.\u0001();
				if (!this.\u0002(flag2))
				{
					APEnvironmentFacade.Instance.LanguageModelMgr.DoOutput(this.ComconNew);
					this.AllOk = false;
					return false;
				}
				if (this.ComconNew == null)
				{
					this.AllOk = false;
					return false;
				}
				this.ComconNew.ContainsCode = true;
				if (!this.\u0012())
				{
					Messages.\u0001(this.ComconNew, APEnvironmentFacade.Instance.MessageStorage, CompilerPhaseControllerGenerateCode.MessageCategory);
					this.AllOk = false;
					return false;
				}
				if (this.OnlineChange && !CompilerServicesInternal.\u0001(this.ComconNew))
				{
					Messages.\u0001(this.ComconNew, APEnvironmentFacade.Instance.MessageStorage, CompilerPhaseControllerGenerateCode.MessageCategory);
					this.AllOk = false;
					return false;
				}
				this.\u0002();
			}
			catch (Exception u)
			{
				this.AllOk = this.\u0001(u);
			}
			finally
			{
				this.CompileInformation.\u0002();
				this.Timer.\u0004("GenerateCode");
				this.\u0003();
				this.\u0007(!flag);
			}
			return this.AllOk;
		}

		// Token: 0x0600394D RID: 14669 RVA: 0x000ECC10 File Offset: 0x000EAE10
		private void \u0001()
		{
			if (!this.OnlineChangePossible)
			{
				this.LMM.ClearDownloadContext(this.CompileInformation.ApplicationGuid);
				this.CompileInformation.ComconOld = null;
			}
			if (this.ComconOld != null && this.ComconOld.DefineChanged(this.Precomp))
			{
				foreach (_ICompiledPOU icompiledPOU in this.Precomp.AllCompiledPOUs)
				{
					icompiledPOU.UpdateTimeStamp();
				}
			}
		}

		// Token: 0x0600394E RID: 14670 RVA: 0x000ECCA4 File Offset: 0x000EAEA4
		private bool? \u0001(bool \u0002, bool \u0003)
		{
			bool? result = this.\u0001(\u0003);
			if (result == null && this.\u0010())
			{
				return new bool?(false);
			}
			if (this.\u0001(\u0002))
			{
				return new bool?(true);
			}
			return result;
		}

		// Token: 0x0600394F RID: 14671 RVA: 0x000ECCE4 File Offset: 0x000EAEE4
		private bool \u0001(bool \u0002)
		{
			bool result = false;
			if (\u0002)
			{
				if (this.CompileInformation.ComconOldOriginal != null)
				{
					this.LMM[this.CompileInformation.ApplicationGuid] = this.CompileInformation.ComconOldOriginal;
				}
				string u = \u0081.\u0001.ProjectUpToDate;
				_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, u, Severity.Text, MessageId.None);
				APEnvironmentFacade.Instance.AddMessage(CompilerPhaseControllerGenerateCode.MessageCategory, message);
				result = true;
			}
			return result;
		}

		// Token: 0x06003950 RID: 14672 RVA: 0x000ECD48 File Offset: 0x000EAF48
		private bool \u0010()
		{
			if (this.CompileInformation.OnlineChange && !this.OnlineChangePossible)
			{
				string u = global::\u0003.\u0006.\u0001(MessageId.Err_NoOnlineChangePossible, Array.Empty<object>());
				_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, u, Severity.Error, MessageId.Err_NoOnlineChangePossible);
				APEnvironmentFacade.Instance.AddMessage(CompilerPhaseControllerGenerateCode.MessageCategory, message);
				this.AllOk = false;
				return true;
			}
			return false;
		}

		// Token: 0x06003951 RID: 14673 RVA: 0x000ECDA4 File Offset: 0x000EAFA4
		private static bool \u0001()
		{
			IOEMCustomization oemcustomization = APEnvironmentFacade.Instance.OEMCustomization;
			return oemcustomization != null && oemcustomization.HasValue("LanguageModelManager", "SuppressProgressUpdate") && oemcustomization.GetBoolValue("LanguageModelManager", "SuppressProgressUpdate");
		}

		// Token: 0x06003952 RID: 14674 RVA: 0x000ECDE4 File Offset: 0x000EAFE4
		private void \u0001(out bool \u0002, out bool \u0003)
		{
			this.LMM = APEnvironmentFacade.Instance.LanguageModelMgr;
			APEnvironmentFacade.Instance.PrecompileChecker.Disable();
			this.CompileInformation.Timer.\u0001("BeginGenerateCode");
			APEnvironmentFacade.Instance.ClearMessages(CompilerPhaseControllerGenerateCode.MessageCategory);
			APEnvironmentFacade.Instance.LanguageModelMgr.Progress.StartLengthyOperation();
			if (CompilerPhaseControllerGenerateCode.\u0001())
			{
				this.CompileInformation.Callback = null;
			}
			else
			{
				this.CompileInformation.Callback = APEnvironmentFacade.Instance.LanguageModelMgr.Progress.Callback;
			}
			APEnvironmentFacade.Instance.LanguageModelMgr.Progress.NotifyNextTask(this.CompileInformation.Callback, true, \u0081.\u0001.GenerateCodeProgress, 0, null);
			this.CompileInformation.ComconNew = this.LMM[this.CompileInformation.ApplicationGuid];
			this.OnlineChangePossible = true;
			this.FastOnlineChange = false;
			this.CompileInformation.CheckAll = false;
			bool u = true;
			bool u2 = false;
			this.UpToDate = (this.CompileInformation.ComconOld != null && this.ComconOld.IsUpToDate(this.Precomp, this.LMM.Pool, out u, out u2));
			this.OnlineChangePossible = u;
			this.FastOnlineChange = u2;
			this.CompileForFullDownload = (!this.OnlineChange && this.ComconOld != null && this.ComconOld.ContainsOnlineChangeCode);
			\u0002 = (this.UpToDate && !this.CompileForFullDownload);
			this.\u0006(!\u0002);
			if (this.ComconOld != null)
			{
				foreach (_ICompiledPOU icompiledPOU in this.ComconOld.GetAllCompiledPOUsEx().OfType<_ICompiledPOU>())
				{
					icompiledPOU.SetFlag(CompiledPOUFlags.ToTypify, false);
				}
			}
			\u0003 = (!this.FastOnlineChange || this.CompileForFullDownload || !this.OnlineChange || this.UpToDate);
		}

		// Token: 0x06003953 RID: 14675 RVA: 0x000ECFE4 File Offset: 0x000EB1E4
		private bool? \u0001(bool \u0002)
		{
			this.Timer.Output = this.Precomp.IsDefined("debug_dump_times");
			this.Timer.\u0001("FastOnlineChange");
			bool? result = this.\u0002(\u0002);
			this.Timer.\u0004("FastOnlineChange");
			return result;
		}

		// Token: 0x06003954 RID: 14676 RVA: 0x000ED034 File Offset: 0x000EB234
		private bool? \u0002(bool \u0002)
		{
			if (\u0002)
			{
				return null;
			}
			if (!this.\u0011())
			{
				return new bool?(false);
			}
			IOnlineChangeDetails u;
			bool? flag = this.FastOnlineChanger.\u0001(CompilerPhaseControllerGenerateCode.MessageCategory, this.Callback, out u);
			bool? flag2 = flag;
			bool flag3 = true;
			if (flag2.GetValueOrDefault() == flag3 & flag2 != null)
			{
				this.CompileInformation.OnlineChangeDetails = u;
				this.CompileInformation.ComconNew = APEnvironmentFacade.Instance.LanguageModelMgr[this.ApplicationGuid];
				this.ComconNew.CompiledSymbolTables = new \u0084.\u0007(this.ComconNew);
				this.CompileInformation.CompilerPhase5_Codegenerator.\u0002();
				this.AllOk = (Messages.\u0001(this.ComconNew, APEnvironmentFacade.Instance.MessageStorage, CompilerPhaseControllerGenerateCode.MessageCategory) && this.AllOk);
				if (this.AllOk && this.OnlineChange)
				{
					this.CompileInformation.CompilerPhase6_AfterCodegeneration.\u0003();
				}
				this.ComconNew.ResetExprementHashTables();
				IOEMCustomization oemcustomization = APEnvironmentFacade.Instance.OEMCustomization;
				if (oemcustomization == null || !oemcustomization.HasValue("LanguageModelManager", "DisableChecksumComputationForFastOnlineChange"))
				{
					this.CompileInformation.CompilerPhase6_AfterCodegeneration.\u0004();
				}
				this.ComconNew.ProjectChecksum = this.ComconNew.CalculateProjectChecksum();
				this.ComconNew.bFastOnlineChange = true;
				return new bool?(this.AllOk);
			}
			flag2 = flag;
			flag3 = false;
			if (flag2.GetValueOrDefault() == flag3 & flag2 != null)
			{
				this.AllOk = false;
				return new bool?(false);
			}
			if (this.ComconNew != null)
			{
				this.ComconNew.bFastOnlineChange = false;
			}
			return null;
		}

		// Token: 0x06003955 RID: 14677 RVA: 0x000ED1E0 File Offset: 0x000EB3E0
		private void \u0006(bool \u0002)
		{
			if (\u0002 && this.\u0001(this.Precomp.GetTargetSettings()))
			{
				this.CompileInformation.ComconOld = null;
				this.CompileInformation.OnlineChange = false;
				this.FastOnlineChange = false;
			}
		}

		// Token: 0x06003956 RID: 14678 RVA: 0x000ED218 File Offset: 0x000EB418
		private bool \u0001(ITargetSettings \u0002)
		{
			bool boolValue = global::\u0016.\u0004.CompactDownload.GetBoolValue(\u0002);
			bool boolValue2 = global::\u0016.\u0004.MinimalSystem.GetBoolValue(\u0002);
			bool flag = boolValue || boolValue2;
			if (flag)
			{
				APEnvironmentFacade.Instance.LanguageModelMgr.ForceRebuildAll(this.ApplicationGuid);
				APEnvironmentFacade.Instance.LanguageModelMgr.ClearDownloadContext(this.ApplicationGuid);
			}
			return flag;
		}

		// Token: 0x06003957 RID: 14679 RVA: 0x000ED26C File Offset: 0x000EB46C
		private bool \u0011()
		{
			ILMCommandService3 ilmcommandService = APEnvironmentFacade.Instance.LMServiceProvider.CommandService as ILMCommandService3;
			if (ilmcommandService != null && ilmcommandService.IsAsyncUpdateDownloadInfoInProgress(this.ApplicationGuid))
			{
				CompilerServicesInternal.\u0001 u = CompilerServicesInternal.\u0001.\u0001;
				IOEMCustomization oemcustomization = APEnvironmentFacade.Instance.OEMCustomization;
				if (oemcustomization != null && oemcustomization.HasValue("LanguageModelManager", "WaitForDownloadInfoSaving"))
				{
					u = (CompilerServicesInternal.\u0001)oemcustomization.GetIntValue("LanguageModelManager", "WaitForDownloadInfoSaving");
				}
				switch (u)
				{
				case CompilerServicesInternal.\u0001.\u0002:
				{
					IProgressCallback progressCallback = this.Callback;
					if (progressCallback != null)
					{
						progressCallback.NextTask(\u0081.\u0001.WaitForDownloadInfoSaving, 0, null);
					}
					ilmcommandService.WaitForAsyncUpdateDownloadInfoCompleted(this.ApplicationGuid);
					IProgressCallback progressCallback2 = this.Callback;
					if (progressCallback2 != null)
					{
						progressCallback2.NextTask(\u0081.\u0001.Build, 0, null);
					}
					break;
				}
				case CompilerServicesInternal.\u0001.\u0003:
				{
					_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, \u0081.\u0002.Err_CancelledByUser, Severity.Error, MessageId.Err_CancelledByUser);
					APEnvironmentFacade.Instance.AddMessage(APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory, message);
					return false;
				}
				}
			}
			return true;
		}

		// Token: 0x06003958 RID: 14680 RVA: 0x000ED35C File Offset: 0x000EB55C
		private bool \u0002(bool \u0002)
		{
			this.Timer.\u0001("Compile_Phase");
			bool flag = this.ComconNew != null && this.ComconNew.ContainsCode;
			bool flag2 = this.ComconNew != null && this.ComconNew.ContainsOnlineChangeCode != this.OnlineChange;
			bool u = flag || flag2;
			bool result = this.CompileInformation.CompilerPhaseControllerCompile.\u0001(u, \u0002);
			this.Timer.\u0004("Compile_Phase");
			return result;
		}

		// Token: 0x06003959 RID: 14681 RVA: 0x000ED3D8 File Offset: 0x000EB5D8
		private bool \u0012()
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.Progress.NotifyNextTask(this.Callback, true, \u0081.\u0001.DoLocation, 0, null);
			this.ComconNew.DSFCallback = CompilerPhaseControllerGenerateCode.\u0001(this.ComconNew);
			this.Timer.\u0001("Location");
			bool result = this.CompileInformation.CompilerPhase3_Locator.\u0001();
			this.Timer.\u0004("Location");
			return result;
		}

		// Token: 0x0600395A RID: 14682 RVA: 0x000ED450 File Offset: 0x000EB650
		private static IMemoryAllocationCallback \u0001(_ICompileContext \u0002)
		{
			Guid guid = Guid.Empty;
			try
			{
				string stringValue = global::\u0016.\u0004.MemoryAllocationCallback.GetStringValue(\u0002.GetTargetSettings());
				guid = new Guid(stringValue);
				if (guid == Guid.Empty)
				{
					return null;
				}
				return APEnvironmentFacade.Instance.TryCreateMemoryAllocationCallback(guid);
			}
			catch (TypeNotFoundException)
			{
				string text = global::\u0003.\u0006.\u0001(MessageId.Err_NoMemoryAllocationCallback, Array.Empty<object>());
				text = new \u007F.\u0012(guid).\u0001(text);
				_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, text, Severity.FatalError, MessageId.Err_NoMemoryAllocationCallback);
				APEnvironmentFacade.Instance.AddMessage(CompilerPhaseControllerGenerateCode.MessageCategory, message);
			}
			catch
			{
			}
			return null;
		}

		// Token: 0x0600395B RID: 14683 RVA: 0x000ED4FC File Offset: 0x000EB6FC
		private void \u0002()
		{
			this.Timer.\u0001("Codegeneration");
			ICodegenerator codegenerator = this.CompileInformation.CompilerPhase5_Codegenerator.\u0001();
			if (codegenerator != null)
			{
				this.CompileInformation.CompilerPhase5_Codegenerator.\u0001(codegenerator);
				bool u = this.AllOk;
				this.CompileInformation.CompilerPhase6_AfterCodegeneration.\u0001(codegenerator, ref u);
				this.AllOk = u;
			}
			else
			{
				Guid u0080_u = this.CompileInformation.CompilerPhase5_Codegenerator.\u0002();
				string text = global::\u0003.\u0006.\u0001(MessageId.Err_NoCodegenerator, Array.Empty<object>());
				text = new global::\u0012.\u0015(u0080_u).\u0001(text);
				_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, text, Severity.FatalError, MessageId.Err_NoCodegenerator);
				APEnvironmentFacade.Instance.AddMessage(CompilerPhaseControllerGenerateCode.MessageCategory, message);
			}
			this.Timer.\u0004("Codegeneration");
		}

		// Token: 0x0600395C RID: 14684 RVA: 0x000ED5BC File Offset: 0x000EB7BC
		private void \u0007(bool \u0002)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.Progress.EndLengthyOperation();
			bool flag;
			int num = this.\u0001(out flag);
			this.AllOk = (this.AllOk && num == 0);
			if (!this.AllOk)
			{
				APEnvironmentFacade.Instance.LanguageModelMgr[this.ApplicationGuid] = null;
			}
			else if (APEnvironmentFacade.Instance.LanguageModelMgr.GetReferenceContext(this.ApplicationGuid) == null)
			{
				APEnvironmentFacade.Instance.LanguageModelMgr.SetReferenceContext(this.ApplicationGuid, this.ComconNew, true);
			}
			_ILanguageModelManagerConsolidated2 ilanguageModelManagerConsolidated = APEnvironmentFacade.Instance.LanguageModelMgr as _ILanguageModelManagerConsolidated2;
			if (ilanguageModelManagerConsolidated != null)
			{
				if (flag)
				{
					ilanguageModelManagerConsolidated.StoreCompileContextWithStackOverflow(this.ApplicationGuid, this.ComconNew);
				}
				else
				{
					ilanguageModelManagerConsolidated.RemoveCompiledApplicationSetWithStackoverflow(this.ApplicationGuid);
				}
			}
			if (!this.AllOk || this.ComconNew == null)
			{
				AfterUnsuccessfullGenerateCodeEventArgs e = new AfterUnsuccessfullGenerateCodeEventArgs(this.ApplicationGuid, this.Errors, true);
				ILMCompileService3 ilmcompileService = APEnvironmentFacade.Instance.LMServiceProvider.CompileService as ILMCompileService3;
				if (ilmcompileService != null)
				{
					ilmcompileService.OnAfterUnsuccessfullGenerateCode(e);
				}
			}
			else
			{
				AfterGenerateCodeEventArgs e2 = new AfterGenerateCodeEventArgs(this.ApplicationGuid);
				APEnvironmentFacade.Instance.LanguageModelMgr.OnAfterGenerateCode(e2);
				this.\u0008(\u0002);
			}
			APEnvironmentFacade.Instance.PrecompileChecker.Enable();
			APEnvironmentFacade.Instance.PrecompileChecker.TryStart();
		}

		// Token: 0x0600395D RID: 14685 RVA: 0x000ED710 File Offset: 0x000EB910
		private void \u0008(bool \u0002)
		{
			if (\u0002)
			{
				bool flag = !this.KeepCompileInformation;
				foreach (_ICompiledPOU icompiledPOU in this.ComconNew.CompiledPOUList)
				{
					if (!this.KeepCompileInformation)
					{
						icompiledPOU.SetParseTree(null);
					}
					if (flag && icompiledPOU.CompiledCode != null && !typeof(_ICompiledCodeStub).IsAssignableFrom(icompiledPOU.CompiledCode.GetType()))
					{
						icompiledPOU.CompiledCode = global::\u0019.\u0003.\u0001(icompiledPOU.CompiledCode);
					}
				}
			}
		}

		// Token: 0x0600395E RID: 14686 RVA: 0x000ED7B0 File Offset: 0x000EB9B0
		private int \u0001(out bool \u0002)
		{
			\u0002 = false;
			int num = 0;
			int num2 = 0;
			this.Errors = null;
			this.Warnings = null;
			IMessage[] messages = APEnvironmentFacade.Instance.GetMessages(CompilerPhaseControllerGenerateCode.MessageCategory, Severity.FatalError);
			if (messages == null || messages.Length == 0)
			{
				this.Errors = APEnvironmentFacade.Instance.GetMessages(CompilerPhaseControllerGenerateCode.MessageCategory, Severity.Error);
				this.Warnings = APEnvironmentFacade.Instance.GetMessages(CompilerPhaseControllerGenerateCode.MessageCategory, Severity.Warning);
			}
			else
			{
				this.Errors = messages;
			}
			if (this.Errors != null)
			{
				num = this.Errors.Length;
			}
			if (this.Warnings != null)
			{
				num2 = this.Warnings.Length;
			}
			string format;
			if (num > 0)
			{
				format = \u0081.\u0001.BuildCompleteErrors;
				\u0002 = this.\u0013();
			}
			else
			{
				format = \u0081.\u0001.BuildCompleteOK;
			}
			string u = string.Format(format, num, num2);
			_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, u, Severity.Text, MessageId.None);
			APEnvironmentFacade.Instance.AddMessage(CompilerPhaseControllerGenerateCode.MessageCategory, message);
			return num;
		}

		// Token: 0x0600395F RID: 14687 RVA: 0x000ED890 File Offset: 0x000EBA90
		private bool \u0013()
		{
			if (this.Errors == null)
			{
				return false;
			}
			int num = this.Errors.OfType<_ICompilerMessage>().Where(new Func<_ICompilerMessage, bool>(CompilerPhaseControllerGenerateCode.<>c.<>9.\u0001)).Count<_ICompilerMessage>();
			return this.Errors.Length == num;
		}

		// Token: 0x06003960 RID: 14688 RVA: 0x000ED8E8 File Offset: 0x000EBAE8
		private bool \u0001(Exception \u0002)
		{
			if (this.ComconNew != null)
			{
				Messages.\u0001(this.ComconNew, APEnvironmentFacade.Instance.MessageStorage, CompilerPhaseControllerGenerateCode.MessageCategory);
			}
			if (\u0002 is CancelledByUserException)
			{
				string u = \u0081.\u0002.Err_CancelledByUser;
				_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, u, Severity.Error, MessageId.Err_CancelledByUser);
				APEnvironmentFacade.Instance.AddMessage(CompilerPhaseControllerGenerateCode.MessageCategory, message);
			}
			else if (\u0002 is OutOfMemoryException)
			{
				string u = \u0081.\u0002.Err_SystemOutOfMemory;
				_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, u, Severity.Error, MessageId.Err_SystemOutOfMemory);
				APEnvironmentFacade.Instance.AddMessage(CompilerPhaseControllerGenerateCode.MessageCategory, message);
			}
			else if (!(\u0002 is LateCompileErrorException))
			{
				string u = "Internal error:" + \u0002.ToString();
				_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, u, Severity.Error, MessageId.None);
				APEnvironmentFacade.Instance.AddMessage(CompilerPhaseControllerGenerateCode.MessageCategory, message);
			}
			bool result = false;
			APEnvironmentFacade.Instance.LanguageModelMgr[this.ApplicationGuid] = null;
			return result;
		}

		// Token: 0x06003961 RID: 14689 RVA: 0x000ED9C0 File Offset: 0x000EBBC0
		private void \u0003()
		{
			if (this.Timer.Output)
			{
				this.Timer.\u0002("Location", "Zeit zum Lokieren und add late language model {0} ms");
				this.Timer.\u0002("FastOnlineChange", "Zeit fuer FastOnlineChange: {0} ms");
				this.Timer.\u0002("Compile_Phase", "Zeit fuer Typify_Phase: {0} ms");
				this.Timer.\u0002("GenerateSpecialPOUs", "Zeit zum code generieren Sonder POUs {0} ms");
				this.Timer.\u0002("GlobalInit", "Zeit Global Init {0} ms");
				this.Timer.\u0002("GlobalExit", "Zeit für Global Exit {0} ms");
				this.Timer.\u0002("CodeInit", "Zeit Code Init {0} ms");
				this.Timer.\u0002("RelocationPou", "Zeit Relocate POU {0} ms");
				this.Timer.\u0002("Codegeneration", "Zeit Codegeneration {0} ms");
				this.Timer.\u0002("GenerateCode", "Gesamtzeit {0} ms");
			}
		}

		// Token: 0x04000B63 RID: 2915
		[CompilerGenerated]
		private global::\u000E.\u001B \u0001;

		// Token: 0x04000B64 RID: 2916
		[CompilerGenerated]
		private _ILanguageModelManagerConsolidated \u0001;

		// Token: 0x04000B65 RID: 2917
		[CompilerGenerated]
		private IMessage[] \u0001;

		// Token: 0x04000B66 RID: 2918
		[CompilerGenerated]
		private IMessage[] \u0002;

		// Token: 0x04000B67 RID: 2919
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x04000B68 RID: 2920
		[CompilerGenerated]
		private bool \u0002;

		// Token: 0x04000B69 RID: 2921
		[CompilerGenerated]
		private bool \u0003;

		// Token: 0x04000B6A RID: 2922
		[CompilerGenerated]
		private bool \u0004;

		// Token: 0x04000B6B RID: 2923
		[CompilerGenerated]
		private bool \u0005;

		// Token: 0x04000B6C RID: 2924
		[CompilerGenerated]
		private readonly global::\u0017.\u0016 \u0001;

		// Token: 0x04000B6D RID: 2925
		private const string \u0001 = "DisableChecksumComputationForFastOnlineChange";

		// Token: 0x04000B6E RID: 2926
		private const string \u0002 = "LanguageModelManager";

		// Token: 0x04000B6F RID: 2927
		private const string \u0003 = "WaitForDownloadInfoSaving";
	}
}
