using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.CommonCompilerData;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000231 RID: 561
	public class GreenTreeContext
	{
		// Token: 0x060024E2 RID: 9442 RVA: 0x0005C8FD File Offset: 0x0005B8FD
		private GreenTreeContext()
		{
		}

		// Token: 0x17000A5F RID: 2655
		// (get) Token: 0x060024E3 RID: 9443 RVA: 0x0005C910 File Offset: 0x0005B910
		public static GreenTreeContext Singleton { get; } = new GreenTreeContext();

		// Token: 0x17000A60 RID: 2656
		// (get) Token: 0x060024E4 RID: 9444 RVA: 0x0005C917 File Offset: 0x0005B917
		public GreenTreeTables Tables { get; } = new GreenTreeTables();

		// Token: 0x060024E5 RID: 9445 RVA: 0x0005C920 File Offset: 0x0005B920
		public void ConvertParseTreeToGreenTree(_ICompiledPOU cpou)
		{
			object tableLock = this.Tables.TableLock;
			lock (tableLock)
			{
				try
				{
					if (!GreenTreeContext.TEST_FOR_MEMORY)
					{
						_IStatement originalParseTree = ((CompiledPOU)cpou).OriginalParseTree;
						if (!(originalParseTree is _IEmptyStatement))
						{
							if (!(originalParseTree is IGreenTreeExprement))
							{
								if (CompilerProxy.GreenTreeConverter_OrNull != null)
								{
									Debug.Assert(originalParseTree != null);
									Debug.Assert(originalParseTree is _ISequenceStatement);
									CompactedPrecompileParseTreeInformation compactedPrecompileParseTreeInformation = new CompactedPrecompileParseTreeInformation();
									SequenceStatement_Green sequenceStatement_Green = (SequenceStatement_Green)CompilerProxy.GreenTreeConverter_OrNull.BuildGreenTree(originalParseTree, this.Tables, GreenTreeFactory.Singleton, compactedPrecompileParseTreeInformation);
									Debug.Assert(sequenceStatement_Green != null);
									cpou.SetParseTree(sequenceStatement_Green);
									((ICompiledPOUWithCompactedParseTree)cpou).CompactedParseTreeInformation = compactedPrecompileParseTreeInformation;
								}
							}
						}
					}
				}
				catch
				{
				}
			}
		}

		// Token: 0x060024E6 RID: 9446 RVA: 0x0005C9FC File Offset: 0x0005B9FC
		public _IExpression ConvertInitialValueToGreenTrees(IVariableWithCompactedInitialValue var, out CompactedPrecompileParseTreeInformation parseTreeInfo)
		{
			object tableLock = this.Tables.TableLock;
			_IExpression result;
			lock (tableLock)
			{
				parseTreeInfo = null;
				if (GreenTreeContext.TEST_FOR_MEMORY)
				{
					result = null;
				}
				else
				{
					_IExpression originalInitial = var.OriginalInitial;
					if (!GreenTreeContext.VarInitialValueNeedsCompaction(originalInitial))
					{
						result = null;
					}
					else
					{
						parseTreeInfo = new CompactedPrecompileParseTreeInformation();
						Expression_Green expression_Green = (Expression_Green)CompilerProxy.GreenTreeConverter_OrNull.BuildGreenTree(originalInitial, this.Tables, GreenTreeFactory.Singleton, parseTreeInfo);
						Debug.Assert(expression_Green != null);
						result = expression_Green;
					}
				}
			}
			return result;
		}

		// Token: 0x060024E7 RID: 9447 RVA: 0x0005CA8C File Offset: 0x0005BA8C
		public void CompactVariables(_ISignature sign)
		{
			if (GreenTreeContext.TEST_FOR_MEMORY)
			{
				return;
			}
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351600)
			{
				return;
			}
			LList<_IVariable> llist = new LList<_IVariable>();
			foreach (_IVariable ivariable in sign.AllVariables)
			{
				if (ivariable != null)
				{
					if (this.TestForExceptionList(ivariable))
					{
						llist.Add(ivariable);
					}
					else
					{
						llist.Add(GreenVariableFactory.CreateGreenVariable(ivariable));
					}
				}
			}
			(sign as _ISignature2).ReplaceVariablesByGreenVariables(llist);
		}

		// Token: 0x060024E8 RID: 9448 RVA: 0x0005CB20 File Offset: 0x0005BB20
		public bool TestForExceptionList(_IVariable var)
		{
			TypeClass @class = var._Type.Class;
			return @class == TypeClass.Lazy || @class == TypeClass.VarLenArray || GreenTreeContext.IsImplicitEnumVar(var) != null || var is AbstractGreenVariable;
		}

		// Token: 0x060024E9 RID: 9449 RVA: 0x0005CB5C File Offset: 0x0005BB5C
		public static IImplicitEnumerationType IsImplicitEnumVar(_IVariable var)
		{
			IImplicitEnumerationType implicitEnumerationType = var.Type as IImplicitEnumerationType;
			if (implicitEnumerationType == null && var.Type is _IArrayType)
			{
				implicitEnumerationType = (var._Type.BaseType as IImplicitEnumerationType);
			}
			return implicitEnumerationType;
		}

		// Token: 0x060024EA RID: 9450 RVA: 0x0005CB98 File Offset: 0x0005BB98
		private static bool VarInitialValueNeedsCompaction(_IExpression initialValueExp)
		{
			if (initialValueExp == null || initialValueExp is IGreenTreeExprement)
			{
				return false;
			}
			if (initialValueExp is _IStructureInitialization)
			{
				return true;
			}
			if (initialValueExp is _IArrayInitialization)
			{
				return true;
			}
			_IOperatorExpression ioperatorExpression = initialValueExp as _IOperatorExpression;
			return ioperatorExpression != null && ioperatorExpression._OperandsList.Count > 1;
		}

		// Token: 0x060024EB RID: 9451 RVA: 0x0005CBE4 File Offset: 0x0005BBE4
		public void ConvertParseTreeToRedTree(_IStatement parseTree, ICompactedParseTreeInformation parseTreeInfo, IGreenTreeConverter converter, ITreeFactory treeFactory, out _IStatement redParseTree)
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
			if (CompilerProxy.GreenTreeConverter_OrNull == null)
			{
				return;
			}
			Debug.Assert(parseTree is _ISequenceStatement);
			SequenceStatement_Green exp = (SequenceStatement_Green)parseTree;
			redParseTree = (converter.BuildRedTree(exp, treeFactory, parseTreeInfo) as _IStatement);
		}

		// Token: 0x060024EC RID: 9452 RVA: 0x0005CC3A File Offset: 0x0005BC3A
		public void ConvertParseTreeToRedTree(_IStatement parseTree, ICompactedParseTreeInformation parseTreeInfo, out _IStatement redParseTree)
		{
			this.ConvertParseTreeToRedTree(parseTree, parseTreeInfo, CompilerProxy.GreenTreeConverter_OrNull, RedTreeFactory.Singleton, out redParseTree);
		}

		// Token: 0x060024ED RID: 9453 RVA: 0x0005CC4F File Offset: 0x0005BC4F
		public void ConvertInitialValueToRedTree(_IExpression initialValueExp, ICompactedParseTreeInformation parseTreeInfo, out _IExpression redParseTree)
		{
			redParseTree = initialValueExp;
			if (CompilerProxy.GreenTreeConverter_OrNull == null)
			{
				return;
			}
			if (initialValueExp == null)
			{
				return;
			}
			if (!(initialValueExp is IGreenTreeExprement))
			{
				return;
			}
			redParseTree = (CompilerProxy.GreenTreeConverter_OrNull.BuildRedTree(initialValueExp, RedTreeFactory.Singleton, parseTreeInfo) as _IExpression);
		}

		// Token: 0x060024EE RID: 9454 RVA: 0x0005CC81 File Offset: 0x0005BC81
		public void ClearTables()
		{
			this.Tables.Clear();
		}

		// Token: 0x04000702 RID: 1794
		private static bool TEST_FOR_MEMORY = false;
	}
}
