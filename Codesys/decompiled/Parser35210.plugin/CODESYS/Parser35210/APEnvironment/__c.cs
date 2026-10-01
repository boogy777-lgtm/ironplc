using System;
using System.Diagnostics.CodeAnalysis;

namespace CODESYS.Parser35210
{
	[ExcludeFromCodeCoverage]
	internal static class APEnvironment
	{
		private static Lazy<DependencyBag> s_bag = new Lazy<DependencyBag>(() => new DependencyBag());
	}
}
