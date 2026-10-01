using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0002;
using \u000E;
using \u000F;
using \u0015;
using \u0019;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u007F;
using \u0084;

namespace \u001A
{
	// Token: 0x020001FD RID: 509
	internal sealed class \u0007 : EmptyVisitor352000, IReplacer
	{
		// Token: 0x06002203 RID: 8707 RVA: 0x00075CBC File Offset: 0x00073EBC
		public \u0007(global::\u000E.\u0011 \u0083\u0005, _IDataManager \u000F\u0006)
		{
			this._Scope = \u0083\u0005._Scope;
			this.\u0001 = \u000F\u0006;
			this.\u0001 = \u0083\u0005.Comcon;
			this.\u0001 = new global::\u0015.\u0005(this);
			this.\u0001.Push(false);
			ICodegenerator3 codegenerator = \u0083\u0005.CodeGen as ICodegenerator3;
			this.\u0001 = (codegenerator == null || !codegenerator.GetProperty(CodegeneratorProperties.SupportsVectorOperations));
		}

		// Token: 0x06002204 RID: 8708 RVA: 0x00075D70 File Offset: 0x00073F70
		public static ReplacerController \u0001(global::\u000E.\u0011 \u0002, _IDataManager \u0003)
		{
			return new ReplacerController(new global::\u001A.\u0007(\u0002, \u0003), new \u0084.\u0012());
		}

		// Token: 0x06002205 RID: 8709 RVA: 0x00075D84 File Offset: 0x00073F84
		public void ReplaceCode(_ICompiledPOU cpou)
		{
			if (this.\u0001)
			{
				this.visit(cpou);
			}
		}

		// Token: 0x17000673 RID: 1651
		// (get) Token: 0x06002206 RID: 8710 RVA: 0x00075D98 File Offset: 0x00073F98
		// (set) Token: 0x06002207 RID: 8711 RVA: 0x00075DA0 File Offset: 0x00073FA0
		private IScope5 _Scope { get; set; }

		// Token: 0x17000674 RID: 1652
		// (get) Token: 0x06002208 RID: 8712 RVA: 0x00075DAC File Offset: 0x00073FAC
		private _ISignature Signature
		{
			get
			{
				_ISignature isignature = this._Scope.MethodSignature as _ISignature;
				if (isignature == null)
				{
					isignature = (this._Scope.LocalSignature as _ISignature);
				}
				return isignature;
			}
		}

		// Token: 0x17000675 RID: 1653
		// (get) Token: 0x06002209 RID: 8713 RVA: 0x00075DE0 File Offset: 0x00073FE0
		private bool AllowTranslation
		{
			get
			{
				return this.\u0001.Peek();
			}
		}

		// Token: 0x0600220A RID: 8714 RVA: 0x00075DF0 File Offset: 0x00073FF0
		private _IVariable \u0001(ICompiledType \u0002)
		{
			global::\u001A.\u0007.\u0003 u = new global::\u001A.\u0007.\u0003();
			foreach (global::\u001A.\u0007.\u0001 u2 in this.\u0001)
			{
				if (!u2.\u0001 && this.\u0001(\u0002, u2))
				{
					u2.\u0001 = true;
					u2.\u0001 = this.\u0001;
					return u2.\u0001;
				}
			}
			u.\u0001 = "__VECTMP" + this.\u0001.Count.ToString();
			global::\u0019.\u0001.\u0001(this.\u0001, this.Signature, null, u.\u0001, \u0002);
			Locator.\u0001(this.\u0001, this.\u0001, null, this.Signature, null);
			_IVariable ivariable = this.Signature.AllVariables.First(new Func<_IVariable, bool>(u.\u0001));
			this.\u0001.Add(new global::\u001A.\u0007.\u0001(ivariable, this.\u0001));
			return ivariable;
		}

		// Token: 0x0600220B RID: 8715 RVA: 0x00075F04 File Offset: 0x00074104
		private bool \u0001(ICompiledType \u0002, global::\u001A.\u0007.\u0001 \u0003)
		{
			bool result;
			if (\u0003.\u0001.CompiledType.IsEqual(\u0002))
			{
				result = true;
			}
			else
			{
				ICompiledType3 compiledType = \u0003.\u0001.CompiledType as ICompiledType3;
				result = (compiledType != null && compiledType.IsEqual(\u0002, this._Scope));
			}
			return result;
		}

		// Token: 0x0600220C RID: 8716 RVA: 0x00075F54 File Offset: 0x00074154
		private void \u0001()
		{
			this.\u0001++;
		}

		// Token: 0x0600220D RID: 8717 RVA: 0x00075F64 File Offset: 0x00074164
		private void \u0002()
		{
			for (int i = 0; i < this.\u0001.Count; i++)
			{
				if (this.\u0001[i].\u0001 == this.\u0001)
				{
					this.\u0001[i].\u0001 = false;
				}
			}
			this.\u0001--;
		}

		// Token: 0x0600220E RID: 8718 RVA: 0x00075FC0 File Offset: 0x000741C0
		private void \u0001(_IStatement \u0002)
		{
			this.\u0001.Push(true);
			this.\u0001();
			global::\u001A.\u0007.\u0002 u = new global::\u001A.\u0007.\u0002();
			u.\u0001 = \u0002.GetFlag(StatementFlag.GenerateBP);
			if (\u0002._Position != null)
			{
				u.\u0001 = global::\u0019.\u0003.\u0001(\u0002._Position.EditorPosition, \u0002._Position.PositionOffset);
			}
			this.\u0001.Push(u);
			\u0002.SetFlag(StatementFlag.GenerateBP, false);
		}

		// Token: 0x0600220F RID: 8719 RVA: 0x00076034 File Offset: 0x00074234
		private _IStatement \u0001(_IStatement \u0002)
		{
			while (\u0002 is _ISequenceStatement)
			{
				\u0002 = ((_ISequenceStatement)\u0002)._StatementList[0];
			}
			return \u0002;
		}

		// Token: 0x06002210 RID: 8720 RVA: 0x00076054 File Offset: 0x00074254
		private void \u0002(_IStatement \u0002)
		{
			this.\u0002();
			this.\u0001.Pop();
			this.\u0001.\u0003(\u0002);
			this.\u0001.\u0003();
			global::\u001A.\u0007.\u0002 u = this.\u0001.Pop();
			_IStatement istatement = this.\u0001(\u0002);
			istatement.SetFlag(StatementFlag.GenerateBP, u.\u0001);
			istatement._Position = u.\u0001;
		}

		// Token: 0x06002211 RID: 8721 RVA: 0x000760B8 File Offset: 0x000742B8
		private _IExpression \u0001(_IExpression \u0002, out _IVariable \u0003)
		{
			\u0003 = this.\u0001(\u0002._CompiledType.DeRefType);
			return this.\u0001(\u0002, \u0003);
		}

		// Token: 0x06002212 RID: 8722 RVA: 0x000760D8 File Offset: 0x000742D8
		private _IExpression \u0001(_IExpression \u0002)
		{
			if (\u0002.DataLocation(this._Scope) != null || \u0002.IsLiteral)
			{
				return \u0002;
			}
			_IVariable ivariable;
			return this.\u0001(\u0002, out ivariable);
		}

		// Token: 0x06002213 RID: 8723 RVA: 0x00076108 File Offset: 0x00074308
		private _IExpression \u0001(_IExpression \u0002, _IVariable \u0003)
		{
			_IVariableExpression ivariableExpression = global::\u0019.\u0003.\u0001(\u0003, this.Signature);
			_IAssignmentExpression u = global::\u0019.\u0003.\u0001(ivariableExpression, (_IExpression)\u0002.Duplicate());
			global::\u0002.\u0006.\u0001(u, this._Scope, this.\u0001, this.\u0001);
			this.\u0001.Add(global::\u0019.\u0003.\u0001(u));
			return (_IExpression)ivariableExpression.Duplicate();
		}

		// Token: 0x06002214 RID: 8724 RVA: 0x00076168 File Offset: 0x00074368
		private _ISequenceStatement \u0001(_IStatement \u0002)
		{
			_ISequenceStatement isequenceStatement = global::\u0019.\u0003.\u0001();
			isequenceStatement.Add(\u0002);
			return isequenceStatement;
		}

		// Token: 0x06002215 RID: 8725 RVA: 0x00076178 File Offset: 0x00074378
		private _IStatement \u0001(IEnumerable<_IStatement> \u0002)
		{
			if (\u0002.Count<_IStatement>() == 1)
			{
				return \u0002.First<_IStatement>();
			}
			return global::\u0019.\u0003.\u0001(\u0002);
		}

		// Token: 0x06002216 RID: 8726 RVA: 0x00076190 File Offset: 0x00074390
		private void \u0003(_IStatement \u0002)
		{
			this.\u0001.Add(\u0002);
		}

		// Token: 0x06002217 RID: 8727 RVA: 0x000761A0 File Offset: 0x000743A0
		private void \u0001(IEnumerable<_IStatement> \u0002)
		{
			this.\u0001.AddRange(\u0002);
		}

		// Token: 0x06002218 RID: 8728 RVA: 0x000761B0 File Offset: 0x000743B0
		private List<_IStatement> \u0001(Action \u0002)
		{
			List<_IStatement> u = this.\u0001;
			this.\u0001 = new List<_IStatement>();
			\u0002();
			List<_IStatement> u2 = this.\u0001;
			this.\u0001 = u;
			return u2;
		}

		// Token: 0x06002219 RID: 8729 RVA: 0x000761E4 File Offset: 0x000743E4
		public override void visit(_ICompiledPOU cpou)
		{
			if (!cpou.GetFlagInternal(InternalCompiledPOUFlags.ContainsVectorOperation))
			{
				return;
			}
			this.\u0001.\u0001();
			this.\u0001 = cpou;
			this.\u0001.Clear();
			(cpou.ParseTree as _IExprement).Accept(this.\u0001);
		}

		// Token: 0x0600221A RID: 8730 RVA: 0x00076234 File Offset: 0x00074434
		public override void visit(_IExpressionStatement expstat)
		{
			global::\u001A.\u0007.\u0004 u = new global::\u001A.\u0007.\u0004();
			u.\u0001 = expstat;
			u.\u0001 = this;
			if (!this.\u0001.\u0001(u.\u0001))
			{
				return;
			}
			this.\u0001(u.\u0001);
			List<_IStatement> list = this.\u0001(new Action(u.\u0001));
			list.Add(u.\u0001);
			this.\u0002(this.\u0001(list));
		}

		// Token: 0x0600221B RID: 8731 RVA: 0x000762A4 File Offset: 0x000744A4
		public override void visit(_IIfStatement ifst)
		{
			global::\u001A.\u0007.\u0005 u = new global::\u001A.\u0007.\u0005();
			u.\u0001 = ifst;
			u.\u0001 = this;
			if (!this.\u0001.\u0001(u.\u0001))
			{
				return;
			}
			this.\u0001(u.\u0001);
			List<_IStatement> list = this.\u0001(new Action(u.\u0001));
			List<_IStatement> list2 = this.\u0001(new Action(u.\u0002));
			list2.Add(u.\u0001._IfThen);
			u.\u0001._IfThen = this.\u0001(list2);
			List<_IStatement> list3;
			if (u.\u0001._ElseIf != null)
			{
				list3 = this.\u0001(new Action(u.\u0003));
				list3.Add(u.\u0001._IfElse);
			}
			else
			{
				list3 = new List<_IStatement>();
			}
			List<_IStatement> u2 = list3;
			u.\u0001._IfElse = this.\u0001(u2);
			list.Add(u.\u0001);
			this.\u0002(this.\u0001(list));
		}

		// Token: 0x0600221C RID: 8732 RVA: 0x00076398 File Offset: 0x00074598
		public override void visit(_IWhileStatement whilst)
		{
			global::\u001A.\u0007.\u0006 u = new global::\u001A.\u0007.\u0006();
			u.\u0001 = whilst;
			u.\u0001 = this;
			if (!this.\u0001.\u0001(u.\u0001._Condition))
			{
				return;
			}
			this.\u0001(u.\u0001);
			List<_IStatement> list = this.\u0001(new Action(u.\u0001));
			List<_IStatement> list2 = this.\u0001(new Action(u.\u0002));
			list2.Add(u.\u0001._Controlled);
			foreach (_IStatement istatement in list)
			{
				list2.Add((_IStatement)istatement.Duplicate());
			}
			u.\u0001._Controlled = this.\u0001(list2);
			list.Add(u.\u0001);
			this.\u0002(this.\u0001(list));
		}

		// Token: 0x0600221D RID: 8733 RVA: 0x00076490 File Offset: 0x00074690
		public override void visit(_IRepeatStatement repeat)
		{
			global::\u001A.\u0007.\u0007 u = new global::\u001A.\u0007.\u0007();
			u.\u0001 = repeat;
			u.\u0001 = this;
			if (!this.\u0001.\u0001(u.\u0001._Condition))
			{
				return;
			}
			this.\u0001(u.\u0001);
			List<_IStatement> collection = this.\u0001(new Action(u.\u0001));
			List<_IStatement> list = this.\u0001(new Action(u.\u0002));
			list.Add(u.\u0001._Controlled);
			list.AddRange(collection);
			u.\u0001._Controlled = this.\u0001(list);
			this.\u0002(u.\u0001);
		}

		// Token: 0x0600221E RID: 8734 RVA: 0x00076534 File Offset: 0x00074734
		public override void visit(_IForStatement forloop)
		{
			global::\u001A.\u0007.\u0008 u = new global::\u001A.\u0007.\u0008();
			u.\u0001 = forloop;
			u.\u0001 = this;
			if (!this.\u0001.\u0001(u.\u0001._Condition) && !this.\u0001.\u0001(u.\u0001._Counter) && !this.\u0001.\u0001(u.\u0001._CounterStart))
			{
				return;
			}
			this.\u0001(u.\u0001);
			List<_IStatement> list = this.\u0001(new Action(u.\u0001));
			list.Add(global::\u0019.\u0003.\u0001(u.\u0001._CounterStart));
			List<_IStatement> list2 = this.\u0001(new Action(u.\u0002));
			list.AddRange(list2);
			List<_IStatement> list3 = this.\u0001(new Action(u.\u0003));
			list3.Add(u.\u0001._Controlled);
			List<_IStatement> collection = this.\u0001(new Action(u.\u0004));
			list3.AddRange(collection);
			list3.Add(global::\u0019.\u0003.\u0001(u.\u0001._Counter));
			foreach (_IStatement istatement in list2)
			{
				list3.Add((_IStatement)istatement.Duplicate());
			}
			_IWhileStatement item = global::\u0019.\u0003.\u0001(u.\u0001._Condition, this.\u0001(list3));
			list.Add(item);
			this.\u0002(this.\u0001(list));
		}

		// Token: 0x0600221F RID: 8735 RVA: 0x000766C0 File Offset: 0x000748C0
		public override void visit(_ICallExpression call)
		{
			if (!this.AllowTranslation)
			{
				return;
			}
			call.Accept(this.\u0001);
			this.\u0001.\u0003();
			this.\u0001(call);
		}

		// Token: 0x06002220 RID: 8736 RVA: 0x000766EC File Offset: 0x000748EC
		public override void visit(_IOperatorExpression op)
		{
			if (!this.AllowTranslation)
			{
				return;
			}
			Operator code = op.Code;
			if (code <= Operator.Sel)
			{
				if (code == Operator.Mux)
				{
					this.\u0003(op);
					return;
				}
				if (code == Operator.Sel)
				{
					this.\u0004(op);
					return;
				}
			}
			else
			{
				if (code == Operator.And_Then)
				{
					this.\u0002(op);
					return;
				}
				if (code == Operator.Or_Else)
				{
					this.\u0001(op);
					return;
				}
			}
			op.Accept(this.\u0001);
			this.\u0001.\u0003();
			this.\u0001(op);
		}

		// Token: 0x06002221 RID: 8737 RVA: 0x0007676C File Offset: 0x0007496C
		private void \u0001(_IOperatorExpression \u0002)
		{
			global::\u001A.\u0007.\u000E u000E = new global::\u001A.\u0007.\u000E();
			u000E.\u0001 = \u0002;
			u000E.\u0001 = this;
			u000E.\u0001 = this.\u0001(u000E.\u0001._CompiledType);
			_IExpression iexpression = this.\u0002(global::\u0019.\u0003.\u0001(u000E.\u0001, this.Signature));
			_IExpression u = this.\u0002(global::\u0019.\u0003.\u0001(true));
			_IExpressionStatement u2 = global::\u0019.\u0003.\u0001((_IExpression)iexpression.Duplicate(), u);
			List<_IStatement> u3 = this.\u0001(new Action(u000E.\u0001));
			List<_IStatement> u4 = this.\u0001(new Action(u000E.\u0002));
			_IIfStatement u5 = global::\u0019.\u0003.\u0001(u000E.\u0001[0], this.\u0001(u2), global::\u0019.\u0003.\u0001(u4));
			this.\u0001(u3);
			this.\u0003(u5);
			this.\u0001.\u0001(iexpression);
		}

		// Token: 0x06002222 RID: 8738 RVA: 0x00076840 File Offset: 0x00074A40
		private void \u0002(_IOperatorExpression \u0002)
		{
			global::\u001A.\u0007.\u000F u000F = new global::\u001A.\u0007.\u000F();
			u000F.\u0001 = \u0002;
			u000F.\u0001 = this;
			u000F.\u0001 = this.\u0001(u000F.\u0001._CompiledType);
			_IExpression iexpression = this.\u0002(global::\u0019.\u0003.\u0001(u000F.\u0001, this.Signature));
			_IExpression u = this.\u0002(global::\u0019.\u0003.\u0001(false));
			_IExpressionStatement u2 = global::\u0019.\u0003.\u0001((_IExpression)iexpression.Duplicate(), u);
			List<_IStatement> u3 = this.\u0001(new Action(u000F.\u0001));
			List<_IStatement> u4 = this.\u0001(new Action(u000F.\u0002));
			_IIfStatement u5 = global::\u0019.\u0003.\u0001(u000F.\u0001[0], global::\u0019.\u0003.\u0001(u4), this.\u0001(u2));
			this.\u0001(u3);
			this.\u0003(u5);
			this.\u0001.\u0001(iexpression);
		}

		// Token: 0x06002223 RID: 8739 RVA: 0x00076914 File Offset: 0x00074B14
		private void \u0003(_IOperatorExpression \u0002)
		{
			global::\u001A.\u0007.\u0010 u = new global::\u001A.\u0007.\u0010();
			u.\u0001 = this;
			u.\u0001 = \u0002;
			u.\u0001[0] = this.\u0001.\u0001(u.\u0001[0]);
			u.\u0001 = this.\u0001(u.\u0001._CompiledType);
			TypeClass @class = u.\u0001[0].Type.DeRefType.Class;
			List<ICase> list = new List<ICase>();
			u.\u0001 = 1;
			while (u.\u0001 < u.\u0001._OperandsList.Count)
			{
				Action u2;
				if ((u2 = u.\u0001) == null)
				{
					u2 = (u.\u0001 = new Action(u.\u0001));
				}
				List<_IStatement> u3 = this.\u0001(u2);
				_ICaseLabelStatement u4 = global::\u0019.\u0003.\u0001(this.\u0002(global::\u0019.\u0003.\u0001((long)(u.\u0001 - 1), @class)));
				list.Add(global::\u0019.\u0003.\u0001(u4, global::\u0019.\u0003.\u0001(u3)));
				int u5 = u.\u0001 + 1;
				u.\u0001 = u5;
			}
			_ICaseStatement u6 = global::\u0019.\u0003.\u0001(u.\u0001[0], list, global::\u0019.\u0003.\u0001());
			this.\u0003(u6);
			_IExpression u7 = this.\u0002(global::\u0019.\u0003.\u0001(u.\u0001, this.Signature));
			this.\u0001.\u0001(u7);
			this.\u0001.\u0003();
		}

		// Token: 0x06002224 RID: 8740 RVA: 0x00076A70 File Offset: 0x00074C70
		private void \u0004(_IOperatorExpression \u0002)
		{
			global::\u001A.\u0007.\u0011 u = new global::\u001A.\u0007.\u0011();
			u.\u0001 = this;
			u.\u0001 = \u0002;
			u.\u0001[0] = this.\u0001.\u0001(u.\u0001[0]);
			u.\u0001 = this.\u0001(u.\u0001._CompiledType);
			List<_IStatement> u2 = this.\u0001(new Action(u.\u0001));
			List<_IStatement> u3 = this.\u0001(new Action(u.\u0002));
			_IIfStatement u4 = global::\u0019.\u0003.\u0001(u.\u0001[0], global::\u0019.\u0003.\u0001(u2), global::\u0019.\u0003.\u0001(u3));
			this.\u0003(u4);
			_IExpression u5 = this.\u0002(global::\u0019.\u0003.\u0001(u.\u0001, this.Signature));
			this.\u0001.\u0001(u5);
			this.\u0001.\u0003();
		}

		// Token: 0x06002225 RID: 8741 RVA: 0x00076B48 File Offset: 0x00074D48
		private _IExpression \u0002(_IExpression \u0002)
		{
			global::\u0002.\u0006.\u0001(\u0002, this._Scope, this.\u0001, this.\u0001);
			return \u0002;
		}

		// Token: 0x06002226 RID: 8742 RVA: 0x00076B64 File Offset: 0x00074D64
		private bool \u0001(_IExpression \u0002)
		{
			_IOperatorExpression ioperatorExpression = \u0002 as _IOperatorExpression;
			if (ioperatorExpression == null)
			{
				return false;
			}
			switch (ioperatorExpression.Code)
			{
			case Operator.__vcAdd:
			case Operator.__vcSub:
			case Operator.__vcMul:
			case Operator.__vcDiv:
			case Operator.__vcMin:
			case Operator.__vcMax:
				this.\u0006(ioperatorExpression);
				break;
			case Operator.__vcDot:
				this.\u0005(ioperatorExpression);
				break;
			case Operator.__vcSqrt:
				this.\u0007(ioperatorExpression);
				break;
			case Operator.__vcSetReal:
			case Operator.__vcSetLReal:
				this.\u0008(ioperatorExpression);
				break;
			case Operator.__vcLoadReal:
			case Operator.__vcLoadLReal:
				this.\u000E(ioperatorExpression);
				break;
			case Operator.__vcStore:
				this.\u000F(ioperatorExpression);
				break;
			default:
				return false;
			}
			return true;
		}

		// Token: 0x06002227 RID: 8743 RVA: 0x00076C00 File Offset: 0x00074E00
		private void \u0001(_IExpression \u0002)
		{
			if (this.\u0001(\u0002))
			{
				return;
			}
			if (this.\u0001.\u0001(\u0002, this._Scope))
			{
				_IExpression u = this.\u0001(\u0002);
				this.\u0001.\u0001(u);
			}
		}

		// Token: 0x06002228 RID: 8744 RVA: 0x00076C40 File Offset: 0x00074E40
		private void \u0005(_IOperatorExpression \u0002)
		{
			bool flag;
			int num = (\u0002._OperandsList[0]._CompiledType as _IVectorType).DimensionInt(this._Scope, out flag);
			_IExpression iexpression = this.\u0001(\u0002._OperandsList[0]);
			_IExpression iexpression2 = this.\u0001(\u0002._OperandsList[1]);
			_IExpression iexpression3 = null;
			for (int i = 0; i < num; i++)
			{
				_IExpression u = global::\u0019.\u0003.\u0001((IExpression)iexpression.Duplicate(), global::\u0019.\u0003.\u0001((long)i));
				_IExpression u2 = global::\u0019.\u0003.\u0001((IExpression)iexpression2.Duplicate(), global::\u0019.\u0003.\u0001((long)i));
				_IOperatorExpression ioperatorExpression = global::\u0019.\u0003.\u0001(Operator.Times, u, u2);
				if (iexpression3 == null)
				{
					iexpression3 = ioperatorExpression;
				}
				else
				{
					iexpression3 = global::\u0019.\u0003.\u0001(Operator.Plus, iexpression3, ioperatorExpression);
				}
			}
			global::\u0002.\u0006.\u0001(iexpression3, this._Scope, this.\u0001, this.\u0001);
			this.\u0001.\u0001(iexpression3);
		}

		// Token: 0x06002229 RID: 8745 RVA: 0x00076D30 File Offset: 0x00074F30
		private void \u0006(_IOperatorExpression \u0002)
		{
			_IExpression iexpression = \u0002._OperandsList[0];
			_IExpression iexpression2 = \u0002._OperandsList[1];
			_IVectorType ivectorType = \u0002._CompiledType as _IVectorType;
			bool flag;
			int num = ivectorType.DimensionInt(this._Scope, out flag);
			TypeClass @class = ivectorType.BaseType.Class;
			TypeClass class2 = iexpression.Type.DeRefType.Class;
			TypeClass class3 = iexpression2.Type.DeRefType.Class;
			_IExpression iexpression3 = this.\u0001(iexpression);
			_IExpression iexpression4 = this.\u0001(iexpression2);
			_IVariableExpression ivariableExpression = global::\u0019.\u0003.\u0001(this.\u0001(\u0002._CompiledType), this.Signature);
			global::\u0002.\u0006.\u0001(ivariableExpression, this._Scope, this.\u0001, this.\u0001);
			Operator u = Operator.Plus;
			if (\u0002.Code == Operator.__vcMul)
			{
				u = Operator.Times;
			}
			else if (\u0002.Code == Operator.__vcAdd)
			{
				u = Operator.Plus;
			}
			else if (\u0002.Code == Operator.__vcSub)
			{
				u = Operator.Minus;
			}
			else if (\u0002.Code == Operator.__vcDiv)
			{
				u = Operator.Divide;
			}
			else if (\u0002.Code == Operator.__vcMin)
			{
				u = Operator.Min;
			}
			else if (\u0002.Code == Operator.__vcMax)
			{
				u = Operator.Max;
			}
			for (int i = 0; i < num; i++)
			{
				_IExpression u2;
				if (class2 == TypeClass.__Vector)
				{
					u2 = global::\u0019.\u0003.\u0001((IExpression)iexpression3.Duplicate(), global::\u0019.\u0003.\u0001((long)i));
				}
				else if (class2 == @class)
				{
					u2 = (_IExpression)iexpression3.Duplicate();
				}
				else
				{
					u2 = null;
				}
				_IExpression u3;
				if (class3 == TypeClass.__Vector)
				{
					u3 = global::\u0019.\u0003.\u0001((IExpression)iexpression4.Duplicate(), global::\u0019.\u0003.\u0001((long)i));
				}
				else if (class3 == @class)
				{
					u3 = (_IExpression)iexpression4.Duplicate();
				}
				else
				{
					u3 = null;
				}
				IExpression u4 = global::\u0019.\u0003.\u0001((IExpression)ivariableExpression.Duplicate(), global::\u0019.\u0003.\u0001((long)i));
				_IOperatorExpression u5 = global::\u0019.\u0003.\u0001(u, u2, u3);
				_IAssignmentExpression u6 = global::\u0019.\u0003.\u0001(u4, u5);
				_IExpressionStatement item = global::\u0019.\u0003.\u0001(u6);
				global::\u0002.\u0006.\u0001(u6, this._Scope, this.\u0001, this.\u0001);
				this.\u0001.Add(item);
			}
			this.\u0001.\u0001(ivariableExpression);
		}

		// Token: 0x0600222A RID: 8746 RVA: 0x00076F5C File Offset: 0x0007515C
		private void \u0007(_IOperatorExpression \u0002)
		{
			_IExpression iexpression = \u0002._OperandsList[0];
			_IVectorType ivectorType = \u0002._CompiledType as _IVectorType;
			bool flag;
			int num = ivectorType.DimensionInt(this._Scope, out flag);
			TypeClass @class = ivectorType.BaseType.Class;
			TypeClass class2 = iexpression.Type.DeRefType.Class;
			_IExpression iexpression2 = this.\u0001(iexpression);
			_IVariableExpression ivariableExpression = global::\u0019.\u0003.\u0001(this.\u0001(\u0002._CompiledType), this.Signature);
			global::\u0002.\u0006.\u0001(ivariableExpression, this._Scope, this.\u0001, this.\u0001);
			Operator u = Operator.Sqrt;
			if (\u0002.Code == Operator.__vcSqrt)
			{
				u = Operator.Sqrt;
			}
			for (int i = 0; i < num; i++)
			{
				_IExpression u2;
				if (class2 == TypeClass.__Vector)
				{
					u2 = global::\u0019.\u0003.\u0001((IExpression)iexpression2.Duplicate(), global::\u0019.\u0003.\u0001((long)i));
				}
				else if (class2 == @class)
				{
					u2 = (_IExpression)iexpression2.Duplicate();
				}
				else
				{
					u2 = null;
				}
				IExpression u3 = global::\u0019.\u0003.\u0001((IExpression)ivariableExpression.Duplicate(), global::\u0019.\u0003.\u0001((long)i));
				_IOperatorExpression u4 = global::\u0019.\u0003.\u0001(u, u2);
				_IAssignmentExpression u5 = global::\u0019.\u0003.\u0001(u3, u4);
				_IExpressionStatement item = global::\u0019.\u0003.\u0001(u5);
				global::\u0002.\u0006.\u0001(u5, this._Scope, this.\u0001, this.\u0001);
				this.\u0001.Add(item);
			}
			this.\u0001.\u0001(ivariableExpression);
		}

		// Token: 0x0600222B RID: 8747 RVA: 0x000770B4 File Offset: 0x000752B4
		private void \u0008(_IOperatorExpression \u0002)
		{
			bool flag;
			int num = (\u0002._CompiledType as _IVectorType).DimensionInt(this._Scope, out flag);
			_IVariableExpression ivariableExpression = global::\u0019.\u0003.\u0001(this.\u0001(\u0002._CompiledType), this.Signature);
			global::\u0002.\u0006.\u0001(ivariableExpression, this._Scope, this.\u0001, this.\u0001);
			for (int i = 0; i < num; i++)
			{
				IExpression u = global::\u0019.\u0003.\u0001((IExpression)ivariableExpression.Duplicate(), global::\u0019.\u0003.\u0001((long)i));
				_IExpression u2 = \u0002._OperandsList[i];
				_IAssignmentExpression u3 = global::\u0019.\u0003.\u0001(u, u2);
				global::\u0002.\u0006.\u0001(u3, this._Scope, this.\u0001, this.\u0001);
				_IExpressionStatement u4 = global::\u0019.\u0003.\u0001(u3);
				this.\u0003(u4);
			}
			this.\u0001.\u0001(ivariableExpression);
		}

		// Token: 0x0600222C RID: 8748 RVA: 0x00077178 File Offset: 0x00075378
		private void \u000E(_IOperatorExpression \u0002)
		{
			_IVectorType ivectorType = \u0002._CompiledType as _IVectorType;
			bool flag;
			int num = ivectorType.DimensionInt(this._Scope, out flag);
			_IVariableExpression ivariableExpression = global::\u0019.\u0003.\u0001(this.\u0001(\u0002._CompiledType), this.Signature);
			global::\u0002.\u0006.\u0001(ivariableExpression, this._Scope, this.\u0001, this.\u0001);
			_IVariableExpression ivariableExpression2 = global::\u0019.\u0003.\u0001(this.\u0001(global::\u0019.\u0003.\u0001(ivectorType._Base)), this.Signature);
			_IAssignmentExpression u = global::\u0019.\u0003.\u0001((IExpression)ivariableExpression2.Duplicate(), (IExpression)\u0002[1].Duplicate());
			global::\u0002.\u0006.\u0001(u, this._Scope, this.\u0001, this.\u0001);
			this.\u0003(global::\u0019.\u0003.\u0001(u));
			for (int i = 0; i < num; i++)
			{
				IExpression u2 = global::\u0019.\u0003.\u0001((IExpression)ivariableExpression.Duplicate(), global::\u0019.\u0003.\u0001((long)i));
				_IIndexAccessExpression u3 = global::\u0019.\u0003.\u0001((IExpression)ivariableExpression2.Duplicate(), global::\u0019.\u0003.\u0001((long)i));
				_IAssignmentExpression u4 = global::\u0019.\u0003.\u0001(u2, u3);
				global::\u0002.\u0006.\u0001(u4, this._Scope, this.\u0001, this.\u0001);
				_IExpressionStatement u5 = global::\u0019.\u0003.\u0001(u4);
				this.\u0003(u5);
			}
			this.\u0001.\u0001(ivariableExpression);
		}

		// Token: 0x0600222D RID: 8749 RVA: 0x000772B8 File Offset: 0x000754B8
		private void \u000F(_IOperatorExpression \u0002)
		{
			_IVectorType ivectorType = \u0002._CompiledType as _IVectorType;
			bool flag;
			int num = ivectorType.DimensionInt(this._Scope, out flag);
			_IVariableExpression ivariableExpression = global::\u0019.\u0003.\u0001(this.\u0001(global::\u0019.\u0003.\u0001(ivectorType._Base)), this.Signature);
			_IAssignmentExpression u = global::\u0019.\u0003.\u0001((IExpression)ivariableExpression.Duplicate(), (IExpression)\u0002[0].Duplicate());
			global::\u0002.\u0006.\u0001(u, this._Scope, this.\u0001, this.\u0001);
			this.\u0003(global::\u0019.\u0003.\u0001(u));
			_IExpression iexpression = this.\u0001(\u0002[1]);
			for (int i = 0; i < num; i++)
			{
				_IIndexAccessExpression u2 = global::\u0019.\u0003.\u0001((IExpression)iexpression.Duplicate(), global::\u0019.\u0003.\u0001((long)i));
				_IAssignmentExpression u3 = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001((IExpression)ivariableExpression.Duplicate(), global::\u0019.\u0003.\u0001((long)i)), u2);
				global::\u0002.\u0006.\u0001(u3, this._Scope, this.\u0001, this.\u0001);
				_IExpressionStatement u4 = global::\u0019.\u0003.\u0001(u3);
				this.\u0003(u4);
			}
			this.\u0001.\u0001(iexpression);
		}

		// Token: 0x0600222E RID: 8750 RVA: 0x000773D4 File Offset: 0x000755D4
		public static _IExprement \u0001(_IType \u0002, IScope5 \u0003, _IExpression \u0004, _ICompileContext \u0005, ISignature \u0006, out bool \u0007)
		{
			\u0007 = false;
			List<IStatement> list = new List<IStatement>();
			_IVectorType ivectorType = \u0002 as _IVectorType;
			bool flag;
			int num = ivectorType.DimensionInt(\u0003, out flag);
			for (int i = 0; i < num; i++)
			{
				_IExpressionStatement item = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001((_IExpression)\u0004.Duplicate(), global::\u0019.\u0003.\u0001((long)i)), global::\u0019.\u0003.\u0001(0.0, ivectorType.BaseType.Class));
				list.Add(item);
			}
			return global::\u0019.\u0003.\u0001(list);
		}

		// Token: 0x040005ED RID: 1517
		private const string \u0001 = "__VECTMP";

		// Token: 0x040005EE RID: 1518
		private readonly _IDataManager \u0001;

		// Token: 0x040005EF RID: 1519
		private readonly _ICompileContext \u0001;

		// Token: 0x040005F0 RID: 1520
		private _ICompiledPOU \u0001;

		// Token: 0x040005F1 RID: 1521
		private readonly global::\u0015.\u0005 \u0001;

		// Token: 0x040005F2 RID: 1522
		private readonly \u007F.\u000F \u0001 = new \u007F.\u000F();

		// Token: 0x040005F3 RID: 1523
		private readonly global::\u000F.\u0010 \u0001 = new global::\u000F.\u0010();

		// Token: 0x040005F4 RID: 1524
		private readonly List<global::\u001A.\u0007.\u0001> \u0001 = new List<global::\u001A.\u0007.\u0001>();

		// Token: 0x040005F5 RID: 1525
		private int \u0001;

		// Token: 0x040005F6 RID: 1526
		private List<_IStatement> \u0001 = new List<_IStatement>();

		// Token: 0x040005F7 RID: 1527
		private readonly Stack<bool> \u0001 = new Stack<bool>();

		// Token: 0x040005F8 RID: 1528
		private readonly Stack<global::\u001A.\u0007.\u0002> \u0001 = new Stack<global::\u001A.\u0007.\u0002>();

		// Token: 0x040005F9 RID: 1529
		private readonly bool \u0001;

		// Token: 0x040005FA RID: 1530
		[CompilerGenerated]
		private IScope5 \u0001;

		// Token: 0x020001FE RID: 510
		private sealed class \u0001
		{
			// Token: 0x0600222F RID: 8751 RVA: 0x00077454 File Offset: 0x00075654
			public \u0001(_IVariable \u001A\u0002, int \u0002\u0005)
			{
				this.\u0001 = \u001A\u0002;
				this.\u0001 = true;
				this.\u0001 = \u0002\u0005;
			}

			// Token: 0x040005FB RID: 1531
			public _IVariable \u0001;

			// Token: 0x040005FC RID: 1532
			public bool \u0001;

			// Token: 0x040005FD RID: 1533
			public int \u0001;
		}

		// Token: 0x020001FF RID: 511
		private sealed class \u0002
		{
			// Token: 0x040005FE RID: 1534
			public bool \u0001;

			// Token: 0x040005FF RID: 1535
			public IMinimalPosition \u0001;
		}

		// Token: 0x02000200 RID: 512
		[CompilerGenerated]
		private sealed class \u0003
		{
			// Token: 0x06002232 RID: 8754 RVA: 0x00077484 File Offset: 0x00075684
			internal bool \u0001(_IVariable \u0002)
			{
				return \u0002.Name.Equals(this.\u0001, StringComparison.InvariantCultureIgnoreCase);
			}

			// Token: 0x04000600 RID: 1536
			public string \u0001;
		}

		// Token: 0x02000201 RID: 513
		[CompilerGenerated]
		private sealed class \u0004
		{
			// Token: 0x06002234 RID: 8756 RVA: 0x000774A0 File Offset: 0x000756A0
			internal void \u0001()
			{
				this.\u0001.Accept(this.\u0001.\u0001);
			}

			// Token: 0x04000601 RID: 1537
			public _IExpressionStatement \u0001;

			// Token: 0x04000602 RID: 1538
			public global::\u001A.\u0007 \u0001;
		}

		// Token: 0x02000202 RID: 514
		[CompilerGenerated]
		private sealed class \u0005
		{
			// Token: 0x06002236 RID: 8758 RVA: 0x000774C0 File Offset: 0x000756C0
			internal void \u0001()
			{
				this.\u0001._Condition.Accept(this.\u0001.\u0001);
			}

			// Token: 0x06002237 RID: 8759 RVA: 0x000774E0 File Offset: 0x000756E0
			internal void \u0002()
			{
				this.\u0001._IfThen.Accept(this.\u0001.\u0001);
			}

			// Token: 0x06002238 RID: 8760 RVA: 0x00077500 File Offset: 0x00075700
			internal void \u0003()
			{
				this.\u0001._IfElse.Accept(this.\u0001.\u0001);
			}

			// Token: 0x04000603 RID: 1539
			public _IIfStatement \u0001;

			// Token: 0x04000604 RID: 1540
			public global::\u001A.\u0007 \u0001;
		}

		// Token: 0x02000203 RID: 515
		[CompilerGenerated]
		private sealed class \u0006
		{
			// Token: 0x0600223A RID: 8762 RVA: 0x00077528 File Offset: 0x00075728
			internal void \u0001()
			{
				this.\u0001._Condition.Accept(this.\u0001.\u0001);
			}

			// Token: 0x0600223B RID: 8763 RVA: 0x00077548 File Offset: 0x00075748
			internal void \u0002()
			{
				this.\u0001._Controlled.Accept(this.\u0001.\u0001);
			}

			// Token: 0x04000605 RID: 1541
			public _IWhileStatement \u0001;

			// Token: 0x04000606 RID: 1542
			public global::\u001A.\u0007 \u0001;
		}

		// Token: 0x02000204 RID: 516
		[CompilerGenerated]
		private sealed class \u0007
		{
			// Token: 0x0600223D RID: 8765 RVA: 0x00077570 File Offset: 0x00075770
			internal void \u0001()
			{
				this.\u0001._Condition.Accept(this.\u0001.\u0001);
			}

			// Token: 0x0600223E RID: 8766 RVA: 0x00077590 File Offset: 0x00075790
			internal void \u0002()
			{
				this.\u0001._Controlled.Accept(this.\u0001.\u0001);
			}

			// Token: 0x04000607 RID: 1543
			public _IRepeatStatement \u0001;

			// Token: 0x04000608 RID: 1544
			public global::\u001A.\u0007 \u0001;
		}

		// Token: 0x02000205 RID: 517
		[CompilerGenerated]
		private sealed class \u0008
		{
			// Token: 0x06002240 RID: 8768 RVA: 0x000775B8 File Offset: 0x000757B8
			internal void \u0001()
			{
				this.\u0001._CounterStart.Accept(this.\u0001.\u0001);
			}

			// Token: 0x06002241 RID: 8769 RVA: 0x000775D8 File Offset: 0x000757D8
			internal void \u0002()
			{
				this.\u0001._Condition.Accept(this.\u0001.\u0001);
			}

			// Token: 0x06002242 RID: 8770 RVA: 0x000775F8 File Offset: 0x000757F8
			internal void \u0003()
			{
				this.\u0001._Controlled.Accept(this.\u0001.\u0001);
			}

			// Token: 0x06002243 RID: 8771 RVA: 0x00077618 File Offset: 0x00075818
			internal void \u0004()
			{
				this.\u0001._Counter.Accept(this.\u0001.\u0001);
			}

			// Token: 0x04000609 RID: 1545
			public _IForStatement \u0001;

			// Token: 0x0400060A RID: 1546
			public global::\u001A.\u0007 \u0001;
		}

		// Token: 0x02000206 RID: 518
		[CompilerGenerated]
		private sealed class \u000E
		{
			// Token: 0x06002245 RID: 8773 RVA: 0x00077640 File Offset: 0x00075840
			internal void \u0001()
			{
				this.\u0001[0] = this.\u0001.\u0001.\u0001(this.\u0001[0]);
			}

			// Token: 0x06002246 RID: 8774 RVA: 0x0007766C File Offset: 0x0007586C
			internal void \u0002()
			{
				this.\u0001[1] = this.\u0001.\u0001.\u0001(this.\u0001[1]);
				this.\u0001.\u0001(this.\u0001[1], this.\u0001);
			}

			// Token: 0x0400060B RID: 1547
			public _IOperatorExpression \u0001;

			// Token: 0x0400060C RID: 1548
			public global::\u001A.\u0007 \u0001;

			// Token: 0x0400060D RID: 1549
			public _IVariable \u0001;
		}

		// Token: 0x02000207 RID: 519
		[CompilerGenerated]
		private sealed class \u000F
		{
			// Token: 0x06002248 RID: 8776 RVA: 0x000776C8 File Offset: 0x000758C8
			internal void \u0001()
			{
				this.\u0001[0] = this.\u0001.\u0001.\u0001(this.\u0001[0]);
			}

			// Token: 0x06002249 RID: 8777 RVA: 0x000776F4 File Offset: 0x000758F4
			internal void \u0002()
			{
				this.\u0001[1] = this.\u0001.\u0001.\u0001(this.\u0001[1]);
				this.\u0001.\u0001(this.\u0001[1], this.\u0001);
			}

			// Token: 0x0400060E RID: 1550
			public _IOperatorExpression \u0001;

			// Token: 0x0400060F RID: 1551
			public global::\u001A.\u0007 \u0001;

			// Token: 0x04000610 RID: 1552
			public _IVariable \u0001;
		}

		// Token: 0x02000208 RID: 520
		[CompilerGenerated]
		private sealed class \u0010
		{
			// Token: 0x0600224B RID: 8779 RVA: 0x00077750 File Offset: 0x00075950
			internal void \u0001()
			{
				this.\u0001.\u0001(this.\u0001.\u0001.\u0001(this.\u0001[this.\u0001]), this.\u0001);
			}

			// Token: 0x04000611 RID: 1553
			public global::\u001A.\u0007 \u0001;

			// Token: 0x04000612 RID: 1554
			public _IOperatorExpression \u0001;

			// Token: 0x04000613 RID: 1555
			public _IVariable \u0001;

			// Token: 0x04000614 RID: 1556
			public int \u0001;

			// Token: 0x04000615 RID: 1557
			public Action \u0001;
		}

		// Token: 0x02000209 RID: 521
		[CompilerGenerated]
		private sealed class \u0011
		{
			// Token: 0x0600224D RID: 8781 RVA: 0x00077790 File Offset: 0x00075990
			internal void \u0001()
			{
				this.\u0001.\u0001(this.\u0001.\u0001.\u0001(this.\u0001[1]), this.\u0001);
			}

			// Token: 0x0600224E RID: 8782 RVA: 0x000777C0 File Offset: 0x000759C0
			internal void \u0002()
			{
				this.\u0001.\u0001(this.\u0001.\u0001.\u0001(this.\u0001[2]), this.\u0001);
			}

			// Token: 0x04000616 RID: 1558
			public global::\u001A.\u0007 \u0001;

			// Token: 0x04000617 RID: 1559
			public _IOperatorExpression \u0001;

			// Token: 0x04000618 RID: 1560
			public _IVariable \u0001;
		}
	}
}
