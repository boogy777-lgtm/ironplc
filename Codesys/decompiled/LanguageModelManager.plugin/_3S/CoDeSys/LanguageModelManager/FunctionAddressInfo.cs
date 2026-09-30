using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000F6 RID: 246
	internal class FunctionAddressInfo : AddressInfoBase, IFunctionAddressInfo, IAddressInfo
	{
		// Token: 0x06001211 RID: 4625 RVA: 0x00033568 File Offset: 0x00032568
		public FunctionAddressInfo(int nSize, IType t) : base(t)
		{
			this._nSize = nSize;
		}

		// Token: 0x17000539 RID: 1337
		// (get) Token: 0x06001212 RID: 4626 RVA: 0x0003359B File Offset: 0x0003259B
		public override int Size
		{
			get
			{
				return this._nSize;
			}
		}

		// Token: 0x06001213 RID: 4627 RVA: 0x000335A3 File Offset: 0x000325A3
		public void SetSize(int nSize)
		{
			this._nSize = nSize;
		}

		// Token: 0x1700053A RID: 1338
		// (get) Token: 0x06001214 RID: 4628 RVA: 0x000335AC File Offset: 0x000325AC
		// (set) Token: 0x06001215 RID: 4629 RVA: 0x000335B4 File Offset: 0x000325B4
		public int FunctionPointerArea
		{
			get
			{
				return this._nFunctionPointerArea;
			}
			set
			{
				this._nFunctionPointerArea = value;
			}
		}

		// Token: 0x1700053B RID: 1339
		// (get) Token: 0x06001216 RID: 4630 RVA: 0x000335BD File Offset: 0x000325BD
		// (set) Token: 0x06001217 RID: 4631 RVA: 0x000335C5 File Offset: 0x000325C5
		public int FunctionPointerOffset
		{
			get
			{
				return this._nFunctionPointerOffset;
			}
			set
			{
				this._nFunctionPointerOffset = value;
			}
		}

		// Token: 0x1700053C RID: 1340
		// (get) Token: 0x06001218 RID: 4632 RVA: 0x000335CE File Offset: 0x000325CE
		// (set) Token: 0x06001219 RID: 4633 RVA: 0x000335D6 File Offset: 0x000325D6
		public int ResultSize
		{
			get
			{
				return this._nResultSize;
			}
			set
			{
				this._nResultSize = value;
			}
		}

		// Token: 0x1700053D RID: 1341
		// (get) Token: 0x0600121A RID: 4634 RVA: 0x000335DF File Offset: 0x000325DF
		// (set) Token: 0x0600121B RID: 4635 RVA: 0x000335E7 File Offset: 0x000325E7
		public int ResultOffset
		{
			get
			{
				return this._nResultOffset;
			}
			set
			{
				this._nResultOffset = value;
			}
		}

		// Token: 0x1700053E RID: 1342
		// (get) Token: 0x0600121C RID: 4636 RVA: 0x000335F0 File Offset: 0x000325F0
		// (set) Token: 0x0600121D RID: 4637 RVA: 0x000335F8 File Offset: 0x000325F8
		public ICompiledType ResultCompiledType
		{
			get
			{
				return this._type;
			}
			set
			{
				this._type = value;
			}
		}

		// Token: 0x1700053F RID: 1343
		// (get) Token: 0x0600121E RID: 4638 RVA: 0x00033601 File Offset: 0x00032601
		// (set) Token: 0x0600121F RID: 4639 RVA: 0x00033609 File Offset: 0x00032609
		public short ImplementationStyle
		{
			get
			{
				return this._nImplementationStyle;
			}
			set
			{
				this._nImplementationStyle = value;
			}
		}

		// Token: 0x17000540 RID: 1344
		// (get) Token: 0x06001220 RID: 4640 RVA: 0x00033612 File Offset: 0x00032612
		// (set) Token: 0x06001221 RID: 4641 RVA: 0x0003361A File Offset: 0x0003261A
		public short InputParameterCount
		{
			get
			{
				return this._nNumParameters;
			}
			set
			{
				this._nNumParameters = value;
			}
		}

		// Token: 0x17000541 RID: 1345
		// (get) Token: 0x06001222 RID: 4642 RVA: 0x00033623 File Offset: 0x00032623
		// (set) Token: 0x06001223 RID: 4643 RVA: 0x0003362B File Offset: 0x0003262B
		public IAddressInfo[] InputParameterAddressInfos
		{
			get
			{
				return this._paramAddrInfos;
			}
			set
			{
				this._paramAddrInfos = value;
			}
		}

		// Token: 0x17000542 RID: 1346
		// (get) Token: 0x06001224 RID: 4644 RVA: 0x00033634 File Offset: 0x00032634
		// (set) Token: 0x06001225 RID: 4645 RVA: 0x0003363C File Offset: 0x0003263C
		public int[] InputParameterOffsets
		{
			get
			{
				return this._paramOffsets;
			}
			set
			{
				this._paramOffsets = value;
			}
		}

		// Token: 0x17000543 RID: 1347
		// (get) Token: 0x06001226 RID: 4646 RVA: 0x00033645 File Offset: 0x00032645
		// (set) Token: 0x06001227 RID: 4647 RVA: 0x0003364D File Offset: 0x0003264D
		public int SignatureSize
		{
			get
			{
				return this._nSignSize;
			}
			set
			{
				this._nSignSize = value;
			}
		}

		// Token: 0x0400043A RID: 1082
		private int _nSize = -1;

		// Token: 0x0400043B RID: 1083
		private int _nFunctionPointerArea = -1;

		// Token: 0x0400043C RID: 1084
		private int _nFunctionPointerOffset = -1;

		// Token: 0x0400043D RID: 1085
		private short _nImplementationStyle;

		// Token: 0x0400043E RID: 1086
		private short _nNumParameters;

		// Token: 0x0400043F RID: 1087
		private int _nResultSize = -1;

		// Token: 0x04000440 RID: 1088
		private int _nResultOffset = -1;

		// Token: 0x04000441 RID: 1089
		private int _nSignSize;

		// Token: 0x04000442 RID: 1090
		private int[] _paramOffsets;

		// Token: 0x04000443 RID: 1091
		private ICompiledType _type;

		// Token: 0x04000444 RID: 1092
		private IAddressInfo[] _paramAddrInfos;
	}
}
