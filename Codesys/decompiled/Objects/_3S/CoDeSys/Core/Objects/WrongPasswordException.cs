using System;
using CODESYS.Objects.Properties;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000CF RID: 207
	[ReleasedClass]
	public class WrongPasswordException : ObjectManagerException
	{
		// Token: 0x06000347 RID: 839 RVA: 0x00005751 File Offset: 0x00003951
		public WrongPasswordException() : base(-1, Guid.Empty, null, string.Empty)
		{
		}

		// Token: 0x17000143 RID: 323
		// (get) Token: 0x06000348 RID: 840 RVA: 0x00005765 File Offset: 0x00003965
		public override string Message
		{
			get
			{
				return Resources.WrongPasswordException;
			}
		}
	}
}
