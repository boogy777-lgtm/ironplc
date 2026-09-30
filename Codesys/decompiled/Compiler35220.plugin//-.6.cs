using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using \u0014;
using \u0016;
using \u001D;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Features;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0082
{
	// Token: 0x0200016E RID: 366
	internal sealed class \u0006
	{
		// Token: 0x060018D2 RID: 6354 RVA: 0x0004D038 File Offset: 0x0004B238
		internal \u0006(\u001D.\u0004 \u000E\u0006)
		{
			this.\u0001 = \u000E\u0006;
			this.\u0001 = Assembly.GetAssembly(base.GetType());
			this.\u0001 = typeof(APEnvironmentFacade).Namespace;
		}

		// Token: 0x060018D3 RID: 6355 RVA: 0x0004D070 File Offset: 0x0004B270
		private bool \u0001(_ILanguageModelManagerConsolidated \u0002, Guid \u0003)
		{
			Guid deviceOfApplication = \u0002.ApplicationDeviceTable.GetDeviceOfApplication(\u0003);
			IDeviceIdentification targetIdOfDevice = \u0002.ApplicationDeviceTable.GetTargetIdOfDevice(deviceOfApplication);
			string text = targetIdOfDevice.Id.ToUpperInvariant();
			text = text.Replace(" ", "");
			if (string.CompareOrdinal(text, "10109000") >= 0 && string.CompareOrdinal(text, "1010900F") <= 0)
			{
				Version v = new Version(targetIdOfDevice.Version);
				Version v2 = new Version(3, 3, 0, 0);
				Version v3 = new Version(3, 5, 0, 0);
				if (v >= v2 && v < v3)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060018D4 RID: 6356 RVA: 0x0004D10C File Offset: 0x0004B30C
		internal void \u0001(_ILanguageModelManagerConsolidated \u0002, Guid \u0003, bool \u0004)
		{
			try
			{
				\u0002.SuppressChangedEvents = true;
				Guid guid = Guid.Empty;
				_IPreCompileContext ipreCompileContext = \u0002._GetPrecompileContext(\u0003);
				ITargetSettings targetSettings = null;
				if (ipreCompileContext != null)
				{
					targetSettings = ipreCompileContext.GetTargetSettings();
				}
				if (ipreCompileContext != null && ipreCompileContext.MinimalSystem)
				{
					this.\u0001(\u0002, \u0003, guid, "Library32External.xml", SignatureFlag.SuperGlobal | SignatureFlag.SimulationExternal);
				}
				else
				{
					guid = \u0082.\u0006.\u0002;
					_IPreCompileContext ipreCompileContext2 = \u0002._GetPrecompileContext(\u0003);
					if (ipreCompileContext2 != null)
					{
						ipreCompileContext2.Remove(\u0082.\u0006.\u0002);
						foreach (Guid objectGuid in \u0002.GetRelatedObjects(\u0082.\u0006.\u0002))
						{
							ipreCompileContext2.Remove(objectGuid);
						}
					}
					this.\u0001(\u0002, \u0003, guid, "Library32External.xml", SignatureFlag.SuperGlobal | SignatureFlag.TopLevel | SignatureFlag.SimulationExternal);
					if (\u0004)
					{
						this.\u0001(\u0002, \u0003, guid, "syslibsV35000.xml", SignatureFlag.SuperGlobal | SignatureFlag.TopLevel | SignatureFlag.SimulationExternal);
					}
					else
					{
						this.\u0001(\u0002, \u0003, guid, "syslibs_without_byte.xml", SignatureFlag.SuperGlobal | SignatureFlag.TopLevel | SignatureFlag.SimulationExternal);
						this.\u0001(\u0002, \u0003, guid, "sysmemset.xml", SignatureFlag.SuperGlobal | SignatureFlag.TopLevel | SignatureFlag.SimulationExternal);
					}
					if (ipreCompileContext != null)
					{
						bool u = false;
						if (targetSettings != null && this.\u0001(\u0002, \u0003))
						{
							u = true;
						}
						this.\u0001(\u0002, \u0003, guid, targetSettings, u);
						if (targetSettings != null && Helper.\u0001(targetSettings))
						{
							this.\u0001(\u0002, \u0003, guid, "syslibsOptV35000.xml", SignatureFlag.SuperGlobal | SignatureFlag.TopLevel | SignatureFlag.SimulationExternal);
						}
						Version runtimeVersion = \u0002.GetRuntimeVersion(\u0003);
						this.\u0001(\u0002, \u0003, guid, runtimeVersion);
						this.\u0001(\u0002, \u0003, \u0004, guid, runtimeVersion);
						this.\u0001(\u0002, \u0003, guid, targetSettings);
						this.\u0001(\u0002, \u0004);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.\u0001(false, ex.ToString());
			}
			finally
			{
				\u0002.SuppressChangedEvents = false;
			}
		}

		// Token: 0x060018D5 RID: 6357 RVA: 0x0004D2FC File Offset: 0x0004B4FC
		private void \u0001(_ILanguageModelManagerConsolidated \u0002, Guid \u0003, Guid \u0004, ITargetSettings \u0005, bool \u0006)
		{
			if (global::\u0016.\u0004.LintDataTypes.GetBoolValue(\u0005))
			{
				this.\u0001(\u0002, \u0003, \u0004, \u0006, "LibraryInt64External.xml", SignatureFlag.SuperGlobal | SignatureFlag.TopLevel | SignatureFlag.SimulationExternal);
			}
			if (global::\u0016.\u0004.LRealDataType.GetBoolValue(\u0005))
			{
				this.\u0001(\u0002, \u0003, \u0004, \u0006, "LibraryReal64External.xml", SignatureFlag.SuperGlobal | SignatureFlag.TopLevel | SignatureFlag.SimulationExternal);
			}
			if (global::\u0016.\u0004.LRealDataType.GetBoolValue(\u0005) && global::\u0016.\u0004.LintDataTypes.GetBoolValue(\u0005))
			{
				this.\u0001(\u0002, \u0003, \u0004, \u0006, "Library64BitConversions.xml", SignatureFlag.SuperGlobal | SignatureFlag.TopLevel | SignatureFlag.SimulationExternal);
			}
		}

		// Token: 0x060018D6 RID: 6358 RVA: 0x0004D38C File Offset: 0x0004B58C
		private void \u0001(_ILanguageModelManagerConsolidated \u0002, bool \u0003)
		{
			if (\u0003)
			{
				this.\u0001(\u0002, \u0082.\u0006.\u0001, Guid.Empty, "LibrarySystemTypes3570.xml", SignatureFlag.None);
			}
		}

		// Token: 0x060018D7 RID: 6359 RVA: 0x0004D3AC File Offset: 0x0004B5AC
		private void \u0001(_ILanguageModelManagerConsolidated \u0002, Guid \u0003, Guid \u0004, ITargetSettings \u0005)
		{
			if (\u0005 != null && global::\u0016.\u0004.ExternalRealStringConversions.GetBoolValue(\u0005))
			{
				this.\u0001(\u0002, \u0003, \u0004, "syslibs_external_string_real_conversion.xml", SignatureFlag.SuperGlobal | SignatureFlag.TopLevel | SignatureFlag.SimulationExternal);
			}
		}

		// Token: 0x060018D8 RID: 6360 RVA: 0x0004D3D8 File Offset: 0x0004B5D8
		private void \u0001(_ILanguageModelManagerConsolidated \u0002, Guid \u0003, bool \u0004, Guid \u0005, Version \u0006)
		{
			if (\u0004 && \u0006 != null && \u0006 >= new Version(3, 5, 4, 0))
			{
				this.\u0001(\u0002, \u0003, \u0005, "sysmemcopy.xml", SignatureFlag.SuperGlobal | SignatureFlag.TopLevel | SignatureFlag.SimulationExternal);
				this.\u0001(\u0002, \u0003, \u0005, "sysstringcompare.xml", SignatureFlag.SuperGlobal | SignatureFlag.TopLevel | SignatureFlag.SimulationExternal);
			}
		}

		// Token: 0x060018D9 RID: 6361 RVA: 0x0004D434 File Offset: 0x0004B634
		private void \u0001(_ILanguageModelManagerConsolidated \u0002, Guid \u0003, Guid \u0004, Version \u0005)
		{
			if (\u0005 != null && \u0005 >= new Version(3, 5, 14, 0))
			{
				this.\u0001(\u0002, \u0003, \u0004, "AtomicOperators.xml", SignatureFlag.SuperGlobal | SignatureFlag.TopLevel | SignatureFlag.SimulationExternal);
			}
		}

		// Token: 0x060018DA RID: 6362 RVA: 0x0004D46C File Offset: 0x0004B66C
		private void \u0001(_ILanguageModelManagerConsolidated \u0002, Guid \u0003, Guid \u0004, bool \u0005, string \u0006, SignatureFlag \u0007)
		{
			StreamReader streamReader = new StreamReader(this.\u0001.GetManifestResourceStream(this.\u0001 + ".Resources.SystemLibrary." + \u0006));
			string u = streamReader.ReadToEnd();
			if (\u0005)
			{
				this.\u0001.\u0001(\u0002, u, \u0003, \u0004, \u0007);
			}
			else
			{
				this.\u0001(\u0002, u, \u0003, \u0004, \u0007);
			}
			streamReader.Close();
		}

		// Token: 0x060018DB RID: 6363 RVA: 0x0004D4CC File Offset: 0x0004B6CC
		private void \u0001(_ILanguageModelManagerConsolidated \u0002, string \u0003, SignatureFlag \u0004)
		{
			this.\u0001(\u0002, "Resources.SystemLibrary", \u0003, \u0004);
		}

		// Token: 0x060018DC RID: 6364 RVA: 0x0004D4DC File Offset: 0x0004B6DC
		private void \u0001(_ILanguageModelManagerConsolidated \u0002, Guid \u0003, string \u0004, SignatureFlag \u0005)
		{
			this.\u0001(\u0002, \u0003, Guid.Empty, "Resources.SystemLibrary", \u0004, \u0005);
		}

		// Token: 0x060018DD RID: 6365 RVA: 0x0004D4F4 File Offset: 0x0004B6F4
		private void \u0001(_ILanguageModelManagerConsolidated \u0002, Guid \u0003, Guid \u0004, string \u0005, SignatureFlag \u0006)
		{
			this.\u0001(\u0002, \u0003, \u0004, "Resources.SystemLibrary", \u0005, \u0006);
		}

		// Token: 0x060018DE RID: 6366 RVA: 0x0004D508 File Offset: 0x0004B708
		private void \u0001(_ILanguageModelManagerConsolidated \u0002, string \u0003, string \u0004, SignatureFlag \u0005)
		{
			this.\u0001(\u0002, Guid.Empty, Guid.Empty, \u0003, \u0004, \u0005);
		}

		// Token: 0x060018DF RID: 6367 RVA: 0x0004D520 File Offset: 0x0004B720
		private void \u0001(_ILanguageModelManagerConsolidated \u0002, Guid \u0003, Guid \u0004, string \u0005, string \u0006, SignatureFlag \u0007)
		{
			this.\u0001(\u0002, \u0003, \u0004, this.\u0001, \u0005, \u0006, \u0007);
		}

		// Token: 0x060018E0 RID: 6368 RVA: 0x0004D538 File Offset: 0x0004B738
		private void \u0001(_ILanguageModelManagerConsolidated \u0002, Guid \u0003, Guid \u0004, string \u0005, string \u0006, string \u0007, SignatureFlag \u0008)
		{
			using (Stream manifestResourceStream = this.\u0001.GetManifestResourceStream(string.Concat(new string[]
			{
				\u0005,
				".",
				\u0006,
				".",
				\u0007
			})))
			{
				using (StreamReader streamReader = new StreamReader(manifestResourceStream))
				{
					string u = streamReader.ReadToEnd();
					this.\u0001.\u0001(\u0002, u, \u0003, \u0004, \u0008);
				}
			}
		}

		// Token: 0x060018E1 RID: 6369 RVA: 0x0004D5CC File Offset: 0x0004B7CC
		internal void \u0001(_ILanguageModelManagerConsolidated \u0002)
		{
			try
			{
				\u0002.SuppressChangedEvents = true;
				this.\u0001(\u0002, "Library32.xml", SignatureFlag.SuperGlobal);
				this.\u0001(\u0002, "TypeClassEnumIntern_V351600.xml", SignatureFlag.SuperGlobal);
				this.\u0001(\u0002, \u0082.\u0006.\u0001, "TypeClassEnum_V351600.xml", SignatureFlag.SuperGlobal);
				this.\u0001(\u0002, "LibraryInt64.xml", SignatureFlag.SuperGlobal);
				this.\u0001(\u0002, "LibraryDateTime64.xml", SignatureFlag.SuperGlobal);
				this.\u0001(\u0002, "LibraryReal64.xml", SignatureFlag.SuperGlobal);
				if (APEnvironmentFacade.Instance.IsPrimaryProjectALibrary())
				{
					global::\u0014.\u0007.\u0001(\u0002);
				}
				this.\u0002(\u0002);
				this.\u0001(\u0002, \u0082.\u0006.\u0001, "LibrarySystemTypes.xml", SignatureFlag.None);
				this.\u0001(\u0002, \u0082.\u0006.\u0001, "LibrarySystemTypes3410.xml", SignatureFlag.None);
				this.\u0001(\u0002, \u0082.\u0006.\u0001, "LibrarySystemTypes3500.xml", SignatureFlag.None);
				this.\u0001(\u0002, \u0082.\u0006.\u0001, "LibrarySystemTypes3510.xml", SignatureFlag.None);
				this.\u0001(\u0002, \u0082.\u0006.\u0001, "LibrarySystemTypes35110.xml", SignatureFlag.None);
				this.\u0001(\u0002, \u0082.\u0006.\u0001, "LibrarySystemTypes3550.xml", SignatureFlag.None);
				this.\u0001(\u0002, \u0082.\u0006.\u0001, "LibrarySystemTypes3580.xml", SignatureFlag.None);
				this.\u0001(\u0002, \u0082.\u0006.\u0001, "LibrarySystemTypes3590.xml", SignatureFlag.SimulationExternal);
				this.\u0001(\u0002, \u0082.\u0006.\u0001, "LibrarySystemTypes351300.xml", SignatureFlag.None);
				this.\u0001(\u0002, \u0082.\u0006.\u0001, "LibrarySystemTypes351400.xml", SignatureFlag.None);
				this.\u0001(\u0002, \u0082.\u0006.\u0001, "LibrarySystemTypes351600.xml", SignatureFlag.None);
				this.\u0001(\u0002, \u0082.\u0006.\u0001, "LibrarySystemTypes352000.xml", SignatureFlag.None);
				this.\u0001(\u0002, \u0082.\u0006.\u0001, "SystemNamespaceForced.xml", SignatureFlag.SystemNamespaceForced);
				string @namespace = typeof(ReflectionAssignmentCoder).Namespace;
				this.\u0001(\u0002, \u0082.\u0006.\u0001, Guid.Empty, @namespace, "InstancePathAttribute", "ImplicitStringAppend.xml", SignatureFlag.None);
				this.\u0001(\u0002, "LDateConversions.xml", SignatureFlag.SuperGlobal);
			}
			catch (Exception ex)
			{
				Debug.\u0001(false, ex.ToString());
			}
			finally
			{
				\u0002.SuppressChangedEvents = false;
			}
		}

		// Token: 0x060018E2 RID: 6370 RVA: 0x0004D7FC File Offset: 0x0004B9FC
		private void \u0002(_ILanguageModelManagerConsolidated \u0002)
		{
			this.\u0001(\u0002, "LibraryWString.xml", SignatureFlag.SuperGlobal);
			if (APEnvironmentFacade.Instance.CompileOptions.UTF8Encoding)
			{
				this.\u0001(\u0002, "LibraryStringToWStringConversionsUTF8.xml", SignatureFlag.SuperGlobal);
				return;
			}
			this.\u0001(\u0002, "LibraryStringToWStringConversions.xml", SignatureFlag.SuperGlobal);
		}

		// Token: 0x060018E3 RID: 6371 RVA: 0x0004D854 File Offset: 0x0004BA54
		private void \u0001(_ILanguageModelManagerConsolidated \u0002, string \u0003, Guid \u0004, Guid \u0005, SignatureFlag \u0006)
		{
			bool flag = false;
			bool u = false;
			bool flag2 = false;
			string empty = string.Empty;
			KeyValuePair<string, string>[] u2 = new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>(CompileAttributes.ATTRIBUTE_PACK_MODE, "8")
			};
			foreach (ILanguageModel u3 in this.\u0001.\u0001(\u0003, flag, empty, \u0004, \u0005, string.Empty, flag2, \u0006, null, u2))
			{
				this.\u0001.\u0001(\u0002, u3, flag, empty, flag2, \u0006, u);
			}
		}

		// Token: 0x04000461 RID: 1121
		private readonly \u001D.\u0004 \u0001;

		// Token: 0x04000462 RID: 1122
		private readonly Assembly \u0001;

		// Token: 0x04000463 RID: 1123
		private readonly string \u0001;

		// Token: 0x04000464 RID: 1124
		private static Guid \u0001 = new Guid("{e0c003b2-1edd-477a-9148-e4b7c6a4e203}");

		// Token: 0x04000465 RID: 1125
		private static Guid \u0002 = new Guid("{A8A49D95-0FF3-4ade-9D7E-B68780BA19E1}");
	}
}
