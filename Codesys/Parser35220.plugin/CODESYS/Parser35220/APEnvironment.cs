using System;
using System.Diagnostics.CodeAnalysis;

namespace CODESYS.Parser35220
{
	// Token: 0x02000004 RID: 4
	[ExcludeFromCodeCoverage]
	internal static class APEnvironment
	{
		// Token: 0x04000001 RID: 1
		private static Lazy<DependencyBag> s_bag = new Lazy<DependencyBag>(() => new DependencyBag());
	}
}
