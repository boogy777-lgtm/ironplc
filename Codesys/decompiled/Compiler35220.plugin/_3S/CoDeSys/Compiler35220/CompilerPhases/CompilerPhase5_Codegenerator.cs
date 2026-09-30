using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using \u0003;
using \u0004;
using \u0006;
using \u0007;
using \u000E;
using \u000F;
using \u0012;
using \u0014;
using \u0016;
using \u0018;
using \u0019;
using \u001C;
using \u001D;
using \u001E;
using _3S.CoDeSys.Compiler35220.Features;
using _3S.CoDeSys.Compiler35220.ImplicitCode;
using _3S.CoDeSys.Compiler35220.InitialisationCode;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.OnlineChange;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase2_AfterTypification;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0080;
using \u0081;
using \u0082;
using \u0084;

namespace _3S.CoDeSys.Compiler35220.CompilerPhases
{
	// Token: 0x020003FF RID: 1023
	internal sealed class CompilerPhase5_Codegenerator
	{
		// Token: 0x1700096C RID: 2412
		// (get) Token: 0x0600389D RID: 14493 RVA: 0x000E87AC File Offset: 0x000E69AC
		// (set) Token: 0x0600389E RID: 14494 RVA: 0x000E87B4 File Offset: 0x000E69B4
		private global::\u000E.\u001B CompileInformation { get; set; }

		// Token: 0x1700096D RID: 2413
		// (get) Token: 0x0600389F RID: 14495 RVA: 0x000E87C0 File Offset: 0x000E69C0
		private Guid ApplicationGuid
		{
			get
			{
				return this.CompileInformation.ApplicationGuid;
			}
		}

		// Token: 0x1700096E RID: 2414
		// (get) Token: 0x060038A0 RID: 14496 RVA: 0x000E87D0 File Offset: 0x000E69D0
		private bool OnlineChange
		{
			get
			{
				return this.CompileInformation.OnlineChange;
			}
		}

		// Token: 0x1700096F RID: 2415
		// (get) Token: 0x060038A1 RID: 14497 RVA: 0x000E87E0 File Offset: 0x000E69E0
		private bool BootProject
		{
			get
			{
				return this.CompileInformation.BootProject;
			}
		}

		// Token: 0x17000970 RID: 2416
		// (get) Token: 0x060038A2 RID: 14498 RVA: 0x000E87F0 File Offset: 0x000E69F0
		private bool KeepCompileInformation
		{
			get
			{
				return this.CompileInformation.KeepCompileInformation;
			}
		}

		// Token: 0x17000971 RID: 2417
		// (get) Token: 0x060038A3 RID: 14499 RVA: 0x000E8800 File Offset: 0x000E6A00
		private _ICompileContext ComconNew
		{
			get
			{
				return this.CompileInformation.ComconNew;
			}
		}

		// Token: 0x17000972 RID: 2418
		// (get) Token: 0x060038A4 RID: 14500 RVA: 0x000E8810 File Offset: 0x000E6A10
		private _ICompileContext ComconOld
		{
			get
			{
				return this.CompileInformation.ComconOld;
			}
		}

		// Token: 0x17000973 RID: 2419
		// (get) Token: 0x060038A5 RID: 14501 RVA: 0x000E8820 File Offset: 0x000E6A20
		private IProgressCallback Callback
		{
			get
			{
				return this.CompileInformation.Callback;
			}
		}

		// Token: 0x17000974 RID: 2420
		// (get) Token: 0x060038A6 RID: 14502 RVA: 0x000E8830 File Offset: 0x000E6A30
		private global::\u0014.\u0001 Timer
		{
			get
			{
				return this.CompileInformation.Timer;
			}
		}

		// Token: 0x17000975 RID: 2421
		// (get) Token: 0x060038A7 RID: 14503 RVA: 0x000E8840 File Offset: 0x000E6A40
		private static IMessageCategory MessageCategory
		{
			get
			{
				return APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory;
			}
		}

		// Token: 0x17000976 RID: 2422
		// (get) Token: 0x060038A8 RID: 14504 RVA: 0x000E8854 File Offset: 0x000E6A54
		private bool InterfacesChanged
		{
			get
			{
				return this.CompileInformation.InterfacesChanged;
			}
		}

		// Token: 0x17000977 RID: 2423
		// (get) Token: 0x060038A9 RID: 14505 RVA: 0x000E8864 File Offset: 0x000E6A64
		private bool CodeChanged
		{
			get
			{
				return this.CompileInformation.CodeChanged;
			}
		}

		// Token: 0x17000978 RID: 2424
		// (get) Token: 0x060038AA RID: 14506 RVA: 0x000E8874 File Offset: 0x000E6A74
		private bool InitValuesChanged
		{
			get
			{
				return this.CompileInformation.InitValuesChanged;
			}
		}

		// Token: 0x17000979 RID: 2425
		// (get) Token: 0x060038AB RID: 14507 RVA: 0x000E8884 File Offset: 0x000E6A84
		private IOnlineChangeDetails OnlineChangeDetails
		{
			get
			{
				return this.CompileInformation.OnlineChangeDetails;
			}
		}

		// Token: 0x060038AC RID: 14508 RVA: 0x000E8894 File Offset: 0x000E6A94
		public CompilerPhase5_Codegenerator(global::\u000E.\u001B ci)
		{
			this.CompileInformation = ci;
		}

		// Token: 0x060038AD RID: 14509 RVA: 0x000E88A4 File Offset: 0x000E6AA4
		internal void \u0001(IList<ICompiledPOU4> \u0002)
		{
			int num;
			if (this.CompileInformation.ComconNew.IsCodegenMultithreadingAllowed())
			{
				num = Environment.ProcessorCount;
			}
			else
			{
				num = 1;
			}
			\u0082.\u0004 u = new \u0082.\u0004();
			ICodegenerator[] array = new ICodegenerator[num];
			Codegeneration[] array2 = new Codegeneration[num];
			global::\u0018.\u0013[] array3 = new global::\u0018.\u0013[num];
			Thread[] array4 = new Thread[num];
			ConcurrentQueue<_ICompiledPOU> u0008_u;
			if (num <= 1)
			{
				u0008_u = new ConcurrentQueue<_ICompiledPOU>(\u0002.Cast<_ICompiledPOU>());
			}
			else
			{
				u0008_u = new ConcurrentQueue<_ICompiledPOU>(\u0002.OrderBy(new Func<ICompiledPOU4, ICompiledPOU4>(CompilerPhase5_Codegenerator.<>c.<>9.\u0001), new \u0084.\u0005()).Cast<_ICompiledPOU>());
			}
			for (int i = 0; i < num; i++)
			{
				array[i] = this.\u0001();
				array2[i] = new Codegeneration(this.CompileInformation.ComconNew, this.CompileInformation.ComconOld, this.CompileInformation.KeepCompileInformation, this.CompileInformation.OnlineChange, array[i]);
				array3[i] = new global::\u0018.\u0013(u0008_u, this.CompileInformation.CompilerPhase4_Typechecker, array2[i], u);
				array4[i] = new Thread(new ThreadStart(array3[i].\u0001));
			}
			this.CompileInformation.Timer.\u0001("StartThreads");
			for (int j = 0; j < num; j++)
			{
				array4[j].Start();
			}
			int[] array5 = new int[num];
			for (int k = 0; k < num; k++)
			{
				array5[0] = 0;
			}
			int num2 = 0;
			while (this.\u0001(\u0002, num, u, array3, array4, array5, ref num2))
			{
			}
			this.CompileInformation.CompilerPhase4_Typechecker.\u0001();
			if (u.\u0001().Length != 0)
			{
				throw new AggregateException(u.\u0001());
			}
			this.CompileInformation.Timer.\u0004("StartThreads");
		}

		// Token: 0x060038AE RID: 14510 RVA: 0x000E8A64 File Offset: 0x000E6C64
		private bool \u0001(IList<ICompiledPOU4> \u0002, int \u0003, \u0082.\u0004 \u0004, global::\u0018.\u0013[] \u0005, Thread[] \u0006, int[] \u0007, ref int \u0008)
		{
			if (\u0006[\u0008].Join(500))
			{
				\u0008++;
			}
			this.\u0001(\u0002, \u0003, \u0005, \u0007);
			if (\u0004.\u0001().Length != 0)
			{
				for (int i = 0; i < \u0003; i++)
				{
					\u0005[i].Cancel = true;
				}
			}
			LList<string> llist = \u0004.\u0001();
			if (llist.Count <= 0)
			{
				return \u0008 < \u0003;
			}
			try
			{
				APEnvironmentFacade.Instance.LanguageModelMgr.Progress.NotifyTaskProgress(this.CompileInformation.Callback, llist[0], llist.Count);
			}
			catch (CancelledByUserException)
			{
				for (int j = 0; j < \u0003; j++)
				{
					\u0005[j].Cancel = true;
				}
			}
			bool flag = true;
			ILMCompileOptions3 ilmcompileOptions = APEnvironmentFacade.Instance.LMServiceProvider.ConfigurationService.CompileOptions as ILMCompileOptions3;
			if (ilmcompileOptions != null)
			{
				flag = ilmcompileOptions.ReportCompiledPousDuringIncrementalCompile;
			}
			if (!this.CompileInformation.OnlineChange && this.CompileInformation.ComconOld != null && flag)
			{
				foreach (string arg in llist)
				{
					string u = string.Format(\u0081.\u0001.GenerateCode, arg);
					_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, u, Severity.Information, MessageId.None);
					APEnvironmentFacade.Instance.AddMessage(APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory, message);
				}
			}
			return \u0008 < \u0003;
		}

		// Token: 0x060038AF RID: 14511 RVA: 0x000E8BE4 File Offset: 0x000E6DE4
		private void \u0001(IScope5 \u0002, IList<ICompiledPOU4> \u0003)
		{
			this.\u0002(\u0003);
			CompileEventArgs e = new CompileEventArgs(this.ApplicationGuid);
			APEnvironmentFacade.Instance.LanguageModelMgr.OnBeforeGenerateCompiledCode(e);
			this.CompileInformation.CompilerPhase5_Codegenerator.\u0001(\u0003);
			LList<ICompiledPOU4> llist = new LList<ICompiledPOU4>();
			this.\u0001(\u0003, llist);
			if (this.ComconNew.DSFCallback != null)
			{
				List<ICompiledPOU4> list = new List<ICompiledPOU4>(llist.Count);
				list.AddRange(llist);
				this.ComconNew.DSFCallback.OnBeforeCodeAllocation(list, this.ComconNew);
			}
			this.\u0001(\u0002, llist);
		}

		// Token: 0x060038B0 RID: 14512 RVA: 0x000E8C74 File Offset: 0x000E6E74
		private void \u0001(IList<ICompiledPOU4> \u0002, int \u0003, global::\u0018.\u0013[] \u0004, int[] \u0005)
		{
			if (this.CompileInformation.Precomp.IsDefined("debug_dump_times"))
			{
				for (int i = 0; i < \u0003; i++)
				{
					if (\u0005[i] == 0 && \u0004[i].\u0001)
					{
						\u0005[i] = 1;
						this.CompileInformation.Timer.\u0001("StartThreads", string.Format("Thread {0} done after {{0}} ms, {1} POUs left, {2} POUs done", i, \u0002.Count, \u0004[i].\u0001));
					}
				}
			}
		}

		// Token: 0x060038B1 RID: 14513 RVA: 0x000E8CF8 File Offset: 0x000E6EF8
		internal ICodegenerator \u0001()
		{
			bool simulationMode = this.CompileInformation.Precomp.SimulationMode;
			ICodegenerator result;
			try
			{
				IDeviceIdentification targetIdOfDevice = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetTargetIdOfDevice(this.CompileInformation.DeviceGuid);
				ITargetSettings targetSettings = APEnvironmentFacade.Instance.GetTargetSettingsById(targetIdOfDevice);
				if (targetSettings == null)
				{
					result = null;
				}
				else
				{
					if (simulationMode)
					{
						Guid deviceOfApplication = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceOfApplication(this.CompileInformation.ApplicationGuid);
						targetSettings = APEnvironmentFacade.Instance.GetSimulationTargetSettings(targetIdOfDevice, deviceOfApplication);
						if (targetSettings == null)
						{
							return null;
						}
					}
					string stringValue = global::\u0016.\u0004.CodegeneratorGuid.GetStringValue(targetSettings);
					if (string.IsNullOrEmpty(stringValue))
					{
						result = null;
					}
					else
					{
						Guid typeGuid = new Guid(stringValue);
						ICodegenerator codegenerator = APEnvironmentFacade.Instance.CreateCodegenerator(typeGuid);
						codegenerator.Setup(targetSettings);
						if (codegenerator is IDisassembler2)
						{
							(codegenerator as IDisassembler2).GenerateDisassembleCode = this.CompileInformation.KeepCompileInformation;
						}
						result = codegenerator;
					}
				}
			}
			catch
			{
				result = null;
			}
			return result;
		}

		// Token: 0x060038B2 RID: 14514 RVA: 0x000E8DFC File Offset: 0x000E6FFC
		internal Guid \u0002()
		{
			Guid result;
			try
			{
				bool simulationMode = this.CompileInformation.Precomp.SimulationMode;
				IDeviceIdentification targetIdOfDevice = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetTargetIdOfDevice(this.CompileInformation.DeviceGuid);
				ITargetSettings targetSettings = APEnvironmentFacade.Instance.GetTargetSettingsById(targetIdOfDevice);
				if (targetSettings == null)
				{
					result = Guid.Empty;
				}
				else
				{
					if (simulationMode)
					{
						Guid deviceOfApplication = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceOfApplication(this.CompileInformation.ApplicationGuid);
						targetSettings = APEnvironmentFacade.Instance.GetSimulationTargetSettings(targetIdOfDevice, deviceOfApplication);
						if (targetSettings == null)
						{
							return Guid.Empty;
						}
					}
					string stringValue = global::\u0016.\u0004.CodegeneratorGuid.GetStringValue(targetSettings);
					if (string.IsNullOrEmpty(stringValue))
					{
						result = Guid.Empty;
					}
					else
					{
						Guid guid = new Guid(stringValue);
						Guid b = new Guid("{901DECDD-9EB4-4a5b-951F-42FB21B27718}");
						if (guid == b)
						{
							string stringValue2 = global::\u0016.\u0004.BackendGuid.GetStringValue(targetSettings);
							if (string.IsNullOrEmpty(stringValue2))
							{
								result = Guid.Empty;
							}
							else
							{
								result = new Guid(stringValue2);
							}
						}
						else
						{
							result = guid;
						}
					}
				}
			}
			catch
			{
				result = Guid.Empty;
			}
			return result;
		}

		// Token: 0x060038B3 RID: 14515 RVA: 0x000E8F1C File Offset: 0x000E711C
		internal void \u0001(ICodegenerator \u0002)
		{
			IScope5 u = global::\u0007.\u0005.\u0001(this.ComconNew);
			this.ComconNew.ResetExprementHashTables();
			this.ComconNew.Codegenerator = \u0002;
			\u0002.StartGeneration();
			Codegeneration codegeneration = new Codegeneration(this.ComconNew, this.ComconOld, this.KeepCompileInformation, this.OnlineChange);
			_ISignature u2;
			this.\u0001(out u2);
			CompilerPhase5_Codegenerator.\u0001(\u0081.\u0001.GeneratingCode, CompilerPhase5_Codegenerator.MessageCategory);
			this.\u0001 = false;
			IList<ICompiledPOU4> compiledPOUsToCompileEx = this.ComconNew.GetCompiledPOUsToCompileEx();
			this.\u0001(u, compiledPOUsToCompileEx);
			if (!this.CompileInformation.CompilerPhase4_Typechecker.ErrorsOccured)
			{
				this.\u0001(\u0002, codegeneration);
				if (this.\u0007())
				{
					this.\u0001(codegeneration, u2);
				}
			}
		}

		// Token: 0x060038B4 RID: 14516 RVA: 0x000E8FCC File Offset: 0x000E71CC
		private void \u0001(out _ISignature \u0002)
		{
			\u0002 = \u0080.\u0019.\u0001(this.ComconNew, this.ComconOld);
			bool flag = this.BootProject;
			if (this.OnlineChange && !flag)
			{
				\u001D.\u000F.\u0001(this.ComconOld, true, null);
				\u001D.\u000F.\u0001(this.ComconNew, true, null);
				global::\u0004.\u0014.\u0001(this.ComconNew, this.ComconOld);
			}
			\u001D.\u000F.\u0001(this.ComconNew, false, this.ComconOld);
			InitExitSignatureInfo initExitSignatureInfo = new InitExitSignatureInfo(this.ComconNew);
			\u001E.\u0019.\u0001(this.ComconNew, this.ComconOld, initExitSignatureInfo);
			new GVLInitialisationFunctionCreator(this.ComconOld).\u0001(this.ComconNew, this.BootProject, this.OnlineChange, this.ComconOld, initExitSignatureInfo);
			ReflectionAssignmentCoder.\u0001(this.ComconNew, this.ComconOld);
			CodeInitGenerator.\u0001(this.ComconNew, this.ComconOld);
			ObjectsToCompileDetector.\u0001(this.ComconNew, this.ComconOld);
			int num = CompilerPhase5_Codegenerator.\u0001(this.ComconNew, this.ComconOld, this.OnlineChange);
			APEnvironmentFacade.Instance.LanguageModelMgr.Progress.NotifyNextTask(this.Callback, true, \u0081.\u0001.GenerateCodeProgress, num + 3, \u0081.\u0001.POUUnit);
		}

		// Token: 0x060038B5 RID: 14517 RVA: 0x000E90FC File Offset: 0x000E72FC
		private void \u0001(Codegeneration \u0002, _ISignature \u0003)
		{
			this.\u0001();
			this.Timer.\u0001("GenerateSpecialPOUs");
			this.Timer.\u0001("GlobalInit");
			InitExitSignatureInfo u;
			bool u2;
			this.\u0001(\u0002, \u0003, out u, out u2);
			this.Timer.\u0004("GlobalInit");
			this.Timer.\u0001("GlobalExit");
			if (!this.ComconNew.MinimalSystem)
			{
				this.\u0002(\u0002, u2);
				\u0084.\u0014.\u0001(this.OnlineChange, u2, this.ComconNew, this.ComconOld, u, this.BootProject, \u0002, ref this.\u0001, false, false);
			}
			this.Timer.\u0004("GlobalExit");
			this.Timer.\u0001("CodeInit");
			this.\u0001(\u0002, u2);
			this.Timer.\u0004("CodeInit");
			this.\u0002();
			this.Timer.\u0004("GenerateSpecialPOUs");
		}

		// Token: 0x060038B6 RID: 14518 RVA: 0x000E91E8 File Offset: 0x000E73E8
		private void \u0001(Codegeneration \u0002, bool \u0003)
		{
			ushort u = 0;
			int u2 = -1;
			if (this.\u0001)
			{
				return;
			}
			if (!this.ComconNew.MinimalSystem)
			{
				_ISignature isignature = null;
				if (this.OnlineChange && this.ComconNew.SimpleConcurrentOnlineChange)
				{
					bool onlineChangeInOwnSegment = this.ComconNew.DataManager._MemorySettings.OnlineChangeInOwnSegment;
					isignature = CodeInitGenerator.\u0001(this.ComconNew, this.OnlineChange && !onlineChangeInOwnSegment);
					global::\u0004.\u0014.\u0001(this.ComconNew, this.ComconOld, isignature, this.BootProject, this.CodeChanged, \u0003, true, \u0002, CompilerPhase5_Codegenerator.MessageCategory);
					global::\u0004.\u0014.\u0001(this.ComconNew, this.ComconOld, isignature, this.BootProject, this.CodeChanged, \u0003, false, \u0002, CompilerPhase5_Codegenerator.MessageCategory);
				}
				_ICompiledPOU icompiledPOU = CodeInitGenerator.\u0001(this.ComconNew, this.OnlineChange, isignature != null, this.ComconOld, out isignature, false);
				\u0002.\u0003(icompiledPOU);
				if (!MemoryCompiler.\u0003(this.ComconNew.DataManager, ref u, ref u2, this.ComconNew.DataManager.PackMode, icompiledPOU.CompiledCode.CodeSize, this.ComconNew.DataManager._MemorySettings.CodeSegmentSize, DataSegmentFlags.Code))
				{
					string u3 = global::\u0003.\u0006.\u0001(MessageId.Err_OutOfCodeMemory, new object[]
					{
						icompiledPOU.Name,
						icompiledPOU.CompiledCode.CodeSize
					});
					_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, u3, Severity.Error, MessageId.None);
					APEnvironmentFacade.Instance.AddMessage(CompilerPhase5_Codegenerator.MessageCategory, message);
					this.\u0001 = true;
				}
				else
				{
					icompiledPOU.CompiledCode.Location = global::\u0019.\u0003.\u0001(u, u2);
				}
				icompiledPOU.SetFlag(CompiledPOUFlags.ToCompile | CompiledPOUFlags.ToRemoveAfterDownload, true);
				if (this.BootProject)
				{
					icompiledPOU.SetFlag(CompiledPOUFlags.BootProjectRelevant, true);
				}
				this.ComconNew.AddCompiledPOU(icompiledPOU, isignature, true, null);
				this.Timer.\u0004("CodeInit");
				APEnvironmentFacade.Instance.LanguageModelMgr.Progress.NotifyTaskProgress(this.Callback, "relocation pou");
				CompilerPhase5_Codegenerator.\u0001(\u0081.\u0001.GenerateRelocations, CompilerPhase5_Codegenerator.MessageCategory);
			}
			this.Timer.\u0001("RelocationPou");
			if (!this.\u0001)
			{
				global::\u000F.\u001A u001A = new global::\u000F.\u001A(this.ComconNew, this.OnlineChange, this.BootProject, \u0002);
				if (this.ComconNew.MinimalSystem)
				{
					u001A.\u0001();
				}
				else
				{
					u001A.\u0001(this.Callback);
				}
			}
			this.Timer.\u0004("RelocationPou");
		}

		// Token: 0x060038B7 RID: 14519 RVA: 0x000E944C File Offset: 0x000E764C
		private void \u0002(Codegeneration \u0002, bool \u0003)
		{
			ushort u = 0;
			int u2 = -1;
			APEnvironmentFacade.Instance.LanguageModelMgr.Progress.NotifyTaskProgress(this.Callback, "code init");
			if (this.OnlineChange)
			{
				OnlineChangeDetails onlineChangeDetails = new OnlineChangeDetails(this.InterfacesChanged, this.CodeChanged);
				this.CompileInformation.OnlineChangeDetails = onlineChangeDetails;
				Codegeneration codegeneration = new Codegeneration(this.ComconOld, null, this.KeepCompileInformation, false);
				InitExitSignatureInfo u3 = new InitExitSignatureInfo(this.ComconOld);
				_ICompiledPOU icompiledPOU = \u001D.\u000F.\u0001(this.ComconNew, this.ComconOld, u3, onlineChangeDetails);
				if (icompiledPOU != null)
				{
					_ISignature isignature = this.ComconOld[icompiledPOU.SignatureId];
					_ISignature sign = this.ComconNew["GLOBAL__EXIT__COPY"];
					codegeneration.\u0003(icompiledPOU, isignature);
					if (!MemoryCompiler.\u0003(this.ComconNew.DataManager, ref u, ref u2, this.ComconNew.DataManager.PackMode, icompiledPOU.CompiledCode.CodeSize, this.ComconNew.DataManager._MemorySettings.CodeSegmentSize, DataSegmentFlags.Code))
					{
						string u4 = global::\u0003.\u0006.\u0001(MessageId.Err_OutOfCodeMemory, new object[]
						{
							icompiledPOU.Name,
							icompiledPOU.CompiledCode.CodeSize
						});
						_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, u4, Severity.Error, MessageId.Err_OutOfCodeMemory);
						APEnvironmentFacade.Instance.AddMessage(CompilerPhase5_Codegenerator.MessageCategory, message);
						this.\u0001 = true;
					}
					else
					{
						icompiledPOU.CompiledCode.Location = global::\u0019.\u0003.\u0001(u, u2);
					}
					icompiledPOU.SetFlag(CompiledPOUFlags.ToCompile, true);
					icompiledPOU.SetFlag(CompiledPOUFlags.ToRemoveAfterDownload, true);
					this.ComconNew.AddCompiledPOU(icompiledPOU, sign, true, null);
					this.ComconOld.RemoveSignature(isignature);
				}
				_ICompiledPOU icompiledPOU2 = global::\u0004.\u0014.\u0001(this.ComconNew, this.ComconOld, \u0002, \u0003, this.OnlineChangeDetails as OnlineChangeDetails);
				_ISignature sign2 = this.ComconNew[icompiledPOU2.SignatureId];
				if (!MemoryCompiler.\u0003(this.ComconNew.DataManager, ref u, ref u2, this.ComconNew.DataManager.PackMode, icompiledPOU2.CompiledCode.CodeSize, this.ComconNew.DataManager._MemorySettings.CodeSegmentSize, DataSegmentFlags.Code))
				{
					string u4 = global::\u0003.\u0006.\u0001(MessageId.Err_OutOfCodeMemory, new object[]
					{
						icompiledPOU2.Name,
						icompiledPOU2.CompiledCode.CodeSize
					});
					_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, u4, Severity.Error, MessageId.Err_OutOfCodeMemory);
					APEnvironmentFacade.Instance.AddMessage(CompilerPhase5_Codegenerator.MessageCategory, message);
					this.\u0001 = true;
				}
				else
				{
					icompiledPOU2.CompiledCode.Location = global::\u0019.\u0003.\u0001(u, u2);
				}
				icompiledPOU2.SetFlag(CompiledPOUFlags.ToCompile, true);
				icompiledPOU2.SetFlag(CompiledPOUFlags.ToRemoveAfterDownload, true);
				this.ComconNew.AddCompiledPOU(icompiledPOU2, sign2, true, null);
				if (!this.\u0008())
				{
					this.\u0001 = true;
				}
			}
		}

		// Token: 0x060038B8 RID: 14520 RVA: 0x000E9708 File Offset: 0x000E7908
		private void \u0001(Codegeneration \u0002, _ISignature \u0003, out InitExitSignatureInfo \u0004, out bool \u0005)
		{
			string u = \u0081.\u0001.GenerateGlobalInit;
			_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, u, Severity.Text, MessageId.None);
			APEnvironmentFacade.Instance.AddMessage(CompilerPhase5_Codegenerator.MessageCategory, message);
			APEnvironmentFacade.Instance.LanguageModelMgr.Progress.NotifyTaskProgress(this.Callback, "global init");
			\u0005 = (this.OnlineChange && !this.InterfacesChanged);
			if (this.InitValuesChanged)
			{
				\u0005 = false;
			}
			if (this.BootProject)
			{
				\u0005 = true;
			}
			\u0004 = new InitExitSignatureInfo(this.ComconNew);
			if (this.ComconNew.IsDefined("global_init_in_cycle"))
			{
				_ISignature u000E = this.ComconNew["global__init__x"];
				\u0084.\u0014.\u0001(this.OnlineChange, \u0005, this.ComconNew, this.ComconOld, \u0004, this.BootProject, \u0002, u000E, ref this.\u0001, false, false);
				\u0084.\u0014.\u0001(this.OnlineChange, \u0005, this.ComconNew, this.ComconOld, \u0004, this.BootProject, \u0002, u000E, ref this.\u0001, true, false);
			}
			else
			{
				\u0084.\u0014.\u0001(this.OnlineChange, \u0005, this.ComconNew, this.ComconOld, \u0004, this.BootProject, \u0002, \u0003, ref this.\u0001, false, false);
			}
			CompilerPhase5_Codegenerator.\u0001(\u0081.\u0001.GenerateCodeInit, CompilerPhase5_Codegenerator.MessageCategory);
		}

		// Token: 0x060038B9 RID: 14521 RVA: 0x000E984C File Offset: 0x000E7A4C
		private void \u0001()
		{
			if (this.InterfacesChanged)
			{
				this.ComconNew.DataId = Guid.NewGuid();
				if (this.OnlineChange)
				{
					this.ComconNew.LastDataId = this.ComconOld.DataId;
				}
				else
				{
					this.ComconNew.LastDataId = this.ComconNew.DataId;
				}
			}
			else
			{
				this.ComconNew.DataId = this.ComconOld.DataId;
				this.ComconNew.LastDataId = this.ComconNew.DataId;
			}
			if (this.CodeChanged)
			{
				this.ComconNew.CodeId = Guid.NewGuid();
				if (this.OnlineChange)
				{
					this.ComconNew.LastCodeId = this.ComconOld.CodeId;
				}
				else
				{
					this.ComconNew.LastCodeId = this.ComconNew.CodeId;
				}
			}
			else
			{
				this.ComconNew.CodeId = this.ComconOld.CodeId;
				this.ComconNew.LastCodeId = this.ComconNew.CodeId;
			}
			if (this.BootProject)
			{
				this.CompileInformation.OnlineChange = false;
			}
		}

		// Token: 0x060038BA RID: 14522 RVA: 0x000E9968 File Offset: 0x000E7B68
		private bool \u0007()
		{
			bool flag = !this.BootProject && !this.InterfacesChanged && !this.CodeChanged && (this.OnlineChange || (this.ComconOld != null && !this.ComconOld.ContainsOnlineChangeCode));
			if (flag)
			{
				APEnvironmentFacade.Instance.LanguageModelMgr[this.ApplicationGuid] = this.ComconOld;
				string u = \u0081.\u0001.CodeNoChange;
				_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, u, Severity.Information, MessageId.None);
				APEnvironmentFacade.Instance.AddMessage(CompilerPhase5_Codegenerator.MessageCategory, message);
				this.ComconNew.ResetExprementHashTables();
				this.ComconOld.LibCheckSum = this.ComconNew.LibCheckSum;
				this.ComconOld.PoolLibCheckSum = this.ComconNew.PoolLibCheckSum;
				this.ComconOld.ParameterTableChecksum = this.ComconNew.ParameterTableChecksum;
				this.ComconOld.ProjectChecksum = this.ComconNew.ProjectChecksum;
				((_ICompileContext2)this.ComconOld).PrecompileContextNamesChecksum = ((_ICompileContext2)this.ComconNew).PrecompileContextNamesChecksum;
			}
			return !flag;
		}

		// Token: 0x060038BB RID: 14523 RVA: 0x000E9A7C File Offset: 0x000E7C7C
		private void \u0001(ICodegenerator \u0002, Codegeneration \u0003)
		{
			if (!this.ComconNew.MinimalSystem && !this.\u0001)
			{
				if (this.ComconNew.GenerateContent)
				{
					\u0084.\u0011.\u0001(this.ComconNew, this.ComconOld, \u0002, this.OnlineChange);
				}
				global::\u0012.\u0010.\u0001(this.ComconNew, this.ComconOld, \u0002, this.OnlineChange);
				global::\u0006.\u0012.\u0001(this.ComconNew, this.ComconOld);
				LicenseCheckGenerator.\u0001(this.ComconNew, this.Timer, \u0003);
			}
		}

		// Token: 0x060038BC RID: 14524 RVA: 0x000E9B00 File Offset: 0x000E7D00
		private void \u0001(IScope5 \u0002, LList<ICompiledPOU4> \u0003)
		{
			ushort u = 0;
			int u2 = 0;
			foreach (_ICompiledPOU icompiledPOU in \u0003.OfType<_ICompiledPOU>())
			{
				DataSegmentFlags dataSegmentFlags = DataSegmentFlags.Code;
				if (this.ComconNew.DSFCallback != null)
				{
					_ISignature sign = \u0002[icompiledPOU.SignatureId] as _ISignature;
					dataSegmentFlags = this.ComconNew.DSFCallback.GetDataSegmentFlagForCode(icompiledPOU, sign, this.ComconNew, dataSegmentFlags);
				}
				if (!MemoryCompiler.\u0003(this.ComconNew.DataManager, ref u, ref u2, this.ComconNew.DataManager.PackMode, icompiledPOU.CompiledCode.CodeSize, this.ComconNew.DataManager._MemorySettings.CodeSegmentSize, dataSegmentFlags))
				{
					string u3 = global::\u0003.\u0006.\u0001(MessageId.Err_OutOfCodeMemory, new object[]
					{
						icompiledPOU.Name,
						icompiledPOU.CompiledCode.CodeSize
					});
					_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, u3, Severity.Error, MessageId.Err_OutOfCodeMemory);
					APEnvironmentFacade.Instance.AddMessage(CompilerPhase5_Codegenerator.MessageCategory, message);
					this.\u0001 = true;
				}
				else
				{
					icompiledPOU.CompiledCode.Location = global::\u0019.\u0003.\u0001(u, u2);
				}
			}
		}

		// Token: 0x060038BD RID: 14525 RVA: 0x000E9C54 File Offset: 0x000E7E54
		private void \u0001(IList<ICompiledPOU4> \u0002, LList<ICompiledPOU4> \u0003)
		{
			for (int i = 0; i < \u0002.Count; i++)
			{
				_ICompiledPOU icompiledPOU = \u0002[i] as _ICompiledPOU;
				if (icompiledPOU.CompiledCode != null && !icompiledPOU.GetFlag(CompiledPOUFlags.Blob) && !icompiledPOU.GetFlag(CompiledPOUFlags.ConstBlob) && !icompiledPOU.GetFlag(CompiledPOUFlags.NoCompile) && this.\u0001(\u0003, icompiledPOU))
				{
					\u0003.Add(icompiledPOU);
					if (!(icompiledPOU.Name == IdentifierConstants.PartialInitMethodName))
					{
						this.CompileInformation.CodeChanged = true;
					}
				}
			}
		}

		// Token: 0x060038BE RID: 14526 RVA: 0x000E9CE0 File Offset: 0x000E7EE0
		private bool \u0001(LList<ICompiledPOU4> \u0002, _ICompiledPOU \u0003)
		{
			if (this.ComconOld == null)
			{
				return true;
			}
			_ICompiledPOU icompiledPOU = this.ComconOld._GetCompiledPOUById(\u0003.SignatureId);
			if (icompiledPOU == null)
			{
				return true;
			}
			if (!\u0003.GetFlag(CompiledPOUFlags.ToCompile))
			{
				\u0003.CompiledCode = icompiledPOU.CompiledCode;
				icompiledPOU.Checksum = \u0003.Checksum;
				icompiledPOU.ObjectGuid = \u0003.ObjectGuid;
				\u0003.SetFlag(CompiledPOUFlags.ToCompile, false);
				if (this.ComconNew.DataManager._MemorySettings.OnlineChangeInOwnSegment)
				{
					\u0002.Add(\u0003);
				}
				return false;
			}
			return true;
		}

		// Token: 0x060038BF RID: 14527 RVA: 0x000E9D64 File Offset: 0x000E7F64
		private void \u0002(IList<ICompiledPOU4> \u0002)
		{
			for (int i = 0; i < \u0002.Count; i++)
			{
				_ICompiledPOU icompiledPOU = \u0002[i] as _ICompiledPOU;
				if (!icompiledPOU.GetFlag(CompiledPOUFlags.ContainsNoCode) && !icompiledPOU.GetFlag(CompiledPOUFlags.Blob) && !icompiledPOU.GetFlag(CompiledPOUFlags.ConstBlob))
				{
					this.\u0001(icompiledPOU);
					if (!icompiledPOU.GetFlag(CompiledPOUFlags.ToCompile))
					{
						if (this.ComconOld != null)
						{
							_ICompiledPOU icompiledPOU2 = this.ComconOld._GetCompiledPOUById(icompiledPOU.SignatureId);
							if (icompiledPOU2 != null && !icompiledPOU.GetFlag(CompiledPOUFlags.NoCompile))
							{
								this.\u0001(icompiledPOU, icompiledPOU2);
								goto IL_86;
							}
						}
						this.\u0002(icompiledPOU);
					}
				}
				IL_86:;
			}
		}

		// Token: 0x060038C0 RID: 14528 RVA: 0x000E9E08 File Offset: 0x000E8008
		private void \u0001(_ICompiledPOU \u0002, _ICompiledPOU \u0003)
		{
			\u0002.CompiledCode = \u0003.CompiledCode;
			\u0002.CodeGeneratorStackSize = \u0003.CodeGeneratorStackSize;
			\u0002.SetBreakpointList(\u0003.BreakpointList);
			\u0002.SetFlag(CompiledPOUFlags.ToCompile, false);
			if (\u0003.TryCatchCodeAddresses != null)
			{
				for (int i = 0; i < \u0003.TryCatchCodeAddresses.Count; i++)
				{
					int iOffset = \u0003.TryCatchCodeAddresses[i];
					\u0002.AddTryCatchCodeAddressIndex(iOffset, i);
				}
				Debug.\u0001(\u0002.TryCatchFPAddresses.Count == \u0002.TryCatchCodeAddresses.Count);
			}
			if (!this.ComconNew.DataManager._MemorySettings.OnlineChangeInOwnSegment)
			{
				Debug.\u0001(MemoryCompiler.\u0002(this.ComconNew.DataManager, \u0002.CompiledCode.Location.Area, \u0002.CompiledCode.Location.Offset, \u0002.CompiledCode.CodeSize, DataSegmentFlags.None));
			}
		}

		// Token: 0x060038C1 RID: 14529 RVA: 0x000E9EEC File Offset: 0x000E80EC
		private void \u0001(_ICompiledPOU \u0002)
		{
			if (\u0002.GetFlag(CompiledPOUFlags.ContainsNoParseTree) && \u0002.GetFlag(CompiledPOUFlags.ToCompile))
			{
				_ISignature isignature = this.ComconNew.GetSignatureById(\u0002.SignatureId) as _ISignature;
				string u = string.Format(\u0081.\u0002.Err_InternalErrorProhibitingOnlineChange, 2);
				IMessage cm = global::\u0019.\u0003.\u0001(null, u, Severity.Error, MessageId.Err_InternalErrorProhibitingOnlineChange);
				isignature.AddError(cm);
				\u0002.SetFlag(CompiledPOUFlags.ToCompile, false);
			}
		}

		// Token: 0x060038C2 RID: 14530 RVA: 0x000E9F54 File Offset: 0x000E8154
		private void \u0002(_ICompiledPOU \u0002)
		{
			\u0002.SetFlag(CompiledPOUFlags.ToCompile, true);
			if (!\u0002.GetFlag(CompiledPOUFlags.Typified))
			{
				IScope5 scope = global::\u0007.\u0005.\u0001(this.ComconNew, \u0002.SignatureId);
				ExpressionTypifierWithSpecialTasks visitor = new ExpressionTypifierWithSpecialTasks(scope, this.ComconNew, true, \u0002);
				TypeCheckerVisitor visitor2 = new TypeCheckerVisitor(scope, this.ComconNew, true);
				ErrorVisitor visitor3 = new ErrorVisitor();
				\u0002.DuplicateParseTreeForCompilation();
				\u0002.Accept(visitor);
				\u0002.Accept(visitor2);
				\u0002.Accept(visitor3);
				\u0002.SetFlag(CompiledPOUFlags.Typified, true);
			}
		}

		// Token: 0x060038C3 RID: 14531 RVA: 0x000E9FC8 File Offset: 0x000E81C8
		internal void \u0002()
		{
			this.Timer.\u0001("CheckStackUsage");
			if (!this.CompileInformation.Precomp.IsDefined("suppress_stack_check"))
			{
				this.\u0003();
			}
			this.\u0004();
			this.Timer.\u0004("CheckStackUsage");
			this.Timer.\u0002("CheckStackUsage", "Time for checking stack usage: {0} ms");
		}

		// Token: 0x060038C4 RID: 14532 RVA: 0x000EA030 File Offset: 0x000E8230
		private void \u0003()
		{
			int nMaxStackSize;
			int nMaxStackInExternalFuctions;
			if (!CallTree.TryGetMaxStackSize(this.ComconNew, out nMaxStackSize, out nMaxStackInExternalFuctions))
			{
				return;
			}
			new CallTree(this.ComconNew, true, nMaxStackSize, nMaxStackInExternalFuctions).CheckStackUsage(null);
		}

		// Token: 0x060038C5 RID: 14533 RVA: 0x000EA064 File Offset: 0x000E8264
		private void \u0004()
		{
			if (this.ComconNew.TaskList.Count <= 1)
			{
				return;
			}
			ICodegenerator3 codegenerator = this.ComconNew.Codegenerator as ICodegenerator3;
			if (codegenerator == null || !codegenerator.GetProperty(CodegeneratorProperties.CheckConcurrentBitAccess))
			{
				return;
			}
			\u001C.\u0005.\u0001(this.ComconNew);
		}

		// Token: 0x060038C6 RID: 14534 RVA: 0x000EA0B0 File Offset: 0x000E82B0
		private static void \u0001(string \u0002, IMessageCategory \u0003)
		{
			_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, \u0002, Severity.Text, MessageId.None);
			APEnvironmentFacade.Instance.AddMessage(\u0003, message);
		}

		// Token: 0x060038C7 RID: 14535 RVA: 0x000EA0D4 File Offset: 0x000E82D4
		private bool \u0008()
		{
			if (this.OnlineChangeDetails == null || this.ComconOld == null)
			{
				return true;
			}
			bool result = true;
			LDictionary<IDirectVariable, IVariableInfo> ldictionary = new LDictionary<IDirectVariable, IVariableInfo>();
			LDictionary<IDirectVariable, IVariableInfo> ldictionary2 = new LDictionary<IDirectVariable, IVariableInfo>();
			this.\u0001(ldictionary, ldictionary2);
			foreach (IDirectVariable directVariable in ldictionary2.Keys)
			{
				if (ldictionary.ContainsKey(directVariable) && !directVariable.Incomplete)
				{
					IVariableInfo variableInfo = ldictionary2[directVariable];
					IVariableInfo variableInfo2 = ldictionary[directVariable];
					ISignature signature = this.ComconOld[variableInfo.SignatureId];
					IVariable variable = signature[variableInfo.VariableId];
					ISignature signature2 = this.ComconNew[variableInfo2.SignatureId];
					IVariable variable2 = signature2[variableInfo2.VariableId];
					MessageId u;
					string u2;
					if (variable2.Id == variable.Id && signature2.Id == signature.Id)
					{
						u = MessageId.Err_NoCopyCodeForVariableAtDirectAddress;
						u2 = global::\u0003.\u0006.\u0001(MessageId.Err_NoCopyCodeForVariableAtDirectAddress, Array.Empty<object>());
					}
					else
					{
						u = MessageId.Err_AddressSourceIsAddressDest;
						u2 = global::\u0003.\u0006.\u0001(MessageId.Err_AddressSourceIsAddressDest, new object[]
						{
							variable.OrgName,
							directVariable,
							variable2.OrgName
						});
					}
					_ISourcePosition isourcePosition = global::\u0019.\u0003.\u0001(variable.SourcePosition.ProjectHandle, variable.SourcePosition.ObjectGuid, variable.SourcePosition.Position, variable.SourcePosition.PositionOffset, variable.SourcePosition.Length);
					isourcePosition.SetObjectIdentification(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(signature.LibraryPath), signature.ObjectGuid);
					_ICompilerMessage message = global::\u0019.\u0003.\u0001(isourcePosition, u2, Severity.Error, u);
					IMessageCategory messageCategory = APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory;
					APEnvironmentFacade.Instance.AddMessage(messageCategory, message);
					_ISourcePosition isourcePosition2 = global::\u0019.\u0003.\u0001(variable2.SourcePosition.ProjectHandle, variable2.SourcePosition.ObjectGuid, variable2.SourcePosition.Position, variable2.SourcePosition.PositionOffset, variable2.SourcePosition.Length);
					isourcePosition2.SetObjectIdentification(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(signature2.LibraryPath), signature2.ObjectGuid);
					u2 = global::\u0003.\u0006.\u0001(MessageId.Inf_RelatedPosition, Array.Empty<object>());
					message = global::\u0019.\u0003.\u0001(isourcePosition2, u2, Severity.Information, MessageId.Inf_RelatedPosition);
					APEnvironmentFacade.Instance.AddMessage(messageCategory, message);
					result = false;
				}
			}
			return result;
		}

		// Token: 0x060038C8 RID: 14536 RVA: 0x000EA36C File Offset: 0x000E856C
		private void \u0001(LDictionary<IDirectVariable, IVariableInfo> \u0002, LDictionary<IDirectVariable, IVariableInfo> \u0003)
		{
			foreach (IVariableInfo u in this.OnlineChangeDetails.VariablesAffected)
			{
				this.\u0001(\u0002, \u0003, u);
			}
		}

		// Token: 0x060038C9 RID: 14537 RVA: 0x000EA3C8 File Offset: 0x000E85C8
		private void \u0001(LDictionary<IDirectVariable, IVariableInfo> \u0002, LDictionary<IDirectVariable, IVariableInfo> \u0003, IVariableInfo \u0004)
		{
			if ((\u0004.Flags & VarFlag.OnlChangeCopy) != VarFlag.OnlChangeCopy || (\u0004.Flags & VarFlag.LocationChanged) != VarFlag.LocationChanged)
			{
				return;
			}
			ISignature signature = this.ComconOld[\u0004.SignatureId];
			if (signature == null)
			{
				return;
			}
			IVariable variable = signature[\u0004.VariableId];
			if (variable == null)
			{
				return;
			}
			ISignature signature2 = this.ComconNew[\u0004.SignatureId];
			if (signature2 == null)
			{
				return;
			}
			IVariable variable2 = signature2[\u0004.VariableId];
			if (variable2 == null)
			{
				return;
			}
			if (variable.Address != null && !\u0003.ContainsKey(variable.Address))
			{
				\u0003[variable.Address] = \u0004;
			}
			if (variable2.Address != null && !\u0002.ContainsKey(variable2.Address))
			{
				\u0002[variable2.Address] = \u0004;
			}
		}

		// Token: 0x060038CA RID: 14538 RVA: 0x000EA494 File Offset: 0x000E8694
		private static void \u0001(_ISignature \u0002, IScope5 \u0003)
		{
			\u0002.SetFlag(SignatureFlag.InitializeVirtualFunctionTable, true);
			foreach (int nId in \u0002.DeclarerIds)
			{
				_ISignature isignature = \u0003[nId] as _ISignature;
				if (isignature.BaseSignatureId == \u0002.Id)
				{
					CompilerPhase5_Codegenerator.\u0001(isignature, \u0003);
				}
			}
		}

		// Token: 0x060038CB RID: 14539 RVA: 0x000EA4EC File Offset: 0x000E86EC
		internal static int \u0001(_ICompileContext \u0002, _ICompileContext \u0003, bool \u0004)
		{
			int num = 0;
			IList<ICompiledPOU4> compiledPOUsToCompileEx = \u0002.GetCompiledPOUsToCompileEx();
			bool newVFTable = \u0002.NewVFTable;
			IScope5 u = \u0002.CreateGlobalIScope() as IScope5;
			if (!newVFTable)
			{
				return 0;
			}
			foreach (_ICompiledPOU icompiledPOU in compiledPOUsToCompileEx.OfType<_ICompiledPOU>())
			{
				if (icompiledPOU.GetFlag(CompiledPOUFlags.ToCompile) || \u0003 == null || !\u0004)
				{
					num++;
					if (icompiledPOU.GetFlag(CompiledPOUFlags.ToCompile) && \u0004)
					{
						_ISignature isignature = \u0002[icompiledPOU.SignatureId];
						Debug.\u0001(isignature != null);
						if (isignature != null && isignature.POUType == Operator.Method)
						{
							_ISignature isignature2 = \u0002[isignature.ParentSignatureId];
							Debug.\u0001(isignature2 != null);
							CompilerPhase5_Codegenerator.\u0001(isignature2, u);
						}
					}
				}
			}
			return num;
		}

		// Token: 0x04000B4B RID: 2891
		[CompilerGenerated]
		private global::\u000E.\u001B \u0001;

		// Token: 0x04000B4C RID: 2892
		private bool \u0001;
	}
}
