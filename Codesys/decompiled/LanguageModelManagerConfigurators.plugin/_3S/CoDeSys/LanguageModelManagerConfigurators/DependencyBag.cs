using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.ComponentModel;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.Core.Options;
using _3S.CoDeSys.OnlineUI;
using CODESYS.ProjectFormat.SideCar;
using CODESYS.ProjectLanguageModelProvider;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	internal class DependencyBag : IDependencyInjectable
	{
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<IEngine9> EngineProvider { get; private set; }

		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<IProjectSideCarService> ProjectSideCarServiceProvider { get; private set; }

		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<ILanguageModelManager29> LanguageModelMgrProvider { get; private set; }

		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<ICompilerVersionManager7> CompilerVersionMgrProvider { get; private set; }

		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<IObjectManager9> ObjectMgrProvider { get; private set; }

		[InjectSingleInstance]
		public ISingleInstanceProvider<IBuildProperty5> BuildPropertyProvider { get; private set; }

		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<ILMServiceProvider> LMServiceProviderProvider { get; private set; }

		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<IOptionStorage> OptionStorageProvider { get; private set; }

		[InjectSingleInstance(Shared = true, Optional = true)]
		public ISharedSingleInstanceProvider<IOnlineUIServices4> OnlineUIServicesProvider { get; private set; }

		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<IProjectLanguageModelProvider> ProjectLanguageModelProvider { get; private set; }

		public DependencyBag()
		{
			ComponentModel.Singleton.InjectDependencies(this, GetType());
		}

		public void InjectionComplete()
		{
		}
	}
}
