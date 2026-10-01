using System;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000AC RID: 172
	internal static class SourceInformationSynchronizer
	{
		// Token: 0x06000A4B RID: 2635 RVA: 0x00017600 File Offset: 0x00016600
		public static void SignatureChanged(Guid guidApplication, ISignature signOld, ISignature signNew, bool bSignificant)
		{
			if (signOld != null && !bSignificant)
			{
				if (guidApplication != Guid.Empty)
				{
					SourceInformationSynchronizer.UpdateOnlineVariableComments(APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.GetCompiledApplicationSet(guidApplication) as _ICompileContext, signNew);
					return;
				}
				foreach (ILMCompiledApplicationSet ilmcompiledApplicationSet in APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.CompiledApplicationSets)
				{
					SourceInformationSynchronizer.UpdateOnlineVariableComments((_ICompileContext)ilmcompiledApplicationSet, signNew);
				}
			}
		}

		// Token: 0x06000A4C RID: 2636 RVA: 0x00017694 File Offset: 0x00016694
		public static void PouChanged(Guid guidApplication, _ICompiledPOU cpouOld, _ICompiledPOU cpouNew, out LDictionary<long, long> lmap)
		{
			LDictionary<IMinimalPosition, IMinimalPosition> ldictionary = null;
			lmap = new LDictionary<long, long>();
			if (cpouOld != null)
			{
				if (guidApplication != Guid.Empty)
				{
					SourceInformationSynchronizer.UpdateSourcePositionsOfBreakpoints(APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.GetCompiledApplicationSet(guidApplication) as CompileContext, cpouOld, cpouNew, ref ldictionary);
				}
				else
				{
					foreach (ILMCompiledApplicationSet ilmcompiledApplicationSet in APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.CompiledApplicationSets)
					{
						SourceInformationSynchronizer.UpdateSourcePositionsOfBreakpoints((_ICompileContext)ilmcompiledApplicationSet, cpouOld, cpouNew, ref ldictionary);
					}
				}
			}
			if (ldictionary != null)
			{
				foreach (IMinimalPosition minimalPosition in ldictionary.Keys)
				{
					IMinimalPosition minimalPosition2 = ldictionary[minimalPosition];
					SourcePosition sourcePosition = new SourcePosition(-1, Guid.Empty, minimalPosition.EditorPosition, minimalPosition.PositionOffset, 0);
					SourcePosition sourcePosition2 = new SourcePosition(-1, Guid.Empty, minimalPosition2.EditorPosition, minimalPosition2.PositionOffset, 0);
					lmap[sourcePosition.PositionCombination] = sourcePosition2.PositionCombination;
				}
			}
		}

		// Token: 0x06000A4D RID: 2637 RVA: 0x000177C8 File Offset: 0x000167C8
		private static void UpdateOnlineVariableComments(_ICompileContext comcon, ISignature signNew)
		{
			if (comcon != null)
			{
				_ISignature isignature = comcon[signNew.ObjectGuid];
				if (isignature != null)
				{
					foreach (_IVariable ivariable in signNew.All.OfType<_IVariable>())
					{
						_IVariable ivariable2 = isignature[ivariable.Name] as _IVariable;
						if (ivariable2 != null)
						{
							ivariable2.SetAttributeValue(CompileAttributes.ATTRIBUTE_COMMENT, ivariable.Comment);
						}
					}
				}
			}
		}

		// Token: 0x06000A4E RID: 2638 RVA: 0x0001784C File Offset: 0x0001684C
		private static void UpdateSourcePositionsOfBreakpoints(_ICompileContext comconCompiled, _ICompiledPOU cpouOld, _ICompiledPOU cpouNew, ref LDictionary<IMinimalPosition, IMinimalPosition> mappos)
		{
			if (comconCompiled != null)
			{
				_ICompiledPOU icompiledPOU = comconCompiled.GetCompiledPOU(cpouNew.ObjectGuid) as _ICompiledPOU;
				if (cpouOld != null && icompiledPOU != null && icompiledPOU.BreakpointList != null)
				{
					ExpressionComparer expressionComparer = new ExpressionComparer(cpouNew, true);
					expressionComparer.visit(cpouOld);
					mappos = expressionComparer.PositionTable;
					(icompiledPOU.BreakpointList as _IBreakpointList).UpdateSourcePositions(mappos);
				}
			}
		}
	}
}
