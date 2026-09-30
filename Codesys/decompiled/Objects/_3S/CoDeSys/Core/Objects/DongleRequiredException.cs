using System;
using CODESYS.Objects.Properties;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000B6 RID: 182
	[ReleasedClass]
	public class DongleRequiredException : ObjectManagerException
	{
		// Token: 0x060002EB RID: 747 RVA: 0x0000514C File Offset: 0x0000334C
		public DongleRequiredException(string stStreamName, IDongleEncryption dongleEncryption) : base(-1, Guid.Empty, string.Empty, string.Empty)
		{
			if (stStreamName == null)
			{
				throw new ArgumentNullException("stStreamName");
			}
			if (dongleEncryption == null)
			{
				throw new ArgumentNullException("dongleEncryption");
			}
			this._stStreamName = stStreamName;
			this._dongleEncryption = dongleEncryption;
		}

		// Token: 0x1700010C RID: 268
		// (get) Token: 0x060002EC RID: 748 RVA: 0x00005199 File Offset: 0x00003399
		public IDongleEncryption DongleEncryption
		{
			get
			{
				return this._dongleEncryption;
			}
		}

		// Token: 0x1700010D RID: 269
		// (get) Token: 0x060002ED RID: 749 RVA: 0x000051A1 File Offset: 0x000033A1
		public override string Message
		{
			get
			{
				return string.Format(Resources.DongleRequiredException, this._stStreamName);
			}
		}

		// Token: 0x0400011D RID: 285
		private string _stStreamName;

		// Token: 0x0400011E RID: 286
		private IDongleEncryption _dongleEncryption;
	}
}
