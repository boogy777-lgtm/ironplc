using System;
using CODESYS.Objects.Properties;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000B8 RID: 184
	[ReleasedClass]
	public class InvalidProjectHandleException : ObjectManagerException
	{
		// Token: 0x060002F2 RID: 754 RVA: 0x000051EA File Offset: 0x000033EA
		public InvalidProjectHandleException(int nProjectHandle) : base(nProjectHandle, Guid.Empty, null, string.Empty)
		{
		}

		// Token: 0x17000111 RID: 273
		// (get) Token: 0x060002F3 RID: 755 RVA: 0x000051FE File Offset: 0x000033FE
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x17000112 RID: 274
		// (get) Token: 0x060002F4 RID: 756 RVA: 0x00005206 File Offset: 0x00003406
		public override string Message
		{
			get
			{
				return string.Format(Resources.InvalidProjectHandleException, this._nProjectHandle);
			}
		}
	}
}
