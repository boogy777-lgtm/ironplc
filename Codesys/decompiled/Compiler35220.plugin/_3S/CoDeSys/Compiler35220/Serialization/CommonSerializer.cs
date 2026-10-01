using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.Serialization
{
	// Token: 0x02000099 RID: 153
	public static class CommonSerializer
	{
		// Token: 0x06000CDF RID: 3295 RVA: 0x00020CB0 File Offset: 0x0001EEB0
		public static void WriteInt(BinaryWriter bw, int nValue)
		{
			bw.Write(nValue);
		}

		// Token: 0x06000CE0 RID: 3296 RVA: 0x00020CBC File Offset: 0x0001EEBC
		public static void WriteUInt(BinaryWriter bw, uint uiValue)
		{
			bw.Write(uiValue);
		}

		// Token: 0x06000CE1 RID: 3297 RVA: 0x00020CC8 File Offset: 0x0001EEC8
		public static void WriteGuid(BinaryWriter bw, Guid guid)
		{
			bw.Write(guid.ToByteArray());
		}

		// Token: 0x06000CE2 RID: 3298 RVA: 0x00020CD8 File Offset: 0x0001EED8
		public static void WriteString(BinaryWriter bw, string st)
		{
			bw.Write(st);
		}

		// Token: 0x06000CE3 RID: 3299 RVA: 0x00020CE4 File Offset: 0x0001EEE4
		public static bool SerializeNullabe(BinaryWriter bw, bool bNull)
		{
			bw.Write(bNull ? 0 : 1);
			return bNull;
		}

		// Token: 0x06000CE4 RID: 3300 RVA: 0x00020CF4 File Offset: 0x0001EEF4
		public static void SerializeKeyValuePairs<KEY, VALUE>(BinaryWriter bw, IEnumerable<KeyValuePair<KEY, VALUE>> pairs, Action<BinaryWriter, KEY> keyAction, Action<BinaryWriter, VALUE> valueAction)
		{
			IEnumerable<KeyValuePair<KEY, VALUE>> enumerable = pairs.Where(new Func<KeyValuePair<KEY, VALUE>, bool>(CommonSerializer.<>c__5<KEY, VALUE>.<>9.\u0001));
			IEnumerable<KeyValuePair<KEY, VALUE>> enumerable2 = pairs.Where(new Func<KeyValuePair<KEY, VALUE>, bool>(CommonSerializer.<>c__5<KEY, VALUE>.<>9.\u0002));
			bw.Write(enumerable.Count<KeyValuePair<KEY, VALUE>>());
			bw.Write(enumerable2.Count<KeyValuePair<KEY, VALUE>>());
			foreach (KeyValuePair<KEY, VALUE> keyValuePair in enumerable)
			{
				keyAction(bw, keyValuePair.Key);
			}
			foreach (KeyValuePair<KEY, VALUE> keyValuePair2 in enumerable2)
			{
				keyAction(bw, keyValuePair2.Key);
				valueAction(bw, keyValuePair2.Value);
			}
		}

		// Token: 0x06000CE5 RID: 3301 RVA: 0x00020DF4 File Offset: 0x0001EFF4
		public static void SerializeList<ELEMENT>(BinaryWriter bw, IEnumerable<ELEMENT> list, Action<BinaryWriter, ELEMENT> action)
		{
			bw.Write(list.Count<ELEMENT>());
			foreach (ELEMENT arg in list)
			{
				action(bw, arg);
			}
		}

		// Token: 0x06000CE6 RID: 3302 RVA: 0x00020E4C File Offset: 0x0001F04C
		public static void SerializeDataSegmentFlags(BinaryWriter bw, DataSegmentFlags flags)
		{
			bw.Write((ushort)flags);
		}

		// Token: 0x06000CE7 RID: 3303 RVA: 0x00020E58 File Offset: 0x0001F058
		public static void SerializeMemoryManager(BinaryWriter bw, IMemoryManager memoryManager)
		{
			CommonSerializer.SerializeMemoryManager(bw, (IMemoryManagerSerializable)memoryManager);
		}

		// Token: 0x06000CE8 RID: 3304 RVA: 0x00020E68 File Offset: 0x0001F068
		public static void SerializeMemoryManager(BinaryWriter bw, IMemoryManagerSerializable memoryManager)
		{
			bw.Write(memoryManager.Size);
			bw.Write(memoryManager.Base);
			bw.Write(memoryManager.Count);
			for (int i = 0; i < memoryManager.Count; i++)
			{
				_IMemManGap imemManGap = memoryManager[i];
				bw.Write(imemManGap.Offset);
				bw.Write(imemManGap.Size);
			}
		}

		// Token: 0x06000CE9 RID: 3305 RVA: 0x00020ECC File Offset: 0x0001F0CC
		public static void SerializeDefines(BinaryWriter bw, Hashtable hashTable)
		{
			IEnumerable<KeyValuePair<string, string>> enumerable = hashTable.Cast<DictionaryEntry>().Select(new Func<DictionaryEntry, KeyValuePair<string, string>>(CommonSerializer.<>c.<>9.\u0001));
			enumerable = CompileContextSerializer.Sort(enumerable);
			CommonSerializer.SerializeKeyValuePairs<string, string>(bw, enumerable, new Action<BinaryWriter, string>(CommonSerializer.WriteString), new Action<BinaryWriter, string>(CommonSerializer.WriteString));
		}

		// Token: 0x06000CEA RID: 3306 RVA: 0x00020F2C File Offset: 0x0001F12C
		public static void SerializeTaskInfo(BinaryWriter bw, ITaskInfo2 taskInfo)
		{
			bw.Write(taskInfo.TaskName);
			bw.Write(taskInfo.TaskGuid.ToByteArray());
			bw.Write(taskInfo.ObjectGuid.ToByteArray());
			bw.Write(taskInfo.ParentTaskName ?? string.Empty);
		}

		// Token: 0x06000CEB RID: 3307 RVA: 0x00020F84 File Offset: 0x0001F184
		public static void SerializeDirectVariable_Nullable(BinaryWriter bw, IDirectVariable directVariable)
		{
			byte b;
			if (directVariable == null)
			{
				b = 0;
			}
			else if (directVariable.Incomplete)
			{
				b = 1;
			}
			else
			{
				b = 2;
			}
			bw.Write(b);
			if (b != 0)
			{
				if (b == 1)
				{
					bw.Write((uint)directVariable.Location);
					return;
				}
				if (b == 2)
				{
					bw.Write((uint)directVariable.Location);
					bw.Write((uint)directVariable.Size);
					bw.Write(directVariable.Components.Length);
					foreach (int value in directVariable.Components)
					{
						bw.Write(value);
					}
				}
			}
		}

		// Token: 0x06000CEC RID: 3308 RVA: 0x0002100C File Offset: 0x0001F20C
		public static void SerializeMessage(BinaryWriter bw, _ICompilerMessage msg)
		{
			bw.Write((int)msg.MessageId);
			bw.Write((int)msg.Severity);
			bw.Write(msg.Text);
			bw.Write(msg.ProjectHandle);
			bw.Write(msg.ObjectGuid.ToByteArray());
			bw.Write(msg.SignatureGuid.ToByteArray());
			long value = PositionHelper.CombinePosition(msg.Position, msg.PositionOffset);
			bw.Write(value);
			bw.Write(msg.Length);
			bw.Write((int)msg.ShowAttribute);
		}

		// Token: 0x06000CED RID: 3309 RVA: 0x000210A4 File Offset: 0x0001F2A4
		public static void SerializeStaticMemorySegment(BinaryWriter bw, IStaticMemorySegment sms)
		{
			bw.Write(sms.SubApplicationGuid.ToByteArray());
			bw.Write(sms.Offset);
			bw.Write(sms.Size);
		}

		// Token: 0x06000CEE RID: 3310 RVA: 0x000210E0 File Offset: 0x0001F2E0
		public static void SerializeSlotPOUList(BinaryWriter bw, _ISlotPOUList2 spl)
		{
			IEnumerable<Guid> allTaskObjectGuids = spl.AllTaskObjectGuids;
			LList<Guid> llist = CompileContextSerializer.Sort(allTaskObjectGuids.ToArray<Guid>());
			bw.Write(allTaskObjectGuids.Count<Guid>());
			foreach (Guid guidTask in llist)
			{
				int[] slotArray = spl.GetSlotArray(guidTask);
				bw.Write(guidTask.ToByteArray());
				bw.Write(slotArray.Length);
				CompileContextSerializer.Sort(slotArray);
				foreach (int num in slotArray)
				{
					bw.Write(num);
					IEnumerable<Guid> allTaskSlotPouGuids = spl.GetAllTaskSlotPouGuids(guidTask, num);
					bw.Write(allTaskSlotPouGuids.Count<Guid>());
					foreach (Guid guid in allTaskSlotPouGuids)
					{
						bw.Write(guid.ToByteArray());
					}
				}
			}
			int[] array2;
			Guid[] downloadGuidsSortedBySlot = spl.GetDownloadGuidsSortedBySlot(out array2);
			bw.Write(array2.Length);
			for (int j = 0; j < array2.Length; j++)
			{
				bw.Write(array2[j]);
				bw.Write(downloadGuidsSortedBySlot[j].ToByteArray());
			}
			int[] array3;
			Guid[] onlineChangeGuidsSortedBySlot = spl.GetOnlineChangeGuidsSortedBySlot(out array3);
			bw.Write(array3.Length);
			for (int k = 0; k < array3.Length; k++)
			{
				bw.Write(array3[k]);
				bw.Write(onlineChangeGuidsSortedBySlot[k].ToByteArray());
			}
		}

		// Token: 0x06000CEF RID: 3311 RVA: 0x0002127C File Offset: 0x0001F47C
		internal static void \u0001(BinaryWriter \u0002, IArchivable \u0003)
		{
			IArchiveWriter archiveWriter = APEnvironmentFacade.Instance.CreateNewLowMemoryFootprintBinaryArchiveWriter();
			archiveWriter.Initialize(\u0002.BaseStream, Encoding.UTF8);
			archiveWriter.Save(\u0003);
		}
	}
}
