using System;
using CODESYS.Objects.Properties;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000C0 RID: 192
	[ReleasedClass]
	public class ObjectManagerAlreadyExistsException : ObjectManagerException
	{
		// Token: 0x06000310 RID: 784 RVA: 0x000053D9 File Offset: 0x000035D9
		public ObjectManagerAlreadyExistsException() : base(-1, Guid.Empty, null, string.Empty)
		{
		}

		// Token: 0x17000123 RID: 291
		// (get) Token: 0x06000311 RID: 785 RVA: 0x000053ED File Offset: 0x000035ED
		public override string Message
		{
			get
			{
				return Resources.ObjectManagerAlreadyExistsException;
			}
		}
	}
}
