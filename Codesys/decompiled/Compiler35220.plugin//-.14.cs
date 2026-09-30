using System;
using System.Runtime.CompilerServices;
using \u0002;
using \u000E;
using \u0016;
using \u0019;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0081
{
	// Token: 0x020002C9 RID: 713
	internal sealed class \u0013 : AbstractStatementReplacer, IReplacer
	{
		// Token: 0x1700077C RID: 1916
		// (get) Token: 0x06002B2C RID: 11052 RVA: 0x000981F0 File Offset: 0x000963F0
		private global::\u000E.\u0011 Context { get; }

		// Token: 0x1700077D RID: 1917
		// (get) Token: 0x06002B2D RID: 11053 RVA: 0x000981F8 File Offset: 0x000963F8
		private StatementReplacerVisitor Visitor { get; }

		// Token: 0x1700077E RID: 1918
		// (get) Token: 0x06002B2E RID: 11054 RVA: 0x00098200 File Offset: 0x00096400
		// (set) Token: 0x06002B2F RID: 11055 RVA: 0x00098208 File Offset: 0x00096408
		private _ICompiledPOU2 CompiledPou { get; set; }

		// Token: 0x1700077F RID: 1919
		// (get) Token: 0x06002B30 RID: 11056 RVA: 0x00098214 File Offset: 0x00096414
		private IScope5 _Scope
		{
			get
			{
				return this.Context._Scope;
			}
		}

		// Token: 0x17000780 RID: 1920
		// (get) Token: 0x06002B31 RID: 11057 RVA: 0x00098230 File Offset: 0x00096430
		// (set) Token: 0x06002B32 RID: 11058 RVA: 0x00098238 File Offset: 0x00096438
		private LList<_ISubRoutineStatement> Subroutines { get; set; }

		// Token: 0x06002B33 RID: 11059 RVA: 0x00098244 File Offset: 0x00096444
		private \u0013(global::\u000E.\u0011 \u0083\u0005, global::\u000E.\u0012 \u001F\u0006)
		{
			this.Context = \u0083\u0005;
			this.\u0001 = new global::\u0002.\u0008(this._Scope, \u0083\u0005.Comcon);
			this.\u0001 = new global::\u0016.\u0010(\u0083\u0005.Comcon);
			this.Visitor = new StatementReplacerVisitor(this, \u0083\u0005);
			this.\u0001 = \u001F\u0006;
		}

		// Token: 0x06002B34 RID: 11060 RVA: 0x0009829C File Offset: 0x0009649C
		public void ReplaceCode(_ICompiledPOU cpou)
		{
			this.CompiledPou = (_ICompiledPOU2)cpou;
			this.\u0001 = false;
			this.Subroutines = new LList<_ISubRoutineStatement>();
			_ISignature isignature = this.Context.Comcon[cpou.SignatureId];
			if (this.\u0001.ToVisitInternal || cpou.GetFlagInternal(InternalCompiledPOUFlags.ContainsTryCatch) || isignature.GetFlag(SignatureFlag.RawSTProperty))
			{
				this.Visitor.ReplaceCode(cpou);
			}
			if (!this.\u0001)
			{
				_ISequenceStatement isequenceStatement = this.\u0001.\u0001(cpou);
				_ISequenceStatement isequenceStatement2 = (_ISequenceStatement)cpou.GetParseTree();
				foreach (_IStatement sm in isequenceStatement._StatementList)
				{
					isequenceStatement2.Add(sm);
				}
			}
			if (this.Subroutines != null)
			{
				foreach (_ISubRoutineStatement sm2 in this.Subroutines)
				{
					((_ISequenceStatement)cpou.GetParseTree()).Add(sm2);
				}
			}
		}

		// Token: 0x06002B35 RID: 11061 RVA: 0x000983CC File Offset: 0x000965CC
		public override _IStatement visit(_ITryCatchStatement tryCatchStatement)
		{
			Debug.\u0001(tryCatchStatement._ReplacedSequence == null);
			this.Subroutines.Add(this.\u0001.\u0001(this.CompiledPou, tryCatchStatement, ref this.\u0001));
			return null;
		}

		// Token: 0x06002B36 RID: 11062 RVA: 0x00098400 File Offset: 0x00096600
		public override _IStatement visit(_IPragmaStatement pragmaStatement)
		{
			if (pragmaStatement.Text == "returnlabelposition" && !this.\u0001)
			{
				_IStatement result = this.\u0001.\u0001(this.CompiledPou);
				this.\u0001 = true;
				return result;
			}
			return null;
		}

		// Token: 0x06002B37 RID: 11063 RVA: 0x00098438 File Offset: 0x00096638
		public override _IStatement visit(_IRepeatStatement repeat)
		{
			_IExpression iexpression = repeat._Condition;
			if (iexpression is ICallExpression)
			{
				iexpression = \u0019.\u0003.\u0001(Operator.Equal, iexpression, \u0019.\u0003.\u0001(true));
				iexpression = this.Context.Generator.\u0001<_IExpression>(iexpression, this.Context._Scope, this.Context.CompiledPOU);
				this.Context.Generator.CopyPositionAndMessages(repeat._Condition, iexpression);
				repeat._Condition = iexpression;
			}
			return null;
		}

		// Token: 0x06002B38 RID: 11064 RVA: 0x000984BC File Offset: 0x000966BC
		internal static ReplacerController \u0001(global::\u000E.\u0011 \u0002)
		{
			global::\u000E.\u0012 u = new global::\u000E.\u0012(\u0002);
			return new ReplacerController(new \u0081.\u0013(\u0002, u), u);
		}

		// Token: 0x04000832 RID: 2098
		[CompilerGenerated]
		private readonly global::\u000E.\u0011 \u0001;

		// Token: 0x04000833 RID: 2099
		[CompilerGenerated]
		private readonly StatementReplacerVisitor \u0001;

		// Token: 0x04000834 RID: 2100
		[CompilerGenerated]
		private _ICompiledPOU2 \u0001;

		// Token: 0x04000835 RID: 2101
		private int \u0001;

		// Token: 0x04000836 RID: 2102
		private readonly global::\u0002.\u0008 \u0001;

		// Token: 0x04000837 RID: 2103
		private readonly global::\u0016.\u0010 \u0001;

		// Token: 0x04000838 RID: 2104
		[CompilerGenerated]
		private LList<_ISubRoutineStatement> \u0001;

		// Token: 0x04000839 RID: 2105
		private bool \u0001;

		// Token: 0x0400083A RID: 2106
		private readonly global::\u000E.\u0012 \u0001;
	}
}
