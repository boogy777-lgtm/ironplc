using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using \u0003;
using \u0004;
using \u0005;
using \u0007;
using \u0008;
using \u000E;
using \u0011;
using \u0012;
using \u0013;
using \u0014;
using \u0016;
using \u0017;
using \u0018;
using \u0019;
using \u001B;
using \u001C;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0082;
using \u0084;

namespace \u001D
{
	// Token: 0x0200017C RID: 380
	internal sealed class \u0004 : ILanguageModelHandling2, ILanguageModelHandling
	{
		// Token: 0x060019CD RID: 6605 RVA: 0x00050678 File Offset: 0x0004E878
		internal \u0004()
		{
			this.\u0001 = new \u0082.\u0006(this);
		}

		// Token: 0x060019CE RID: 6606 RVA: 0x0005068C File Offset: 0x0004E88C
		public void \u0001(_ILanguageModelManagerConsolidated \u0002)
		{
			this.\u0001.\u0001(\u0002);
		}

		// Token: 0x060019CF RID: 6607 RVA: 0x0005069C File Offset: 0x0004E89C
		public void \u0001(_ILanguageModelManagerConsolidated \u0002, string \u0003, bool \u0004, string \u0005, Guid \u0006, Guid \u0007, string \u0008, bool \u000E, bool \u000F, SignatureFlag \u0010, IList<IList<string>> \u0011)
		{
			this.\u0002(\u0002, \u0003, \u0004, \u0005, \u0006, \u0007, \u0008, \u000E, \u000F, \u0010, \u0011);
		}

		// Token: 0x060019D0 RID: 6608 RVA: 0x000506C4 File Offset: 0x0004E8C4
		public void \u0001(_ILanguageModelManagerConsolidated \u0002, ILanguageModel \u0003, bool \u0004, string \u0005, bool \u0006, SignatureFlag \u0007, bool \u0008)
		{
			this.\u0002(\u0002, \u0003, \u0004, \u0005, \u0006, \u0007, \u0008);
		}

		// Token: 0x060019D1 RID: 6609 RVA: 0x000506D8 File Offset: 0x0004E8D8
		internal static void \u0001(_ISignature \u0002)
		{
			Debug.\u0001(\u0002 != null);
			if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_SIGNATURE_FLAG))
			{
				string attributeValue = \u0002.GetAttributeValue(CompileAttributes.ATTRIBUTE_SIGNATURE_FLAG);
				try
				{
					long sf = long.Parse(attributeValue);
					\u0002.SetFlag((SignatureFlag)sf, true);
				}
				catch
				{
				}
			}
			if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_LINK_ALWAYS))
			{
				\u0002.SetFlag(SignatureFlag.TopLevel, true);
			}
			if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_IMPLICIT_REFERENCE_TYPE))
			{
				\u0002.SetFlag(SignatureFlag.InhibitOnlineChange, true);
			}
			if (\u0002.POUType == Operator.VarGlobal && \u0002.HasAttribute(CompileAttributes.ATTRIBUTE_TASKLOCALGVL))
			{
				\u0002.AddAttribute("inhibit-online-change", null);
				\u0002.AddAttribute("subsequent", null);
			}
			if (\u0002.HasAttribute("external"))
			{
				\u0002.SetFlag(SignatureFlag.External, true);
			}
		}

		// Token: 0x060019D2 RID: 6610 RVA: 0x000507A8 File Offset: 0x0004E9A8
		internal static void \u0001(_ISignature \u0002, _ICompiledPOU \u0003)
		{
			\u001D.\u0004.\u0001(\u0002);
			if (\u0003 == null)
			{
				return;
			}
			if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_POU_FLAG) && \u0003 != null)
			{
				string attributeValue = \u0002.GetAttributeValue(CompileAttributes.ATTRIBUTE_POU_FLAG);
				try
				{
					CompiledPOUFlags compiledPOUFlags = (CompiledPOUFlags)long.Parse(attributeValue);
					\u0003.SetFlag(compiledPOUFlags, true);
					if ((compiledPOUFlags & CompiledPOUFlags.TopLevel) != (CompiledPOUFlags)0)
					{
						\u0003.UpdateChecksum();
					}
				}
				catch
				{
				}
			}
			if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_ALLOW_DISASSEMBLY))
			{
				global::\u001B.\u0004.\u0001(StatementFlag.Library, false, \u0003);
				\u0003.SetFlagInternal(InternalCompiledPOUFlags.LibraryAccessPermitted, true);
			}
			if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_LINK_ALWAYS))
			{
				\u0003.SetFlag(CompiledPOUFlags.TopLevel, true);
				\u0003.UpdateChecksum();
			}
		}

		// Token: 0x060019D3 RID: 6611 RVA: 0x00050844 File Offset: 0x0004EA44
		public void \u0001(_ILanguageModelManagerConsolidated \u0002, Guid \u0003, bool \u0004)
		{
			this.\u0001.\u0001(\u0002, \u0003, \u0004);
		}

		// Token: 0x060019D4 RID: 6612 RVA: 0x00050854 File Offset: 0x0004EA54
		private static void \u0001(_ILanguageModelManagerConsolidated \u0002, Guid \u0003)
		{
			try
			{
				_IPreCompileContext ipreCompileContext = \u0002._GetPrecompileContext(\u0003);
				foreach (Guid objectGuid in \u0002.GetRelatedObjects(\u001D.\u0004.\u0001))
				{
					ipreCompileContext.Remove(objectGuid);
				}
			}
			catch (Exception ex)
			{
				Debug.\u0001(false, ex.ToString());
			}
		}

		// Token: 0x060019D5 RID: 6613 RVA: 0x000508CC File Offset: 0x0004EACC
		private void \u0001(_ILanguageModelManagerConsolidated \u0002, Guid \u0003, int \u0004)
		{
			try
			{
				Guid deviceOfApplication = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceOfApplication(\u0003);
				string u = global::\u0016.\u0008.\u0001(\u0003, deviceOfApplication, \u0004);
				this.\u0001(\u0002, u, \u0003, \u001D.\u0004.\u0001, SignatureFlag.InhibitOnlineChange | SignatureFlag.SuperGlobal | SignatureFlag.TopLevel);
			}
			catch (Exception ex)
			{
				Debug.\u0001(false, ex.ToString());
			}
		}

		// Token: 0x060019D6 RID: 6614 RVA: 0x00050930 File Offset: 0x0004EB30
		public void \u0001(_IPreCompileContext \u0002, int \u0003, ICodegenerator3 \u0004)
		{
			StreamReader streamReader = new StreamReader(Assembly.GetAssembly(APEnvironmentFacade.Instance.LanguageModelMgr.GetType()).GetManifestResourceStream("_3S.CoDeSys.LanguageModelManager.Resources.OperatorCurrentTaskInfo.xml"));
			string text = streamReader.ReadToEnd();
			string text2 = "__TaskSpecificInfoGVL.taskEntryAddress[index]";
			string text3 = "pbyStack";
			if (\u0004 != null && \u0004.GetProperty(CodegeneratorProperties.PositiveStackGrow))
			{
				text2 = "pbyStack";
				text3 = "__TaskSpecificInfoGVL.taskEntryAddress[index]";
			}
			text = string.Format(text, new object[]
			{
				\u0002.TaskList.Count,
				\u0003,
				text2,
				text3
			});
			this.\u0001(APEnvironmentFacade.Instance.LanguageModelMgr, text, \u0002.ApplicationGuid, Guid.Empty, SignatureFlag.SuperGlobal);
			streamReader.Close();
		}

		// Token: 0x060019D7 RID: 6615 RVA: 0x000509E8 File Offset: 0x0004EBE8
		internal void \u0001(_ILanguageModelManagerConsolidated \u0002, string \u0003, Guid \u0004, Guid \u0005, SignatureFlag \u0006)
		{
			bool u = false;
			bool u000E = false;
			bool u000F = false;
			string empty = string.Empty;
			this.\u0002(\u0002, \u0003, u, empty, \u0004, \u0005, string.Empty, u000E, u000F, \u0006, null);
		}

		// Token: 0x060019D8 RID: 6616 RVA: 0x00050A18 File Offset: 0x0004EC18
		internal void \u0002(_ILanguageModelManagerConsolidated \u0002, string \u0003, bool \u0004, string \u0005, Guid \u0006, Guid \u0007, string \u0008, bool \u000E, bool \u000F, SignatureFlag \u0010, IList<IList<string>> \u0011)
		{
			this.\u0003(\u0002, \u0003, \u0004, \u0005, \u0006, \u0007, \u0008, \u000E, \u000F, \u0010, \u0011);
		}

		// Token: 0x060019D9 RID: 6617 RVA: 0x00050A40 File Offset: 0x0004EC40
		public void \u0003(_ILanguageModelManagerConsolidated \u0002, string \u0003, bool \u0004, string \u0005, Guid \u0006, Guid \u0007, string \u0008, bool \u000E, bool \u000F, SignatureFlag \u0010, IList<IList<string>> \u0011)
		{
			foreach (ILanguageModel u in this.\u0001(\u0003, \u0004, \u0005, \u0006, \u0007, \u0008, \u000E, \u0010, \u0011, null))
			{
				this.\u0001(\u0002, u, \u0004, \u0005, \u000E, \u0010, \u000F);
			}
		}

		// Token: 0x060019DA RID: 6618 RVA: 0x00050AA8 File Offset: 0x0004ECA8
		public void \u0002(_ILanguageModelManagerConsolidated \u0002, ILanguageModel \u0003, bool \u0004, string \u0005, bool \u0006, SignatureFlag \u0007, bool \u0008)
		{
			Guid applicationGuid = \u0003.ApplicationGuid;
			Guid deviceGuid = \u0003.DeviceGuid;
			Guid languageModelObject = \u0003.LanguageModelObject;
			string libraryId = \u0003.LibraryId;
			bool flag = false;
			_IPreCompileContext ipreCompileContext;
			if (libraryId != string.Empty)
			{
				ipreCompileContext = \u001D.\u0004.\u0001(\u0002, applicationGuid, libraryId, out flag);
			}
			else if (applicationGuid != Guid.Empty)
			{
				ipreCompileContext = \u001D.\u0004.\u0001(\u0002, applicationGuid);
			}
			else
			{
				ipreCompileContext = \u001D.\u0004.\u0001(\u0002, ref \u0007, deviceGuid);
			}
			if (flag)
			{
				return;
			}
			this.\u0001(\u0002, \u0003.LMDevice, deviceGuid);
			this.\u0001(\u0002, \u0003.LMApplication, deviceGuid, applicationGuid, ipreCompileContext, \u0005);
			if (\u0003.LMApplication != null)
			{
				ipreCompileContext.UpdatePointerSize();
			}
			\u001D.\u0004.\u0001(\u0003, ipreCompileContext);
			bool flag2;
			\u001D.\u0004.\u0001(\u0003.LMTaskList, out flag2, languageModelObject, ipreCompileContext);
			\u001D.\u0004.\u0001(\u0002, \u0003, ipreCompileContext, applicationGuid, languageModelObject);
			\u001D.\u0004.\u0001(\u0002, \u0003);
			foreach (ILMPOU ilmpou in \u0003.Pous)
			{
				if (\u0004)
				{
					ilmpou.External = true;
				}
				if (\u0006)
				{
					ilmpou.EnableSystemCall = true;
				}
				_ILMEntity ilmentity = (_ILMEntity)ilmpou;
				ilmentity.CompilerDefines = \u0005;
				ilmentity.DefaultFlag = \u0007;
				this.\u0001(\u0002, ilmpou, languageModelObject, ipreCompileContext, libraryId);
			}
			foreach (ILMGlobVarlist ilmglobVarlist in \u0003.GlobalVariableLists)
			{
				_ILMEntity ilmentity2 = (_ILMEntity)ilmglobVarlist;
				ilmentity2.CompilerDefines = \u0005;
				ilmentity2.DefaultFlag = \u0007;
				this.\u0001(\u0002, ilmglobVarlist, languageModelObject, ipreCompileContext, libraryId);
			}
			foreach (ILMDataType ilmdataType in \u0003.DataTypes)
			{
				_ILMEntity ilmentity3 = (_ILMEntity)ilmdataType;
				ilmentity3.CompilerDefines = \u0005;
				ilmentity3.DefaultFlag = \u0007;
				if (\u0004)
				{
					ilmentity3.DefaultFlag = (\u0007 | SignatureFlag.External);
				}
				this.\u0001(\u0002, ilmdataType, languageModelObject, ipreCompileContext, libraryId);
			}
			if (flag2)
			{
				\u0002.OnTaskConfigChanged(applicationGuid);
			}
		}

		// Token: 0x060019DB RID: 6619 RVA: 0x00050C78 File Offset: 0x0004EE78
		private static _IPreCompileContext \u0001(_ILanguageModelManagerConsolidated \u0002, Guid \u0003, string \u0004, out bool \u0005)
		{
			if (\u0003 != Guid.Empty)
			{
				\u0005 = true;
				return null;
			}
			\u0005 = false;
			return \u0002.LibList.GetLibraryContext(\u0004, true);
		}

		// Token: 0x060019DC RID: 6620 RVA: 0x00050C9C File Offset: 0x0004EE9C
		private static _IPreCompileContext \u0001(_ILanguageModelManagerConsolidated \u0002, ref SignatureFlag \u0003, Guid \u0004)
		{
			_IPreCompileContext pool = \u0002.Pool;
			if (\u0004 == Guid.Empty)
			{
				\u0003 |= SignatureFlag.PoolSignature;
			}
			return pool;
		}

		// Token: 0x060019DD RID: 6621 RVA: 0x00050CC0 File Offset: 0x0004EEC0
		private static _IPreCompileContext \u0001(_ILanguageModelManagerConsolidated \u0002, Guid \u0003)
		{
			_IPreCompileContext ipreCompileContext = \u0002._GetPrecompileContext(\u0003);
			if (ipreCompileContext == null)
			{
				ipreCompileContext = global::\u0019.\u0003.\u0001(string.Empty, \u0003, KindOfContext.Target);
				\u0002._SetPrecompileContext(ipreCompileContext);
				ITargetSettings targetSettings = ipreCompileContext.GetTargetSettings();
				string stringValue = global::\u0016.\u0004.CompilerDefines.GetStringValue(targetSettings);
				ipreCompileContext.AddDefines(stringValue, true);
			}
			return ipreCompileContext;
		}

		// Token: 0x060019DE RID: 6622 RVA: 0x00050D08 File Offset: 0x0004EF08
		private static void \u0001(_ILanguageModelManagerConsolidated \u0002, ILanguageModel \u0003)
		{
			foreach (ILMPOU ilmpou in \u0003.Pous)
			{
				\u0002.RemovePreCompCrossReferences(ilmpou.POUGuid);
			}
			foreach (ILMGlobVarlist ilmglobVarlist in \u0003.GlobalVariableLists)
			{
				\u0002.RemovePreCompCrossReferences(ilmglobVarlist.GVLGuid);
			}
			foreach (ILMDataType ilmdataType in \u0003.DataTypes)
			{
				\u0002.RemovePreCompCrossReferences(ilmdataType.DUTGuid);
			}
		}

		// Token: 0x060019DF RID: 6623 RVA: 0x00050D8C File Offset: 0x0004EF8C
		private static void \u0001(ILanguageModel \u0002, _IPreCompileContext \u0003)
		{
			ILanguageModel2 languageModel = \u0002 as ILanguageModel2;
			if (languageModel != null && languageModel.StaticMemorySegments != null)
			{
				\u0003.AddStaticMemorySegments(languageModel.StaticMemorySegments);
			}
		}

		// Token: 0x060019E0 RID: 6624 RVA: 0x00050DB8 File Offset: 0x0004EFB8
		private static void \u0001(_ILanguageModelManagerConsolidated \u0002, ILanguageModel \u0003, _IPreCompileContext \u0004, Guid \u0005, Guid \u0006)
		{
			ILanguageModel3 languageModel = \u0003 as ILanguageModel3;
			if (((languageModel != null) ? languageModel.LMLibraryList2 : null) != null)
			{
				\u0002.LibList.RemoveLibraryReferences(\u0006);
				\u0002.RemoveLibListForApp(\u0005);
				\u0004.GetParameterTableTable().Clear();
				foreach (ILMLibraryList2 ilmlibraryList in languageModel.LMLibraryList2)
				{
					_IPreCompileContext u = string.IsNullOrEmpty(ilmlibraryList.LibraryId) ? \u0004 : \u0002.LibList.GetLibraryContext(ilmlibraryList.LibraryId, true);
					\u001D.\u0004.\u0001(ilmlibraryList as _ILMLibraryList, \u0005, u, \u0004);
				}
				\u0004.ClearStaticLibraryTables();
				foreach (_IPreCompileContext ipreCompileContext in APEnvironmentFacade.Instance.LanguageModelMgr.AllPreCompileContexts(true, false).OfType<_IPreCompileContext>())
				{
					ipreCompileContext.Dirty = true;
				}
			}
		}

		// Token: 0x060019E1 RID: 6625 RVA: 0x00050EBC File Offset: 0x0004F0BC
		private void \u0001(_ILanguageModelManagerConsolidated \u0002, ILMDevice \u0003, Guid \u0004)
		{
			if (\u0003 == null)
			{
				return;
			}
			if (!string.IsNullOrEmpty(\u0003.Name))
			{
				\u0002.ApplicationDeviceTable.SetDeviceName(\u0004, \u0003.Name);
			}
			if (\u0004 != Guid.Empty && \u0003.DeviceIdentification != null)
			{
				\u0002.ApplicationDeviceTable.SetTargetIdOfDevice(\u0004, \u0003.DeviceIdentification);
			}
			foreach (Guid guidApplication in \u0002.ApplicationDeviceTable.GetApplicationsOfDevice(\u0004))
			{
				_IPreCompileContext ipreCompileContext = \u0002._GetPrecompileContext(guidApplication);
				if (ipreCompileContext != null)
				{
					ipreCompileContext.ResetTargetSettings();
					ipreCompileContext.TargetDefineTable.Clear();
					ITargetSettings targetSettings = ipreCompileContext.GetTargetSettings();
					string stringValue = global::\u0016.\u0004.CompilerDefines.GetStringValue(targetSettings);
					ipreCompileContext.AddDefines(stringValue, true);
				}
			}
		}

		// Token: 0x060019E2 RID: 6626 RVA: 0x00050F74 File Offset: 0x0004F174
		private void \u0001(_ILanguageModelManagerConsolidated \u0002, Guid \u0003, _IPreCompileContext \u0004, IEnumerable<string> \u0005)
		{
			if (\u0005.Any<string>())
			{
				_ILanguageModelBuilder ilanguageModelBuilder = APEnvironmentFacade.Instance.LanguageModelMgr.CreateLanguageModelBuilder() as _ILanguageModelBuilder;
				ILMGlobVarlist ilmglobVarlist = ilanguageModelBuilder.CreateGlobVarlist("_ApplicationErrorsGVL", \u001D.\u0004.\u0002);
				IPragmaStatement state = ilanguageModelBuilder.CreateMessageGuidPragmaStatement(\u0003);
				ilmglobVarlist.Interface = ilanguageModelBuilder.CreateSequenceStatement();
				ilmglobVarlist.Interface.AddStatement(state);
				ilmglobVarlist.Interface.AddStatement(ilanguageModelBuilder.CreateVariableDeclarationListStatement(null, VarFlag.Global, ilanguageModelBuilder.CreateSequenceStatement()));
				foreach (string stError in \u0005)
				{
					ilmglobVarlist.Interface.AddError(stError);
				}
				_ILMEntity ilmentity = (_ILMEntity)ilmglobVarlist;
				ilmentity.CompilerDefines = string.Empty;
				ilmentity.DefaultFlag = SignatureFlag.Global;
				this.\u0001(\u0002, ilmglobVarlist, \u001D.\u0004.\u0002, \u0004, string.Empty);
			}
		}

		// Token: 0x060019E3 RID: 6627 RVA: 0x00051060 File Offset: 0x0004F260
		private void \u0001(_ILanguageModelManagerConsolidated \u0002, ILMApplication \u0003, Guid \u0004, Guid \u0005, _IPreCompileContext \u0006, string \u0007)
		{
			if (\u0003 == null)
			{
				return;
			}
			if (\u0004 != Guid.Empty && \u0003.DeviceIdentification != null)
			{
				\u0002.ApplicationDeviceTable.SetTargetIdOfDevice(\u0004, \u0003.DeviceIdentification);
			}
			string stName = \u0003.DeviceName;
			\u0002.ApplicationDeviceTable.SetDeviceName(\u0004, \u0003.DeviceName);
			if (!string.IsNullOrEmpty(\u0003.ApplicationName))
			{
				stName = \u0003.ApplicationName;
			}
			\u0002.ApplicationDeviceTable.SetApplicationName(\u0005, stName, false);
			if (!string.IsNullOrEmpty(\u0003.SimulationApplicationName))
			{
				\u0002.ApplicationDeviceTable.SetApplicationName(\u0005, \u0003.SimulationApplicationName, true);
			}
			else
			{
				\u0002.ApplicationDeviceTable.SetApplicationName(\u0005, stName, true);
			}
			\u0002.ApplicationDeviceTable.SetMemorySettingsProvider(\u0005, \u0003.MemorySettingsProvider);
			LList<string> u = new LList<string>();
			ILMApplication2 ilmapplication = \u0003 as ILMApplication2;
			ILMApplication3 ilmapplication2 = \u0003 as ILMApplication3;
			ILMApplication4 ilmapplication3 = \u0003 as ILMApplication4;
			if (\u0006 != null)
			{
				if (ilmapplication != null)
				{
					\u0006.TargetInputSize = ilmapplication.TargetInputSize;
					\u0006.TargetOutputSize = ilmapplication.TargetOutputSize;
					\u0006.TargetMemorySize = ilmapplication.TargetMemorySize;
				}
				if (ilmapplication2 != null)
				{
					\u0006.GenerateContent = ilmapplication2.GenerateContent;
				}
				if (ilmapplication3 != null)
				{
					\u0006.TargetStaticSize = ilmapplication3.TargetStaticSize;
				}
			}
			if (\u0005 != Guid.Empty)
			{
				\u0002.ApplicationDeviceTable.RemoveByGuid(\u0005);
				\u0002.ApplicationDeviceTable.AddApplicationDevice(\u0005, \u0004);
				if (\u0003.ParentApp != Guid.Empty)
				{
					\u0002.ApplicationDeviceTable.SetParentApplication(\u0003.ParentApp, \u0005);
				}
				this.\u0001(\u0002, \u0005, \u0006.HasByteSupport());
				if (\u0002.GetReferenceContext(\u0005) == null && APEnvironmentFacade.Instance.LanguageModelMgr.EnableBackgroundLoading)
				{
					APEnvironmentFacade.Instance.LanguageModelMgr.LoadReferenceContextInBackground(\u0005);
				}
			}
			\u0006.TargetDefineTable.Clear();
			ITargetSettings targetSettings = \u0006.GetTargetSettings();
			string stringValue = global::\u0016.\u0004.CompilerDefines.GetStringValue(targetSettings);
			\u0006.AddDefines(stringValue, true);
			\u0006.DefineTable.Clear();
			\u0006.AddDefines(\u0007, false);
			\u001D.\u0004.\u0001(\u0002, \u0005);
			\u0006.SupportDynamicMemory = (\u0003.DynamicMemorySize > 0);
			if (\u0003.DynamicMemorySize > 0)
			{
				this.\u0001(\u0002, \u0005, \u0003.DynamicMemorySize);
			}
			this.\u0001(\u0002, \u0005, \u0006, u);
		}

		// Token: 0x060019E4 RID: 6628 RVA: 0x00051290 File Offset: 0x0004F490
		private static void \u0001(ILMTaskList \u0002, out bool \u0003, Guid \u0004, _IPreCompileContext \u0005)
		{
			\u0003 = (\u0002 != null);
			if (\u0002 == null)
			{
				return;
			}
			APEnvironmentFacade.Instance.LanguageModelMgr.AddRelatedObject(\u0004, \u0002.TaskConfigGuid, \u0002.TaskConfigGuid);
			\u0005.TaskList.RemoveTaskInfo(\u0002.TaskConfigGuid);
			foreach (ITaskInfo taskInfo in \u0002.Tasks)
			{
				APEnvironmentFacade.Instance.LanguageModelMgr.AddRelatedObject(\u0004, taskInfo.ObjectGuid, taskInfo.TaskGuid);
				\u0005.TaskList.AddTaskInfo(\u0002.TaskConfigGuid, taskInfo.TaskGuid, taskInfo.TaskName);
			}
		}

		// Token: 0x060019E5 RID: 6629 RVA: 0x00051328 File Offset: 0x0004F528
		private static void \u0001(_ILMLibraryList \u0002, Guid \u0003, _IPreCompileContext \u0004, _IPreCompileContext \u0005)
		{
			if (\u0002 == null)
			{
				return;
			}
			ICaseInsensitiveDictionary<ICaseInsensitiveDictionary<IExpression>> parameterTableTable = \u0005.GetParameterTableTable();
			_ILanguageModelManagerConsolidated languageModelMgr = APEnvironmentFacade.Instance.LanguageModelMgr;
			languageModelMgr.AddLibListForApp(\u0003, \u0004, \u0002);
			foreach (ILMLibraryInfo ilmlibraryInfo in \u0002.AllLibraries)
			{
				_IPreCompileContext libraryContext = languageModelMgr.LibList.GetLibraryContext(ilmlibraryInfo.Identification, true);
				libraryContext.Namespace = ilmlibraryInfo.DefaultNamespace;
				libraryContext.LinkInSimulation = ilmlibraryInfo.LinkInSimulation;
				libraryContext.QualifiedAccessOnly = ilmlibraryInfo.QualifiedOnly;
				if (!libraryContext.LinkAll)
				{
					libraryContext.LinkAll = ilmlibraryInfo.LinkAllContent;
				}
				if (ilmlibraryInfo is ILMLibraryInfo3)
				{
					libraryContext.OnlineChangeable = (ilmlibraryInfo as ILMLibraryInfo3).OnlineChangeable;
				}
				ILibParameterTable paramTable = ilmlibraryInfo.ParamTable;
				if (((paramTable != null) ? paramTable.ParameterTable : null) != null)
				{
					CaseInsensitiveDictionary<IExpression> caseInsensitiveDictionary = new CaseInsensitiveDictionary<IExpression>();
					foreach (object obj in paramTable.ParameterTable.Keys)
					{
						string text = (string)obj;
						caseInsensitiveDictionary[text] = (paramTable.ParameterTable[text] as IExpression);
					}
					\u001D.\u0004.\u0001(ilmlibraryInfo, \u0005, caseInsensitiveDictionary, parameterTableTable);
				}
				languageModelMgr.LibList.AddLibraryReference(\u0002.LibManGuid, ilmlibraryInfo.Identification);
			}
		}

		// Token: 0x060019E6 RID: 6630 RVA: 0x000514C4 File Offset: 0x0004F6C4
		private static void \u0001(ILMLibraryInfo \u0002, _IPreCompileContext \u0003, CaseInsensitiveDictionary<IExpression> \u0004, ICaseInsensitiveDictionary<ICaseInsensitiveDictionary<IExpression>> \u0005)
		{
			ILMLibraryInfo5 ilmlibraryInfo = \u0002 as ILMLibraryInfo5;
			if (ilmlibraryInfo != null && ilmlibraryInfo.PoolLibrary)
			{
				if (!\u0005.ContainsKey(\u0002.Identification))
				{
					\u0003.AddLibraryParamTable(\u0002.Identification, \u0004);
					return;
				}
			}
			else
			{
				\u0003.AddLibraryParamTable(\u0002.Identification, \u0004);
			}
		}

		// Token: 0x060019E7 RID: 6631 RVA: 0x00051510 File Offset: 0x0004F710
		public void \u0001(_ILanguageModelManagerConsolidated \u0002, ILMPOU \u0003, Guid \u0004, _IPreCompileContext \u0005, string \u0006)
		{
			\u0002.AddRelatedObject(\u0004, \u0003.ObjectGuid, \u0003.POUGuid);
			_ILMEntity ilmentity = (_ILMEntity)\u0003;
			global::\u0011.\u0006 u = \u001D.\u0004.\u0001(\u0003);
			ILanguageModelBuilder12 u2 = global::\u0019.\u0003.Builder;
			IReadOnlyCollection<IEmbeddedLanguageService> embeddedLanguageServices = APEnvironmentFacade.Instance.GetEmbeddedLanguageServices();
			ISequenceStatement @interface = \u0003.Interface;
			if (@interface == null)
			{
				throw new InvalidOperationException("Can never be null here, since we create a dummy interface for actions.");
			}
			\u001C.\u0008.\u0001(u2, embeddedLanguageServices, @interface, (_ISequenceStatement)\u0003.Body, \u0003);
			if (!global::\u0008.\u0002.\u0001(\u0003.Body as _IExprement))
			{
				_IErrorStatement ierrorStatement = global::\u0019.\u0003.\u0001();
				ierrorStatement.SetPositionIntern(global::\u0019.\u0003.\u0001(0L, 0));
				\u0003.Body = global::\u0019.\u0003.\u0001(new List<IStatement>
				{
					ierrorStatement
				});
				global::\u0003.\u0006.\u0002(ierrorStatement, MessageId.Err_MaxNestingDepthExceeded, Array.Empty<object>());
			}
			int projectHandle = \u0002.LibList.GetProjectHandle(\u0006);
			if (!\u0005.PrecompiledLibrary)
			{
				MacroReplacement.ReplaceMacroOperators(projectHandle, \u0003);
			}
			_ISignature isignature = \u001D.\u0004.\u0001(\u0002, \u0003, \u0005, \u0006, ilmentity, u);
			_ICompiledPOU icompiledPOU = \u001D.\u0004.\u0001(\u0002, \u0003, \u0005, \u0006, isignature);
			if (isignature != null)
			{
				\u001D.\u0004.\u0001(isignature, icompiledPOU);
			}
			if (icompiledPOU != null && isignature != null)
			{
				\u001D.\u0004.\u0001(\u0003, isignature, icompiledPOU);
				global::\u0014.\u0013.\u0001(\u0005, isignature, icompiledPOU);
			}
			if (isignature != null)
			{
				((ILMPOU)ilmentity).Body = null;
				isignature.RawDeclaration = ilmentity;
				if (\u0002.LateLibraryLoadFinished && string.IsNullOrEmpty(isignature.LibraryPath))
				{
					APEnvironmentFacade.Instance.PrecompileChecker.AddRecentLMResult(new LanguageModelResult(\u0005, isignature));
				}
			}
		}

		// Token: 0x060019E8 RID: 6632 RVA: 0x00051668 File Offset: 0x0004F868
		private static void \u0001(ILMPOU \u0002, _ISignature \u0003, _ICompiledPOU \u0004)
		{
			if (\u0003.HasAttribute(CompileAttributes.ATTRIBUTE_MONITORING) && \u0003.GetAttributeValue(CompileAttributes.ATTRIBUTE_MONITORING) == CompileAttributes.ATTRIBUTEVALUE_CALL)
			{
				\u0004.SetFlag(CompiledPOUFlags.TopLevel, true);
				\u0004.UpdateChecksum();
			}
			if ((\u0002.Slot != -1 && \u0002.TaskReference != Guid.Empty) || \u0002.DownloadSlot != -1 || \u0002.OnlineChangeSlot != -1)
			{
				\u0003.SetFlag(SignatureFlag.TopLevel, true);
			}
		}

		// Token: 0x060019E9 RID: 6633 RVA: 0x000516E4 File Offset: 0x0004F8E4
		private static global::\u0011.\u0006 \u0001(ILMPOU \u0002)
		{
			global::\u0011.\u0006 u;
			if (\u0002.Action && \u0002.Interface == null)
			{
				u = new global::\u0011.\u0006("PROGRAM " + \u0002.Name);
				\u0002.Interface = u.\u0002();
			}
			else
			{
				u = new global::\u0011.\u0006("");
			}
			return u;
		}

		// Token: 0x060019EA RID: 6634 RVA: 0x00051734 File Offset: 0x0004F934
		private static _ISignature \u0001(_ILanguageModelManagerConsolidated \u0002, ILMPOU \u0003, _IPreCompileContext \u0004, string \u0005, _ILMEntity \u0006, global::\u0011.\u0006 \u0007)
		{
			_ISignature isignature = null;
			if (\u0003.Interface != null)
			{
				global::\u0004.\u0004 u = new global::\u0004.\u0004(\u0003.POUGuid, \u0002.PCCRVariables, \u0002.PCCRCalls, \u0002.PCCRDirVars, true, !\u0004.PrecompiledLibrary);
				\u0007.Checker = u;
				_IStatement u2 = (\u0003.Interface as _IStatement).Duplicate() as _IStatement;
				isignature = \u0007.\u0001(\u0004, \u0006.CompilerDefines, u2, null, false, \u0006.DefaultFlag);
				isignature.ObjectGuid = \u0003.POUGuid;
				if (isignature.HasAttribute(CompileAttributes.ATTRIBUTE_MESSAGE_GUID))
				{
					isignature.MessageGuid = new Guid(isignature.GetAttributeValue(CompileAttributes.ATTRIBUTE_MESSAGE_GUID));
				}
				else
				{
					isignature.MessageGuid = \u0003.MessageGuid;
				}
				isignature.ParentObjectGuid = \u0003.ParentObjectGuid;
				isignature.SetFlag(SignatureFlag.External, \u0003.External);
				isignature.LibraryPath = \u0005;
				if (\u0003.InhibitOnlineChange)
				{
					isignature.SetFlag(SignatureFlag.InhibitOnlineChange, true);
				}
				isignature.SetFlag(SignatureFlag.InhibitOnlineChangeOnCodeChanges, \u0003.InhibitOnlineChange);
				isignature.SetFlag(\u0006.DefaultFlag, true);
				if (\u0003.Action)
				{
					isignature.POUType = Operator.Action;
				}
				\u0004.AddSignature(isignature, true);
			}
			return isignature;
		}

		// Token: 0x060019EB RID: 6635 RVA: 0x00051860 File Offset: 0x0004FA60
		private static void \u0001(_ISignature \u0002, ISequenceStatement \u0003)
		{
			if (!\u0002.GetFlag(SignatureFlag.RawSTTransition))
			{
				return;
			}
			_ISequenceStatement isequenceStatement = \u0003 as _ISequenceStatement;
			if (isequenceStatement == null)
			{
				return;
			}
			string attributeValue = \u0002.GetAttributeValue(CompileAttributes.ATTRIBUTE_TRANSITION);
			global::\u0011.\u0004 u = global::\u0016.\u0006.Singleton.\u0001(isequenceStatement, attributeValue);
			if (1 == u.CountStatements && !u.HasExplicitTransitionAssignment && u.SingleExpression != null && u.OwningSequenceStatement != null)
			{
				_IExpressionStatement sm = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(attributeValue), u.SingleExpression);
				((_ISequenceStatement)u.OwningSequenceStatement).Replace(sm, u.StatementPosition);
			}
			if (1 < u.CountStatements && !u.HasExplicitTransitionAssignment)
			{
				isequenceStatement.AddError(global::\u000E.\u0018.\u0001(MessageId.Err_ExplicitTransitionAssignMissing), MessageId.Err_ExplicitTransitionAssignMissing);
			}
		}

		// Token: 0x060019EC RID: 6636 RVA: 0x00051914 File Offset: 0x0004FB14
		private static _ICompiledPOU \u0001(_ILanguageModelManagerConsolidated \u0002, ILMPOU \u0003, _IPreCompileContext \u0004, string \u0005, _ISignature \u0006)
		{
			if (\u0003.Body == null)
			{
				return null;
			}
			\u001D.\u0004.\u0001(\u0006, \u0003.Body);
			_ICompiledPOU icompiledPOU = global::\u0019.\u0003.\u0001(\u0003.Name);
			Guid messageGuid;
			if (\u0003.MessageGuid == Guid.Empty && \u0006.MessageGuid != Guid.Empty)
			{
				messageGuid = \u0006.MessageGuid;
			}
			else
			{
				messageGuid = \u0003.MessageGuid;
			}
			icompiledPOU.MessageGuid = messageGuid;
			if (messageGuid != Guid.Empty)
			{
				_IMessageGuidPragmaStatement imessageGuidPragmaStatement = global::\u0019.\u0003.\u0001(Token.Empty, messageGuid);
				if (\u0003.Body.Statements.Length == 0)
				{
					\u0003.Body.AddStatement(imessageGuidPragmaStatement);
				}
				else
				{
					\u0003.Body.InsertStatement(0, imessageGuidPragmaStatement);
				}
				imessageGuidPragmaStatement.Text = string.Format("{{attribute 'message_guid' := '{0}'}}", messageGuid);
			}
			icompiledPOU.ObjectGuid = \u0003.POUGuid;
			icompiledPOU.LibraryPath = \u0005;
			icompiledPOU.SetParseTree(\u0003.Body as _IStatement);
			IStatementTraverser visitor = new global::\u0013.\u0001(new \u0084.\u0008());
			icompiledPOU.Accept(visitor);
			global::\u0004.\u0004 visitor2 = new global::\u0004.\u0004(icompiledPOU.ObjectGuid, \u0002.PCCRVariables, \u0002.PCCRCalls, \u0002.PCCRDirVars, false, !\u0004.PrecompiledLibrary);
			icompiledPOU.Accept(visitor2);
			icompiledPOU.SetMessages(CompilerServicesInternal.\u0001(icompiledPOU));
			icompiledPOU.SetFlag(CompiledPOUFlags.TopLevel, \u0003.EnableSystemCall);
			\u0004.AddCompiledPOU(icompiledPOU, true);
			\u0004.SlotPOUs.Remove(icompiledPOU.ObjectGuid);
			if (\u0003.Slot != -1 && \u0003.TaskReference != Guid.Empty)
			{
				\u0004.SlotPOUs.Add(\u0003.TaskReference, \u0003.Slot, icompiledPOU.ObjectGuid);
			}
			if (\u0003.DownloadSlot != -1)
			{
				\u0004.SlotPOUs.AddDownloadSlot(\u0003.DownloadSlot, icompiledPOU.ObjectGuid);
			}
			if (\u0003.OnlineChangeSlot != -1)
			{
				\u0004.SlotPOUs.AddOnlineChangeSlot(\u0003.OnlineChangeSlot, icompiledPOU.ObjectGuid);
			}
			return icompiledPOU;
		}

		// Token: 0x060019ED RID: 6637 RVA: 0x00051AF0 File Offset: 0x0004FCF0
		public void \u0001(_ILanguageModelManagerConsolidated \u0002, ILMGlobVarlist \u0003, Guid \u0004, _IPreCompileContext \u0005, string \u0006)
		{
			_ILMEntity ilmentity = (_ILMEntity)\u0003;
			\u0002.AddRelatedObject(\u0004, \u0003.ObjectGuid, \u0003.GVLGuid);
			if (!\u0005.PrecompiledLibrary)
			{
				MacroReplacement.ReplaceMacroOperators(\u0002.LibList.GetProjectHandle(\u0006), \u0003.GVLGuid, \u0003.Interface);
			}
			global::\u0004.\u0004 u = new global::\u0004.\u0004(\u0003.GVLGuid, \u0002.PCCRVariables, \u0002.PCCRCalls, \u0002.PCCRDirVars, true, !\u0005.PrecompiledLibrary);
			global::\u0011.\u0006 u2 = new global::\u0011.\u0006("")
			{
				Checker = u
			};
			_IStatement u3 = (\u0003.Interface as _IStatement).Duplicate() as _IStatement;
			_ISignature isignature = u2.\u0001(\u0005, ilmentity.CompilerDefines, u3, \u0003.Name, true, ilmentity.DefaultFlag);
			isignature.MessageGuid = u2.MessageGuid;
			isignature.ObjectGuid = \u0003.GVLGuid;
			isignature.LibraryPath = \u0006;
			isignature.SetFlag(SignatureFlag.InhibitOnlineChange, \u0003.InhibitOnlineChange);
			if (isignature.POUType == Operator.VarConfig)
			{
				isignature.SetFlag(SignatureFlag.TopLevel, true);
			}
			\u001D.\u0004.\u0001(isignature);
			isignature.SetFlag(ilmentity.DefaultFlag, true);
			isignature.RawDeclaration = ilmentity;
			\u0005.AddSignature(isignature, true);
			if (\u0002.LateLibraryLoadFinished && string.IsNullOrEmpty(isignature.LibraryPath))
			{
				APEnvironmentFacade.Instance.PrecompileChecker.AddRecentLMResult(new LanguageModelResult(\u0005, isignature));
			}
		}

		// Token: 0x060019EE RID: 6638 RVA: 0x00051C54 File Offset: 0x0004FE54
		public void \u0001(_ILanguageModelManagerConsolidated \u0002, ILMDataType \u0003, Guid \u0004, _IPreCompileContext \u0005, string \u0006)
		{
			_ILMEntity ilmentity = (_ILMEntity)\u0003;
			if (!\u0005.PrecompiledLibrary)
			{
				MacroReplacement.ReplaceMacroOperators(\u0002.LibList.GetProjectHandle(\u0006), \u0003.DUTGuid, \u0003.Interface);
			}
			global::\u0004.\u0004 u = new global::\u0004.\u0004(\u0003.DUTGuid, \u0002.PCCRVariables, \u0002.PCCRCalls, \u0002.PCCRDirVars, true, !\u0005.PrecompiledLibrary);
			\u0002.AddRelatedObject(\u0004, \u0003.ObjectGuid, \u0003.DUTGuid);
			global::\u0011.\u0006 u2 = new global::\u0011.\u0006("");
			u2.Checker = u;
			_IStatement u3 = (\u0003.Interface as _IStatement).Duplicate() as _IStatement;
			_ISignature isignature = u2.\u0001(\u0005, ilmentity.CompilerDefines, u3);
			isignature.MessageGuid = u2.MessageGuid;
			isignature.ObjectGuid = \u0003.DUTGuid;
			isignature.LibraryPath = \u0006;
			isignature.SetFlag(SignatureFlag.InhibitOnlineChange, \u0003.InhibitOnlineChange);
			isignature.SetFlag(ilmentity.DefaultFlag, true);
			\u001D.\u0004.\u0001(isignature);
			isignature.RawDeclaration = ilmentity;
			\u0005.AddSignature(isignature, true);
			if (\u0002.LateLibraryLoadFinished && string.IsNullOrEmpty(isignature.LibraryPath))
			{
				APEnvironmentFacade.Instance.PrecompileChecker.AddRecentLMResult(new LanguageModelResult(\u0005, isignature));
			}
		}

		// Token: 0x060019EF RID: 6639 RVA: 0x00051D90 File Offset: 0x0004FF90
		public ILanguageModel \u0001(string \u0002, IList<IList<string>> \u0003)
		{
			string empty = string.Empty;
			if (string.IsNullOrEmpty(\u0002))
			{
				return null;
			}
			try
			{
				Guid empty2 = Guid.Empty;
				Guid empty3 = Guid.Empty;
				bool u = false;
				string empty4 = string.Empty;
				bool u2 = false;
				SignatureFlag u3 = SignatureFlag.None;
				global::\u0005.\u0003 u4 = new global::\u0007.\u0007(\u0002).Result;
				ILanguageModel languageModel = global::\u0019.\u0003.Builder.CreateLanguageModel(empty2, Guid.Empty, empty3, empty4);
				if (u4.Name == "language-model")
				{
					this.\u0001(languageModel, u4, u, empty, u2, u3, \u0003, null);
				}
				else if (u4.Name == "language-model-list")
				{
					foreach (global::\u0005.\u0003 u5 in u4.ChildNodes)
					{
						if (u5.Name == "language-model")
						{
							this.\u0001(languageModel, u5, u, empty, u2, u3, \u0003, null);
						}
					}
				}
				return languageModel;
			}
			catch (Exception ex)
			{
				Debug.\u0001(false, ex.ToString());
			}
			return null;
		}

		// Token: 0x060019F0 RID: 6640 RVA: 0x00051EB4 File Offset: 0x000500B4
		internal IEnumerable<ILanguageModel> \u0001(string \u0002, bool \u0003, string \u0004, Guid \u0005, Guid \u0006, string \u0007, bool \u0008, SignatureFlag \u000E, IList<IList<string>> \u000F, KeyValuePair<string, string>[] \u0010)
		{
			global::\u0005.\u0003 u = new global::\u0007.\u0007(\u0002).Result;
			Dictionary<Guid, ILanguageModel> dictionary = new Dictionary<Guid, ILanguageModel>();
			if (u.Name == "language-model")
			{
				ILanguageModel languageModel = global::\u0019.\u0003.Builder.CreateLanguageModel(\u0005, Guid.Empty, \u0006, \u0007);
				this.\u0001(languageModel, u, \u0003, \u0004, \u0008, \u000E, \u000F, \u0010);
				dictionary.Add(\u0005, languageModel);
			}
			else if (u.Name == "language-model-list")
			{
				foreach (global::\u0005.\u0003 u2 in u.ChildNodes)
				{
					if (u2.Name == "language-model")
					{
						ILanguageModel languageModel2 = null;
						Guid key = \u0005;
						u2.\u0001("application-id", ref key);
						if (!dictionary.TryGetValue(key, out languageModel2))
						{
							languageModel2 = global::\u0019.\u0003.Builder.CreateLanguageModel(\u0005, Guid.Empty, \u0006, \u0007);
							dictionary.Add(key, languageModel2);
						}
						this.\u0001(languageModel2, u2, \u0003, \u0004, \u0008, \u000E, \u000F, \u0010);
					}
				}
			}
			return dictionary.Values;
		}

		// Token: 0x060019F1 RID: 6641 RVA: 0x00051FE0 File Offset: 0x000501E0
		private void \u0001(ILanguageModel \u0002, global::\u0005.\u0003 \u0003, bool \u0004, string \u0005, bool \u0006, SignatureFlag \u0007, IList<IList<string>> \u0008, KeyValuePair<string, string>[] \u000E)
		{
			bool u = false;
			string libraryId = \u0002.LibraryId;
			Guid deviceGuid = \u0002.DeviceGuid;
			Guid applicationGuid = \u0002.ApplicationGuid;
			Guid languageModelObject = \u0002.LanguageModelObject;
			\u0003.\u0001("library-id", ref libraryId);
			\u0003.\u0001("plclogic-id", ref deviceGuid);
			\u0003.\u0001("application-id", ref applicationGuid);
			\u0003.\u0001("object-id", ref languageModelObject);
			\u0002.LibraryId = libraryId;
			\u0002.DeviceGuid = deviceGuid;
			\u0002.ApplicationGuid = applicationGuid;
			\u0002.LanguageModelObject = languageModelObject;
			if (libraryId != string.Empty)
			{
				if (applicationGuid != Guid.Empty)
				{
					return;
				}
				u = true;
			}
			else if (applicationGuid == Guid.Empty)
			{
				\u0007 |= SignatureFlag.PoolSignature;
			}
			foreach (global::\u0005.\u0003 u2 in \u0003.ChildNodes)
			{
				int u000E = -1;
				int u3 = -1;
				int u4 = -1;
				Guid empty = Guid.Empty;
				string text = u2.Name;
				uint num = global::\u0017.\u0019.\u0001(text);
				if (num <= 2474366543U)
				{
					if (num <= 543061556U)
					{
						if (num != 524788293U)
						{
							if (num != 543061556U)
							{
								continue;
							}
							if (!(text == "global-interface"))
							{
								continue;
							}
							\u001D.\u0004.\u0002(\u0002, \u0008, u2);
							continue;
						}
						else
						{
							if (!(text == "application"))
							{
								continue;
							}
							\u001D.\u0004.\u0003(\u0002, u2);
							continue;
						}
					}
					else if (num != 1446209455U)
					{
						if (num != 2474366543U)
						{
							continue;
						}
						if (!(text == "library-list"))
						{
							continue;
						}
						\u001D.\u0004.\u0001(\u0002, u2);
						continue;
					}
					else
					{
						if (!(text == "pou"))
						{
							continue;
						}
						u2.\u0001("slot", ref u000E);
						u2.\u0001("download_slot", ref u3);
						u2.\u0001("online_change_slot", ref u4);
						u2.\u0001("task-id", ref empty);
					}
				}
				else if (num <= 3078004304U)
				{
					if (num != 2873489200U)
					{
						if (num != 3078004304U)
						{
							continue;
						}
						if (!(text == "data-type"))
						{
							continue;
						}
						\u001D.\u0004.\u0001(\u0002, \u0008, u2);
						continue;
					}
					else if (!(text == "method"))
					{
						continue;
					}
				}
				else if (num != 3294899967U)
				{
					if (num != 3497031411U)
					{
						if (num != 4224874459U)
						{
							continue;
						}
						if (!(text == "task-list"))
						{
							continue;
						}
						\u001D.\u0004.\u0002(\u0002, u2);
						continue;
					}
					else
					{
						if (!(text == "device"))
						{
							continue;
						}
						\u001D.\u0004.\u0004(\u0002, u2);
						continue;
					}
				}
				else if (!(text == "action"))
				{
					continue;
				}
				\u001D.\u0004.\u0001(\u0002, \u0004, \u0005, \u0006, \u0008, \u000E, u2, u000E, empty, u3, u4, u);
			}
		}

		// Token: 0x060019F2 RID: 6642 RVA: 0x00052308 File Offset: 0x00050508
		private static void \u0001(ILanguageModel \u0002, IList<IList<string>> \u0003, global::\u0005.\u0003 \u0004)
		{
			global::\u0005.\u0003 u = \u0004.\u0001("interface");
			string stName = null;
			if (!\u0004.\u0001("name", ref stName) || u == null)
			{
				return;
			}
			Guid empty = Guid.Empty;
			Guid empty2 = Guid.Empty;
			\u0004.\u0001("object-id", ref empty);
			\u0004.\u0001("id", ref empty2);
			ILMDataType ilmdataType = global::\u0019.\u0003.Builder.CreateDataType(stName, empty2);
			ilmdataType.ObjectGuid = empty;
			int index = 0;
			global::\u0011.\u0006 u2;
			if (u.\u0001("string-table-reference", ref index))
			{
				u2 = new global::\u0011.\u0006(\u0003[index]);
			}
			else
			{
				u2 = new global::\u0011.\u0006(u.Text);
			}
			bool inhibitOnlineChange = false;
			\u0004.\u0001("inhibit-online-change", ref inhibitOnlineChange);
			ilmdataType.InhibitOnlineChange = inhibitOnlineChange;
			ilmdataType.Interface = u2.\u0002();
			\u0002.AddDataType(ilmdataType);
		}

		// Token: 0x060019F3 RID: 6643 RVA: 0x000523D4 File Offset: 0x000505D4
		private static void \u0002(ILanguageModel \u0002, IList<IList<string>> \u0003, global::\u0005.\u0003 \u0004)
		{
			global::\u0005.\u0003 u = \u0004.\u0001("interface");
			string stName = null;
			if (!\u0004.\u0001("name", ref stName))
			{
				return;
			}
			Guid empty = Guid.Empty;
			Guid empty2 = Guid.Empty;
			\u0004.\u0001("object-id", ref empty);
			\u0004.\u0001("id", ref empty2);
			ILMGlobVarlist ilmglobVarlist = global::\u0019.\u0003.Builder.CreateGlobVarlist(stName, empty2);
			ilmglobVarlist.ObjectGuid = empty;
			int index = 0;
			global::\u0011.\u0006 u2;
			if (u.\u0001("string-table-reference", ref index))
			{
				u2 = new global::\u0011.\u0006(\u0003[index]);
			}
			else
			{
				u2 = new global::\u0011.\u0006(u.Text);
			}
			bool inhibitOnlineChange = false;
			\u0004.\u0001("inhibit-online-change", ref inhibitOnlineChange);
			ilmglobVarlist.InhibitOnlineChange = inhibitOnlineChange;
			ilmglobVarlist.Interface = u2.\u0001(ilmglobVarlist.Name);
			\u0002.AddGlobalVariableList(ilmglobVarlist);
		}

		// Token: 0x060019F4 RID: 6644 RVA: 0x000524A4 File Offset: 0x000506A4
		private static void \u0001(ILanguageModel \u0002, bool \u0003, string \u0004, bool \u0005, IList<IList<string>> \u0006, KeyValuePair<string, string>[] \u0007, global::\u0005.\u0003 \u0008, int \u000E, Guid \u000F, int \u0010, int \u0011, bool \u0012)
		{
			global::\u0005.\u0003 u = \u0008.\u0001("interface");
			global::\u0005.\u0003 u2 = \u0008.\u0001("body");
			string stName = null;
			if (!\u0008.\u0001("name", ref stName))
			{
				return;
			}
			Guid empty = Guid.Empty;
			Guid empty2 = Guid.Empty;
			Guid empty3 = Guid.Empty;
			bool flag = false;
			bool inhibitOnlineChange = false;
			\u0008.\u0001("id", ref empty);
			\u0008.\u0001("pou-id", ref empty2);
			\u0008.\u0001("object-id", ref empty3);
			\u0008.\u0001("external", ref flag);
			flag = (\u0003 || flag);
			\u0008.\u0001("inhibit-online-change", ref inhibitOnlineChange);
			\u0008.\u0001("enable-system-call", ref \u0005);
			ILMPOU ilmpou = global::\u0019.\u0003.Builder.CreatePou(stName, empty);
			ilmpou.ObjectGuid = empty3;
			ilmpou.External = flag;
			ilmpou.InhibitOnlineChange = inhibitOnlineChange;
			ilmpou.EnableSystemCall = \u0005;
			ilmpou.Slot = \u000E;
			ilmpou.TaskReference = \u000F;
			ilmpou.DownloadSlot = \u0010;
			ilmpou.OnlineChangeSlot = \u0011;
			ilmpou.ParentObjectGuid = empty2;
			if (u2 != null)
			{
				int index = 0;
				global::\u0011.\u0006 u3;
				if (u2.\u0001("string-table-reference", ref index))
				{
					u3 = new global::\u0011.\u0006(\u0006[index]);
					u3.ImplicitAnyway = true;
				}
				else
				{
					string text = u2.Text ?? string.Empty;
					if (!string.IsNullOrEmpty(\u0004))
					{
						string str = "{define " + \u0004 + "}" + Environment.NewLine;
						string str2 = "{undefine " + \u0004 + "}" + Environment.NewLine;
						text = str + text + str2;
					}
					u3 = new global::\u0011.\u0006(text, true);
				}
				ilmpou.Body = (u3.\u0001(\u0012) as ISequenceStatement2);
				ilmpou.MessageGuid = u3.MessageGuid;
			}
			if (u != null)
			{
				int index2 = 0;
				global::\u0011.\u0006 u4;
				if (u.\u0001("string-table-reference", ref index2))
				{
					u4 = new global::\u0011.\u0006(\u0006[index2]);
				}
				else
				{
					string u0087_u = u.Text;
					if (\u0007 != null)
					{
						LStringBuilder lstringBuilder = new LStringBuilder();
						foreach (KeyValuePair<string, string> keyValuePair in \u0007)
						{
							lstringBuilder.AppendLine(string.Concat(new string[]
							{
								"{attribute '",
								keyValuePair.Key,
								"':='",
								keyValuePair.Value,
								"'}"
							}));
						}
						lstringBuilder.Append(u.Text);
						u0087_u = lstringBuilder.ToString();
					}
					u4 = new global::\u0011.\u0006(u0087_u);
				}
				ilmpou.Interface = u4.\u0002();
			}
			else if (\u0008.Name == "action")
			{
				ilmpou.Action = true;
			}
			\u0002.AddPou(ilmpou);
		}

		// Token: 0x060019F5 RID: 6645 RVA: 0x00052760 File Offset: 0x00050960
		private static void \u0001(ILanguageModel \u0002, global::\u0005.\u0003 \u0003)
		{
			Guid empty = Guid.Empty;
			Guid empty2 = Guid.Empty;
			\u0003.\u0001("object-id", ref empty2);
			\u0003.\u0001("id", ref empty);
			ILMLibraryList ilmlibraryList = global::\u0019.\u0003.Builder.CreateLibraryList(empty);
			ilmlibraryList.ObjectGuid = empty2;
			foreach (global::\u0005.\u0003 u in \u0003.ChildNodes)
			{
				ILibParameterTable libParameterTable = null;
				foreach (global::\u0005.\u0003 u2 in u.ChildNodes)
				{
					if (u2.Name == "parameter-list")
					{
						libParameterTable = global::\u0019.\u0003.Builder.CreateLibraryParameterTable();
						foreach (global::\u0005.\u0003 u3 in u2.ChildNodes)
						{
							string stName = u3.\u0001("name");
							string stValue = u3.\u0001("value");
							libParameterTable.AddParameter(stName, stValue);
						}
					}
				}
				if (u.Name == "library")
				{
					string empty3 = string.Empty;
					string empty4 = string.Empty;
					if (!u.\u0001("library-id", ref empty3))
					{
						break;
					}
					if (!u.\u0001("namespace", ref empty4))
					{
						break;
					}
					bool bPublishSymbols = false;
					bool bLinkAllContent = false;
					bool bLinkInSimulation = false;
					bool flag = false;
					string stDefaultNamespace = empty4;
					bool bSystemLibrary = false;
					bool bQualifiedOnly = false;
					u.\u0001("publish-symbols-in-container", ref bPublishSymbols);
					u.\u0001("link-all-content", ref bLinkAllContent);
					u.\u0001("LinkInSimulation", ref bLinkInSimulation);
					u.\u0001("system-application", ref flag);
					u.\u0001("default-namespace", ref stDefaultNamespace);
					u.\u0001("system-library", ref bSystemLibrary);
					u.\u0001("qualified-access-only", ref bQualifiedOnly);
					ILMLibraryInfo ilmlibraryInfo = global::\u0019.\u0003.Builder.CreateLibInfo(empty3, stDefaultNamespace, empty4, bSystemLibrary, bPublishSymbols, bLinkAllContent, bLinkInSimulation, bQualifiedOnly);
					ilmlibraryInfo.ParamTable = libParameterTable;
					ilmlibraryList.AddLibraryInfo(ilmlibraryInfo);
				}
			}
			if (\u0002.LMLibraryList != null)
			{
				Debug.\u0001(false, "more than one liblist in language model ???");
				return;
			}
			\u0002.LMLibraryList = ilmlibraryList;
		}

		// Token: 0x060019F6 RID: 6646 RVA: 0x000529E0 File Offset: 0x00050BE0
		private static void \u0002(ILanguageModel \u0002, global::\u0005.\u0003 \u0003)
		{
			Guid empty = Guid.Empty;
			Guid empty2 = Guid.Empty;
			\u0003.\u0001("object-id", ref empty2);
			\u0003.\u0001("id", ref empty);
			ILMTaskList ilmtaskList = global::\u0019.\u0003.Builder.CreateTaskList(empty);
			ilmtaskList.ObjectGuid = empty2;
			foreach (global::\u0005.\u0003 u in \u0003.ChildNodes)
			{
				if (u.Name == "task")
				{
					Guid empty3 = Guid.Empty;
					string stName = null;
					if (u.\u0001("id", ref empty3) && u.\u0001("name", ref stName))
					{
						string stParentTaskName = null;
						u.\u0001("parent-synch-task", ref stParentTaskName);
						ilmtaskList.AddTask(global::\u0019.\u0003.Builder.CreateTaskInfo(empty3, stName, stParentTaskName));
					}
				}
			}
			if (\u0002.LMTaskList != null)
			{
				Debug.\u0001(false, "more than one tasklist in language model ???");
				return;
			}
			\u0002.LMTaskList = ilmtaskList;
		}

		// Token: 0x060019F7 RID: 6647 RVA: 0x00052AE8 File Offset: 0x00050CE8
		private static void \u0003(ILanguageModel \u0002, global::\u0005.\u0003 \u0003)
		{
			Guid empty = Guid.Empty;
			Guid empty2 = Guid.Empty;
			string empty3 = string.Empty;
			string empty4 = string.Empty;
			string empty5 = string.Empty;
			IDeviceIdentification devid = null;
			foreach (global::\u0005.\u0003 u in \u0003.ChildNodes)
			{
				if (u.Name == "device-identification")
				{
					int nType = 0;
					string stId = null;
					string stVersion = null;
					if (u.\u0001("type", ref nType) && u.\u0001("target-id", ref stId) && u.\u0001("version", ref stVersion))
					{
						devid = global::\u0019.\u0003.Builder.CreateDeviceIdentification(nType, stId, stVersion);
					}
				}
			}
			int dynamicMemorySize = -1;
			\u0003.\u0001("parent-application-id", ref empty);
			\u0003.\u0001("memory-settings-provider-id", ref empty2);
			\u0003.\u0001("plclogic", ref empty3);
			\u0003.\u0001("application", ref empty4);
			\u0003.\u0001("simulation-application", ref empty5);
			\u0003.\u0001("dynamic_memory_size", ref dynamicMemorySize);
			ILMApplication ilmapplication = global::\u0019.\u0003.Builder.CreateLMForApplication(empty, empty3, empty4, devid);
			ilmapplication.DynamicMemorySize = dynamicMemorySize;
			ilmapplication.MemorySettingsProvider = empty2;
			ilmapplication.SimulationApplicationName = empty5;
			if (\u0002.LMApplication != null)
			{
				Debug.\u0001(false, "more than one application in language model ???");
				return;
			}
			\u0002.LMApplication = ilmapplication;
		}

		// Token: 0x060019F8 RID: 6648 RVA: 0x00052C54 File Offset: 0x00050E54
		private static void \u0004(ILanguageModel \u0002, global::\u0005.\u0003 \u0003)
		{
			string empty = string.Empty;
			\u0003.\u0001("plclogic", ref empty);
			IDeviceIdentification devid = null;
			foreach (global::\u0005.\u0003 u in \u0003.ChildNodes)
			{
				if (u.Name == "device-identification")
				{
					int nType = 0;
					string stId = null;
					string stVersion = null;
					if (u.\u0001("type", ref nType) && u.\u0001("target-id", ref stId) && u.\u0001("version", ref stVersion))
					{
						devid = global::\u0019.\u0003.Builder.CreateDeviceIdentification(nType, stId, stVersion);
					}
				}
			}
			ILMDevice lmdevice = global::\u0019.\u0003.Builder.CreateLMForDevice(empty, devid);
			if (\u0002.LMDevice != null)
			{
				Debug.\u0001(false, "more than one device in language model ???");
				return;
			}
			\u0002.LMDevice = lmdevice;
		}

		// Token: 0x060019F9 RID: 6649 RVA: 0x00052D38 File Offset: 0x00050F38
		internal static bool \u0001(_ICompileContext \u0002, ILanguageModelList \u0003, _ICompileContext \u0004, bool \u0005, bool \u0006)
		{
			bool flag = true;
			foreach (string u0083_u in (\u0003 as _ILanguageModelList).LanguageModels)
			{
				foreach (global::\u0005.\u0003 u in new global::\u0007.\u0007(u0083_u).Result.ChildNodes)
				{
					int num = -1;
					int num2 = -1;
					int num3 = -1;
					Guid empty = Guid.Empty;
					_ISignature isignature = null;
					IScope5 scope = null;
					string a = u.Name;
					if (!(a == "pou"))
					{
						if (!(a == "method") && !(a == "action"))
						{
							if (!(a == "global-interface"))
							{
								if (!(a == "data-type"))
								{
									continue;
								}
								string text = u.\u0001("name");
								global::\u0005.\u0003 u2 = u.\u0001("interface");
								if (text == null || u2 == null)
								{
									continue;
								}
								string u0087_u = u2.Text;
								Guid empty2 = Guid.Empty;
								u.\u0001("id", ref empty2);
								isignature = ((_IParser)new global::\u0011.\u0006(u0087_u))._ParseInterface(null, null);
								isignature.ObjectGuid = empty2;
								_ISignature isignature2 = null;
								if (\u0004 != null)
								{
									isignature2 = \u0004[isignature.Name];
								}
								if (\u0005)
								{
									if (isignature2 == null || isignature.Checksum != isignature2.Checksum)
									{
										return false;
									}
									continue;
								}
								else
								{
									isignature = isignature.CreateCompiledSignature(null, \u0002.HasByteSupport());
									if (\u0006 && isignature2 != null)
									{
										isignature.Checksum = isignature2.Checksum;
										isignature.ChecksumNoInit = isignature2.ChecksumNoInit;
									}
									\u0002.AddSignature(isignature, isignature2, \u0004, true);
									isignature.SetFlag(SignatureFlag.Generated, true);
									scope = global::\u0007.\u0005.\u0001(\u0002, isignature.Id);
									scope.SetLocalSignature(isignature);
									global::\u0014.\u0013.\u0002(isignature, scope, \u0002);
									global::\u0014.\u0013.\u0001(isignature, scope, \u0002);
									if (flag)
									{
										foreach (IMessage message in isignature.Messages)
										{
											flag = (message.Severity != Severity.Error && message.Severity != Severity.FatalError);
											if (!flag)
											{
												break;
											}
										}
										continue;
									}
									continue;
								}
							}
							else
							{
								string text2 = u.\u0001("name");
								global::\u0005.\u0003 u3 = u.\u0001("interface");
								if (text2 == null || u3 == null)
								{
									continue;
								}
								string u4 = u3.Text;
								Guid empty3 = Guid.Empty;
								u.\u0001("id", ref empty3);
								isignature = ParserHelper.\u0001(text2, u4, false);
								isignature.ObjectGuid = empty3;
								_ISignature isignature3 = null;
								if (\u0004 != null)
								{
									isignature3 = \u0004[isignature.Name];
								}
								if (\u0005)
								{
									if (isignature3 == null || isignature3.Checksum != isignature.Checksum)
									{
										return false;
									}
									continue;
								}
								else
								{
									isignature = isignature.CreateCompiledSignature(isignature3, \u0002, \u0004, \u0002.HasByteSupport());
									if (\u0006 && isignature3 != null)
									{
										isignature.Checksum = isignature3.Checksum;
										isignature.ChecksumNoInit = isignature3.ChecksumNoInit;
									}
									\u0002.AddSignature(isignature, isignature3, \u0004, true);
									isignature.SetFlag(SignatureFlag.Generated, true);
									scope = global::\u0007.\u0005.\u0001(\u0002, isignature.Id);
									scope.SetLocalSignature(isignature);
									global::\u0014.\u0013.\u0002(isignature, scope, \u0002);
									global::\u0014.\u0013.\u0001(isignature, scope, \u0002);
									if (flag)
									{
										foreach (IMessage message2 in isignature.Messages)
										{
											flag = (message2.Severity != Severity.Error && message2.Severity != Severity.FatalError);
											if (!flag)
											{
												break;
											}
										}
										continue;
									}
									continue;
								}
							}
						}
					}
					else
					{
						string text3 = u.\u0001("slot");
						if (text3 != null)
						{
							num = XmlConvert.ToInt32(text3);
						}
						string text4 = u.\u0001("download_slot");
						if (text4 != null)
						{
							num2 = XmlConvert.ToInt32(text4);
						}
						string text5 = u.\u0001("online_change_slot");
						if (text5 != null)
						{
							num3 = XmlConvert.ToInt32(text5);
						}
						string text6 = u.\u0001("task-id");
						if (text6 != null)
						{
							empty = new Guid(text6);
						}
					}
					string text7 = u.\u0001("name");
					global::\u0005.\u0003 u5 = u.\u0001("interface");
					global::\u0005.\u0003 u6 = u.\u0001("body");
					if (text7 != null)
					{
						Guid empty4 = Guid.Empty;
						Guid empty5 = Guid.Empty;
						bool bSet = false;
						bool bSetTrue = false;
						u.\u0001("id", ref empty4);
						u.\u0001("pou-id", ref empty5);
						u.\u0001("external", ref bSet);
						u.\u0001("enable-system-call", ref bSetTrue);
						_ISignature isignature4 = null;
						if (u5 != null)
						{
							isignature = ((_IParser)new global::\u0011.\u0006(u5.Text))._ParseInterface(null, null);
							isignature.ObjectGuid = empty4;
							isignature.ParentObjectGuid = empty5;
							isignature.SetFlag(SignatureFlag.Generated, true);
							isignature.SetFlag(SignatureFlag.External, bSet);
							if (\u0004 != null && isignature.POUType != Operator.Method && isignature.POUType != Operator.Action)
							{
								isignature4 = \u0004[isignature.Name];
							}
							if (\u0005)
							{
								if (isignature.HasAttribute(CompileAttributes.ATTRIBUTE_IGNORE_FOR_FAST_ONLINE_CHANGE_TEST))
								{
									continue;
								}
								if (isignature4 == null || isignature4.Checksum != isignature.Checksum)
								{
									return false;
								}
							}
							else
							{
								isignature = isignature.CreateCompiledSignature(isignature4, \u0002, \u0004, \u0002.HasByteSupport());
								\u0002.AddSignature(isignature, isignature4, \u0004, true);
								scope = global::\u0007.\u0005.\u0001(\u0002, isignature.Id);
								scope.SetLocalSignature(isignature);
								global::\u0014.\u0013.\u0002(isignature, scope, \u0002);
								global::\u0014.\u0013.\u0001(isignature, scope, \u0002);
								if (flag)
								{
									foreach (IMessage message3 in isignature.Messages)
									{
										flag = (message3.Severity != Severity.Error && message3.Severity != Severity.FatalError);
										if (!flag)
										{
											break;
										}
									}
								}
								if (\u0006 && isignature4 != null)
								{
									isignature.Checksum = isignature4.Checksum;
									isignature.ChecksumNoInit = isignature4.ChecksumNoInit;
								}
							}
						}
						else if (u.Name == "action")
						{
							Debug.\u0001(false, "Actions not implemented yet for late lmm");
						}
						else
						{
							Debug.\u0001(false, "wrong language model");
						}
						if (u6 != null)
						{
							_IParser iparser = new global::\u0011.\u0006(u6.Text);
							_IStatement istatement = iparser.ParseST();
							_ICompiledPOU icompiledPOU = global::\u0019.\u0003.\u0001(text7);
							icompiledPOU.MessageGuid = iparser.MessageGuid;
							icompiledPOU.SetParseTree(istatement);
							if (\u0005)
							{
								_ICompiledPOU icompiledPOU2 = null;
								if (isignature4 != null && \u0004 != null)
								{
									icompiledPOU2 = \u0004._GetCompiledPOUById(isignature4.Id);
								}
								if (icompiledPOU2 == null)
								{
									return false;
								}
								global::\u0012.\u0002 u7 = new global::\u0012.\u0002(false);
								u7.Traverser.visit(icompiledPOU);
								icompiledPOU.Checksum = u7.Checksum;
								if (icompiledPOU2.Checksum != icompiledPOU.Checksum)
								{
									return false;
								}
							}
							else
							{
								icompiledPOU.ObjectGuid = empty4;
								icompiledPOU.SetFlag(CompiledPOUFlags.TopLevel, bSetTrue);
								icompiledPOU.SetFlag(CompiledPOUFlags.ToCompile, true);
								icompiledPOU.SetFlag(CompiledPOUFlags.NotForUpToDate, true);
								icompiledPOU.SetFlagInternal(InternalCompiledPOUFlags.ImplicitInitialisationCodeAdded, true);
								icompiledPOU.SetFlagInternal(InternalCompiledPOUFlags.TypeCheckDone, true);
								\u0002.AddCompiledPOU(icompiledPOU, isignature, \u0004);
								if (isignature.POUType == Operator.FunctionBlock)
								{
									ISignature subSignature = isignature.GetSubSignature("__MAIN");
									icompiledPOU.SignatureId = subSignature.Id;
								}
								else
								{
									icompiledPOU.SignatureId = isignature.Id;
								}
								CompilerServicesInternal.\u0001(istatement, scope, \u0002, null, true, true, icompiledPOU);
								global::\u0018.\u000E.\u0001(istatement, \u0002);
								CompilerServicesInternal.\u0001(istatement, scope, \u0002, true);
								IMessage[] array = CompilerServicesInternal.\u0001(istatement);
								IMessage[] array2 = array;
								icompiledPOU.SetFlag(CompiledPOUFlags.Typified, true);
								if (flag)
								{
									foreach (IMessage message4 in array2)
									{
										flag = (message4.Severity != Severity.Error && message4.Severity != Severity.FatalError);
										if (!flag)
										{
											break;
										}
									}
								}
								if (num != -1 && empty != Guid.Empty)
								{
									\u0002.SlotPOUs.Add(empty, num, icompiledPOU.ObjectGuid);
									byte taskIndexByGuid = \u0002.TaskList.GetTaskIndexByGuid(empty);
									isignature.AddTaskReference(taskIndexByGuid);
								}
								if (num2 != -1)
								{
									\u0002.SlotPOUs.AddDownloadSlot(num2, icompiledPOU.ObjectGuid);
								}
								if (num3 != -1)
								{
									\u0002.SlotPOUs.AddOnlineChangeSlot(num3, icompiledPOU.ObjectGuid);
								}
							}
						}
					}
				}
			}
			return flag;
		}

		// Token: 0x0400047B RID: 1147
		private readonly \u0082.\u0006 \u0001;

		// Token: 0x0400047C RID: 1148
		private static Guid \u0001 = new Guid("{BE5544D2-D7BC-4a43-9B17-464BFE7A93EC}");

		// Token: 0x0400047D RID: 1149
		private const string \u0001 = "_ApplicationErrorsGVL";

		// Token: 0x0400047E RID: 1150
		private static readonly Guid \u0002 = new Guid("{27202E43-6685-4C5E-816B-B96BA2B630F8}");
	}
}
