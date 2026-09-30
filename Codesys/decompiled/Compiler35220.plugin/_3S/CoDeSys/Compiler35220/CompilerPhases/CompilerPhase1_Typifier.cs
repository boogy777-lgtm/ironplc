using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0001;
using \u0004;
using \u0007;
using \u000E;
using \u000F;
using \u0014;
using \u0019;
using \u001A;
using \u001E;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0080;
using \u0081;
using \u0084;

namespace _3S.CoDeSys.Compiler35220.CompilerPhases
{
	// Token: 0x020003F1 RID: 1009
	internal sealed class CompilerPhase1_Typifier
	{
		// Token: 0x17000938 RID: 2360
		// (get) Token: 0x060037F2 RID: 14322 RVA: 0x000E52D0 File Offset: 0x000E34D0
		// (set) Token: 0x060037F3 RID: 14323 RVA: 0x000E52D8 File Offset: 0x000E34D8
		private global::\u000E.\u001B CompileInformation { get; set; }

		// Token: 0x17000939 RID: 2361
		// (get) Token: 0x060037F4 RID: 14324 RVA: 0x000E52E4 File Offset: 0x000E34E4
		private _ICompileContext ComconNew
		{
			get
			{
				return this.CompileInformation.ComconNew;
			}
		}

		// Token: 0x1700093A RID: 2362
		// (get) Token: 0x060037F5 RID: 14325 RVA: 0x000E52F4 File Offset: 0x000E34F4
		private bool OnlineChange
		{
			get
			{
				return this.CompileInformation.OnlineChange;
			}
		}

		// Token: 0x1700093B RID: 2363
		// (get) Token: 0x060037F6 RID: 14326 RVA: 0x000E5304 File Offset: 0x000E3504
		private _IPreCompileContext Precomp
		{
			get
			{
				return this.CompileInformation.Precomp;
			}
		}

		// Token: 0x1700093C RID: 2364
		// (get) Token: 0x060037F7 RID: 14327 RVA: 0x000E5314 File Offset: 0x000E3514
		private _IPreCompileContext PrecompPool
		{
			get
			{
				return this.CompileInformation.PrecompPool;
			}
		}

		// Token: 0x1700093D RID: 2365
		// (get) Token: 0x060037F8 RID: 14328 RVA: 0x000E5324 File Offset: 0x000E3524
		private _ICompileContext ComconOld
		{
			get
			{
				return this.CompileInformation.ComconOld;
			}
		}

		// Token: 0x1700093E RID: 2366
		// (get) Token: 0x060037F9 RID: 14329 RVA: 0x000E5334 File Offset: 0x000E3534
		// (set) Token: 0x060037FA RID: 14330 RVA: 0x000E533C File Offset: 0x000E353C
		public bool ErrorsOccured { get; private set; }

		// Token: 0x1700093F RID: 2367
		// (get) Token: 0x060037FB RID: 14331 RVA: 0x000E5348 File Offset: 0x000E3548
		// (set) Token: 0x060037FC RID: 14332 RVA: 0x000E5350 File Offset: 0x000E3550
		private bool ErrorsInCode { get; set; }

		// Token: 0x17000940 RID: 2368
		// (get) Token: 0x060037FD RID: 14333 RVA: 0x000E535C File Offset: 0x000E355C
		private LDictionary<int, CompactedTypifiedParseTreeInformation> ParseTreeInformationTable { get; } = new LDictionary<int, CompactedTypifiedParseTreeInformation>();

		// Token: 0x17000941 RID: 2369
		// (get) Token: 0x060037FE RID: 14334 RVA: 0x000E5364 File Offset: 0x000E3564
		private global::\u000F.\u0015 TypifierLateParseTreeLoader { get; } = new global::\u000F.\u0015();

		// Token: 0x17000942 RID: 2370
		// (get) Token: 0x060037FF RID: 14335 RVA: 0x000E536C File Offset: 0x000E356C
		private CompilerPhase1_Typifier.\u0001 InternalTypifier { get; }

		// Token: 0x06003800 RID: 14336 RVA: 0x000E5374 File Offset: 0x000E3574
		internal CompilerPhase1_Typifier(global::\u000E.\u001B ci)
		{
			this.CompileInformation = ci;
			this.InternalTypifier = new CompilerPhase1_Typifier.\u0001(ci, this);
		}

		// Token: 0x06003801 RID: 14337 RVA: 0x000E53A8 File Offset: 0x000E35A8
		public void \u0001()
		{
			this.\u0002();
			this.ErrorsOccured = this.CompileInformation.CompilerPhase2_AfterTypification.\u0001(this.ErrorsOccured);
			if (!this.ErrorsInCode)
			{
				this.CompileInformation.CompilerPhase4_Typechecker = CompilerPhase4_Typechecker.\u0001(this.CompileInformation, this.ParseTreeInformationTable);
				if (this.ErrorsOccured)
				{
					this.CompileInformation.CompilerPhase4_Typechecker.\u0004(true);
				}
			}
		}

		// Token: 0x06003802 RID: 14338 RVA: 0x000E5414 File Offset: 0x000E3614
		private void \u0002()
		{
			bool flag = false;
			this.ErrorsInCode = false;
			this.InternalTypifier.\u0001();
			this.ComconNew.CompiledSymbolTables = new \u0084.\u0007(this.ComconNew);
			NameManglingService.\u0001(this.ComconNew);
			this.\u0007();
			this.\u0003();
			flag |= CompilerPhase1_Typifier.\u0001(this.ComconNew);
			this.ErrorsOccured = (flag || this.ErrorsInCode);
			DependencySorter dependencySorter = new DependencySorter();
			if (!dependencySorter.\u0001(this.ComconNew))
			{
				this.ErrorsOccured = true;
			}
			this.\u0005();
			this.\u0006();
			this.\u0004();
			this.\u0001(dependencySorter);
			flag |= CompilerPhase1_Typifier.\u0001(this.ComconNew);
			this.ErrorsOccured = (this.ErrorsOccured || flag);
		}

		// Token: 0x06003803 RID: 14339 RVA: 0x000E54D4 File Offset: 0x000E36D4
		private void \u0003()
		{
			if (this.ErrorsOccured)
			{
				return;
			}
			foreach (_ISignature isignature in this.ComconNew.AllSignatureList)
			{
				IScope5 scope = global::\u0007.\u0005.\u0001(this.ComconNew, isignature.Id);
				this.CompileInformation.InterfaceCompiler.\u0002(isignature, scope);
				foreach (object obj in isignature._SubSignatures)
				{
					_ISignature isignature2 = (_ISignature)obj;
					scope.MethodSignature = isignature2;
					this.CompileInformation.InterfaceCompiler.\u0002(isignature2, scope);
				}
			}
		}

		// Token: 0x06003804 RID: 14340 RVA: 0x000E55B0 File Offset: 0x000E37B0
		private void \u0004()
		{
			foreach (_ISignature isignature in this.ComconNew.AllSignatureList)
			{
				IScope5 scope = global::\u0007.\u0005.\u0001(this.ComconNew, isignature.Id);
				this.CompileInformation.InterfaceCompiler.\u0001(isignature, scope);
				foreach (object obj in isignature._SubSignatures)
				{
					_ISignature isignature2 = (_ISignature)obj;
					scope.MethodSignature = isignature2;
					this.CompileInformation.InterfaceCompiler.\u0001(isignature2, scope);
				}
			}
		}

		// Token: 0x06003805 RID: 14341 RVA: 0x000E5684 File Offset: 0x000E3884
		private void \u0001(DependencySorter \u0002)
		{
			if (this.ErrorsOccured)
			{
				return;
			}
			foreach (_ISignature isignature in \u0002.\u0001)
			{
				IScope5 scope = global::\u0007.\u0005.\u0001(this.ComconNew, isignature.Id);
				foreach (_IVariable u in isignature.AllVariables)
				{
					global::\u0004.\u0011.\u0001(u, scope, this.ComconNew, isignature);
				}
				foreach (object obj in isignature._SubSignatures)
				{
					_ISignature isignature2 = (_ISignature)obj;
					scope.MethodSignature = isignature2;
					foreach (_IVariable u2 in isignature2.AllVariables)
					{
						global::\u0004.\u0011.\u0001(u2, scope, this.ComconNew, isignature2);
					}
				}
			}
		}

		// Token: 0x06003806 RID: 14342 RVA: 0x000E57C0 File Offset: 0x000E39C0
		private IEnumerable<_ISignature> \u0001(_ICompileContext \u0002)
		{
			foreach (_ISignature isignature in \u0002.POUSignaturesEx)
			{
				yield return isignature;
			}
			IEnumerator<_ISignature> enumerator = null;
			foreach (_ISignature isignature2 in ((ICompileContextSerializable)\u0002).GlobalSignaturesSerializable.OfType<_ISignature>())
			{
				yield return isignature2;
			}
			enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x06003807 RID: 14343 RVA: 0x000E57D0 File Offset: 0x000E39D0
		private IEnumerable<_ISignature> \u0002(_ICompileContext \u0002)
		{
			foreach (_ISignature isignature in this.\u0001(\u0002))
			{
				yield return isignature;
			}
			IEnumerator<_ISignature> enumerator = null;
			foreach (_ISignature isignature2 in this.\u0001(\u0002))
			{
				foreach (object obj in isignature2._SubSignatures)
				{
					_ISignature isignature3 = (_ISignature)obj;
					yield return isignature3;
				}
				IEnumerator enumerator2 = null;
			}
			enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x06003808 RID: 14344 RVA: 0x000E57E8 File Offset: 0x000E39E8
		private void \u0005()
		{
			if (!this.ErrorsOccured)
			{
				_ICompileContext referenceContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetReferenceContext(this.ComconNew.ApplicationGuid);
				if (referenceContext != null)
				{
					new global::\u0001.\u0010(this.ComconNew, referenceContext).\u0001();
				}
				foreach (_ISignature u in this.\u0002(this.ComconNew))
				{
					this.\u0001(referenceContext, u);
				}
			}
		}

		// Token: 0x06003809 RID: 14345 RVA: 0x000E5874 File Offset: 0x000E3A74
		private void \u0006()
		{
			if (!this.ErrorsOccured)
			{
				foreach (_ISignature u in this.ComconNew.AllSignatures.OfType<_ISignature>().Where(new Func<_ISignature, bool>(CompilerPhase1_Typifier.<>c.<>9.\u0001)))
				{
					if (!this.CompileInformation.InterfaceCompiler.ConstGenericController.\u0002(u))
					{
						this.ErrorsOccured = true;
						break;
					}
				}
			}
		}

		// Token: 0x0600380A RID: 14346 RVA: 0x000E5914 File Offset: 0x000E3B14
		private void \u0001(_ICompileContext \u0002, _ISignature \u0003)
		{
			if (!\u0003.HasErrors)
			{
				if (\u0003.GetFlagInternal(SignatureFlagInternal.ContainsGenericConstants))
				{
					return;
				}
				if (\u0003.GetFlagInternal(SignatureFlagInternal.ContainsGenericInstanceVar) && !this.CompileInformation.InterfaceCompiler.ConstGenericController.\u0001(\u0003))
				{
					this.ErrorsOccured = true;
				}
				_ISignature u = ((\u0002 != null) ? \u0002.GetSignatureById(\u0003.Id) : null) as _ISignature;
				if (!Locator.\u0001(this.ComconNew.DataManager, this.ComconNew, \u0002, \u0003, u))
				{
					this.ErrorsOccured = true;
				}
			}
		}

		// Token: 0x0600380B RID: 14347 RVA: 0x000E59A0 File Offset: 0x000E3BA0
		private void \u0007()
		{
			foreach (_ISignature isignature in this.ComconNew.AllSignatureList)
			{
				IScope5 scope = global::\u0007.\u0005.\u0001(this.ComconNew, isignature.Id);
				this.CompileInformation.InterfaceCompiler.\u0002(isignature, scope);
				foreach (object obj in isignature._SubSignatures)
				{
					_ISignature isignature2 = (_ISignature)obj;
					scope.MethodSignature = isignature2;
					this.CompileInformation.InterfaceCompiler.\u0002(isignature2, scope);
				}
			}
		}

		// Token: 0x0600380C RID: 14348 RVA: 0x000E5A78 File Offset: 0x000E3C78
		private static bool \u0001(_ICompileContext \u0002)
		{
			bool flag = false;
			foreach (_ISignature isignature in \u0002.AllSignatures.OfType<_ISignature>())
			{
				CompilerPhase1_Typifier.\u0001(isignature, ref flag);
				foreach (object obj in isignature._SubSignatures)
				{
					CompilerPhase1_Typifier.\u0001((_ISignature)obj, ref flag);
				}
				if (flag)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600380D RID: 14349 RVA: 0x000E5B20 File Offset: 0x000E3D20
		private static void \u0001(_ISignature \u0002, ref bool \u0003)
		{
			if (\u0003)
			{
				return;
			}
			if (\u0002.Messages == null || \u0002.Messages.Length == 0)
			{
				return;
			}
			foreach (_ICompilerMessage icompilerMessage in \u0002.Messages.OfType<_ICompilerMessage>())
			{
				bool showCompile = icompilerMessage.ShowCompile;
				if (icompilerMessage.Severity == Severity.Error && showCompile)
				{
					\u0003 = true;
					break;
				}
			}
		}

		// Token: 0x0600380E RID: 14350 RVA: 0x000E5B9C File Offset: 0x000E3D9C
		private \u0081.\u0008 \u0001(_ISignature \u0002)
		{
			_ISignature isignature = \u0002;
			if (\u0002.ParentSignatureId != Helper.InvalidId)
			{
				isignature = this.ComconNew[\u0002.ParentSignatureId];
			}
			\u0081.\u0008 u;
			if (isignature.LibraryPath != null && isignature.LibraryPath != string.Empty)
			{
				u = \u0081.\u0008.\u0001(this.ComconNew, isignature.LibraryPath, \u0002, this.PrecompPool, this.ComconOld);
			}
			else
			{
				u = \u0081.\u0008.\u0001(this.ComconNew, \u0002, this.PrecompPool, this.ComconOld);
			}
			u.TypifierLateParseTreeLoader = this.TypifierLateParseTreeLoader;
			return u;
		}

		// Token: 0x04000B1C RID: 2844
		[CompilerGenerated]
		private global::\u000E.\u001B \u0001;

		// Token: 0x04000B1D RID: 2845
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x04000B1E RID: 2846
		[CompilerGenerated]
		private bool \u0002;

		// Token: 0x04000B1F RID: 2847
		[CompilerGenerated]
		private readonly LDictionary<int, CompactedTypifiedParseTreeInformation> \u0001;

		// Token: 0x04000B20 RID: 2848
		[CompilerGenerated]
		private readonly global::\u000F.\u0015 \u0001;

		// Token: 0x04000B21 RID: 2849
		[CompilerGenerated]
		private readonly CompilerPhase1_Typifier.\u0001 \u0001;

		// Token: 0x020003F2 RID: 1010
		private sealed class POUTypifier
		{
			// Token: 0x17000943 RID: 2371
			// (get) Token: 0x0600380F RID: 14351 RVA: 0x000E5C2C File Offset: 0x000E3E2C
			private _ICompileContext ComconNew
			{
				get
				{
					return this.\u0001.ComconNew;
				}
			}

			// Token: 0x17000944 RID: 2372
			// (get) Token: 0x06003810 RID: 14352 RVA: 0x000E5C3C File Offset: 0x000E3E3C
			private bool OnlineChange
			{
				get
				{
					return this.\u0001.OnlineChange;
				}
			}

			// Token: 0x17000945 RID: 2373
			// (get) Token: 0x06003811 RID: 14353 RVA: 0x000E5C4C File Offset: 0x000E3E4C
			private _IPreCompileContext Precomp
			{
				get
				{
					return this.\u0001.Precomp;
				}
			}

			// Token: 0x17000946 RID: 2374
			// (get) Token: 0x06003812 RID: 14354 RVA: 0x000E5C5C File Offset: 0x000E3E5C
			private _IPreCompileContext PrecompPool
			{
				get
				{
					return this.\u0001.PrecompPool;
				}
			}

			// Token: 0x17000947 RID: 2375
			// (get) Token: 0x06003813 RID: 14355 RVA: 0x000E5C6C File Offset: 0x000E3E6C
			private _ICompileContext ComconOld
			{
				get
				{
					return this.\u0001.ComconOld;
				}
			}

			// Token: 0x17000948 RID: 2376
			// (get) Token: 0x06003814 RID: 14356 RVA: 0x000E5C7C File Offset: 0x000E3E7C
			// (set) Token: 0x06003815 RID: 14357 RVA: 0x000E5C84 File Offset: 0x000E3E84
			public bool ErrorsInCode { get; set; }

			// Token: 0x17000949 RID: 2377
			// (get) Token: 0x06003816 RID: 14358 RVA: 0x000E5C90 File Offset: 0x000E3E90
			private LDictionary<int, CompactedTypifiedParseTreeInformation> ParseTreeInformationTable
			{
				get
				{
					return this.\u0001.ParseTreeInformationTable;
				}
			}

			// Token: 0x06003817 RID: 14359 RVA: 0x000E5CA0 File Offset: 0x000E3EA0
			private POUTypifier(CompilerPhase1_Typifier tpc)
			{
				this.\u0001 = tpc;
			}

			// Token: 0x06003818 RID: 14360 RVA: 0x000E5CB0 File Offset: 0x000E3EB0
			internal static void \u0001(CompilerPhase1_Typifier \u0002, _ICompiledPOU \u0003, out bool \u0004)
			{
				CompilerPhase1_Typifier.POUTypifier poutypifier = new CompilerPhase1_Typifier.POUTypifier(\u0002);
				poutypifier.\u0001(\u0003);
				\u0004 = poutypifier.ErrorsInCode;
			}

			// Token: 0x06003819 RID: 14361 RVA: 0x000E5CD4 File Offset: 0x000E3ED4
			private void \u0001(_ICompiledPOU \u0002)
			{
				_ISignature isignature = this.ComconNew[\u0002.SignatureId];
				\u0081.\u0008 u = this.\u0001.\u0001(isignature);
				_IStatement istatement = (\u0002 as ICompiledPOUWithParseTreeProvider).CreateTemporaryRedTree();
				if (!this.\u0001(istatement, \u0002, isignature))
				{
					this.\u0001(\u0002, u, istatement);
				}
				else
				{
					\u0002.SetFlag(CompiledPOUFlags.Typified, true);
				}
				this.\u0001(\u0002, istatement);
			}

			// Token: 0x0600381A RID: 14362 RVA: 0x000E5D34 File Offset: 0x000E3F34
			private void \u0001(ICompiledPOU \u0002, _IStatement \u0003)
			{
				ErrorVisitor errorVisitor = new ErrorVisitor
				{
					VisitErrorStatements = false,
					VisitErrorStatementsInConditionalPragmas = false
				};
				\u0003.Accept(errorVisitor);
				((_ICompiledPOU)\u0002).SetMessages(errorVisitor._Messages);
			}

			// Token: 0x0600381B RID: 14363 RVA: 0x000E5D70 File Offset: 0x000E3F70
			private void \u0001(_ICompiledPOU \u0002, \u0081.\u0008 \u0003, _IStatement \u0004)
			{
				_ICompiledPOU icompiledPOU = null;
				_ISignature isignature = null;
				_ISignature u = null;
				if (this.ComconOld != null)
				{
					icompiledPOU = this.ComconOld._GetCompiledPOUById(\u0002.SignatureId);
					if (icompiledPOU != null)
					{
						isignature = this.ComconOld[icompiledPOU.SignatureId];
					}
					if (isignature != null && isignature.Name == IdentifierConstants.MainSignatureName)
					{
						u = isignature;
						isignature = this.ComconOld[isignature.ParentSignatureId];
					}
				}
				bool flag = icompiledPOU != null && !icompiledPOU.GetFlag(CompiledPOUFlags.TopLevel) && \u0002.GetFlag(CompiledPOUFlags.TopLevel);
				if (!this.OnlineChange || flag || icompiledPOU == null || icompiledPOU.GetFlag(CompiledPOUFlags.ToTypify) || isignature == null || isignature.GetFlag(SignatureFlag.Generated))
				{
					this.\u0002(\u0002, \u0003, \u0004);
					return;
				}
				this.\u0001(\u0002, \u0003, icompiledPOU, isignature, u);
				this.ComconNew.NoNewReferences = false;
			}

			// Token: 0x0600381C RID: 14364 RVA: 0x000E5E44 File Offset: 0x000E4044
			private void \u0001(_ICompiledPOU \u0002, \u0081.\u0008 \u0003, _ICompiledPOU \u0004, _ISignature \u0005, _ISignature \u0006)
			{
				this.ComconNew.NoNewReferences = true;
				this.ComconNew.ContainsOnlineChangeCode = true;
				\u0002.SetMessages(\u0004.Messages);
				\u0002.SetFlag(CompiledPOUFlags.ContainsNoParseTree, true);
				\u0002.SetFlagInternal(InternalCompiledPOUFlags.ContainsTryCatch, \u0004.GetFlagInternal(InternalCompiledPOUFlags.ContainsTryCatch));
				\u0002.SetParseTree(null);
				_ISignature isignature = this.ComconNew[\u0002.SignatureId];
				_ISignature isignature2 = null;
				if (\u0006 != null)
				{
					isignature2 = isignature;
					isignature = this.ComconNew[isignature.ParentSignatureId];
					isignature2.SetCalleeIds(\u0006.CalleeIdList);
				}
				isignature.SetCalleeIds(\u0005.CalleeIdList);
				isignature.SetFlagInternal(SignatureFlagInternal.ContainsAccessToCurrentTask, \u0005.GetFlagInternal(SignatureFlagInternal.ContainsAccessToCurrentTask));
				List<int> list = new List<int>(\u0005.AllUsedIds);
				if (isignature2 != null)
				{
					list.AddRange(\u0006.AllUsedIds);
				}
				this.\u0001(\u0003, isignature, isignature2, list);
			}

			// Token: 0x0600381D RID: 14365 RVA: 0x000E5F1C File Offset: 0x000E411C
			private void \u0001(\u0081.\u0008 \u0002, _ISignature \u0003, _ISignature \u0004, List<int> \u0005)
			{
				foreach (int nId in \u0005)
				{
					_ISignature isignature = this.ComconNew[nId];
					_ISignature isignature2 = this.ComconOld[nId];
					if (isignature2 != null && isignature2.ParentSignatureId != Helper.InvalidId)
					{
						isignature2 = this.ComconOld[isignature2.ParentSignatureId];
					}
					if (isignature == null)
					{
						if (isignature2 == null)
						{
							continue;
						}
						isignature = this.\u0001(\u0002, isignature2);
					}
					if (isignature != null)
					{
						CompilerPhase1_Typifier.POUTypifier.\u0001(\u0003, \u0004, isignature, isignature2);
					}
				}
			}

			// Token: 0x0600381E RID: 14366 RVA: 0x000E5FBC File Offset: 0x000E41BC
			private _ISignature \u0001(\u0081.\u0008 \u0002, _ISignature \u0003)
			{
				ISignature[] array;
				_IPreCompileContext u;
				_ISignature isignature = Helper.\u0001(this.ComconNew, \u0003, this.Precomp, this.PrecompPool, out array, out u);
				if (isignature != null)
				{
					return \u0002.\u0001(isignature, u);
				}
				return null;
			}

			// Token: 0x0600381F RID: 14367 RVA: 0x000E5FF4 File Offset: 0x000E41F4
			private static void \u0001(_ISignature \u0002, _ISignature \u0003, _ISignature \u0004, _ISignature \u0005)
			{
				if (\u0005 != null)
				{
					foreach (int num in \u0005.CallerIds)
					{
						if (num == \u0002.Id || (\u0003 != null && num == \u0003.Id))
						{
							\u0004.AddCaller(num);
							return;
						}
					}
				}
			}

			// Token: 0x06003820 RID: 14368 RVA: 0x000E603C File Offset: 0x000E423C
			private void \u0002(_ICompiledPOU \u0002, \u0081.\u0008 \u0003, _IStatement \u0004)
			{
				TypifierAndCrossReferenceCollector ivisit = new TypifierAndCrossReferenceCollector(\u0003, this.ComconNew, \u0002);
				\u0080.\u000E.\u0001(this.ComconNew, \u0004, \u0002.SignatureId, \u0003);
				\u0004.Accept(ivisit);
				global::\u001A.\u0005.\u0001(\u0004, \u0002.SignatureId, \u0003);
				\u0002.SetFlagInternal(InternalCompiledPOUFlags.ToCheck, true);
				\u0002.SetFlag(CompiledPOUFlags.Typified, true);
				CompactedTypifiedParseTreeInformation compactedTypifiedParseTreeInformation = new CompactedTypifiedParseTreeInformation();
				TypifiedParseTreeInformationCollector.CollectParseTreeInformation(\u0004, compactedTypifiedParseTreeInformation);
				this.ParseTreeInformationTable.Add(\u0002.SignatureId, compactedTypifiedParseTreeInformation);
			}

			// Token: 0x06003821 RID: 14369 RVA: 0x000E60B0 File Offset: 0x000E42B0
			private bool \u0001(_IExprement \u0002, _ICompiledPOU \u0003, _ISignature \u0004)
			{
				if (!CompilerPhase1_Typifier.POUTypifier.\u0001(\u0003))
				{
					return false;
				}
				ErrorVisitor errorVisitor = new ErrorVisitor
				{
					VisitErrorStatements = false,
					VisitErrorStatementsInConditionalPragmas = false
				};
				\u0002.Accept(errorVisitor);
				bool result = false;
				bool flag = \u0004.GetFlag(SignatureFlag.External);
				if (errorVisitor.MessageList.Count > 0)
				{
					foreach (_ICompilerMessage icompilerMessage in errorVisitor.MessageList)
					{
						if ((icompilerMessage.Severity == Severity.Error || icompilerMessage.Severity == Severity.FatalError) && icompilerMessage.ShowCompile)
						{
							result = true;
							if (!flag)
							{
								this.ErrorsInCode = true;
								break;
							}
							break;
						}
					}
				}
				return result;
			}

			// Token: 0x06003822 RID: 14370 RVA: 0x000E6164 File Offset: 0x000E4364
			private static bool \u0001(_ICompiledPOU \u0002)
			{
				bool result = false;
				ICompiledPOUWithCompactedParseTree compiledPOUWithCompactedParseTree = \u0002 as ICompiledPOUWithCompactedParseTree;
				if (((compiledPOUWithCompactedParseTree != null) ? compiledPOUWithCompactedParseTree.CompactedParseTreeInformation : null) != null && compiledPOUWithCompactedParseTree.CompactedParseTreeInformation.MessageTable != null)
				{
					using (IEnumerator<IList<_ICompilerMessage>> enumerator = compiledPOUWithCompactedParseTree.CompactedParseTreeInformation.MessageTable.Values.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							if (enumerator.Current.Any(new Func<_ICompilerMessage, bool>(CompilerPhase1_Typifier.POUTypifier.<>c.<>9.\u0001)))
							{
								result = true;
								break;
							}
						}
					}
				}
				return result;
			}

			// Token: 0x04000B22 RID: 2850
			private readonly CompilerPhase1_Typifier \u0001;

			// Token: 0x04000B23 RID: 2851
			[CompilerGenerated]
			private bool \u0001;
		}

		// Token: 0x020003F4 RID: 1012
		private sealed class \u0001
		{
			// Token: 0x1700094A RID: 2378
			// (get) Token: 0x06003826 RID: 14374 RVA: 0x000E6234 File Offset: 0x000E4434
			// (set) Token: 0x06003827 RID: 14375 RVA: 0x000E623C File Offset: 0x000E443C
			private global::\u000E.\u001B CompileInformation { get; set; }

			// Token: 0x1700094B RID: 2379
			// (get) Token: 0x06003828 RID: 14376 RVA: 0x000E6248 File Offset: 0x000E4448
			private _ICompileContext ComconNew
			{
				get
				{
					return this.CompileInformation.ComconNew;
				}
			}

			// Token: 0x1700094C RID: 2380
			// (get) Token: 0x06003829 RID: 14377 RVA: 0x000E6258 File Offset: 0x000E4458
			private bool OnlineChange
			{
				get
				{
					return this.CompileInformation.OnlineChange;
				}
			}

			// Token: 0x1700094D RID: 2381
			// (get) Token: 0x0600382A RID: 14378 RVA: 0x000E6268 File Offset: 0x000E4468
			private bool BootProject
			{
				get
				{
					return this.CompileInformation.BootProject;
				}
			}

			// Token: 0x1700094E RID: 2382
			// (get) Token: 0x0600382B RID: 14379 RVA: 0x000E6278 File Offset: 0x000E4478
			private _IPreCompileContext Precomp
			{
				get
				{
					return this.CompileInformation.Precomp;
				}
			}

			// Token: 0x1700094F RID: 2383
			// (get) Token: 0x0600382C RID: 14380 RVA: 0x000E6288 File Offset: 0x000E4488
			private _IPreCompileContext PrecompPool
			{
				get
				{
					return this.CompileInformation.PrecompPool;
				}
			}

			// Token: 0x17000950 RID: 2384
			// (get) Token: 0x0600382D RID: 14381 RVA: 0x000E6298 File Offset: 0x000E4498
			private _ICompileContext ComconOld
			{
				get
				{
					return this.CompileInformation.ComconOld;
				}
			}

			// Token: 0x17000951 RID: 2385
			// (get) Token: 0x0600382E RID: 14382 RVA: 0x000E62A8 File Offset: 0x000E44A8
			private IProgressCallback Callback
			{
				get
				{
					return this.CompileInformation.Callback;
				}
			}

			// Token: 0x17000952 RID: 2386
			// (get) Token: 0x0600382F RID: 14383 RVA: 0x000E62B8 File Offset: 0x000E44B8
			private LDictionary<int, CompactedTypifiedParseTreeInformation> ParseTreeInformationTable { get; } = new LDictionary<int, CompactedTypifiedParseTreeInformation>();

			// Token: 0x17000953 RID: 2387
			// (get) Token: 0x06003830 RID: 14384 RVA: 0x000E62C0 File Offset: 0x000E44C0
			private global::\u000F.\u0015 TypifierLateParseTreeLoader { get; } = new global::\u000F.\u0015();

			// Token: 0x17000954 RID: 2388
			// (get) Token: 0x06003831 RID: 14385 RVA: 0x000E62C8 File Offset: 0x000E44C8
			private CompilerPhase1_Typifier PhaseTypifier { get; }

			// Token: 0x06003832 RID: 14386 RVA: 0x000E62D0 File Offset: 0x000E44D0
			private void \u0001(bool \u0002)
			{
				this.PhaseTypifier.ErrorsInCode = \u0002;
			}

			// Token: 0x06003833 RID: 14387 RVA: 0x000E62E0 File Offset: 0x000E44E0
			internal \u0001(global::\u000E.\u001B \u008F\u0004, CompilerPhase1_Typifier \u000E\u0005)
			{
				this.CompileInformation = \u008F\u0004;
				this.PhaseTypifier = \u000E\u0005;
			}

			// Token: 0x06003834 RID: 14388 RVA: 0x000E630C File Offset: 0x000E450C
			internal void \u0001()
			{
				global::\u0014.\u0001 u = new global::\u0014.\u0001(this.ComconNew.IsDefined("debug_dump_times"));
				bool flag = true;
				u.\u0001("TypifyAll");
				if (this.ComconOld != null)
				{
					u.\u0001("FindObjectsToTypify");
					flag = ObjectsToTypifyDetector.\u0001(this.ComconOld, this.OnlineChange, this.BootProject, this.Precomp, this.PrecompPool);
					if (!flag)
					{
						this.ComconNew.CopyLibraryReferences(this.ComconOld);
					}
					u.\u0004("FindObjectsToTypify");
				}
				this.\u0002();
				this.TypifierLateParseTreeLoader.\u0001();
				u.\u0001("TypifySignatures");
				this.\u0003();
				u.\u0004("TypifySignatures");
				u.\u0001("TypifyPOUs");
				this.\u0004();
				u.\u0004("TypifyPOUs");
				this.TypifierLateParseTreeLoader.\u0001(this.ComconNew);
				u.\u0004("TypifyAll");
				if (!flag)
				{
					\u001E.\u001B.\u0001(this.ComconNew, this.ComconOld);
				}
				((_ICompileContext4)this.ComconNew).TypificationDone = true;
				if (this.OnlineChange && this.Precomp.IsDefined("debug_dump_onlinechange"))
				{
					string u2 = string.Format("TypifyAll: {0}", flag.ToString());
					_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, u2, Severity.Text, MessageId.None);
					APEnvironmentFacade.Instance.AddMessage(APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory, message);
				}
				this.\u0001(u);
			}

			// Token: 0x06003835 RID: 14389 RVA: 0x000E6474 File Offset: 0x000E4674
			private void \u0002()
			{
				if (!this.ComconNew.IsDefined("no_duplication_thread"))
				{
					for (int i = 0; i < this.ComconNew.GetAllSignaturesFlatInvariant().Count<_ISignature>(); i++)
					{
						_ISignature isignature = this.ComconNew.GetAllSignaturesFlatInvariant()[i];
						if (isignature != null)
						{
							ICompiledPOUWithParseTreeProvider compiledPOUWithParseTreeProvider = this.ComconNew.GetCompiledPOUById(isignature.Id) as ICompiledPOUWithParseTreeProvider;
							if (compiledPOUWithParseTreeProvider != null)
							{
								compiledPOUWithParseTreeProvider.ParseTreeProvider = global::\u001A.\u0012.\u0001(compiledPOUWithParseTreeProvider, this.TypifierLateParseTreeLoader);
								this.TypifierLateParseTreeLoader.\u0001(compiledPOUWithParseTreeProvider);
							}
						}
					}
				}
			}

			// Token: 0x06003836 RID: 14390 RVA: 0x000E64FC File Offset: 0x000E46FC
			private void \u0003()
			{
				foreach (_ISignature isignature in this.ComconNew.AllFlat)
				{
					if (!isignature.GetFlag(SignatureFlag.Typified) && !isignature.GetFlag(SignatureFlag.Temp))
					{
						APEnvironmentFacade.Instance.LanguageModelMgr.Progress.NotifyTaskProgress(this.Callback, isignature.Name);
						IScope5 u = this.\u0001(isignature);
						isignature.SetFlag(SignatureFlag.Temp, true);
						this.CompileInformation.InterfaceCompiler.\u0004(isignature, u);
						isignature.SetFlag(SignatureFlag.Temp, false);
					}
				}
			}

			// Token: 0x06003837 RID: 14391 RVA: 0x000E65BC File Offset: 0x000E47BC
			private void \u0004()
			{
				for (int i = 0; i < this.ComconNew.GetAllSignaturesFlatInvariant().Count<_ISignature>(); i++)
				{
					_ISignature isignature = this.ComconNew.GetAllSignaturesFlatInvariant()[i];
					if (isignature != null)
					{
						\u0081.\u0008 u = this.\u0001(isignature);
						if (isignature.HasAttribute("contains_implicit_enum"))
						{
							\u0080.\u000E.\u0001(isignature, u);
						}
						this.CompileInformation.InterfaceCompiler.\u0005(isignature, u);
						if (isignature.POUType != Operator.FunctionBlock)
						{
							_ICompiledPOU icompiledPOU = this.ComconNew.GetCompiledPOUById(isignature.Id) as _ICompiledPOU;
							if (icompiledPOU != null && !icompiledPOU.GetFlag(CompiledPOUFlags.Typified))
							{
								this.\u0001(icompiledPOU);
							}
						}
					}
				}
			}

			// Token: 0x06003838 RID: 14392 RVA: 0x000E6660 File Offset: 0x000E4860
			private void \u0001(_ICompiledPOU \u0002)
			{
				APEnvironmentFacade.Instance.LanguageModelMgr.Progress.NotifyTaskProgress(this.Callback, \u0002.Name);
				bool flag;
				CompilerPhase1_Typifier.POUTypifier.\u0001(this.PhaseTypifier, \u0002, out flag);
				if (flag)
				{
					this.\u0001(true);
				}
			}

			// Token: 0x06003839 RID: 14393 RVA: 0x000E66A8 File Offset: 0x000E48A8
			private void \u0001(global::\u0014.\u0001 \u0002)
			{
				if (\u0002.Output)
				{
					\u0002.\u0002("FindObjectsToTypify", "Time to detect objects to typify {0} ms");
					\u0002.\u0002("TypifySignatures", "Time to typify signatures {0} ms");
					\u0002.\u0002("TypifyPOUs", "Time to typify code {0} ms");
					\u0002.\u0002("TypifyAll", "Time to typify complete {0} ms");
					IMessageCategory messageCategory = APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory;
					string u = string.Format("Number of signatures in compile context: {0}", this.ComconNew.AllSignatures.Length);
					_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, u, Severity.Text, MessageId.None);
					APEnvironmentFacade.Instance.AddMessage(messageCategory, message);
					u = string.Format("Number of POUs in compile context: {0}", this.ComconNew.GetAllCompiledPOUsEx().Count<ICompiledPOU4>());
					message = global::\u0019.\u0003.\u0001(null, u, Severity.Text, MessageId.None);
					APEnvironmentFacade.Instance.AddMessage(messageCategory, message);
					u = string.Format("Number of late loaded parse trees: {0}", this.TypifierLateParseTreeLoader.ParseTreesLoaded);
					message = global::\u0019.\u0003.\u0001(null, u, Severity.Text, MessageId.None);
					APEnvironmentFacade.Instance.AddMessage(messageCategory, message);
					u = string.Format("Cumulated time for parse tree loading: {0} ms", this.TypifierLateParseTreeLoader.TimeToLoad / 10000L);
					message = global::\u0019.\u0003.\u0001(null, u, Severity.Text, MessageId.None);
					APEnvironmentFacade.Instance.AddMessage(messageCategory, message);
					u = string.Format("Cumulated time for red tree conversion: {0} ms", this.TypifierLateParseTreeLoader.TimeForRedTree / 10000L);
					message = global::\u0019.\u0003.\u0001(null, u, Severity.Text, MessageId.None);
					APEnvironmentFacade.Instance.AddMessage(messageCategory, message);
				}
			}

			// Token: 0x0600383A RID: 14394 RVA: 0x000E6820 File Offset: 0x000E4A20
			private \u0081.\u0008 \u0001(_ISignature \u0002)
			{
				_ISignature isignature = \u0002;
				if (\u0002.ParentSignatureId != Helper.InvalidId)
				{
					isignature = this.ComconNew[\u0002.ParentSignatureId];
				}
				\u0081.\u0008 u;
				if (isignature.LibraryPath != null && isignature.LibraryPath != string.Empty)
				{
					u = \u0081.\u0008.\u0001(this.ComconNew, isignature.LibraryPath, \u0002, this.PrecompPool, this.ComconOld);
				}
				else
				{
					u = \u0081.\u0008.\u0001(this.ComconNew, \u0002, this.PrecompPool, this.ComconOld);
				}
				u.TypifierLateParseTreeLoader = this.TypifierLateParseTreeLoader;
				return u;
			}

			// Token: 0x04000B26 RID: 2854
			[CompilerGenerated]
			private global::\u000E.\u001B \u0001;

			// Token: 0x04000B27 RID: 2855
			[CompilerGenerated]
			private readonly LDictionary<int, CompactedTypifiedParseTreeInformation> \u0001;

			// Token: 0x04000B28 RID: 2856
			[CompilerGenerated]
			private readonly global::\u000F.\u0015 \u0001;

			// Token: 0x04000B29 RID: 2857
			[CompilerGenerated]
			private readonly CompilerPhase1_Typifier \u0001;
		}
	}
}
