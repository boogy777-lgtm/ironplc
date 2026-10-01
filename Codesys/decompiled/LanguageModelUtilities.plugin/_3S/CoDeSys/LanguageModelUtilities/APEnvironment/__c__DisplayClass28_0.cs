using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.BuildCommands;
using _3S.CoDeSys.Core;
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
	[Obsolete("Use APEnviromentFacade.Instance instead")]
	internal static class APEnvironment
	{
		private static Lazy<DependencyBag> s_bag = new Lazy<DependencyBag>(() => new DependencyBag());

		public static IFeatureSettingsManager FeatureSettingsMgrOrNull => s_bag.Value.FeatureSettingsMgrProvider.Value;

		public static IObjectManager8 ObjectMgr => s_bag.Value.ObjectMgrProvider.Value;

		public static ILibraryLoader8 LibraryLoader => s_bag.Value.LibraryLoaderProvider.Value;

		public static IMessageStorage MessageStorage => s_bag.Value.MessageStorageProvider.Value;

		public static LanguageModelUtilities LanguageModelUtilities => s_bag.Value.LanguageModelUtilitiesProvider.Value;

		public static IEngine Engine => s_bag.Value.EngineProvider.Value;

		public static IAddressInfoFactory AddressInfoFactory => s_bag.Value.AddressInfoFactoryProvider.Value;

		public static ILanguageModelManager22 LanguageModelMgr => s_bag.Value.LanguageModelMgrProvider.Value;

		public static ILMServiceProvider LMServiceProvider => s_bag.Value.LMServiceProviderProvider.Value;

		public static ICompilerVersionManager6 CompilerVersionMgr => s_bag.Value.CompilerVersionMgrProvider.Value;

		public static IOptionStorage OptionStorage => s_bag.Value.OptionStorageProvider.Value;

		public static IEnumerable<IAdditionalCrossReferenceProvider> AdditionalCrossReferenceProviders => s_bag.Value.AdditionalCrossReferenceProvidersProvider.Value;

		public static IEnumerable<ISimpleFilteredAdditionalCrossReferenceProvider> SimpleFilteredAdditionalCrossReferenceProviders
		{
			get
			{
				HashSet<Type> morePreferredOnes = new HashSet<Type>();
				foreach (IAdditionalCrossReferenceProvider additionalCrossReferenceProvider in AdditionalCrossReferenceProviders)
				{
					morePreferredOnes.Add(additionalCrossReferenceProvider.GetType());
				}
				return s_bag.Value.SimpleFilteredAdditionalCrossReferenceProvidersProvider.Value.Where((ISimpleFilteredAdditionalCrossReferenceProvider entry) => !morePreferredOnes.Contains(entry.GetType()));
			}
		}

		public static IEnumerable<ISimpleAdditionalCrossReferenceProvider> SimpleAdditionalCrossReferenceProviders
		{
			get
			{
				HashSet<Type> morePreferredOnes = new HashSet<Type>();
				foreach (IAdditionalCrossReferenceProvider additionalCrossReferenceProvider in AdditionalCrossReferenceProviders)
				{
					morePreferredOnes.Add(additionalCrossReferenceProvider.GetType());
				}
				foreach (ISimpleFilteredAdditionalCrossReferenceProvider simpleFilteredAdditionalCrossReferenceProvider in SimpleFilteredAdditionalCrossReferenceProviders)
				{
					morePreferredOnes.Add(simpleFilteredAdditionalCrossReferenceProvider.GetType());
				}
				return s_bag.Value.SimpleAdditionalCrossReferenceProvidersProvider.Value.Where((ISimpleAdditionalCrossReferenceProvider entry) => !morePreferredOnes.Contains(entry.GetType()));
			}
		}

		public static PrecompileCrossReferenceService PrecompileCrossReferenceService => s_bag.Value.PrecompileCrossReferenceServiceProvider.Value;

		public static IOnlineManager6 OnlineMgr => s_bag.Value.OnlineMgrProvider.Value;

		public static ITargetSettingsManager TargetSettingsMgr => s_bag.Value.TargetSettingsMgrProvider.Value;

		public static ITargetSettingsProvider TargetSettingsProvider => s_bag.Value.TargetSettingsProviderProvider.Value;

		public static ILocalizationManager LocalizationManagerOrNull => s_bag.Value.LocalizationManagerProvider.Value;

		public static IMessageCategory CompilerMessageCategoryOrNull => s_bag.Value.CompilerMessageCategoryProvider.Value;

		public static IEnumerable<IGenerateExtCodeProvider> CreateGenerateExtCodeProviders()
		{
			return s_bag.Value.GenerateExtCodeProvidersProvider.Create();
		}
	}
}
