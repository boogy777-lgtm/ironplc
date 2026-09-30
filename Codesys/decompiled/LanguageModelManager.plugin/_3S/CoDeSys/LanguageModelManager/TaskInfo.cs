using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000C3 RID: 195
	[TypeGuid("{a2ede959-e5fe-4f81-9037-a646588f9067}")]
	[StorageVersion("3.3.0.0")]
	public class TaskInfo : GenericObject2, ITaskInfo2, ITaskInfo
	{
		// Token: 0x06000BFA RID: 3066 RVA: 0x0001EA8C File Offset: 0x0001DA8C
		public TaskInfo()
		{
		}

		// Token: 0x06000BFB RID: 3067 RVA: 0x0001EAB8 File Offset: 0x0001DAB8
		public TaskInfo(Guid guidObject, Guid guidTask, string stTaskName, string stParentTaskName)
		{
			this.m_guidObject = guidObject;
			this.m_guidTask = guidTask;
			this.m_stTaskName = stTaskName;
			this.m_stParentTaskName = stParentTaskName;
		}

		// Token: 0x170002F1 RID: 753
		// (get) Token: 0x06000BFC RID: 3068 RVA: 0x0001EB09 File Offset: 0x0001DB09
		public Guid ObjectGuid
		{
			get
			{
				return this.m_guidObject;
			}
		}

		// Token: 0x170002F2 RID: 754
		// (get) Token: 0x06000BFD RID: 3069 RVA: 0x0001EB11 File Offset: 0x0001DB11
		public string ParentTaskName
		{
			get
			{
				return this.m_stParentTaskName;
			}
		}

		// Token: 0x170002F3 RID: 755
		// (get) Token: 0x06000BFE RID: 3070 RVA: 0x0001EB19 File Offset: 0x0001DB19
		public Guid TaskGuid
		{
			get
			{
				return this.m_guidTask;
			}
		}

		// Token: 0x170002F4 RID: 756
		// (get) Token: 0x06000BFF RID: 3071 RVA: 0x0001EB21 File Offset: 0x0001DB21
		public string TaskName
		{
			get
			{
				return this.m_stTaskName;
			}
		}

		// Token: 0x0400020F RID: 527
		[DefaultSerialization("GuidObject")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private Guid m_guidObject = Guid.Empty;

		// Token: 0x04000210 RID: 528
		[DefaultSerialization("GuidTask")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private Guid m_guidTask = Guid.Empty;

		// Token: 0x04000211 RID: 529
		[DefaultSerialization("TaskName")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string m_stTaskName = string.Empty;

		// Token: 0x04000212 RID: 530
		[DefaultSerialization("ParentTaskName")]
		[StorageVersion("3.5.10.0")]
		[StorageDefaultValue(null)]
		[Obfuscation(Feature = "rename")]
		private string m_stParentTaskName;
	}
}
