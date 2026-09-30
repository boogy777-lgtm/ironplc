using System;
using System.Collections.Generic;
using \u0003;
using \u0019;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001C
{
	// Token: 0x0200016C RID: 364
	internal sealed class \u0008 : ChildVisitor
	{
		// Token: 0x060018BF RID: 6335 RVA: 0x0004CCB8 File Offset: 0x0004AEB8
		private \u0008(ILanguageModelBuilder12 \u000F\u0008, IReadOnlyCollection<IEmbeddedLanguageService> \u0010\u0008, ISequenceStatement \u0011\u0008, ILMPOU \u0087\u0008)
		{
			this.\u0001 = \u000F\u0008;
			this.\u0001 = \u0010\u0008;
			this.\u0001 = \u0011\u0008;
			this.\u0001 = \u0087\u0008;
		}

		// Token: 0x060018C0 RID: 6336 RVA: 0x0004CCE8 File Offset: 0x0004AEE8
		public static void \u0001(ILanguageModelBuilder12 \u0002, IReadOnlyCollection<IEmbeddedLanguageService> \u0003, ISequenceStatement \u0004, _ISequenceStatement \u0005, ILMPOU \u0006)
		{
			new \u001C.\u0008(\u0002, \u0003, \u0004, \u0006).\u0002(\u0005);
		}

		// Token: 0x060018C1 RID: 6337 RVA: 0x0004CCFC File Offset: 0x0004AEFC
		private bool \u0001(_IEmbeddedLanguageStatement \u0002, out IStatement \u0003, out IStatement \u0004)
		{
			foreach (IEmbeddedLanguageService embeddedLanguageService in this.\u0001)
			{
				try
				{
					if (embeddedLanguageService.TryReplace(this.\u0001, \u0002, this.\u0001, out \u0003, out \u0004))
					{
						return true;
					}
				}
				catch (Exception)
				{
				}
			}
			\u0003 = null;
			\u0004 = null;
			return false;
		}

		// Token: 0x060018C2 RID: 6338 RVA: 0x0004CD78 File Offset: 0x0004AF78
		protected override void VisitChild(_IExprement exprement)
		{
			_ISequenceStatement isequenceStatement = exprement as _ISequenceStatement;
			if (isequenceStatement != null)
			{
				this.\u0001(isequenceStatement);
				return;
			}
			_IStatement istatement = exprement as _IStatement;
			if (istatement != null)
			{
				this.\u0001.Push(istatement);
			}
		}

		// Token: 0x060018C3 RID: 6339 RVA: 0x0004CDB0 File Offset: 0x0004AFB0
		private void \u0001(_ISequenceStatement \u0002)
		{
			IList<_IStatement> statementList = \u0002._StatementList;
			for (int i = 0; i < statementList.Count; i++)
			{
				_IEmbeddedLanguageStatement iembeddedLanguageStatement = statementList[i] as _IEmbeddedLanguageStatement;
				if (iembeddedLanguageStatement != null)
				{
					this.\u0001(iembeddedLanguageStatement, statementList, ref i);
				}
				else
				{
					this.\u0001.Push(statementList[i]);
				}
			}
		}

		// Token: 0x060018C4 RID: 6340 RVA: 0x0004CE04 File Offset: 0x0004B004
		private void \u0001(_IEmbeddedLanguageStatement \u0002, IList<_IStatement> \u0003, ref int \u0004)
		{
			IStatement statement;
			IStatement statement2;
			if (this.\u0001(\u0002, out statement, out statement2))
			{
				_IStatement istatement = statement as _IStatement;
				if (istatement != null && !(istatement is _IEmptyStatement))
				{
					_ISequenceStatement isequenceStatement = istatement as _ISequenceStatement;
					if (isequenceStatement == null || isequenceStatement._StatementList.Count != 0)
					{
						\u0003[\u0004] = istatement;
						goto IL_4F;
					}
				}
				\u0003.RemoveAt(\u0004);
				\u0004--;
				IL_4F:
				if (statement2 != null)
				{
					this.\u0001.AddStatement(statement2);
					return;
				}
			}
			else
			{
				\u0003[\u0004] = \u0019.\u0003.\u0001();
				global::\u0003.\u0006.\u0002(\u0003[\u0004], MessageId.Err_UnknownEmbeddedLanguageType, new object[]
				{
					\u0002.Kind
				});
				\u0003[\u0004].SetPositionIntern(\u0002.PositionIntern);
			}
		}

		// Token: 0x060018C5 RID: 6341 RVA: 0x0004CEB4 File Offset: 0x0004B0B4
		private void \u0002(_ISequenceStatement \u0002)
		{
			this.VisitChild(\u0002);
			while (this.\u0001.Count > 0)
			{
				this.\u0001.Pop().Accept(this);
			}
		}

		// Token: 0x0400045C RID: 1116
		private readonly ILanguageModelBuilder12 \u0001;

		// Token: 0x0400045D RID: 1117
		private readonly IReadOnlyCollection<IEmbeddedLanguageService> \u0001;

		// Token: 0x0400045E RID: 1118
		private readonly Stack<_IStatement> \u0001 = new Stack<_IStatement>();

		// Token: 0x0400045F RID: 1119
		private readonly ISequenceStatement \u0001;

		// Token: 0x04000460 RID: 1120
		private readonly ILMPOU \u0001;
	}
}
