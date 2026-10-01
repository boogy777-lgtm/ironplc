using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Declaration
{
	// Token: 0x02000052 RID: 82
	internal class DeclarationStatementDetector : IStatementVisitor<bool>
	{
		// Token: 0x06000568 RID: 1384 RVA: 0x00016F4C File Offset: 0x0001514C
		public static bool CheckIfContainsDeclarationSyntax(_IPragmaIfStatement statement)
		{
			DeclarationStatementDetector visitor = new DeclarationStatementDetector();
			return statement.AcceptStatementVisitor(visitor);
		}

		// Token: 0x06000569 RID: 1385 RVA: 0x00016F68 File Offset: 0x00015168
		public bool visit(_ISequenceStatement statement)
		{
			using (IEnumerator<_IStatement> enumerator = statement._StatementList.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.AcceptStatementVisitor(this))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x0600056A RID: 1386 RVA: 0x00016FBC File Offset: 0x000151BC
		public bool visit(_ICommentStatement statement)
		{
			return false;
		}

		// Token: 0x0600056B RID: 1387 RVA: 0x00016FBC File Offset: 0x000151BC
		public bool visit(_IPragmaStatement statement)
		{
			return false;
		}

		// Token: 0x0600056C RID: 1388 RVA: 0x00016FC0 File Offset: 0x000151C0
		public bool visit(_IPragmaIfStatement statement)
		{
			if (statement.IfThen.AcceptStatementVisitor(this))
			{
				return true;
			}
			if (statement.IfElse.AcceptStatementVisitor(this))
			{
				return true;
			}
			if (statement.ElseIfs != null)
			{
				IElseIf[] elseIfs = statement.ElseIfs;
				for (int i = 0; i < elseIfs.Length; i++)
				{
					_ISequenceStatement isequenceStatement = elseIfs[i].Controlled as _ISequenceStatement;
					if (isequenceStatement != null && isequenceStatement.AcceptStatementVisitor(this))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x0600056D RID: 1389 RVA: 0x00017027 File Offset: 0x00015227
		public bool visit(_IVariableDeclarationStatement statement)
		{
			return true;
		}

		// Token: 0x0600056E RID: 1390 RVA: 0x00017027 File Offset: 0x00015227
		public bool visit(_IVariableDeclarationListStatement statement)
		{
			return true;
		}
	}
}
