using System;
using System.Collections.Generic;
using System.Linq;
using \u0019;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.Phase3_Location
{
	// Token: 0x020002F3 RID: 755
	public static class MemoryCompiler
	{
		// Token: 0x06002E58 RID: 11864 RVA: 0x000AC884 File Offset: 0x000AAA84
		internal static _IDataManager \u0001(_IDataManager \u0002)
		{
			_IDataManager idataManager = \u0019.\u0003.\u0001();
			idataManager._MemorySettings = \u0002._MemorySettings;
			foreach (_IDataSegment idataSegment in \u0002._DataSegments)
			{
				_IDataSegment item = MemoryCompiler.\u0001(idataSegment);
				idataManager._DataSegments.Add(item);
				if (idataSegment.GetFlag(DataSegmentFlags.Area))
				{
					idataManager._AreaSegments.Add(item);
				}
			}
			idataManager.FirstArea = \u0002.FirstArea;
			foreach (IArea area in \u0002._Areas)
			{
				_IArea iarea = (_IArea)area;
				idataManager.AddArea(iarea.Duplicate());
			}
			return idataManager;
		}

		// Token: 0x06002E59 RID: 11865 RVA: 0x000AC95C File Offset: 0x000AAB5C
		private static void \u0001(_IDataManager \u0002, int \u0003, DataSegmentFlags \u0004)
		{
			for (int i = 0; i < \u0002._AreaSegments.Count; i++)
			{
				_IDataSegment areaSegment = \u0002.GetAreaSegment(i);
				if (areaSegment.GetFlag(\u0004) && areaSegment.Flags != (DataSegmentFlags.Area | DataSegmentFlags.Persistent) && areaSegment.Flags != (DataSegmentFlags.Retain | DataSegmentFlags.Area | DataSegmentFlags.Persistent))
				{
					bool flag;
					int u = MemoryCompiler.\u0003(areaSegment, 8, \u0002.DataSegmentSize, \u0003, \u0002._MemorySettings.PackMode, \u0002._MemorySettings.MinSize, \u0004, out flag);
					\u0002._DataSegments.Add(\u0019.\u0003.\u0001(areaSegment.Area, u, \u0003, \u0004));
				}
			}
		}

		// Token: 0x06002E5A RID: 11866 RVA: 0x000AC9F0 File Offset: 0x000AABF0
		internal static bool \u0001(_IDataManager \u0002, _IMemorySettings \u0003, IEnumerable<_IArea> \u0004)
		{
			foreach (_IArea iarea in \u0004)
			{
				\u0002.AddArea(iarea);
				MemoryCompiler.\u0001(\u0002, \u0003, iarea);
			}
			return true;
		}

		// Token: 0x06002E5B RID: 11867 RVA: 0x000ACA44 File Offset: 0x000AAC44
		private static void \u0001(_IDataManager \u0002, _IMemorySettings \u0003, _IArea \u0004)
		{
			DataSegmentFlags dataSegmentFlags = \u0004.DataSegmentFlags;
			if (\u0004.GetAreaFlag(AreaFlags.NoUse))
			{
				dataSegmentFlags = DataSegmentFlags.None;
			}
			_IDataSegment item = \u0019.\u0003.\u0001((ushort)\u0004.Index, 0, \u0004.Size, dataSegmentFlags | DataSegmentFlags.Area);
			\u0002._DataSegments.Add(item);
			\u0002._AreaSegments.Add(item);
			if (\u0004.GetDataSegmentFlag(DataSegmentFlags.Code) && (\u0003.CodeSegmentPrologSize > 0 || \u0003.CodeSegmentHeaderSize > 0))
			{
				MemoryCompiler.\u0002(\u0002, (ushort)\u0004.Index, 0, \u0003.CodeSegmentPrologSize + \u0003.CodeSegmentHeaderSize, DataSegmentFlags.Code);
			}
		}

		// Token: 0x06002E5C RID: 11868 RVA: 0x000ACAD4 File Offset: 0x000AACD4
		internal static bool \u0001(_IDataManager \u0002, _IMemorySettings \u0003, IList<_IArea> \u0004, int \u0005)
		{
			\u0002._DataSegments.Clear();
			\u0002._MemorySettings = \u0003;
			\u0002.FirstArea = \u0005;
			foreach (_IArea area in \u0004)
			{
				\u0002.AddArea(area);
			}
			for (ushort num = 0; num < \u0002.AreaCount; num += 1)
			{
				_IArea area2 = \u0002.GetArea((int)num);
				MemoryCompiler.\u0001(\u0002, \u0003, area2);
			}
			MemoryCompiler.\u0001(\u0002, \u0003.MemoryDataSize, DataSegmentFlags.Memory);
			MemoryCompiler.\u0001(\u0002, \u0003.InputDataSize, DataSegmentFlags.Input);
			MemoryCompiler.\u0001(\u0002, \u0003.OutputDataSize, DataSegmentFlags.Output);
			if (\u0003.RetainInOwnSegment && !\u0003.RetainDynamic)
			{
				MemoryCompiler.\u0001(\u0002, \u0003.RetainDataSize, DataSegmentFlags.Retain);
			}
			return true;
		}

		// Token: 0x06002E5D RID: 11869 RVA: 0x000ACBA0 File Offset: 0x000AADA0
		internal static IArea[] \u0001(_IDataManager \u0002)
		{
			LList<IArea> llist = new LList<IArea>();
			for (int i = \u0002._AreaSegments.Count - 1; i >= 0; i--)
			{
				_IDataSegment idataSegment = \u0002._AreaSegments[i];
				_IArea areaByIndex = \u0002.GetAreaByIndex((int)idataSegment.Area);
				if (areaByIndex != null && !areaByIndex.Dynamic && areaByIndex.Size != 0)
				{
					llist.Add(areaByIndex);
				}
			}
			IArea[] array = new IArea[llist.Count];
			int num = 0;
			for (int j = llist.Count - 1; j >= 0; j--)
			{
				array[num++] = llist[j];
			}
			return array;
		}

		// Token: 0x06002E5E RID: 11870 RVA: 0x000ACC3C File Offset: 0x000AAE3C
		internal static bool \u0001(_IDataManager \u0002, int \u0003)
		{
			_IArea areaByIndex = \u0002.GetAreaByIndex(\u0003);
			if (areaByIndex == null)
			{
				return false;
			}
			_IDataSegment idataSegment = null;
			for (int i = \u0002._AreaSegments.Count - 1; i >= 0; i--)
			{
				_IDataSegment idataSegment2 = \u0002._AreaSegments[i];
				if ((int)idataSegment2.Area == \u0003)
				{
					idataSegment = idataSegment2;
					break;
				}
			}
			Debug.\u0001(idataSegment != null);
			int num = 0;
			if (areaByIndex.GetDataSegmentFlag(DataSegmentFlags.Code))
			{
				num = \u0002._MemorySettings.CodeSegmentPrologSize + \u0002._MemorySettings.CodeSegmentHeaderSize;
			}
			return idataSegment.SizeAllocated == num;
		}

		// Token: 0x06002E5F RID: 11871 RVA: 0x000ACCC8 File Offset: 0x000AAEC8
		internal static _IArea[] \u0001(_IDataManager \u0002)
		{
			int areaAlignment = \u0002._MemorySettings.AreaAlignment;
			LList<IArea> llist = new LList<IArea>();
			for (int i = \u0002._AreaSegments.Count - 1; i >= 0; i--)
			{
				_IDataSegment idataSegment = \u0002._AreaSegments[i];
				_IArea areaByIndex = \u0002.GetAreaByIndex((int)idataSegment.Area);
				if (areaByIndex == null)
				{
					\u0002._AreaSegments.Remove(idataSegment);
				}
				else if (areaByIndex.Dynamic)
				{
					MemoryCompiler.\u0001(\u0002, areaAlignment, llist, idataSegment, areaByIndex);
				}
			}
			_IArea[] array = new _IArea[llist.Count];
			LList<IArea> llist2 = llist;
			IArea[] array2 = array;
			llist2.CopyTo(array2);
			return array;
		}

		// Token: 0x06002E60 RID: 11872 RVA: 0x000ACD60 File Offset: 0x000AAF60
		private static void \u0001(_IDataManager \u0002, int \u0003, LList<IArea> \u0004, _IDataSegment \u0005, _IArea \u0006)
		{
			int num = 0;
			if (\u0006.GetDataSegmentFlag(DataSegmentFlags.Code))
			{
				num = \u0002._MemorySettings.CodeSegmentPrologSize + \u0002._MemorySettings.CodeSegmentHeaderSize;
			}
			if (\u0005.SizeAllocated == num && \u0006.GetAreaFlag(AreaFlags.OnlineChange) && \u0002._AreaSegments.Last<_IDataSegment>() == \u0005)
			{
				if (\u0002._AreaSegments.Last<_IDataSegment>() == \u0005)
				{
					\u0002._AreaSegments.Remove(\u0005);
					\u0002._DataSegments.Remove(\u0005);
					\u0002.RemoveArea(\u0006);
					return;
				}
				if (MemoryCompiler.\u0001(\u0005, 8, 0, \u0006.Size, \u0003))
				{
					\u0006.Size = \u0005.Size;
					\u0006.Dynamic = false;
					\u0004.Add(\u0006);
				}
				return;
			}
			else
			{
				if (\u0005.SizeAllocated == 0 && \u0005.Flags == (DataSegmentFlags.Retain | DataSegmentFlags.Area) && \u0002._MemorySettings.RetainDynamic)
				{
					return;
				}
				if (\u0005.SizeAllocated == 0 && (\u0005.Flags == (DataSegmentFlags.Area | DataSegmentFlags.Persistent) || \u0005.Flags == (DataSegmentFlags.Retain | DataSegmentFlags.Area | DataSegmentFlags.Persistent)) && \u0002._MemorySettings.PersistentDynamic)
				{
					return;
				}
				if (MemoryCompiler.\u0001(\u0005, \u0006.MinimalAreaSize, \u0006.AllocationPlusInPercent, \u0006.Size, \u0003))
				{
					\u0006.Size = \u0005.Size;
					\u0006.Dynamic = false;
					\u0004.Add(\u0006);
					return;
				}
				\u0006.Size = \u0005.Size;
				\u0006.Dynamic = false;
				\u0004.Add(\u0006);
				Debug.\u0001(false);
				return;
			}
		}

		// Token: 0x06002E61 RID: 11873 RVA: 0x000ACEC8 File Offset: 0x000AB0C8
		internal static bool \u0001(_IDataManager \u0002, IDataLocation \u0003, int \u0004, DataSegmentFlags \u0005)
		{
			return MemoryCompiler.\u0001(\u0002, \u0003, \u0004, -1, \u0005);
		}

		// Token: 0x06002E62 RID: 11874 RVA: 0x000ACED4 File Offset: 0x000AB0D4
		internal static bool \u0001(_IDataManager \u0002, IDataLocation \u0003, int \u0004, int \u0005, DataSegmentFlags \u0006)
		{
			return \u0003.IsRelativ || MemoryCompiler.\u0001(\u0002, \u0003.Area, \u0003.Offset, \u0004, \u0006);
		}

		// Token: 0x06002E63 RID: 11875 RVA: 0x000ACEF8 File Offset: 0x000AB0F8
		internal static bool \u0001(_IDataManager \u0002, ushort \u0003, int \u0004, int \u0005, DataSegmentFlags \u0006)
		{
			for (int i = \u0002._DataSegments.Count - 1; i >= 0; i--)
			{
				_IDataSegment idataSegment = \u0002._DataSegments[i];
				if (idataSegment.Area == \u0003 && idataSegment.Address <= \u0004 && idataSegment.Address + idataSegment.Size > \u0004 + \u0005)
				{
					MemoryCompiler.\u0001(idataSegment, \u0004 - idataSegment.Address, \u0005, \u0006);
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002E64 RID: 11876 RVA: 0x000ACF64 File Offset: 0x000AB164
		internal static void \u0001(_IDataManager \u0002, IDataLocation \u0003, int \u0004, _IMemoryManager[] \u0005)
		{
			if (\u0003 != null && \u0003.Area >= 0 && (int)\u0003.Area < \u0005.Length)
			{
				\u0005[(int)\u0003.Area].AllocateHard(\u0003.Offset, \u0004);
			}
		}

		// Token: 0x06002E65 RID: 11877 RVA: 0x000ACF94 File Offset: 0x000AB194
		internal static bool \u0002(_IDataManager \u0002, ushort \u0003, int \u0004, int \u0005, DataSegmentFlags \u0006)
		{
			for (int i = \u0002._DataSegments.Count - 1; i >= 0; i--)
			{
				_IDataSegment idataSegment = \u0002._DataSegments[i];
				if (idataSegment.Area == \u0003 && idataSegment.Address <= \u0004 && idataSegment.Address + idataSegment.Size >= \u0004 + \u0005)
				{
					return \u0005 == 0 || MemoryCompiler.\u0002(idataSegment, \u0004 - idataSegment.Address, \u0005, \u0006);
				}
			}
			return false;
		}

		// Token: 0x06002E66 RID: 11878 RVA: 0x000AD00C File Offset: 0x000AB20C
		internal static bool \u0001(_IDataManager \u0002, ref ushort \u0003, ref int \u0004, int \u0005, int \u0006, int \u0007, DataSegmentFlags \u0008)
		{
			_IDataSegment idataSegment = null;
			if ((\u0008 & DataSegmentFlags.Retain) != DataSegmentFlags.None && !\u0002.RetainInOwnSegment)
			{
				\u0008 &= ~DataSegmentFlags.Retain;
			}
			bool flag = false;
			int num = 0;
			ushort num2 = 0;
			while ((int)num2 < \u0002._DataSegments.Count)
			{
				_IDataSegment idataSegment2 = \u0002._DataSegments[(int)num2];
				if (\u0002.GetAreaByIndex((int)idataSegment2.Area).GetAreaFlag(AreaFlags.OnlineChange) && idataSegment2.GetFlag(\u0008))
				{
					num = MemoryCompiler.\u0003(idataSegment2, \u0005, \u0007, \u0006, \u0002._MemorySettings.PackMode, \u0002._MemorySettings.MinSize, \u0008, out flag);
					if (flag)
					{
						idataSegment = idataSegment2;
						break;
					}
				}
				num2 += 1;
			}
			if (!flag)
			{
				return false;
			}
			\u0004 = num + idataSegment.Address;
			\u0003 = idataSegment.Area;
			return true;
		}

		// Token: 0x06002E67 RID: 11879 RVA: 0x000AD0C4 File Offset: 0x000AB2C4
		internal static _IMemoryManager[] \u0001(_IDataManager \u0002, _ICompileContext \u0003)
		{
			_IMemoryManager[] array = new _IMemoryManager[(int)\u0002.AreaCount];
			for (int i = 0; i < (int)\u0002.AreaCount; i++)
			{
				array[i] = \u0019.\u0003.\u0001(\u0002.GetAreaByIndex(i).Size, 0);
			}
			IScope scope = \u0019.\u0001.\u0001(\u0003);
			foreach (ICompiledPOU compiledPOU in \u0003.GetAllCompiledPOUsEx())
			{
				if (compiledPOU.CompiledCode != null && !compiledPOU.GetFlag(CompiledPOUFlags.ToRemoveAfterDownload))
				{
					MemoryCompiler.\u0001(\u0002, compiledPOU.CompiledCode.Location, compiledPOU.CompiledCode.CodeSize, array);
				}
			}
			foreach (_ISignature isignature in \u0003.AllFlat)
			{
				if (isignature.VirtualFunctionTable != null)
				{
					MemoryCompiler.\u0001(\u0002, isignature.VirtualFunctionTable.DataLocation, isignature.VirtualFunctionTable.Size, array);
				}
				MemoryCompiler.\u0001(\u0002, isignature.FPDataLocation, \u0003.PointerSize, array);
				foreach (IVariable variable in isignature.AllVariables)
				{
					if (variable.CompiledType != null)
					{
						MemoryCompiler.\u0001(\u0002, variable.DataLocation, variable.CompiledType.Size(scope), array);
					}
				}
			}
			for (int j = 0; j < \u0002.Count; j++)
			{
				if (!\u0002[j].GetFlag(DataSegmentFlags.Area))
				{
					int area = (int)\u0002[j].Area;
					if (area >= 0 && area < array.Length)
					{
						array[area].AllocateHard(\u0002[j].Address, \u0002[j].Size);
					}
				}
			}
			return array;
		}

		// Token: 0x06002E68 RID: 11880 RVA: 0x000AD2C0 File Offset: 0x000AB4C0
		internal static ushort \u0001(_IDataManager \u0002, DataSegmentFlags \u0003)
		{
			_IDataSegment idataSegment = null;
			if ((\u0003 & DataSegmentFlags.Retain) != DataSegmentFlags.None && !\u0002.RetainInOwnSegment)
			{
				\u0003 &= ~DataSegmentFlags.Retain;
			}
			bool flag = false;
			try
			{
				for (ushort num = 0; num < (ushort)\u0002._DataSegments.Count; num += 1)
				{
					_IDataSegment idataSegment2 = \u0002._DataSegments[(int)num];
					if (((\u0003 & DataSegmentFlags.Retain) == DataSegmentFlags.None || !\u0002.RetainInOwnSegment || \u0002._MemorySettings.RetainDynamic || idataSegment2.Flags == DataSegmentFlags.Retain) && idataSegment2.GetFlag(\u0003))
					{
						idataSegment = idataSegment2;
						flag = true;
						break;
					}
				}
			}
			catch
			{
				return CompilerServicesInternal.InvalidRefId;
			}
			if (!flag)
			{
				return CompilerServicesInternal.InvalidRefId;
			}
			return idataSegment.Area;
		}

		// Token: 0x06002E69 RID: 11881 RVA: 0x000AD370 File Offset: 0x000AB570
		internal static bool \u0002(_IDataManager \u0002, ref ushort \u0003, ref int \u0004, int \u0005, int \u0006, int \u0007, DataSegmentFlags \u0008)
		{
			bool flag = (\u0008 & DataSegmentFlags.Persistent) == DataSegmentFlags.Persistent;
			_IDataSegment idataSegment = null;
			bool flag2 = false;
			int num = 0;
			try
			{
				for (ushort num2 = 0; num2 < (ushort)\u0002._DataSegments.Count; num2 += 1)
				{
					_IDataSegment idataSegment2 = \u0002._DataSegments[(int)num2];
					if (idataSegment2.GetFlag(DataSegmentFlags.Retain) || idataSegment2.GetFlag(DataSegmentFlags.Persistent))
					{
						if (flag)
						{
							num = MemoryCompiler.\u0002(idataSegment2, \u0005, \u0007, \u0006, \u0002._MemorySettings.PackMode, \u0002._MemorySettings.MinSize, \u0008, out flag2);
						}
						else
						{
							num = MemoryCompiler.\u0001(idataSegment2, \u0005, \u0007, \u0006, \u0002._MemorySettings.PackMode, \u0002._MemorySettings.MinSize, \u0008, out flag2);
						}
						if (flag2)
						{
							idataSegment = idataSegment2;
							break;
						}
					}
				}
			}
			catch
			{
				return false;
			}
			if (!flag2)
			{
				return false;
			}
			\u0004 = num + idataSegment.Address;
			\u0003 = idataSegment.Area;
			return true;
		}

		// Token: 0x06002E6A RID: 11882 RVA: 0x000AD46C File Offset: 0x000AB66C
		internal static IEnumerable<_IDataSegment> \u0001(_IDataManager \u0002, DataSegmentFlags \u0003, bool \u0004)
		{
			ushort num2;
			for (ushort num = 0; num < (ushort)\u0002._DataSegments.Count; num = num2 + 1)
			{
				_IDataSegment idataSegment = \u0002._DataSegments[(int)num];
				DataSegmentFlags dataSegmentFlags = DataSegmentFlags.Retain;
				if (\u0002._MemorySettings.RetainDynamic)
				{
					dataSegmentFlags = (DataSegmentFlags.Retain | DataSegmentFlags.Area);
				}
				if (((\u0003 & DataSegmentFlags.Retain) == DataSegmentFlags.None || \u0004 || !\u0002.RetainInOwnSegment || idataSegment.Flags == dataSegmentFlags) && idataSegment.GetFlag(\u0003))
				{
					yield return idataSegment;
				}
				num2 = num;
			}
			yield break;
		}

		// Token: 0x06002E6B RID: 11883 RVA: 0x000AD48C File Offset: 0x000AB68C
		public static _IDataSegment GetPreferredDataSegment(_IDataManager datman, DataSegmentFlags flags)
		{
			bool u = (flags & DataSegmentFlags.Persistent) == DataSegmentFlags.Persistent;
			if ((flags & DataSegmentFlags.Retain) != DataSegmentFlags.None && !datman.RetainInOwnSegment)
			{
				flags &= ~DataSegmentFlags.Retain;
			}
			return MemoryCompiler.\u0001(datman, flags, u).FirstOrDefault<_IDataSegment>();
		}

		// Token: 0x06002E6C RID: 11884 RVA: 0x000AD4CC File Offset: 0x000AB6CC
		internal static bool \u0003(_IDataManager \u0002, ref ushort \u0003, ref int \u0004, int \u0005, int \u0006, int \u0007, DataSegmentFlags \u0008)
		{
			return \u0010.\u0001(\u0002, ref \u0003, ref \u0004, \u0005, \u0006, \u0007, \u0008);
		}

		// Token: 0x06002E6D RID: 11885 RVA: 0x000AD4E0 File Offset: 0x000AB6E0
		internal static _IDataSegment \u0001(_IDataSegment \u0002)
		{
			_IDataSegment idataSegment = \u0019.\u0003.\u0001(\u0002.Area, \u0002.Address, \u0002.Size, \u0002.Flags);
			idataSegment.DPTableOffset = \u0002.DPTableOffset;
			idataSegment.MemMan = \u0002.MemMan.Duplicate();
			return idataSegment;
		}

		// Token: 0x06002E6E RID: 11886 RVA: 0x000AD51C File Offset: 0x000AB71C
		internal static bool \u0001(_IDataSegment \u0002, int \u0003, int \u0004, DataSegmentFlags \u0005)
		{
			return \u0002.MemMan.Free(\u0003, \u0004);
		}

		// Token: 0x06002E6F RID: 11887 RVA: 0x000AD52C File Offset: 0x000AB72C
		internal static bool \u0002(_IDataSegment \u0002, int \u0003, int \u0004, DataSegmentFlags \u0005)
		{
			return \u0002.MemMan.Allocate(\u0003, \u0004);
		}

		// Token: 0x06002E70 RID: 11888 RVA: 0x000AD53C File Offset: 0x000AB73C
		internal static int \u0001(_IDataSegment \u0002, int \u0003, int \u0004, int \u0005, int \u0006, int \u0007, DataSegmentFlags \u0008, out bool \u000E)
		{
			return \u0002.MemMan.AllocateHighestAddress(\u0003, \u0004, \u0005, \u0006, \u0007, out \u000E);
		}

		// Token: 0x06002E71 RID: 11889 RVA: 0x000AD554 File Offset: 0x000AB754
		internal static int \u0002(_IDataSegment \u0002, int \u0003, int \u0004, int \u0005, int \u0006, int \u0007, DataSegmentFlags \u0008, out bool \u000E)
		{
			return \u0002.MemMan.AllocateWithoutGap(\u0003, \u0004, \u0005, \u0006, \u0007, out \u000E);
		}

		// Token: 0x06002E72 RID: 11890 RVA: 0x000AD56C File Offset: 0x000AB76C
		internal static int \u0003(_IDataSegment \u0002, int \u0003, int \u0004, int \u0005, int \u0006, int \u0007, DataSegmentFlags \u0008, out bool \u000E)
		{
			return \u0002.MemMan.Allocate(\u0003, \u0004, \u0005, \u0006, \u0007, out \u000E);
		}

		// Token: 0x06002E73 RID: 11891 RVA: 0x000AD584 File Offset: 0x000AB784
		internal static bool \u0001(_IDataSegment \u0002, int \u0003, int \u0004, int \u0005, int \u0006)
		{
			if (\u0002.MemMan.Shrink(\u0003, \u0004, \u0005, \u0006))
			{
				\u0002.Size = \u0002.MemMan.Size;
				return true;
			}
			return false;
		}
	}
}
