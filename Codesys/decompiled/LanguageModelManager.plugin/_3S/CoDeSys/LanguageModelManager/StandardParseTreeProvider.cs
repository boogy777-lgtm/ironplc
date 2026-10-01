using System;
using System.IO;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.CommonCompilerData;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000036 RID: 54
	internal class StandardParseTreeProvider : IParseTreeProvider
	{
		// Token: 0x1700006E RID: 110
		// (get) Token: 0x06000298 RID: 664 RVA: 0x00009D5A File Offset: 0x00008D5A
		private CompiledPOU CompiledPOU { get; }

		// Token: 0x1700006F RID: 111
		// (get) Token: 0x06000299 RID: 665 RVA: 0x00009D62 File Offset: 0x00008D62
		private IGreenTreeConverter GreenTreeConverter { get; }

		// Token: 0x17000070 RID: 112
		// (get) Token: 0x0600029A RID: 666 RVA: 0x00009D6A File Offset: 0x00008D6A
		private ITreeFactory TreeFactory { get; }

		// Token: 0x17000071 RID: 113
		// (get) Token: 0x0600029B RID: 667 RVA: 0x00009D72 File Offset: 0x00008D72
		internal WeakReference<_IStatement> RedParseTree { get; } = new WeakReference<_IStatement>(null);

		// Token: 0x17000072 RID: 114
		// (get) Token: 0x0600029C RID: 668 RVA: 0x00009D7A File Offset: 0x00008D7A
		// (set) Token: 0x0600029D RID: 669 RVA: 0x00009D84 File Offset: 0x00008D84
		public ICompactedParseTreeInformation CompactedParseTreeInformation
		{
			get
			{
				return this._compactedParseTreeInformation;
			}
			set
			{
				WeakReference<_IStatement> redParseTree = this.RedParseTree;
				lock (redParseTree)
				{
					this._compactedParseTreeInformation = value;
					this.RedParseTree.SetTarget(null);
				}
			}
		}

		// Token: 0x0600029E RID: 670 RVA: 0x00009DD4 File Offset: 0x00008DD4
		internal StandardParseTreeProvider(_ICompiledPOU2 cpou, IGreenTreeConverter converter, ITreeFactory treeFactory)
		{
			this.CompiledPOU = (cpou as CompiledPOU);
			this.GreenTreeConverter = converter;
			this.TreeFactory = treeFactory;
		}

		// Token: 0x0600029F RID: 671 RVA: 0x00009E04 File Offset: 0x00008E04
		public _IStatement GetParseTree()
		{
			_IStatement istatement = null;
			if (this.CompiledPOU.OriginalParseTree is _IEmptyStatement && !this.CompiledPOU.GetFlag(CompiledPOUFlags.Compiled) && !string.IsNullOrEmpty(this.CompiledPOU.LibraryPath))
			{
				istatement = this.LateLoadParseTreeFromLib();
				if (istatement != null)
				{
					return istatement;
				}
			}
			if (this.CompiledPOU.OriginalParseTree is IGreenTreeExprement)
			{
				WeakReference<_IStatement> redParseTree = this.RedParseTree;
				lock (redParseTree)
				{
					if (this.RedParseTree.TryGetTarget(out istatement))
					{
						return istatement;
					}
					_IStatement istatement2;
					this.ConvertParseTreeToRedTree(this.CompiledPOU.OriginalParseTree, this.CompactedParseTreeInformation, out istatement2);
					this.RedParseTree.SetTarget(istatement2);
					return istatement2;
				}
			}
			if (this.CompiledPOU.OriginalParseTree == null && this.CompiledPOU.GetFlag(CompiledPOUFlags.Compiled) && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV3201)
			{
				return this.CreateTypifiedTree();
			}
			return this.CompiledPOU.OriginalParseTree;
		}

		// Token: 0x060002A0 RID: 672 RVA: 0x00009F18 File Offset: 0x00008F18
		private _IStatement LateLoadParseTreeFromLib()
		{
			_IStatement result = null;
			if (this.CompiledPOU.ObjectGuid == Guid.Empty)
			{
				return null;
			}
			if (string.IsNullOrEmpty(this.CompiledPOU.LibraryPath))
			{
				return null;
			}
			int projectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(this.CompiledPOU.LibraryPath);
			if (projectHandle == -1)
			{
				return null;
			}
			Stream parseTreeStreamOfCompiledLibraryPOU = APEnvironmentFacade.Instance.GetParseTreeStreamOfCompiledLibraryPOU(projectHandle, this.CompiledPOU.ObjectGuid);
			if (parseTreeStreamOfCompiledLibraryPOU != null)
			{
				ISharedDataStorage sharedDataStorage = APEnvironmentFacade.Instance.GetSharedDataStorage(projectHandle);
				IArchiveReader archiveReader;
				if (sharedDataStorage != null)
				{
					archiveReader = APEnvironmentFacade.Instance.CreateNewEncryptedBinaryArchiveReader();
				}
				else
				{
					archiveReader = APEnvironmentFacade.Instance.CreateEncryptedBinaryArchiveReader();
				}
				archiveReader.Initialize(parseTreeStreamOfCompiledLibraryPOU);
				result = (_IStatement)((IArchiveReader2)archiveReader).Load(sharedDataStorage);
				parseTreeStreamOfCompiledLibraryPOU.Close();
			}
			return result;
		}

		// Token: 0x060002A1 RID: 673 RVA: 0x00009FE4 File Offset: 0x00008FE4
		private _IStatement CreateTypifiedTree()
		{
			IPreCompileContext preCompileContext = null;
			IPreCompileContext preCompileContext2 = null;
			_ICompileContext icompileContext = null;
			if (this.CompiledPOU.LibraryPath != null)
			{
				preCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(this.CompiledPOU.LibraryPath);
			}
			this.FindMyContext(ref preCompileContext2, ref icompileContext);
			IPreCompileContext precompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(Guid.Empty);
			if (icompileContext == null)
			{
				return this.CompiledPOU.OriginalParseTree;
			}
			CompiledPOU compiledPOU = null;
			if (preCompileContext != null)
			{
				compiledPOU = (preCompileContext.GetCompiledPOU(this.CompiledPOU.ObjectGuid) as CompiledPOU);
			}
			if (compiledPOU == null && precompileContext != null)
			{
				compiledPOU = (precompileContext.GetCompiledPOU(this.CompiledPOU.ObjectGuid) as CompiledPOU);
			}
			if (compiledPOU == null && preCompileContext2 != null)
			{
				compiledPOU = (preCompileContext2.GetCompiledPOU(this.CompiledPOU.ObjectGuid) as CompiledPOU);
			}
			if (compiledPOU == null)
			{
				return this.CompiledPOU.OriginalParseTree;
			}
			_IStatement istatement = compiledPOU.GetParseTree().Duplicate() as _IStatement;
			this.TypifyStatementAccordingToVersion(icompileContext, compiledPOU, istatement);
			if (APEnvironmentFacade.Instance.LanguageModelMgr.CompilationInProgress && ((APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351050 && !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351100) || APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351110))
			{
				this.SetParseTreeWithSideEffects(istatement);
			}
			return istatement;
		}

		// Token: 0x060002A2 RID: 674 RVA: 0x0000A128 File Offset: 0x00009128
		private void FindMyContext(ref IPreCompileContext precomApp, ref _ICompileContext comcon)
		{
			foreach (ILMCompiledApplicationSet ilmcompiledApplicationSet in APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.CompiledApplicationSets)
			{
				if (ilmcompiledApplicationSet != null && ilmcompiledApplicationSet.GetCompiledPOUById(this.CompiledPOU.SignatureId) == this.CompiledPOU)
				{
					precomApp = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(ilmcompiledApplicationSet.ApplicationGuid);
					comcon = (_ICompileContext)ilmcompiledApplicationSet;
					break;
				}
			}
		}

		// Token: 0x060002A3 RID: 675 RVA: 0x0000A1BC File Offset: 0x000091BC
		private void TypifyStatementAccordingToVersion(_ICompileContext comcon, CompiledPOU cpou, _IStatement statement)
		{
			IScope5 scope = CompilerProxy.CreateScope(comcon, this.CompiledPOU.SignatureId);
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35400)
			{
				CompilerProxy.TypifyAndCheckExprement(statement, scope, comcon);
				return;
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300)
			{
				CompilerProxy.TypifyExprement(statement, scope, comcon, null, false, false, cpou);
				statement.SetFlag(StatementFlag.TypifiedNotChecked, true);
				return;
			}
			CompilerProxy.TypifyExprement(statement, scope, comcon, null, false, true, cpou);
			statement.SetFlag(StatementFlag.TypifiedNotChecked, true);
		}

		// Token: 0x060002A4 RID: 676 RVA: 0x0000A234 File Offset: 0x00009234
		public _IStatement CreateTemporaryRedTree()
		{
			if (this.CompiledPOU.OriginalParseTree is _IEmptyStatement && !this.CompiledPOU.GetFlag(CompiledPOUFlags.Compiled) && !string.IsNullOrEmpty(this.CompiledPOU.LibraryPath))
			{
				_IStatement istatement = this.LateLoadParseTreeFromLib();
				if (istatement != null)
				{
					return istatement;
				}
			}
			WeakReference<_IStatement> redParseTree = this.RedParseTree;
			lock (redParseTree)
			{
				_IStatement result = null;
				if (this.RedParseTree.TryGetTarget(out result))
				{
					return result;
				}
				if (this.CompiledPOU.OriginalParseTree != null)
				{
					_IStatement result2;
					this.ConvertParseTreeToRedTree(this.CompiledPOU.OriginalParseTree, this.CompactedParseTreeInformation, out result2);
					this.RedParseTree.SetTarget(null);
					return result2;
				}
			}
			return this.CompiledPOU.OriginalParseTree;
		}

		// Token: 0x060002A5 RID: 677 RVA: 0x0000A30C File Offset: 0x0000930C
		public void CreateParseTreeForCompiledPOU(_ICompiledPOU pouRet)
		{
			CompiledPOU compiledPOU = pouRet as CompiledPOU;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV3200)
			{
				if (this.CompiledPOU.OriginalParseTree is _IEmptyStatement)
				{
					_IStatement istatement = this.LateLoadParseTreeFromLib();
					if (istatement != null)
					{
						compiledPOU.SetParseTreeDirectly(istatement);
						compiledPOU.SetFlagInternal(InternalCompiledPOUFlags.SkipParseTreeDuplication, true);
					}
					else
					{
						compiledPOU.SetParseTreeDirectly(new SequenceStatement());
					}
				}
				else if (this.CompiledPOU.OriginalParseTree == null)
				{
					compiledPOU.SetParseTreeDirectly(this.GetParseTree());
				}
				else
				{
					compiledPOU.SetParseTreeDirectly(this.CompiledPOU.OriginalParseTree);
				}
			}
			else
			{
				if (this.CompiledPOU.OriginalParseTree == null)
				{
					compiledPOU.SetParseTreeDirectly(this.GetParseTree());
				}
				else
				{
					compiledPOU.SetParseTreeDirectly(this.CompiledPOU.OriginalParseTree.Duplicate() as _IStatement);
				}
				CompilerProxy.CheckSyntax(compiledPOU);
			}
			compiledPOU.CompactedParseTreeInformation = this.CompactedParseTreeInformation;
		}

		// Token: 0x060002A6 RID: 678 RVA: 0x0000A3E8 File Offset: 0x000093E8
		public void SetParseTreeWithSideEffects(_IStatement parseTree)
		{
			this.SetParseTreeDirectly(parseTree);
			if (parseTree == null)
			{
				this.CompiledPOU.SetFlag(CompiledPOUFlags.ContainsNoParseTree, true);
			}
			WeakReference<_IStatement> redParseTree = this.RedParseTree;
			lock (redParseTree)
			{
				this.RedParseTree.SetTarget(null);
				this.CompactedParseTreeInformation = null;
			}
		}

		// Token: 0x060002A7 RID: 679 RVA: 0x0000A450 File Offset: 0x00009450
		public void SetParseTreeDirectly(_IStatement parseTree)
		{
			this.CompiledPOU.SetParseTreeDirectly(parseTree);
		}

		// Token: 0x060002A8 RID: 680 RVA: 0x0000A460 File Offset: 0x00009460
		public void DuplicateParseTreeForCompilation()
		{
			if (this.CompiledPOU.GetFlagInternal(InternalCompiledPOUFlags.SkipParseTreeDuplication))
			{
				return;
			}
			if (this.CompiledPOU.ParseTreeRaw is IGreenTreeExprement)
			{
				_IStatement parseTreeRaw;
				this.ConvertParseTreeToRedTree(this.CompiledPOU.ParseTreeRaw, this.CompactedParseTreeInformation, out parseTreeRaw);
				this.CompiledPOU.ParseTreeRaw = parseTreeRaw;
			}
			else
			{
				this.CompiledPOU.ParseTreeRaw = (this.GetParseTree().Duplicate() as _IStatement);
			}
			this.CompiledPOU.SetFlagInternal(InternalCompiledPOUFlags.SkipParseTreeDuplication, true);
		}

		// Token: 0x060002A9 RID: 681 RVA: 0x0000A4E8 File Offset: 0x000094E8
		public _IStatement GetParseTreeForSerialization(bool bDeleteComments)
		{
			_IStatement istatement = this.CompiledPOU.ParseTreeRaw;
			bool flag = false;
			if (this.CompiledPOU.ParseTreeRaw is IGreenTreeExprement)
			{
				this.ConvertParseTreeToRedTree(this.CompiledPOU.ParseTreeRaw, this.CompactedParseTreeInformation, out istatement);
				flag = true;
			}
			if (bDeleteComments)
			{
				if (!flag)
				{
					istatement = (this.CompiledPOU.ParseTreeRaw.Duplicate() as _IStatement);
				}
				CompilerProxy.RemoveStatementComments(istatement);
			}
			return istatement;
		}

		// Token: 0x060002AA RID: 682 RVA: 0x0000A554 File Offset: 0x00009554
		private void ConvertParseTreeToRedTree(_IStatement parseTree, ICompactedParseTreeInformation parseTreeInfo, out _IStatement redParseTree)
		{
			redParseTree = parseTree;
			if (parseTree == null)
			{
				return;
			}
			if (parseTree is _IEmptyStatement)
			{
				return;
			}
			if (!(parseTree is IGreenTreeExprement))
			{
				return;
			}
			Debug.Assert(parseTree is _ISequenceStatement);
			Debug.Assert(parseTree is IGreenTreeExprement);
			redParseTree = (this.GreenTreeConverter.BuildRedTree(parseTree, this.TreeFactory, parseTreeInfo) as _IStatement);
		}

		// Token: 0x0400006A RID: 106
		private ICompactedParseTreeInformation _compactedParseTreeInformation;
	}
}
