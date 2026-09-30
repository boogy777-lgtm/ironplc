using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000F0 RID: 240
	internal class VariableArrayAccessAddressInfo : AddressInfoBase, IArrayAccessAddressInfo2, IArrayAccessAddressInfo, IAddressInfo, IMyAddressInfo, IAddressInfo4, IAddressInfo3, IAddressInfo2
	{
		// Token: 0x060011D1 RID: 4561 RVA: 0x00032C94 File Offset: 0x00031C94
		public VariableArrayAccessAddressInfo(IAddressInfo baseArray, int nBaseSize, IAddressInfo[] indices, IArrayBounds[] bounds, IType t) : base(t)
		{
			if (baseArray == null)
			{
				throw new ArgumentNullException("baseArray");
			}
			if (indices == null)
			{
				throw new ArgumentNullException("indices");
			}
			if (bounds == null)
			{
				throw new ArgumentNullException("bounds");
			}
			if (indices.Length != bounds.Length)
			{
				throw new ArgumentException("length of bounds does not match length of indices", "indices");
			}
			this._baseArray = (IMyAddressInfo)baseArray;
			this._baseSize = nBaseSize;
			this._indices = indices;
			this._bounds = bounds;
		}

		// Token: 0x17000520 RID: 1312
		// (get) Token: 0x060011D2 RID: 4562 RVA: 0x00032D10 File Offset: 0x00031D10
		public override bool ContainsStackRelativeAddress
		{
			get
			{
				bool flag = this._baseArray.ContainsStackRelativeAddress;
				foreach (IAddressInfo addressInfo in this._indices)
				{
					flag = (flag || (addressInfo is IAddressInfo4 && (addressInfo as IAddressInfo4).ContainsStackRelativeAddress));
					if (flag)
					{
						break;
					}
				}
				return flag;
			}
		}

		// Token: 0x17000521 RID: 1313
		// (get) Token: 0x060011D3 RID: 4563 RVA: 0x00032D63 File Offset: 0x00031D63
		public IAddressInfo Base
		{
			get
			{
				return this._baseArray;
			}
		}

		// Token: 0x17000522 RID: 1314
		// (get) Token: 0x060011D4 RID: 4564 RVA: 0x00032D6B File Offset: 0x00031D6B
		public IAddressInfo[] Indexes
		{
			get
			{
				return this._indices;
			}
		}

		// Token: 0x17000523 RID: 1315
		// (get) Token: 0x060011D5 RID: 4565 RVA: 0x00032D73 File Offset: 0x00031D73
		public IArrayBounds[] Bounds
		{
			get
			{
				return this._bounds;
			}
		}

		// Token: 0x17000524 RID: 1316
		// (get) Token: 0x060011D6 RID: 4566 RVA: 0x00032D7B File Offset: 0x00031D7B
		public override int Size
		{
			get
			{
				return this._baseArray.Size;
			}
		}

		// Token: 0x17000525 RID: 1317
		// (get) Token: 0x060011D7 RID: 4567 RVA: 0x00032D88 File Offset: 0x00031D88
		public int BaseSize
		{
			get
			{
				return this._baseSize;
			}
		}

		// Token: 0x060011D8 RID: 4568 RVA: 0x00032D90 File Offset: 0x00031D90
		public void SetSize(int nSize)
		{
			this._baseArray.SetSize(nSize);
		}

		// Token: 0x060011D9 RID: 4569 RVA: 0x00032DA0 File Offset: 0x00031DA0
		public IMyAddressInfo Duplicate()
		{
			IAddressInfo[] array = new IAddressInfo[this._indices.Length];
			for (int i = 0; i < this._indices.Length; i++)
			{
				array[i] = ((IMyAddressInfo)this._indices[i]).Duplicate();
			}
			IArrayBounds[] array2 = new IArrayBounds[this._bounds.Length];
			for (int j = 0; j < this._bounds.Length; j++)
			{
				array2[j] = (IArrayBounds)((ICloneable)this._bounds[j]).Clone();
			}
			return new VariableArrayAccessAddressInfo(this._baseArray.Duplicate(), this._baseSize, array, array2, base.Type)
			{
				SignatureID = base.SignatureID,
				VariableID = base.VariableID
			};
		}

		// Token: 0x060011DA RID: 4570 RVA: 0x00032E54 File Offset: 0x00031E54
		public override bool Equals(object obj)
		{
			VariableArrayAccessAddressInfo variableArrayAccessAddressInfo = obj as VariableArrayAccessAddressInfo;
			if (variableArrayAccessAddressInfo == null)
			{
				return false;
			}
			if (this._baseSize != variableArrayAccessAddressInfo._baseSize)
			{
				return false;
			}
			if (!this._baseArray.Equals(variableArrayAccessAddressInfo._baseArray))
			{
				return false;
			}
			if (this._indices.Length != variableArrayAccessAddressInfo._indices.Length)
			{
				return false;
			}
			for (int i = 0; i < this._indices.Length; i++)
			{
				if (!this._indices[i].Equals(variableArrayAccessAddressInfo._indices[i]))
				{
					return false;
				}
			}
			if (this._bounds.Length != variableArrayAccessAddressInfo._bounds.Length)
			{
				return false;
			}
			for (int j = 0; j < this._bounds.Length; j++)
			{
				if (!this._bounds[j].Equals(variableArrayAccessAddressInfo._bounds[j]))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060011DB RID: 4571 RVA: 0x00032F14 File Offset: 0x00031F14
		public override int GetHashCode()
		{
			int num = this._baseArray.GetHashCode() ^ this._baseSize.GetHashCode();
			foreach (IAddressInfo addressInfo in this._indices)
			{
				num ^= addressInfo.GetHashCode();
			}
			foreach (IArrayBounds arrayBounds in this._bounds)
			{
				num ^= arrayBounds.GetHashCode();
			}
			return num;
		}

		// Token: 0x0400042A RID: 1066
		private readonly IMyAddressInfo _baseArray;

		// Token: 0x0400042B RID: 1067
		private readonly IAddressInfo[] _indices;

		// Token: 0x0400042C RID: 1068
		private readonly IArrayBounds[] _bounds;

		// Token: 0x0400042D RID: 1069
		private readonly int _baseSize;
	}
}
