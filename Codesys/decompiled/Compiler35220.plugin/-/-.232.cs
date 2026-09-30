using System;
using System.Collections.Generic;
using System.Linq;
using \u0008;
using \u000E;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace \u0006
{
	// Token: 0x0200028B RID: 651
	internal sealed class \u0007 : global::\u0008.\u0010
	{
		// Token: 0x060028E0 RID: 10464 RVA: 0x0008EF20 File Offset: 0x0008D120
		public \u0007(\u0081.\u0010 \u0096\u0007, IExpressionAndStatementReplacer \u0018\u0006, global::\u000E.\u0011 \u0083\u0005) : base(\u0096\u0007, \u0018\u0006, \u0083\u0005)
		{
			this.\u0001 = \u0018\u0006;
		}

		// Token: 0x060028E1 RID: 10465 RVA: 0x0008EF34 File Offset: 0x0008D134
		public override void \u0001(_ISequenceStatement \u0002)
		{
			IList<_IStatement> statementList = \u0002._StatementList;
			IList<Tuple<int, _IStatement>> list = new List<Tuple<int, _IStatement>>();
			for (int i = 0; i < statementList.Count; i++)
			{
				statementList[i].Accept(this);
				_IStatement istatement = this.\u0001.TakeCurrentStatementToReplace();
				if (istatement != null)
				{
					statementList[i] = istatement;
				}
				_IStatement istatement2 = this.\u0001.TakeCurrentStatementToInsert();
				if (istatement2 != null)
				{
					list.Add(new Tuple<int, _IStatement>(i, istatement2));
				}
			}
			foreach (Tuple<int, _IStatement> tuple in list.Reverse<Tuple<int, _IStatement>>())
			{
				statementList.Insert(tuple.Item1, tuple.Item2);
			}
		}

		// Token: 0x060028E2 RID: 10466 RVA: 0x0008EFF8 File Offset: 0x0008D1F8
		public override void \u0001(_IExpressionStatement \u0002)
		{
			_IAssignmentExpression iassignmentExpression = \u0002._Expr as _IAssignmentExpression;
			if (iassignmentExpression != null)
			{
				iassignmentExpression._RValue.Accept(this);
				_IExpression iexpression = this.\u0001.ReplaceAssignmentOnStatementPosition(iassignmentExpression);
				if (iexpression != null)
				{
					\u0002._Expr = iexpression;
					return;
				}
			}
			else
			{
				\u0002._Expr.Accept(this);
			}
		}

		// Token: 0x0400078C RID: 1932
		private new readonly IExpressionAndStatementReplacer \u0001;
	}
}
