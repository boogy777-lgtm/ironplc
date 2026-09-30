using _3S.CoDeSys.BuildCommands;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.ComponentModel;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.Core.Online;
using _3S.CoDeSys.Core.Options;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LibManObject;
using _3S.CoDeSys.ProjectLocalization;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class DependencyBag : IDependencyInjectable
	{
		[InjectSingleInstance(Shared = true, Optional = true)]
		public ISharedSingleInstanceProvider<IFeatureSettingsManager> FeatureSettingsMgrProvider { get; private set; }

		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<IObjectManager8> ObjectMgrProvider { get; private set; }

		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<ILibraryLoader8> LibraryLoaderProvider { get; private set; }

		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<IMessageStorage> MessageStorageProvider { get; private set; }

		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<LanguageModelUtilities> LanguageModelUtilitiesProvider { get; private set; }

		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<IEngine> EngineProvider { get; private set; }

		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<IAddressInfoFactory> AddressInfoFactoryProvider { get; private set; }

		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<ILanguageModelManager22> LanguageModelMgrProvider { get; private set; }

		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<ILMServiceProvider> LMServiceProviderProvider { get; private set; }

		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<ICompilerVersionManager6> CompilerVersionMgrProvider { get; private set; }

		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<IOptionStorage> OptionStorageProvider { get; private set; }

		[InjectMultipleInstances(Shared = true, Optional = true)]
		public ISharedMultipleInstancesProvider<IAdditionalCrossReferenceProvider> AdditionalCrossReferenceProvidersProvider { get; private set; }

		[InjectMultipleInstances(Shared = true, Optional = true)]
		public ISharedMultipleInstancesProvider<ISimpleAdditionalCrossReferenceProvider> SimpleAdditionalCrossReferenceProvidersProvider { get; private set; }

		[InjectMultipleInstances(Shared = true, Optional = true)]
		public ISharedMultipleInstancesProvider<ISimpleFilteredAdditionalCrossReferenceProvider> SimpleFilteredAdditionalCrossReferenceProvidersProvider { get; private set; }

		[InjectMultipleInstances(Optional = true)]
		public IMultipleInstancesProvider<IGenerateExtCodeProvider> GenerateExtCodeProvidersProvider { get; private set; }

		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<PrecompileCrossReferenceService> PrecompileCrossReferenceServiceProvider { get; private set; }

		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<IOnlineManager6> OnlineMgrProvider { get; private set; }

		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<ITargetSettingsManager> TargetSettingsMgrProvider { get; private set; }

		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<ITargetSettingsProvider> TargetSettingsProviderProvider { get; private set; }

		[InjectSingleInstance(Shared = true, Optional = true)]
		public ISharedSingleInstanceProvider<ILocalizationManager> LocalizationManagerProvider { get; private set; }

		[InjectSingleInstance(Shared = true, Optional = true)]
		public ISharedSingleInstanceProvider<IMessageCategory> CompilerMessageCategoryProvider { get; private set; }

		public DependencyBag()
		{
			ComponentModel.Singleton.InjectDependencies(this, GetType());
		}

		public void InjectionComplete()
		{
		}
	}
}
