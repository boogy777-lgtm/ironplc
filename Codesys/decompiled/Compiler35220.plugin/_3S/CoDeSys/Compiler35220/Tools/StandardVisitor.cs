using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.Tools
{
	// Token: 0x02000057 RID: 87
	public class StandardVisitor : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor
	{
		// Token: 0x17000352 RID: 850
		// (get) Token: 0x06000617 RID: 1559 RVA: 0x0000CD74 File Offset: 0x0000AF74
		protected LStack<StandardVisitor.StackContent> Stack { get; } = new LStack<StandardVisitor.StackContent>();

		// Token: 0x06000618 RID: 1560 RVA: 0x0000CD7C File Offset: 0x0000AF7C
		public StandardVisitor(AccessFlag access = AccessFlag.None)
		{
			this.Push(access);
		}

		// Token: 0x06000619 RID: 1561 RVA: 0x0000CD98 File Offset: 0x0000AF98
		protected void Push(AccessFlag access)
		{
			this.Stack.Push(new StandardVisitor.StackContent(access));
		}

		// Token: 0x17000353 RID: 851
		// (get) Token: 0x0600061A RID: 1562 RVA: 0x0000CDAC File Offset: 0x0000AFAC
		protected StandardVisitor.StackContent TopOfStack
		{
			get
			{
				return this.Stack.Peek();
			}
		}

		// Token: 0x0600061B RID: 1563 RVA: 0x0000CDBC File Offset: 0x0000AFBC
		protected void Pop()
		{
			this.Stack.Pop();
		}

		// Token: 0x0600061C RID: 1564 RVA: 0x0000CDCC File Offset: 0x0000AFCC
		public virtual void visit(_ICompiledPOU cpou)
		{
			cpou.GetParseTree().Accept(this);
		}

		// Token: 0x0600061D RID: 1565 RVA: 0x0000CDDC File Offset: 0x0000AFDC
		public virtual void visit(_IWhileStatement whilst)
		{
			this.Push(AccessFlag.Read);
			whilst._Condition.Accept(this);
			this.Pop();
			whilst._Controlled.Accept(this);
		}

		// Token: 0x0600061E RID: 1566 RVA: 0x0000CE04 File Offset: 0x0000B004
		public virtual void visit(_IRepeatStatement repeat)
		{
			this.Push(AccessFlag.Read);
			repeat._Condition.Accept(this);
			this.Pop();
			repeat._Controlled.Accept(this);
		}

		// Token: 0x0600061F RID: 1567 RVA: 0x0000CE2C File Offset: 0x0000B02C
		public virtual void visit(_IForStatement forloop)
		{
			forloop._CounterStart.Accept(this);
			this.Push(AccessFlag.Read);
			forloop._UpperBound.Accept(this);
			this.Pop();
			if (forloop.By != null)
			{
				this.Push(AccessFlag.Read);
				forloop._By.Accept(this);
				this.Pop();
			}
			forloop._Controlled.Accept(this);
		}

		// Token: 0x06000620 RID: 1568 RVA: 0x0000CE8C File Offset: 0x0000B08C
		public virtual void visit(_IExitStatement exit)
		{
		}

		// Token: 0x06000621 RID: 1569 RVA: 0x0000CE90 File Offset: 0x0000B090
		public virtual void visit(_IContinueStatement cont)
		{
		}

		// Token: 0x06000622 RID: 1570 RVA: 0x0000CE94 File Offset: 0x0000B094
		public virtual void visit(_ISequenceStatement seq)
		{
			for (int i = 0; i < seq._StatementList.Count; i++)
			{
				seq._StatementList[i].Accept(this);
			}
		}

		// Token: 0x06000623 RID: 1571 RVA: 0x0000CECC File Offset: 0x0000B0CC
		public virtual void visit(_IAssignmentExpression assign)
		{
			this.Push(AccessFlag.Write);
			assign._LValue.Accept(this);
			bool varInOut = this.TopOfStack.VarInOut;
			this.Pop();
			AccessFlag access;
			if (assign.KindOf == Operator.RefAssign || (assign.LValue.Type != null && assign.LValue.Type.Class == TypeClass.Reference))
			{
				access = AccessFlag.Write;
			}
			else
			{
				access = AccessFlag.Read;
			}
			if (varInOut && (assign._LValue is ICompoAccessExpression || assign._LValue is IVariableExpression))
			{
				access = (AccessFlag.Read | AccessFlag.Write);
			}
			this.Push(access);
			assign._RValue.Accept(this);
			this.Pop();
		}

		// Token: 0x06000624 RID: 1572 RVA: 0x0000CF6C File Offset: 0x0000B16C
		public virtual void visit(_IIfStatement ifst)
		{
			this.Push(AccessFlag.Read);
			ifst._Condition.Accept(this);
			this.Pop();
			ifst._IfThen.Accept(this);
			foreach (_IElseIf ielseIf in ifst._ElseIf)
			{
				this.Push(AccessFlag.Read);
				ielseIf._Condition.Accept(this);
				this.Pop();
				ielseIf._Controlled.Accept(this);
			}
			_IStatement ifElse = ifst._IfElse;
			if (ifElse == null)
			{
				return;
			}
			ifElse.Accept(this);
		}

		// Token: 0x06000625 RID: 1573 RVA: 0x0000D00C File Offset: 0x0000B20C
		public virtual void visit(_IReturnStatement returnst)
		{
			if (returnst._Condition != null)
			{
				this.Push(AccessFlag.Read);
				returnst._Condition.Accept(this);
				this.Pop();
			}
		}

		// Token: 0x06000626 RID: 1574 RVA: 0x0000D030 File Offset: 0x0000B230
		public virtual void visit(_IJumpStatement gotost)
		{
			if (gotost._Condition != null)
			{
				this.Push(AccessFlag.Read);
				gotost._Condition.Accept(this);
				this.Pop();
			}
		}

		// Token: 0x06000627 RID: 1575 RVA: 0x0000D054 File Offset: 0x0000B254
		public virtual void visit(_ILabelStatement label)
		{
		}

		// Token: 0x06000628 RID: 1576 RVA: 0x0000D058 File Offset: 0x0000B258
		public virtual void visit(_ICommentStatement comment)
		{
		}

		// Token: 0x06000629 RID: 1577 RVA: 0x0000D05C File Offset: 0x0000B25C
		public virtual void visit(_IPragmaStatement pragma)
		{
		}

		// Token: 0x0600062A RID: 1578 RVA: 0x0000D060 File Offset: 0x0000B260
		public virtual void visit(_IExpressionStatement expstat)
		{
			this.Push(AccessFlag.Read);
			expstat._Expr.Accept(this);
			this.Pop();
		}

		// Token: 0x0600062B RID: 1579 RVA: 0x0000D07C File Offset: 0x0000B27C
		public virtual void visit(_ICallExpression call)
		{
			this.Push(AccessFlag.Call);
			call._Callee.Accept(this);
			this.Pop();
			if (call._Condition != null)
			{
				this.Push(AccessFlag.Read);
				call._Condition.Accept(this);
				this.Pop();
			}
			LHashSet<int> lhashSet = new LHashSet<int>();
			if (call.InputAssigns != null)
			{
				for (int i = 0; i < call.InputAssigns.Length; i++)
				{
					_IAssignmentExpression iassignmentExpression = call.InputAssigns[i] as _IAssignmentExpression;
					if (iassignmentExpression != null)
					{
						if (iassignmentExpression.LValue is INullExpression)
						{
							bool flag = StandardTraverser.IsImplicitReferenceAssignment(call, iassignmentExpression, i);
							this.TopOfStack.VarInOut = true;
							if (flag)
							{
								this.Push(AccessFlag.Read | AccessFlag.Write);
							}
							else
							{
								this.Push(AccessFlag.Read);
							}
							iassignmentExpression._RValue.Accept(this);
							this.Pop();
						}
						else
						{
							this.Push(AccessFlag.Read);
							iassignmentExpression.Accept(this);
							if (iassignmentExpression._LValue != null && iassignmentExpression._LValue is _IVariableExpression)
							{
								_IVariableExpression ivariableExpression = iassignmentExpression._LValue as _IVariableExpression;
								lhashSet.Add(ivariableExpression.PrecompileVariableId);
							}
							this.Pop();
						}
					}
				}
			}
			else
			{
				foreach (_IExpression iexpression in call.ParamExpressions)
				{
					if (iexpression != null)
					{
						this.Push(AccessFlag.Read);
						iexpression.Accept(this);
						this.Pop();
					}
				}
			}
			foreach (_IExpression iexpression2 in call.OutputExpressions)
			{
				if (iexpression2 != null)
				{
					this.Push(AccessFlag.Write);
					iexpression2.Accept(this);
					this.Pop();
				}
			}
			foreach (_IExpression iexpression3 in call.Inputs)
			{
				_IVariableExpression ivariableExpression2 = iexpression3 as _IVariableExpression;
				if ((ivariableExpression2 == null || !lhashSet.Contains(ivariableExpression2.PrecompileVariableId)) && iexpression3 != null)
				{
					this.Push(AccessFlag.Write);
					iexpression3.Accept(this);
					this.Pop();
				}
			}
			foreach (_IExpression iexpression4 in call.Outputs)
			{
				if (iexpression4 != null)
				{
					this.Push(AccessFlag.Read);
					iexpression4.Accept(this);
					this.Pop();
				}
			}
		}

		// Token: 0x0600062C RID: 1580 RVA: 0x0000D2FC File Offset: 0x0000B4FC
		public virtual void visit(_IOperatorExpression op)
		{
			foreach (_IExprement iexprement in op._OperandsList)
			{
				if (op.Code == Operator.Adr || op.Code == Operator.__RefAdr)
				{
					this.TopOfStack.Access = (AccessFlag.Write | AccessFlag.Address);
				}
				iexprement.Accept(this);
			}
		}

		// Token: 0x0600062D RID: 1581 RVA: 0x0000D36C File Offset: 0x0000B56C
		public virtual void visit(_IConversionExpression conv)
		{
			conv._Exp.Accept(this);
		}

		// Token: 0x0600062E RID: 1582 RVA: 0x0000D37C File Offset: 0x0000B57C
		public virtual void visit(_IThisExpression thisexp)
		{
		}

		// Token: 0x0600062F RID: 1583 RVA: 0x0000D380 File Offset: 0x0000B580
		public virtual void visit(_IBaseExpression baseexp)
		{
		}

		// Token: 0x06000630 RID: 1584 RVA: 0x0000D384 File Offset: 0x0000B584
		public virtual void visit(_ILiteralExpression literal)
		{
		}

		// Token: 0x06000631 RID: 1585 RVA: 0x0000D388 File Offset: 0x0000B588
		public virtual void visit(_IAddressExpression address)
		{
		}

		// Token: 0x06000632 RID: 1586 RVA: 0x0000D38C File Offset: 0x0000B58C
		public virtual void visit(_IVariableExpression variable)
		{
			ISignature signatureForPrecompileID = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(variable.PrecompileSignatureId);
			if (signatureForPrecompileID != null)
			{
				IVariable variable2 = signatureForPrecompileID[variable.PrecompileVariableId];
				if (variable2 != null)
				{
					this.TopOfStack.VarInOut = variable2.HasFlag(VarFlag.Inout);
				}
			}
		}

		// Token: 0x06000633 RID: 1587 RVA: 0x0000D3D8 File Offset: 0x0000B5D8
		public virtual void visit(_IIndexAccessExpression indexaccess)
		{
			this.Push(AccessFlag.Read);
			for (int i = 0; i < indexaccess.NumAccesses; i++)
			{
				indexaccess.GetAccess(i).Accept(this);
			}
			this.Pop();
			indexaccess._Var.Accept(this);
		}

		// Token: 0x06000634 RID: 1588 RVA: 0x0000D41C File Offset: 0x0000B61C
		public virtual void visit(_ICompoAccessExpression compo)
		{
		}

		// Token: 0x06000635 RID: 1589 RVA: 0x0000D420 File Offset: 0x0000B620
		public virtual void visit(_IDeRefAccessExpression deref)
		{
			deref._Base.Accept(this);
		}

		// Token: 0x06000636 RID: 1590 RVA: 0x0000D430 File Offset: 0x0000B630
		public virtual void visit(_ICopyScopeExpression copyexp)
		{
			copyexp._Base.Accept(this);
		}

		// Token: 0x06000637 RID: 1591 RVA: 0x0000D440 File Offset: 0x0000B640
		public virtual void visit(_IGlobalScopeExpression globexp)
		{
			globexp._Base.Accept(this);
		}

		// Token: 0x06000638 RID: 1592 RVA: 0x0000D450 File Offset: 0x0000B650
		public virtual void visit(_ISystemScopeExpression systemscope)
		{
			systemscope._Base.Accept(this);
		}

		// Token: 0x06000639 RID: 1593 RVA: 0x0000D460 File Offset: 0x0000B660
		public virtual void visit(_IEmptyStatement empty)
		{
		}

		// Token: 0x0600063A RID: 1594 RVA: 0x0000D464 File Offset: 0x0000B664
		public virtual void visit(_ICaseRangeExpression caserange)
		{
			caserange._Low.Accept(this);
			caserange._High.Accept(this);
		}

		// Token: 0x0600063B RID: 1595 RVA: 0x0000D480 File Offset: 0x0000B680
		public virtual void visit(_ICaseLabelStatement caselabel)
		{
			foreach (_IExpression iexpression in caselabel._cases)
			{
				iexpression.Accept(this);
			}
		}

		// Token: 0x0600063C RID: 1596 RVA: 0x0000D4CC File Offset: 0x0000B6CC
		public virtual void visit(_ICaseStatement casest)
		{
			this.Push(AccessFlag.Read);
			casest._Switch.Accept(this);
			this.Pop();
			foreach (_ICase icase in casest._Cases)
			{
				this.Push(AccessFlag.Read);
				icase._Label.Accept(this);
				this.Pop();
				icase._Controlled.Accept(this);
			}
			if (casest._Else != null)
			{
				casest._Else.Accept(this);
			}
		}

		// Token: 0x0600063D RID: 1597 RVA: 0x0000D564 File Offset: 0x0000B764
		public virtual void visit(_IErrorExpression errorexp)
		{
		}

		// Token: 0x0600063E RID: 1598 RVA: 0x0000D568 File Offset: 0x0000B768
		public virtual void visit(_IErrorStatement errorst)
		{
		}

		// Token: 0x0600063F RID: 1599 RVA: 0x0000D56C File Offset: 0x0000B76C
		public virtual void visit(_INullExpression nullexp)
		{
		}

		// Token: 0x06000640 RID: 1600 RVA: 0x0000D570 File Offset: 0x0000B770
		public virtual void visit(_INullStatement nullstmt)
		{
		}

		// Token: 0x06000641 RID: 1601 RVA: 0x0000D574 File Offset: 0x0000B774
		public virtual void visit(_IQualifiedNameExpression qne)
		{
		}

		// Token: 0x06000642 RID: 1602 RVA: 0x0000D578 File Offset: 0x0000B778
		public virtual void visit(_IVariableDeclarationStatement vds)
		{
			foreach (_IExpression iexpression in vds.NameList)
			{
				iexpression.Accept(this);
			}
			if (vds.Initial != null)
			{
				vds.Initial.Accept(this);
			}
			if (vds.InputAssigns != null)
			{
				foreach (_IAssignmentExpression iassignmentExpression in vds.InputAssigns)
				{
					iassignmentExpression.Accept(this);
				}
			}
		}

		// Token: 0x06000643 RID: 1603 RVA: 0x0000D61C File Offset: 0x0000B81C
		public virtual void visit(_IVariableDeclarationListStatement vdls)
		{
			vdls.VariableDeclaration.Accept(this);
		}

		// Token: 0x06000644 RID: 1604 RVA: 0x0000D62C File Offset: 0x0000B82C
		public virtual void visit(_IPOUDeclarationStatement pds)
		{
			pds.Declarations.Accept(this);
			if (pds.Implements != null)
			{
				foreach (_IExpression iexpression in pds.Implements)
				{
					iexpression.Accept(this);
				}
			}
			if (pds.Extends != null)
			{
				foreach (_IExpression iexpression2 in pds.Extends)
				{
					iexpression2.Accept(this);
				}
			}
		}

		// Token: 0x06000645 RID: 1605 RVA: 0x0000D6D0 File Offset: 0x0000B8D0
		public virtual void visit(_ITypeDeclarationStatement tds)
		{
			tds.Declarations.Accept(this);
			if (tds.Initial != null)
			{
				tds.Initial.Accept(this);
			}
			if (tds.Extends != null)
			{
				tds.Extends.Accept(this);
			}
		}

		// Token: 0x06000646 RID: 1606 RVA: 0x0000D708 File Offset: 0x0000B908
		public virtual void visit(_IEnumDeclarationStatement eds)
		{
			if (eds._Value != null)
			{
				eds._Value.Accept(this);
			}
		}

		// Token: 0x06000647 RID: 1607 RVA: 0x0000D720 File Offset: 0x0000B920
		public virtual void visit(_IEnumDeclarationListStatement eds)
		{
			foreach (_IEnumDeclarationStatement ienumDeclarationStatement in eds.Enums)
			{
				ienumDeclarationStatement.Accept(this);
			}
		}

		// Token: 0x06000648 RID: 1608 RVA: 0x0000D76C File Offset: 0x0000B96C
		public virtual void visit(_IMultipleIndexInitialization errorst)
		{
			errorst._Value.Accept(this);
			errorst._Number.Accept(this);
		}

		// Token: 0x06000649 RID: 1609 RVA: 0x0000D788 File Offset: 0x0000B988
		public virtual void visit(_IArrayInitialization errorexp)
		{
			foreach (_IExpression iexpression in errorexp._InitValues)
			{
				iexpression.Accept(this);
			}
		}

		// Token: 0x0600064A RID: 1610 RVA: 0x0000D7D4 File Offset: 0x0000B9D4
		public virtual void visit(_IStructureInitialization errorst)
		{
			foreach (_IAssignmentExpression iassignmentExpression in errorst._CompoInits)
			{
				iassignmentExpression.Accept(this);
			}
		}

		// Token: 0x0600064B RID: 1611 RVA: 0x0000D820 File Offset: 0x0000BA20
		public virtual void visit(_IDefineReference defref)
		{
		}

		// Token: 0x0600064C RID: 1612 RVA: 0x0000D824 File Offset: 0x0000BA24
		public virtual void visit(_IVariableReference varref)
		{
			if (varref.InstancePath != null)
			{
				varref.InstancePath.Accept(this);
			}
		}

		// Token: 0x0600064D RID: 1613 RVA: 0x0000D83C File Offset: 0x0000BA3C
		public virtual void visit(_ITypeReference typeref)
		{
			if (typeref.InstancePath != null)
			{
				typeref.InstancePath.Accept(this);
			}
		}

		// Token: 0x0600064E RID: 1614 RVA: 0x0000D854 File Offset: 0x0000BA54
		public virtual void visit(_IPouReference pouref)
		{
			if (pouref.InstancePath != null)
			{
				pouref.InstancePath.Accept(this);
			}
		}

		// Token: 0x0600064F RID: 1615 RVA: 0x0000D86C File Offset: 0x0000BA6C
		public virtual void visit(_ITaskReference taskref)
		{
		}

		// Token: 0x06000650 RID: 1616 RVA: 0x0000D870 File Offset: 0x0000BA70
		public virtual void visit(_IResourceReference resref)
		{
		}

		// Token: 0x06000651 RID: 1617 RVA: 0x0000D874 File Offset: 0x0000BA74
		public virtual void visit(_IDefinedExpression defexp)
		{
			this.Push(AccessFlag.Read);
			defexp.ItemReference.Accept(this);
			this.Pop();
		}

		// Token: 0x06000652 RID: 1618 RVA: 0x0000D890 File Offset: 0x0000BA90
		public virtual void visit(_IPragmaOperatorExpression popexp)
		{
			foreach (_IExpression iexpression in popexp.Operands)
			{
				iexpression.Accept(this);
			}
		}

		// Token: 0x06000653 RID: 1619 RVA: 0x0000D8DC File Offset: 0x0000BADC
		public virtual void visit(_IPragmaIfStatement pifst)
		{
			pifst.Condition.Accept(this);
			pifst.IfThen.Accept(this);
			foreach (_IPragmaElseIf ipragmaElseIf in pifst.ElseIf)
			{
				ipragmaElseIf.Condition.Accept(this);
				ipragmaElseIf.Controlled.Accept(this);
			}
			if (pifst.IfElse != null)
			{
				pifst.IfElse.Accept(this);
			}
		}

		// Token: 0x06000654 RID: 1620 RVA: 0x0000D964 File Offset: 0x0000BB64
		public virtual void visit(_IBreakPointStatement bpstate)
		{
		}

		// Token: 0x06000655 RID: 1621 RVA: 0x0000D968 File Offset: 0x0000BB68
		public virtual void visit(_IDefineStatement defstate)
		{
		}

		// Token: 0x06000656 RID: 1622 RVA: 0x0000D96C File Offset: 0x0000BB6C
		public virtual void visit(_IXRefExpression xref)
		{
			xref.XRef.Accept(this);
			xref.XRefFrom.Accept(this);
		}

		// Token: 0x06000657 RID: 1623 RVA: 0x0000D988 File Offset: 0x0000BB88
		public virtual void visit(_IHasTypeExpression hastype)
		{
			this.Push(AccessFlag.Read);
			hastype.Variable.Accept(this);
			this.Pop();
		}

		// Token: 0x06000658 RID: 1624 RVA: 0x0000D9A4 File Offset: 0x0000BBA4
		public virtual void visit(_IIsEnumTypeExpression isenumtype)
		{
		}

		// Token: 0x06000659 RID: 1625 RVA: 0x0000D9A8 File Offset: 0x0000BBA8
		public virtual void visit(_IHasAttributeExpression hasattribute)
		{
			this.Push(AccessFlag.Read);
			hasattribute.ItemReference.Accept(this);
			this.Pop();
		}

		// Token: 0x0600065A RID: 1626 RVA: 0x0000D9C4 File Offset: 0x0000BBC4
		public virtual void visit(_IHasValueExpression hasvalue)
		{
		}

		// Token: 0x0600065B RID: 1627 RVA: 0x0000D9C8 File Offset: 0x0000BBC8
		public virtual void visit(_IHasConstantValueExpression hasconstantvalue)
		{
			this.Push(AccessFlag.Read);
			hasconstantvalue._Constant.Accept(this);
			this.Pop();
		}

		// Token: 0x0600065C RID: 1628 RVA: 0x0000D9E4 File Offset: 0x0000BBE4
		public virtual void visit(_IPragmaAssertion assertion)
		{
			assertion.Condition.Accept(this);
		}

		// Token: 0x0600065D RID: 1629 RVA: 0x0000D9F4 File Offset: 0x0000BBF4
		public virtual void visit(_ICompilerVersionExpression compiversionexp)
		{
		}

		// Token: 0x0600065E RID: 1630 RVA: 0x0000D9F8 File Offset: 0x0000BBF8
		public virtual void visit(_ICastExpression castexp)
		{
			castexp.BaseExpression.Accept(this);
		}

		// Token: 0x0600065F RID: 1631 RVA: 0x0000DA08 File Offset: 0x0000BC08
		public virtual void visit(_INewExpression newexp)
		{
			newexp._Count.Accept(this);
			if (newexp._FBInitParams != null)
			{
				foreach (IAssignmentExpression assignmentExpression in newexp._FBInitParams)
				{
					((_IAssignmentExpression)assignmentExpression).Accept(this);
				}
			}
		}

		// Token: 0x06000660 RID: 1632 RVA: 0x0000DA6C File Offset: 0x0000BC6C
		public virtual void visit(_ITypeExpression typeexp)
		{
		}

		// Token: 0x06000661 RID: 1633 RVA: 0x0000DA70 File Offset: 0x0000BC70
		public virtual void visit(_IHasConstantTypeExpression hasConstantTypeExpression)
		{
			this.Push(AccessFlag.Read);
			hasConstantTypeExpression._Constant.Accept(this);
			this.Pop();
		}

		// Token: 0x06000662 RID: 1634 RVA: 0x0000DA8C File Offset: 0x0000BC8C
		public virtual void visit(_INamespaceAccessExpression namespaceaccess)
		{
			namespaceaccess._Namespace.Accept(this);
			_IExpression access = namespaceaccess._Access;
			if (access == null)
			{
				return;
			}
			access.Accept(this);
		}

		// Token: 0x06000663 RID: 1635 RVA: 0x0000DAAC File Offset: 0x0000BCAC
		public virtual void visit(_IRuntimeVersionExpression runtimeversionexp)
		{
		}

		// Token: 0x06000664 RID: 1636 RVA: 0x0000DAB0 File Offset: 0x0000BCB0
		public virtual void visit(_ICurrentTaskExpression currentTask)
		{
			currentTask._Base.Accept(this);
		}

		// Token: 0x06000665 RID: 1637 RVA: 0x0000DAC0 File Offset: 0x0000BCC0
		public virtual void visit(_IPoolScopeExpression poolscope)
		{
			poolscope._Base.Accept(this);
		}

		// Token: 0x06000666 RID: 1638 RVA: 0x0000DAD0 File Offset: 0x0000BCD0
		public virtual void visit(_IPartialAccessExpression partialAccessExpression)
		{
			partialAccessExpression._Left.Accept(this);
		}

		// Token: 0x06000667 RID: 1639 RVA: 0x0000DAE0 File Offset: 0x0000BCE0
		public virtual void visit(_IProjectDefinedExpression projectDefinedExpression)
		{
			_IDefineReference defineReference = projectDefinedExpression.DefineReference;
			if (defineReference == null)
			{
				return;
			}
			defineReference.Accept(this);
		}

		// Token: 0x040000AF RID: 175
		[CompilerGenerated]
		private readonly LStack<StandardVisitor.StackContent> \u0001;

		// Token: 0x02000058 RID: 88
		protected class StackContent
		{
			// Token: 0x06000668 RID: 1640 RVA: 0x0000DAF4 File Offset: 0x0000BCF4
			public StackContent(AccessFlag access)
			{
				this.Access = access;
				this.VarInOut = false;
			}

			// Token: 0x17000354 RID: 852
			// (get) Token: 0x06000669 RID: 1641 RVA: 0x0000DB0C File Offset: 0x0000BD0C
			// (set) Token: 0x0600066A RID: 1642 RVA: 0x0000DB14 File Offset: 0x0000BD14
			public AccessFlag Access { get; set; }

			// Token: 0x17000355 RID: 853
			// (get) Token: 0x0600066B RID: 1643 RVA: 0x0000DB20 File Offset: 0x0000BD20
			// (set) Token: 0x0600066C RID: 1644 RVA: 0x0000DB28 File Offset: 0x0000BD28
			public bool VarInOut { get; set; }

			// Token: 0x040000B0 RID: 176
			[CompilerGenerated]
			private AccessFlag \u0001;

			// Token: 0x040000B1 RID: 177
			[CompilerGenerated]
			private bool \u0001;
		}
	}
}
