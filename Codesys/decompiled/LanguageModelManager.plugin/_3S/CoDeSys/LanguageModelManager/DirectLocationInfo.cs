using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000141 RID: 321
	public class DirectLocationInfo : _IDirectLocationInfo
	{
		// Token: 0x06001B1E RID: 6942 RVA: 0x0004D293 File Offset: 0x0004C293
		internal DirectLocationInfo(IDataLocation datloc, IMessage message, bool bError)
		{
			this._datloc = datloc;
			this._message = message;
			this._bError = bError;
		}

		// Token: 0x17000745 RID: 1861
		// (get) Token: 0x06001B1F RID: 6943 RVA: 0x0004D2B0 File Offset: 0x0004C2B0
		// (set) Token: 0x06001B20 RID: 6944 RVA: 0x0004D2B8 File Offset: 0x0004C2B8
		public IDataLocation DatLoc
		{
			get
			{
				return this._datloc;
			}
			set
			{
				this._datloc = value;
			}
		}

		// Token: 0x17000746 RID: 1862
		// (get) Token: 0x06001B21 RID: 6945 RVA: 0x0004D2C1 File Offset: 0x0004C2C1
		// (set) Token: 0x06001B22 RID: 6946 RVA: 0x0004D2C9 File Offset: 0x0004C2C9
		public IMessage Message
		{
			get
			{
				return this._message;
			}
			set
			{
				this._message = value;
			}
		}

		// Token: 0x17000747 RID: 1863
		// (get) Token: 0x06001B23 RID: 6947 RVA: 0x0004D2D2 File Offset: 0x0004C2D2
		// (set) Token: 0x06001B24 RID: 6948 RVA: 0x0004D2DA File Offset: 0x0004C2DA
		public bool Error
		{
			get
			{
				return this._bError;
			}
			set
			{
				this._bError = value;
			}
		}

		// Token: 0x040005AD RID: 1453
		internal IDataLocation _datloc;

		// Token: 0x040005AE RID: 1454
		internal IMessage _message;

		// Token: 0x040005AF RID: 1455
		internal bool _bError;
	}
}
