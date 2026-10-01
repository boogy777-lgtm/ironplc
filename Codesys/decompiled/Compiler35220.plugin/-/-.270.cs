using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0017;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001E
{
	// Token: 0x020002D2 RID: 722
	internal sealed class \u0013 : \u0017.\u0014, IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor, IExprementVisitor2
	{
		// Token: 0x06002B6E RID: 11118 RVA: 0x00098FA8 File Offset: 0x000971A8
		private \u0013()
		{
		}

		// Token: 0x1700078A RID: 1930
		// (get) Token: 0x06002B6F RID: 11119 RVA: 0x00098FBC File Offset: 0x000971BC
		// (set) Token: 0x06002B70 RID: 11120 RVA: 0x00098FC4 File Offset: 0x000971C4
		private _ICompiledPOU2 CurrentPOU { get; set; }

		// Token: 0x1700078B RID: 1931
		// (get) Token: 0x06002B71 RID: 11121 RVA: 0x00098FD0 File Offset: 0x000971D0
		// (set) Token: 0x06002B72 RID: 11122 RVA: 0x00098FD8 File Offset: 0x000971D8
		private int ID { get; set; }

		// Token: 0x1700078C RID: 1932
		// (get) Token: 0x06002B73 RID: 11123 RVA: 0x00098FE4 File Offset: 0x000971E4
		// (set) Token: 0x06002B74 RID: 11124 RVA: 0x00098FEC File Offset: 0x000971EC
		private bool ReturnReplaced { get; set; }

		// Token: 0x1700078D RID: 1933
		// (get) Token: 0x06002B75 RID: 11125 RVA: 0x00098FF8 File Offset: 0x000971F8
		private string LocalReturn
		{
			get
			{
				return string.Format("TryCatchReturnLabel_{0}_{1}", this.CurrentPOU.SignatureId, this.ID);
			}
		}

		// Token: 0x06002B76 RID: 11126 RVA: 0x00099020 File Offset: 0x00097220
		internal static bool \u0001(_ISequenceStatement \u0002, _ICompiledPOU2 \u0003, int \u0004)
		{
			if (\u0002 == null)
			{
				return false;
			}
			\u001E.\u0013 u = new \u001E.\u0013
			{
				CurrentPOU = \u0003,
				ID = \u0004
			};
			u.\u0001(\u0002);
			\u0002.AddStatement(\u0019.\u0003.\u0001(u.LocalReturn));
			return u.ReturnReplaced;
		}

		// Token: 0x06002B77 RID: 11127 RVA: 0x00099064 File Offset: 0x00097264
		[ExcludeFromCodeCoverage]
		public void \u0001(_ICompiledPOU \u0002)
		{
			_ISequenceStatement isequenceStatement = \u0002.ParseTree as _ISequenceStatement;
			if (isequenceStatement != null)
			{
				isequenceStatement.Accept(this);
			}
		}

		// Token: 0x06002B78 RID: 11128 RVA: 0x00099088 File Offset: 0x00097288
		public void \u0001(_ITryCatchStatement \u0002)
		{
			\u0002._ReplacedSequence.Accept(this);
		}

		// Token: 0x06002B79 RID: 11129 RVA: 0x00099098 File Offset: 0x00097298
		public void \u0001(_ISequenceStatement \u0002)
		{
			for (int i = 0; i < \u0002._StatementList.Count; i++)
			{
				_IExprement iexprement = \u0002._StatementList[i];
				this.\u0001.Push(null);
				iexprement.Accept(this);
				_IStatement istatement = this.\u0001.Pop();
				if (istatement != null)
				{
					\u0002.RemoveStatement(i);
					if (i == \u0002.StatementList.ToList<IStatement>().Count)
					{
						\u0002.AddStatement(istatement);
					}
					else
					{
						\u0002.InsertStatement(i, istatement);
					}
				}
			}
		}

		// Token: 0x06002B7A RID: 11130 RVA: 0x00099114 File Offset: 0x00097314
		public void \u0001(_IReturnStatement \u0002)
		{
			_ISequenceStatement isequenceStatement = \u0019.\u0003.\u0001();
			this.ReturnReplaced = true;
			isequenceStatement.AddStatement(\u0019.\u0003.\u0001(\u0019.\u0003.\u0001("__tryreturn"), \u0019.\u0003.\u0001(true)));
			isequenceStatement.AddStatement(\u0019.\u0003.\u0001(this.LocalReturn));
			if (\u0002._Condition != null)
			{
				_IIfStatement state = \u0019.\u0003.\u0001(\u0002._Condition, isequenceStatement);
				isequenceStatement = \u0019.\u0003.\u0001();
				isequenceStatement.AddStatement(state);
			}
			this.\u0001.Pop();
			this.\u0001.Push(isequenceStatement);
		}

		// Token: 0x06002B7B RID: 11131 RVA: 0x00099194 File Offset: 0x00097394
		public void \u0001(_IRepeatStatement \u0002)
		{
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x06002B7C RID: 11132 RVA: 0x000991A4 File Offset: 0x000973A4
		public void \u0001(_IForStatement \u0002)
		{
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x06002B7D RID: 11133 RVA: 0x000991B4 File Offset: 0x000973B4
		public void \u0001(_IWhileStatement \u0002)
		{
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x06002B7E RID: 11134 RVA: 0x000991C4 File Offset: 0x000973C4
		public void \u0001(_ICaseStatement \u0002)
		{
			foreach (_ICase icase in \u0002._Cases)
			{
				icase._Controlled.Accept(this);
			}
			if (\u0002._Else != null)
			{
				\u0002._Else.Accept(this);
			}
		}

		// Token: 0x06002B7F RID: 11135 RVA: 0x00099228 File Offset: 0x00097428
		public void \u0001(_IIfStatement \u0002)
		{
			\u0002._IfThen.Accept(this);
			if (\u0002._IfElse != null)
			{
				\u0002._IfElse.Accept(this);
			}
		}

		// Token: 0x06002B80 RID: 11136 RVA: 0x0009924C File Offset: 0x0009744C
		public void \u0001(_IExitStatement \u0002)
		{
		}

		// Token: 0x06002B81 RID: 11137 RVA: 0x00099250 File Offset: 0x00097450
		public void \u0001(_IContinueStatement \u0002)
		{
		}

		// Token: 0x06002B82 RID: 11138 RVA: 0x00099254 File Offset: 0x00097454
		[ExcludeFromCodeCoverage]
		public void \u0001(_IPragmaIfStatement \u0002)
		{
			\u0002.IfThen.Accept(this);
			if (\u0002.IfElse != null)
			{
				\u0002.IfElse.Accept(this);
			}
		}

		// Token: 0x0400084E RID: 2126
		[CompilerGenerated]
		private _ICompiledPOU2 \u0001;

		// Token: 0x0400084F RID: 2127
		[CompilerGenerated]
		private int \u0001;

		// Token: 0x04000850 RID: 2128
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x04000851 RID: 2129
		private readonly Stack<_IStatement> \u0001 = new Stack<_IStatement>();
	}
}
