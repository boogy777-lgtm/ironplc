using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000156 RID: 342
	internal class ProgressX : IProgress
	{
		// Token: 0x17000770 RID: 1904
		// (get) Token: 0x06001B9E RID: 7070 RVA: 0x0004E38B File Offset: 0x0004D38B
		internal static ProgressX Singleton { get; } = new ProgressX();

		// Token: 0x06001B9F RID: 7071 RVA: 0x0004E392 File Offset: 0x0004D392
		public void NotifyNextTask(IProgressCallback callback, bool bAbortable, string stTask, int nItemsTotal, string stItemUnit)
		{
			ProgressX._NotifyNextTask(callback, bAbortable, stTask, nItemsTotal, stItemUnit);
		}

		// Token: 0x06001BA0 RID: 7072 RVA: 0x0004E3A0 File Offset: 0x0004D3A0
		public void NotifyTaskProgress(IProgressCallback callback, string stItem, int nInc)
		{
			ProgressX._NotifyTaskProgress(callback, stItem, nInc);
		}

		// Token: 0x06001BA1 RID: 7073 RVA: 0x0004E3AA File Offset: 0x0004D3AA
		public void NotifyTaskProgress(IProgressCallback callback, string stItem)
		{
			ProgressX._NotifyTaskProgress(callback, stItem);
		}

		// Token: 0x06001BA2 RID: 7074 RVA: 0x0004E3B3 File Offset: 0x0004D3B3
		public void NotifyNextTask(bool bAbortable, string stTask, int nItemsTotal, string stItemUnit)
		{
			ProgressX._NotifyNextTask(this.Callback, bAbortable, stTask, nItemsTotal, stItemUnit);
		}

		// Token: 0x06001BA3 RID: 7075 RVA: 0x0004E3C5 File Offset: 0x0004D3C5
		public void NotifyTaskProgress(string stItem, int nInc)
		{
			ProgressX._NotifyTaskProgress(this.Callback, stItem, nInc);
		}

		// Token: 0x06001BA4 RID: 7076 RVA: 0x0004E3D4 File Offset: 0x0004D3D4
		public void NotifyTaskProgress(string stItem)
		{
			ProgressX._NotifyTaskProgress(this.Callback, stItem);
		}

		// Token: 0x06001BA5 RID: 7077 RVA: 0x0004E3E2 File Offset: 0x0004D3E2
		internal static void _NotifyNextTask(IProgressCallback callback, bool bAbortable, string stTask, int nItemsTotal, string stItemUnit)
		{
			if (callback != null)
			{
				callback.Abortable = bAbortable;
				callback.NextTask(stTask, nItemsTotal, stItemUnit);
				if (bAbortable && callback.Aborting)
				{
					throw new CancelledByUserException();
				}
			}
		}

		// Token: 0x06001BA6 RID: 7078 RVA: 0x0004E409 File Offset: 0x0004D409
		internal static void _NotifyTaskProgress(IProgressCallback callback, string stItem)
		{
			ProgressX._NotifyTaskProgress(callback, stItem, 1);
		}

		// Token: 0x06001BA7 RID: 7079 RVA: 0x0004E414 File Offset: 0x0004D414
		internal static void _NotifyTaskProgress(IProgressCallback callback, string stItem, int nInc)
		{
			if (callback != null)
			{
				ProgressX._nInc += nInc;
				if (DateTime.Now.Ticks - ProgressX._lastTick > 10000000L)
				{
					if (callback.Aborting)
					{
						throw new CancelledByUserException();
					}
					callback.TaskProgress(stItem, ProgressX._nInc);
					ProgressX._lastTick = DateTime.Now.Ticks;
					ProgressX._nInc = 0;
				}
			}
		}

		// Token: 0x06001BA8 RID: 7080 RVA: 0x0004E47D File Offset: 0x0004D47D
		public void StartLengthyOperation()
		{
			this._progressStack.Push(APEnvironmentFacade.Instance.StartLengthyOperation());
			ProgressX._lastTick = 0L;
		}

		// Token: 0x06001BA9 RID: 7081 RVA: 0x0004E49B File Offset: 0x0004D49B
		public void EndLengthyOperation()
		{
			if (this._progressStack.Count <= 0)
			{
				return;
			}
			IProgressCallback progressCallback = this._progressStack.Pop();
			if (progressCallback == null)
			{
				return;
			}
			progressCallback.Finish();
		}

		// Token: 0x17000771 RID: 1905
		// (get) Token: 0x06001BAA RID: 7082 RVA: 0x0004E4C1 File Offset: 0x0004D4C1
		public IProgressCallback Callback
		{
			get
			{
				if (this._progressStack.Count <= 0)
				{
					return null;
				}
				return this._progressStack.Peek();
			}
		}

		// Token: 0x040005D5 RID: 1493
		private static int _nInc = 0;

		// Token: 0x040005D6 RID: 1494
		private static long _lastTick = 0L;

		// Token: 0x040005D7 RID: 1495
		private readonly Stack<IProgressCallback> _progressStack = new Stack<IProgressCallback>();
	}
}
