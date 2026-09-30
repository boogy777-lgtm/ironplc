using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManagerLegacy
{
	internal static class APEnvironment
	{
		private static Lazy<DependencyBag> s_bag = new Lazy<DependencyBag>(() => new DependencyBag());

		public static ILMServiceProvider LMServiceProvider => s_bag.Value.LMServiceProviderProvider.Value;
	}
}
