using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using CODESYS.ProjectFormat.SideCar;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.LanguageModelManager.Interfaces;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.LanguageModelManager.LMCompiledSetOperations;
using _3S.CoDeSys.LibManObject;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000D9 RID: 217
	internal class DelayedLoader : _IDelayedLoader
	{
		// Token: 0x06000F69 RID: 3945 RVA: 0x000298F4 File Offset: 0x000288F4
		internal DelayedLoader()
		{
			string environmentVariable = Environment.GetEnvironmentVariable("AP_DEBUG_LMM_DELAYED_LOADER");
			if (environmentVariable != null)
			{
				this._asyncLogger = new AsyncLogger(environmentVariable);
			}
			this.DebugLog("Creating DelayedLoader");
		}

		// Token: 0x06000F6A RID: 3946 RVA: 0x0002994D File Offset: 0x0002894D
		private void DebugLog(string text)
		{
			AsyncLogger asyncLogger = this._asyncLogger;
			if (asyncLogger == null)
			{
				return;
			}
			asyncLogger.Log(text);
		}

		// Token: 0x06000F6B RID: 3947 RVA: 0x00029960 File Offset: 0x00028960
		public void LibraryLoader_AfterLoadingAllLibraries(object sender, LoadLibrariesEventArgs e)
		{
			bool flag = !APEnvironmentFacade.Instance.LanguageModelMgr.EnableBackgroundLoading;
			bool flag2 = APEnvironmentFacade.Instance.ExistsPrimaryProject && APEnvironmentFacade.Instance.PrimaryProjectHandle == e.ProjectHandle;
			if (flag && flag2 && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35800)
			{
				APEnvironmentFacade.Instance.InvokeInPrimaryThread(new LanguageModelManagerConsolidated.LateLibraryLoadFinished_Delegate(LanguageModelManagerConsolidated._LMM.OnLateLibraryLoadFinished), new object[0], true);
			}
		}

		// Token: 0x06000F6C RID: 3948 RVA: 0x000299D8 File Offset: 0x000289D8
		public void RemoveItem(string stLibraryId)
		{
			LQueue<IDelayedLoadItem> pendingItems = this._pendingItems;
			lock (pendingItems)
			{
				foreach (IDelayedLoadItem delayedLoadItem in this._pendingItems)
				{
					if (delayedLoadItem is DelayedLibraryLoadItem && (delayedLoadItem as DelayedLibraryLoadItem).LibraryId == stLibraryId)
					{
						(delayedLoadItem as DelayedLibraryLoadItem).Removed = true;
					}
				}
			}
			LList<IDelayedLoadItem> incompleteItems = this._incompleteItems;
			lock (incompleteItems)
			{
				foreach (IDelayedLoadItem delayedLoadItem2 in this._incompleteItems)
				{
					if (delayedLoadItem2 is DelayedLibraryLoadItem && (delayedLoadItem2 as DelayedLibraryLoadItem).LibraryId == stLibraryId)
					{
						(delayedLoadItem2 as DelayedLibraryLoadItem).Removed = true;
					}
				}
			}
		}

		// Token: 0x06000F6D RID: 3949 RVA: 0x00029B04 File Offset: 0x00028B04
		public void EnqueueRefContextItem(string stApplicationFileName, string stProcessText, ISideCarEntry sideCarEntry)
		{
			this.EnqueueItem(new DelayedReferenceContextLoadItem(stApplicationFileName, stProcessText, sideCarEntry));
		}

		// Token: 0x06000F6E RID: 3950 RVA: 0x00029B14 File Offset: 0x00028B14
		public void EnqueueItem(IDelayedLoadItem item)
		{
			LQueue<IDelayedLoadItem> pendingItems = this._pendingItems;
			lock (pendingItems)
			{
				foreach (IDelayedLoadItem delayedLoadItem in this._pendingItems)
				{
					if (delayedLoadItem.IsEqual(item))
					{
						if (delayedLoadItem is DelayedLibraryLoadItem)
						{
							(delayedLoadItem as DelayedLibraryLoadItem).CallerData = (item as DelayedLibraryLoadItem).CallerData;
							(delayedLoadItem as DelayedLibraryLoadItem).Removed = false;
						}
						return;
					}
				}
				this._pendingItems.Enqueue(item);
			}
		}

		// Token: 0x06000F6F RID: 3951 RVA: 0x00029BCC File Offset: 0x00028BCC
		internal void Process(bool bWaitForCompletion, IProgressCallback callback)
		{
			if (bWaitForCompletion && callback == null)
			{
				if (!this.AnythingToDo)
				{
					return;
				}
				try
				{
					callback = APEnvironmentFacade.Instance.StartLengthyOperation();
					Debug.Assert(callback != null);
					this.Process(true, callback);
					return;
				}
				finally
				{
					if (callback != null)
					{
						callback.Finish();
					}
				}
			}
			if (bWaitForCompletion)
			{
				this._callback = callback;
			}
			if (this._callback != null)
			{
				LQueue<IDelayedLoadItem> pendingItems = this._pendingItems;
				int count;
				lock (pendingItems)
				{
					count = this._pendingItems.Count;
				}
				if (count > 0 || (bWaitForCompletion && this._ctrlThread != null))
				{
					this._callback.NextTask(Strings.LoadingLibraries, count, Strings.Libraries);
				}
			}
			if (this._ctrlThread == null)
			{
				this._ctrlThread = new Thread(new ThreadStart(this.ThreadFunc))
				{
					Name = "Delayed-Loader-Control"
				};
				this._ctrlThread.Start();
			}
			try
			{
				if (bWaitForCompletion)
				{
					this.ActiveWaitForControlThread();
				}
			}
			catch (Exception arg)
			{
				this.DebugLog(string.Format("Failed ActiveWaitForControlThread in Process: {0}", arg));
			}
			IProgressCallback callback2 = this._callback;
			if (callback2 != null)
			{
				callback2.NextTask(string.Empty, 0, string.Empty);
			}
			this._callback = null;
		}

		// Token: 0x06000F70 RID: 3952 RVA: 0x00029D18 File Offset: 0x00028D18
		private void CreateAndStartThreads()
		{
			int maxDegreeOfParallelism = SmartCodingOptionsHelper.MaxDegreeOfParallelism;
			this._threads = new LList<Thread>(maxDegreeOfParallelism);
			for (int i = 0; i < maxDegreeOfParallelism; i++)
			{
				Thread thread = this.CreateThread(i);
				this._threads.Add(thread);
				thread.Start();
			}
		}

		// Token: 0x06000F71 RID: 3953 RVA: 0x00029D60 File Offset: 0x00028D60
		private Thread CreateThread(int i)
		{
			return new Thread(new ThreadStart(this.ThreadLoadFunc))
			{
				CurrentCulture = CultureInfo.CurrentCulture,
				CurrentUICulture = CultureInfo.CurrentUICulture,
				Priority = ThreadPriority.Lowest,
				Name = string.Format("Delayed-Loader-Worker-{0}", i)
			};
		}

		// Token: 0x06000F72 RID: 3954 RVA: 0x00029DB4 File Offset: 0x00028DB4
		private bool AtLeastOneThreadAlive()
		{
			using (IEnumerator<Thread> enumerator = this._threads.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.IsAlive)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06000F73 RID: 3955 RVA: 0x00029E08 File Offset: 0x00028E08
		public void CompleteLanguageModel(IProgressCallback callback)
		{
			bool flag = this.AnythingToDo;
			int iPrimaryProjectHandle;
			if (APEnvironmentFacade.Instance.DoesPrimaryProjectExist(out iPrimaryProjectHandle))
			{
				flag = DelayedLoader.ForceLoadingProjectsProvidingLanguageModel(flag, iPrimaryProjectHandle);
				APEnvironmentFacade.Instance.LanguageModelMgr.EnsureImplicitLanguageModelPresent();
			}
			if (flag)
			{
				this.Process(true, callback);
			}
		}

		// Token: 0x06000F74 RID: 3956 RVA: 0x00029E4C File Offset: 0x00028E4C
		private static bool ForceLoadingProjectsProvidingLanguageModel(bool bAnythingToDo, int iPrimaryProjectHandle)
		{
			LDictionary<int, bool> ldictionary = new LDictionary<int, bool>();
			ldictionary[iPrimaryProjectHandle] = false;
			for (;;)
			{
				int num = -1;
				foreach (KeyValuePair<int, bool> keyValuePair in ldictionary)
				{
					if (!keyValuePair.Value)
					{
						num = keyValuePair.Key;
						break;
					}
				}
				if (num >= 0)
				{
					Debug.Assert(APEnvironmentFacade.Instance.ExistProject(num));
					bAnythingToDo = (bAnythingToDo || !APEnvironmentFacade.Instance.IsLoadProjectFinished(num));
					APEnvironmentFacade.Instance.FinishLoadProject(num);
					ldictionary[num] = true;
					using (IEnumerator<int> enumerator2 = APEnvironmentFacade.Instance.GetAllProjectsWithAttribute(ProjectAttributes.ProvidesLanguageModel).GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							int num2 = enumerator2.Current;
							if (!ldictionary.ContainsKey(num2))
							{
								ldictionary[num2] = false;
							}
						}
						continue;
					}
					break;
				}
				break;
			}
			return bAnythingToDo;
		}

		// Token: 0x06000F75 RID: 3957 RVA: 0x00029F54 File Offset: 0x00028F54
		internal void StopProcessing()
		{
			this._bStopThread = true;
			LQueue<IDelayedLoadItem> pendingItems;
			try
			{
				this.ActiveWaitForControlThread();
				pendingItems = this._pendingItems;
				lock (pendingItems)
				{
					this._pendingItems.Clear();
				}
				this.WaitForIncompleteItems();
			}
			catch (Exception arg)
			{
				this.DebugLog(string.Format("Failed ActiveWaitForControlThread in StopProcessing: {0}", arg));
			}
			pendingItems = this._pendingItems;
			lock (pendingItems)
			{
				this._pendingItems.Clear();
			}
			this._ctrlThread = null;
			this._threads.Clear();
		}

		// Token: 0x17000470 RID: 1136
		// (get) Token: 0x06000F76 RID: 3958 RVA: 0x0002A014 File Offset: 0x00029014
		internal int PendingLibrariesInLoadQueue
		{
			get
			{
				LQueue<IDelayedLoadItem> pendingItems = this._pendingItems;
				IDelayedLoadItem[] array;
				lock (pendingItems)
				{
					array = this._pendingItems.ToArray();
				}
				int num = 0;
				IDelayedLoadItem[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					if (array2[i] is DelayedLibraryLoadItem)
					{
						num++;
					}
				}
				return num;
			}
		}

		// Token: 0x17000471 RID: 1137
		// (get) Token: 0x06000F77 RID: 3959 RVA: 0x0002A084 File Offset: 0x00029084
		internal bool AnythingToDo
		{
			get
			{
				LQueue<IDelayedLoadItem> pendingItems = this._pendingItems;
				bool result;
				lock (pendingItems)
				{
					LList<IDelayedLoadItem> incompleteItems = this._incompleteItems;
					lock (incompleteItems)
					{
						result = (this._pendingItems.Count > 0 || this._incompleteItems.Count > 0 || this._ctrlThread != null);
					}
				}
				return result;
			}
		}

		// Token: 0x06000F78 RID: 3960 RVA: 0x0002A114 File Offset: 0x00029114
		private void ThreadLoadFunc()
		{
			try
			{
				this._ThreadLoadFunc();
			}
			catch (Exception ex)
			{
				this.DebugLog(string.Concat(new string[]
				{
					"Exception in thread: ",
					Thread.CurrentThread.Name,
					": ",
					ex.Message,
					" ",
					ex.StackTrace
				}));
			}
		}

		// Token: 0x06000F79 RID: 3961 RVA: 0x0002A184 File Offset: 0x00029184
		private void _ThreadLoadFunc()
		{
			long num = 0L;
			int num2 = 0;
			this.DebugLog("Start ThreadLocalFunc. Thread: " + Thread.CurrentThread.Name);
			for (;;)
			{
				LQueue<IDelayedLoadItem> pendingItems = this._pendingItems;
				IDelayedLoadItem delayedLoadItem;
				bool flag2;
				lock (pendingItems)
				{
					if (this._bStopThread)
					{
						this._pendingItems.Clear();
					}
					if (this._pendingItems.Count == 0)
					{
						break;
					}
					delayedLoadItem = this._pendingItems.Dequeue();
					flag2 = true;
					foreach (IDelayedLoadItem delayedLoadItem2 in this._pendingItems)
					{
						DelayedLibraryLoadItem delayedLibraryLoadItem = delayedLoadItem2 as DelayedLibraryLoadItem;
						if (delayedLibraryLoadItem != null && !delayedLibraryLoadItem.Removed)
						{
							flag2 = false;
							break;
						}
					}
				}
				num2++;
				if (this._callback != null && (DateTime.UtcNow.Ticks - num >= 2500000L || flag2))
				{
					this.DebugLog("Start _callback.TaskProgress " + delayedLoadItem.ProcessText + ". Thread: " + Thread.CurrentThread.Name);
					APEnvironmentFacade.Instance.InvokeInPrimaryThread(new DelayedLoader.TaskProgressDelegate(this._callback.TaskProgress), new object[]
					{
						delayedLoadItem.ProcessText,
						num2
					}, false);
					this.DebugLog("Finish _callback.TaskProgress " + delayedLoadItem.ProcessText + ". Thread: " + Thread.CurrentThread.Name);
					num = DateTime.UtcNow.Ticks;
					num2 = 0;
				}
				this.DebugLog("Start ProcessLoad " + delayedLoadItem.ProcessText + ". Thread: " + Thread.CurrentThread.Name);
				delayedLoadItem.ProcessLoad(flag2, this._asyncLogger);
				this.DebugLog("Finish ProcessLoad " + delayedLoadItem.ProcessText + ". Thread: " + Thread.CurrentThread.Name);
				LList<IDelayedLoadItem> incompleteItems = this._incompleteItems;
				lock (incompleteItems)
				{
					this._incompleteItems.Add(delayedLoadItem);
					continue;
				}
				break;
			}
			this.DebugLog("Finish ThreadLocalFunc. Thread: " + Thread.CurrentThread.Name);
		}

		// Token: 0x06000F7A RID: 3962 RVA: 0x0002A3D8 File Offset: 0x000293D8
		private void ThreadFunc()
		{
			this.DebugLog("Start ControlThread. Thread: " + Thread.CurrentThread.Name);
			this._bStopThread = false;
			this.CreateAndStartThreads();
			this.ActiveWait();
			if (this._bStopThread)
			{
				this._ctrlThread = null;
				this.DebugLog("Abort after load. Thread: " + Thread.CurrentThread.Name);
				return;
			}
			this.WaitForIncompleteItems();
			this._ctrlThread = null;
			if (this._bStopThread)
			{
				this.DebugLog("Abort after wait for incomplete items. Thread: " + Thread.CurrentThread.Name);
				return;
			}
			APEnvironmentFacade.Instance.InvokeInPrimaryThread(new LanguageModelManagerConsolidated.LateLibraryLoadFinished_Delegate(LanguageModelManagerConsolidated._LMM.OnLateLibraryLoadFinished), new object[0], true);
			this.DebugLog("Finish control thread. Thread: " + Thread.CurrentThread.Name);
		}

		// Token: 0x06000F7B RID: 3963 RVA: 0x0002A4A8 File Offset: 0x000294A8
		private void WaitForIncompleteItems()
		{
			this.DebugLog("Start WaitForIncompleteItems");
			for (;;)
			{
				Thread.Sleep(0);
				LList<IDelayedLoadItem> incompleteItems = this._incompleteItems;
				lock (incompleteItems)
				{
					for (int i = this._incompleteItems.Count - 1; i >= 0; i--)
					{
						int num = 0;
						foreach (IDelayedLoadItem delayedLoadItem in this._incompleteItems)
						{
							if (!delayedLoadItem.IsCompleted && !delayedLoadItem.DuringPostProcess && delayedLoadItem is DelayedLibraryLoadItem)
							{
								num++;
							}
						}
						if (this._bStopThread)
						{
							this._incompleteItems[i].IsCompleted = true;
						}
						bool isCompleted = this._incompleteItems[i].IsCompleted;
						if (!isCompleted && !this._incompleteItems[i].DuringPostProcess)
						{
							this._incompleteItems[i].PostProcessLoad(num == 1);
						}
						if (isCompleted)
						{
							this._incompleteItems.RemoveAt(i);
						}
					}
					if (this._incompleteItems.Count != 0)
					{
						continue;
					}
					this.DebugLog("Finish WaitForIncompleteItems");
				}
				break;
			}
		}

		// Token: 0x06000F7C RID: 3964 RVA: 0x0002A5F0 File Offset: 0x000295F0
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high")]
		private void ActiveWaitForControlThread()
		{
			if (this._ctrlThread == null)
			{
				return;
			}
			try
			{
				this.DebugLog("Start ActiveWaitForControlThread");
				DateTime now = DateTime.Now;
				while (this._ctrlThread != null && !this._ctrlThread.Join(100))
				{
					Application.DoEvents();
					if (DateTime.Now - now > TimeSpan.FromMinutes(5.0))
					{
						foreach (Thread thread in this._threads)
						{
							if (thread.IsAlive)
							{
								try
								{
									thread.Abort();
								}
								catch
								{
								}
							}
						}
					}
				}
				this.DebugLog("Finish ActiveWaitForControlThread");
			}
			catch (Exception arg)
			{
				this.DebugLog(string.Format("Exception in ActiveWaitForControlThread: {0}", arg));
			}
		}

		// Token: 0x06000F7D RID: 3965 RVA: 0x0002A6DC File Offset: 0x000296DC
		private void ActiveWait()
		{
			try
			{
				this.DebugLog("Start ActiveWait");
				while (this.AtLeastOneThreadAlive())
				{
					foreach (Thread thread in this._threads)
					{
						if (thread.IsAlive)
						{
							thread.Join(100);
						}
					}
				}
				this.DebugLog("Finish ActiveWait");
			}
			catch (Exception arg)
			{
				this.DebugLog(string.Format("Exception in ActiveWait: {0}", arg));
			}
		}

		// Token: 0x06000F7E RID: 3966 RVA: 0x0002A778 File Offset: 0x00029778
		public void AttachAfterLoadingAllLibraries()
		{
			APEnvironmentFacade.Instance.AfterLoadingAllLibraries += this.LibraryLoader_AfterLoadingAllLibraries;
		}

		// Token: 0x04000388 RID: 904
		private readonly AsyncLogger _asyncLogger;

		// Token: 0x04000389 RID: 905
		private LList<Thread> _threads = new LList<Thread>();

		// Token: 0x0400038A RID: 906
		private Thread _ctrlThread;

		// Token: 0x0400038B RID: 907
		private IProgressCallback _callback;

		// Token: 0x0400038C RID: 908
		private bool _bStopThread;

		// Token: 0x0400038D RID: 909
		private readonly LQueue<IDelayedLoadItem> _pendingItems = new LQueue<IDelayedLoadItem>();

		// Token: 0x0400038E RID: 910
		private readonly LList<IDelayedLoadItem> _incompleteItems = new LList<IDelayedLoadItem>();

		// Token: 0x020002A3 RID: 675
		// (Invoke) Token: 0x06002B71 RID: 11121
		private delegate void TaskProgressDelegate(string st, int nIncrement);
	}
}
