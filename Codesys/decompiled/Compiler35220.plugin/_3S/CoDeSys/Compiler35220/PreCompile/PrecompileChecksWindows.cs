using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using \u0005;
using \u0012;
using \u0014;
using _3S.CoDeSys.Compiler35220.CompilerVersion;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0081;

namespace _3S.CoDeSys.Compiler35220.PreCompile
{
	// Token: 0x02000180 RID: 384
	public class PrecompileChecksWindows : _ICheckerThread, _ICheckerThread2
	{
		// Token: 0x1700059E RID: 1438
		// (get) Token: 0x06001A34 RID: 6708 RVA: 0x00053B9C File Offset: 0x00051D9C
		// (set) Token: 0x06001A35 RID: 6709 RVA: 0x00053BA4 File Offset: 0x00051DA4
		private global::\u0005.\u0002 LMItemQueue { get; set; } = new global::\u0005.\u0002();

		// Token: 0x06001A36 RID: 6710 RVA: 0x00053BB0 File Offset: 0x00051DB0
		public PrecompileChecksWindows()
		{
			APEnvironment.ObjectMgr.ProjectClosing += this.\u0001;
			APEnvironment.CompilerVersionEventMgr.CompilerVersionChanged += this.\u0001;
			this.\u0001 = APEnvironment.Engine;
			this.\u0001.Projects.PrimaryProjectSwitched += this.\u0001;
			this.\u0001 = APEnvironment.Engine.Projects;
			this.\u0001 = APEnvironment.Engine.Projects.PrimaryProject;
			this.\u0001 = (ILMPreCompileCheckerService)APEnvironment.LMServiceProvider.PreCompileService;
			this.\u0001 = APEnvironment.LanguageModelMgr;
		}

		// Token: 0x06001A37 RID: 6711 RVA: 0x00053C94 File Offset: 0x00051E94
		private void \u0001(IProject \u0002, IProject \u0003)
		{
			this.\u0001();
			this.\u0001 = \u0003;
			this.\u0002();
		}

		// Token: 0x1700059F RID: 1439
		// (get) Token: 0x06001A38 RID: 6712 RVA: 0x00053CAC File Offset: 0x00051EAC
		// (set) Token: 0x06001A39 RID: 6713 RVA: 0x00053CBC File Offset: 0x00051EBC
		public bool AllDone
		{
			get
			{
				return this.\u0001.PrecompileChecksDone;
			}
			set
			{
				this.\u0001.PrecompileChecksDone = value;
			}
		}

		// Token: 0x170005A0 RID: 1440
		// (get) Token: 0x06001A3A RID: 6714 RVA: 0x00053CCC File Offset: 0x00051ECC
		// (set) Token: 0x06001A3B RID: 6715 RVA: 0x00053CD4 File Offset: 0x00051ED4
		public static bool CheckInNoUIMode { get; set; }

		// Token: 0x170005A1 RID: 1441
		// (get) Token: 0x06001A3C RID: 6716 RVA: 0x00053CDC File Offset: 0x00051EDC
		// (set) Token: 0x06001A3D RID: 6717 RVA: 0x00053CE4 File Offset: 0x00051EE4
		public static bool Attached { get; set; }

		// Token: 0x170005A2 RID: 1442
		// (get) Token: 0x06001A3E RID: 6718 RVA: 0x00053CEC File Offset: 0x00051EEC
		// (set) Token: 0x06001A3F RID: 6719 RVA: 0x00053CF4 File Offset: 0x00051EF4
		public static bool DebugOutput { get; set; }

		// Token: 0x06001A40 RID: 6720 RVA: 0x00053CFC File Offset: 0x00051EFC
		public void EnablePrecompileChecksInNoUIMode()
		{
			PrecompileChecksWindows.CheckInNoUIMode = true;
		}

		// Token: 0x06001A41 RID: 6721 RVA: 0x00053D04 File Offset: 0x00051F04
		public void DisablePrecompileChecksInNoUIMode()
		{
			PrecompileChecksWindows.CheckInNoUIMode = false;
		}

		// Token: 0x06001A42 RID: 6722 RVA: 0x00053D0C File Offset: 0x00051F0C
		private void \u0001()
		{
			if (this.\u0001 != null)
			{
				this.\u0001.ActiveApplicationChanged -= this.\u0001;
			}
		}

		// Token: 0x06001A43 RID: 6723 RVA: 0x00053D30 File Offset: 0x00051F30
		private void \u0002()
		{
			if (this.\u0001 != null)
			{
				this.\u0001.ActiveApplicationChanged += this.\u0001;
			}
		}

		// Token: 0x06001A44 RID: 6724 RVA: 0x00053D54 File Offset: 0x00051F54
		private void \u0001(IProject \u0002)
		{
			if (CompilerVersionsProvider.\u0001(APEnvironmentFacade.Instance.CompilerVersionToUseInternal()))
			{
				this.LMItemQueue.\u0002();
				this.\u0010();
			}
		}

		// Token: 0x06001A45 RID: 6725 RVA: 0x00053D78 File Offset: 0x00051F78
		private void \u0001(object \u0002, CompilerVersionChangedEventArgs \u0003)
		{
			if (!CompilerVersionsProvider.\u0001(\u0003.NewCompilerVersion))
			{
				this.\u000F();
			}
		}

		// Token: 0x06001A46 RID: 6726 RVA: 0x00053D90 File Offset: 0x00051F90
		private void \u0001(object \u0002, ProjectClosingEventArgs \u0003)
		{
			if (this.\u0001.PrimaryProject != null && \u0003.ProjectHandle == this.\u0001.PrimaryProject.Handle)
			{
				this.Disable();
			}
		}

		// Token: 0x06001A47 RID: 6727 RVA: 0x00053DC0 File Offset: 0x00051FC0
		private void \u0003()
		{
			object u = this.\u0001;
			lock (u)
			{
				if (this.\u0001 != null && this.\u0001 != global::\u0012.\u0001.MaxDegreeOfParallelism)
				{
					this.\u000F();
				}
				if (this.\u0001 == null)
				{
					this.\u0001 = global::\u0012.\u0001.MaxDegreeOfParallelism;
					this.\u0001 = new CancellationTokenSource();
					this.\u0001 = new LList<Thread>(this.\u0001);
					this.\u0001 = new LList<bool>(this.\u0001);
					for (int i = 0; i < this.\u0001; i++)
					{
						this.\u0001.Add(false);
						Thread thread = this.\u0001(i);
						this.\u0001.Add(thread);
						thread.IsBackground = true;
						thread.Start();
					}
				}
			}
		}

		// Token: 0x06001A48 RID: 6728 RVA: 0x00053E94 File Offset: 0x00052094
		private Thread \u0001(int \u0002)
		{
			PrecompileChecksWindows.\u0001 u = new PrecompileChecksWindows.\u0001();
			u.\u0001 = this;
			u.\u0001 = \u0002;
			return new Thread(new ThreadStart(u.\u0001))
			{
				CurrentCulture = CultureInfo.CurrentCulture,
				CurrentUICulture = CultureInfo.CurrentUICulture,
				Priority = ThreadPriority.Lowest
			};
		}

		// Token: 0x06001A49 RID: 6729 RVA: 0x00053EE4 File Offset: 0x000520E4
		public void Enable()
		{
			object u = this.\u0001;
			lock (u)
			{
				this.\u0001 = true;
			}
		}

		// Token: 0x06001A4A RID: 6730 RVA: 0x00053F28 File Offset: 0x00052128
		public void Disable()
		{
			object u = this.\u0001;
			lock (u)
			{
				if (this.\u0001)
				{
					this.\u0001 = false;
					if (PrecompileChecksWindows.Attached)
					{
						Application.Idle -= this.\u0001;
					}
					PrecompileChecksWindows.Attached = false;
					this.\u0004();
				}
			}
		}

		// Token: 0x06001A4B RID: 6731 RVA: 0x00053F9C File Offset: 0x0005219C
		private void \u0004()
		{
			IProgressCallback progressCallback = null;
			DateTime now = DateTime.Now;
			this.Clear();
			while (this.\u0003())
			{
				if (progressCallback == null && DateTime.Now - now >= TimeSpan.FromMilliseconds(100.0))
				{
					progressCallback = this.\u0001.StartLengthyOperation();
					progressCallback.Abortable = false;
					progressCallback.NextTask(\u0081.\u0001.StoppingPrecompileChecks, -1, "");
				}
				try
				{
					this.\u000E();
					this.\u0001(10);
				}
				catch
				{
				}
			}
			if (progressCallback != null)
			{
				progressCallback.Finish();
			}
		}

		// Token: 0x06001A4C RID: 6732 RVA: 0x00054038 File Offset: 0x00052238
		public void TryStart()
		{
			object u = this.\u0001;
			lock (u)
			{
				if (this.\u0001)
				{
					if (PrecompileChecksWindows.Attached)
					{
						this.\u0008();
					}
					else
					{
						this.\u0005();
					}
				}
			}
		}

		// Token: 0x06001A4D RID: 6733 RVA: 0x00054094 File Offset: 0x00052294
		internal void \u0005()
		{
			if (this.\u0001.Frame == null && !PrecompileChecksWindows.CheckInNoUIMode)
			{
				return;
			}
			if (!PrecompileChecksWindows.Attached)
			{
				this.Clear();
				long ticks = DateTime.Now.Ticks;
				this.\u0010();
				long num = (DateTime.Now.Ticks - ticks) / 10000L;
				Debug.\u0002(string.Format("Time fill language model list: {0}", num));
				Application.Idle += this.\u0001;
				this.\u0008();
			}
			PrecompileChecksWindows.Attached = true;
		}

		// Token: 0x06001A4E RID: 6734 RVA: 0x00054120 File Offset: 0x00052320
		public void FinishPrecompileChecks()
		{
			this.\u0010();
			this.TryStart();
			while (!this.ChecksDone())
			{
				Thread.Yield();
			}
			this.AllDone = this.LMItemQueue.Empty;
			this.\u0007();
		}

		// Token: 0x06001A4F RID: 6735 RVA: 0x00054158 File Offset: 0x00052358
		private void \u0006()
		{
			try
			{
				PrecompileChecksWindows.DebugOutput = (Environment.GetEnvironmentVariable("AP_DEBUG_LMM_DUMP_TIMES_PRECOMPILE") != null);
			}
			catch (Exception)
			{
				PrecompileChecksWindows.DebugOutput = false;
			}
			this.\u0001 = new global::\u0014.\u0001("Precompile");
			this.\u0001.\u0001();
		}

		// Token: 0x06001A50 RID: 6736 RVA: 0x000541B0 File Offset: 0x000523B0
		private void \u0007()
		{
			global::\u0014.\u0001 u = this.\u0001;
			if (u != null)
			{
				u.\u0002();
			}
			if (PrecompileChecksWindows.DebugOutput && this.\u0001 != null)
			{
				string environmentVariable = Environment.GetEnvironmentVariable("AP_DEBUG_LMM_DUMP_TIMES_PRECOMPILE");
				if (!string.IsNullOrEmpty(environmentVariable) && this.\u0001.\u0001().Milliseconds > 10)
				{
					AuthFile.AppendAllText(environmentVariable, this.\u0001.\u0001());
				}
			}
		}

		// Token: 0x06001A51 RID: 6737 RVA: 0x00054218 File Offset: 0x00052418
		public bool ChecksDone()
		{
			return !this.\u0001();
		}

		// Token: 0x06001A52 RID: 6738 RVA: 0x00054224 File Offset: 0x00052424
		private bool \u0001()
		{
			if (!this.\u0003())
			{
				return false;
			}
			if (!this.\u0001.LateLibraryLoadFinished)
			{
				return true;
			}
			try
			{
				this.\u0001(10);
			}
			catch
			{
			}
			using (IEnumerator<_IPreCompileContext> enumerator = this.\u0001._AllApplicationPreCompileContexts().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.Dirty)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06001A53 RID: 6739 RVA: 0x000542B0 File Offset: 0x000524B0
		private void \u0001(object \u0002, EventArgs \u0003)
		{
			try
			{
				if (this.\u0001.LateLibraryLoadFinished)
				{
					this.\u0010();
					this.\u0011();
				}
			}
			catch
			{
			}
		}

		// Token: 0x06001A54 RID: 6740 RVA: 0x000542F0 File Offset: 0x000524F0
		private void \u0008()
		{
			this.\u0003();
			this.\u0006();
			this.\u000E();
		}

		// Token: 0x06001A55 RID: 6741 RVA: 0x00054304 File Offset: 0x00052504
		private void \u000E()
		{
			object obj = this.LMItemQueue.WorkQueueLocker;
			lock (obj)
			{
				for (int i = 0; i < this.\u0001.Count; i++)
				{
					this.\u0001[i] = false;
				}
				Monitor.PulseAll(this.LMItemQueue.WorkQueueLocker);
			}
		}

		// Token: 0x06001A56 RID: 6742 RVA: 0x00054378 File Offset: 0x00052578
		private bool \u0002()
		{
			if (this.\u0001 == null)
			{
				return true;
			}
			using (IEnumerator<Thread> enumerator = this.\u0001.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.ThreadState != ThreadState.Stopped)
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06001A57 RID: 6743 RVA: 0x000543D8 File Offset: 0x000525D8
		private void \u000F()
		{
			object u = this.\u0001;
			lock (u)
			{
				if (this.\u0001 != null)
				{
					this.\u0001.Cancel();
					while (!this.\u0002())
					{
						this.\u000E();
						Thread.Sleep(5);
						Application.DoEvents();
					}
					this.\u0001 = null;
				}
			}
		}

		// Token: 0x06001A58 RID: 6744 RVA: 0x0005444C File Offset: 0x0005264C
		private bool \u0003()
		{
			if (this.\u0001 == null)
			{
				return false;
			}
			object obj = this.LMItemQueue.WorkQueueLocker;
			bool result;
			lock (obj)
			{
				result = this.\u0001.Any(new Func<bool, bool>(PrecompileChecksWindows.<>c.<>9.\u0001));
			}
			return result;
		}

		// Token: 0x06001A59 RID: 6745 RVA: 0x000544C4 File Offset: 0x000526C4
		private void \u0001(int \u0002)
		{
			if (this.\u0001 == null)
			{
				return;
			}
			foreach (Thread thread in this.\u0001)
			{
				if (thread != null && thread.IsAlive)
				{
					thread.Join(\u0002);
					break;
				}
			}
		}

		// Token: 0x06001A5A RID: 6746 RVA: 0x00054528 File Offset: 0x00052728
		private void \u0002(int \u0002)
		{
			while (!this.\u0001.IsCancellationRequested)
			{
				this.\u0003(\u0002);
				while (this.\u0001 && !this.\u0001.IsCancellationRequested)
				{
					LanguageModelResult? u = this.LMItemQueue.\u0001();
					if (u == null)
					{
						break;
					}
					this.\u0001(u);
				}
			}
		}

		// Token: 0x06001A5B RID: 6747 RVA: 0x00054580 File Offset: 0x00052780
		private void \u0001(LanguageModelResult? \u0002)
		{
			if (\u0002 == null)
			{
				return;
			}
			LanguageModelResult value = \u0002.Value;
			_ISignature u = value.\u0001;
			if (u != null && !u.GetFlagInternal(SignatureFlagInternal.Checked))
			{
				_ICompiledPOU icompiledPOU = value.\u0001.GetCompiledPOU(u.ObjectGuid) as _ICompiledPOU;
				if (global::\u0005.\u0002.\u0001(u))
				{
					u.PrecompileMessages = null;
					value.\u0001.SetSignatureChecked(u);
					if (icompiledPOU != null)
					{
						icompiledPOU.SetPrecompileMessages(null);
						value.\u0001.SetPouChecked(icompiledPOU);
					}
				}
				else
				{
					value.\u0001.CheckSignature(u);
					value.\u0001.SetSignatureChecked(u);
					if (icompiledPOU != null)
					{
						value.\u0001.CheckPOUCode(icompiledPOU);
						value.\u0001.SetPouChecked(icompiledPOU);
					}
				}
				LList<_ISignature> u2 = this.\u0001;
				lock (u2)
				{
					this.\u0001.Add(u);
				}
			}
		}

		// Token: 0x06001A5C RID: 6748 RVA: 0x00054678 File Offset: 0x00052878
		private void \u0003(int \u0002)
		{
			object obj = this.LMItemQueue.WorkQueueLocker;
			lock (obj)
			{
				while (this.LMItemQueue.\u0002() == null && !this.\u0001.IsCancellationRequested)
				{
					this.\u0001[\u0002] = true;
					Monitor.Wait(this.LMItemQueue.WorkQueueLocker);
				}
				this.\u0001[\u0002] = false;
			}
		}

		// Token: 0x06001A5D RID: 6749 RVA: 0x00054708 File Offset: 0x00052908
		private void \u0010()
		{
			if (this.\u0001.PrimaryProject == null)
			{
				return;
			}
			if (this.\u0001.Frame == null && !PrecompileChecksWindows.CheckInNoUIMode)
			{
				return;
			}
			this.LMItemQueue.\u0001();
			this.AllDone = this.LMItemQueue.Empty;
			this.\u0008();
		}

		// Token: 0x06001A5E RID: 6750 RVA: 0x0005475C File Offset: 0x0005295C
		public void AddRecentLMResult(LanguageModelResult result)
		{
			this.\u0001(result);
		}

		// Token: 0x06001A5F RID: 6751 RVA: 0x00054768 File Offset: 0x00052968
		private void \u0001(LanguageModelResult \u0002)
		{
			this.LMItemQueue.\u0001(\u0002);
			this.AllDone = false;
			this.TryStart();
		}

		// Token: 0x06001A60 RID: 6752 RVA: 0x00054784 File Offset: 0x00052984
		public void Clear()
		{
			LList<_ISignature> u = this.\u0001;
			lock (u)
			{
				this.\u0001.Clear();
			}
			this.LMItemQueue.\u0003();
		}

		// Token: 0x06001A61 RID: 6753 RVA: 0x000547D8 File Offset: 0x000529D8
		private void \u0011()
		{
			LList<_ISignature> u = this.\u0001;
			LList<_ISignature> u2;
			lock (u)
			{
				u2 = this.\u0001;
				this.\u0001 = new LList<_ISignature>();
			}
			if (u2.Count > 0 && this.\u0001.EnablePrecomCheck)
			{
				this.\u0001.PrecompileErrors.ShowPrecompileErrors(null, u2);
			}
		}

		// Token: 0x04000488 RID: 1160
		private const string \u0001 = "AP_DEBUG_LMM_DUMP_TIMES_PRECOMPILE";

		// Token: 0x04000489 RID: 1161
		private const bool \u0001 = true;

		// Token: 0x0400048A RID: 1162
		private const bool \u0002 = false;

		// Token: 0x0400048B RID: 1163
		private volatile LList<_ISignature> \u0001 = new LList<_ISignature>();

		// Token: 0x0400048C RID: 1164
		[CompilerGenerated]
		private global::\u0005.\u0002 \u0001;

		// Token: 0x0400048D RID: 1165
		private int \u0001;

		// Token: 0x0400048E RID: 1166
		private LList<Thread> \u0001;

		// Token: 0x0400048F RID: 1167
		private LList<bool> \u0001 = new LList<bool>();

		// Token: 0x04000490 RID: 1168
		private CancellationTokenSource \u0001;

		// Token: 0x04000491 RID: 1169
		private readonly object \u0001 = new object();

		// Token: 0x04000492 RID: 1170
		private global::\u0014.\u0001 \u0001;

		// Token: 0x04000493 RID: 1171
		private readonly IEngine \u0001;

		// Token: 0x04000494 RID: 1172
		private readonly IProjects \u0001;

		// Token: 0x04000495 RID: 1173
		private readonly _ILanguageModelManagerConsolidated \u0001;

		// Token: 0x04000496 RID: 1174
		private readonly ILMPreCompileCheckerService \u0001;

		// Token: 0x04000497 RID: 1175
		private IProject \u0001;

		// Token: 0x04000498 RID: 1176
		[CompilerGenerated]
		private static bool \u0003;

		// Token: 0x04000499 RID: 1177
		[CompilerGenerated]
		private static bool \u0004;

		// Token: 0x0400049A RID: 1178
		[CompilerGenerated]
		private static bool \u0005;

		// Token: 0x0400049B RID: 1179
		private volatile bool \u0001 = true;

		// Token: 0x02000181 RID: 385
		[CompilerGenerated]
		private sealed class \u0001
		{
			// Token: 0x06001A63 RID: 6755 RVA: 0x0005485C File Offset: 0x00052A5C
			internal void \u0001()
			{
				this.\u0001.\u0002(this.\u0001);
			}

			// Token: 0x0400049C RID: 1180
			public PrecompileChecksWindows \u0001;

			// Token: 0x0400049D RID: 1181
			public int \u0001;
		}
	}
}
