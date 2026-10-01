using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using \u000E;
using \u000F;
using \u0015;
using \u0019;
using \u001D;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0013
{
	// Token: 0x020001A6 RID: 422
	internal sealed class \u0005 : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor, IOperatorExpressionVisitor6, IOperatorExpressionVisitor5, IOperatorExpressionVisitor4, IOperatorExpressionVisitor3, IOperatorExpressionVisitor2, IOperatorExpressionVisitor
	{
		// Token: 0x170005E1 RID: 1505
		// (get) Token: 0x06001DED RID: 7661 RVA: 0x00061128 File Offset: 0x0005F328
		// (set) Token: 0x06001DEE RID: 7662 RVA: 0x00061130 File Offset: 0x0005F330
		private global::\u000F.\u0007 Scope { get; set; }

		// Token: 0x170005E2 RID: 1506
		// (get) Token: 0x06001DEF RID: 7663 RVA: 0x0006113C File Offset: 0x0005F33C
		private bool AddImplicitDereferenzations { get; }

		// Token: 0x06001DF0 RID: 7664 RVA: 0x00061144 File Offset: 0x0005F344
		public \u0005(global::\u000F.\u0007 \u009B\u0002, bool \u0096\u0005)
		{
			this.Scope = \u009B\u0002;
			this.AddImplicitDereferenzations = \u0096\u0005;
		}

		// Token: 0x06001DF1 RID: 7665 RVA: 0x0006115C File Offset: 0x0005F35C
		public void \u0001(IStatement \u0002)
		{
			_IStatement istatement = \u0002 as _IStatement;
			try
			{
				istatement.Accept(this);
			}
			catch (Exception ex)
			{
				string text = "Internal error in _IStatement: " + istatement.ToString();
				istatement.AddError(text);
				text = "Exception text: " + ex.ToString();
				istatement.AddMessage(text, istatement._Position, Severity.Error, istatement.PositionLength, MessageId.None);
			}
		}

		// Token: 0x06001DF2 RID: 7666 RVA: 0x000611CC File Offset: 0x0005F3CC
		public void \u0001(_ICompiledPOU \u0002)
		{
		}

		// Token: 0x06001DF3 RID: 7667 RVA: 0x000611D0 File Offset: 0x0005F3D0
		public void \u0001(_ISequenceStatement \u0002)
		{
			IList<_IStatement> statementList = \u0002._StatementList;
			for (int i = 0; i < statementList.Count; i++)
			{
				_IStatement istatement = statementList[i];
				try
				{
					istatement.Accept(this);
				}
				catch (Exception ex)
				{
					string text = "Internal error in _IStatement: " + istatement.ToString();
					istatement.AddError(text);
					text = "Exception text: " + ex.ToString();
					istatement.AddMessage(text, istatement._Position, Severity.Error, istatement.PositionLength, MessageId.None);
				}
			}
		}

		// Token: 0x06001DF4 RID: 7668 RVA: 0x0006125C File Offset: 0x0005F45C
		internal _IExpression \u0001(_IExpression \u0002)
		{
			if (!this.AddImplicitDereferenzations)
			{
				return \u0002;
			}
			global::\u000E.\u000F.\u0001(ref \u0002, true);
			return \u0002;
		}

		// Token: 0x06001DF5 RID: 7669 RVA: 0x00061274 File Offset: 0x0005F474
		public _IExpression \u0001(_IExpression \u0002, ICompiledType \u0003)
		{
			return this.\u0001(\u0002, \u0003, false);
		}

		// Token: 0x06001DF6 RID: 7670 RVA: 0x00061280 File Offset: 0x0005F480
		public _IExpression \u0001(_IExpression \u0002, ICompiledType \u0003, bool \u0004)
		{
			bool flag = false;
			if (!\u0004)
			{
				\u0002 = this.\u0001(\u0002);
			}
			global::\u000E.\u000F.\u0001(this.Scope, \u0002, \u0002.Type, \u0003, this.Scope, this.Scope, ref \u0002, out flag);
			_IConversionExpression iconversionExpression = \u0002 as _IConversionExpression;
			if (iconversionExpression != null && (iconversionExpression.From == TypeClass.Userdef || iconversionExpression.To == TypeClass.Userdef))
			{
				return iconversionExpression._Exp;
			}
			return \u0002;
		}

		// Token: 0x06001DF7 RID: 7671 RVA: 0x000612E8 File Offset: 0x0005F4E8
		public void \u0001(_IWhileStatement \u0002)
		{
			\u0002._Condition.Accept(this);
			\u0002._Condition = this.\u0001(\u0002._Condition, TypeTable.Bool);
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x06001DF8 RID: 7672 RVA: 0x0006131C File Offset: 0x0005F51C
		public void \u0001(_IRepeatStatement \u0002)
		{
			\u0002._Controlled.Accept(this);
			\u0002._Condition.Accept(this);
			\u0002._Condition = this.\u0001(\u0002._Condition, TypeTable.Bool);
		}

		// Token: 0x06001DF9 RID: 7673 RVA: 0x00061350 File Offset: 0x0005F550
		public void \u0001(_IForStatement \u0002)
		{
			if (\u0002._CounterStart != null)
			{
				\u0002._CounterStart.Accept(this);
			}
			if (\u0002._Condition != null)
			{
				\u0002._Condition.Accept(this);
				\u0002._Condition = this.\u0001(\u0002._Condition, TypeTable.Bool);
			}
			if (\u0002._UpperBound != null)
			{
				\u0002._UpperBound.Accept(this);
				\u0002._UpperBound = this.\u0001(\u0002._UpperBound, \u0002._CounterStart.Type);
			}
			if (\u0002._Counter != null)
			{
				\u0002._Counter.Accept(this);
			}
			if (\u0002._By != null)
			{
				\u0002._By.Accept(this);
				\u0002._By = this.\u0001(\u0002._By, \u0002._CounterStart.Type);
			}
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x06001DFA RID: 7674 RVA: 0x00061420 File Offset: 0x0005F620
		public void \u0001(_IExitStatement \u0002)
		{
		}

		// Token: 0x06001DFB RID: 7675 RVA: 0x00061424 File Offset: 0x0005F624
		public void \u0001(_IContinueStatement \u0002)
		{
		}

		// Token: 0x06001DFC RID: 7676 RVA: 0x00061428 File Offset: 0x0005F628
		private _IExpression \u0002(_IExpression \u0002, ICompiledType \u0003)
		{
			\u0002.Accept(this);
			if (\u0002.Type.Class != TypeClass.Reference)
			{
				if (this.Scope.\u0001(\u0002, true) != null)
				{
					return this.\u0001(\u0002, \u0003, true);
				}
				if (this.AddImplicitDereferenzations)
				{
					_IExpression iexpression = (_IExpression)global::\u0019.\u0003.Builder.CreateOperatorExpression(null, Operator.Adr, \u0002);
					iexpression.Type = (ICompiledType)global::\u0019.\u0003.Builder.CreateReferenceType(\u0002.Type);
					return iexpression;
				}
			}
			return this.\u0001(\u0002, \u0003, true);
		}

		// Token: 0x06001DFD RID: 7677 RVA: 0x000614A4 File Offset: 0x0005F6A4
		private _IExpression \u0003(_IExpression \u0002, ICompiledType \u0003)
		{
			\u0002.Accept(this);
			if (this.\u0001(\u0002.Type))
			{
				return this.\u0001(\u0002, \u0003);
			}
			if (this.Scope.\u0001(\u0002, true) != null || \u0002 is IOperatorExpression || \u0002 is IConversionExpression)
			{
				return this.\u0001(\u0002, global::\u0019.\u0003.\u0001(TypeTable.Byte));
			}
			_IExpression iexpression = (_IExpression)global::\u0019.\u0003.Builder.CreateOperatorExpression(null, Operator.Adr, \u0002);
			iexpression.Type = (ICompiledType)global::\u0019.\u0003.Builder.CreateReferenceType(\u0002.Type);
			return iexpression;
		}

		// Token: 0x06001DFE RID: 7678 RVA: 0x00061530 File Offset: 0x0005F730
		private bool \u0001(ICompiledType \u0002)
		{
			IUserdefType userdefType = \u0002 as IUserdefType;
			if (userdefType != null)
			{
				_ISignature isignature = this.Scope.\u0001(userdefType);
				return isignature != null && isignature.POUType == Operator.Interface;
			}
			return false;
		}

		// Token: 0x06001DFF RID: 7679 RVA: 0x00061564 File Offset: 0x0005F764
		public void \u0001(_IAssignmentExpression \u0002)
		{
			\u0002._LValue.Accept(this);
			if (\u0002.KindOf == Operator.RefAssign)
			{
				\u0002._RValue = this.\u0002(\u0002._RValue, \u0002._LValue.Type);
				return;
			}
			if (this.\u0001(\u0002._LValue._CompiledType))
			{
				\u0002._RValue = this.\u0003(\u0002._RValue, \u0002._LValue.Type);
				return;
			}
			\u0002._LValue = this.\u0001(\u0002._LValue);
			\u0002._RValue.Accept(this);
			\u0002._RValue = this.\u0001(\u0002._RValue, \u0002.LValue.Type);
		}

		// Token: 0x06001E00 RID: 7680 RVA: 0x00061614 File Offset: 0x0005F814
		public void \u0001(_IIfStatement \u0002)
		{
			\u0002._Condition.Accept(this);
			\u0002._Condition = this.\u0001(\u0002._Condition, TypeTable.Bool);
			\u0002._IfThen.Accept(this);
			foreach (_IElseIf ielseIf in \u0002._ElseIf)
			{
				ielseIf._Condition.Accept(this);
				ielseIf._Condition = this.\u0001(ielseIf._Condition, TypeTable.Bool);
				ielseIf._Controlled.Accept(this);
			}
			_IStatement ifElse = \u0002._IfElse;
			if (ifElse == null)
			{
				return;
			}
			ifElse.Accept(this);
		}

		// Token: 0x06001E01 RID: 7681 RVA: 0x000616CC File Offset: 0x0005F8CC
		public void \u0001(_IReturnStatement \u0002)
		{
			if (\u0002._Condition != null)
			{
				\u0002._Condition.Accept(this);
				\u0002._Condition = this.\u0001(\u0002._Condition, TypeTable.Bool);
			}
		}

		// Token: 0x06001E02 RID: 7682 RVA: 0x000616FC File Offset: 0x0005F8FC
		public void \u0001(_IJumpStatement \u0002)
		{
			if (\u0002._Condition != null)
			{
				\u0002._Condition.Accept(this);
				\u0002._Condition = this.\u0001(\u0002._Condition, TypeTable.Bool);
			}
		}

		// Token: 0x06001E03 RID: 7683 RVA: 0x0006172C File Offset: 0x0005F92C
		public void \u0001(_ILabelStatement \u0002)
		{
		}

		// Token: 0x06001E04 RID: 7684 RVA: 0x00061730 File Offset: 0x0005F930
		public void \u0001(_ICommentStatement \u0002)
		{
		}

		// Token: 0x06001E05 RID: 7685 RVA: 0x00061734 File Offset: 0x0005F934
		public void \u0001(_IPragmaStatement \u0002)
		{
		}

		// Token: 0x06001E06 RID: 7686 RVA: 0x00061738 File Offset: 0x0005F938
		public void \u0001(_IExpressionStatement \u0002)
		{
			\u0002._Expr.Accept(this);
		}

		// Token: 0x06001E07 RID: 7687 RVA: 0x00061748 File Offset: 0x0005F948
		public void \u0001(_IVariableDeclarationStatement \u0002)
		{
		}

		// Token: 0x06001E08 RID: 7688 RVA: 0x0006174C File Offset: 0x0005F94C
		public void \u0001(_IVariableDeclarationListStatement \u0002)
		{
		}

		// Token: 0x06001E09 RID: 7689 RVA: 0x00061750 File Offset: 0x0005F950
		public void \u0001(_IPOUDeclarationStatement \u0002)
		{
		}

		// Token: 0x06001E0A RID: 7690 RVA: 0x00061754 File Offset: 0x0005F954
		public void \u0001(_ITypeDeclarationStatement \u0002)
		{
		}

		// Token: 0x06001E0B RID: 7691 RVA: 0x00061758 File Offset: 0x0005F958
		public void \u0001(_IEnumDeclarationStatement \u0002)
		{
		}

		// Token: 0x06001E0C RID: 7692 RVA: 0x0006175C File Offset: 0x0005F95C
		public void \u0001(_IEnumDeclarationListStatement \u0002)
		{
		}

		// Token: 0x06001E0D RID: 7693 RVA: 0x00061760 File Offset: 0x0005F960
		public void \u0001(_ICallExpression \u0002)
		{
			if (\u0002._Condition != null)
			{
				\u0002._Condition.Accept(this);
				\u0002._Condition = this.\u0001(\u0002._Condition, TypeTable.Bool);
			}
			\u0002._Callee.Accept(this);
			_IUserdefType iuserdefType = null;
			if (\u0002._Callee.Type != null)
			{
				iuserdefType = (\u0002._Callee.Type.DeRefType as _IUserdefType);
			}
			_ISignature isignature = (iuserdefType != null) ? this.Scope.\u0001(iuserdefType) : null;
			if (isignature == null)
			{
				return;
			}
			global::\u0013.\u0005.\u0001(\u0002, isignature);
			IList<_IExpression> paramExpressions = \u0002.ParamExpressions;
			ICompiledType[] u = global::\u0015.\u0003.\u0001(\u0002, isignature);
			this.\u0001(paramExpressions, u);
			IList<_IExpression> outputExpressions = \u0002.OutputExpressions;
			this.\u0001(\u0002, isignature, outputExpressions);
		}

		// Token: 0x06001E0E RID: 7694 RVA: 0x00061810 File Offset: 0x0005FA10
		private void \u0001(_ICallExpression \u0002, _ISignature \u0003, IList<_IExpression> \u0004)
		{
			ICompiledType[] array = global::\u0015.\u0003.\u0002(\u0002, \u0003);
			for (int i = 0; i < \u0004.Count; i++)
			{
				_IExpression iexpression = \u0004[i];
				if (iexpression != null)
				{
					iexpression.Accept(this);
					\u0004[i] = this.\u0001(\u0004[i], array[i]);
				}
			}
		}

		// Token: 0x06001E0F RID: 7695 RVA: 0x00061860 File Offset: 0x0005FA60
		private void \u0001(IList<_IExpression> \u0002, ICompiledType[] \u0003)
		{
			for (int i = 0; i < \u0002.Count; i++)
			{
				_IExpression iexpression = \u0002[i];
				if (iexpression != null)
				{
					if (\u0003[i].Class == TypeClass.Reference)
					{
						\u0002[i] = this.\u0002(\u0002[i], \u0003[i]);
					}
					else if (this.\u0001(\u0003[i]))
					{
						\u0002[i] = this.\u0003(\u0002[i], \u0003[i]);
					}
					else
					{
						iexpression.Accept(this);
						\u0002[i] = this.\u0001(\u0002[i], \u0003[i]);
					}
				}
			}
		}

		// Token: 0x06001E10 RID: 7696 RVA: 0x000618F4 File Offset: 0x0005FAF4
		private static void \u0001(_ICallExpression \u0002, _ISignature \u0003)
		{
			if (\u0003.POUType == Operator.Function || \u0003.POUType == Operator.Method)
			{
				using (IEnumerator<_IVariable> enumerator = \u0003.AllVariables.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						_IVariable ivariable = enumerator.Current;
						if (string.Equals(ivariable.Name, \u0003.Name, StringComparison.InvariantCultureIgnoreCase))
						{
							\u0002.Type = ivariable._Type;
							break;
						}
					}
					return;
				}
			}
			\u0002.Type = null;
		}

		// Token: 0x06001E11 RID: 7697 RVA: 0x00061978 File Offset: 0x0005FB78
		public void \u0001(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			for (int i = 0; i < operandsList.Count; i++)
			{
				operandsList[i].Accept(this);
			}
			\u0002.AcceptOperatorVisitor(this);
		}

		// Token: 0x06001E12 RID: 7698 RVA: 0x000619B4 File Offset: 0x0005FBB4
		public void \u0001(_ICastExpression \u0002)
		{
		}

		// Token: 0x06001E13 RID: 7699 RVA: 0x000619B8 File Offset: 0x0005FBB8
		public void \u0001(_INewExpression \u0002)
		{
			\u0002._Count.Accept(this);
			\u0002._Count = this.\u0001(\u0002._Count, TypeTable.AnyInt);
			if (\u0002._FBInitParams != null)
			{
				foreach (IAssignmentExpression assignmentExpression in \u0002._FBInitParams)
				{
					((_IAssignmentExpression)assignmentExpression).Accept(this);
				}
			}
		}

		// Token: 0x06001E14 RID: 7700 RVA: 0x00061A34 File Offset: 0x0005FC34
		public void \u0001(_ITypeExpression \u0002)
		{
		}

		// Token: 0x06001E15 RID: 7701 RVA: 0x00061A38 File Offset: 0x0005FC38
		public void \u0001(_IConversionExpression \u0002)
		{
			\u0002._Exp.Accept(this);
		}

		// Token: 0x06001E16 RID: 7702 RVA: 0x00061A48 File Offset: 0x0005FC48
		public void \u0001(_IThisExpression \u0002)
		{
		}

		// Token: 0x06001E17 RID: 7703 RVA: 0x00061A4C File Offset: 0x0005FC4C
		public void \u0001(_IBaseExpression \u0002)
		{
		}

		// Token: 0x06001E18 RID: 7704 RVA: 0x00061A50 File Offset: 0x0005FC50
		public void \u0001(_ILiteralExpression \u0002)
		{
		}

		// Token: 0x06001E19 RID: 7705 RVA: 0x00061A54 File Offset: 0x0005FC54
		public void \u0001(_IAddressExpression \u0002)
		{
		}

		// Token: 0x06001E1A RID: 7706 RVA: 0x00061A58 File Offset: 0x0005FC58
		public void \u0001(_IQualifiedNameExpression \u0002)
		{
		}

		// Token: 0x06001E1B RID: 7707 RVA: 0x00061A5C File Offset: 0x0005FC5C
		public void \u0001(_IVariableExpression \u0002)
		{
		}

		// Token: 0x06001E1C RID: 7708 RVA: 0x00061A60 File Offset: 0x0005FC60
		public void \u0001(_IIndexAccessExpression \u0002)
		{
			\u0002._Var.Accept(this);
			for (int i = 0; i < \u0002.NumAccesses; i++)
			{
				_IExpression access = \u0002.GetAccess(i);
				access.Accept(this);
				\u0002[i] = this.\u0001(access, TypeTable.AnyInt);
			}
		}

		// Token: 0x06001E1D RID: 7709 RVA: 0x00061AAC File Offset: 0x0005FCAC
		public void \u0001(_ICompoAccessExpression \u0002)
		{
		}

		// Token: 0x06001E1E RID: 7710 RVA: 0x00061AB0 File Offset: 0x0005FCB0
		public void \u0001(_IDeRefAccessExpression \u0002)
		{
		}

		// Token: 0x06001E1F RID: 7711 RVA: 0x00061AB4 File Offset: 0x0005FCB4
		public void \u0001(_ICopyScopeExpression \u0002)
		{
		}

		// Token: 0x06001E20 RID: 7712 RVA: 0x00061AB8 File Offset: 0x0005FCB8
		public void \u0001(_IGlobalScopeExpression \u0002)
		{
		}

		// Token: 0x06001E21 RID: 7713 RVA: 0x00061ABC File Offset: 0x0005FCBC
		public void \u0001(_ISystemScopeExpression \u0002)
		{
		}

		// Token: 0x06001E22 RID: 7714 RVA: 0x00061AC0 File Offset: 0x0005FCC0
		public void \u0001(_IPoolScopeExpression \u0002)
		{
		}

		// Token: 0x06001E23 RID: 7715 RVA: 0x00061AC4 File Offset: 0x0005FCC4
		public void \u0001(_INamespaceAccessExpression \u0002)
		{
		}

		// Token: 0x06001E24 RID: 7716 RVA: 0x00061AC8 File Offset: 0x0005FCC8
		public void \u0001(_ICurrentTaskExpression \u0002)
		{
		}

		// Token: 0x06001E25 RID: 7717 RVA: 0x00061ACC File Offset: 0x0005FCCC
		public void \u0001(_IEmptyStatement \u0002)
		{
		}

		// Token: 0x06001E26 RID: 7718 RVA: 0x00061AD0 File Offset: 0x0005FCD0
		public void \u0001(_ICaseRangeExpression \u0002)
		{
			\u0002._Low.Accept(this);
			\u0002._High.Accept(this);
		}

		// Token: 0x06001E27 RID: 7719 RVA: 0x00061AEC File Offset: 0x0005FCEC
		public void \u0001(_ICaseLabelStatement \u0002)
		{
			foreach (_IExpression iexpression in \u0002._cases)
			{
				iexpression.Accept(this);
			}
		}

		// Token: 0x06001E28 RID: 7720 RVA: 0x00061B38 File Offset: 0x0005FD38
		public void \u0001(_ICaseStatement \u0002)
		{
			\u0002._Switch.Accept(this);
			IList<_ICase> cases = \u0002._Cases;
			foreach (_ICase icase in cases)
			{
				icase._Label.Accept(this);
				icase._Controlled.Accept(this);
			}
			_IStatement @else = \u0002._Else;
			if (@else != null)
			{
				@else.Accept(this);
			}
			if (\u0002._Switch.Type == null)
			{
				return;
			}
			\u0002._Switch = this.\u0001(\u0002._Switch, TypeTable.AnyInt);
			foreach (_ICase icase2 in cases)
			{
				IList<_IExpression> cases2 = icase2._Label._cases;
				for (int i = 0; i < cases2.Count; i++)
				{
					if (icase2._Label[i].Type != null)
					{
						if (icase2._Label[i] is _ICaseRangeExpression)
						{
							_ICaseRangeExpression icaseRangeExpression = icase2._Label[i] as _ICaseRangeExpression;
							icaseRangeExpression._Low = this.\u0001(icaseRangeExpression._Low, \u0002._Switch.Type);
							icaseRangeExpression._High = this.\u0001(icaseRangeExpression._High, \u0002._Switch.Type);
						}
						else
						{
							icase2._Label[i] = this.\u0001(icase2._Label[i], \u0002._Switch.Type);
						}
					}
				}
			}
		}

		// Token: 0x06001E29 RID: 7721 RVA: 0x00061CE0 File Offset: 0x0005FEE0
		public void \u0001(_IErrorExpression \u0002)
		{
		}

		// Token: 0x06001E2A RID: 7722 RVA: 0x00061CE4 File Offset: 0x0005FEE4
		public void \u0001(_IErrorStatement \u0002)
		{
		}

		// Token: 0x06001E2B RID: 7723 RVA: 0x00061CE8 File Offset: 0x0005FEE8
		public void \u0001(_INullExpression \u0002)
		{
		}

		// Token: 0x06001E2C RID: 7724 RVA: 0x00061CEC File Offset: 0x0005FEEC
		public void \u0001(_INullStatement \u0002)
		{
		}

		// Token: 0x06001E2D RID: 7725 RVA: 0x00061CF0 File Offset: 0x0005FEF0
		public void \u0001(_IMultipleIndexInitialization \u0002)
		{
			\u0002._Number.Accept(this);
			\u0002._Number = this.\u0001(\u0002._Number, TypeTable.AnyInt);
			\u0002._Value.Accept(this);
		}

		// Token: 0x06001E2E RID: 7726 RVA: 0x00061D24 File Offset: 0x0005FF24
		public void \u0001(_IArrayInitialization \u0002)
		{
			_IArrayType iarrayType = \u0002.Type as _IArrayType;
			for (int i = 0; i < \u0002._InitValues.Count; i++)
			{
				\u0002._InitValues[i].Accept(this);
				\u0002._InitValues[i] = this.\u0001(\u0002._InitValues[i], iarrayType._Base);
			}
		}

		// Token: 0x06001E2F RID: 7727 RVA: 0x00061D8C File Offset: 0x0005FF8C
		public void \u0001(_IStructureInitialization \u0002)
		{
			foreach (_IAssignmentExpression iassignmentExpression in \u0002._CompoInits)
			{
				iassignmentExpression.Accept(this);
			}
		}

		// Token: 0x06001E30 RID: 7728 RVA: 0x00061DD8 File Offset: 0x0005FFD8
		public void \u0001(_IDefineReference \u0002)
		{
		}

		// Token: 0x06001E31 RID: 7729 RVA: 0x00061DDC File Offset: 0x0005FFDC
		public void \u0001(_IVariableReference \u0002)
		{
		}

		// Token: 0x06001E32 RID: 7730 RVA: 0x00061DE0 File Offset: 0x0005FFE0
		public void \u0001(_ITypeReference \u0002)
		{
		}

		// Token: 0x06001E33 RID: 7731 RVA: 0x00061DE4 File Offset: 0x0005FFE4
		public void \u0001(_IPouReference \u0002)
		{
		}

		// Token: 0x06001E34 RID: 7732 RVA: 0x00061DE8 File Offset: 0x0005FFE8
		public void \u0001(_ITaskReference \u0002)
		{
		}

		// Token: 0x06001E35 RID: 7733 RVA: 0x00061DEC File Offset: 0x0005FFEC
		public void \u0001(_IResourceReference \u0002)
		{
		}

		// Token: 0x06001E36 RID: 7734 RVA: 0x00061DF0 File Offset: 0x0005FFF0
		public void \u0001(_IDefinedExpression \u0002)
		{
		}

		// Token: 0x06001E37 RID: 7735 RVA: 0x00061DF4 File Offset: 0x0005FFF4
		public void \u0001(_IPragmaOperatorExpression \u0002)
		{
		}

		// Token: 0x06001E38 RID: 7736 RVA: 0x00061DF8 File Offset: 0x0005FFF8
		public void \u0001(_IPragmaAssertion \u0002)
		{
		}

		// Token: 0x06001E39 RID: 7737 RVA: 0x00061DFC File Offset: 0x0005FFFC
		public void \u0001(_ICompilerVersionExpression \u0002)
		{
		}

		// Token: 0x06001E3A RID: 7738 RVA: 0x00061E00 File Offset: 0x00060000
		public void \u0001(_IRuntimeVersionExpression \u0002)
		{
		}

		// Token: 0x06001E3B RID: 7739 RVA: 0x00061E04 File Offset: 0x00060004
		public void \u0001(_IPragmaIfStatement \u0002)
		{
			\u0002.Condition.Accept(this);
			\u0002.Condition = this.\u0001(\u0002.Condition, TypeTable.Bool);
			\u0002.IfThen.Accept(this);
			IList<_IPragmaElseIf> elseIf = \u0002.ElseIf;
			if (elseIf != null)
			{
				foreach (_IPragmaElseIf ipragmaElseIf in elseIf)
				{
					ipragmaElseIf.Condition.Accept(this);
					ipragmaElseIf.Condition = this.\u0001(ipragmaElseIf.Condition, TypeTable.Bool);
					ipragmaElseIf.Controlled.Accept(this);
				}
			}
			_IStatement ifElse = \u0002.IfElse;
			if (ifElse == null)
			{
				return;
			}
			ifElse.Accept(this);
		}

		// Token: 0x06001E3C RID: 7740 RVA: 0x00061EC0 File Offset: 0x000600C0
		public void \u0001(_IBreakPointStatement \u0002)
		{
		}

		// Token: 0x06001E3D RID: 7741 RVA: 0x00061EC4 File Offset: 0x000600C4
		public void \u0001(_IDefineStatement \u0002)
		{
		}

		// Token: 0x06001E3E RID: 7742 RVA: 0x00061EC8 File Offset: 0x000600C8
		public void \u0001(_IXRefExpression \u0002)
		{
		}

		// Token: 0x06001E3F RID: 7743 RVA: 0x00061ECC File Offset: 0x000600CC
		public void \u0001(_IHasTypeExpression \u0002)
		{
		}

		// Token: 0x06001E40 RID: 7744 RVA: 0x00061ED0 File Offset: 0x000600D0
		public void \u0001(_IIsEnumTypeExpression \u0002)
		{
		}

		// Token: 0x06001E41 RID: 7745 RVA: 0x00061ED4 File Offset: 0x000600D4
		public void \u0001(_IHasAttributeExpression \u0002)
		{
		}

		// Token: 0x06001E42 RID: 7746 RVA: 0x00061ED8 File Offset: 0x000600D8
		public void \u0001(_IHasValueExpression \u0002)
		{
		}

		// Token: 0x06001E43 RID: 7747 RVA: 0x00061EDC File Offset: 0x000600DC
		public void \u0001(_IHasConstantValueExpression \u0002)
		{
		}

		// Token: 0x06001E44 RID: 7748 RVA: 0x00061EE0 File Offset: 0x000600E0
		public void \u0001(_IHasConstantTypeExpression \u0002)
		{
		}

		// Token: 0x06001E45 RID: 7749 RVA: 0x00061EE4 File Offset: 0x000600E4
		public void \u0001(_ITryCatchStatement \u0002)
		{
			\u0002.DefaultTraverse(this);
		}

		// Token: 0x06001E46 RID: 7750 RVA: 0x00061EF0 File Offset: 0x000600F0
		public void \u0001(_IPartialAccessExpression \u0002)
		{
		}

		// Token: 0x06001E47 RID: 7751 RVA: 0x00061EF4 File Offset: 0x000600F4
		public void \u0001(_IProjectDefinedExpression \u0002)
		{
		}

		// Token: 0x06001E48 RID: 7752 RVA: 0x00061EF8 File Offset: 0x000600F8
		public void \u0002(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001E49 RID: 7753 RVA: 0x00061EFC File Offset: 0x000600FC
		public void \u0003(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001E4A RID: 7754 RVA: 0x00061F00 File Offset: 0x00060100
		public void \u0004(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001E4B RID: 7755 RVA: 0x00061F04 File Offset: 0x00060104
		public void \u0005(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			for (int i = 0; i < operandsList.Count; i++)
			{
				\u0002[i] = this.\u0001(\u0002[i], \u0002.Type);
			}
		}

		// Token: 0x06001E4C RID: 7756 RVA: 0x00061F44 File Offset: 0x00060144
		public void \u0006(_IOperatorExpression \u0002)
		{
			this.\u0005(\u0002);
		}

		// Token: 0x06001E4D RID: 7757 RVA: 0x00061F50 File Offset: 0x00060150
		public void \u0007(_IOperatorExpression \u0002)
		{
			if (\u0002._OperandsList.Count != 2)
			{
				return;
			}
			\u0002[1] = this.\u0001(\u0002[1], TypeTable.Bool);
		}

		// Token: 0x06001E4E RID: 7758 RVA: 0x00061F7C File Offset: 0x0006017C
		public void \u0008(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			if (operandsList.Count == 1 && operandsList[0].Type != null)
			{
				\u0002[0] = this.\u0001(\u0002[0], TypeTable.UDInt);
			}
		}

		// Token: 0x06001E4F RID: 7759 RVA: 0x00061FC0 File Offset: 0x000601C0
		public void \u000E(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001E50 RID: 7760 RVA: 0x00061FC4 File Offset: 0x000601C4
		public void \u000F(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001E51 RID: 7761 RVA: 0x00061FC8 File Offset: 0x000601C8
		public void \u0010(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001E52 RID: 7762 RVA: 0x00061FCC File Offset: 0x000601CC
		public void \u0011(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001E53 RID: 7763 RVA: 0x00061FD0 File Offset: 0x000601D0
		public void \u0012(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001E54 RID: 7764 RVA: 0x00061FD4 File Offset: 0x000601D4
		public void \u0013(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001E55 RID: 7765 RVA: 0x00061FD8 File Offset: 0x000601D8
		public void \u0014(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001E56 RID: 7766 RVA: 0x00061FDC File Offset: 0x000601DC
		public void \u0015(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001E57 RID: 7767 RVA: 0x00061FE0 File Offset: 0x000601E0
		public void \u0016(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001E58 RID: 7768 RVA: 0x00061FE4 File Offset: 0x000601E4
		public void \u0017(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001E59 RID: 7769 RVA: 0x00061FE8 File Offset: 0x000601E8
		public void \u0018(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001E5A RID: 7770 RVA: 0x00061FEC File Offset: 0x000601EC
		public void \u0019(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001E5B RID: 7771 RVA: 0x00061FF0 File Offset: 0x000601F0
		public void \u001A(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001E5C RID: 7772 RVA: 0x00061FF4 File Offset: 0x000601F4
		public void \u001B(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			if (operandsList.Count < 2)
			{
				return;
			}
			ISignature signature = this.Scope.\u0001(operandsList[0]._CompiledType as _IUserdefType);
			if (signature == null)
			{
				return;
			}
			IVariable[] allInputs = signature.AllInputs;
			if (allInputs.Length != operandsList.Count - 2)
			{
				return;
			}
			for (int i = 0; i < allInputs.Length; i++)
			{
				if (operandsList[i + 2].Type != null)
				{
					IVariable variable = allInputs[i];
					operandsList[i + 2] = this.\u0001(operandsList[i + 2], variable.CompiledType);
				}
			}
		}

		// Token: 0x06001E5D RID: 7773 RVA: 0x0006208C File Offset: 0x0006028C
		public void \u001C(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001E5E RID: 7774 RVA: 0x00062090 File Offset: 0x00060290
		public void \u001D(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001E5F RID: 7775 RVA: 0x00062094 File Offset: 0x00060294
		public void \u001E(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001E60 RID: 7776 RVA: 0x00062098 File Offset: 0x00060298
		public void \u001F(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001E61 RID: 7777 RVA: 0x0006209C File Offset: 0x0006029C
		public void \u007F(_IOperatorExpression \u0002)
		{
			for (int i = 0; i < \u0002._OperandsList.Count; i++)
			{
				\u0002._OperandsList[i] = this.\u0001(\u0002._OperandsList[i]);
			}
		}

		// Token: 0x06001E62 RID: 7778 RVA: 0x000620E0 File Offset: 0x000602E0
		public void \u0080(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001E63 RID: 7779 RVA: 0x000620E4 File Offset: 0x000602E4
		public void \u0081(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001E64 RID: 7780 RVA: 0x000620E8 File Offset: 0x000602E8
		public void \u0082(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001E65 RID: 7781 RVA: 0x000620EC File Offset: 0x000602EC
		public void \u0083(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001E66 RID: 7782 RVA: 0x000620F0 File Offset: 0x000602F0
		public void \u0084(_IOperatorExpression \u0002)
		{
			if (\u0002._OperandsList.Count < 2 || \u0002[1] == null)
			{
				return;
			}
			\u0002[1] = this.\u0001(\u0002[1], TypeTable.Bool);
		}

		// Token: 0x06001E67 RID: 7783 RVA: 0x00062124 File Offset: 0x00060324
		public void \u0086(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001E68 RID: 7784 RVA: 0x00062128 File Offset: 0x00060328
		public void \u0087(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001E69 RID: 7785 RVA: 0x0006212C File Offset: 0x0006032C
		public void \u0088(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001E6A RID: 7786 RVA: 0x00062130 File Offset: 0x00060330
		public void \u0089(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001E6B RID: 7787 RVA: 0x00062134 File Offset: 0x00060334
		public void \u008A(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001E6C RID: 7788 RVA: 0x00062138 File Offset: 0x00060338
		public void \u008B(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			for (int i = 0; i < operandsList.Count; i++)
			{
				if (i == 0 && \u0002[i] is ILiteralExpression && !TypeTable.IsConcreteType((\u0002[i] as ILiteralExpression).ConstantType))
				{
					\u0002[i] = this.\u0001(\u0002[i], TypeTable.UDInt);
				}
				else
				{
					\u0002[i] = this.\u0001(\u0002[i], TypeTable.AnyInt);
				}
			}
		}

		// Token: 0x06001E6D RID: 7789 RVA: 0x000621BC File Offset: 0x000603BC
		public void \u008C(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			for (int i = 1; i < operandsList.Count; i++)
			{
				\u0002[i] = this.\u0001(\u0002[i], TypeTable.DInt);
			}
		}

		// Token: 0x06001E6E RID: 7790 RVA: 0x000621FC File Offset: 0x000603FC
		public void \u008D(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			_IType u = TypeTable.DWord;
			if (this.Scope.PointerSize > 4)
			{
				u = TypeTable.LWord;
			}
			for (int i = 1; i < operandsList.Count; i++)
			{
				\u0002[i] = this.\u0001(\u0002[i], u);
			}
		}

		// Token: 0x06001E6F RID: 7791 RVA: 0x00062250 File Offset: 0x00060450
		public void \u008E(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001E70 RID: 7792 RVA: 0x00062254 File Offset: 0x00060454
		public void \u008F(_IOperatorExpression \u0002)
		{
			if (\u0002._OperandsList.Count > 1)
			{
				\u0002[0] = this.\u0001(\u0002[0], TypeTable.DWord);
				\u0002[1] = this.\u0001(\u0002[1], TypeTable.Bool);
			}
		}

		// Token: 0x06001E71 RID: 7793 RVA: 0x000622A4 File Offset: 0x000604A4
		public void \u0090(_IOperatorExpression \u0002)
		{
			this.\u008F(\u0002);
			if (\u0002._OperandsList.Count > 1)
			{
				\u0002[2] = this.\u0001(\u0002[2], TypeTable.USInt);
			}
		}

		// Token: 0x06001E72 RID: 7794 RVA: 0x000622D4 File Offset: 0x000604D4
		public void \u0091(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			for (int i = 0; i < operandsList.Count; i++)
			{
				\u0002[i] = this.\u0001(\u0002[i], \u0002.Type);
			}
		}

		// Token: 0x06001E73 RID: 7795 RVA: 0x00062314 File Offset: 0x00060514
		public void \u0092(_IOperatorExpression \u0002)
		{
			if (\u0002._OperandsList.Count != 2)
			{
				return;
			}
			_IExpression[] array = new _IExpression[]
			{
				\u0002[0],
				\u0002[1]
			};
			if (array[0] != null && array[1] != null && array[0].Type != null && array[1].Type != null && array[0].Type.Class == array[1].Type.Class && !TypeTable.IsBlock(array[0].Type.Class))
			{
				return;
			}
			_IType u = global::\u001D.\u0005.\u0001(\u0002.Code, false, 0, array, null, this.Scope, null);
			for (int i = 0; i < \u0002._OperandsList.Count; i++)
			{
				\u0002[i] = this.\u0001(\u0002[i], u);
			}
		}

		// Token: 0x06001E74 RID: 7796 RVA: 0x000623DC File Offset: 0x000605DC
		public void \u0093(_IOperatorExpression \u0002)
		{
			for (int i = 0; i < \u0002._OperandsList.Count; i++)
			{
				\u0002[i] = this.\u0001(\u0002[i], \u0002.Type);
			}
		}

		// Token: 0x06001E75 RID: 7797 RVA: 0x0006241C File Offset: 0x0006061C
		public void \u0094(_IOperatorExpression \u0002)
		{
			for (int i = 0; i < \u0002._OperandsList.Count; i++)
			{
				\u0002[i] = this.\u0001(\u0002[i], \u0002.Type);
			}
		}

		// Token: 0x06001E76 RID: 7798 RVA: 0x0006245C File Offset: 0x0006065C
		public void \u0095(_IOperatorExpression \u0002)
		{
			switch (\u0002.Code)
			{
			case Operator.__vcAdd:
			case Operator.__vcSub:
			case Operator.__vcMul:
			case Operator.__vcDiv:
			case Operator.__vcMin:
			case Operator.__vcMax:
			{
				ICompiledType deRefType = \u0002[0]._CompiledType.DeRefType;
				ICompiledType deRefType2 = \u0002[1]._CompiledType.DeRefType;
				if (\u0002.Code == Operator.__vcMul)
				{
					if (TypeTable.IsNumber(deRefType.Class) && deRefType2.Class == TypeClass.__Vector)
					{
						\u0002[0] = this.\u0001(\u0002[0], deRefType2.BaseType);
						return;
					}
					if (deRefType.Class == TypeClass.__Vector && TypeTable.IsNumber(deRefType2.Class))
					{
						\u0002[1] = this.\u0001(\u0002[1], deRefType.BaseType);
						return;
					}
				}
				if (\u0002.Code == Operator.__vcDiv && deRefType.Class == TypeClass.__Vector && TypeTable.IsNumber(deRefType2.Class))
				{
					\u0002[1] = this.\u0001(\u0002[1], deRefType.BaseType);
					return;
				}
				break;
			}
			case Operator.__vcDot:
			case Operator.__vcSqrt:
				break;
			case Operator.__vcSetReal:
			case Operator.__vcSetLReal:
			{
				_IType itype = TypeTable.Get((\u0002.Code == Operator.__vcSetLReal) ? Operator.LReal : Operator.Real);
				for (int i = 0; i < \u0002._OperandsList.Count; i++)
				{
					\u0002[i] = this.\u0001(\u0002[i], itype);
				}
				return;
			}
			case Operator.__vcLoadReal:
			case Operator.__vcLoadLReal:
			{
				_IType itype = TypeTable.Get((\u0002.Code == Operator.__vcLoadLReal) ? Operator.LReal : Operator.Real);
				\u0002[1] = this.\u0001(\u0002[1], global::\u0019.\u0003.\u0001(itype));
				return;
			}
			case Operator.__vcStore:
			{
				ICompiledType type = \u0002[1].Type;
				if (type == null)
				{
					return;
				}
				\u0002[0] = this.\u0001(\u0002[0], global::\u0019.\u0003.\u0001(TypeTable.Get(type.BaseType.Class)));
				return;
			}
			default:
				return;
			}
		}

		// Token: 0x06001E77 RID: 7799 RVA: 0x00062644 File Offset: 0x00060844
		public void \u0096(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			switch (\u0002.Code)
			{
			case Operator.Limit:
				if (operandsList.Count != 3)
				{
					return;
				}
				if (operandsList[0].Type != null && operandsList[1].Type != null && operandsList[2].Type != null)
				{
					\u0002[0] = this.\u0001(\u0002[0], \u0002.Type);
					\u0002[1] = this.\u0001(\u0002[1], \u0002.Type);
					\u0002[2] = this.\u0001(\u0002[2], \u0002.Type);
					return;
				}
				break;
			case Operator.Min:
			case Operator.Max:
				this.\u0092(\u0002);
				return;
			case Operator.Trunc:
				break;
			case Operator.Mux:
				if (operandsList.Count != 3)
				{
					return;
				}
				\u0002[0] = this.\u0001(\u0002[0], TypeTable.AnyInt);
				for (int i = 1; i < operandsList.Count; i++)
				{
					\u0002[i] = this.\u0001(\u0002[i], \u0002.Type);
				}
				return;
			case Operator.Sel:
				if (operandsList.Count != 3)
				{
					return;
				}
				\u0002[0] = this.\u0001(\u0002[0], TypeTable.Bool);
				for (int j = 1; j < operandsList.Count; j++)
				{
					\u0002[j] = this.\u0001(\u0002[j], \u0002.Type);
				}
				return;
			default:
				return;
			}
		}

		// Token: 0x040004E9 RID: 1257
		[CompilerGenerated]
		private global::\u000F.\u0007 \u0001;

		// Token: 0x040004EA RID: 1258
		[CompilerGenerated]
		private readonly bool \u0001;
	}
}
