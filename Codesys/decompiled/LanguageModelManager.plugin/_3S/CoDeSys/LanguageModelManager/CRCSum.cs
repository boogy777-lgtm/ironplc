using System;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000137 RID: 311
	public class CRCSum : ICRCSum, ICRCSumCloneable
	{
		// Token: 0x06001AAD RID: 6829 RVA: 0x0004C235 File Offset: 0x0004B235
		public CRCSum()
		{
			this._crcSum = new CRC32();
		}

		// Token: 0x06001AAE RID: 6830 RVA: 0x0004C248 File Offset: 0x0004B248
		private CRCSum(CRC32 crcSum)
		{
			this._crcSum = crcSum;
		}

		// Token: 0x06001AAF RID: 6831 RVA: 0x0004C257 File Offset: 0x0004B257
		public ICRCSum Clone()
		{
			return new CRCSum(this._crcSum.Clone());
		}

		// Token: 0x06001AB0 RID: 6832 RVA: 0x0004C269 File Offset: 0x0004B269
		public uint CRC32Finish(byte[] bBuffer, int iSize)
		{
			return this._crcSum.Finish(bBuffer, iSize);
		}

		// Token: 0x06001AB1 RID: 6833 RVA: 0x0004C278 File Offset: 0x0004B278
		public uint CRC32Finish()
		{
			return this._crcSum.Finish(Array.Empty<byte>(), 0);
		}

		// Token: 0x06001AB2 RID: 6834 RVA: 0x0004C28B File Offset: 0x0004B28B
		public void CRC32Update(byte[] bBuffer, int iSize)
		{
			this._crcSum.Update(bBuffer, iSize);
		}

		// Token: 0x0400058F RID: 1423
		private readonly CRC32 _crcSum;
	}
}
