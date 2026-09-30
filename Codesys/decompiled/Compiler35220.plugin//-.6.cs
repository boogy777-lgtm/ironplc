using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u000E;
using \u0012;
using \u0014;
using \u0016;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0081
{
	// Token: 0x02000141 RID: 321
	internal sealed class \u0006
	{
		// Token: 0x1700052B RID: 1323
		// (get) Token: 0x06001613 RID: 5651 RVA: 0x00041590 File Offset: 0x0003F790
		// (set) Token: 0x06001614 RID: 5652 RVA: 0x00041598 File Offset: 0x0003F798
		private \u001B CompileInformation { get; set; }

		// Token: 0x1700052C RID: 1324
		// (get) Token: 0x06001615 RID: 5653 RVA: 0x000415A4 File Offset: 0x0003F7A4
		// (set) Token: 0x06001616 RID: 5654 RVA: 0x000415B4 File Offset: 0x0003F7B4
		private _ICompileContext ComconNew
		{
			get
			{
				return this.CompileInformation.ComconNew;
			}
			set
			{
				this.CompileInformation.ComconNew = value;
			}
		}

		// Token: 0x1700052D RID: 1325
		// (get) Token: 0x06001617 RID: 5655 RVA: 0x000415C4 File Offset: 0x0003F7C4
		private _ICompileContext ComconParent
		{
			get
			{
				return this.CompileInformation.ComconParent;
			}
		}

		// Token: 0x1700052E RID: 1326
		// (get) Token: 0x06001618 RID: 5656 RVA: 0x000415D4 File Offset: 0x0003F7D4
		private _ICompileContext ComconOld
		{
			get
			{
				return this.CompileInformation.ComconOld;
			}
		}

		// Token: 0x1700052F RID: 1327
		// (get) Token: 0x06001619 RID: 5657 RVA: 0x000415E4 File Offset: 0x0003F7E4
		private _IPreCompileContext4 Precomp
		{
			get
			{
				return this.CompileInformation.Precomp as _IPreCompileContext4;
			}
		}

		// Token: 0x17000530 RID: 1328
		// (get) Token: 0x0600161A RID: 5658 RVA: 0x000415F8 File Offset: 0x0003F7F8
		private _IPreCompileContext PrecompPool
		{
			get
			{
				return this.CompileInformation.PrecompPool;
			}
		}

		// Token: 0x17000531 RID: 1329
		// (get) Token: 0x0600161B RID: 5659 RVA: 0x00041608 File Offset: 0x0003F808
		// (set) Token: 0x0600161C RID: 5660 RVA: 0x00041610 File Offset: 0x0003F810
		public global::\u0012.\u000E CompiledSignatureCreator { get; set; }

		// Token: 0x0600161D RID: 5661 RVA: 0x0004161C File Offset: 0x0003F81C
		internal \u0006(\u001B \u008F\u0004)
		{
			this.CompileInformation = \u008F\u0004;
		}

		// Token: 0x0600161E RID: 5662 RVA: 0x0004162C File Offset: 0x0003F82C
		public void \u0001(bool \u0002, out IList<_ICompilerMessage> \u0003)
		{
			this.ComconNew = this.Precomp.CreateEmptyCompiledContext(this.ComconOld, this.ComconParent, \u0002, out \u0003);
			this.CompiledSignatureCreator = new global::\u0012.\u000E(this.ComconNew, this.ComconOld, this.Precomp);
			this.\u0001(\u0002);
		}

		// Token: 0x0600161F RID: 5663 RVA: 0x0004167C File Offset: 0x0003F87C
		private void \u0001()
		{
			if (this.Precomp.TargetDefineTable != null)
			{
				string[] array = new string[this.Precomp.TargetDefineTable.Keys.Count];
				this.Precomp.TargetDefineTable.Keys.CopyTo(array, 0);
				foreach (string text in array)
				{
					this.ComconNew.Define(text, this.Precomp.TargetDefineTable[text] as string, true);
				}
			}
			if (this.Precomp.DefineTable != null)
			{
				string[] array3 = new string[this.Precomp.DefineTable.Keys.Count];
				this.Precomp.DefineTable.Keys.CopyTo(array3, 0);
				foreach (string text2 in array3)
				{
					this.ComconNew.Define(text2, this.Precomp.DefineTable[text2] as string, true);
				}
			}
		}

		// Token: 0x06001620 RID: 5664 RVA: 0x0004177C File Offset: 0x0003F97C
		private void \u0001(bool \u0002)
		{
			this.\u0001();
			this.\u0001(this.Precomp, this.Precomp.ApplicationGuid == Guid.Empty || \u0002);
			bool flag = this.Precomp.ApplicationGuid != Guid.Empty;
			bool flag2 = this.Precomp.IsDefined(CompileAttributes.ATTRIBUTE_POOL_IGNORE_TOPLEVEL_POUS);
			if (flag && !flag2)
			{
				this.\u0001(APEnvironmentFacade.Instance.LanguageModelMgr.Pool, false);
			}
			Dictionary<string, bool> u = new Dictionary<string, bool>();
			IPreCompileContext[] source = this.ComconNew._LibraryTable.GetVisibleLibraries(this.Precomp).ToArray<_IPreCompileContext>();
			foreach (_IPreCompileContext ipreCompileContext in source.OfType<_IPreCompileContext>())
			{
				string namespaceOfLibrary = this.ComconNew._LibraryTable.GetNamespaceOfLibrary(this.Precomp, ipreCompileContext.LibraryPath);
				this.ComconNew.AddLibrary(ipreCompileContext, this.ComconOld, namespaceOfLibrary, false, false);
				this.\u0001(ipreCompileContext, u);
			}
			if (!this.ComconNew.MinimalSystem)
			{
				this.\u0001(APEnvironmentFacade.Instance.LanguageModelMgr._SystemContext, false);
			}
			else if (!this.Precomp.MinimalSystem)
			{
				_IPreCompileContext systemContext = APEnvironmentFacade.Instance.LanguageModelMgr._SystemContext;
				this.\u0001(systemContext, u);
			}
			ICodegenerator codegenerator = CompilerServicesInternal.\u0001(APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceOfApplication(this.Precomp.ApplicationGuid), this.Precomp.ApplicationGuid, this.ComconNew.SimulationMode, false);
			if (codegenerator != null)
			{
				string[] functionsToLinkAlways = codegenerator.FunctionsToLinkAlways;
				this.\u0001(functionsToLinkAlways, (CompiledPOUFlags)0);
			}
			if (APEnvironmentFacade.Instance.LMServiceProvider.ConfigurationService.ExecutionpointLoggingEnabled(this.ComconNew.ApplicationGuid))
			{
				LList<string> llist = new LList<string>();
				llist.Add(CGConstants.any32_to_string);
				llist.Add(CGConstants.new_real32_to_string);
				LList<string> llist2 = llist;
				if (this.ComconNew.TypeIsSupported(TypeClass.LInt))
				{
					llist2.Add(CGConstants.any64_to_string);
				}
				if (this.ComconNew.TypeIsSupported(TypeClass.LReal))
				{
					llist2.Add(CGConstants.real64_to_string);
				}
				this.\u0001(llist2.ToArray(), CompiledPOUFlags.TopLevel);
			}
			bool flag3;
			this.ComconNew.ParameterTableChecksum = this.Precomp.CalculateParameterTableChecksum(out flag3);
			_ILibraryTable ilibraryTable = this.Precomp._GetLibraryTable(this.Precomp.ApplicationGuid);
			this.ComconNew.LibCheckSum = ilibraryTable.CalculateChecksum(this.Precomp);
		}

		// Token: 0x06001621 RID: 5665 RVA: 0x000419FC File Offset: 0x0003FBFC
		private static bool \u0001(_ISignature \u0002, bool \u0003, bool \u0004)
		{
			if (\u0002.GetFlag(SignatureFlag.TopLevel) || \u0003)
			{
				return true;
			}
			if (!\u0004)
			{
				return false;
			}
			bool flag = \u0002.POUType == Operator.VarGlobal || \u0002.POUType == Operator.VarAccess || \u0002.POUType == Operator.VarConfig || \u0002.GetFlag(SignatureFlag.Enum);
			bool flag2 = \u0002.GetFlag(SignatureFlag.SuperGlobal);
			return flag && !flag2;
		}

		// Token: 0x06001622 RID: 5666 RVA: 0x00041A60 File Offset: 0x0003FC60
		private bool \u0001(_IPreCompileContext \u0002)
		{
			ITargetSettings targetSettings = \u0002.GetTargetSettings();
			return global::\u0016.\u0004.LinkAllGlobalVariables.GetBoolValue(targetSettings);
		}

		// Token: 0x06001623 RID: 5667 RVA: 0x00041A80 File Offset: 0x0003FC80
		private void \u0001(_IPreCompileContext \u0002, bool \u0003)
		{
			bool u = \u0002.ApplicationGuid != Guid.Empty && this.\u0001(\u0002);
			foreach (_ISignature u2 in \u0002._AllSignatures)
			{
				if (\u0081.\u0006.\u0001(u2, \u0003, u))
				{
					this.CompiledSignatureCreator.\u0001(u2, \u0002);
				}
			}
		}

		// Token: 0x06001624 RID: 5668 RVA: 0x00041AFC File Offset: 0x0003FCFC
		private void \u0001(_IPreCompileContext \u0002, IScope5 \u0003, bool \u0004)
		{
			foreach (_ISignature isignature in \u0002._AllSignatures)
			{
				bool flag = \u0004 && !isignature.HasAttribute("ignore_link_all");
				if (isignature.GetFlag(SignatureFlag.TopLevel) || flag)
				{
					string stName = string.Empty;
					if (this.ComconNew.LibraryIsUnique(\u0002))
					{
						stName = string.Format("{0}.{1}", global::\u0014.\u0002.\u0001(\u0002.LibraryPath), isignature.OrgName);
					}
					if (\u0003[stName] == null)
					{
						this.CompiledSignatureCreator.\u0001(isignature, \u0002);
					}
				}
			}
		}

		// Token: 0x06001625 RID: 5669 RVA: 0x00041BB0 File Offset: 0x0003FDB0
		private void \u0001(_IPreCompileContext \u0002, Dictionary<string, bool> \u0003)
		{
			bool flag = \u0003.ContainsKey(\u0002.LibraryPath) && !\u0003[\u0002.LibraryPath] && \u0002.LinkAll;
			if (\u0003.ContainsKey(\u0002.LibraryPath) && !flag)
			{
				return;
			}
			bool linkAll = \u0002.LinkAll;
			\u0003[\u0002.LibraryPath] = linkAll;
			APEnvironmentFacade.Instance.LanguageModelMgr.LateLoadLibraryPreCompileContext(\u0002);
			IScope5 u = this.ComconNew.CreateGlobalIScope() as IScope5;
			this.\u0001(\u0002, u, linkAll);
			IPreCompileContext[] source = this.ComconNew._LibraryTable.GetVisibleLibraries(\u0002).ToArray<_IPreCompileContext>();
			foreach (_IPreCompileContext u2 in source.OfType<_IPreCompileContext>())
			{
				this.\u0001(u2, \u0003);
			}
			foreach (_ILibraryPlaceholder placeholder in \u0002.Placeholders)
			{
				_IPreCompileContext ipreCompileContext = this.Precomp.ResolveLibraryPlaceholder(this.ComconNew.GetTargetSettings(), this.ComconNew.ApplicationGuid, placeholder, this.ComconNew.GetDeviceIdentification());
				if (ipreCompileContext != null)
				{
					this.\u0001(ipreCompileContext, \u0003);
				}
			}
		}

		// Token: 0x06001626 RID: 5670 RVA: 0x00041CF4 File Offset: 0x0003FEF4
		private void \u0001(string[] \u0002, CompiledPOUFlags \u0003)
		{
			foreach (string stName in \u0002)
			{
				_ISignature isignature = this.PrecompPool[stName];
				if (isignature != null)
				{
					this.\u0001(\u0003, isignature);
					this.CompiledSignatureCreator.\u0001(isignature, this.PrecompPool);
				}
			}
		}

		// Token: 0x06001627 RID: 5671 RVA: 0x00041D40 File Offset: 0x0003FF40
		private void \u0001(CompiledPOUFlags \u0002, _ISignature \u0003)
		{
			if (\u0003 != null)
			{
				_ICompiledPOU pou = this.PrecompPool.GetPOU(\u0003.ObjectGuid);
				if (pou != null)
				{
					pou.SetFlag(\u0002, true);
				}
				foreach (_ISignature isignature in this.PrecompPool._GetSubSignatures(\u0003.ObjectGuid))
				{
					if (!(isignature.ObjectGuid == Guid.Empty))
					{
						pou = this.PrecompPool.GetPOU(isignature.ObjectGuid);
						if (pou != null)
						{
							pou.SetFlag(\u0002, true);
						}
					}
				}
			}
		}

		// Token: 0x040003E1 RID: 993
		[CompilerGenerated]
		private \u001B \u0001;

		// Token: 0x040003E2 RID: 994
		[CompilerGenerated]
		private global::\u0012.\u000E \u0001;
	}
}
