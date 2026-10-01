using System;
using \u0006;
using \u0007;
using \u000E;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x02000283 RID: 643
	public class ExpressionStatementReplacerVisitor : IStatementVisitor<_IStatement>, IReplacer
	{
		// Token: 0x060028A6 RID: 10406 RVA: 0x0008E584 File Offset: 0x0008C784
		internal ExpressionStatementReplacerVisitor(IExpressionStatementReplacer replacer, global::\u000E.\u0011 context)
		{
			this.\u0001 = replacer;
			this.\u0001 = context;
		}

		// Token: 0x060028A7 RID: 10407 RVA: 0x0008E59C File Offset: 0x0008C79C
		public void ReplaceCode(_ICompiledPOU cpou)
		{
			this.\u0001 = cpou;
			this.\u0001.GetParseTree().\u0001(this);
		}

		// Token: 0x060028A8 RID: 10408 RVA: 0x0008E5B8 File Offset: 0x0008C7B8
		public _IStatement visit(_IWhileStatement whilst)
		{
			whilst._Controlled.\u0001(this);
			return null;
		}

		// Token: 0x060028A9 RID: 10409 RVA: 0x0008E5C8 File Offset: 0x0008C7C8
		public _IStatement visit(_IRepeatStatement repeat)
		{
			repeat._Controlled.\u0001(this);
			return null;
		}

		// Token: 0x060028AA RID: 10410 RVA: 0x0008E5D8 File Offset: 0x0008C7D8
		public _IStatement visit(_IForStatement forloop)
		{
			forloop._Controlled.\u0001(this);
			return null;
		}

		// Token: 0x060028AB RID: 10411 RVA: 0x0008E5E8 File Offset: 0x0008C7E8
		public _IStatement visit(_IIfStatement ifst)
		{
			ifst._IfThen.\u0001(this);
			if (ifst._IfElse != null)
			{
				ifst._IfElse.\u0001(this);
			}
			return null;
		}

		// Token: 0x060028AC RID: 10412 RVA: 0x0008E610 File Offset: 0x0008C810
		public _IStatement visit(_IPragmaStatement pragmaStatement)
		{
			int nId;
			if (pragmaStatement.IsLocalSignaturePragma(out nId))
			{
				this.\u0001._Scope.LocalSignature = this.\u0001._Scope[nId];
				((global::\u0007.\u0005)this.\u0001._Scope).\u0003();
			}
			return null;
		}

		// Token: 0x060028AD RID: 10413 RVA: 0x0008E668 File Offset: 0x0008C868
		public _IStatement visit(_IReturnStatement returnStatement)
		{
			return null;
		}

		// Token: 0x060028AE RID: 10414 RVA: 0x0008E66C File Offset: 0x0008C86C
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
			return null;
		}

		// Token: 0x060028AF RID: 10415 RVA: 0x0008E6C8 File Offset: 0x0008C8C8
		public _IStatement visit(_IExpressionStatement expstat)
		{
			return this.\u0001.ReplaceExpressionStatement(expstat, this.\u0001);
		}

		// Token: 0x060028B0 RID: 10416 RVA: 0x0008E6DC File Offset: 0x0008C8DC
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
			return null;
		}

		// Token: 0x060028B1 RID: 10417 RVA: 0x0008E744 File Offset: 0x0008C944
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

		// Token: 0x060028B2 RID: 10418 RVA: 0x0008E788 File Offset: 0x0008C988
		public _IStatement visitGeneric(_IStatement statement)
		{
			return null;
		}

		// Token: 0x04000777 RID: 1911
		private readonly IExpressionStatementReplacer \u0001;

		// Token: 0x04000778 RID: 1912
		private readonly global::\u000E.\u0011 \u0001;

		// Token: 0x04000779 RID: 1913
		private _ICompiledPOU \u0001;
	}
}
