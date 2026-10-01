using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0019
{
	// Token: 0x020002F6 RID: 758
	internal sealed class \u0010
	{
		// Token: 0x170007CE RID: 1998
		// (get) Token: 0x06002E90 RID: 11920 RVA: 0x000ADFD4 File Offset: 0x000AC1D4
		private _IDataManager Datman { get; }

		// Token: 0x170007CF RID: 1999
		// (get) Token: 0x06002E91 RID: 11921 RVA: 0x000ADFDC File Offset: 0x000AC1DC
		private int Granularity { get; }

		// Token: 0x170007D0 RID: 2000
		// (get) Token: 0x06002E92 RID: 11922 RVA: 0x000ADFE4 File Offset: 0x000AC1E4
		private int Size { get; }

		// Token: 0x170007D1 RID: 2001
		// (get) Token: 0x06002E93 RID: 11923 RVA: 0x000ADFEC File Offset: 0x000AC1EC
		private int SegmentSize { get; }

		// Token: 0x170007D2 RID: 2002
		// (get) Token: 0x06002E94 RID: 11924 RVA: 0x000ADFF4 File Offset: 0x000AC1F4
		// (set) Token: 0x06002E95 RID: 11925 RVA: 0x000ADFFC File Offset: 0x000AC1FC
		private DataSegmentFlags Flags { get; set; }

		// Token: 0x06002E96 RID: 11926 RVA: 0x000AE008 File Offset: 0x000AC208
		private \u0010(_IDataManager \u0098\u0005, ushort \u0099\u0005, int \u009A\u0005, int \u009B\u0005, int \u009C\u0005, int \u009D\u0005, DataSegmentFlags \u0019\u0002)
		{
			this.Datman = \u0098\u0005;
			this.Granularity = \u009B\u0005;
			this.Size = \u009C\u0005;
			this.SegmentSize = \u009D\u0005;
			this.Flags = \u0019\u0002;
			this.\u0001 = \u0099\u0005;
			this.\u0001 = \u009A\u0005;
		}

		// Token: 0x06002E97 RID: 11927 RVA: 0x000AE048 File Offset: 0x000AC248
		internal static bool \u0001(_IDataManager \u0002, ref ushort \u0003, ref int \u0004, int \u0005, int \u0006, int \u0007, DataSegmentFlags \u0008)
		{
			\u0010 u = new \u0010(\u0002, \u0003, \u0004, \u0005, \u0006, \u0007, \u0008);
			bool result = u.\u0001();
			\u0003 = u.\u0001;
			\u0004 = u.\u0001;
			return result;
		}

		// Token: 0x06002E98 RID: 11928 RVA: 0x000AE080 File Offset: 0x000AC280
		private bool \u0001()
		{
			bool flag = (this.Flags & DataSegmentFlags.Persistent) == DataSegmentFlags.Persistent;
			bool flag2 = (this.Flags & DataSegmentFlags.Retain) == DataSegmentFlags.Retain;
			bool u = (this.Flags & DataSegmentFlags.Code) == DataSegmentFlags.Code && this.Datman._MemorySettings.OnlineChangeInOwnSegment;
			bool result;
			if (this.\u0001(flag, u, out result))
			{
				return result;
			}
			if ((flag2 || flag) && this.Datman._MemorySettings.OneSRAM)
			{
				return MemoryCompiler.\u0002(this.Datman, ref this.\u0001, ref this.\u0001, this.Granularity, this.Size, this.SegmentSize, this.Flags);
			}
			_IDataSegment idataSegment = null;
			if ((this.Flags & DataSegmentFlags.Retain) != DataSegmentFlags.None && !this.Datman.RetainInOwnSegment)
			{
				this.Flags &= ~DataSegmentFlags.Retain;
			}
			bool flag3 = false;
			int num = 0;
			try
			{
				foreach (_IDataSegment idataSegment2 in MemoryCompiler.\u0001(this.Datman, this.Flags, flag))
				{
					num = MemoryCompiler.\u0003(idataSegment2, this.Granularity, this.SegmentSize, this.Size, this.Datman._MemorySettings.PackMode, this.Datman._MemorySettings.MinSize, this.Flags, out flag3);
					if (flag)
					{
						idataSegment2.MemMan.DeleteGapsButLast();
					}
					if (flag3)
					{
						idataSegment = idataSegment2;
						break;
					}
				}
			}
			catch
			{
				return false;
			}
			if (!flag3)
			{
				return false;
			}
			this.\u0001 = num + idataSegment.Address;
			this.\u0001 = idataSegment.Area;
			return true;
		}

		// Token: 0x06002E99 RID: 11929 RVA: 0x000AE23C File Offset: 0x000AC43C
		private bool \u0001(bool \u0002, bool \u0003, out bool \u0004)
		{
			\u0004 = false;
			if (this.Datman.Reference == null || \u0002 || \u0003)
			{
				return false;
			}
			if (!\u0010.\u0001(this.Datman.Reference, ref this.\u0001, ref this.\u0001, this.Granularity, this.Size, this.SegmentSize, this.Flags))
			{
				\u0004 = MemoryCompiler.\u0001(this.Datman, ref this.\u0001, ref this.\u0001, this.Granularity, this.Size, this.SegmentSize, this.Flags);
				return true;
			}
			\u0004 = MemoryCompiler.\u0002(this.Datman, this.\u0001, this.\u0001, this.Size, this.Flags);
			return true;
		}

		// Token: 0x040008DC RID: 2268
		[CompilerGenerated]
		private readonly _IDataManager \u0001;

		// Token: 0x040008DD RID: 2269
		private ushort \u0001;

		// Token: 0x040008DE RID: 2270
		private int \u0001;

		// Token: 0x040008DF RID: 2271
		[CompilerGenerated]
		private readonly int \u0002;

		// Token: 0x040008E0 RID: 2272
		[CompilerGenerated]
		private readonly int \u0003;

		// Token: 0x040008E1 RID: 2273
		[CompilerGenerated]
		private readonly int \u0004;

		// Token: 0x040008E2 RID: 2274
		[CompilerGenerated]
		private DataSegmentFlags \u0001;
	}
}
