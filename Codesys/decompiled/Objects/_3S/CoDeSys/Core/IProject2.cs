using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Options;

namespace _3S.CoDeSys.Core
{
	// Token: 0x0200001B RID: 27
	[ReleasedInterface]
	public interface IProject2 : IProject
	{
		// Token: 0x0600008F RID: 143
		IOptionKey GetProjectOptionsRootKey();

		// Token: 0x17000037 RID: 55
		// (get) Token: 0x06000090 RID: 144
		bool ReadOnly { get; }
	}
}
