using _3S.CoDeSys.Core.ComponentModel;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManagerLegacy
{
	internal class DependencyBag : IDependencyInjectable
	{
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<ILMServiceProvider> LMServiceProviderProvider { get; private set; }

		public DependencyBag()
		{
			ComponentModel.Singleton.InjectDependencies(this, GetType());
		}

		public void InjectionComplete()
		{
		}
	}
}
