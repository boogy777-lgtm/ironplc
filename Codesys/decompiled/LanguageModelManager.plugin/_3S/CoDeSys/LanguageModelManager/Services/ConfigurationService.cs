using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.CommonCompilerData;

namespace _3S.CoDeSys.LanguageModelManager.Services
{
	// Token: 0x02000246 RID: 582
	public class ConfigurationService : ILMConfigurationService
	{
		// Token: 0x17000AF1 RID: 2801
		// (get) Token: 0x060026F5 RID: 9973 RVA: 0x00061D03 File Offset: 0x00060D03
		public IEnumerable<IAttribute> RegisteredAttributes
		{
			get
			{
				return APEnvironmentFacade.Instance.LanguageModelMgr.AttributeManager.RegisteredAttributes;
			}
		}

		// Token: 0x060026F6 RID: 9974 RVA: 0x00061D1C File Offset: 0x00060D1C
		public bool ExecutionpointLoggingEnabled(Guid appGuid)
		{
			if (appGuid == Guid.Empty)
			{
				return false;
			}
			try
			{
				bool enableBreakpointLogging = APEnvironmentFacade.Instance.CompileOptions.EnableBreakpointLogging;
				Guid guid = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceOfApplication(appGuid);
				if (guid == Guid.Empty)
				{
					guid = APEnvironmentFacade.Instance.GetOnlineApplicationDeviceGuid(APEnvironmentFacade.Instance.PrimaryProjectHandle, appGuid);
				}
				if (guid == Guid.Empty)
				{
					return enableBreakpointLogging;
				}
				IDeviceIdentification deviceIdentification = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetTargetIdOfDevice(guid);
				if (deviceIdentification == null)
				{
					deviceIdentification = APEnvironmentFacade.Instance.GetDeviceIdentification(APEnvironmentFacade.Instance.PrimaryProjectHandle, guid);
				}
				if (deviceIdentification == null)
				{
					return enableBreakpointLogging;
				}
				ITargetSettings targetSettingsById = APEnvironmentFacade.Instance.GetTargetSettingsById(deviceIdentification);
				if (targetSettingsById == null)
				{
					return enableBreakpointLogging;
				}
				bool boolValue = LocalTargetSettings.BreakpointsSupported.GetBoolValue(targetSettingsById);
				bool boolValue2 = LocalTargetSettings.EnableBreakpointLogging.GetBoolValue(targetSettingsById);
				Version v = new Version(LocalTargetSettings.RuntimeVersion.GetStringValue(targetSettingsById));
				if (!boolValue || !boolValue2 || v < new Version(3, 5, 5, 0) || !enableBreakpointLogging)
				{
					return false;
				}
			}
			catch
			{
			}
			return true;
		}

		// Token: 0x17000AF2 RID: 2802
		// (get) Token: 0x060026F7 RID: 9975 RVA: 0x00061E4C File Offset: 0x00060E4C
		public ILMWarningConfiguration WarningConfiguration
		{
			get
			{
				return APEnvironmentFacade.Instance.WarningHelper;
			}
		}

		// Token: 0x17000AF3 RID: 2803
		// (get) Token: 0x060026F8 RID: 9976 RVA: 0x00061E58 File Offset: 0x00060E58
		public ILMCompileOptions CompileOptions
		{
			get
			{
				return APEnvironmentFacade.Instance.CompileOptions;
			}
		}

		// Token: 0x17000AF4 RID: 2804
		// (get) Token: 0x060026F9 RID: 9977 RVA: 0x00061E64 File Offset: 0x00060E64
		public ILMLibraryDevelopmentOptions LibraryDevelopmentOptions
		{
			get
			{
				return APEnvironmentFacade.Instance.LibraryDevelopmentOptions;
			}
		}
	}
}
