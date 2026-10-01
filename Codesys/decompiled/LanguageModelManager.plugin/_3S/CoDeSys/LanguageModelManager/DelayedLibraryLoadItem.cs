using System;
using System.Diagnostics;
using System.IO;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.Interfaces;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000D7 RID: 215
	internal class DelayedLibraryLoadItem : IDelayedLoadItem
	{
		// Token: 0x06000F56 RID: 3926 RVA: 0x00029508 File Offset: 0x00028508
		internal DelayedLibraryLoadItem(Stream stream, IArchiveReader reader, string stLibraryId, bool bInsertToLMM, ISharedDataStorage sharedDataStorage, _IPreCompileContext precomFromLMM, SetLibraryPreCompileContextCompletionEventHandler completionEventHandler, object callerData, LibraryInfo libraryInfo)
		{
			this.Stream = stream;
			this.Reader = reader;
			this.LibraryId = stLibraryId;
			this.InsertToLMM = bInsertToLMM;
			this.SharedDataStorage = sharedDataStorage;
			this.PrecomFromLMM = precomFromLMM;
			this.LinkInSimulation = libraryInfo.LinkInSimulation;
			this.CompletionEventHandler = completionEventHandler;
			this.CallerData = callerData;
			this.QualifiedAccessOnly = libraryInfo.QualifiedAccessOnly;
			this.IsInterfaceLibrary = libraryInfo.IsInterfaceLibrary;
			this.Support32BitOnly = libraryInfo.Support32BitOnly;
			this.OnlineChangeable = libraryInfo.OnlineChangeable;
			this.UnitTestingDefine = libraryInfo.UnitTestingDefine;
		}

		// Token: 0x1700046B RID: 1131
		// (get) Token: 0x06000F57 RID: 3927 RVA: 0x000295A6 File Offset: 0x000285A6
		// (set) Token: 0x06000F58 RID: 3928 RVA: 0x000295AE File Offset: 0x000285AE
		public object CallerData { get; set; }

		// Token: 0x1700046C RID: 1132
		// (get) Token: 0x06000F59 RID: 3929 RVA: 0x000295B7 File Offset: 0x000285B7
		public string ProcessText
		{
			get
			{
				return this.LibraryId;
			}
		}

		// Token: 0x1700046D RID: 1133
		// (get) Token: 0x06000F5A RID: 3930 RVA: 0x000295BF File Offset: 0x000285BF
		// (set) Token: 0x06000F5B RID: 3931 RVA: 0x000295C7 File Offset: 0x000285C7
		public bool Removed { get; set; }

		// Token: 0x06000F5C RID: 3932 RVA: 0x000295D0 File Offset: 0x000285D0
		public void ProcessLoad(bool bLastLibraryItem, AsyncLogger asyncLogger)
		{
			try
			{
				if (this.Removed)
				{
					this.IsCompleted = true;
				}
				else
				{
					if (asyncLogger != null)
					{
						asyncLogger.Log("Start loading " + this.ProcessText);
					}
					PreCompileContext preCompileContext;
					if (this.SharedDataStorage != null)
					{
						Debug.Assert(this.Reader is IArchiveReader2);
						preCompileContext = (PreCompileContext)((IArchiveReader2)this.Reader).Load(this.SharedDataStorage);
					}
					else
					{
						preCompileContext = (PreCompileContext)this.Reader.Load();
					}
					this.Stream.Close();
					if (asyncLogger != null)
					{
						asyncLogger.Log("Start updating " + this.ProcessText);
					}
					preCompileContext.LinkInSimulation = this.LinkInSimulation;
					preCompileContext.QualifiedAccessOnly = this.QualifiedAccessOnly;
					preCompileContext.Support32BitOnly = this.Support32BitOnly;
					preCompileContext.IsInterfaceLibrary = this.IsInterfaceLibrary;
					preCompileContext.OnlineChangeable = this.OnlineChangeable;
					preCompileContext.UnitTestingDefine = this.UnitTestingDefine;
					LanguageModelManagerConsolidated.SetFlagsForLibrarySignatures(preCompileContext, this.LibraryId, this.LinkInSimulation, this.IsInterfaceLibrary, this.OnlineChangeable);
					if (asyncLogger != null)
					{
						asyncLogger.Log("Start publishing " + this.ProcessText);
					}
					SetLibraryPreCompileContextCompletionEventArgs setLibraryPreCompileContextCompletionEventArgs = new SetLibraryPreCompileContextCompletionEventArgs(this.LibraryId, bLastLibraryItem, this.CallerData);
					APEnvironmentFacade.Instance.InvokeInPrimaryThread(new LanguageModelManagerConsolidated.SetLibraryPreCompileContextFromArchive_Phase2_Delegate(LanguageModelManagerConsolidated._LMM.SetLibraryPreCompileContextFromArchive_Phase2), new object[]
					{
						this,
						preCompileContext,
						setLibraryPreCompileContextCompletionEventArgs
					}, true);
				}
			}
			catch
			{
				this.IsCompleted = true;
			}
		}

		// Token: 0x06000F5D RID: 3933 RVA: 0x00029760 File Offset: 0x00028760
		public void PostProcessLoad(bool bLastLibraryItem)
		{
			if (this.Removed)
			{
				this.IsCompleted = true;
				return;
			}
			this.EnterPostProcess();
			SetLibraryPreCompileContextCompletionPostProcessEventArgs setLibraryPreCompileContextCompletionPostProcessEventArgs = new SetLibraryPreCompileContextCompletionPostProcessEventArgs(this.LibraryId, bLastLibraryItem, this.CallerData, new ParameterlessDelegate(this.SetCompleted), new ParameterlessDelegate(this.LeavePostProcess), new ExitImmediatelyDelegate(this.ExitImmediately));
			APEnvironmentFacade.Instance.InvokeInPrimaryThread(new LanguageModelManagerConsolidated.SetLibraryPreCompileContextCompletionPostProcess_Delegate(LanguageModelManagerConsolidated._LMM.RaiseSetLibraryPreCompileContextCompletionPostProcess), new object[]
			{
				setLibraryPreCompileContextCompletionPostProcessEventArgs
			}, true);
		}

		// Token: 0x06000F5E RID: 3934 RVA: 0x000297DF File Offset: 0x000287DF
		private void SetCompleted()
		{
			this.IsCompleted = true;
		}

		// Token: 0x06000F5F RID: 3935 RVA: 0x000297E8 File Offset: 0x000287E8
		private void EnterPostProcess()
		{
			this.DuringPostProcess = true;
		}

		// Token: 0x06000F60 RID: 3936 RVA: 0x000297F1 File Offset: 0x000287F1
		private void LeavePostProcess()
		{
			this.DuringPostProcess = false;
		}

		// Token: 0x06000F61 RID: 3937 RVA: 0x000297FA File Offset: 0x000287FA
		private bool ExitImmediately()
		{
			return this.IsCompleted;
		}

		// Token: 0x06000F62 RID: 3938 RVA: 0x00029804 File Offset: 0x00028804
		public bool IsEqual(IDelayedLoadItem item)
		{
			DelayedLibraryLoadItem delayedLibraryLoadItem = item as DelayedLibraryLoadItem;
			return delayedLibraryLoadItem != null && this.LibraryId == delayedLibraryLoadItem.LibraryId;
		}

		// Token: 0x1700046E RID: 1134
		// (get) Token: 0x06000F63 RID: 3939 RVA: 0x0002982E File Offset: 0x0002882E
		// (set) Token: 0x06000F64 RID: 3940 RVA: 0x00029836 File Offset: 0x00028836
		public bool IsCompleted { get; set; }

		// Token: 0x1700046F RID: 1135
		// (get) Token: 0x06000F65 RID: 3941 RVA: 0x0002983F File Offset: 0x0002883F
		// (set) Token: 0x06000F66 RID: 3942 RVA: 0x00029847 File Offset: 0x00028847
		public bool DuringPostProcess { get; set; }

		// Token: 0x04000375 RID: 885
		internal readonly Stream Stream;

		// Token: 0x04000376 RID: 886
		internal readonly IArchiveReader Reader;

		// Token: 0x04000377 RID: 887
		internal readonly string LibraryId;

		// Token: 0x04000378 RID: 888
		internal readonly bool InsertToLMM;

		// Token: 0x04000379 RID: 889
		internal readonly ISharedDataStorage SharedDataStorage;

		// Token: 0x0400037A RID: 890
		internal readonly _IPreCompileContext PrecomFromLMM;

		// Token: 0x0400037B RID: 891
		internal readonly bool LinkInSimulation;

		// Token: 0x0400037C RID: 892
		internal readonly bool QualifiedAccessOnly;

		// Token: 0x0400037D RID: 893
		internal readonly bool IsInterfaceLibrary;

		// Token: 0x0400037E RID: 894
		internal readonly bool Support32BitOnly;

		// Token: 0x0400037F RID: 895
		internal readonly bool OnlineChangeable;

		// Token: 0x04000380 RID: 896
		internal readonly string UnitTestingDefine;

		// Token: 0x04000381 RID: 897
		internal readonly SetLibraryPreCompileContextCompletionEventHandler CompletionEventHandler;
	}
}
