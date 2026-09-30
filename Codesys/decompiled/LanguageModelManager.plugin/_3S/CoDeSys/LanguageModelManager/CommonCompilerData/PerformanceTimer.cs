using System;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.CommonCompilerData
{
	// Token: 0x020001CC RID: 460
	internal class PerformanceTimer
	{
		// Token: 0x17000879 RID: 2169
		// (get) Token: 0x06002061 RID: 8289 RVA: 0x00059858 File Offset: 0x00058858
		// (set) Token: 0x06002062 RID: 8290 RVA: 0x00059860 File Offset: 0x00058860
		public bool Output { get; set; }

		// Token: 0x06002063 RID: 8291 RVA: 0x00059869 File Offset: 0x00058869
		public PerformanceTimer()
		{
		}

		// Token: 0x06002064 RID: 8292 RVA: 0x000598A8 File Offset: 0x000588A8
		public PerformanceTimer(string defaultId)
		{
			this.DefaultId = defaultId;
		}

		// Token: 0x06002065 RID: 8293 RVA: 0x000598F8 File Offset: 0x000588F8
		public PerformanceTimer(bool bOutput)
		{
			this.Output = bOutput;
		}

		// Token: 0x06002066 RID: 8294 RVA: 0x00059948 File Offset: 0x00058948
		internal void StartTiming()
		{
			this.StartTiming(this.DefaultId);
		}

		// Token: 0x06002067 RID: 8295 RVA: 0x00059956 File Offset: 0x00058956
		internal void StartTiming(string id)
		{
			this._startTimes[id] = DateTime.Now;
		}

		// Token: 0x06002068 RID: 8296 RVA: 0x00059969 File Offset: 0x00058969
		internal void Snapshot(string id)
		{
			this.StartTiming(id);
		}

		// Token: 0x06002069 RID: 8297 RVA: 0x00059972 File Offset: 0x00058972
		internal void StopTimingWithOutput(string stMessageFmt)
		{
			this.StopTiming(this.DefaultId);
			this.OutputMeasurement(this.DefaultId, stMessageFmt);
		}

		// Token: 0x0600206A RID: 8298 RVA: 0x0005998D File Offset: 0x0005898D
		internal void StopTiming()
		{
			this.StopTiming(this.DefaultId);
		}

		// Token: 0x0600206B RID: 8299 RVA: 0x0005999B File Offset: 0x0005899B
		internal void StopTiming(string id)
		{
			this._endTimes[id] = DateTime.Now;
		}

		// Token: 0x0600206C RID: 8300 RVA: 0x000599B0 File Offset: 0x000589B0
		internal void OutputProgress(string id, string stMessageFmt)
		{
			if (this.Output)
			{
				long lTime = (DateTime.Now.Ticks - this._startTimes[id].Ticks) / 10000L;
				this.OutputMeasurement(lTime, stMessageFmt);
			}
		}

		// Token: 0x0600206D RID: 8301 RVA: 0x000599F7 File Offset: 0x000589F7
		internal TimeSpan GetDuration()
		{
			return this.GetDuration(this.DefaultId);
		}

		// Token: 0x0600206E RID: 8302 RVA: 0x00059A05 File Offset: 0x00058A05
		internal TimeSpan GetDuration(string id)
		{
			return this._endTimes[id] - this._startTimes[id];
		}

		// Token: 0x0600206F RID: 8303 RVA: 0x00059A24 File Offset: 0x00058A24
		internal void OutputMeasurement(string stMessageFmt)
		{
			this.OutputMeasurement(this.DefaultId, stMessageFmt);
		}

		// Token: 0x06002070 RID: 8304 RVA: 0x00059A34 File Offset: 0x00058A34
		internal void OutputMeasurement(string id, string stMessageFmt)
		{
			if (!this._endTimes.ContainsKey(id))
			{
				return;
			}
			this.OutputMeasurement((this._endTimes[id].Ticks - this._startTimes[id].Ticks) / 10000L, stMessageFmt);
		}

		// Token: 0x06002071 RID: 8305 RVA: 0x00059A88 File Offset: 0x00058A88
		internal void OutputMeasurement(long lTime, string stMessageFmt)
		{
			if (this.Output)
			{
				string stError = string.Format(stMessageFmt, lTime);
				_ICompilerMessage message = new CompilerMessage(null, stError, Severity.Text, MessageId.None);
				APEnvironmentFacade.Instance.AddMessage(this._cmc, message);
			}
		}

		// Token: 0x06002072 RID: 8306 RVA: 0x00059AC8 File Offset: 0x00058AC8
		internal void OutputDuration(string endTimeId, string startTimeId, string stMessageFmt)
		{
			this.OutputMeasurement((this._startTimes[endTimeId].Ticks - this._startTimes[startTimeId].Ticks) / 10000L, stMessageFmt);
		}

		// Token: 0x06002073 RID: 8307 RVA: 0x00059B0C File Offset: 0x00058B0C
		internal string OutputMeasurementTMStyle()
		{
			return this.OutputMeasurementTMStyle(this.DefaultId);
		}

		// Token: 0x06002074 RID: 8308 RVA: 0x00059B1C File Offset: 0x00058B1C
		internal string OutputMeasurementTMStyle(string id)
		{
			return string.Format("{4}Operation '{0}' : start time {1}, end time {2}, duration {3}", new object[]
			{
				id,
				this._startTimes[id].ToLongTimeString(),
				this._endTimes[id].ToLongTimeString(),
				this.GetDuration(id),
				Environment.NewLine
			});
		}

		// Token: 0x04000666 RID: 1638
		private readonly string DefaultId = "__default__";

		// Token: 0x04000668 RID: 1640
		private readonly LDictionary<string, DateTime> _startTimes = new LDictionary<string, DateTime>();

		// Token: 0x04000669 RID: 1641
		private readonly LDictionary<string, DateTime> _endTimes = new LDictionary<string, DateTime>();

		// Token: 0x0400066A RID: 1642
		private readonly IMessageCategory _cmc = APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory;

		// Token: 0x020002CD RID: 717
		// (Invoke) Token: 0x06002C54 RID: 11348
		public delegate void OutputMessageDelegate(string msg);
	}
}
