using System;
using System.IO;
using \u0012;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Tools
{
	// Token: 0x02000076 RID: 118
	public class MyChecksumStream : ChecksumStream
	{
		// Token: 0x060009CC RID: 2508 RVA: 0x00013584 File Offset: 0x00011784
		public MyChecksumStream(bool bMemstreamCompatible)
		{
			this.\u0001 = APEnvironmentFacade.Instance.LanguageModelMgr.CreateCheckSumComputer();
			this.\u0001 = bMemstreamCompatible;
		}

		// Token: 0x17000407 RID: 1031
		// (get) Token: 0x060009CD RID: 2509 RVA: 0x000135A8 File Offset: 0x000117A8
		public override uint Checksum
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x17000408 RID: 1032
		// (get) Token: 0x060009CE RID: 2510 RVA: 0x000135B0 File Offset: 0x000117B0
		public override bool CanRead
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000409 RID: 1033
		// (get) Token: 0x060009CF RID: 2511 RVA: 0x000135B4 File Offset: 0x000117B4
		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700040A RID: 1034
		// (get) Token: 0x060009D0 RID: 2512 RVA: 0x000135B8 File Offset: 0x000117B8
		public override bool CanWrite
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060009D1 RID: 2513 RVA: 0x000135BC File Offset: 0x000117BC
		public override void Flush()
		{
		}

		// Token: 0x1700040B RID: 1035
		// (get) Token: 0x060009D2 RID: 2514 RVA: 0x000135C0 File Offset: 0x000117C0
		public override long Length
		{
			get
			{
				return 0L;
			}
		}

		// Token: 0x1700040C RID: 1036
		// (get) Token: 0x060009D3 RID: 2515 RVA: 0x000135C4 File Offset: 0x000117C4
		// (set) Token: 0x060009D4 RID: 2516 RVA: 0x000135C8 File Offset: 0x000117C8
		public override long Position
		{
			get
			{
				return 0L;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		// Token: 0x060009D5 RID: 2517 RVA: 0x000135D0 File Offset: 0x000117D0
		public override int Read(byte[] buffer, int offset, int count)
		{
			return 0;
		}

		// Token: 0x060009D6 RID: 2518 RVA: 0x000135D4 File Offset: 0x000117D4
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new InvalidOperationException();
		}

		// Token: 0x060009D7 RID: 2519 RVA: 0x000135DC File Offset: 0x000117DC
		public override void SetLength(long value)
		{
			throw new InvalidOperationException();
		}

		// Token: 0x060009D8 RID: 2520 RVA: 0x000135E4 File Offset: 0x000117E4
		public override void Write(byte[] buffer, int offset, int count)
		{
			this.\u0001.CRC32Update(buffer, count);
			this.\u0001 += count;
		}

		// Token: 0x060009D9 RID: 2521 RVA: 0x00013604 File Offset: 0x00011804
		internal uint \u0001()
		{
			this.Flush();
			ICRCSum icrcsum = this.\u0001;
			if (this.\u0001)
			{
				icrcsum = ((ICRCSumCloneable)this.\u0001).Clone();
				this.\u0001(icrcsum);
			}
			return icrcsum.CRC32Finish(null, 0);
		}

		// Token: 0x060009DA RID: 2522 RVA: 0x00013648 File Offset: 0x00011848
		public override void Close()
		{
			base.Close();
			this.\u0001(this.\u0001);
			this.\u0001 = this.\u0001.CRC32Finish(null, 0);
		}

		// Token: 0x060009DB RID: 2523 RVA: 0x00013670 File Offset: 0x00011870
		private void \u0001(ICRCSum \u0002)
		{
			if (this.\u0001)
			{
				long num = (long)this.\u0001;
				long num2 = \u0002.\u0001(num) - num;
				byte[] bBuffer = new byte[1];
				int num3 = 0;
				while ((long)num3 < num2)
				{
					\u0002.CRC32Update(bBuffer, 1);
					num3++;
				}
			}
		}

		// Token: 0x04000137 RID: 311
		private readonly ICRCSum \u0001;

		// Token: 0x04000138 RID: 312
		private uint \u0001;

		// Token: 0x04000139 RID: 313
		private readonly bool \u0001;

		// Token: 0x0400013A RID: 314
		private int \u0001;
	}
}
