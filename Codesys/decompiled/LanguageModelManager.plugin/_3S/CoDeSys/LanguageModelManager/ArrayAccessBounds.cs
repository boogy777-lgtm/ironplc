using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000EF RID: 239
	internal class ArrayAccessBounds : IArrayBounds, ICloneable
	{
		// Token: 0x060011CB RID: 4555 RVA: 0x00032BF7 File Offset: 0x00031BF7
		public ArrayAccessBounds(long lowerBound, long upperBound)
		{
			this._lowerBound = lowerBound;
			this._upperBound = upperBound;
		}

		// Token: 0x1700051E RID: 1310
		// (get) Token: 0x060011CC RID: 4556 RVA: 0x00032C0D File Offset: 0x00031C0D
		public long LowerBound
		{
			get
			{
				return this._lowerBound;
			}
		}

		// Token: 0x1700051F RID: 1311
		// (get) Token: 0x060011CD RID: 4557 RVA: 0x00032C15 File Offset: 0x00031C15
		public long UpperBound
		{
			get
			{
				return this._upperBound;
			}
		}

		// Token: 0x060011CE RID: 4558 RVA: 0x00032C1D File Offset: 0x00031C1D
		public object Clone()
		{
			return new ArrayAccessBounds(this._lowerBound, this._upperBound);
		}

		// Token: 0x060011CF RID: 4559 RVA: 0x00032C30 File Offset: 0x00031C30
		public override bool Equals(object obj)
		{
			ArrayAccessBounds arrayAccessBounds = obj as ArrayAccessBounds;
			return arrayAccessBounds != null && this._lowerBound == arrayAccessBounds._lowerBound && this._upperBound == arrayAccessBounds._upperBound;
		}

		// Token: 0x060011D0 RID: 4560 RVA: 0x00032C68 File Offset: 0x00031C68
		public override int GetHashCode()
		{
			return this._lowerBound.GetHashCode() ^ this._upperBound.GetHashCode();
		}

		// Token: 0x04000428 RID: 1064
		private readonly long _lowerBound;

		// Token: 0x04000429 RID: 1065
		private readonly long _upperBound;
	}
}
