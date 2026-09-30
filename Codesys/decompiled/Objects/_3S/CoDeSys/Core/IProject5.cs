using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core
{
	// Token: 0x0200001E RID: 30
	[ReleasedInterface]
	public interface IProject5 : IProject4, IProject3, IProject2, IProject
	{
		// Token: 0x17000038 RID: 56
		// (get) Token: 0x06000093 RID: 147
		// (set) Token: 0x06000094 RID: 148
		bool ExplicitlySaved { get; set; }
	}
}
