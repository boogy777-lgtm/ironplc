using System;
using System.Collections.Generic;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000C2 RID: 194
	[TypeGuid("{e6a670d4-8bad-4716-a29b-770da7f0eef7}")]
	[StorageVersion("3.3.0.0")]
	public class SlotPOUList : GenericObject2, _ISlotPOUList2, _ISlotPOUList
	{
		// Token: 0x06000BEA RID: 3050 RVA: 0x0001E170 File Offset: 0x0001D170
		public void Add(Guid guidTask, int nSlot, Guid guidObject)
		{
			LSortedList<int, LList<Guid>> lsortedList = null;
			if (!this.m_htTaskList.TryGetValue(guidTask, ref lsortedList))
			{
				lsortedList = new LSortedList<int, LList<Guid>>();
				this.m_htTaskList[guidTask] = lsortedList;
			}
			LList<Guid> llist = null;
			if (!lsortedList.TryGetValue(nSlot, ref llist))
			{
				llist = new LList<Guid>();
				lsortedList[nSlot] = llist;
			}
			llist.Add(guidObject);
			LList<Guid> llist2 = null;
			if (!this.m_htTaskGuidsOfObject.TryGetValue(guidObject, ref llist2))
			{
				llist2 = new LList<Guid>();
				this.m_htTaskGuidsOfObject[guidObject] = llist2;
			}
			llist2.Add(guidTask);
		}

		// Token: 0x06000BEB RID: 3051 RVA: 0x0001E1F0 File Offset: 0x0001D1F0
		public void AddDownloadSlot(int nSlot, Guid guidObject)
		{
			LList<Guid> llist;
			if (!this.m_slDownloadSlots.TryGetValue(nSlot, ref llist))
			{
				llist = new LList<Guid>();
				this.m_slDownloadSlots[nSlot] = llist;
			}
			llist.Add(guidObject);
		}

		// Token: 0x06000BEC RID: 3052 RVA: 0x0001E228 File Offset: 0x0001D228
		public void AddOnlineChangeSlot(int nSlot, Guid guidObject)
		{
			LList<Guid> llist;
			if (!this.m_slOnlineChangeSlots.TryGetValue(nSlot, ref llist))
			{
				llist = new LList<Guid>();
				this.m_slOnlineChangeSlots[nSlot] = llist;
			}
			llist.Add(guidObject);
		}

		// Token: 0x06000BED RID: 3053 RVA: 0x0001E260 File Offset: 0x0001D260
		public void Remove(Guid ObjectGuid)
		{
			Guid[] array = new Guid[this.m_htTaskList.Keys.Count];
			this.m_htTaskList.Keys.CopyTo(array, 0);
			foreach (Guid guid in array)
			{
				LSortedList<int, LList<Guid>> lsortedList = this.m_htTaskList[guid];
				for (int j = lsortedList.Keys.Count - 1; j >= 0; j--)
				{
					LList<Guid> llist = lsortedList[lsortedList.Keys[j]];
					for (int k = llist.Count - 1; k >= 0; k--)
					{
						if (llist[k] == ObjectGuid)
						{
							llist.RemoveAt(k);
						}
					}
					if (llist.Count == 0)
					{
						lsortedList.RemoveAt(j);
					}
				}
				if (lsortedList.Count == 0)
				{
					this.m_htTaskList.Remove(guid);
				}
			}
			this.m_htTaskGuidsOfObject.Remove(ObjectGuid);
			foreach (LList<Guid> llist2 in this.m_slDownloadSlots.Values)
			{
				for (int l = llist2.Count - 1; l >= 0; l--)
				{
					if (llist2[l] == ObjectGuid)
					{
						llist2.RemoveAt(l);
					}
				}
			}
			foreach (LList<Guid> llist3 in this.m_slOnlineChangeSlots.Values)
			{
				for (int m = llist3.Count - 1; m >= 0; m--)
				{
					if (llist3[m] == ObjectGuid)
					{
						llist3.RemoveAt(m);
					}
				}
			}
		}

		// Token: 0x06000BEE RID: 3054 RVA: 0x0001E444 File Offset: 0x0001D444
		public void RemoveByTaskGuid(Guid TaskGuid)
		{
			this.m_htTaskList.Remove(TaskGuid);
			Guid[] array = new Guid[this.m_htTaskGuidsOfObject.Keys.Count];
			this.m_htTaskGuidsOfObject.Keys.CopyTo(array, 0);
			for (int i = 0; i < array.Length; i++)
			{
				LList<Guid> llist = this.m_htTaskGuidsOfObject[array[i]];
				for (int j = llist.Count - 1; j >= 0; j--)
				{
					if (llist[j] == TaskGuid)
					{
						llist.RemoveAt(j);
					}
				}
				if (llist.Count == 0)
				{
					this.m_htTaskGuidsOfObject.Remove(array[i]);
				}
			}
		}

		// Token: 0x170002EF RID: 751
		private LSortedList<int, LList<Guid>> this[Guid guidTask]
		{
			get
			{
				LSortedList<int, LList<Guid>> result = null;
				this.m_htTaskList.TryGetValue(guidTask, ref result);
				return result;
			}
		}

		// Token: 0x06000BF0 RID: 3056 RVA: 0x0001E50C File Offset: 0x0001D50C
		public int[] GetSlotArray(Guid guidTask)
		{
			LSortedList<int, LList<Guid>> lsortedList = this[guidTask];
			if (lsortedList == null)
			{
				return Array.Empty<int>();
			}
			int[] array = new int[lsortedList.Keys.Count];
			lsortedList.Keys.CopyTo(array, 0);
			return array;
		}

		// Token: 0x06000BF1 RID: 3057 RVA: 0x0001E54C File Offset: 0x0001D54C
		public _ISignature[] GetTaskSlotSignatures(Guid guidTask, int nSlot, _ICompileContext comcon)
		{
			LSortedList<int, LList<Guid>> lsortedList = this[guidTask];
			if (lsortedList == null)
			{
				return Array.Empty<_ISignature>();
			}
			LList<Guid> llist = null;
			if (!lsortedList.TryGetValue(nSlot, ref llist))
			{
				return Array.Empty<_ISignature>();
			}
			_ISignature[] array = new _ISignature[llist.Count];
			for (int i = 0; i < llist.Count; i++)
			{
				Guid guidObject = llist[i];
				_ISignature isignature = comcon[guidObject];
				array[i] = isignature;
			}
			return array;
		}

		// Token: 0x06000BF2 RID: 3058 RVA: 0x0001E5B4 File Offset: 0x0001D5B4
		public _ICompiledPOU[] GetTaskSlotPOUs(Guid guidTask, int nSlot, _ICompileContext comcon)
		{
			LSortedList<int, LList<Guid>> lsortedList = this[guidTask];
			if (lsortedList == null)
			{
				return Array.Empty<_ICompiledPOU>();
			}
			if (!lsortedList.ContainsKey(nSlot))
			{
				return Array.Empty<_ICompiledPOU>();
			}
			LList<Guid> llist = lsortedList[nSlot];
			_ICompiledPOU[] array = new _ICompiledPOU[llist.Count];
			int num = 0;
			for (int i = 0; i < llist.Count; i++)
			{
				Guid guidObject = llist[i];
				_ISignature isignature = comcon[guidObject];
				array[i] = null;
				if (isignature != null)
				{
					array[i] = comcon._GetCompiledPOUById(isignature.Id);
					num++;
				}
			}
			if (num == array.Length)
			{
				return array;
			}
			_ICompiledPOU[] array2 = new _ICompiledPOU[num];
			num = 0;
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j] == null)
				{
					this.Remove(llist[j]);
				}
				else
				{
					array2[num++] = array[j];
				}
			}
			return array2;
		}

		// Token: 0x06000BF3 RID: 3059 RVA: 0x0001E688 File Offset: 0x0001D688
		public _ISlotPOUList Duplicate()
		{
			SlotPOUList slotPOUList = new SlotPOUList();
			Guid[] array = new Guid[this.m_htTaskList.Keys.Count];
			this.m_htTaskList.Keys.CopyTo(array, 0);
			foreach (Guid guid in array)
			{
				LSortedList<int, LList<Guid>> lsortedList = this.m_htTaskList[guid];
				int[] array3 = new int[lsortedList.Keys.Count];
				lsortedList.Keys.CopyTo(array3, 0);
				for (int j = 0; j < array3.Length; j++)
				{
					foreach (Guid guidObject in lsortedList[array3[j]])
					{
						slotPOUList.Add(guid, array3[j], guidObject);
					}
				}
			}
			int[] array5;
			Guid[] array4 = this.GetDownloadGuidsSortedBySlot(out array5);
			for (int k = 0; k < array5.Length; k++)
			{
				slotPOUList.AddDownloadSlot(array5[k], array4[k]);
			}
			array4 = this.GetOnlineChangeGuidsSortedBySlot(out array5);
			for (int l = 0; l < array5.Length; l++)
			{
				slotPOUList.AddOnlineChangeSlot(array5[l], array4[l]);
			}
			return slotPOUList;
		}

		// Token: 0x06000BF4 RID: 3060 RVA: 0x0001E7E0 File Offset: 0x0001D7E0
		public Guid[] GetAllTasksForObjectGuid(Guid guidObject)
		{
			LList<Guid> llist = null;
			if (this.m_htTaskGuidsOfObject.TryGetValue(guidObject, ref llist))
			{
				Guid[] array = new Guid[llist.Count];
				llist.CopyTo(array);
				return array;
			}
			return Array.Empty<Guid>();
		}

		// Token: 0x06000BF5 RID: 3061 RVA: 0x0001E81C File Offset: 0x0001D81C
		public _ISignature[] GetAllTaskPOUs(_ICompileContext comcon)
		{
			_ISignature[] array = new _ISignature[this.m_htTaskGuidsOfObject.Keys.Count];
			Guid[] array2 = new Guid[this.m_htTaskGuidsOfObject.Keys.Count];
			this.m_htTaskGuidsOfObject.Keys.CopyTo(array2, 0);
			for (int i = 0; i < array2.Length; i++)
			{
				array[i] = comcon[array2[i]];
			}
			return array;
		}

		// Token: 0x06000BF6 RID: 3062 RVA: 0x0001E888 File Offset: 0x0001D888
		public IEnumerable<Guid> GetAllTaskSlotPouGuids(Guid guidTask, int nSlot)
		{
			LSortedList<int, LList<Guid>> lsortedList = this[guidTask];
			if (lsortedList == null)
			{
				return Array.Empty<Guid>();
			}
			if (!lsortedList.ContainsKey(nSlot))
			{
				return Array.Empty<Guid>();
			}
			return lsortedList[nSlot];
		}

		// Token: 0x170002F0 RID: 752
		// (get) Token: 0x06000BF7 RID: 3063 RVA: 0x0001E8BC File Offset: 0x0001D8BC
		public IEnumerable<Guid> AllTaskObjectGuids
		{
			get
			{
				return this.m_htTaskGuidsOfObject.Keys;
			}
		}

		// Token: 0x06000BF8 RID: 3064 RVA: 0x0001E8CC File Offset: 0x0001D8CC
		public Guid[] GetDownloadGuidsSortedBySlot(out int[] nSlots)
		{
			LList<Guid> llist = new LList<Guid>();
			LList<int> llist2 = new LList<int>();
			LList<Guid>[] array = new LList<Guid>[this.m_slDownloadSlots.Values.Count];
			int[] array2 = new int[this.m_slDownloadSlots.Keys.Count];
			this.m_slDownloadSlots.Values.CopyTo(array, 0);
			this.m_slDownloadSlots.Keys.CopyTo(array2, 0);
			for (int i = 0; i < array.Length; i++)
			{
				LList<Guid> llist3 = array[i];
				int num = array2[i];
				for (int j = 0; j < llist3.Count; j++)
				{
					llist.Add(llist3[j]);
					llist2.Add(num);
				}
			}
			Guid[] array3 = new Guid[llist.Count];
			nSlots = new int[llist2.Count];
			llist.CopyTo(array3);
			llist2.CopyTo(nSlots);
			return array3;
		}

		// Token: 0x06000BF9 RID: 3065 RVA: 0x0001E9AC File Offset: 0x0001D9AC
		public Guid[] GetOnlineChangeGuidsSortedBySlot(out int[] nSlots)
		{
			LList<Guid> llist = new LList<Guid>();
			LList<int> llist2 = new LList<int>();
			LList<Guid>[] array = new LList<Guid>[this.m_slOnlineChangeSlots.Values.Count];
			int[] array2 = new int[this.m_slOnlineChangeSlots.Keys.Count];
			this.m_slOnlineChangeSlots.Values.CopyTo(array, 0);
			this.m_slOnlineChangeSlots.Keys.CopyTo(array2, 0);
			for (int i = 0; i < array.Length; i++)
			{
				LList<Guid> llist3 = array[i];
				int num = array2[i];
				for (int j = 0; j < llist3.Count; j++)
				{
					llist.Add(llist3[j]);
					llist2.Add(num);
				}
			}
			Guid[] array3 = new Guid[llist.Count];
			nSlots = new int[llist2.Count];
			llist.CopyTo(array3);
			llist2.CopyTo(nSlots);
			return array3;
		}

		// Token: 0x0400020B RID: 523
		[DefaultSerialization("TaskList")]
		[StorageVersion("3.3.0.0")]
		[StorageSaveAsNonGenericCollection]
		[Obfuscation(Feature = "rename")]
		private LDictionary<Guid, LSortedList<int, LList<Guid>>> m_htTaskList = new LDictionary<Guid, LSortedList<int, LList<Guid>>>();

		// Token: 0x0400020C RID: 524
		[DefaultSerialization("TaskGuidsOfObject")]
		[StorageVersion("3.3.0.0")]
		[StorageSaveAsNonGenericCollection]
		[Obfuscation(Feature = "rename")]
		private LDictionary<Guid, LList<Guid>> m_htTaskGuidsOfObject = new LDictionary<Guid, LList<Guid>>();

		// Token: 0x0400020D RID: 525
		[DefaultSerialization("DownloadSlots")]
		[StorageVersion("3.3.0.0")]
		[StorageSaveAsNonGenericCollection]
		[Obfuscation(Feature = "rename")]
		private LSortedList<int, LList<Guid>> m_slDownloadSlots = new LSortedList<int, LList<Guid>>();

		// Token: 0x0400020E RID: 526
		[DefaultSerialization("OnlineChangeSlots")]
		[StorageVersion("3.3.0.0")]
		[StorageSaveAsNonGenericCollection]
		[Obfuscation(Feature = "rename")]
		private LSortedList<int, LList<Guid>> m_slOnlineChangeSlots = new LSortedList<int, LList<Guid>>();
	}
}
