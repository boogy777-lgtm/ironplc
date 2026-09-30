using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.WhiteParseTrees
{
	internal static class APEnvironment
	{
		private static readonly Lazy<DependencyBag> s_bag = new Lazy<DependencyBag>(() => new DependencyBag());

		public static _ILanguageModelManagerConsolidated LanguageModelManagerConsolidated => s_bag.Value.LanguageModelManagerConsolidatedProvider.Value;

		public static ILMServiceProvider LMServiceProvider => s_bag.Value.LMServiceProviderProvider.Value;
	}
}
