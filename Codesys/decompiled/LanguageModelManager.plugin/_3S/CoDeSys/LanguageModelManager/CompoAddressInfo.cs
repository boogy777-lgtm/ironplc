using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000F5 RID: 245
	internal class CompoAddressInfo : AddressInfoBase, ICompoAddressInfo, IAddressInfo, IMyAddressInfo, IAddressInfo4, IAddressInfo3, IAddressInfo2
	{
		// Token: 0x06001208 RID: 4616 RVA: 0x00033467 File Offset: 0x00032467
		public CompoAddressInfo(IAddressInfo aiBase, int nSize, int nOffset, IType t) : base(t)
		{
			this._aiBase = (aiBase as IMyAddressInfo);
			this._aiBase.SetSize(nSize);
			this._nOffset = nOffset;
		}

		// Token: 0x17000535 RID: 1333
		// (get) Token: 0x06001209 RID: 4617 RVA: 0x00033490 File Offset: 0x00032490
		public override int Size
		{
			get
			{
				return this._aiBase.Size;
			}
		}

		// Token: 0x0600120A RID: 4618 RVA: 0x0003349D File Offset: 0x0003249D
		public void SetSize(int nSize)
		{
			this._aiBase.SetSize(nSize);
		}

		// Token: 0x0600120B RID: 4619 RVA: 0x000334AB File Offset: 0x000324AB
		public IMyAddressInfo Duplicate()
		{
			return new CompoAddressInfo(this._aiBase.Duplicate(), this.Size, this._nOffset, base.Type)
			{
				SignatureID = base.SignatureID,
				VariableID = base.VariableID
			};
		}

		// Token: 0x0600120C RID: 4620 RVA: 0x000334E8 File Offset: 0x000324E8
		public override bool Equals(object obj)
		{
			CompoAddressInfo compoAddressInfo = obj as CompoAddressInfo;
			return compoAddressInfo != null && this._aiBase.Equals(compoAddressInfo._aiBase) && this._nOffset == compoAddressInfo._nOffset;
		}

		// Token: 0x0600120D RID: 4621 RVA: 0x00033524 File Offset: 0x00032524
		public override int GetHashCode()
		{
			return this._aiBase.GetHashCode() ^ this._nOffset.GetHashCode();
		}

		// Token: 0x17000536 RID: 1334
		// (get) Token: 0x0600120E RID: 4622 RVA: 0x0003354B File Offset: 0x0003254B
		public IAddressInfo Base
		{
			get
			{
				return this._aiBase;
			}
		}

		// Token: 0x17000537 RID: 1335
		// (get) Token: 0x0600120F RID: 4623 RVA: 0x00033553 File Offset: 0x00032553
		public int Offset
		{
			get
			{
				return this._nOffset;
			}
		}

		// Token: 0x17000538 RID: 1336
		// (get) Token: 0x06001210 RID: 4624 RVA: 0x0003355B File Offset: 0x0003255B
		public override bool ContainsStackRelativeAddress
		{
			get
			{
				return this._aiBase.ContainsStackRelativeAddress;
			}
		}

		// Token: 0x04000438 RID: 1080
		private readonly IMyAddressInfo _aiBase;

		// Token: 0x04000439 RID: 1081
		private readonly int _nOffset;
	}
}
