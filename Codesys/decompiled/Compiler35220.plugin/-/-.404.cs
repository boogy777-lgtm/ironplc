using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using \u0003;
using \u0007;
using \u000E;
using \u000F;
using \u0014;
using \u0016;
using \u0019;
using \u001E;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.CompilerPhases;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0081;

namespace \u0004
{
	// Token: 0x02000404 RID: 1028
	internal sealed class \u001B
	{
		// Token: 0x17000990 RID: 2448
		// (get) Token: 0x060038FF RID: 14591 RVA: 0x000EB834 File Offset: 0x000E9A34
		// (set) Token: 0x06003900 RID: 14592 RVA: 0x000EB83C File Offset: 0x000E9A3C
		private global::\u000E.\u001B CompileInformation { get; set; }

		// Token: 0x17000991 RID: 2449
		// (get) Token: 0x06003901 RID: 14593 RVA: 0x000EB848 File Offset: 0x000E9A48
		public Guid ApplicationGuid
		{
			get
			{
				return this.CompileInformation.ApplicationGuid;
			}
		}

		// Token: 0x17000992 RID: 2450
		// (get) Token: 0x06003902 RID: 14594 RVA: 0x000EB858 File Offset: 0x000E9A58
		public Guid DeviceGuid
		{
			get
			{
				return this.CompileInformation.DeviceGuid;
			}
		}

		// Token: 0x17000993 RID: 2451
		// (get) Token: 0x06003903 RID: 14595 RVA: 0x000EB868 File Offset: 0x000E9A68
		public _ICompileContext ComconNew
		{
			get
			{
				return this.CompileInformation.ComconNew;
			}
		}

		// Token: 0x17000994 RID: 2452
		// (get) Token: 0x06003904 RID: 14596 RVA: 0x000EB878 File Offset: 0x000E9A78
		// (set) Token: 0x06003905 RID: 14597 RVA: 0x000EB888 File Offset: 0x000E9A88
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

		// Token: 0x17000995 RID: 2453
		// (get) Token: 0x06003906 RID: 14598 RVA: 0x000EB898 File Offset: 0x000E9A98
		public _ICompileContext ComconOld
		{
			get
			{
				return this.CompileInformation.ComconOld;
			}
		}

		// Token: 0x17000996 RID: 2454
		// (get) Token: 0x06003907 RID: 14599 RVA: 0x000EB8A8 File Offset: 0x000E9AA8
		// (set) Token: 0x06003908 RID: 14600 RVA: 0x000EB8B0 File Offset: 0x000E9AB0
		public _ICompileContext ComconLastCompile { get; set; }

		// Token: 0x17000997 RID: 2455
		// (get) Token: 0x06003909 RID: 14601 RVA: 0x000EB8BC File Offset: 0x000E9ABC
		public bool OnlineChange
		{
			get
			{
				return this.CompileInformation.OnlineChange;
			}
		}

		// Token: 0x17000998 RID: 2456
		// (get) Token: 0x0600390A RID: 14602 RVA: 0x000EB8CC File Offset: 0x000E9ACC
		public bool BootProject
		{
			get
			{
				return this.CompileInformation.BootProject;
			}
		}

		// Token: 0x17000999 RID: 2457
		// (get) Token: 0x0600390B RID: 14603 RVA: 0x000EB8DC File Offset: 0x000E9ADC
		public bool KeepCompileInformation
		{
			get
			{
				return this.CompileInformation.KeepCompileInformation;
			}
		}

		// Token: 0x1700099A RID: 2458
		// (get) Token: 0x0600390C RID: 14604 RVA: 0x000EB8EC File Offset: 0x000E9AEC
		public _IPreCompileContext Precomp
		{
			get
			{
				return this.CompileInformation.Precomp;
			}
		}

		// Token: 0x1700099B RID: 2459
		// (get) Token: 0x0600390D RID: 14605 RVA: 0x000EB8FC File Offset: 0x000E9AFC
		public _IPreCompileContext PrecompPool
		{
			get
			{
				return this.CompileInformation.PrecompPool;
			}
		}

		// Token: 0x1700099C RID: 2460
		// (get) Token: 0x0600390E RID: 14606 RVA: 0x000EB90C File Offset: 0x000E9B0C
		// (set) Token: 0x0600390F RID: 14607 RVA: 0x000EB914 File Offset: 0x000E9B14
		public bool ErrorsOccured { get; set; }

		// Token: 0x1700099D RID: 2461
		// (get) Token: 0x06003910 RID: 14608 RVA: 0x000EB920 File Offset: 0x000E9B20
		public IProgressCallback Callback
		{
			get
			{
				return this.CompileInformation.Callback;
			}
		}

		// Token: 0x1700099E RID: 2462
		// (get) Token: 0x06003911 RID: 14609 RVA: 0x000EB930 File Offset: 0x000E9B30
		public \u0081.\u0006 CompileContextCreator { get; }

		// Token: 0x06003912 RID: 14610 RVA: 0x000EB938 File Offset: 0x000E9B38
		internal \u001B(global::\u000E.\u001B \u008F\u0004)
		{
			this.CompileInformation = \u008F\u0004;
			this.CompileContextCreator = new \u0081.\u0006(\u008F\u0004);
		}

		// Token: 0x06003913 RID: 14611 RVA: 0x000EB954 File Offset: 0x000E9B54
		private bool \u0005()
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

		// Token: 0x06003914 RID: 14612 RVA: 0x000EBA44 File Offset: 0x000E9C44
		internal static bool \u0001(Guid \u0002, bool \u0003, bool \u0004, bool \u0005, out bool \u0006, out _ICompileContext \u0007, out _ICompileContext \u0008, IProgressCallback \u000E, bool \u000F, bool \u0010, bool \u0011)
		{
			global::\u000E.\u001B u001B = \u001E.\u001A.\u0001(\u0002, \u0003, \u0004, \u0010);
			global::\u0004.\u001B u001B2 = new global::\u0004.\u001B(u001B);
			u001B.Callback = \u000E;
			u001B.CheckAll = \u000F;
			u001B.\u0001();
			bool flag = u001B2.\u0001(\u0005, \u0011, \u000F);
			if (flag && !u001B2.UpToDate)
			{
				Locator.\u0001(u001B.ComconNew, u001B.ComconOld);
				u001B.CompilerPhase4_Typechecker.\u0004(false);
			}
			\u0006 = u001B2.UpToDate;
			\u0007 = u001B2.ComconNew;
			\u0008 = u001B2.ComconOld;
			u001B.\u0002();
			return flag && !u001B.CompilerPhase4_Typechecker.ErrorsOccured;
		}

		// Token: 0x06003915 RID: 14613 RVA: 0x000EBAE0 File Offset: 0x000E9CE0
		internal bool \u0001(bool \u0002, bool \u0003)
		{
			return this.\u0001(\u0002, \u0003, this.CompileInformation.CheckAll);
		}

		// Token: 0x1700099F RID: 2463
		// (get) Token: 0x06003916 RID: 14614 RVA: 0x000EBAF8 File Offset: 0x000E9CF8
		// (set) Token: 0x06003917 RID: 14615 RVA: 0x000EBB00 File Offset: 0x000E9D00
		internal bool UpToDate { get; set; }

		// Token: 0x170009A0 RID: 2464
		// (get) Token: 0x06003918 RID: 14616 RVA: 0x000EBB0C File Offset: 0x000E9D0C
		// (set) Token: 0x06003919 RID: 14617 RVA: 0x000EBB14 File Offset: 0x000E9D14
		private global::\u0014.\u0001 Timer { get; set; }

		// Token: 0x170009A1 RID: 2465
		// (get) Token: 0x0600391A RID: 14618 RVA: 0x000EBB20 File Offset: 0x000E9D20
		private _ILanguageModelManagerConsolidated LMM
		{
			get
			{
				return APEnvironmentFacade.Instance.LanguageModelMgr;
			}
		}

		// Token: 0x170009A2 RID: 2466
		// (get) Token: 0x0600391B RID: 14619 RVA: 0x000EBB2C File Offset: 0x000E9D2C
		private IMessageCategory CompilerMessageCategory
		{
			get
			{
				return APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory;
			}
		}

		// Token: 0x0600391C RID: 14620 RVA: 0x000EBB40 File Offset: 0x000E9D40
		private bool \u0001(bool \u0002, bool \u0003, bool \u0004)
		{
			this.Timer = new global::\u0014.\u0001(this.Precomp.IsDefined("debug_dump_times"));
			this.Timer.\u0001();
			APEnvironmentFacade.Instance.ClearMessages(this.CompilerMessageCategory);
			string u = string.Format(\u0081.\u0001.BuildStarted, this.LMM.GetApplicationNameByGuid(this.ApplicationGuid));
			_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, u, Severity.Text, MessageId.None);
			APEnvironmentFacade.Instance.AddMessage(this.CompilerMessageCategory, message);
			if (!this.LMM.RaiseAndCheckBeforeCompile(this.ApplicationGuid, this.CompilerMessageCategory))
			{
				return false;
			}
			this.LMM.DelayedLoader.CompleteLanguageModel(this.Callback);
			this.ErrorsOccured = false;
			this.Timer.\u0003("Zeit für OnBeforeCompile {0} ms");
			this.CompileInformation.Precomp = APEnvironmentFacade.Instance.LanguageModelMgr._GetPrecompileContext(this.ApplicationGuid);
			this.UpToDate = true;
			try
			{
				IDeviceIdentification deviceId = this.\u0001(this.CompileInformation.DeviceGuid);
				Guid parentApplication = this.LMM.ApplicationDeviceTable.GetParentApplication(this.ApplicationGuid);
				bool flag = true;
				if (this.\u0001(parentApplication, this.CompileInformation.DeviceGuid, ref flag))
				{
					return true;
				}
				if (this.\u0001(\u0002, ref flag))
				{
					this.CompileInformation.CompilerPhase4_Typechecker = CompilerPhase4_Typechecker.\u0001(this.CompileInformation);
					return true;
				}
				if (\u0003 && !this.\u0005())
				{
					return false;
				}
				this.UpToDate = false;
				ICodegenerator codegenerator = this.CompileInformation.CompilerPhase5_Codegenerator.\u0001();
				this.\u0002(codegenerator);
				IList<_ICompilerMessage> u2 = null;
				this.CompileContextCreator.\u0001(\u0004, out u2);
				if (Guid.Empty == this.ApplicationGuid && codegenerator == null && APEnvironmentFacade.Instance.CheckAllPoolObjectsConfigurationProviderOrNull != null)
				{
					this.ComconNew.DeviceSpecificProperties = APEnvironmentFacade.Instance.CheckAllPoolObjectsConfigurationProviderOrNull.DeviceSpecificProperties;
				}
				if (this.\u0001(u2))
				{
					return false;
				}
				this.ComconNew.TimeStampContext = this.Precomp.TimeStamp;
				this.ComconNew.TimeStampPool = this.LMM.Pool.TimeStamp;
				this.ComconNew.DeviceId = deviceId;
				this.ComconNew.ProjectChecksum = this.ComconNew.CalculateProjectChecksum();
				IPreCompileContext precompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(Guid.Empty);
				((_ICompileContext2)this.ComconNew).PrecompileContextNamesChecksum = Helper.\u0001(new _IPreCompileContext[]
				{
					this.Precomp,
					precompileContext as _IPreCompileContext2
				});
				u = \u0081.\u0001.TypifyAll;
				message = global::\u0019.\u0003.\u0001(null, u, Severity.Text, MessageId.None);
				APEnvironmentFacade.Instance.AddMessage(this.CompilerMessageCategory, message);
				if (!this.\u0001(codegenerator))
				{
					return false;
				}
				this.\u0003(\u0004);
				APEnvironmentFacade.Instance.LanguageModelMgr.Progress.NotifyNextTask(this.Callback, true, \u0081.\u0001.DoTypification, 0, \u0081.\u0001.POUUnit);
				this.CompileInformation.CompilerPhase1_Typifier.\u0001();
				this.ErrorsOccured = (this.ErrorsOccured || this.CompileInformation.CompilerPhase1_Typifier.ErrorsOccured);
				this.LMM[this.ApplicationGuid] = this.ComconNew;
			}
			catch (Exception u3)
			{
				this.ErrorsOccured = true;
				this.\u0001(u3);
			}
			finally
			{
				this.\u0001();
			}
			return !this.ErrorsOccured;
		}

		// Token: 0x0600391D RID: 14621 RVA: 0x000EBECC File Offset: 0x000EA0CC
		private bool \u0001(IList<_ICompilerMessage> \u0002)
		{
			if (\u0002.Count > 0)
			{
				foreach (_ICompilerMessage message in \u0002)
				{
					APEnvironmentFacade.Instance.AddMessage(this.CompilerMessageCategory, message);
				}
				return true;
			}
			return !global::\u0016.\u0014.\u0001(this.ComconNew, this.Precomp, this.CompilerMessageCategory);
		}

		// Token: 0x0600391E RID: 14622 RVA: 0x000EBF48 File Offset: 0x000EA148
		private bool \u0001(ICodegenerator \u0002)
		{
			if (\u0002 != null)
			{
				this.ComconNew.Codegenerator = \u0002;
				if (this.ComconNew.PointerSize == 8 && !global::\u0016.\u0014.\u0001(this.ComconNew, this.CompilerMessageCategory))
				{
					this.ErrorsOccured = true;
					return false;
				}
				this.\u0001(\u0002);
			}
			else
			{
				this.ComconNew.Define("AddrSize", (this.ComconNew.PointerSize == 8) ? "8" : "4", false);
			}
			return true;
		}

		// Token: 0x0600391F RID: 14623 RVA: 0x000EBFC4 File Offset: 0x000EA1C4
		private IDeviceIdentification \u0001(Guid \u0002)
		{
			IDeviceIdentification targetIdOfDevice;
			if (\u0002 == Guid.Empty)
			{
				targetIdOfDevice = this.LMM.ApplicationDeviceTable.GetTargetIdOfDevice(this.ApplicationGuid);
			}
			else
			{
				targetIdOfDevice = this.LMM.ApplicationDeviceTable.GetTargetIdOfDevice(\u0002);
			}
			ITargetSettings targetSettingsById = APEnvironmentFacade.Instance.GetTargetSettingsById(targetIdOfDevice);
			if ((targetSettingsById == null || targetSettingsById is IStubTargetSettings) && this.ApplicationGuid != Guid.Empty)
			{
				this.ErrorsOccured = true;
				if (!this.Precomp.SimulationMode)
				{
					_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, global::\u0003.\u0006.\u0001(MessageId.Err_DeviceNotInstalled, Array.Empty<object>()), Severity.Error, MessageId.Err_DeviceNotInstalled);
					APEnvironmentFacade.Instance.AddMessage(this.CompilerMessageCategory, message);
				}
				else
				{
					_ICompilerMessage message2 = global::\u0019.\u0003.\u0001(null, global::\u0003.\u0006.\u0001(MessageId.Wrn_DeviceNotInstalledForSimulation, Array.Empty<object>()), Severity.Warning, MessageId.Wrn_DeviceNotInstalledForSimulation);
					APEnvironmentFacade.Instance.AddMessage(this.CompilerMessageCategory, message2);
				}
			}
			this.ComconLastCompile = this.LMM[this.ApplicationGuid];
			if (this.ComconLastCompile != null && this.ComconLastCompile.SimulationMode != this.Precomp.SimulationMode)
			{
				this.ComconLastCompile = null;
			}
			this.CompileInformation.ComconOld = this.LMM.GetReferenceContextSynchronLoad(this.ApplicationGuid);
			if (this.ComconLastCompile != null && this.ComconLastCompile.DeviceId != null && this.ComconLastCompile.DeviceId.Id != targetIdOfDevice.Id)
			{
				this.LMM.RemoveCompileContext(this.ApplicationGuid);
				this.LMM.ClearDownloadContext(this.ApplicationGuid);
				this.CompileInformation.ComconOld = null;
				this.ComconLastCompile = null;
			}
			return targetIdOfDevice;
		}

		// Token: 0x06003920 RID: 14624 RVA: 0x000EC16C File Offset: 0x000EA36C
		private bool \u0001(bool \u0002, ref bool \u0003)
		{
			if (!\u0002 && this.ComconLastCompile != null && this.ComconLastCompile.IsUpToDate(this.Precomp, this.LMM.Pool, out \u0003))
			{
				string u = \u0081.\u0001.ProjectUpToDate;
				_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, u, Severity.Text, MessageId.None);
				APEnvironmentFacade.Instance.AddMessage(this.CompilerMessageCategory, message);
				this.CompileInformation.ComconNew = this.ComconLastCompile;
				return true;
			}
			return false;
		}

		// Token: 0x06003921 RID: 14625 RVA: 0x000EC1DC File Offset: 0x000EA3DC
		private bool \u0001(Guid \u0002, Guid \u0003, ref bool \u0004)
		{
			if (\u0002 != Guid.Empty)
			{
				this.ComconParent = this.LMM.GetReferenceContext(\u0002);
				if (this.ComconParent == null)
				{
					this.ComconParent = this.LMM[\u0003];
				}
				_IPreCompileContext ipreCompileContext = this.LMM._GetPrecompileContext(\u0002);
				if (this.ComconParent == null || !this.ComconParent.IsUpToDate(ipreCompileContext, this.LMM.Pool, out \u0004))
				{
					this.LMM.GenerateCode(\u0002, false, this.KeepCompileInformation);
					this.ComconParent = this.LMM[\u0002];
					if (!ipreCompileContext.IsEmpty())
					{
						this.LMM.RemoveCompileContext(this.ApplicationGuid);
						this.LMM.ClearDownloadContext(this.ApplicationGuid);
						this.CompileInformation.ComconOld = null;
						this.ComconLastCompile = null;
					}
				}
				if (this.ComconParent == null)
				{
					string u = global::\u0003.\u0006.\u0001(MessageId.Txt_ParentContextNotUpToDate, new object[]
					{
						this.LMM.GetApplicationNameByGuid(\u0002),
						this.LMM.GetApplicationNameByGuid(this.ApplicationGuid)
					});
					_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, u, Severity.Text, MessageId.Txt_ParentContextNotUpToDate);
					APEnvironmentFacade.Instance.AddMessage(this.CompilerMessageCategory, message);
					return true;
				}
			}
			return false;
		}

		// Token: 0x06003922 RID: 14626 RVA: 0x000EC310 File Offset: 0x000EA510
		private void \u0003(bool \u0002)
		{
			if (this.ApplicationGuid == Guid.Empty || \u0002)
			{
				this.ComconNew.Define("CheckAllPoolObjects", "", false);
				string libraryDevelopmentOptionsCompilerDefinesToUse = APEnvironmentFacade.Instance.LibraryDevelopmentOptionsCompilerDefinesToUse;
				if (!string.IsNullOrEmpty(libraryDevelopmentOptionsCompilerDefinesToUse))
				{
					foreach (string text in libraryDevelopmentOptionsCompilerDefinesToUse.Split(new char[]
					{
						','
					}))
					{
						this.ComconNew.Define(text.Trim(), "");
					}
				}
			}
		}

		// Token: 0x06003923 RID: 14627 RVA: 0x000EC394 File Offset: 0x000EA594
		private void \u0001(ICodegenerator \u0002)
		{
			\u0002.StartGeneration();
			\u0002.Initialize(new \u001E.\u000E(global::\u0007.\u0005.\u0001(this.ComconNew), false, this.ComconNew), global::\u0007.\u0005.\u0001(this.ComconNew));
			global::\u000F.\u0019.\u0001(this.ComconNew, \u0002 as ICodegenerator6);
			if (\u0002.MotorolaByteOrder)
			{
				this.ComconNew.Define("ByteOrder", "Motorola", false);
			}
			else
			{
				this.ComconNew.Define("ByteOrder", "Intel", false);
			}
			if (\u0002 is ICodegenerator4 && (\u0002 as ICodegenerator4).RegisterSize == 8)
			{
				this.ComconNew.Define("AddrSize", "8", false);
			}
			else
			{
				this.ComconNew.Define("AddrSize", "4", false);
			}
			this.ComconNew.Define("PackMode", this.ComconNew.DataManager.PackMode.ToString(), false);
			string name = \u0002.GetType().Name;
			this.ComconNew.Define("Platform", name, false);
		}

		// Token: 0x06003924 RID: 14628 RVA: 0x000EC4A4 File Offset: 0x000EA6A4
		private void \u0001()
		{
			this.CompileInformation.ComconOld = APEnvironmentFacade.Instance.LanguageModelMgr.GetReferenceContext(this.ApplicationGuid);
			ILateCompileContext latecompilecontext = global::\u0019.\u0003.Builder.CreateLateCompileContext(this.ComconNew, this.ComconOld);
			AfterCompileEventArgs3 e = new AfterCompileEventArgs3(this.ApplicationGuid, APEnvironmentFacade.Instance.MessageStorage.GetMessages(this.CompilerMessageCategory), this.ErrorsOccured, latecompilecontext, this.UpToDate);
			this.LMM.OnAfterCompile(e);
		}

		// Token: 0x06003925 RID: 14629 RVA: 0x000EC524 File Offset: 0x000EA724
		private void \u0001(Exception \u0002)
		{
			if (\u0002 is CancelledByUserException)
			{
				string u = \u0081.\u0002.Err_CancelledByUser;
				_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, u, Severity.Error, MessageId.Err_CancelledByUser);
				APEnvironmentFacade.Instance.AddMessage(this.CompilerMessageCategory, message);
				return;
			}
			if (\u0002 is OutOfMemoryException)
			{
				string u2 = \u0081.\u0002.Err_SystemOutOfMemory;
				_ICompilerMessage message2 = global::\u0019.\u0003.\u0001(null, u2, Severity.Error, MessageId.Err_SystemOutOfMemory);
				APEnvironmentFacade.Instance.AddMessage(this.CompilerMessageCategory, message2);
				return;
			}
			string u3 = "Internal error:" + \u0002.ToString();
			_ICompilerMessage message3 = global::\u0019.\u0003.\u0001(null, u3, Severity.Error, MessageId.None);
			APEnvironmentFacade.Instance.AddMessage(this.CompilerMessageCategory, message3);
		}

		// Token: 0x06003926 RID: 14630 RVA: 0x000EC5C0 File Offset: 0x000EA7C0
		public static ITargetSettings \u0001(Guid \u0002)
		{
			Guid guid = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceOfApplication(\u0002);
			if (guid == Guid.Empty)
			{
				guid = \u0002;
			}
			IDeviceIdentification targetIdOfDevice = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetTargetIdOfDevice(guid);
			return APEnvironmentFacade.Instance.GetTargetSettingsById(targetIdOfDevice);
		}

		// Token: 0x06003927 RID: 14631 RVA: 0x000EC614 File Offset: 0x000EA814
		public static IDeviceIdentification \u0001(Guid \u0002)
		{
			Guid guid = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceOfApplication(\u0002);
			if (guid == Guid.Empty)
			{
				guid = \u0002;
			}
			return APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetTargetIdOfDevice(guid);
		}

		// Token: 0x06003928 RID: 14632 RVA: 0x000EC65C File Offset: 0x000EA85C
		internal static bool \u0001(ITargetSettings \u0002)
		{
			return global::\u0016.\u0004.SimpleCycle.GetBoolValue(\u0002);
		}

		// Token: 0x06003929 RID: 14633 RVA: 0x000EC66C File Offset: 0x000EA86C
		private void \u0002(ICodegenerator \u0002)
		{
			try
			{
				global::\u0004.\u001B.\u0001(this.Precomp, \u0002);
			}
			catch (Exception ex)
			{
				string str = ex.ToString();
				Debug.\u0001(false, "Internal error in system library: " + str);
			}
		}

		// Token: 0x0600392A RID: 14634 RVA: 0x000EC6B0 File Offset: 0x000EA8B0
		private static void \u0001(_IPreCompileContext \u0002, ICodegenerator \u0003)
		{
			if (\u0002.TaskList.Count <= 0)
			{
				return;
			}
			ITargetSettings targetSettings = global::\u0004.\u001B.\u0001(\u0002.ApplicationGuid);
			if (global::\u0004.\u001B.\u0001(targetSettings))
			{
				return;
			}
			uint overriddenStackSize = APEnvironmentFacade.Instance.TaskStackSizeProvider.GetOverriddenStackSize(APEnvironmentFacade.Instance.PrimaryProjectHandle, \u0002.ApplicationGuid);
			int u = (overriddenStackSize == 0U) ? global::\u0016.\u0004.MaxStackSize.GetIntValue(targetSettings) : ((int)overriddenStackSize);
			global::\u0007.\u0001.\u0001(\u0002, u, \u0003 as ICodegenerator3);
			for (int i = 0; i < \u0002.TaskList.Count; i++)
			{
				ITaskInfo taskInfo = \u0002.TaskList[i];
				LStringBuilder lstringBuilder = new LStringBuilder();
				lstringBuilder.Append("{implicit on}");
				lstringBuilder.AppendLine("FUNCTION " + IdentifierConstants.GetCycleCode(taskInfo.TaskName) + ": BOOL");
				lstringBuilder.AppendLine("VAR_INPUT");
				lstringBuilder.AppendLine("\tptaskinfo: POINTER TO _IMPLICIT_TASK_INFO;");
				lstringBuilder.AppendLine("\tpapplicationinfo: POINTER TO _IMPLICIT_APPLICATION_INFO;");
				lstringBuilder.AppendLine("\thTaskInfo: POINTER TO BYTE;");
				lstringBuilder.AppendLine("END_VAR");
				lstringBuilder.Append("{implicit off}");
				_ISignature isignature = global::\u0019.\u0001.\u0001(lstringBuilder.ToString());
				Guid guid = Guid.Empty;
				_ISignature isignature2 = \u0002[isignature.Name];
				if (isignature2 != null)
				{
					guid = isignature2.ObjectGuid;
				}
				else
				{
					guid = Guid.NewGuid();
				}
				isignature.ObjectGuid = guid;
				isignature.SetFlag(SignatureFlag.Generated | SignatureFlag.TopLevel, true);
				\u0002.AddSignature(isignature, false);
				APEnvironmentFacade.Instance.LanguageModelMgr.AddRelatedObject(Guid.Empty, taskInfo.ObjectGuid, guid);
				_IStatement parseTree = global::\u0019.\u0001.\u0001(";").ParseST(false);
				_ICompiledPOU icompiledPOU = global::\u0019.\u0003.\u0001(isignature.Name);
				icompiledPOU.ObjectGuid = guid;
				icompiledPOU.SetParseTree(parseTree);
				icompiledPOU.SetFlag(CompiledPOUFlags.NotForUpToDate, true);
				icompiledPOU.SetFlagInternal(InternalCompiledPOUFlags.ImplicitInitialisationCodeAdded, true);
				\u0002.AddCompiledPOU(icompiledPOU, false);
			}
		}

		// Token: 0x04000B5B RID: 2907
		[CompilerGenerated]
		private global::\u000E.\u001B \u0001;

		// Token: 0x04000B5C RID: 2908
		[CompilerGenerated]
		private _ICompileContext \u0001;

		// Token: 0x04000B5D RID: 2909
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x04000B5E RID: 2910
		[CompilerGenerated]
		private readonly \u0081.\u0006 \u0001;

		// Token: 0x04000B5F RID: 2911
		internal const string \u0001 = "LanguageModelManager";

		// Token: 0x04000B60 RID: 2912
		private const string \u0002 = "WaitForDownloadInfoSaving";

		// Token: 0x04000B61 RID: 2913
		[CompilerGenerated]
		private bool \u0002;

		// Token: 0x04000B62 RID: 2914
		[CompilerGenerated]
		private global::\u0014.\u0001 \u0001;
	}
}
