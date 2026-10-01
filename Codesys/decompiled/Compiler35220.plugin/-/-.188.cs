using System;
using System.Collections.Generic;
using \u0014;
using \u0019;
using \u001E;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u001F
{
	// Token: 0x02000216 RID: 534
	internal sealed class \u000E : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor, IExprementVisitor2
	{
		// Token: 0x06002327 RID: 8999 RVA: 0x00078FB0 File Offset: 0x000771B0
		public \u000E(\u001E.\u000E \u000E\u0004)
		{
			this.\u0001 = \u000E\u0004;
		}

		// Token: 0x06002328 RID: 9000 RVA: 0x00078FF0 File Offset: 0x000771F0
		public void \u0001()
		{
			\u0019.\u0006 u = new \u0019.\u0006(-1);
			if (this.TopOfStack != null && this.TopOfStack.CurrentExceptionHandlingIndex != -1)
			{
				u.CurrentExceptionHandlingIndex = this.TopOfStack.CurrentExceptionHandlingIndex;
			}
			this.\u0001.Push(u);
		}

		// Token: 0x06002329 RID: 9001 RVA: 0x00079038 File Offset: 0x00077238
		public void \u0001(int \u0002)
		{
			\u0019.\u0006 u = new \u0019.\u0006(\u0002);
			if (this.TopOfStack != null && this.TopOfStack.CurrentExceptionHandlingIndex != -1)
			{
				u.CurrentExceptionHandlingIndex = this.TopOfStack.CurrentExceptionHandlingIndex;
			}
			this.\u0001.Push(u);
		}

		// Token: 0x17000686 RID: 1670
		// (get) Token: 0x0600232A RID: 9002 RVA: 0x00079080 File Offset: 0x00077280
		public \u0019.\u0006 TopOfStack
		{
			get
			{
				if (this.\u0001.Count != 0)
				{
					return this.\u0001.Peek();
				}
				return null;
			}
		}

		// Token: 0x0600232B RID: 9003 RVA: 0x0007909C File Offset: 0x0007729C
		public void \u0002()
		{
			this.\u0001.Pop();
		}

		// Token: 0x17000687 RID: 1671
		// (get) Token: 0x0600232C RID: 9004 RVA: 0x000790AC File Offset: 0x000772AC
		public string ReturnLabel
		{
			get
			{
				if (this.\u0001 == null)
				{
					return "ERROR";
				}
				return "return__label__" + this.\u0001.SignatureId.ToString();
			}
		}

		// Token: 0x0600232D RID: 9005 RVA: 0x000790E4 File Offset: 0x000772E4
		public _IBreakpoint \u0001(_IExprement \u0002)
		{
			_IBreakpoint ibreakpoint = this.\u0001.\u0001(\u0002) as _IBreakpoint;
			if (ibreakpoint == null)
			{
				return null;
			}
			ibreakpoint.ExceptionHandlingSuccessor = this.TopOfStack.CurrentExceptionHandlingIndex;
			return ibreakpoint;
		}

		// Token: 0x0600232E RID: 9006 RVA: 0x0007911C File Offset: 0x0007731C
		public void \u0001(_ICompiledPOU \u0002)
		{
			this.\u0001 = \u0002;
			this.\u0001 = (\u0002.BreakpointList as _IBreakpointList);
			this.\u0001.Clear();
			this.\u0001.Clear();
			this.\u0001();
			\u0002.GetParseTree().Accept(this);
			this.\u0001.FirstIndex = this.TopOfStack.CurrentBPIndex;
			this.\u0002();
			Debug.\u0001(this.\u0001.Count == 0);
			\u0002.SetBreakpointList(this.\u0001);
			this.\u0001 = null;
		}

		// Token: 0x0600232F RID: 9007 RVA: 0x000791AC File Offset: 0x000773AC
		public void \u0001(_ISequenceStatement \u0002)
		{
			int u = this.TopOfStack.CurrentBPIndex;
			IList<_IStatement> statementList = \u0002._StatementList;
			for (int i = statementList.Count - 1; i >= 0; i--)
			{
				_IStatement istatement = statementList[i];
				if (istatement.GetFlag(StatementFlag.GenerateBP) || istatement is ISequenceStatement || this.TopOfStack.DoBP)
				{
					try
					{
						this.\u0001(u);
						istatement.Accept(this);
						if (this.TopOfStack.CurrentBPIndex >= 0)
						{
							u = this.TopOfStack.CurrentBPIndex;
						}
						this.\u0002();
					}
					catch (Exception ex)
					{
						Debug.\u0001(false, ex.ToString());
					}
				}
			}
			this.TopOfStack.CurrentBPIndex = u;
		}

		// Token: 0x06002330 RID: 9008 RVA: 0x00079268 File Offset: 0x00077468
		public void \u0001(_IWhileStatement \u0002)
		{
			_IBreakpoint ibreakpoint = this.\u0001(\u0002);
			if (ibreakpoint == null)
			{
				return;
			}
			int u = this.\u0001;
			this.\u0001 = this.TopOfStack.CurrentBPIndex;
			ibreakpoint.AddSuccessor(this.TopOfStack.CurrentBPIndex);
			this.TopOfStack.CurrentBPIndex = this.\u0001.Add(ref ibreakpoint);
			int u2 = this.\u0002;
			this.\u0002 = this.TopOfStack.CurrentBPIndex;
			this.\u0001(this.TopOfStack.CurrentBPIndex);
			\u0002._Controlled.Accept(this);
			int nSucc = this.TopOfStack.CurrentBPIndex;
			this.\u0002();
			this.\u0001(this.TopOfStack.CurrentBPIndex);
			\u0002._Condition.Accept(this);
			this.\u0002();
			ibreakpoint.AddSuccessor(nSucc);
			this.\u0001 = u;
			this.\u0002 = u2;
		}

		// Token: 0x06002331 RID: 9009 RVA: 0x00079344 File Offset: 0x00077544
		public void \u0001(_IRepeatStatement \u0002)
		{
			_IBreakpoint ibreakpoint = this.\u0001(\u0002._Condition);
			if (ibreakpoint == null)
			{
				return;
			}
			ibreakpoint.AddSuccessor(this.TopOfStack.CurrentBPIndex);
			int u = this.\u0001.Add(ref ibreakpoint);
			this.\u0001(u);
			\u0002._Condition.Accept(this);
			this.\u0002();
			int u2 = this.\u0001;
			this.\u0001 = this.TopOfStack.CurrentBPIndex;
			int u3 = this.\u0002;
			this.\u0002 = u;
			this.\u0001(u);
			\u0002._Controlled.Accept(this);
			int num = this.TopOfStack.CurrentBPIndex;
			this.\u0002();
			ibreakpoint.AddSuccessor(num);
			this.\u0001 = u2;
			this.\u0002 = u3;
			this.TopOfStack.CurrentBPIndex = num;
		}

		// Token: 0x06002332 RID: 9010 RVA: 0x0007940C File Offset: 0x0007760C
		public void \u0001(_IForStatement \u0002)
		{
			_IBreakpoint ibreakpoint = this.\u0001(\u0002);
			_IBreakpoint ibreakpoint2 = this.\u0001(\u0002.UpperBound as _IExpression);
			bool flag = false;
			if (ibreakpoint2 == null)
			{
				ibreakpoint2 = this.\u0001(\u0002.Condition as _IExpression);
				flag = true;
			}
			if (ibreakpoint == null || ibreakpoint2 == null)
			{
				return;
			}
			int num = this.\u0001.Add(ref ibreakpoint2);
			int u = this.\u0001;
			this.\u0001 = this.TopOfStack.CurrentBPIndex;
			int u2 = this.\u0002;
			this.\u0002 = num;
			this.\u0001(num);
			if (flag)
			{
				\u0002._Condition.Accept(this);
			}
			else
			{
				\u0002._UpperBound.Accept(this);
			}
			this.\u0002();
			this.\u0001(num);
			\u0002._Controlled.Accept(this);
			int num2 = this.TopOfStack.CurrentBPIndex;
			this.\u0002();
			ibreakpoint.AddSuccessor(num);
			ibreakpoint2.AddSuccessors(new int[]
			{
				num2,
				this.TopOfStack.CurrentBPIndex
			});
			this.\u0001 = u;
			this.\u0002 = u2;
			int u3 = this.\u0001.Add(ref ibreakpoint);
			this.\u0001(u3);
			\u0002._CounterStart.Accept(this);
			this.\u0002();
			this.TopOfStack.CurrentBPIndex = u3;
		}

		// Token: 0x06002333 RID: 9011 RVA: 0x00079548 File Offset: 0x00077748
		public void \u0001(_IExitStatement \u0002)
		{
			_IBreakpoint ibreakpoint = this.\u0001(\u0002);
			if (ibreakpoint == null)
			{
				return;
			}
			ibreakpoint.AddSuccessor(this.\u0001);
			this.TopOfStack.CurrentBPIndex = this.\u0001.Add(ref ibreakpoint);
		}

		// Token: 0x06002334 RID: 9012 RVA: 0x00079588 File Offset: 0x00077788
		public void \u0001(_IContinueStatement \u0002)
		{
			_IBreakpoint ibreakpoint = this.\u0001(\u0002);
			if (ibreakpoint == null)
			{
				return;
			}
			ibreakpoint.AddSuccessor(this.\u0002);
			this.TopOfStack.CurrentBPIndex = this.\u0001.Add(ref ibreakpoint);
		}

		// Token: 0x06002335 RID: 9013 RVA: 0x000795C8 File Offset: 0x000777C8
		public void \u0001(_IAssignmentExpression \u0002)
		{
			\u0002._LValue.Accept(this);
			\u0002._RValue.Accept(this);
		}

		// Token: 0x06002336 RID: 9014 RVA: 0x000795E4 File Offset: 0x000777E4
		public void \u0001(_IIfStatement \u0002)
		{
			_IBreakpoint ibreakpoint = this.\u0001(\u0002);
			if (ibreakpoint == null)
			{
				return;
			}
			int num = this.TopOfStack.CurrentBPIndex;
			if (\u0002.IfElse != null)
			{
				this.\u0001(this.TopOfStack.CurrentBPIndex);
				\u0002._IfElse.Accept(this);
				num = this.TopOfStack.CurrentBPIndex;
				this.\u0002();
			}
			IList<_IElseIf> elseIf = \u0002._ElseIf;
			if (elseIf != null)
			{
				for (int i = elseIf.Count - 1; i >= 0; i--)
				{
					_IElseIf ielseIf = elseIf[i];
					_IBreakpoint ibreakpoint2 = this.\u0001(ielseIf);
					if (ibreakpoint2 != null)
					{
						this.\u0001(this.TopOfStack.CurrentBPIndex);
						ielseIf._Controlled.Accept(this);
						ibreakpoint2.AddSuccessor(this.TopOfStack.CurrentBPIndex);
						this.\u0002();
						ibreakpoint2.AddSuccessor(num);
						num = this.\u0001.Add(ref ibreakpoint2);
						this.\u0001(num);
						ielseIf._Condition.Accept(this);
						this.\u0002();
					}
				}
			}
			bool flag = false;
			if (\u0002._IfThen is _ISequenceStatement)
			{
				IStatement statement = null;
				if ((\u0002._IfThen as _ISequenceStatement)._StatementList.Count > 0)
				{
					statement = (\u0002._IfThen as _ISequenceStatement)._StatementList[0];
				}
				flag = (statement != null && statement is IPragmaStatement && (statement as IPragmaStatement).Text == "cfc_conditional_statement");
			}
			this.\u0001(this.TopOfStack.CurrentBPIndex);
			if (flag)
			{
				this.TopOfStack.DoBP = true;
			}
			int num2 = this.TopOfStack.CurrentBPIndex;
			\u0002._IfThen.Accept(this);
			bool flag2 = num2 == this.TopOfStack.CurrentBPIndex;
			if (flag && !flag2)
			{
				_IBreakpoint ibreakpoint3 = this.\u0001[this.TopOfStack.CurrentBPIndex] as _IBreakpoint;
				if (ibreakpoint3 != null)
				{
					if (ibreakpoint3.Successors != null)
					{
						ibreakpoint.AddSuccessors(ibreakpoint3.Successors);
					}
					if (ibreakpoint3.StepInSuccessors != null)
					{
						foreach (IStepInPosition sip in ibreakpoint3.StepInSuccessors)
						{
							ibreakpoint.AddStepInSuccessor(sip);
						}
					}
					this.\u0001.Remove(this.TopOfStack.CurrentBPIndex);
				}
			}
			else
			{
				ibreakpoint.AddSuccessor(this.TopOfStack.CurrentBPIndex);
			}
			this.\u0002();
			int u = this.\u0001.Add(ref ibreakpoint);
			this.\u0001(u);
			\u0002._Condition.Accept(this);
			this.\u0002();
			ibreakpoint.AddSuccessor(num);
			this.TopOfStack.CurrentBPIndex = u;
		}

		// Token: 0x06002337 RID: 9015 RVA: 0x0007987C File Offset: 0x00077A7C
		public void \u0001(_IReturnStatement \u0002)
		{
			_IBreakpoint ibreakpoint = this.\u0001(\u0002);
			if (ibreakpoint == null)
			{
				return;
			}
			int u = this.\u0001.Add(ref ibreakpoint);
			if (\u0002._Condition != null)
			{
				this.\u0001(u);
				\u0002._Condition.Accept(this);
				this.\u0002();
				ibreakpoint.AddSuccessor(this.TopOfStack.CurrentBPIndex);
			}
			ibreakpoint.AddSuccessor(0);
			this.TopOfStack.CurrentBPIndex = u;
		}

		// Token: 0x06002338 RID: 9016 RVA: 0x000798E8 File Offset: 0x00077AE8
		public void \u0001(_IJumpStatement \u0002)
		{
			_IBreakpoint ibreakpoint = this.\u0001(\u0002);
			if (ibreakpoint == null)
			{
				return;
			}
			int num = this.\u0001.Add(ref ibreakpoint);
			if (\u0002._Condition != null)
			{
				this.\u0001(num);
				\u0002._Condition.Accept(this);
				this.\u0002();
				ibreakpoint.AddSuccessor(this.TopOfStack.CurrentBPIndex);
			}
			if (!this.\u0001.ContainsKey(\u0002.Label))
			{
				if (!this.\u0001.ContainsKey(\u0002.Label))
				{
					this.\u0001.Add(\u0002.Label, new LList<int>(1));
				}
				this.\u0001[\u0002.Label].Add(num);
			}
			else
			{
				ibreakpoint.AddSuccessor(this.\u0001[\u0002.Label]);
			}
			this.TopOfStack.CurrentBPIndex = num;
		}

		// Token: 0x06002339 RID: 9017 RVA: 0x000799BC File Offset: 0x00077BBC
		public void \u0001(_ILabelStatement \u0002)
		{
			LList<int> llist = null;
			if (this.\u0001.TryGetValue(\u0002.Text, out llist))
			{
				foreach (int nIndex in llist)
				{
					(this.\u0001[nIndex] as _IBreakpoint).AddSuccessor(this.TopOfStack.CurrentBPIndex);
				}
				this.\u0001.Remove(\u0002.Text);
			}
			if (string.Compare(this.ReturnLabel, \u0002.Text, StringComparison.OrdinalIgnoreCase) == 0)
			{
				_IBreakpoint ibreakpoint = this.\u0001(\u0002);
				if (ibreakpoint != null && ibreakpoint.AssemblySuccessors != null)
				{
					this.TopOfStack.CurrentBPIndex = this.\u0001.Add(ref ibreakpoint);
				}
			}
			this.\u0001.Add(\u0002.Text, this.TopOfStack.CurrentBPIndex);
		}

		// Token: 0x0600233A RID: 9018 RVA: 0x00079AA4 File Offset: 0x00077CA4
		public void \u0001(_ICommentStatement \u0002)
		{
		}

		// Token: 0x0600233B RID: 9019 RVA: 0x00079AA8 File Offset: 0x00077CA8
		public void \u0001(_IPragmaStatement \u0002)
		{
		}

		// Token: 0x0600233C RID: 9020 RVA: 0x00079AAC File Offset: 0x00077CAC
		public void \u0001(_IExpressionStatement \u0002)
		{
			_IBreakpoint ibreakpoint = this.\u0001(\u0002);
			if (ibreakpoint == null)
			{
				return;
			}
			int u = this.\u0001.Add(ref ibreakpoint);
			ibreakpoint.AddSuccessor(this.TopOfStack.CurrentBPIndex);
			this.\u0001(u);
			\u0002._Expr.Accept(this);
			this.\u0002();
			this.TopOfStack.CurrentBPIndex = u;
		}

		// Token: 0x0600233D RID: 9021 RVA: 0x00079B0C File Offset: 0x00077D0C
		public void \u0001(_ICallExpression \u0002)
		{
			int num = this.TopOfStack.CurrentBPIndex;
			if (num < 0 || num >= this.\u0001.Count)
			{
				return;
			}
			_IBreakpoint ibreakpoint = this.\u0001[num] as _IBreakpoint;
			ICallExprInfo callInfo = \u0002.CallInfo;
			if (callInfo != null)
			{
				_IAssignmentExpression instanceAssignment = callInfo.InstanceAssignment;
				if (instanceAssignment != null)
				{
					instanceAssignment.Accept(this);
				}
			}
			_IExpression condition = \u0002._Condition;
			if (condition != null)
			{
				condition.Accept(this);
			}
			foreach (_IExpression iexpression in \u0002.ParamExpressions)
			{
				if (iexpression != null)
				{
					iexpression.Accept(this);
				}
			}
			foreach (_IExpression iexpression2 in \u0002.OutputExpressions)
			{
				if (iexpression2 != null)
				{
					iexpression2.Accept(this);
				}
			}
			IBreakpoint breakpoint = \u0002.CallBreakpoint;
			if (breakpoint == null)
			{
				breakpoint = this.\u0001(\u0002);
			}
			if (breakpoint != null && this.TopOfStack.CurrentExceptionHandlingIndex >= 0)
			{
				(breakpoint as _IBreakpoint).ExceptionHandlingSuccessor = this.TopOfStack.CurrentExceptionHandlingIndex;
			}
			if (callInfo != null)
			{
				IBreakpoint beforeCallBreakpoint = \u0002.BeforeCallBreakpoint;
				IBreakpoint breakpoint2 = breakpoint;
				if (breakpoint2 != null && beforeCallBreakpoint != null && breakpoint2.Offset == 0 && num >= 0)
				{
					int num2 = num;
					do
					{
						breakpoint2 = (this.\u0001[num2--] as _IBreakpoint);
					}
					while (breakpoint2 != null && breakpoint2.Offset <= beforeCallBreakpoint.Offset && num2 >= 0);
					if (breakpoint2 != null)
					{
						int[] assemblySuccessors = breakpoint2.AssemblySuccessors;
						breakpoint2 = \u0002.CreateBreakpoint(breakpoint2.Offset, 0);
						foreach (int nSuccessorOffset in assemblySuccessors)
						{
							breakpoint2.AddAssemblySuccessor(nSuccessorOffset);
						}
					}
				}
				_IStepInPosition istepInPosition = \u0019.\u0003.\u0001(callInfo.IdCalledSignature, breakpoint2);
				istepInPosition.KindOfCall = callInfo.KindOfCall;
				ibreakpoint.AddStepInSuccessor(istepInPosition);
				istepInPosition.StepInBreakpoint = beforeCallBreakpoint;
			}
		}

		// Token: 0x0600233E RID: 9022 RVA: 0x00079D10 File Offset: 0x00077F10
		public void \u0001(_IOperatorExpression \u0002)
		{
			foreach (_IExpression iexpression in \u0002._OperandsList)
			{
				iexpression.Accept(this);
			}
		}

		// Token: 0x0600233F RID: 9023 RVA: 0x00079D5C File Offset: 0x00077F5C
		public void \u0001(_IConversionExpression \u0002)
		{
			\u0002._Exp.Accept(this);
		}

		// Token: 0x06002340 RID: 9024 RVA: 0x00079D6C File Offset: 0x00077F6C
		public void \u0001(_ICastExpression \u0002)
		{
		}

		// Token: 0x06002341 RID: 9025 RVA: 0x00079D70 File Offset: 0x00077F70
		public void \u0001(_INewExpression \u0002)
		{
			\u0002._Count.Accept(this);
			if (\u0002._FBInitParams != null)
			{
				foreach (IAssignmentExpression assignmentExpression in \u0002._FBInitParams)
				{
					((_IAssignmentExpression)assignmentExpression).Accept(this);
				}
			}
		}

		// Token: 0x06002342 RID: 9026 RVA: 0x00079DD4 File Offset: 0x00077FD4
		public void \u0001(_ITypeExpression \u0002)
		{
		}

		// Token: 0x06002343 RID: 9027 RVA: 0x00079DD8 File Offset: 0x00077FD8
		public void \u0001(_IThisExpression \u0002)
		{
		}

		// Token: 0x06002344 RID: 9028 RVA: 0x00079DDC File Offset: 0x00077FDC
		public void \u0001(_IBaseExpression \u0002)
		{
		}

		// Token: 0x06002345 RID: 9029 RVA: 0x00079DE0 File Offset: 0x00077FE0
		public void \u0001(_ILiteralExpression \u0002)
		{
		}

		// Token: 0x06002346 RID: 9030 RVA: 0x00079DE4 File Offset: 0x00077FE4
		public void \u0001(_IAddressExpression \u0002)
		{
		}

		// Token: 0x06002347 RID: 9031 RVA: 0x00079DE8 File Offset: 0x00077FE8
		public void \u0001(_IQualifiedNameExpression \u0002)
		{
		}

		// Token: 0x06002348 RID: 9032 RVA: 0x00079DEC File Offset: 0x00077FEC
		public void \u0001(_IVariableExpression \u0002)
		{
			IVariableExprInfo varInfo = \u0002.VarInfo;
			if (varInfo != null && varInfo.IndexInfo != null && varInfo.IndexInfo.IndexExpression != null)
			{
				(varInfo.IndexInfo.IndexExpression as _IExpression).Accept(this);
			}
		}

		// Token: 0x06002349 RID: 9033 RVA: 0x00079E30 File Offset: 0x00078030
		public void \u0001(_IIndexAccessExpression \u0002)
		{
			\u0002._Var.Accept(this);
			for (int i = 0; i < \u0002.NumAccesses; i++)
			{
				\u0002.GetAccess(i).Accept(this);
			}
		}

		// Token: 0x0600234A RID: 9034 RVA: 0x00079E68 File Offset: 0x00078068
		public void \u0001(_ICompoAccessExpression \u0002)
		{
			\u0002._Left.Accept(this);
			\u0002._Right.Accept(this);
		}

		// Token: 0x0600234B RID: 9035 RVA: 0x00079E84 File Offset: 0x00078084
		public void \u0001(_IDeRefAccessExpression \u0002)
		{
			global::\u0014.\u000F u000F = \u0002.DeRefInfo as global::\u0014.\u000F;
			if (u000F != null && u000F.IndexInfo != null && u000F.IndexInfo.IndexExpression != null)
			{
				(u000F.IndexInfo.IndexExpression as _IExpression).Accept(this);
			}
			\u0002._Base.Accept(this);
		}

		// Token: 0x0600234C RID: 9036 RVA: 0x00079ED8 File Offset: 0x000780D8
		public void \u0001(_ICopyScopeExpression \u0002)
		{
			\u0002._Base.Accept(this);
		}

		// Token: 0x0600234D RID: 9037 RVA: 0x00079EE8 File Offset: 0x000780E8
		public void \u0001(_IGlobalScopeExpression \u0002)
		{
			\u0002._Base.Accept(this);
		}

		// Token: 0x0600234E RID: 9038 RVA: 0x00079EF8 File Offset: 0x000780F8
		public void \u0001(_ISystemScopeExpression \u0002)
		{
			\u0002._Base.Accept(this);
		}

		// Token: 0x0600234F RID: 9039 RVA: 0x00079F08 File Offset: 0x00078108
		public void \u0001(_IPoolScopeExpression \u0002)
		{
			\u0002._Base.Accept(this);
		}

		// Token: 0x06002350 RID: 9040 RVA: 0x00079F18 File Offset: 0x00078118
		public void \u0001(_INamespaceAccessExpression \u0002)
		{
			\u0002._Access.Accept(this);
		}

		// Token: 0x06002351 RID: 9041 RVA: 0x00079F28 File Offset: 0x00078128
		public void \u0001(_ICurrentTaskExpression \u0002)
		{
			\u0002._Base.Accept(this);
		}

		// Token: 0x06002352 RID: 9042 RVA: 0x00079F38 File Offset: 0x00078138
		public void \u0001(_IEmptyStatement \u0002)
		{
		}

		// Token: 0x06002353 RID: 9043 RVA: 0x00079F3C File Offset: 0x0007813C
		public void \u0001(_ICaseRangeExpression \u0002)
		{
			\u0002._Low.Accept(this);
			\u0002._High.Accept(this);
		}

		// Token: 0x06002354 RID: 9044 RVA: 0x00079F58 File Offset: 0x00078158
		public void \u0001(_ICaseLabelStatement \u0002)
		{
			_IBreakpoint ibreakpoint = this.\u0001(\u0002);
			if (ibreakpoint == null)
			{
				return;
			}
			int u = this.\u0001.Add(ref ibreakpoint);
			ibreakpoint.AddSuccessor(this.TopOfStack.CurrentBPIndex);
			this.\u0001(u);
			foreach (_IExpression iexpression in \u0002._cases)
			{
				iexpression.Accept(this);
			}
			this.\u0002();
			this.TopOfStack.CurrentBPIndex = u;
		}

		// Token: 0x06002355 RID: 9045 RVA: 0x00079FE8 File Offset: 0x000781E8
		public void \u0001(_ICaseStatement \u0002)
		{
			_IBreakpoint ibreakpoint = this.\u0001(\u0002);
			if (ibreakpoint == null)
			{
				return;
			}
			int num = this.TopOfStack.CurrentBPIndex;
			if (\u0002._Else != null)
			{
				this.\u0001(num);
				\u0002._Else.Accept(this);
				int nSucc = this.TopOfStack.CurrentBPIndex;
				this.\u0002();
				ibreakpoint.AddSuccessor(nSucc);
			}
			else
			{
				ibreakpoint.AddSuccessor(num);
			}
			IList<_ICase> cases = \u0002._Cases;
			for (int i = cases.Count - 1; i >= 0; i--)
			{
				_ICase icase = cases[i];
				this.\u0001(num);
				icase._Controlled.Accept(this);
				int nSucc2 = this.TopOfStack.CurrentBPIndex;
				this.\u0002();
				ibreakpoint.AddSuccessor(nSucc2);
			}
			int u = this.\u0001.Add(ref ibreakpoint);
			this.TopOfStack.CurrentBPIndex = u;
		}

		// Token: 0x06002356 RID: 9046 RVA: 0x0007A0BC File Offset: 0x000782BC
		public void \u0001(_IErrorExpression \u0002)
		{
		}

		// Token: 0x06002357 RID: 9047 RVA: 0x0007A0C0 File Offset: 0x000782C0
		public void \u0001(_IErrorStatement \u0002)
		{
		}

		// Token: 0x06002358 RID: 9048 RVA: 0x0007A0C4 File Offset: 0x000782C4
		public void \u0001(_INullExpression \u0002)
		{
		}

		// Token: 0x06002359 RID: 9049 RVA: 0x0007A0C8 File Offset: 0x000782C8
		public void \u0001(_INullStatement \u0002)
		{
		}

		// Token: 0x0600235A RID: 9050 RVA: 0x0007A0CC File Offset: 0x000782CC
		public void \u0001(_IVariableDeclarationStatement \u0002)
		{
		}

		// Token: 0x0600235B RID: 9051 RVA: 0x0007A0D0 File Offset: 0x000782D0
		public void \u0001(_IVariableDeclarationListStatement \u0002)
		{
		}

		// Token: 0x0600235C RID: 9052 RVA: 0x0007A0D4 File Offset: 0x000782D4
		public void \u0001(_IPOUDeclarationStatement \u0002)
		{
		}

		// Token: 0x0600235D RID: 9053 RVA: 0x0007A0D8 File Offset: 0x000782D8
		public void \u0001(_ITypeDeclarationStatement \u0002)
		{
		}

		// Token: 0x0600235E RID: 9054 RVA: 0x0007A0DC File Offset: 0x000782DC
		public void \u0001(_IEnumDeclarationStatement \u0002)
		{
		}

		// Token: 0x0600235F RID: 9055 RVA: 0x0007A0E0 File Offset: 0x000782E0
		public void \u0001(_IEnumDeclarationListStatement \u0002)
		{
		}

		// Token: 0x06002360 RID: 9056 RVA: 0x0007A0E4 File Offset: 0x000782E4
		public void \u0001(_IMultipleIndexInitialization \u0002)
		{
		}

		// Token: 0x06002361 RID: 9057 RVA: 0x0007A0E8 File Offset: 0x000782E8
		public void \u0001(_IArrayInitialization \u0002)
		{
		}

		// Token: 0x06002362 RID: 9058 RVA: 0x0007A0EC File Offset: 0x000782EC
		public void \u0001(_IStructureInitialization \u0002)
		{
		}

		// Token: 0x06002363 RID: 9059 RVA: 0x0007A0F0 File Offset: 0x000782F0
		public void \u0001(_IDefineReference \u0002)
		{
		}

		// Token: 0x06002364 RID: 9060 RVA: 0x0007A0F4 File Offset: 0x000782F4
		public void \u0001(_IVariableReference \u0002)
		{
		}

		// Token: 0x06002365 RID: 9061 RVA: 0x0007A0F8 File Offset: 0x000782F8
		public void \u0001(_ITypeReference \u0002)
		{
		}

		// Token: 0x06002366 RID: 9062 RVA: 0x0007A0FC File Offset: 0x000782FC
		public void \u0001(_IPouReference \u0002)
		{
		}

		// Token: 0x06002367 RID: 9063 RVA: 0x0007A100 File Offset: 0x00078300
		public void \u0001(_ITaskReference \u0002)
		{
		}

		// Token: 0x06002368 RID: 9064 RVA: 0x0007A104 File Offset: 0x00078304
		public void \u0001(_IResourceReference \u0002)
		{
		}

		// Token: 0x06002369 RID: 9065 RVA: 0x0007A108 File Offset: 0x00078308
		public void \u0001(_IDefinedExpression \u0002)
		{
		}

		// Token: 0x0600236A RID: 9066 RVA: 0x0007A10C File Offset: 0x0007830C
		public void \u0001(_IPragmaOperatorExpression \u0002)
		{
		}

		// Token: 0x0600236B RID: 9067 RVA: 0x0007A110 File Offset: 0x00078310
		public void \u0001(_IPragmaIfStatement \u0002)
		{
		}

		// Token: 0x0600236C RID: 9068 RVA: 0x0007A114 File Offset: 0x00078314
		public void \u0001(_IPragmaAssertion \u0002)
		{
		}

		// Token: 0x0600236D RID: 9069 RVA: 0x0007A118 File Offset: 0x00078318
		public void \u0001(_ICompilerVersionExpression \u0002)
		{
		}

		// Token: 0x0600236E RID: 9070 RVA: 0x0007A11C File Offset: 0x0007831C
		public void \u0001(_IRuntimeVersionExpression \u0002)
		{
		}

		// Token: 0x0600236F RID: 9071 RVA: 0x0007A120 File Offset: 0x00078320
		public void \u0001(_IBreakPointStatement \u0002)
		{
			if (this.TopOfStack.CurrentBPIndex > 0)
			{
				this.\u0001.ChangeSourcePos(this.\u0001[this.TopOfStack.CurrentBPIndex], \u0002.BPPosition);
			}
		}

		// Token: 0x06002370 RID: 9072 RVA: 0x0007A158 File Offset: 0x00078358
		public void \u0001(_IDefineStatement \u0002)
		{
		}

		// Token: 0x06002371 RID: 9073 RVA: 0x0007A15C File Offset: 0x0007835C
		public void \u0001(_IXRefExpression \u0002)
		{
		}

		// Token: 0x06002372 RID: 9074 RVA: 0x0007A160 File Offset: 0x00078360
		public void \u0001(_IHasTypeExpression \u0002)
		{
		}

		// Token: 0x06002373 RID: 9075 RVA: 0x0007A164 File Offset: 0x00078364
		public void \u0001(_IIsEnumTypeExpression \u0002)
		{
		}

		// Token: 0x06002374 RID: 9076 RVA: 0x0007A168 File Offset: 0x00078368
		public void \u0001(_IHasAttributeExpression \u0002)
		{
		}

		// Token: 0x06002375 RID: 9077 RVA: 0x0007A16C File Offset: 0x0007836C
		public void \u0001(_IHasValueExpression \u0002)
		{
		}

		// Token: 0x06002376 RID: 9078 RVA: 0x0007A170 File Offset: 0x00078370
		public void \u0001(_IHasConstantValueExpression \u0002)
		{
		}

		// Token: 0x06002377 RID: 9079 RVA: 0x0007A174 File Offset: 0x00078374
		public void \u0001(_IHasConstantTypeExpression \u0002)
		{
		}

		// Token: 0x06002378 RID: 9080 RVA: 0x0007A178 File Offset: 0x00078378
		public void \u0001(_IPartialAccessExpression \u0002)
		{
			\u0002._Left.Accept(this);
		}

		// Token: 0x06002379 RID: 9081 RVA: 0x0007A188 File Offset: 0x00078388
		public void \u0001(_IProjectDefinedExpression \u0002)
		{
		}

		// Token: 0x0600237A RID: 9082 RVA: 0x0007A18C File Offset: 0x0007838C
		public void \u0001(_ITryCatchStatement \u0002)
		{
			if (\u0002._Finally != null)
			{
				\u0002._Finally.Accept(this);
			}
			int u = this.TopOfStack.CurrentBPIndex;
			if (\u0002._Catch != null)
			{
				\u0002._Catch.Accept(this);
			}
			int num = this.TopOfStack.CurrentBPIndex;
			this.TopOfStack.CurrentBPIndex = u;
			int u2 = this.TopOfStack.CurrentExceptionHandlingIndex;
			this.TopOfStack.CurrentExceptionHandlingIndex = num;
			if (\u0002._Try != null)
			{
				\u0002._Try.Accept(this);
			}
			int num2 = this.TopOfStack.CurrentBPIndex;
			_IBreakpoint ibreakpoint = this.\u0001(\u0002);
			if (ibreakpoint != null)
			{
				(this.\u0001[num2] as _IBreakpoint).AddSuccessor(num);
				return;
			}
			ibreakpoint.AddSuccessor(num2);
			ibreakpoint.AddSuccessor(num);
			int u3 = this.\u0001.Add(ref ibreakpoint);
			this.TopOfStack.CurrentBPIndex = u3;
			this.TopOfStack.CurrentExceptionHandlingIndex = u2;
		}

		// Token: 0x04000631 RID: 1585
		private _IBreakpointList \u0001;

		// Token: 0x04000632 RID: 1586
		private readonly ICaseInsensitiveDictionary<int> \u0001 = new CaseInsensitiveDictionary<int>();

		// Token: 0x04000633 RID: 1587
		private readonly ICaseInsensitiveDictionary<LList<int>> \u0001 = new CaseInsensitiveDictionary<LList<int>>();

		// Token: 0x04000634 RID: 1588
		private readonly Stack<\u0019.\u0006> \u0001 = new Stack<\u0019.\u0006>();

		// Token: 0x04000635 RID: 1589
		private int \u0001 = -1;

		// Token: 0x04000636 RID: 1590
		private int \u0002 = -1;

		// Token: 0x04000637 RID: 1591
		private _ICompiledPOU \u0001;

		// Token: 0x04000638 RID: 1592
		private readonly \u001E.\u000E \u0001;
	}
}
