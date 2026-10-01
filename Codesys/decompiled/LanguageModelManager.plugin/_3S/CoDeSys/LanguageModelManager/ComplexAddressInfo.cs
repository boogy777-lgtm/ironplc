using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Online;
using _3S.CoDeSys.OnlineExpressionInterpreter;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000EA RID: 234
	internal class ComplexAddressInfo : AddressInfoBase, IComplexAddressInfo, IAddressInfo, IMyAddressInfo, IAddressInfo4, IAddressInfo3, IAddressInfo2
	{
		// Token: 0x0600117B RID: 4475 RVA: 0x000325B2 File Offset: 0x000315B2
		internal ComplexAddressInfo() : base(null)
		{
		}

		// Token: 0x170004FB RID: 1275
		// (get) Token: 0x0600117C RID: 4476 RVA: 0x000325BB File Offset: 0x000315BB
		public IOnlineVarRef5 VarRefProxy
		{
			get
			{
				return this._oexp;
			}
		}

		// Token: 0x0600117D RID: 4477 RVA: 0x000325C3 File Offset: 0x000315C3
		internal void SetOnlineExpression(IOnlineExpression oexp)
		{
			this._oexp = new OnlineExpressionProxy(oexp);
		}

		// Token: 0x0600117E RID: 4478 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void SetSize(int nSize)
		{
		}

		// Token: 0x0600117F RID: 4479 RVA: 0x000325D1 File Offset: 0x000315D1
		public IMyAddressInfo Duplicate()
		{
			return new ComplexAddressInfo
			{
				_oexp = this._oexp
			};
		}

		// Token: 0x170004FC RID: 1276
		// (get) Token: 0x06001180 RID: 4480 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override int Size
		{
			get
			{
				return 0;
			}
		}

		// Token: 0x04000412 RID: 1042
		private OnlineExpressionProxy _oexp;
	}
}
