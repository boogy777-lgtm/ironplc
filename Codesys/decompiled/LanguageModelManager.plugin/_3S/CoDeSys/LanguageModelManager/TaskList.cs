using System;
using System.Collections;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000C4 RID: 196
	[TypeGuid("{b60bf7c1-48fb-428f-888b-da4337fb7f3c}")]
	[StorageVersion("3.3.0.0")]
	public class TaskList : GenericObject2, _ITaskList
	{
		// Token: 0x170002F5 RID: 757
		// (get) Token: 0x06000C00 RID: 3072 RVA: 0x0001EB2C File Offset: 0x0001DB2C
		// (set) Token: 0x06000C01 RID: 3073 RVA: 0x0001EB57 File Offset: 0x0001DB57
		[DefaultSerialization("TaskListArray")]
		[StorageVersion("3.3.0.0")]
		public TaskInfo[] TaskListArray
		{
			get
			{
				TaskInfo[] array = new TaskInfo[this.m_alTaskList.Count];
				this.m_alTaskList.CopyTo(array);
				return array;
			}
			set
			{
				this.m_alTaskList.AddRange(value);
			}
		}

		// Token: 0x06000C03 RID: 3075 RVA: 0x0001EB78 File Offset: 0x0001DB78
		public void Clear()
		{
			this.m_alTaskList.Clear();
		}

		// Token: 0x170002F6 RID: 758
		// (get) Token: 0x06000C04 RID: 3076 RVA: 0x0001EB85 File Offset: 0x0001DB85
		public int Count
		{
			get
			{
				return this.m_alTaskList.Count;
			}
		}

		// Token: 0x170002F7 RID: 759
		public ITaskInfo this[int n]
		{
			get
			{
				return this.m_alTaskList[n] as TaskInfo;
			}
		}

		// Token: 0x170002F8 RID: 760
		public ITaskInfo this[Guid guidTask]
		{
			get
			{
				foreach (object obj in this.m_alTaskList)
				{
					TaskInfo taskInfo = (TaskInfo)obj;
					if (taskInfo.TaskGuid == guidTask)
					{
						return taskInfo;
					}
				}
				return null;
			}
		}

		// Token: 0x170002F9 RID: 761
		public ITaskInfo this[string stName]
		{
			get
			{
				foreach (object obj in this.m_alTaskList)
				{
					TaskInfo taskInfo = (TaskInfo)obj;
					if (taskInfo.TaskName == stName)
					{
						return taskInfo;
					}
				}
				return null;
			}
		}

		// Token: 0x06000C08 RID: 3080 RVA: 0x0001EC78 File Offset: 0x0001DC78
		public byte GetTaskIndexByGuid(Guid guidTask)
		{
			for (int i = 0; i < this.m_alTaskList.Count; i++)
			{
				if (this[i].TaskGuid == guidTask)
				{
					return (byte)i;
				}
			}
			return byte.MaxValue;
		}

		// Token: 0x06000C09 RID: 3081 RVA: 0x0001ECB8 File Offset: 0x0001DCB8
		public byte GetTaskIndexByName(string stName)
		{
			for (int i = 0; i < this.m_alTaskList.Count; i++)
			{
				if (this[i].TaskName == stName)
				{
					return (byte)i;
				}
			}
			return byte.MaxValue;
		}

		// Token: 0x06000C0A RID: 3082 RVA: 0x0001ECF7 File Offset: 0x0001DCF7
		public void AddTaskInfo(Guid guidObject, Guid guidTask, string stName)
		{
			this.m_alTaskList.Add(new TaskInfo(guidObject, guidTask, stName, null));
		}

		// Token: 0x06000C0B RID: 3083 RVA: 0x0001ED0E File Offset: 0x0001DD0E
		public void AddTaskInfo(Guid guidObject, Guid guidTask, string stName, string stParentTaskName)
		{
			this.m_alTaskList.Add(new TaskInfo(guidObject, guidTask, stName, stParentTaskName));
		}

		// Token: 0x06000C0C RID: 3084 RVA: 0x0001ED28 File Offset: 0x0001DD28
		public void RemoveTaskInfo(Guid guidObject)
		{
			for (int i = this.m_alTaskList.Count - 1; i >= 0; i--)
			{
				ITaskInfo taskInfo = this[i];
				if (taskInfo.ObjectGuid == guidObject)
				{
					this.m_alTaskList.Remove(taskInfo);
				}
			}
		}

		// Token: 0x06000C0D RID: 3085 RVA: 0x0001ED70 File Offset: 0x0001DD70
		public bool IsEqual(_ITaskList tlRef)
		{
			if (this.Count != tlRef.Count)
			{
				return false;
			}
			for (int i = 0; i < this.Count; i++)
			{
				ITaskInfo2 taskInfo = this[i] as ITaskInfo2;
				ITaskInfo2 taskInfo2 = tlRef[i] as ITaskInfo2;
				if (taskInfo.ObjectGuid != taskInfo2.ObjectGuid || taskInfo.TaskGuid != taskInfo2.TaskGuid || taskInfo.TaskName != taskInfo2.TaskName || taskInfo.ParentTaskName != taskInfo2.ParentTaskName)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000C0E RID: 3086 RVA: 0x0001EE08 File Offset: 0x0001DE08
		public _ITaskList Duplicate()
		{
			TaskList taskList = new TaskList();
			foreach (object obj in this.m_alTaskList)
			{
				TaskInfo taskInfo = (TaskInfo)obj;
				taskList.AddTaskInfo(taskInfo.ObjectGuid, taskInfo.TaskGuid, taskInfo.TaskName, taskInfo.ParentTaskName);
			}
			return taskList;
		}

		// Token: 0x04000213 RID: 531
		[Obfuscation(Feature = "rename")]
		private ArrayList m_alTaskList = new ArrayList();
	}
}
