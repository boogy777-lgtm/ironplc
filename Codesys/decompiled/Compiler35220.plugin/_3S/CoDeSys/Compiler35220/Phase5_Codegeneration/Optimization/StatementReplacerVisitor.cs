using System;
using \u0006;
using \u0007;
using \u000E;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x0200027C RID: 636
	public class StatementReplacerVisitor : IStatementVisitor<_IStatement>, IReplacer
	{
		// Token: 0x06002828 RID: 10280 RVA: 0x0008B90C File Offset: 0x00089B0C
		internal StatementReplacerVisitor(IStatementVisitor<_IStatement> replacer, global::\u000E.\u0011 context)
		{
			this._replacer = replacer;
			this.\u0001 = context;
		}

		// Token: 0x06002829 RID: 10281 RVA: 0x0008B924 File Offset: 0x00089B24
		public void ReplaceCode(_ICompiledPOU cpou)
		{
			this.\u0001.CompiledPOU = cpou;
			cpou.GetParseTree().\u0001(this);
		}

		// Token: 0x0600282A RID: 10282 RVA: 0x0008B940 File Offset: 0x00089B40
		public _IStatement visit(_IWhileStatement whilst)
		{
			whilst._Controlled.\u0001(this);
			return this._replacer.visit(whilst);
		}

		// Token: 0x0600282B RID: 10283 RVA: 0x0008B95C File Offset: 0x00089B5C
		public _IStatement visit(_IRepeatStatement repeat)
		{
			repeat._Controlled.\u0001(this);
			return this._replacer.visit(repeat);
		}

		// Token: 0x0600282C RID: 10284 RVA: 0x0008B978 File Offset: 0x00089B78
		public _IStatement visit(_IForStatement forloop)
		{
			forloop._Controlled.\u0001(this);
			return this._replacer.visit(forloop);
		}

		// Token: 0x0600282D RID: 10285 RVA: 0x0008B994 File Offset: 0x00089B94
		public virtual _IStatement visit(_IIfStatement ifst)
		{
			ifst._IfThen.\u0001(this);
			if (ifst._IfElse != null)
			{
				ifst._IfElse.\u0001(this);
			}
			if (ifst._ElseIf != null && ifst._ElseIf.Count > 0)
			{
				throw new LateCompileErrorException();
			}
			return this._replacer.visit(ifst);
		}

		// Token: 0x0600282E RID: 10286 RVA: 0x0008B9EC File Offset: 0x00089BEC
		public _IStatement visit(_IPragmaStatement pragmaStatement)
		{
			int nId;
			if (pragmaStatement.IsLocalSignaturePragma(out nId))
			{
				this.\u0001._Scope.LocalSignature = this.\u0001._Scope[nId];
				((global::\u0007.\u0005)this.\u0001._Scope).\u0003();
			}
			return this._replacer.visit(pragmaStatement);
		}

		// Token: 0x0600282F RID: 10287 RVA: 0x0008BA48 File Offset: 0x00089C48
		public _IStatement visit(_IReturnStatement returnStatement)
		{
			return this._replacer.visit(returnStatement);
		}

		// Token: 0x06002830 RID: 10288 RVA: 0x0008BA58 File Offset: 0x00089C58
		public _IStatement visit(_ITryCatchStatement tryCatchStatement)
		{
			if (tryCatchStatement._ReplacedSequence != null)
			{
				tryCatchStatement._ReplacedSequence.\u0001(this);
			}
			else
			{
				tryCatchStatement._Try.\u0001(this);
				if (tryCatchStatement._Catch != null)
				{
					tryCatchStatement._Catch.\u0001(this);
				}
				if (tryCatchStatement._Finally != null)
				{
					tryCatchStatement._Finally.\u0001(this);
				}
			}
			return this._replacer.visit(tryCatchStatement);
		}

		// Token: 0x06002831 RID: 10289 RVA: 0x0008BAC0 File Offset: 0x00089CC0
		public _IStatement visit(_IExpressionStatement expstat)
		{
			return this._replacer.visit(expstat);
		}

		// Token: 0x06002832 RID: 10290 RVA: 0x0008BAD0 File Offset: 0x00089CD0
		public _IStatement visit(_ICaseStatement caseStatement)
		{
			foreach (_ICase icase in caseStatement._Cases)
			{
				icase._Controlled.\u0001(this);
			}
			if (caseStatement._Else != null)
			{
				caseStatement._Else.\u0001(this);
			}
			return this._replacer.visit(caseStatement);
		}

		// Token: 0x06002833 RID: 10291 RVA: 0x0008BB44 File Offset: 0x00089D44
		public _IStatement visit(_ISequenceStatement sequenceStatement)
		{
			for (int i = 0; i < sequenceStatement._StatementList.Count; i++)
			{
				_IStatement istatement = sequenceStatement._StatementList[i].\u0001(this);
				if (istatement != null)
				{
					sequenceStatement.Replace(istatement, i);
				}
			}
			return null;
		}

		// Token: 0x06002834 RID: 10292 RVA: 0x0008BB88 File Offset: 0x00089D88
		public _IStatement visitGeneric(_IStatement statement)
		{
			return this._replacer.visitGeneric(statement);
		}

		// Token: 0x0400076C RID: 1900
		protected readonly IStatementVisitor<_IStatement> _replacer;

		// Token: 0x0400076D RID: 1901
		private global::\u000E.\u0011 \u0001;
	}
}
