using System;
using System.IO;
using CODESYS.ProjectFormat.SideCar;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.CommonCompilerData;
using _3S.CoDeSys.LanguageModelManager.Interfaces;

namespace _3S.CoDeSys.LanguageModelManager.LMCompiledSetOperations
{
	// Token: 0x020001B7 RID: 439
	internal class DelayedReferenceContextLoadItem : IDelayedLoadItem
	{
		// Token: 0x06001F84 RID: 8068 RVA: 0x00056EE0 File Offset: 0x00055EE0
		internal DelayedReferenceContextLoadItem(string stPath, string stProcessText, ISideCarEntry sideCarEntry)
		{
			this._stReferenceContextPath = stPath;
			this.ProcessText = stProcessText;
			this._sideCarEntry = sideCarEntry;
		}

		// Token: 0x17000843 RID: 2115
		// (get) Token: 0x06001F85 RID: 8069 RVA: 0x00056EFD File Offset: 0x00055EFD
		public string ProcessText { get; }

		// Token: 0x06001F86 RID: 8070 RVA: 0x00056F08 File Offset: 0x00055F08
		public void ProcessLoad(bool bLastLibraryItem, AsyncLogger asyncLogger)
		{
			if (this._stReferenceContextPath == null || !this._sideCarEntry.Exists)
			{
				this.IsCompleted = true;
				return;
			}
			PerformanceTimer performanceTimer = new PerformanceTimer("CompileinfoLoad");
			performanceTimer.StartTiming();
			try
			{
				CompileContext compileContext;
				using (Stream stream = this._sideCarEntry.ReadFrom())
				{
					compileContext = LMCompiledSetPersistence.LoadFromStream<CompileContext>(stream);
				}
				IAPEnvironmentFacade instance = APEnvironmentFacade.Instance;
				Delegate dlgt = new LanguageModelManagerConsolidated.SetReferenceContextFromArchive_Delegate(LanguageModelManagerConsolidated._LMM.SetReferenceContextFromArchive_Phase2);
				object[] array = new object[3];
				array[0] = this;
				array[1] = compileContext;
				instance.InvokeInPrimaryThread(dlgt, array, true);
			}
			catch (Exception ex)
			{
				string msg = string.Format(Strings.CompileContextLoadError, this._stReferenceContextPath, ex.Message);
				APEnvironmentFacade.Instance.InvokeInPrimaryThread(new Action(delegate
				{
					APEnvironmentFacade.Instance.MessageStorage.AddMessage(CompileContextMessageCategory.Singleton, new CompileContextMessage(msg, Severity.Warning));
				}), Array.Empty<object>(), true);
				this.IsCompleted = true;
			}
			performanceTimer.StopTiming();
			string environmentVariable = Environment.GetEnvironmentVariable("AP_DEBUG_LMM_DUMP_TIMES_COMPILEINFO");
			if (!string.IsNullOrEmpty(environmentVariable))
			{
				APEnvironmentFacade.Instance.FileSystem.AppendAllText(environmentVariable, performanceTimer.OutputMeasurementTMStyle());
			}
		}

		// Token: 0x06001F87 RID: 8071 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void PostProcessLoad(bool bLastLibraryItem)
		{
		}

		// Token: 0x06001F88 RID: 8072 RVA: 0x00057028 File Offset: 0x00056028
		public bool IsEqual(IDelayedLoadItem item)
		{
			DelayedReferenceContextLoadItem delayedReferenceContextLoadItem = item as DelayedReferenceContextLoadItem;
			return delayedReferenceContextLoadItem != null && this._stReferenceContextPath == delayedReferenceContextLoadItem._stReferenceContextPath;
		}

		// Token: 0x17000844 RID: 2116
		// (get) Token: 0x06001F89 RID: 8073 RVA: 0x00057052 File Offset: 0x00056052
		// (set) Token: 0x06001F8A RID: 8074 RVA: 0x0005705A File Offset: 0x0005605A
		public bool IsCompleted { get; set; }

		// Token: 0x17000845 RID: 2117
		// (get) Token: 0x06001F8B RID: 8075 RVA: 0x00004E6B File Offset: 0x00003E6B
		// (set) Token: 0x06001F8C RID: 8076 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public bool DuringPostProcess
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		// Token: 0x0400062F RID: 1583
		private readonly string _stReferenceContextPath;

		// Token: 0x04000630 RID: 1584
		private readonly ISideCarEntry _sideCarEntry;
	}
}
