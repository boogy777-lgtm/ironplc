using System;
using CODESYS.Objects.Properties;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000C9 RID: 201
	[ReleasedClass]
	public class ProjectIntegrityException : ObjectManagerException
	{
		// Token: 0x06000336 RID: 822 RVA: 0x00005630 File Offset: 0x00003830
		public ProjectIntegrityException(string stStreamName) : base(-1, Guid.Empty, string.Empty, string.Empty)
		{
			this._stStreamName = stStreamName;
		}

		// Token: 0x17000139 RID: 313
		// (get) Token: 0x06000337 RID: 823 RVA: 0x0000564F File Offset: 0x0000384F
		public override string Message
		{
			get
			{
				return string.Format(Resources.ProjectIntegrityException, this._stStreamName);
			}
		}

		// Token: 0x0400012C RID: 300
		private string _stStreamName;
	}
}
