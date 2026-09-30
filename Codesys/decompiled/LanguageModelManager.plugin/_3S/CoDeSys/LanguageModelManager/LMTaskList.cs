using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200010F RID: 271
	internal class LMTaskList : ILMTaskList
	{
		// Token: 0x0600146F RID: 5231 RVA: 0x0003C237 File Offset: 0x0003B237
		internal LMTaskList(Guid guidTaskConfig)
		{
			this._guidTaskConfig = guidTaskConfig;
		}

		// Token: 0x06001470 RID: 5232 RVA: 0x0003C251 File Offset: 0x0003B251
		public void AddTask(ITaskInfo task)
		{
			this._tasklist.Add(task);
		}

		// Token: 0x1700059C RID: 1436
		// (get) Token: 0x06001471 RID: 5233 RVA: 0x0003C25F File Offset: 0x0003B25F
		public ITaskInfo[] Tasks
		{
			get
			{
				return this._tasklist.ToArray();
			}
		}

		// Token: 0x1700059D RID: 1437
		// (get) Token: 0x06001472 RID: 5234 RVA: 0x0003C26C File Offset: 0x0003B26C
		// (set) Token: 0x06001473 RID: 5235 RVA: 0x0003C274 File Offset: 0x0003B274
		public Guid TaskConfigGuid
		{
			get
			{
				return this._guidTaskConfig;
			}
			set
			{
				this._guidTaskConfig = value;
			}
		}

		// Token: 0x1700059E RID: 1438
		// (get) Token: 0x06001474 RID: 5236 RVA: 0x0003C27D File Offset: 0x0003B27D
		// (set) Token: 0x06001475 RID: 5237 RVA: 0x0003C285 File Offset: 0x0003B285
		public Guid ObjectGuid
		{
			get
			{
				return this._guidObject;
			}
			set
			{
				this._guidObject = value;
			}
		}

		// Token: 0x040004AF RID: 1199
		private Guid _guidTaskConfig;

		// Token: 0x040004B0 RID: 1200
		private Guid _guidObject;

		// Token: 0x040004B1 RID: 1201
		private readonly LList<ITaskInfo> _tasklist = new LList<ITaskInfo>();
	}
}
