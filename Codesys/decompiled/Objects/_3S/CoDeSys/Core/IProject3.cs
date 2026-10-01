using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core
{
	// Token: 0x0200001C RID: 28
	[ReleasedInterface]
	public interface IProject3 : IProject2, IProject
	{
		// Token: 0x06000091 RID: 145
		void SetDirty(bool bDirty);
	}
}
