using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.Legacy
{
	// Token: 0x02000276 RID: 630
	internal class CompileContextCreator
	{
		// Token: 0x17000BE0 RID: 3040
		// (get) Token: 0x06002A4F RID: 10831 RVA: 0x0006BD5F File Offset: 0x0006AD5F
		private _IPreCompileContext4 Precomp { get; }

		// Token: 0x06002A50 RID: 10832 RVA: 0x0006BD67 File Offset: 0x0006AD67
		internal CompileContextCreator(_IPreCompileContext4 precomp)
		{
			this.Precomp = precomp;
		}

		// Token: 0x06002A51 RID: 10833 RVA: 0x0006BD78 File Offset: 0x0006AD78
		public _ICompileContext CreateCompiledContext(_ICompileContext comconOld, _ICompileContext comconParent, bool bLinkAll, out IList<_ICompilerMessage> errors)
		{
			_ICompileContext icompileContext = this.Precomp.CreateEmptyCompiledContext(comconOld, comconParent, bLinkAll, out errors);
			this.InsertCompileContext(icompileContext as CompileContext, comconOld as CompileContext, bLinkAll);
			return icompileContext;
		}

		// Token: 0x06002A52 RID: 10834 RVA: 0x0006BDAC File Offset: 0x0006ADAC
		private void InsertCompileContext(CompileContext comcon, CompileContext comconOld, bool bLinkAll)
		{
			ITargetSettings targetSettings = comcon.GetTargetSettings();
			IDeviceIdentification deviceIdentification = comcon.GetDeviceIdentification();
			this.AddDefinesToCompileContext(comcon);
			CompileContextCreator.InsertTopLevelPOUs(comcon, comconOld, this.Precomp, this.Precomp.ApplicationGuid == Guid.Empty || bLinkAll);
			bool flag = APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34100 && this.Precomp.ApplicationGuid != Guid.Empty;
			bool flag2 = APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35510 && this.Precomp.IsDefined(CompileAttributes.ATTRIBUTE_POOL_IGNORE_TOPLEVEL_POUS);
			if (flag && !flag2)
			{
				CompileContextCreator.InsertTopLevelPOUs(comcon, comconOld, APEnvironmentFacade.Instance.LanguageModelMgr.Pool, false);
			}
			Dictionary<string, bool> dicLibs = new Dictionary<string, bool>();
			_IPreCompileContext[] array;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300)
			{
				array = comcon._LibraryTable.GetVisibleLibraries(this.Precomp).ToArray<_IPreCompileContext>();
			}
			else
			{
				array = (this.Precomp.LibraryContexts as _IPreCompileContext[]);
			}
			foreach (_IPreCompileContext ipreCompileContext in array)
			{
				string stNamespace;
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300)
				{
					stNamespace = comcon._LibraryTable.GetNamespaceOfLibrary(this.Precomp, ipreCompileContext.LibraryPath);
				}
				else
				{
					stNamespace = this.Precomp.GetNameOfLibrary(ipreCompileContext, comcon);
				}
				comcon.AddLibrary(ipreCompileContext, comconOld, stNamespace, false, false);
				this.AddLibraryContextToCompileContext0(comcon, comconOld, ipreCompileContext, dicLibs);
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35500 && !comcon.MinimalSystem)
			{
				CompileContextCreator.InsertTopLevelPOUs(comcon, comconOld, APEnvironmentFacade.Instance.LanguageModelMgr._SystemContext, false);
			}
			else if ((APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34200 || (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33250 && !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34000)) && !this.Precomp.MinimalSystem)
			{
				_IPreCompileContext systemContext = APEnvironmentFacade.Instance.LanguageModelMgr._SystemContext;
				this.AddLibraryContextToCompileContext0(comcon, comconOld, systemContext, dicLibs);
			}
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300)
			{
				foreach (_ILibraryPlaceholder ilibraryPlaceholder in this.Precomp.Placeholders)
				{
					_IPreCompileContext ipreCompileContext2 = this.Precomp.ResolveLibraryPlaceholder(comcon.GetTargetSettings(), comcon.ApplicationGuid, ilibraryPlaceholder, comcon.GetDeviceIdentification());
					if (ipreCompileContext2 != null)
					{
						comcon.AddLibrary(ipreCompileContext2, comconOld, ((LibraryPlaceholder)ilibraryPlaceholder).m_stNamespace, false, false);
						this.AddLibraryContextToCompileContext0(comcon, comconOld, ipreCompileContext2, dicLibs);
					}
				}
			}
			ICodegenerator codegenerator = CompilerProxy.CreateCodegenerator(APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceOfApplication(this.Precomp.ApplicationGuid), this.Precomp.ApplicationGuid, comcon.SimulationMode, false);
			if (codegenerator != null)
			{
				_IPreCompileContext pool = APEnvironmentFacade.Instance.LanguageModelMgr.Pool;
				string[] functionsToLinkAlways = codegenerator.FunctionsToLinkAlways;
				this.AddImplicitlyNeededFunctions(functionsToLinkAlways, comcon, comconOld, pool, (CompiledPOUFlags)0);
			}
			if (APEnvironmentFacade.Instance.LMServiceProvider.ConfigurationService.ExecutionpointLoggingEnabled(comcon.ApplicationGuid))
			{
				_IPreCompileContext pool2 = APEnvironmentFacade.Instance.LanguageModelMgr.Pool;
				LList<string> llist = new LList<string>();
				llist.Add(CGConstants.any32_to_string);
				llist.Add(CGConstants.new_real32_to_string);
				if (comcon.TypeIsSupported(TypeClass.LInt))
				{
					llist.Add(CGConstants.any64_to_string);
				}
				if (comcon.TypeIsSupported(TypeClass.LReal))
				{
					llist.Add(CGConstants.real64_to_string);
				}
				this.AddImplicitlyNeededFunctions(llist.ToArray(), comcon, comconOld, pool2, CompiledPOUFlags.TopLevel);
			}
			bool flag3;
			comcon.ParameterTableChecksum = this.Precomp.CalculateParameterTableChecksum(out flag3);
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351300)
			{
				_ILibraryTable ilibraryTable = this.Precomp._GetLibraryTable(this.Precomp.ApplicationGuid);
				comcon.LibCheckSum = ilibraryTable.CalculateChecksum(this.Precomp);
				return;
			}
			comcon.LibCheckSum = this.Precomp.CalculateLibChecksum(targetSettings, comcon.ApplicationGuid, deviceIdentification);
			comcon.PoolLibCheckSum = APEnvironmentFacade.Instance.LanguageModelMgr.Pool.CalculateLibChecksum(targetSettings, comcon.ApplicationGuid, deviceIdentification);
		}

		// Token: 0x06002A53 RID: 10835 RVA: 0x0006C19C File Offset: 0x0006B19C
		private void AddDefinesToCompileContext(CompileContext comcon)
		{
			if (this.Precomp.TargetDefineTable != null)
			{
				string[] array = new string[this.Precomp.TargetDefineTable.Keys.Count];
				this.Precomp.TargetDefineTable.Keys.CopyTo(array, 0);
				foreach (string text in array)
				{
					comcon.Define(text, this.Precomp.TargetDefineTable[text] as string, true);
				}
			}
			if (this.Precomp.DefineTable != null)
			{
				string[] array3 = new string[this.Precomp.DefineTable.Keys.Count];
				this.Precomp.DefineTable.Keys.CopyTo(array3, 0);
				foreach (string text2 in array3)
				{
					comcon.Define(text2, this.Precomp.DefineTable[text2] as string, true);
				}
			}
		}

		// Token: 0x06002A54 RID: 10836 RVA: 0x0006C294 File Offset: 0x0006B294
		private void AddImplicitlyNeededFunctions(string[] stFunctionsToLink, CompileContext comcon, CompileContext comconOld, _IPreCompileContext precomPool, CompiledPOUFlags flags)
		{
			foreach (string stName in stFunctionsToLink)
			{
				_ISignature isignature = precomPool[stName];
				if (isignature != null)
				{
					_ISignature isignature2 = comcon.AddCompiledSignature(isignature, isignature.GetSearchName(comcon), precomPool, comconOld, false);
					_ICompiledPOU pou = precomPool.GetPOU(isignature.ObjectGuid);
					if (pou != null)
					{
						pou.SetFlag(flags, true);
						comcon.AddCompiledPOU(pou.CreateCompiledPOU(), isignature2, comconOld);
					}
					foreach (_ISignature isignature3 in isignature2.GetSubSignatures())
					{
						if (!(isignature3.ObjectGuid == Guid.Empty))
						{
							pou = precomPool.GetPOU(isignature3.ObjectGuid);
							if (pou != null)
							{
								pou.SetFlag(flags, true);
								comcon.AddCompiledPOU(pou.CreateCompiledPOU(), isignature3, comconOld);
							}
						}
					}
				}
			}
		}

		// Token: 0x06002A55 RID: 10837 RVA: 0x0006C36C File Offset: 0x0006B36C
		private static void InsertTopLevelPOUs(CompileContext comcon, CompileContext comconOld, _IPreCompileContext precom, bool bAll)
		{
			foreach (_ISignature isignature in precom._AllSignatures)
			{
				if (isignature.GetFlag(SignatureFlag.TopLevel) || bAll)
				{
					_ISignature isignature2 = comcon.AddCompiledSignature(isignature, isignature.GetSearchName(comcon), precom, comconOld, false);
					_ICompiledPOU pou = precom.GetPOU(isignature.ObjectGuid);
					if (pou != null)
					{
						comcon.AddCompiledPOU(pou.CreateCompiledPOU(), isignature2, comconOld);
					}
					CompileContextCreator.AddSubPOUs(comcon, comconOld, precom, isignature2);
				}
			}
		}

		// Token: 0x06002A56 RID: 10838 RVA: 0x0006C3FC File Offset: 0x0006B3FC
		private static void AddSubPOUs(CompileContext comcon, CompileContext comconOld, _IPreCompileContext precom, _ISignature signCompiled)
		{
			foreach (_ISignature isignature in signCompiled.GetSubSignatures())
			{
				if (!(isignature.ObjectGuid == Guid.Empty))
				{
					_ICompiledPOU pou = precom.GetPOU(isignature.ObjectGuid);
					if (pou != null)
					{
						comcon.AddCompiledPOU(pou.CreateCompiledPOU(), isignature, comconOld);
					}
				}
			}
		}

		// Token: 0x06002A57 RID: 10839 RVA: 0x0006C452 File Offset: 0x0006B452
		private void AddLibraryContextToCompileContext0(CompileContext comcon, CompileContext comconOld, _IPreCompileContext precom, Dictionary<string, bool> dicLibs)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34200)
			{
				this.AddLibraryContextToCompileContext(comcon, comconOld, precom, dicLibs);
				return;
			}
			this.AddLibraryContextToCompileContext(comcon, comconOld, precom, new Dictionary<string, bool>());
		}

		// Token: 0x06002A58 RID: 10840 RVA: 0x0006C480 File Offset: 0x0006B480
		private void AddLibraryContextToCompileContext(CompileContext comcon, CompileContext comconOld, _IPreCompileContext precom, Dictionary<string, bool> dicLibs)
		{
			bool flag = dicLibs.ContainsKey(precom.LibraryPath) && !dicLibs[precom.LibraryPath] && precom.LinkAll && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34300;
			if (dicLibs.ContainsKey(precom.LibraryPath) && !flag)
			{
				return;
			}
			bool flag2 = precom.LinkAll;
			if (this.Precomp.DeviceApplication && (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35900 || !precom.IgnoreLinkAll))
			{
				flag2 = true;
			}
			dicLibs[precom.LibraryPath] = flag2;
			APEnvironmentFacade.Instance.LanguageModelMgr.LateLoadLibraryPreCompileContext(precom as PreCompileContext);
			IScope5 scope = comcon.GlobalScope();
			foreach (_ISignature isignature in precom._AllSignatures)
			{
				bool flag3 = flag2 && (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35900 || !isignature.HasAttribute("ignore_link_all"));
				if (isignature.GetFlag(SignatureFlag.TopLevel) || flag3 || (this.Precomp.ApplicationGuid == Guid.Empty && !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34400))
				{
					string stName = string.Empty;
					if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35600 && comcon.LibraryIsUnique(precom))
					{
						stName = string.Format("{0}.{1}", LibraryHelper.VersionFreeLibraryPath(precom.LibraryPath), isignature.OrgName);
					}
					else
					{
						stName = string.Format("{0}.{1}", precom.LibraryPath, isignature.OrgName);
						if ((APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34200 || (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33250 && !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34000)) && string.IsNullOrEmpty(precom.LibraryPath))
						{
							stName = isignature.OrgName;
						}
					}
					if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33120)
					{
						if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35350)
						{
							if (scope[stName] != null)
							{
								continue;
							}
						}
						else if (comcon[stName] != null)
						{
							continue;
						}
					}
					_ISignature isignature2 = comcon.AddCompiledSignature(isignature, stName, precom, comconOld, false);
					_ICompiledPOU pou = precom.GetPOU(isignature.ObjectGuid);
					if (pou != null)
					{
						comcon.AddCompiledPOU(pou.CreateCompiledPOU(), isignature2, comconOld);
					}
					foreach (_ISignature isignature3 in isignature2.GetSubSignatures())
					{
						if (!(isignature3.ObjectGuid == Guid.Empty))
						{
							pou = precom.GetPOU(isignature3.ObjectGuid);
							if (pou != null)
							{
								comcon.AddCompiledPOU(pou.CreateCompiledPOU(), isignature3, comconOld);
							}
						}
					}
				}
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33120)
			{
				_IPreCompileContext[] array;
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300)
				{
					array = comcon._LibraryTable.GetVisibleLibraries(precom).ToArray<_IPreCompileContext>();
				}
				else
				{
					array = (precom.LibraryContexts as _IPreCompileContext[]);
				}
				foreach (_IPreCompileContext precom2 in array)
				{
					this.AddLibraryContextToCompileContext(comcon, comconOld, precom2, dicLibs);
				}
				foreach (_ILibraryPlaceholder placeholder in precom.Placeholders)
				{
					_IPreCompileContext ipreCompileContext = this.Precomp.ResolveLibraryPlaceholder(comcon.GetTargetSettings(), comcon.ApplicationGuid, placeholder, comcon.GetDeviceIdentification());
					if (ipreCompileContext != null)
					{
						this.AddLibraryContextToCompileContext(comcon, comconOld, ipreCompileContext, dicLibs);
					}
				}
			}
		}
	}
}
