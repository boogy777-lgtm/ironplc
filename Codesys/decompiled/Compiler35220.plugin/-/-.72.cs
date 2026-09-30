using System;
using System.Runtime.CompilerServices;
using \u0007;
using \u0012;
using \u0019;
using \u001C;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0002
{
	// Token: 0x020000FE RID: 254
	internal sealed class \u0003 : IParseTreeProvider
	{
		// Token: 0x170004AB RID: 1195
		// (get) Token: 0x060012A6 RID: 4774 RVA: 0x00033388 File Offset: 0x00031588
		private _ICompiledPOU2 CompiledPOU { get; }

		// Token: 0x170004AC RID: 1196
		// (get) Token: 0x060012A7 RID: 4775 RVA: 0x00033390 File Offset: 0x00031590
		private IGreenTreeConverter GreenTreeConverter { get; }

		// Token: 0x170004AD RID: 1197
		// (get) Token: 0x060012A8 RID: 4776 RVA: 0x00033398 File Offset: 0x00031598
		private ITreeFactory TreeFactory { get; }

		// Token: 0x170004AE RID: 1198
		// (get) Token: 0x060012A9 RID: 4777 RVA: 0x000333A0 File Offset: 0x000315A0
		private ICompiledPOUWithParseTreeProvider CompiledPOUWithSpecials
		{
			get
			{
				return this.CompiledPOU as ICompiledPOUWithParseTreeProvider;
			}
		}

		// Token: 0x170004AF RID: 1199
		// (get) Token: 0x060012AA RID: 4778 RVA: 0x000333B0 File Offset: 0x000315B0
		// (set) Token: 0x060012AB RID: 4779 RVA: 0x000333B8 File Offset: 0x000315B8
		public ILMCompiledParseTreeService CompiledParseTreeService
		{
			get
			{
				return this.\u0001;
			}
			set
			{
				WeakReference<_IStatement> obj = this.RedParseTree;
				lock (obj)
				{
					this.\u0001 = value;
					this.RedParseTree.SetTarget(null);
				}
			}
		}

		// Token: 0x170004B0 RID: 1200
		// (get) Token: 0x060012AC RID: 4780 RVA: 0x00033408 File Offset: 0x00031608
		internal WeakReference<_IStatement> RedParseTree { get; } = new WeakReference<_IStatement>(null);

		// Token: 0x170004B1 RID: 1201
		// (get) Token: 0x060012AD RID: 4781 RVA: 0x00033410 File Offset: 0x00031610
		// (set) Token: 0x060012AE RID: 4782 RVA: 0x00033418 File Offset: 0x00031618
		public ICompactedParseTreeInformation CompactedParseTreeInformation
		{
			get
			{
				return this.\u0001;
			}
			set
			{
				WeakReference<_IStatement> obj = this.RedParseTree;
				lock (obj)
				{
					this.\u0001 = value;
					this.RedParseTree.SetTarget(null);
				}
			}
		}

		// Token: 0x060012AF RID: 4783 RVA: 0x00033468 File Offset: 0x00031668
		internal \u0003(_ICompiledPOU2 \u0012\u0002, IGreenTreeConverter \u0013\u0002, ITreeFactory \u0014\u0002)
		{
			this.CompiledPOU = \u0012\u0002;
			this.GreenTreeConverter = \u0013\u0002;
			this.TreeFactory = \u0014\u0002;
		}

		// Token: 0x060012B0 RID: 4784 RVA: 0x00033494 File Offset: 0x00031694
		public _IStatement \u0001()
		{
			if (this.CompiledPOUWithSpecials.ParseTreeRaw is _IEmptyStatement && !this.CompiledPOU.GetFlag(CompiledPOUFlags.Compiled) && !string.IsNullOrEmpty(this.CompiledPOU.LibraryPath))
			{
				_IStatement istatement = \u001C.\u0004.\u0001(this.CompiledPOU);
				if (istatement != null)
				{
					return istatement;
				}
			}
			if (this.CompiledPOUWithSpecials.ParseTreeRaw is IGreenTreeExprement)
			{
				WeakReference<_IStatement> obj = this.RedParseTree;
				lock (obj)
				{
					_IStatement istatement;
					if (this.RedParseTree.TryGetTarget(out istatement))
					{
						return istatement;
					}
					_IStatement istatement2;
					this.\u0001(this.CompiledPOUWithSpecials.ParseTreeRaw, this.CompactedParseTreeInformation, out istatement2);
					if (this.CompiledParseTreeService != null)
					{
						this.CompiledParseTreeService.TypeCheckTemporaryParseTree(this.CompiledPOU, istatement2 as ISequenceStatement3);
					}
					this.RedParseTree.SetTarget(istatement2);
					return istatement2;
				}
			}
			if (this.CompiledPOUWithSpecials.ParseTreeRaw == null && this.CompiledPOU.GetFlag(CompiledPOUFlags.Compiled))
			{
				return this.\u0002();
			}
			return this.CompiledPOUWithSpecials.ParseTreeRaw;
		}

		// Token: 0x060012B1 RID: 4785 RVA: 0x000335BC File Offset: 0x000317BC
		private _IStatement \u0002()
		{
			IPreCompileContext preCompileContext = null;
			IPreCompileContext preCompileContext2 = null;
			_ICompileContext icompileContext = null;
			if (this.CompiledPOU.LibraryPath != null)
			{
				preCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(this.CompiledPOU.LibraryPath);
			}
			foreach (ILMCompiledApplicationSet ilmcompiledApplicationSet in APEnvironmentFacade.Instance.LMServiceProvider.CompileService.CompiledApplicationSets)
			{
				if (ilmcompiledApplicationSet != null && ilmcompiledApplicationSet.GetCompiledPOUById(this.CompiledPOU.SignatureId) == this.CompiledPOU)
				{
					preCompileContext2 = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(ilmcompiledApplicationSet.ApplicationGuid);
					icompileContext = (_ICompileContext)ilmcompiledApplicationSet;
					break;
				}
			}
			IPreCompileContext precompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(Guid.Empty);
			if (icompileContext == null)
			{
				return this.CompiledPOUWithSpecials.ParseTreeRaw;
			}
			_ICompiledPOU icompiledPOU = null;
			if (preCompileContext != null)
			{
				icompiledPOU = (preCompileContext.GetCompiledPOU(this.CompiledPOU.ObjectGuid) as _ICompiledPOU);
			}
			if (icompiledPOU == null && precompileContext != null)
			{
				icompiledPOU = (precompileContext.GetCompiledPOU(this.CompiledPOU.ObjectGuid) as _ICompiledPOU);
			}
			if (icompiledPOU == null && preCompileContext2 != null)
			{
				icompiledPOU = (preCompileContext2.GetCompiledPOU(this.CompiledPOU.ObjectGuid) as _ICompiledPOU);
			}
			if (icompiledPOU == null)
			{
				return this.CompiledPOUWithSpecials.ParseTreeRaw;
			}
			_IStatement istatement = icompiledPOU.GetParseTree().Duplicate() as _IStatement;
			IScope5 u = global::\u0007.\u0005.\u0001(icompileContext, this.CompiledPOU.SignatureId);
			\u0019.\u0001.\u0001(istatement, u, icompileContext, icompiledPOU);
			if (APEnvironmentFacade.Instance.LanguageModelMgr.CompilationInProgress)
			{
				this.\u0001(istatement);
			}
			return istatement;
		}

		// Token: 0x060012B2 RID: 4786 RVA: 0x00033760 File Offset: 0x00031960
		public _IStatement \u0003()
		{
			if (this.CompiledPOUWithSpecials.ParseTreeRaw is _IEmptyStatement && !this.CompiledPOU.GetFlag(CompiledPOUFlags.Compiled) && !string.IsNullOrEmpty(this.CompiledPOU.LibraryPath))
			{
				_IStatement istatement = \u001C.\u0004.\u0001(this.CompiledPOU);
				if (istatement != null)
				{
					return istatement;
				}
			}
			WeakReference<_IStatement> obj = this.RedParseTree;
			lock (obj)
			{
				_IStatement result = null;
				if (this.RedParseTree.TryGetTarget(out result))
				{
					return result;
				}
				if (this.CompiledPOUWithSpecials.ParseTreeRaw != null)
				{
					_IStatement result2;
					this.\u0001(this.CompiledPOUWithSpecials.ParseTreeRaw, this.CompactedParseTreeInformation, out result2);
					this.RedParseTree.SetTarget(null);
					return result2;
				}
			}
			return this.CompiledPOUWithSpecials.ParseTreeRaw;
		}

		// Token: 0x060012B3 RID: 4787 RVA: 0x00033840 File Offset: 0x00031A40
		public void \u0001(_ICompiledPOU \u0002)
		{
			ICompiledPOUWithParseTreeProvider compiledPOUWithParseTreeProvider = \u0002 as ICompiledPOUWithParseTreeProvider;
			ICompiledPOUWithCompactedParseTree compiledPOUWithCompactedParseTree = \u0002 as ICompiledPOUWithCompactedParseTree;
			if (this.CompiledPOUWithSpecials.ParseTreeRaw is _IEmptyStatement)
			{
				compiledPOUWithParseTreeProvider.ParseTreeProvider.SetParseTreeDirectly(this.CompiledPOUWithSpecials.ParseTreeRaw);
			}
			else if (this.CompiledPOUWithSpecials.ParseTreeRaw == null)
			{
				compiledPOUWithParseTreeProvider.ParseTreeRaw = this.\u0001();
			}
			else
			{
				compiledPOUWithParseTreeProvider.ParseTreeRaw = this.CompiledPOUWithSpecials.ParseTreeRaw;
			}
			compiledPOUWithCompactedParseTree.CompactedParseTreeInformation = this.CompactedParseTreeInformation;
		}

		// Token: 0x060012B4 RID: 4788 RVA: 0x000338BC File Offset: 0x00031ABC
		public void \u0001(_IStatement \u0002)
		{
			this.\u0002(\u0002);
			if (\u0002 == null)
			{
				this.CompiledPOU.SetFlag(CompiledPOUFlags.ContainsNoParseTree, true);
			}
			WeakReference<_IStatement> obj = this.RedParseTree;
			lock (obj)
			{
				this.RedParseTree.SetTarget(null);
				this.CompactedParseTreeInformation = null;
			}
		}

		// Token: 0x060012B5 RID: 4789 RVA: 0x00033924 File Offset: 0x00031B24
		public void \u0002(_IStatement \u0002)
		{
			this.CompiledPOUWithSpecials.ParseTreeRaw = \u0002;
		}

		// Token: 0x060012B6 RID: 4790 RVA: 0x00033934 File Offset: 0x00031B34
		public void \u0001()
		{
			if (this.CompiledPOU.GetFlagInternal(InternalCompiledPOUFlags.SkipParseTreeDuplication))
			{
				return;
			}
			if (this.CompiledPOUWithSpecials.ParseTreeRaw is IGreenTreeExprement)
			{
				_IStatement parseTreeRaw;
				this.\u0001(this.CompiledPOUWithSpecials.ParseTreeRaw, this.CompactedParseTreeInformation, out parseTreeRaw);
				this.CompiledPOUWithSpecials.ParseTreeRaw = parseTreeRaw;
			}
			else
			{
				this.CompiledPOUWithSpecials.ParseTreeRaw = (this.\u0001().Duplicate() as _IStatement);
			}
			this.CompiledPOU.SetFlagInternal(InternalCompiledPOUFlags.SkipParseTreeDuplication, true);
		}

		// Token: 0x060012B7 RID: 4791 RVA: 0x000339BC File Offset: 0x00031BBC
		public _IStatement \u0001(bool \u0002)
		{
			_IStatement istatement = this.CompiledPOUWithSpecials.ParseTreeRaw;
			bool flag = false;
			if (this.CompiledPOUWithSpecials.ParseTreeRaw is IGreenTreeExprement)
			{
				this.\u0001(this.CompiledPOUWithSpecials.ParseTreeRaw, this.CompactedParseTreeInformation, out istatement);
				flag = true;
			}
			if (\u0002)
			{
				if (!flag)
				{
					istatement = (this.CompiledPOUWithSpecials.ParseTreeRaw.Duplicate() as _IStatement);
				}
				global::\u0012.\u0004.\u0001(istatement);
			}
			return istatement;
		}

		// Token: 0x060012B8 RID: 4792 RVA: 0x00033A28 File Offset: 0x00031C28
		private void \u0001(_IStatement \u0002, ICompactedParseTreeInformation \u0003, out _IStatement \u0004)
		{
			\u0004 = \u0002;
			if (\u0002 == null)
			{
				return;
			}
			if (\u0002 is _IEmptyStatement)
			{
				return;
			}
			if (!(\u0002 is IGreenTreeExprement))
			{
				return;
			}
			Debug.\u0001(\u0002 is _ISequenceStatement);
			Debug.\u0001(\u0002 is IGreenTreeExprement);
			\u0004 = (this.GreenTreeConverter.BuildRedTree(\u0002, this.TreeFactory, \u0003) as _IStatement);
		}

		// Token: 0x0400031E RID: 798
		[CompilerGenerated]
		private readonly _ICompiledPOU2 \u0001;

		// Token: 0x0400031F RID: 799
		[CompilerGenerated]
		private readonly IGreenTreeConverter \u0001;

		// Token: 0x04000320 RID: 800
		[CompilerGenerated]
		private readonly ITreeFactory \u0001;

		// Token: 0x04000321 RID: 801
		private ILMCompiledParseTreeService \u0001;

		// Token: 0x04000322 RID: 802
		[CompilerGenerated]
		private readonly WeakReference<_IStatement> \u0001;

		// Token: 0x04000323 RID: 803
		private ICompactedParseTreeInformation \u0001;
	}
}
