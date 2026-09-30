using System;
using System.Runtime.CompilerServices;
using \u0008;
using \u000E;
using \u0014;
using \u0019;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace \u001C
{
	// Token: 0x02000275 RID: 629
	internal sealed class \u0010 : AbstractReplacer, IReplacer, ISpecificExpressionReplacer, IExpressionAndStatementReplacer
	{
		// Token: 0x1700072C RID: 1836
		// (get) Token: 0x060027F0 RID: 10224 RVA: 0x0008B2B0 File Offset: 0x000894B0
		private \u0081.\u0010 Visitor { get; }

		// Token: 0x060027F1 RID: 10225 RVA: 0x0008B2B8 File Offset: 0x000894B8
		private \u0010(global::\u000E.\u0011 \u0083\u0005)
		{
			this.\u0001 = \u0083\u0005;
			this.Visitor = \u0081.\u0010.\u0001(this, \u0083\u0005);
		}

		// Token: 0x060027F2 RID: 10226 RVA: 0x0008B2D4 File Offset: 0x000894D4
		public void ReplaceCode(_ICompiledPOU cpou)
		{
			this.\u0001.CompiledPOU = cpou;
			this.Visitor.ReplaceCode(cpou);
		}

		// Token: 0x1700072D RID: 1837
		// (get) Token: 0x060027F3 RID: 10227 RVA: 0x0008B2F0 File Offset: 0x000894F0
		// (set) Token: 0x060027F4 RID: 10228 RVA: 0x0008B318 File Offset: 0x00089518
		private _ISequenceStatement SequenceToReplace
		{
			get
			{
				_ISequenceStatement result;
				if ((result = this.\u0001) == null)
				{
					result = (this.\u0001 = global::\u0019.\u0003.\u0001());
				}
				return result;
			}
			set
			{
				this.\u0001 = value;
			}
		}

		// Token: 0x1700072E RID: 1838
		// (get) Token: 0x060027F5 RID: 10229 RVA: 0x0008B324 File Offset: 0x00089524
		// (set) Token: 0x060027F6 RID: 10230 RVA: 0x0008B34C File Offset: 0x0008954C
		private _ISequenceStatement SequenceToInsert
		{
			get
			{
				_ISequenceStatement result;
				if ((result = this.\u0002) == null)
				{
					result = (this.\u0002 = global::\u0019.\u0003.\u0001());
				}
				return result;
			}
			set
			{
				this.\u0002 = value;
			}
		}

		// Token: 0x060027F7 RID: 10231 RVA: 0x0008B358 File Offset: 0x00089558
		public _IStatement TakeCurrentStatementToReplace()
		{
			_IStatement u = this.\u0001;
			this.SequenceToReplace = null;
			return u;
		}

		// Token: 0x060027F8 RID: 10232 RVA: 0x0008B368 File Offset: 0x00089568
		public _IStatement TakeCurrentStatementToInsert()
		{
			_IStatement u = this.\u0002;
			this.SequenceToInsert = null;
			return u;
		}

		// Token: 0x060027F9 RID: 10233 RVA: 0x0008B378 File Offset: 0x00089578
		public _IExpression ReplaceAssignmentOnStatementPosition(_IAssignmentExpression assign)
		{
			return this.\u0002(assign);
		}

		// Token: 0x060027FA RID: 10234 RVA: 0x0008B384 File Offset: 0x00089584
		public override _IExpression ReplaceAssignmentExpression(_IAssignmentExpression assignmentExpression)
		{
			return this.\u0001(assignmentExpression);
		}

		// Token: 0x060027FB RID: 10235 RVA: 0x0008B390 File Offset: 0x00089590
		public bool \u0001(_IAssignmentExpression \u0002)
		{
			_IVariable ivariable = \u0002._LValue.GetVariable(this.\u0001._Scope) as _IVariable;
			bool flag;
			IVariable variable;
			this.\u0001(\u0002, out flag, out variable);
			return flag || (ivariable != null && ivariable.IsProperty && (\u0002.KindOf == Operator.SetAssign || \u0002.KindOf == Operator.ResetAssign));
		}

		// Token: 0x060027FC RID: 10236 RVA: 0x0008B3F4 File Offset: 0x000895F4
		private _IExpression \u0001(_IAssignmentExpression \u0002)
		{
			_IVariable u = \u0002._LValue.GetVariable(this.\u0001._Scope) as _IVariable;
			bool flag;
			IVariable u2;
			this.\u0001(\u0002, out flag, out u2);
			_IStatement istatement = this.\u0001(\u0002, flag, u);
			if (istatement != null)
			{
				this.SequenceToInsert.Add(istatement);
				_IExpression iexpression = (_IExpression)\u0002._RValue.Duplicate();
				iexpression._Position = global::\u0019.\u0003.\u0001(0L, 0);
				return this.\u0001.Generator.\u0001<_IExpression>(iexpression, this.\u0001._Scope, this.\u0001.CompiledPOU);
			}
			if (flag)
			{
				return this.\u0001(\u0002, u2);
			}
			return \u0002;
		}

		// Token: 0x060027FD RID: 10237 RVA: 0x0008B49C File Offset: 0x0008969C
		private _IExpression \u0002(_IAssignmentExpression \u0002)
		{
			_IVariable u = \u0002._LValue.GetVariable(this.\u0001._Scope) as _IVariable;
			bool flag;
			IVariable u2;
			this.\u0001(\u0002, out flag, out u2);
			_IStatement istatement = this.\u0001(\u0002, flag, u);
			if (istatement != null)
			{
				this.SequenceToReplace.Add(istatement);
				return null;
			}
			if (flag)
			{
				return this.\u0001(\u0002, u2);
			}
			return null;
		}

		// Token: 0x060027FE RID: 10238 RVA: 0x0008B4F8 File Offset: 0x000896F8
		private void \u0001(_IAssignmentExpression \u0002, out bool \u0003, out IVariable \u0004)
		{
			\u0003 = false;
			\u0004 = null;
			_ICompoAccessExpression icompoAccessExpression = \u0002._LValue as _ICompoAccessExpression;
			if (icompoAccessExpression != null)
			{
				\u0004 = icompoAccessExpression._Left.GetVariable(this.\u0001._Scope);
				if (icompoAccessExpression.Right.IsLiteral && \u0004 != null && (\u0004.HasAttribute(CompileAttributes.SET_BITACCESS) || \u0004.HasAttribute(CompileAttributes.DEVICE_PARAMETER)))
				{
					\u0003 = true;
				}
			}
		}

		// Token: 0x060027FF RID: 10239 RVA: 0x0008B564 File Offset: 0x00089764
		private _IStatement \u0001(_IAssignmentExpression \u0002, bool \u0003, _IVariable \u0004)
		{
			if ((\u0003 && (\u0002.KindOf == Operator.SetAssign || \u0002.KindOf == Operator.ResetAssign)) || (\u0004 != null && \u0004.IsProperty && (\u0002.KindOf == Operator.SetAssign || \u0002.KindOf == Operator.ResetAssign)))
			{
				_IIfStatement iifStatement = global::\u0019.\u0003.\u0001();
				iifStatement._Condition = (\u0002._RValue.Duplicate() as _IExpression);
				_IAssignmentExpression iassignmentExpression = global::\u0019.\u0003.\u0001(\u0002._LValue);
				iassignmentExpression._RValue = global::\u0019.\u0003.\u0001(\u0002.KindOf != Operator.ResetAssign);
				this.\u0001.Generator.\u0001<_IAssignmentExpression>(iassignmentExpression, this.\u0001._Scope, this.\u0001.CompiledPOU);
				_IExpression u = this.ReplaceAssignmentExpression(iassignmentExpression);
				_ISequenceStatement isequenceStatement = global::\u0019.\u0003.\u0001();
				isequenceStatement.Add(global::\u0019.\u0003.\u0001(u, Token.Empty));
				iifStatement._IfThen = isequenceStatement;
				_IIfStatement iifStatement2 = this.\u0001.Generator.\u0001<_IIfStatement>(iifStatement, this.\u0001._Scope, this.\u0001.CompiledPOU);
				iifStatement2._Position = \u0002._Position;
				return iifStatement2;
			}
			return null;
		}

		// Token: 0x06002800 RID: 10240 RVA: 0x0008B68C File Offset: 0x0008988C
		private _IExpression \u0001(_IAssignmentExpression \u0002, IVariable \u0003)
		{
			string text;
			if (\u0003.HasAttribute(CompileAttributes.SET_BITACCESS))
			{
				text = \u0003.GetAttributeValue(CompileAttributes.SET_BITACCESS);
				text = text.Replace("BITNR", ((ICompoAccessExpression)\u0002._LValue).Right.ToString());
				text = Helper.\u0001(text, ((_ICompoAccessExpression)\u0002._LValue)._Left);
				text = text.Replace(\u0003.OrgName, \u0002._RValue.ToString());
			}
			else
			{
				bool flag;
				int @int = ((ICompoAccessExpression)\u0002._LValue).Right.Literal(this.\u0001._Scope).GetInt(out flag);
				text = global::\u0014.\u0013.\u0001(\u0003 as _IVariable, @int, \u0002._RValue.ToString());
			}
			_IExpression iexpression = this.\u0001.Generator.GenerateExpression(text, this.\u0001._Scope, this.\u0001.CompiledPOU);
			iexpression._Position = \u0002._Position;
			return iexpression;
		}

		// Token: 0x06002801 RID: 10241 RVA: 0x0008B778 File Offset: 0x00089978
		internal static ReplacerController \u0001(global::\u000E.\u0011 \u0002)
		{
			\u001C.\u0010 u = new \u001C.\u0010(\u0002);
			return new ReplacerController(u, new global::\u0008.\u000F(u));
		}

		// Token: 0x04000764 RID: 1892
		private global::\u000E.\u0011 \u0001;

		// Token: 0x04000765 RID: 1893
		[CompilerGenerated]
		private readonly \u0081.\u0010 \u0001;

		// Token: 0x04000766 RID: 1894
		private _ISequenceStatement \u0001;

		// Token: 0x04000767 RID: 1895
		private _ISequenceStatement \u0002;
	}
}
