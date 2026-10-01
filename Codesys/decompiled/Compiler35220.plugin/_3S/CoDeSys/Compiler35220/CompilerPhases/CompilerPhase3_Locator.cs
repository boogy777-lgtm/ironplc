using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0001;
using \u0004;
using \u000E;
using \u0010;
using \u0011;
using \u0016;
using \u0017;
using \u0019;
using \u001C;
using \u001D;
using _3S.CoDeSys.Compiler35220.Features;
using _3S.CoDeSys.Compiler35220.Features.RetainsInCycle;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0080;
using \u0083;

namespace _3S.CoDeSys.Compiler35220.CompilerPhases
{
	// Token: 0x020003FA RID: 1018
	internal sealed class CompilerPhase3_Locator
	{
		// Token: 0x1700095D RID: 2397
		// (get) Token: 0x06003864 RID: 14436 RVA: 0x000E7780 File Offset: 0x000E5980
		// (set) Token: 0x06003865 RID: 14437 RVA: 0x000E7788 File Offset: 0x000E5988
		private global::\u000E.\u001B CompileInformation { get; set; }

		// Token: 0x1700095E RID: 2398
		// (get) Token: 0x06003866 RID: 14438 RVA: 0x000E7794 File Offset: 0x000E5994
		private _ICompileContext ComconNew
		{
			get
			{
				return this.CompileInformation.ComconNew;
			}
		}

		// Token: 0x1700095F RID: 2399
		// (get) Token: 0x06003867 RID: 14439 RVA: 0x000E77A4 File Offset: 0x000E59A4
		private _ICompileContext ComconOld
		{
			get
			{
				return this.CompileInformation.ComconOld;
			}
		}

		// Token: 0x06003868 RID: 14440 RVA: 0x000E77B4 File Offset: 0x000E59B4
		internal CompilerPhase3_Locator(global::\u000E.\u001B ci)
		{
			this.CompileInformation = ci;
		}

		// Token: 0x06003869 RID: 14441 RVA: 0x000E77C4 File Offset: 0x000E59C4
		internal bool \u0001()
		{
			_ILanguageModelManagerConsolidated languageModelMgr = APEnvironmentFacade.Instance.LanguageModelMgr;
			bool flag = false;
			CompileEventArgs e = new CompileEventArgs(this.CompileInformation.ApplicationGuid);
			languageModelMgr.OnBeforeLocation(e);
			try
			{
				LList<_IArea> llist = new LList<_IArea>(1);
				int num = 0;
				_IMemorySettings memorySettings = APEnvironmentFacade.Instance.LanguageModelMgr.MemorySettingsHelper.GetMemorySettings(this.CompileInformation.DeviceGuid, this.CompileInformation.ParentApplicationGuid, this.CompileInformation.ApplicationGuid, this.ComconNew.SimulationMode);
				this.ComconNew.MemorySettingsChecksum = memorySettings.CalculateChecksum();
				if (this.CompileInformation.ComconDevice != null)
				{
					num = (int)this.CompileInformation.ComconDevice.DataManager.AreaCount;
				}
				if (this.CompileInformation.ComconParent != null)
				{
					num += (int)this.CompileInformation.ComconParent.DataManager.AreaCount;
				}
				if (this.ComconOld != null)
				{
					_IDataManager idataManager = MemoryCompiler.\u0001(this.ComconOld.DataManager);
					this.ComconNew.DataManager.Reference = idataManager;
					this.ComconOld.DataManager._MemorySettings = this.ComconNew.DataManager._MemorySettings;
					CompilerPhase3_Locator.\u0001(llist, idataManager);
					this.\u0001(llist, memorySettings);
				}
				else
				{
					foreach (_IArea iarea in memorySettings.Areas)
					{
						llist.Add(iarea);
					}
				}
				this.\u0002(llist, memorySettings);
				this.ComconNew.ConfigureMemory(memorySettings, llist, num);
				if (!this.\u0003())
				{
					flag = false;
					return false;
				}
				Locator.\u0001(this.ComconNew, this.ComconOld);
				if (!global::\u0004.\u0010.\u0001(this.ComconNew, false))
				{
					flag = false;
					return false;
				}
				ILanguageModelList languageModelList = global::\u0019.\u0003.Builder.CreateLanguageModelList();
				List<ILanguageModel> list = new List<ILanguageModel>();
				AddLanguageModelEventArgs3 e2 = new AddLanguageModelEventArgs3(this.CompileInformation.ApplicationGuid, languageModelList, list, global::\u0019.\u0003.Builder, this.CompileInformation.OnlineChange);
				languageModelMgr.OnAddLateLanguageModel(e2);
				flag = \u001D.\u0004.\u0001(this.ComconNew, languageModelList, this.ComconOld, false, this.CompileInformation.BootProject);
				flag = (flag && \u0083.\u0007.\u0001(this.ComconNew, list, this.ComconOld, false, this.CompileInformation.BootProject));
				if (!flag)
				{
					return false;
				}
				Locator.\u0001(this.ComconNew, this.ComconOld);
				this.\u0001();
				\u0080.\u0003.\u0001(this.ComconNew);
				global::\u0010.\u0012.\u0001(this.ComconNew, this.ComconOld);
				global::\u0011.\u0015.\u0001(this.ComconNew, this.ComconOld);
				this.\u0002();
				if (this.ComconOld != null)
				{
					global::\u0001.\u0010 u = new global::\u0001.\u0010(this.ComconNew, this.ComconOld);
					u.\u0001();
					u.\u0002();
				}
				if (this.CompileInformation.OnlineChange)
				{
					global::\u0004.\u0014.\u0002(this.ComconNew, this.ComconOld, this.CompileInformation.OnlineChange);
				}
				global::\u0004.\u0019.\u0001(this.ComconNew, this.ComconOld, this.ComconNew.HasByteSupport());
				\u001C.\u0014.\u0001(this.ComconNew, this.ComconOld);
				global::\u0004.\u0014.\u0001(this.ComconNew, this.ComconOld, this.CompileInformation.BootProject);
				bool flag2;
				Locator.\u0002(this.ComconNew, this.ComconOld, this.CompileInformation.BootProject, out flag2);
				if (flag2)
				{
					flag = false;
				}
				if (this.CompileInformation.OnlineChange)
				{
					flag = (flag && this.\u0002());
				}
			}
			finally
			{
				languageModelMgr.OnAfterLocation(e);
			}
			return flag;
		}

		// Token: 0x0600386A RID: 14442 RVA: 0x000E7B5C File Offset: 0x000E5D5C
		private bool \u0002()
		{
			bool result = true;
			foreach (_ISignature isignature in this.ComconNew.GetAllSignaturesFlatEx().Cast<_ISignature>().Where(new Func<_ISignature, bool>(CompilerPhase3_Locator.<>c.<>9.\u0001)))
			{
				_ISignature isignature2 = this.ComconOld[isignature.Id];
				if (isignature2 != null && isignature.Size != isignature2.Size)
				{
					isignature.AddMessage(Severity.Error, MessageId.Err_NoOnlineChangeOnDynamicObjects, Array.Empty<object>());
					result = false;
				}
			}
			return result;
		}

		// Token: 0x0600386B RID: 14443 RVA: 0x000E7C0C File Offset: 0x000E5E0C
		private static void \u0001(LList<_IArea> \u0002, _IDataManager \u0003)
		{
			for (int i = 0; i < (int)\u0003.AreaCount; i++)
			{
				_IArea iarea = \u0003.GetArea(i).Duplicate();
				iarea.SetAreaFlag(AreaFlags.OnlineChange, false);
				\u0002.Add(iarea);
			}
		}

		// Token: 0x0600386C RID: 14444 RVA: 0x000E7C48 File Offset: 0x000E5E48
		private void \u0001(LList<_IArea> \u0002, _IMemorySettings \u0003)
		{
			if (\u0003.AdditionalAreas)
			{
				foreach (_IArea iarea in Locator.\u0001(this.ComconOld, \u0003, \u0002))
				{
					\u0002.Add(iarea);
				}
			}
		}

		// Token: 0x0600386D RID: 14445 RVA: 0x000E7CA4 File Offset: 0x000E5EA4
		private void \u0002(LList<_IArea> \u0002, _IMemorySettings \u0003)
		{
			if (\u0003.OnlineChangeInOwnSegment)
			{
				bool flag = true;
				foreach (_IArea iarea in \u0002)
				{
					if (iarea.GetDataSegmentFlag(DataSegmentFlags.Code))
					{
						if (this.ComconOld == null && flag)
						{
							flag = false;
						}
						else
						{
							iarea.SetAreaFlag(AreaFlags.NoUse, !iarea.GetAreaFlag(AreaFlags.NoUse));
						}
					}
				}
			}
		}

		// Token: 0x0600386E RID: 14446 RVA: 0x000E7D20 File Offset: 0x000E5F20
		private bool \u0003()
		{
			ILanguageModelList languageModelList = global::\u0019.\u0003.Builder.CreateLanguageModelList();
			IVarConfigCodeGenerator varConfigCodeGenerator = null;
			ITargetSettings targetSettings = this.ComconNew.GetTargetSettings();
			bool boolValue = global::\u0016.\u0004.DoPersistentCode.GetBoolValue(targetSettings);
			string stringValue = global::\u0016.\u0004.VarconfigGeneratorGuid.GetStringValue(this.ComconNew.GetTargetSettings());
			Guid guid = new Guid(stringValue);
			if (guid != Guid.Empty)
			{
				varConfigCodeGenerator = APEnvironmentFacade.Instance.TryCreateVarConfigCodeGenerator(guid);
			}
			if (varConfigCodeGenerator == null)
			{
				varConfigCodeGenerator = new VarConfigCodegenerator();
			}
			varConfigCodeGenerator.GenerateVarConfigCode(this.ComconNew.ApplicationGuid, languageModelList);
			if (this.ComconNew.RetainInCycle)
			{
				RetainCycleCodegenerator.DoGenerateRetainInFBCode(this.ComconNew.ApplicationGuid, languageModelList);
			}
			if (boolValue)
			{
				PersistentCodegenerator.DoGeneratePersistentInFBCode(this.ComconNew.ApplicationGuid, languageModelList);
			}
			return new global::\u0017.\u0007(this.ComconNew, languageModelList, this.ComconOld).\u0001();
		}

		// Token: 0x0600386F RID: 14447 RVA: 0x000E7DF4 File Offset: 0x000E5FF4
		private void \u0001()
		{
			foreach (_ISignature isignature in this.ComconNew.POUSignatures)
			{
				if (isignature.POUType == Operator.FunctionBlock || isignature.POUType == Operator.Interface)
				{
					isignature.CreateVirtualFunctionTable(this.ComconNew);
				}
			}
		}

		// Token: 0x06003870 RID: 14448 RVA: 0x000E7E60 File Offset: 0x000E6060
		private void \u0002()
		{
			if (this.CompileInformation.ComconOld == null)
			{
				this.CompileInformation.InterfacesChanged = true;
				this.CompileInformation.CodeChanged = true;
				this.CompileInformation.InitValuesChanged = true;
				return;
			}
			if (!this.CompileInformation.ComconOld.LibraryParamTablesEqual(this.CompileInformation.Precomp))
			{
				this.CompileInformation.InitValuesChanged = true;
			}
			foreach (_ISignature isignature in this.CompileInformation.ComconNew.AllFlat)
			{
				_ISignature isignature2 = this.CompileInformation.ComconOld[isignature.Id];
				if (isignature2 == null)
				{
					this.CompileInformation.InterfacesChanged = true;
				}
				else if (isignature2.Checksum != 0U)
				{
					if (isignature2.Checksum == isignature.Checksum)
					{
						isignature2.ObjectGuid = isignature.ObjectGuid;
					}
					else if (isignature2.ChecksumNoInit == isignature.ChecksumNoInit)
					{
						this.CompileInformation.CodeChanged = true;
						this.CompileInformation.InitValuesChanged = true;
					}
					else
					{
						this.CompileInformation.InterfacesChanged = true;
					}
				}
				else if (isignature2.IsEqualCompile(isignature, true))
				{
					isignature2.TimeStamp = isignature.TimeStamp;
					isignature2.ObjectGuid = isignature.ObjectGuid;
				}
				else if (isignature2.IsEqualCompile(isignature, false))
				{
					this.CompileInformation.CodeChanged = true;
				}
				else
				{
					this.CompileInformation.InterfacesChanged = true;
				}
			}
			foreach (_ISignature isignature3 in this.CompileInformation.ComconOld.AllFlat)
			{
				if (!(isignature3.ObjectGuid == Guid.Empty) && !isignature3.GetFlag(SignatureFlag.Generated) && this.CompileInformation.ComconNew[isignature3.Id] == null)
				{
					this.CompileInformation.InterfacesChanged = true;
					break;
				}
			}
		}

		// Token: 0x04000B3D RID: 2877
		[CompilerGenerated]
		private global::\u000E.\u001B \u0001;
	}
}
