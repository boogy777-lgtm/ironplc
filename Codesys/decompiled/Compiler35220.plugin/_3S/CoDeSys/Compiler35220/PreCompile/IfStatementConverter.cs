using System;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.PreCompile
{
	// Token: 0x0200015D RID: 349
	public static class IfStatementConverter
	{
		// Token: 0x0600182A RID: 6186 RVA: 0x0004B350 File Offset: 0x00049550
		public static void ReplaceElseIfs(_IIfStatement ifstatement)
		{
			if (ifstatement._ElseIf.Count > 0)
			{
				_IStatement ifElse = ifstatement._IfElse;
				for (int i = ifstatement._ElseIf.Count - 1; i >= 0; i--)
				{
					_IElseIf ielseIf = ifstatement._ElseIf[i];
					_IIfStatement iifStatement = \u0003.\u0001();
					iifStatement._Condition = ielseIf._Condition;
					iifStatement._IfThen = ielseIf._Controlled;
					iifStatement._IfElse = ifElse;
					iifStatement._Position = ielseIf._Position;
					iifStatement.SetFlag(StatementFlag.GenerateFlow | StatementFlag.GenerateBP, true);
					_ISequenceStatement isequenceStatement = \u0003.\u0001(1);
					isequenceStatement._Position = ielseIf._Position;
					isequenceStatement.Add(iifStatement);
					ifElse = isequenceStatement;
				}
				ifstatement.ClearElseIf();
				ifstatement._IfElse = ifElse;
			}
		}
	}
}
