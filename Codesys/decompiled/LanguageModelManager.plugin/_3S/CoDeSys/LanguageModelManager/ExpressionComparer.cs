using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200011D RID: 285
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)", Justification = "Visitor pattern")]
	public class ExpressionComparer : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor, IExprementVisitor2
	{
		// Token: 0x0600172A RID: 5930 RVA: 0x0003F7F4 File Offset: 0x0003E7F4
		public ExpressionComparer(_ICompiledPOU cpouToCompareWith, bool bCreatePositionTable)
		{
			this.m_cpouToCompareWith = cpouToCompareWith;
			this.m_bCreatePositionTable = bCreatePositionTable;
		}

		// Token: 0x0600172B RID: 5931 RVA: 0x0003F848 File Offset: 0x0003E848
		public ExpressionComparer(_IExprement exprToCompareWith)
		{
			this.Push(exprToCompareWith);
		}

		// Token: 0x0600172C RID: 5932 RVA: 0x0003F894 File Offset: 0x0003E894
		private void Push(_IExprement exprement)
		{
			EXCStackContent excstackContent = new EXCStackContent();
			excstackContent.CurrentExprement = exprement;
			this.m_stackAttributes.Push(excstackContent);
		}

		// Token: 0x0600172D RID: 5933 RVA: 0x0003F8BA File Offset: 0x0003E8BA
		private void Pop()
		{
			this.m_stackAttributes.Pop();
		}

		// Token: 0x170005D1 RID: 1489
		// (get) Token: 0x0600172E RID: 5934 RVA: 0x0003F8C8 File Offset: 0x0003E8C8
		internal LDictionary<IMinimalPosition, IMinimalPosition> PositionTable
		{
			get
			{
				return this._positiontable;
			}
		}

		// Token: 0x170005D2 RID: 1490
		// (get) Token: 0x0600172F RID: 5935 RVA: 0x0003F8D0 File Offset: 0x0003E8D0
		private EXCStackContent TopOfStack
		{
			get
			{
				return this.m_stackAttributes.Peek() as EXCStackContent;
			}
		}

		// Token: 0x06001730 RID: 5936 RVA: 0x0003F8E2 File Offset: 0x0003E8E2
		public bool IsEqual(bool bCode, bool bComments, bool bPositions)
		{
			return (!bCode || this.CodeEqual) && (!bComments || this.m_bCommentsEqual) && (!bPositions || this.m_bPositionsEqual);
		}

		// Token: 0x170005D3 RID: 1491
		// (get) Token: 0x06001731 RID: 5937 RVA: 0x0003F907 File Offset: 0x0003E907
		// (set) Token: 0x06001732 RID: 5938 RVA: 0x0003F90F File Offset: 0x0003E90F
		public bool CodeEqual { get; set; } = true;

		// Token: 0x170005D4 RID: 1492
		// (get) Token: 0x06001733 RID: 5939 RVA: 0x0003F918 File Offset: 0x0003E918
		// (set) Token: 0x06001734 RID: 5940 RVA: 0x0003F920 File Offset: 0x0003E920
		public bool MessagesEqual { get; set; } = true;

		// Token: 0x06001735 RID: 5941 RVA: 0x0003F92C File Offset: 0x0003E92C
		public bool CompareCommon(_IExprement expr1, _IExprement expr2)
		{
			if (expr2 == null)
			{
				this.CodeEqual = false;
				return false;
			}
			if (this.m_bPositionsEqual)
			{
				if ((expr1._Position != null && expr2._Position == null) || (expr1._Position == null && expr2._Position != null))
				{
					this.m_bPositionsEqual = false;
				}
				else if (expr1._Position != null && expr2._Position != null && ((expr1._Position.EditorPosition & 281474976710655L) != (expr2._Position.EditorPosition & 281474976710655L) || expr1._Position.PositionOffset != expr2._Position.PositionOffset || expr1.PositionLength != expr2.PositionLength))
				{
					this.m_bPositionsEqual = false;
				}
			}
			if (this.MessagesEqual)
			{
				IList<_ICompilerMessage> messagesList = expr1.MessagesList;
				IList<_ICompilerMessage> messagesList2 = expr2.MessagesList;
				if (messagesList == null && messagesList2 == null)
				{
					this.MessagesEqual = true;
				}
				else if (messagesList == null || messagesList2 == null)
				{
					this.MessagesEqual = false;
				}
				else if (messagesList.Count != messagesList2.Count)
				{
					this.MessagesEqual = false;
				}
				else
				{
					for (int i = 0; i < messagesList.Count; i++)
					{
						if (messagesList[i].Severity != messagesList2[i].Severity || messagesList[i].Text != messagesList2[i].Text)
						{
							this.MessagesEqual = false;
						}
					}
				}
			}
			if (this.m_bCreatePositionTable && expr2._Position != null && expr1._Position != null && !this._positiontable.ContainsKey(expr1._Position))
			{
				this._positiontable[expr1._Position] = expr2._Position;
			}
			if (this.m_bCopyMessages)
			{
				expr1.MessagesList = expr2.MessagesList;
			}
			if (!expr2.GetType().Equals(expr1.GetType()) || !this.CodeEqual)
			{
				this.CodeEqual = false;
				return false;
			}
			return true;
		}

		// Token: 0x06001736 RID: 5942 RVA: 0x0003FAFC File Offset: 0x0003EAFC
		public void visit(_ICompiledPOU cpou)
		{
			if (cpou.Name != this.m_cpouToCompareWith.Name)
			{
				this.CodeEqual = false;
				return;
			}
			this.Push(this.m_cpouToCompareWith.GetParseTree());
			cpou.GetParseTree().Accept(this);
			this.Pop();
		}

		// Token: 0x06001737 RID: 5943 RVA: 0x0003FB4C File Offset: 0x0003EB4C
		public void visit(_IWhileStatement whilst)
		{
			if (!this.CompareCommon(whilst, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IWhileStatement iwhileStatement = this.TopOfStack.CurrentExprement as _IWhileStatement;
			if (iwhileStatement == null)
			{
				return;
			}
			this.Push(iwhileStatement._Condition);
			whilst._Condition.Accept(this);
			this.Pop();
			this.Push(iwhileStatement._Controlled);
			whilst._Controlled.Accept(this);
			this.Pop();
		}

		// Token: 0x06001738 RID: 5944 RVA: 0x0003FBC0 File Offset: 0x0003EBC0
		public void visit(_IRepeatStatement repeat)
		{
			if (!this.CompareCommon(repeat, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IRepeatStatement irepeatStatement = this.TopOfStack.CurrentExprement as _IRepeatStatement;
			if (irepeatStatement == null)
			{
				return;
			}
			this.Push(irepeatStatement._Condition);
			repeat._Condition.Accept(this);
			this.Pop();
			this.Push(irepeatStatement._Controlled);
			repeat._Controlled.Accept(this);
			this.Pop();
		}

		// Token: 0x06001739 RID: 5945 RVA: 0x0003FC34 File Offset: 0x0003EC34
		public void visit(_IForStatement forloop)
		{
			if (!this.CompareCommon(forloop, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IForStatement iforStatement = this.TopOfStack.CurrentExprement as _IForStatement;
			if (iforStatement == null)
			{
				return;
			}
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351600 && forloop._Counter != null)
			{
				this.Push(iforStatement._Counter);
				forloop._Counter.Accept(this);
				this.Pop();
			}
			this.Push(iforStatement._CounterStart);
			forloop._CounterStart.Accept(this);
			this.Pop();
			this.Push(iforStatement._UpperBound);
			forloop._UpperBound.Accept(this);
			this.Pop();
			if ((forloop._By == null && iforStatement._By != null) || (forloop._By != null && iforStatement._By == null))
			{
				this.CodeEqual = false;
			}
			else if (forloop._By != null && iforStatement._By != null)
			{
				this.Push(iforStatement._By);
				forloop._By.Accept(this);
				this.Pop();
			}
			this.Push(iforStatement._Controlled);
			forloop._Controlled.Accept(this);
			this.Pop();
		}

		// Token: 0x0600173A RID: 5946 RVA: 0x0003FD53 File Offset: 0x0003ED53
		public void visit(_IExitStatement exit)
		{
			this.CompareCommon(exit, this.TopOfStack.CurrentExprement);
		}

		// Token: 0x0600173B RID: 5947 RVA: 0x0003FD53 File Offset: 0x0003ED53
		public void visit(_IContinueStatement cont)
		{
			this.CompareCommon(cont, this.TopOfStack.CurrentExprement);
		}

		// Token: 0x0600173C RID: 5948 RVA: 0x0003FD68 File Offset: 0x0003ED68
		private static bool IsCommentStatement(_IStatement statement)
		{
			return statement is _ICommentStatement;
		}

		// Token: 0x0600173D RID: 5949 RVA: 0x0003FD74 File Offset: 0x0003ED74
		public void visit(_ISequenceStatement seq)
		{
			if (!this.CompareCommon(seq, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_ISequenceStatement isequenceStatement = this.TopOfStack.CurrentExprement as _ISequenceStatement;
			if (isequenceStatement == null)
			{
				return;
			}
			IList<_IStatement> statementList = seq._StatementList;
			IList<_IStatement> statementList2 = isequenceStatement._StatementList;
			int i = 0;
			int j = 0;
			while (i < statementList.Count && j < statementList2.Count && this.CodeEqual)
			{
				bool flag = ExpressionComparer.IsCommentStatement(statementList[i]);
				bool flag2 = ExpressionComparer.IsCommentStatement(statementList2[j]);
				if (flag && !flag2)
				{
					i++;
					this.m_bCommentsEqual = false;
				}
				else if (!flag && flag2)
				{
					j++;
					this.m_bCommentsEqual = false;
				}
				else
				{
					this.Push(statementList2[j]);
					statementList[i].Accept(this);
					this.Pop();
					i++;
					j++;
				}
			}
			if (this.CodeEqual)
			{
				while (i < statementList.Count)
				{
					if (!ExpressionComparer.IsCommentStatement(statementList[i]))
					{
						this.CodeEqual = false;
					}
					else
					{
						this.m_bCommentsEqual = false;
					}
					i++;
				}
				while (j < statementList2.Count)
				{
					if (!ExpressionComparer.IsCommentStatement(statementList2[j]))
					{
						this.CodeEqual = false;
					}
					else
					{
						this.m_bCommentsEqual = false;
					}
					j++;
				}
			}
		}

		// Token: 0x0600173E RID: 5950 RVA: 0x0003FEBC File Offset: 0x0003EEBC
		public void visit(_IAssignmentExpression assign)
		{
			if (!this.CompareCommon(assign, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IAssignmentExpression iassignmentExpression = this.TopOfStack.CurrentExprement as _IAssignmentExpression;
			if (iassignmentExpression == null)
			{
				return;
			}
			this.Push(iassignmentExpression._LValue);
			assign._LValue.Accept(this);
			this.Pop();
			this.Push(iassignmentExpression._RValue);
			assign._RValue.Accept(this);
			this.Pop();
			if (assign.KindOf != iassignmentExpression.KindOf)
			{
				this.CodeEqual = false;
			}
		}

		// Token: 0x0600173F RID: 5951 RVA: 0x0003FF44 File Offset: 0x0003EF44
		public void visit(_IIfStatement ifst)
		{
			if (!this.CompareCommon(ifst, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IIfStatement iifStatement = this.TopOfStack.CurrentExprement as _IIfStatement;
			if (iifStatement == null)
			{
				return;
			}
			this.Push(iifStatement._Condition);
			ifst._Condition.Accept(this);
			this.Pop();
			this.Push(iifStatement._IfThen);
			ifst._IfThen.Accept(this);
			this.Pop();
			if (ifst._ElseIf != null && iifStatement._ElseIf != null)
			{
				IList<_IElseIf> elseIf = ifst._ElseIf;
				IList<_IElseIf> elseIf2 = iifStatement._ElseIf;
				if (elseIf.Count != elseIf2.Count)
				{
					this.CodeEqual = false;
				}
				else
				{
					for (int i = 0; i < elseIf.Count; i++)
					{
						this.Push(elseIf2[i]._Condition);
						elseIf[i]._Condition.Accept(this);
						this.Pop();
						this.Push(elseIf2[i]._Controlled);
						elseIf[i]._Controlled.Accept(this);
						this.Pop();
					}
				}
			}
			else if (ifst._ElseIf != null || iifStatement._ElseIf != null)
			{
				this.CodeEqual = false;
			}
			if (ifst._IfElse != null && iifStatement._IfElse != null)
			{
				this.Push(iifStatement._IfElse);
				ifst._IfElse.Accept(this);
				this.Pop();
				return;
			}
			if (ifst._IfElse != null || iifStatement._IfElse != null)
			{
				this.CodeEqual = false;
			}
		}

		// Token: 0x06001740 RID: 5952 RVA: 0x000400B8 File Offset: 0x0003F0B8
		public void visit(_IReturnStatement returnst)
		{
			if (!this.CompareCommon(returnst, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IReturnStatement ireturnStatement = this.TopOfStack.CurrentExprement as _IReturnStatement;
			if (ireturnStatement == null)
			{
				return;
			}
			if (returnst._Condition != null && ireturnStatement._Condition != null)
			{
				this.Push(ireturnStatement._Condition);
				returnst._Condition.Accept(this);
				this.Pop();
				return;
			}
			if (returnst._Condition != null || ireturnStatement._Condition != null)
			{
				this.CodeEqual = false;
			}
		}

		// Token: 0x06001741 RID: 5953 RVA: 0x00040138 File Offset: 0x0003F138
		public void visit(_IJumpStatement gotost)
		{
			if (!this.CompareCommon(gotost, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IJumpStatement ijumpStatement = this.TopOfStack.CurrentExprement as _IJumpStatement;
			if (ijumpStatement == null)
			{
				return;
			}
			if (gotost._Condition != null && ijumpStatement._Condition != null)
			{
				this.Push(ijumpStatement._Condition);
				gotost._Condition.Accept(this);
				this.Pop();
			}
			else if (gotost._Condition != null || ijumpStatement._Condition != null)
			{
				this.CodeEqual = false;
			}
			if (gotost.Label != ijumpStatement.Label)
			{
				this.CodeEqual = false;
			}
		}

		// Token: 0x06001742 RID: 5954 RVA: 0x000401D0 File Offset: 0x0003F1D0
		public void visit(_ILabelStatement label)
		{
			if (!this.CompareCommon(label, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_ILabelStatement ilabelStatement = this.TopOfStack.CurrentExprement as _ILabelStatement;
			if (ilabelStatement == null)
			{
				return;
			}
			if (label.Text != ilabelStatement.Text)
			{
				this.CodeEqual = false;
			}
		}

		// Token: 0x06001743 RID: 5955 RVA: 0x00040224 File Offset: 0x0003F224
		public void visit(_ICommentStatement comment)
		{
			if (!this.CompareCommon(comment, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_ICommentStatement icommentStatement = this.TopOfStack.CurrentExprement as _ICommentStatement;
			if (icommentStatement == null)
			{
				return;
			}
			if (comment.Text != icommentStatement.Text)
			{
				this.m_bCommentsEqual = false;
			}
		}

		// Token: 0x06001744 RID: 5956 RVA: 0x00040278 File Offset: 0x0003F278
		public void visit(_IPragmaStatement pragma)
		{
			if (!this.CompareCommon(pragma, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IPragmaStatement ipragmaStatement = this.TopOfStack.CurrentExprement as _IPragmaStatement;
			if (ipragmaStatement == null)
			{
				return;
			}
			if (pragma.Text != ipragmaStatement.Text)
			{
				this.CodeEqual = false;
			}
		}

		// Token: 0x06001745 RID: 5957 RVA: 0x000402CC File Offset: 0x0003F2CC
		public void visit(_IExpressionStatement expstat)
		{
			if (!this.CompareCommon(expstat, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IExpressionStatement iexpressionStatement = this.TopOfStack.CurrentExprement as _IExpressionStatement;
			if (iexpressionStatement == null)
			{
				return;
			}
			this.Push(iexpressionStatement._Expr);
			expstat._Expr.Accept(this);
			this.Pop();
		}

		// Token: 0x06001746 RID: 5958 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IVariableDeclarationStatement vds)
		{
		}

		// Token: 0x06001747 RID: 5959 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IVariableDeclarationListStatement vdls)
		{
		}

		// Token: 0x06001748 RID: 5960 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IPOUDeclarationStatement pds)
		{
		}

		// Token: 0x06001749 RID: 5961 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_ITypeDeclarationStatement tds)
		{
		}

		// Token: 0x0600174A RID: 5962 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IEnumDeclarationStatement eds)
		{
		}

		// Token: 0x0600174B RID: 5963 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IEnumDeclarationListStatement eds)
		{
		}

		// Token: 0x0600174C RID: 5964 RVA: 0x00040324 File Offset: 0x0003F324
		public void visit(_ICallExpression call)
		{
			if (!this.CompareCommon(call, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_ICallExpression icallExpression = this.TopOfStack.CurrentExprement as _ICallExpression;
			if (icallExpression == null)
			{
				return;
			}
			if (call._Condition != null && icallExpression._Condition != null)
			{
				this.Push(icallExpression._Condition);
				call._Condition.Accept(this);
				this.Pop();
			}
			else if (call._Condition != null || icallExpression._Condition != null)
			{
				this.CodeEqual = false;
			}
			if (call.ExpectedType != null && icallExpression.ExpectedType != null)
			{
				if (!call.ExpectedType.IsEqual(icallExpression.ExpectedType))
				{
					this.CodeEqual = false;
				}
			}
			else if (call.ExpectedType != null || call.ExpectedType != null)
			{
				this.CodeEqual = false;
			}
			this.Push(icallExpression._Callee);
			call._Callee.Accept(this);
			this.Pop();
			if (call.Inputs.Count > 0 && call.Inputs[0] == null)
			{
				if (icallExpression.Inputs.Count != call.Inputs.Count)
				{
					this.CodeEqual = false;
				}
				else if (icallExpression.Inputs[0] != null)
				{
					this.CodeEqual = false;
				}
				else
				{
					IList<_IExpression> paramExpressions = call.ParamExpressions;
					IList<_IExpression> paramExpressions2 = icallExpression.ParamExpressions;
					if (paramExpressions.Count != paramExpressions2.Count)
					{
						this.CodeEqual = false;
					}
					else
					{
						for (int i = 0; i < paramExpressions.Count; i++)
						{
							this.Push(paramExpressions2[i]);
							paramExpressions[i].Accept(this);
							this.Pop();
						}
					}
				}
			}
			else if (icallExpression.Inputs.Count > 0 && icallExpression.Inputs[0] == null)
			{
				this.CodeEqual = false;
			}
			else
			{
				IList<_IAssignmentExpression> inputAssigns = call._InputAssigns;
				IList<_IAssignmentExpression> inputAssigns2 = icallExpression._InputAssigns;
				if (inputAssigns.Count != inputAssigns2.Count)
				{
					this.CodeEqual = false;
				}
				else
				{
					for (int j = 0; j < inputAssigns.Count; j++)
					{
						this.Push(inputAssigns2[j]);
						inputAssigns[j].Accept(this);
						this.Pop();
					}
				}
			}
			IList<_IAssignmentExpression> outputAssigns = call._OutputAssigns;
			IList<_IAssignmentExpression> outputAssigns2 = icallExpression._OutputAssigns;
			if (outputAssigns.Count != outputAssigns2.Count)
			{
				this.CodeEqual = false;
				return;
			}
			for (int k = 0; k < outputAssigns.Count; k++)
			{
				this.Push(outputAssigns2[k]);
				outputAssigns[k].Accept(this);
				this.Pop();
			}
		}

		// Token: 0x0600174D RID: 5965 RVA: 0x000405B0 File Offset: 0x0003F5B0
		public void visit(_IOperatorExpression op)
		{
			if (!this.CompareCommon(op, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IOperatorExpression ioperatorExpression = this.TopOfStack.CurrentExprement as _IOperatorExpression;
			if (ioperatorExpression == null)
			{
				return;
			}
			if (op.Code != ioperatorExpression.Code)
			{
				this.CodeEqual = false;
				return;
			}
			IList<_IExpression> operandsList = op._OperandsList;
			IList<_IExpression> operandsList2 = ioperatorExpression._OperandsList;
			if (operandsList.Count != operandsList2.Count)
			{
				this.CodeEqual = false;
				return;
			}
			for (int i = 0; i < operandsList.Count; i++)
			{
				this.Push(operandsList2[i]);
				operandsList[i].Accept(this);
				this.Pop();
			}
		}

		// Token: 0x0600174E RID: 5966 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_ICastExpression castexp)
		{
		}

		// Token: 0x0600174F RID: 5967 RVA: 0x00040654 File Offset: 0x0003F654
		public void visit(_INewExpression typeref)
		{
			if (!this.CompareCommon(typeref, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_INewExpression inewExpression = this.TopOfStack.CurrentExprement as _INewExpression;
			if (inewExpression == null)
			{
				return;
			}
			this.Push(inewExpression._Count);
			typeref._Count.Accept(this);
			this.Pop();
			if (!inewExpression._TypeToCast.IsEqual(typeref._TypeToCast))
			{
				this.CodeEqual = false;
			}
			if (typeref._FBInitParams == null)
			{
				if (inewExpression._FBInitParams != null)
				{
					this.CodeEqual = false;
				}
				return;
			}
			if (inewExpression._FBInitParams == null || inewExpression._FBInitParams.Count != typeref._FBInitParams.Count)
			{
				this.CodeEqual = false;
				return;
			}
			for (int i = 0; i < typeref._FBInitParams.Count; i++)
			{
				this.Push(inewExpression._FBInitParams[i] as _IAssignmentExpression);
				(typeref._FBInitParams[i] as _IAssignmentExpression).Accept(this);
				this.Pop();
			}
		}

		// Token: 0x06001750 RID: 5968 RVA: 0x00040750 File Offset: 0x0003F750
		public void visit(_ITypeExpression typeexp)
		{
			if (!this.CompareCommon(typeexp, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_ITypeExpression itypeExpression = this.TopOfStack.CurrentExprement as _ITypeExpression;
			if (itypeExpression == null)
			{
				return;
			}
			if (!itypeExpression._CompiledType.IsEqual(typeexp._CompiledType))
			{
				this.CodeEqual = false;
			}
		}

		// Token: 0x06001751 RID: 5969 RVA: 0x000407A4 File Offset: 0x0003F7A4
		public void visit(_IConversionExpression conv)
		{
			if (!this.CompareCommon(conv, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IConversionExpression iconversionExpression = this.TopOfStack.CurrentExprement as _IConversionExpression;
			if (iconversionExpression == null)
			{
				return;
			}
			this.Push(iconversionExpression._Exp);
			conv._Exp.Accept(this);
			this.Pop();
			if (conv.From != iconversionExpression.From || conv.To != iconversionExpression.To)
			{
				this.CodeEqual = false;
			}
		}

		// Token: 0x06001752 RID: 5970 RVA: 0x0003FD53 File Offset: 0x0003ED53
		public void visit(_IThisExpression thisexp)
		{
			this.CompareCommon(thisexp, this.TopOfStack.CurrentExprement);
		}

		// Token: 0x06001753 RID: 5971 RVA: 0x0003FD53 File Offset: 0x0003ED53
		public void visit(_IBaseExpression baseexp)
		{
			this.CompareCommon(baseexp, this.TopOfStack.CurrentExprement);
		}

		// Token: 0x06001754 RID: 5972 RVA: 0x0004081C File Offset: 0x0003F81C
		public void visit(_ILiteralExpression literal)
		{
			if (!this.CompareCommon(literal, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_ILiteralExpression iliteralExpression = this.TopOfStack.CurrentExprement as _ILiteralExpression;
			if (iliteralExpression == null)
			{
				return;
			}
			if (literal.ConstantType != iliteralExpression.ConstantType || literal.LongValue != iliteralExpression.LongValue || literal.ULongValue != iliteralExpression.ULongValue || literal.RealValue != iliteralExpression.RealValue || literal.StringValue != iliteralExpression.StringValue || literal.Negative != iliteralExpression.Negative)
			{
				this.CodeEqual = false;
			}
		}

		// Token: 0x06001755 RID: 5973 RVA: 0x000408B4 File Offset: 0x0003F8B4
		public void visit(_IAddressExpression address)
		{
			if (!this.CompareCommon(address, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IAddressExpression iaddressExpression = this.TopOfStack.CurrentExprement as _IAddressExpression;
			if (iaddressExpression == null)
			{
				return;
			}
			if (!address.DirectAddress.IsEqual(iaddressExpression.DirectAddress))
			{
				this.CodeEqual = false;
			}
		}

		// Token: 0x06001756 RID: 5974 RVA: 0x00040908 File Offset: 0x0003F908
		public void visit(_IQualifiedNameExpression qne)
		{
			if (!this.CompareCommon(qne, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IQualifiedNameExpression iqualifiedNameExpression = this.TopOfStack.CurrentExprement as _IQualifiedNameExpression;
			if (iqualifiedNameExpression == null)
			{
				return;
			}
			if (iqualifiedNameExpression.Name != qne.Name || iqualifiedNameExpression.Namespace != qne.Namespace)
			{
				this.CodeEqual = false;
			}
		}

		// Token: 0x06001757 RID: 5975 RVA: 0x0004096C File Offset: 0x0003F96C
		public void visit(_IVariableExpression variable)
		{
			if (!this.CompareCommon(variable, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IVariableExpression ivariableExpression = this.TopOfStack.CurrentExprement as _IVariableExpression;
			if (ivariableExpression == null)
			{
				return;
			}
			if (variable.Name != ivariableExpression.Name)
			{
				this.CodeEqual = false;
			}
		}

		// Token: 0x06001758 RID: 5976 RVA: 0x000409C0 File Offset: 0x0003F9C0
		public void visit(_IIndexAccessExpression indexaccess)
		{
			if (!this.CompareCommon(indexaccess, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IIndexAccessExpression iindexAccessExpression = this.TopOfStack.CurrentExprement as _IIndexAccessExpression;
			if (iindexAccessExpression == null)
			{
				return;
			}
			this.Push(iindexAccessExpression._Var);
			indexaccess._Var.Accept(this);
			this.Pop();
			ICollection<_IExpression> accesses = indexaccess._Accesses;
			ICollection<_IExpression> accesses2 = iindexAccessExpression._Accesses;
			if (accesses.Count != accesses2.Count)
			{
				this.CodeEqual = false;
				return;
			}
			for (int i = 0; i < accesses.Count; i++)
			{
				this.Push(indexaccess.GetAccess(i));
				iindexAccessExpression.GetAccess(i).Accept(this);
				this.Pop();
			}
		}

		// Token: 0x06001759 RID: 5977 RVA: 0x00040A6C File Offset: 0x0003FA6C
		public void visit(_ICompoAccessExpression compo)
		{
			if (!this.CompareCommon(compo, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_ICompoAccessExpression icompoAccessExpression = this.TopOfStack.CurrentExprement as _ICompoAccessExpression;
			if (icompoAccessExpression == null)
			{
				return;
			}
			this.Push(icompoAccessExpression._Left);
			compo._Left.Accept(this);
			this.Pop();
			this.Push(icompoAccessExpression._Right);
			compo._Right.Accept(this);
			this.Pop();
		}

		// Token: 0x0600175A RID: 5978 RVA: 0x00040AE0 File Offset: 0x0003FAE0
		public void visit(_IDeRefAccessExpression deref)
		{
			if (!this.CompareCommon(deref, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IDeRefAccessExpression ideRefAccessExpression = this.TopOfStack.CurrentExprement as _IDeRefAccessExpression;
			if (ideRefAccessExpression == null)
			{
				return;
			}
			this.Push(ideRefAccessExpression._Base);
			deref._Base.Accept(this);
			this.Pop();
		}

		// Token: 0x0600175B RID: 5979 RVA: 0x00040B38 File Offset: 0x0003FB38
		public void visit(_ICopyScopeExpression copyexp)
		{
			if (!this.CompareCommon(copyexp, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_ICopyScopeExpression icopyScopeExpression = this.TopOfStack.CurrentExprement as _ICopyScopeExpression;
			if (icopyScopeExpression == null)
			{
				return;
			}
			this.Push(icopyScopeExpression._Base);
			copyexp._Base.Accept(this);
			this.Pop();
		}

		// Token: 0x0600175C RID: 5980 RVA: 0x00040B90 File Offset: 0x0003FB90
		public void visit(_IGlobalScopeExpression globexp)
		{
			if (!this.CompareCommon(globexp, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IGlobalScopeExpression iglobalScopeExpression = this.TopOfStack.CurrentExprement as _IGlobalScopeExpression;
			if (iglobalScopeExpression == null)
			{
				return;
			}
			this.Push(iglobalScopeExpression._Base);
			globexp._Base.Accept(this);
			this.Pop();
		}

		// Token: 0x0600175D RID: 5981 RVA: 0x00040BE8 File Offset: 0x0003FBE8
		public void visit(_ISystemScopeExpression systemscope)
		{
			if (!this.CompareCommon(systemscope, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_ISystemScopeExpression isystemScopeExpression = this.TopOfStack.CurrentExprement as _ISystemScopeExpression;
			if (isystemScopeExpression == null)
			{
				return;
			}
			this.Push(isystemScopeExpression._Base);
			systemscope._Base.Accept(this);
			this.Pop();
		}

		// Token: 0x0600175E RID: 5982 RVA: 0x00040C40 File Offset: 0x0003FC40
		public void visit(_IPoolScopeExpression poolscope)
		{
			if (!this.CompareCommon(poolscope, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IPoolScopeExpression ipoolScopeExpression = this.TopOfStack.CurrentExprement as _IPoolScopeExpression;
			if (ipoolScopeExpression == null)
			{
				return;
			}
			this.Push(ipoolScopeExpression._Base);
			poolscope._Base.Accept(this);
			this.Pop();
		}

		// Token: 0x0600175F RID: 5983 RVA: 0x0003FD53 File Offset: 0x0003ED53
		public void visit(_IEmptyStatement empty)
		{
			this.CompareCommon(empty, this.TopOfStack.CurrentExprement);
		}

		// Token: 0x06001760 RID: 5984 RVA: 0x00040C98 File Offset: 0x0003FC98
		public void visit(_ICaseRangeExpression caserange)
		{
			if (!this.CompareCommon(caserange, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_ICaseRangeExpression icaseRangeExpression = this.TopOfStack.CurrentExprement as _ICaseRangeExpression;
			if (icaseRangeExpression == null)
			{
				return;
			}
			this.Push(icaseRangeExpression._High);
			caserange._High.Accept(this);
			this.Pop();
			this.Push(icaseRangeExpression._Low);
			caserange._Low.Accept(this);
			this.Pop();
		}

		// Token: 0x06001761 RID: 5985 RVA: 0x00040D0C File Offset: 0x0003FD0C
		public void visit(_ICaseLabelStatement caselabel)
		{
			if (!this.CompareCommon(caselabel, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_ICaseLabelStatement icaseLabelStatement = this.TopOfStack.CurrentExprement as _ICaseLabelStatement;
			if (icaseLabelStatement == null)
			{
				return;
			}
			IList<_IExpression> cases = caselabel._cases;
			IList<_IExpression> cases2 = icaseLabelStatement._cases;
			if (cases.Count != cases2.Count)
			{
				this.CodeEqual = false;
				return;
			}
			for (int i = 0; i < cases.Count; i++)
			{
				this.Push(cases2[i]);
				cases[i].Accept(this);
				this.Pop();
			}
		}

		// Token: 0x06001762 RID: 5986 RVA: 0x00040D98 File Offset: 0x0003FD98
		public void visit(_ICaseStatement casest)
		{
			if (!this.CompareCommon(casest, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_ICaseStatement icaseStatement = this.TopOfStack.CurrentExprement as _ICaseStatement;
			if (icaseStatement == null)
			{
				return;
			}
			this.Push(icaseStatement._Switch);
			casest._Switch.Accept(this);
			this.Pop();
			IList<_ICase> cases = casest._Cases;
			IList<_ICase> cases2 = icaseStatement._Cases;
			if (cases.Count != cases2.Count)
			{
				this.CodeEqual = false;
			}
			else
			{
				for (int i = 0; i < cases.Count; i++)
				{
					this.Push(cases2[i]._Label);
					cases[i]._Label.Accept(this);
					this.Pop();
					this.Push(cases2[i]._Controlled);
					cases[i]._Controlled.Accept(this);
					this.Pop();
				}
			}
			if ((casest._Else == null && icaseStatement._Else != null) || (casest._Else != null && icaseStatement._Else == null))
			{
				this.CodeEqual = false;
				return;
			}
			if (casest._Else != null)
			{
				this.Push(icaseStatement._Else);
				casest._Else.Accept(this);
				this.Pop();
			}
		}

		// Token: 0x06001763 RID: 5987 RVA: 0x0003FD53 File Offset: 0x0003ED53
		public void visit(_IErrorExpression errorexp)
		{
			this.CompareCommon(errorexp, this.TopOfStack.CurrentExprement);
		}

		// Token: 0x06001764 RID: 5988 RVA: 0x0003FD53 File Offset: 0x0003ED53
		public void visit(_IErrorStatement errorst)
		{
			this.CompareCommon(errorst, this.TopOfStack.CurrentExprement);
		}

		// Token: 0x06001765 RID: 5989 RVA: 0x0003FD53 File Offset: 0x0003ED53
		public void visit(_INullExpression errorexp)
		{
			this.CompareCommon(errorexp, this.TopOfStack.CurrentExprement);
		}

		// Token: 0x06001766 RID: 5990 RVA: 0x0003FD53 File Offset: 0x0003ED53
		public void visit(_INullStatement errorst)
		{
			this.CompareCommon(errorst, this.TopOfStack.CurrentExprement);
		}

		// Token: 0x06001767 RID: 5991 RVA: 0x00040EC8 File Offset: 0x0003FEC8
		public void visit(_IMultipleIndexInitialization errorst)
		{
			if (!this.CompareCommon(errorst, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IMultipleIndexInitialization imultipleIndexInitialization = this.TopOfStack.CurrentExprement as _IMultipleIndexInitialization;
			if (imultipleIndexInitialization == null)
			{
				return;
			}
			this.Push(imultipleIndexInitialization._Number);
			errorst._Number.Accept(this);
			this.Pop();
			this.Push(imultipleIndexInitialization._Value);
			errorst._Value.Accept(this);
			this.Pop();
		}

		// Token: 0x06001768 RID: 5992 RVA: 0x00040F3C File Offset: 0x0003FF3C
		public void visit(_IArrayInitialization errorexp)
		{
			if (!this.CompareCommon(errorexp, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IArrayInitialization iarrayInitialization = this.TopOfStack.CurrentExprement as _IArrayInitialization;
			if (iarrayInitialization == null)
			{
				return;
			}
			IList<_IExpression> initValues = errorexp._InitValues;
			IList<_IExpression> initValues2 = iarrayInitialization._InitValues;
			if (initValues.Count != initValues2.Count)
			{
				this.CodeEqual = false;
				return;
			}
			for (int i = 0; i < initValues.Count; i++)
			{
				this.Push(initValues2[i]);
				initValues[i].Accept(this);
				this.Pop();
			}
		}

		// Token: 0x06001769 RID: 5993 RVA: 0x00040FC8 File Offset: 0x0003FFC8
		public void visit(_IStructureInitialization errorst)
		{
			if (!this.CompareCommon(errorst, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IStructureInitialization istructureInitialization = this.TopOfStack.CurrentExprement as _IStructureInitialization;
			if (istructureInitialization == null)
			{
				return;
			}
			IList<_IAssignmentExpression> compoInits = errorst._CompoInits;
			IList<_IAssignmentExpression> compoInits2 = istructureInitialization._CompoInits;
			if (compoInits.Count != compoInits2.Count)
			{
				this.CodeEqual = false;
				return;
			}
			for (int i = 0; i < compoInits.Count; i++)
			{
				this.Push(compoInits2[i]);
				compoInits[i].Accept(this);
				this.Pop();
			}
		}

		// Token: 0x0600176A RID: 5994 RVA: 0x00041054 File Offset: 0x00040054
		public void visit(_IDefineReference defref)
		{
			if (!this.CompareCommon(defref, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IDefineReference idefineReference = this.TopOfStack.CurrentExprement as _IDefineReference;
			if (idefineReference == null)
			{
				return;
			}
			if (defref.Define != idefineReference.Define)
			{
				this.CodeEqual = false;
			}
		}

		// Token: 0x0600176B RID: 5995 RVA: 0x000410A8 File Offset: 0x000400A8
		public void visit(_IVariableReference varref)
		{
			if (!this.CompareCommon(varref, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IVariableReference ivariableReference = this.TopOfStack.CurrentExprement as _IVariableReference;
			if (ivariableReference == null)
			{
				return;
			}
			this.Push(ivariableReference.InstancePath);
			varref.InstancePath.Accept(this);
			this.Pop();
		}

		// Token: 0x0600176C RID: 5996 RVA: 0x00041100 File Offset: 0x00040100
		public void visit(_ITypeReference typeref)
		{
			if (!this.CompareCommon(typeref, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_ITypeReference itypeReference = this.TopOfStack.CurrentExprement as _ITypeReference;
			if (itypeReference == null)
			{
				return;
			}
			this.Push(itypeReference.InstancePath);
			typeref.InstancePath.Accept(this);
			this.Pop();
		}

		// Token: 0x0600176D RID: 5997 RVA: 0x00041158 File Offset: 0x00040158
		public void visit(_IPouReference pouref)
		{
			if (!this.CompareCommon(pouref, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IPouReference ipouReference = this.TopOfStack.CurrentExprement as _IPouReference;
			if (ipouReference == null)
			{
				return;
			}
			this.Push(ipouReference.InstancePath);
			pouref.InstancePath.Accept(this);
			this.Pop();
		}

		// Token: 0x0600176E RID: 5998 RVA: 0x000411B0 File Offset: 0x000401B0
		public void visit(_ITaskReference taskref)
		{
			if (!this.CompareCommon(taskref, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_ITaskReference itaskReference = this.TopOfStack.CurrentExprement as _ITaskReference;
			if (itaskReference == null)
			{
				return;
			}
			if (taskref.TaskName != itaskReference.TaskName)
			{
				this.CodeEqual = false;
			}
		}

		// Token: 0x0600176F RID: 5999 RVA: 0x00041204 File Offset: 0x00040204
		public void visit(_IResourceReference resref)
		{
			if (!this.CompareCommon(resref, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IResourceReference iresourceReference = this.TopOfStack.CurrentExprement as _IResourceReference;
			if (iresourceReference == null)
			{
				return;
			}
			if (resref.ResourceName != iresourceReference.ResourceName)
			{
				this.CodeEqual = false;
			}
		}

		// Token: 0x06001770 RID: 6000 RVA: 0x00041258 File Offset: 0x00040258
		public void visit(_IDefinedExpression defexp)
		{
			if (!this.CompareCommon(defexp, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IDefinedExpression idefinedExpression = this.TopOfStack.CurrentExprement as _IDefinedExpression;
			if (idefinedExpression == null)
			{
				return;
			}
			this.Push(idefinedExpression.ItemReference);
			defexp.ItemReference.Accept(this);
			this.Pop();
		}

		// Token: 0x06001771 RID: 6001 RVA: 0x000412B0 File Offset: 0x000402B0
		public void visit(_IPragmaOperatorExpression popexp)
		{
			if (!this.CompareCommon(popexp, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IPragmaOperatorExpression ipragmaOperatorExpression = this.TopOfStack.CurrentExprement as _IPragmaOperatorExpression;
			if (ipragmaOperatorExpression == null)
			{
				return;
			}
			if (popexp.Code != ipragmaOperatorExpression.Code)
			{
				this.CodeEqual = false;
				return;
			}
			IList<_IExpression> operands = popexp.Operands;
			IList<_IExpression> operands2 = ipragmaOperatorExpression.Operands;
			if (operands.Count != operands2.Count)
			{
				this.CodeEqual = false;
				return;
			}
			for (int i = 0; i < operands.Count; i++)
			{
				this.Push(operands2[i]);
				operands[i].Accept(this);
				this.Pop();
			}
		}

		// Token: 0x06001772 RID: 6002 RVA: 0x00041354 File Offset: 0x00040354
		public void visit(_IPragmaAssertion assertion)
		{
			if (!this.CompareCommon(assertion, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IPragmaAssertion ipragmaAssertion = this.TopOfStack.CurrentExprement as _IPragmaAssertion;
			if (ipragmaAssertion == null)
			{
				return;
			}
			this.Push(ipragmaAssertion.Condition);
			assertion.Condition.Accept(this);
			this.Pop();
		}

		// Token: 0x06001773 RID: 6003 RVA: 0x000413AC File Offset: 0x000403AC
		public void visit(_ICompilerVersionExpression compiversionexp)
		{
			if (!this.CompareCommon(compiversionexp, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_ICompilerVersionExpression icompilerVersionExpression = this.TopOfStack.CurrentExprement as _ICompilerVersionExpression;
			if (icompilerVersionExpression == null)
			{
				return;
			}
			if (!(compiversionexp.VersionToTest == icompilerVersionExpression.VersionToTest) || compiversionexp.OpComparison != icompilerVersionExpression.OpComparison)
			{
				this.CodeEqual = false;
			}
		}

		// Token: 0x06001774 RID: 6004 RVA: 0x0004140C File Offset: 0x0004040C
		public void visit(_IProjectDefinedExpression projectDefinedExpression)
		{
			if (!this.CompareCommon(projectDefinedExpression, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IProjectDefinedExpression iprojectDefinedExpression = this.TopOfStack.CurrentExprement as _IProjectDefinedExpression;
			if (iprojectDefinedExpression == null)
			{
				return;
			}
			if (projectDefinedExpression.DefineReference == null)
			{
				return;
			}
			this.Push(iprojectDefinedExpression.DefineReference);
			projectDefinedExpression.DefineReference.Accept(this);
			this.Pop();
		}

		// Token: 0x06001775 RID: 6005 RVA: 0x0004146C File Offset: 0x0004046C
		public void visit(_IPragmaIfStatement pifst)
		{
			if (!this.CompareCommon(pifst, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IPragmaIfStatement ipragmaIfStatement = this.TopOfStack.CurrentExprement as _IPragmaIfStatement;
			if (ipragmaIfStatement == null)
			{
				return;
			}
			this.Push(ipragmaIfStatement.Condition);
			pifst.Condition.Accept(this);
			this.Pop();
			this.Push(ipragmaIfStatement.IfThen);
			pifst.IfThen.Accept(this);
			this.Pop();
			if (pifst.ElseIf != null && ipragmaIfStatement.ElseIf != null)
			{
				IList<_IPragmaElseIf> elseIf = pifst.ElseIf;
				IList<_IPragmaElseIf> elseIf2 = ipragmaIfStatement.ElseIf;
				if (elseIf.Count != elseIf2.Count)
				{
					this.CodeEqual = false;
				}
				else
				{
					for (int i = 0; i < elseIf.Count; i++)
					{
						this.Push(elseIf2[i].Condition);
						elseIf[i].Condition.Accept(this);
						this.Pop();
						this.Push(elseIf2[i].Controlled);
						elseIf[i].Controlled.Accept(this);
						this.Pop();
					}
				}
			}
			else if (pifst.ElseIf != null || ipragmaIfStatement.ElseIf != null)
			{
				this.CodeEqual = false;
			}
			if (pifst.IfElse != null && ipragmaIfStatement.IfElse != null)
			{
				this.Push(ipragmaIfStatement.IfElse);
				pifst.IfElse.Accept(this);
				this.Pop();
				return;
			}
			if (pifst.IfElse != null || ipragmaIfStatement.IfElse != null)
			{
				this.CodeEqual = false;
			}
		}

		// Token: 0x06001776 RID: 6006 RVA: 0x000415E0 File Offset: 0x000405E0
		public void visit(_IBreakPointStatement bpstate)
		{
			if (!this.CompareCommon(bpstate, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IBreakPointStatement ibreakPointStatement = this.TopOfStack.CurrentExprement as _IBreakPointStatement;
			if (ibreakPointStatement == null)
			{
				return;
			}
			if (bpstate.SuccessorPosition != ibreakPointStatement.SuccessorPosition || bpstate.BPPosition != ibreakPointStatement.BPPosition)
			{
				this.m_bPositionsEqual = false;
			}
		}

		// Token: 0x06001777 RID: 6007 RVA: 0x0004163C File Offset: 0x0004063C
		public void visit(_IDefineStatement defstate)
		{
			if (!this.CompareCommon(defstate, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IDefineStatement idefineStatement = this.TopOfStack.CurrentExprement as _IDefineStatement;
			if (idefineStatement == null)
			{
				return;
			}
			if (defstate.Ident != idefineStatement.Ident || defstate.Define != idefineStatement.Define || defstate.Value != idefineStatement.Value)
			{
				this.CodeEqual = false;
			}
		}

		// Token: 0x06001778 RID: 6008 RVA: 0x000416B0 File Offset: 0x000406B0
		public void visit(_IXRefExpression xref)
		{
			if (!this.CompareCommon(xref, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IXRefExpression ixrefExpression = this.TopOfStack.CurrentExprement as _IXRefExpression;
			if (ixrefExpression == null)
			{
				return;
			}
			this.Push(ixrefExpression.XRef);
			xref.XRef.Accept(this);
			this.Pop();
			if (xref.XRefFrom != null && ixrefExpression.XRefFrom != null)
			{
				this.Push(ixrefExpression.XRefFrom);
				xref.XRefFrom.Accept(this);
				this.Pop();
				return;
			}
			if (xref.XRefFrom != null || ixrefExpression.XRefFrom != null)
			{
				this.CodeEqual = false;
			}
		}

		// Token: 0x06001779 RID: 6009 RVA: 0x0004174C File Offset: 0x0004074C
		public void visit(_IHasTypeExpression hastype)
		{
			if (!this.CompareCommon(hastype, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IHasTypeExpression ihasTypeExpression = this.TopOfStack.CurrentExprement as _IHasTypeExpression;
			if (ihasTypeExpression == null)
			{
				return;
			}
			if (hastype.Exact != ihasTypeExpression.Exact)
			{
				return;
			}
			this.Push(ihasTypeExpression.Variable);
			hastype.Variable.Accept(this);
			this.Pop();
			if (hastype.ReferencedType == null)
			{
				if (ihasTypeExpression.ReferencedType != null)
				{
					this.CodeEqual = false;
					return;
				}
			}
			else if (!hastype.ReferencedType.IsEqual(ihasTypeExpression.ReferencedType))
			{
				this.CodeEqual = false;
			}
		}

		// Token: 0x0600177A RID: 6010 RVA: 0x000417E4 File Offset: 0x000407E4
		public void visit(_IIsEnumTypeExpression isenumtype)
		{
			if (!this.CompareCommon(isenumtype, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IIsEnumTypeExpression iisEnumTypeExpression = this.TopOfStack.CurrentExprement as _IIsEnumTypeExpression;
			if (iisEnumTypeExpression == null)
			{
				return;
			}
			if (isenumtype.ReferencedType == null)
			{
				if (iisEnumTypeExpression.ReferencedType != null)
				{
					this.CodeEqual = false;
					return;
				}
			}
			else if (!isenumtype.ReferencedType.IsEqual(iisEnumTypeExpression.ReferencedType))
			{
				this.CodeEqual = false;
			}
		}

		// Token: 0x0600177B RID: 6011 RVA: 0x00041850 File Offset: 0x00040850
		public void visit(_IHasAttributeExpression hasattribute)
		{
			if (!this.CompareCommon(hasattribute, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IHasAttributeExpression ihasAttributeExpression = this.TopOfStack.CurrentExprement as _IHasAttributeExpression;
			if (ihasAttributeExpression == null)
			{
				return;
			}
			this.Push(ihasAttributeExpression.ItemReference);
			hasattribute.ItemReference.Accept(this);
			this.Pop();
			if (hasattribute.Attribute != ihasAttributeExpression.Attribute)
			{
				this.CodeEqual = false;
			}
		}

		// Token: 0x0600177C RID: 6012 RVA: 0x000418C0 File Offset: 0x000408C0
		public void visit(_IHasValueExpression hasvalue)
		{
			if (!this.CompareCommon(hasvalue, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IHasValueExpression ihasValueExpression = this.TopOfStack.CurrentExprement as _IHasValueExpression;
			if (ihasValueExpression == null)
			{
				return;
			}
			if (hasvalue.Define != ihasValueExpression.Define || hasvalue.DefineValue != ihasValueExpression.DefineValue)
			{
				this.CodeEqual = false;
			}
		}

		// Token: 0x0600177D RID: 6013 RVA: 0x00041924 File Offset: 0x00040924
		public void visit(_IHasConstantValueExpression hasvalue)
		{
			if (!this.CompareCommon(hasvalue, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IHasConstantValueExpression ihasConstantValueExpression = this.TopOfStack.CurrentExprement as _IHasConstantValueExpression;
			if (ihasConstantValueExpression == null)
			{
				return;
			}
			if (ihasConstantValueExpression._Constant != null)
			{
				this.Push(ihasConstantValueExpression._Constant);
				ihasConstantValueExpression._Constant.Accept(this);
				this.Pop();
			}
			if (ihasConstantValueExpression._ConstantValue != null)
			{
				this.Push(ihasConstantValueExpression._ConstantValue);
				ihasConstantValueExpression._ConstantValue.Accept(this);
				this.Pop();
			}
			if (hasvalue.OpComparison != ihasConstantValueExpression._OpComparison)
			{
				this.CodeEqual = false;
			}
		}

		// Token: 0x0600177E RID: 6014 RVA: 0x000419BC File Offset: 0x000409BC
		public void visit(_IHasConstantTypeExpression hasConstantTypeExpression)
		{
			if (!this.CompareCommon(hasConstantTypeExpression, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IHasConstantTypeExpression ihasConstantTypeExpression = this.TopOfStack.CurrentExprement as _IHasConstantTypeExpression;
			if (ihasConstantTypeExpression == null)
			{
				return;
			}
			if (ihasConstantTypeExpression._Constant != null)
			{
				this.Push(ihasConstantTypeExpression._Constant);
				ihasConstantTypeExpression._Constant.Accept(this);
				this.Pop();
			}
			if (hasConstantTypeExpression._ConstantTypeReplaced != ihasConstantTypeExpression._ConstantTypeReplaced)
			{
				this.CodeEqual = false;
			}
		}

		// Token: 0x0600177F RID: 6015 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_ICurrentTaskExpression currentTask)
		{
		}

		// Token: 0x06001780 RID: 6016 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IRuntimeVersionExpression runtimeversionexp)
		{
		}

		// Token: 0x06001781 RID: 6017 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_INamespaceAccessExpression namespaceaccess)
		{
		}

		// Token: 0x06001782 RID: 6018 RVA: 0x00041A30 File Offset: 0x00040A30
		public void visit(_ITryCatchStatement trycatchstatement)
		{
			if (!this.CompareCommon(trycatchstatement, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_ITryCatchStatement itryCatchStatement = this.TopOfStack.CurrentExprement as _ITryCatchStatement;
			if (trycatchstatement._Try != null)
			{
				this.Push(itryCatchStatement._Try);
				trycatchstatement._Try.Accept(this);
				this.Pop();
			}
			if (trycatchstatement._Exception != null)
			{
				this.Push(itryCatchStatement._Exception);
				trycatchstatement._Exception.Accept(this);
				this.Pop();
			}
			if (trycatchstatement._Catch != null)
			{
				this.Push(itryCatchStatement._Catch);
				trycatchstatement._Catch.Accept(this);
				this.Pop();
			}
			if (trycatchstatement._Finally != null)
			{
				this.Push(itryCatchStatement._Finally);
				trycatchstatement._Finally.Accept(this);
				this.Pop();
			}
		}

		// Token: 0x06001783 RID: 6019 RVA: 0x00041AFC File Offset: 0x00040AFC
		public void visit(_IPartialAccessExpression partialAccessExpression)
		{
			if (!this.CompareCommon(partialAccessExpression, this.TopOfStack.CurrentExprement))
			{
				return;
			}
			_IPartialAccessExpression ipartialAccessExpression = (_IPartialAccessExpression)this.TopOfStack.CurrentExprement;
			if (partialAccessExpression.PartSize != ipartialAccessExpression.PartSize || partialAccessExpression.PartOffset != ipartialAccessExpression.PartOffset)
			{
				this.CodeEqual = false;
				return;
			}
			this.Push(ipartialAccessExpression._Left);
			partialAccessExpression._Left.Accept(this);
			this.Pop();
		}

		// Token: 0x040004E3 RID: 1251
		private readonly _ICompiledPOU m_cpouToCompareWith;

		// Token: 0x040004E4 RID: 1252
		private bool m_bCommentsEqual = true;

		// Token: 0x040004E5 RID: 1253
		private bool m_bPositionsEqual = true;

		// Token: 0x040004E6 RID: 1254
		private readonly bool m_bCreatePositionTable;

		// Token: 0x040004E7 RID: 1255
		private readonly bool m_bCopyMessages;

		// Token: 0x040004E8 RID: 1256
		private readonly LDictionary<IMinimalPosition, IMinimalPosition> _positiontable = new LDictionary<IMinimalPosition, IMinimalPosition>();

		// Token: 0x040004E9 RID: 1257
		private readonly Stack m_stackAttributes = new Stack();
	}
}
