using System;
using System.Collections.Generic;
using System.Globalization;
using \u0016;
using \u0019;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u000F
{
	// Token: 0x020003C3 RID: 963
	internal static class \u0019
	{
		// Token: 0x060036D0 RID: 14032 RVA: 0x000DEDF0 File Offset: 0x000DCFF0
		private static uint \u0001(Version \u0002)
		{
			return (uint)(\u0002.Major << 24 | \u0002.Minor << 16 | \u0002.Build << 8 | \u0002.Revision);
		}

		// Token: 0x060036D1 RID: 14033 RVA: 0x000DEE18 File Offset: 0x000DD018
		internal static void \u0001(_ICompileContext \u0002, ICodegenerator6 \u0003)
		{
			if (\u0003 == null)
			{
				return;
			}
			int num = 0;
			if (((\u0002 != null) ? \u0002.DeviceId : null) != null)
			{
				num = \u0002.DeviceId.Type;
			}
			if (\u0002 == null)
			{
				return;
			}
			_ISignature isignature = \u0002[new Guid("{C912F995-F4B5-48BD-9416-F36B0846C0E1}")];
			ITargetSettings targetSettings = \u0002.GetTargetSettings();
			bool boolValue = \u0016.\u0004.SupportMulticore.GetBoolValue(targetSettings);
			if (isignature != null)
			{
				_IVariable ivariable = isignature["bLittleEndian"] as _IVariable;
				_IVariable ivariable2 = isignature["RuntimeVersion"] as _IVariable;
				_IVariable ivariable3 = isignature["CompilerVersion"] as _IVariable;
				_IVariable ivariable4 = isignature["bSimulationMode"] as _IVariable;
				_IVariable ivariable5 = isignature["nRegisterSize"] as _IVariable;
				_IVariable ivariable6 = isignature["nPackMode"] as _IVariable;
				_IVariable ivariable7 = isignature["bFPUSupport"] as _IVariable;
				_IVariable ivariable8 = isignature["bMulticoreSupport"] as _IVariable;
				_IVariable ivariable9 = isignature["vcOptimalLREAL"] as _IVariable;
				_IVariable ivariable10 = isignature["vcOptimalREAL"] as _IVariable;
				_IVariable ivariable11 = isignature["nRuntimeDeviceType"] as _IVariable;
				_IVariable ivariable12 = isignature["bUTF8Encoding"] as _IVariable;
				ivariable.Initial = \u0019.\u0003.\u0001(!\u0003.MotorolaByteOrder);
				ivariable4.Initial = \u0019.\u0003.\u0001(\u0002.SimulationMode);
				ivariable5.Initial = \u0019.\u0003.\u0001((long)(\u0003.RegisterSize * 8));
				ivariable6.Initial = \u0019.\u0003.\u0001((long)\u0002.DataManager.PackMode);
				ivariable7.Initial = \u0019.\u0003.\u0001(\u0003.FPUSupport);
				ivariable8.Initial = \u0019.\u0003.\u0001(boolValue);
				ivariable11.Initial = \u0019.\u0003.\u0001((long)num);
				ivariable12.Initial = \u0019.\u0003.\u0001(APEnvironmentFacade.Instance.CompileOptions.UTF8Encoding);
				_ILanguageModelBuilder ilanguageModelBuilder = \u0019.\u0003.Builder;
				List<IAssignmentExpression> list = new List<IAssignmentExpression>();
				Version version = APEnvironmentFacade.Instance.CompilerVersionToUseInternal();
				list.Add(ilanguageModelBuilder.CreateAssignmentExpression(null, \u0019.\u0003.\u0001("uiMajor"), \u0019.\u0003.\u0001((long)version.Major)));
				list.Add(ilanguageModelBuilder.CreateAssignmentExpression(null, \u0019.\u0003.\u0001("uiMinor"), \u0019.\u0003.\u0001((long)version.Minor)));
				list.Add(ilanguageModelBuilder.CreateAssignmentExpression(null, \u0019.\u0003.\u0001("uiServicePack"), \u0019.\u0003.\u0001((long)version.Build)));
				list.Add(ilanguageModelBuilder.CreateAssignmentExpression(null, \u0019.\u0003.\u0001("uiPatch"), \u0019.\u0003.\u0001((long)version.Revision)));
				ivariable3.Initial = (ilanguageModelBuilder.CreateStructureInitialisation(null, list) as _IStructureInitialization);
				Version version2 = Helper.\u0001(targetSettings);
				list.Clear();
				list.Add(ilanguageModelBuilder.CreateAssignmentExpression(null, \u0019.\u0003.\u0001("uiMajor"), \u0019.\u0003.\u0001((long)version2.Major)));
				list.Add(ilanguageModelBuilder.CreateAssignmentExpression(null, \u0019.\u0003.\u0001("uiMinor"), \u0019.\u0003.\u0001((long)version2.Minor)));
				list.Add(ilanguageModelBuilder.CreateAssignmentExpression(null, \u0019.\u0003.\u0001("uiServicePack"), \u0019.\u0003.\u0001((long)version2.Build)));
				list.Add(ilanguageModelBuilder.CreateAssignmentExpression(null, \u0019.\u0003.\u0001("uiPatch"), \u0019.\u0003.\u0001((long)version2.Revision)));
				ivariable2.Initial = (ilanguageModelBuilder.CreateStructureInitialisation(null, list) as _IStructureInitialization);
				(isignature["RuntimeVersionNumeric"] as _IVariable).Initial = \u0019.\u0003.\u0001((long)((ulong)\u0019.\u0001(version2)));
				(isignature["CompilerVersionNumeric"] as _IVariable).Initial = \u0019.\u0003.\u0001((long)((ulong)\u0019.\u0001(version)));
				int vectorBlockSize = \u0002.VectorBlockSize;
				if (vectorBlockSize > 8)
				{
					ivariable9.Initial = \u0019.\u0003.\u0001((long)(vectorBlockSize / 8));
					ivariable10.Initial = \u0019.\u0003.\u0001((long)(vectorBlockSize / 4));
				}
				else
				{
					ivariable9.Initial = \u0019.\u0003.\u0001(1L);
					ivariable10.Initial = \u0019.\u0003.\u0001(1L);
				}
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			\u0019.\u0001(\u0002.DeviceId, \u0003, \u0002.DataManager.PackMode, boolValue, \u0002.SimulationMode, dictionary);
			foreach (KeyValuePair<string, string> keyValuePair in dictionary)
			{
				\u0002.Define(keyValuePair.Key, keyValuePair.Value);
			}
		}

		// Token: 0x060036D2 RID: 14034 RVA: 0x000DF26C File Offset: 0x000DD46C
		public static void \u0001(IDeviceIdentification \u0002, ICodegenerator \u0003, int \u0004, bool \u0005, bool \u0006, Dictionary<string, string> \u0007)
		{
			if (\u0007 == null)
			{
				throw new ArgumentNullException("defineTable");
			}
			if (\u0003 != null)
			{
				\u0019.\u0001(\u0003, \u0007);
			}
			\u0019.\u0001(APEnvironmentFacade.Instance.CompileOptions, \u0007);
			if (\u0002 != null)
			{
				\u0007["RuntimeDeviceType"] = \u0002.Type.ToString(CultureInfo.InvariantCulture);
			}
			if (\u0005)
			{
				\u0007["IsMulticoreSupported"] = string.Empty;
			}
			\u0007["PackMode"] = \u0004.ToString(CultureInfo.InvariantCulture);
			if (\u0006)
			{
				\u0007["IsSimulationMode"] = string.Empty;
			}
		}

		// Token: 0x060036D3 RID: 14035 RVA: 0x000DF308 File Offset: 0x000DD508
		private static void \u0001(ICodegenerator \u0002, Dictionary<string, string> \u0003)
		{
			if (!\u0002.MotorolaByteOrder)
			{
				\u0003["IsLittleEndian"] = string.Empty;
			}
			ICodegenerator3 codegenerator = \u0002 as ICodegenerator3;
			if (codegenerator != null)
			{
				\u0003["RegisterSize"] = (codegenerator.RegisterSize * 8).ToString(CultureInfo.InvariantCulture);
			}
			ICodegenerator6 codegenerator2 = \u0002 as ICodegenerator6;
			if (codegenerator2 != null && codegenerator2.FPUSupport)
			{
				\u0003["IsFPUSupported"] = string.Empty;
			}
			ICodegenerator7 codegenerator3 = \u0002 as ICodegenerator7;
			if (codegenerator3 != null && codegenerator3.SupportsTryCatch)
			{
				\u0003["IsTryCatchSupported"] = string.Empty;
			}
		}

		// Token: 0x060036D4 RID: 14036 RVA: 0x000DF39C File Offset: 0x000DD59C
		private static void \u0001(_ICompileOptions2 \u0002, Dictionary<string, string> \u0003)
		{
			if (\u0002.UTF8Encoding)
			{
				\u0003["IsUTF8EncodingSupported"] = string.Empty;
			}
		}
	}
}
