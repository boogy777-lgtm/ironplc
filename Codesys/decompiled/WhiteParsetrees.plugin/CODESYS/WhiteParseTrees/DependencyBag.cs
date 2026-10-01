using _3S.CoDeSys.Core.ComponentModel;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.WhiteParseTrees
{
	internal class DependencyBag : IDependencyInjectable
	{
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<_ILanguageModelManagerConsolidated> LanguageModelManagerConsolidatedProvider { get; private set; }

		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<ILMServiceProvider> LMServiceProviderProvider { get; private set; }

		public DependencyBag()
		{
			_3S.CoDeSys.Core.ComponentModel.ComponentModel.Singleton.InjectDependencies(this, GetType());
		}

		public void InjectionComplete()
		{
		}
	}
}
