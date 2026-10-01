using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.Core.Options;
using _3S.CoDeSys.OnlineUI;
using CODESYS.ProjectFormat.SideCar;
using CODESYS.ProjectLanguageModelProvider;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	internal static class APEnvironment
	{
		private static Lazy<DependencyBag> s_bag = new Lazy<DependencyBag>(() => new DependencyBag());

		public static ICompilerVersionManager7 CompilerVersionMgr => s_bag.Value.CompilerVersionMgrProvider.Value;

		public static IEngine9 Engine => s_bag.Value.EngineProvider.Value;

		public static IProjectSideCarService ProjectSideCarService => s_bag.Value.ProjectSideCarServiceProvider.Value;

		public static ILanguageModelManager29 LanguageModelMgr => s_bag.Value.LanguageModelMgrProvider.Value;

		public static ILMServiceProvider LMServiceProvider => s_bag.Value.LMServiceProviderProvider.Value;

		public static IObjectManager9 ObjectMgr => s_bag.Value.ObjectMgrProvider.Value;

		public static IOptionStorage OptionStorage => s_bag.Value.OptionStorageProvider.Value;

		public static IOnlineUIServices4 OnlineUIServicesOrNull => s_bag.Value.OnlineUIServicesProvider.Value;

		public static IProjectLanguageModelProvider ProjectLanguageModel => s_bag.Value.ProjectLanguageModelProvider.Value;

		public static IBuildProperty5 CreateBuildProperty()
		{
			return s_bag.Value.BuildPropertyProvider.Create();
		}
	}
}
