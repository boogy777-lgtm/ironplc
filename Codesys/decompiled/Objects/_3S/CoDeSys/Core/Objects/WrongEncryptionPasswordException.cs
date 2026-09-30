using System;
using CODESYS.Objects.Properties;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000CE RID: 206
	[ReleasedClass]
	public class WrongEncryptionPasswordException : ObjectManagerException
	{
		// Token: 0x06000345 RID: 837 RVA: 0x00005712 File Offset: 0x00003912
		public WrongEncryptionPasswordException(string stStreamName) : base(-1, Guid.Empty, string.Empty, string.Empty)
		{
			if (stStreamName == null)
			{
				throw new ArgumentNullException("stStreamName");
			}
			this._stStreamName = stStreamName;
		}

		// Token: 0x17000142 RID: 322
		// (get) Token: 0x06000346 RID: 838 RVA: 0x0000573F File Offset: 0x0000393F
		public override string Message
		{
			get
			{
				return string.Format(Resources.WrongEncryptionPasswordException, this._stStreamName);
			}
		}

		// Token: 0x0400012E RID: 302
		private string _stStreamName;
	}
}
