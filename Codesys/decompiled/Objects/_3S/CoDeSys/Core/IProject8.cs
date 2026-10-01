using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core
{
	// Token: 0x02000021 RID: 33
	[ReleasedInterface]
	public interface IProject8 : IProject7, IProject6, IProject5, IProject4, IProject3, IProject2, IProject
	{
		// Token: 0x1700003A RID: 58
		// (get) Token: 0x0600009C RID: 156
		Profile CurrentProfileForSaving { get; }
	}
}
