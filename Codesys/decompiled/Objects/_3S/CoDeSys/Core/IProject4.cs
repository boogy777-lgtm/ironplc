using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core
{
	// Token: 0x0200001D RID: 29
	[ReleasedInterface]
	public interface IProject4 : IProject3, IProject2, IProject
	{
		// Token: 0x06000092 RID: 146
		bool Save(Profile profile, string stProfileName);
	}
}
