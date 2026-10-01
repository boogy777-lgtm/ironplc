using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.Tools
{
	// Token: 0x0200006D RID: 109
	public class StandardTraverser : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor, IExprementVisitor2, IStandardTraverser352000, _IStandardTraverser, IStandardTraverser
	{
		// Token: 0x0600085A RID: 2138 RVA: 0x00010BA4 File Offset: 0x0000EDA4
		public StandardTraverser()
		{
		}

		// Token: 0x0600085B RID: 2139 RVA: 0x00010BB8 File Offset: 0x0000EDB8
		public StandardTraverser(IExprementVisitorNoTraversion351300 expCalledForAll)
		{
			this._expCalledForAll = expCalledForAll;
			this._expCalledForAll.Traverser = this;
			this.Push(AccessFlag.None);
		}

		// Token: 0x0600085C RID: 2140 RVA: 0x00010BE8 File Offset: 0x0000EDE8
		public StandardTraverser(IExprementVisitorNoTraversion351300 expCalledForAll, AccessFlag acc)
		{
			this._expCalledForAll = expCalledForAll;
			this._expCalledForAll.Traverser = this;
			this.Push(acc);
		}

		// Token: 0x0600085D RID: 2141 RVA: 0x00010C18 File Offset: 0x0000EE18
		public StandardTraverser(IExprementVisitorNoTraversion351300 expCalledForAll, bool bNoInit)
		{
			this.m_bNoInit = bNoInit;
			this._expCalledForAll = expCalledForAll;
			this._expCalledForAll.Traverser = this;
			this.Push(AccessFlag.None);
		}

		// Token: 0x0600085E RID: 2142 RVA: 0x00010C4C File Offset: 0x0000EE4C
		public void Reset()
		{
			this.Abort = false;
			this.ImplicitOn = false;
			this.\u0001.Clear();
			this.Push(AccessFlag.None);
		}

		// Token: 0x0600085F RID: 2143 RVA: 0x00010C70 File Offset: 0x0000EE70
		public void Reset(IExprementVisitorNoTraversion expCalledForAll)
		{
			this.Reset();
			this._expCalledForAll = (expCalledForAll as IExprementVisitorNoTraversion351300);
			this._expCalledForAll.Traverser = this;
		}

		// Token: 0x170003FA RID: 1018
		// (get) Token: 0x06000860 RID: 2144 RVA: 0x00010C90 File Offset: 0x0000EE90
		// (set) Token: 0x06000861 RID: 2145 RVA: 0x00010C98 File Offset: 0x0000EE98
		public bool Abort { get; set; }

		// Token: 0x170003FB RID: 1019
		// (get) Token: 0x06000862 RID: 2146 RVA: 0x00010CA4 File Offset: 0x0000EEA4
		// (set) Token: 0x06000863 RID: 2147 RVA: 0x00010CAC File Offset: 0x0000EEAC
		public bool ImplicitOn { get; private set; }

		// Token: 0x170003FC RID: 1020
		// (get) Token: 0x06000864 RID: 2148 RVA: 0x00010CB8 File Offset: 0x0000EEB8
		// (set) Token: 0x06000865 RID: 2149 RVA: 0x00010CC0 File Offset: 0x0000EEC0
		public bool CheckLicenseOperatorFound { get; private set; }

		// Token: 0x170003FD RID: 1021
		// (get) Token: 0x06000866 RID: 2150 RVA: 0x00010CCC File Offset: 0x0000EECC
		public int CurrentNestingDepth
		{
			get
			{
				return this.\u0001.Count;
			}
		}

		// Token: 0x06000867 RID: 2151 RVA: 0x00010CDC File Offset: 0x0000EEDC
		public void Push(AccessFlag access)
		{
			this.\u0001.Push(new StandardTraverser.StackContent(access));
		}

		// Token: 0x170003FE RID: 1022
		// (get) Token: 0x06000868 RID: 2152 RVA: 0x00010CF0 File Offset: 0x0000EEF0
		private StandardTraverser.StackContent ToS
		{
			get
			{
				return this.\u0001.Peek();
			}
		}

		// Token: 0x170003FF RID: 1023
		// (get) Token: 0x06000869 RID: 2153 RVA: 0x00010D00 File Offset: 0x0000EF00
		// (set) Token: 0x0600086A RID: 2154 RVA: 0x00010D14 File Offset: 0x0000EF14
		public AccessFlag TopOfStack
		{
			get
			{
				return this.\u0001.Peek().Access;
			}
			set
			{
				this.Pop();
				this.Push(value);
			}
		}

		// Token: 0x0600086B RID: 2155 RVA: 0x00010D24 File Offset: 0x0000EF24
		public void Pop()
		{
			this.\u0001.Pop();
		}

		// Token: 0x0600086C RID: 2156 RVA: 0x00010D34 File Offset: 0x0000EF34
		public virtual void visit(_IAddressExpression address)
		{
			this._expCalledForAll.visit(address);
		}

		// Token: 0x0600086D RID: 2157 RVA: 0x00010D44 File Offset: 0x0000EF44
		public virtual void visit(_IVariableExpression variable)
		{
			ISignature signatureForPrecompileID = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(variable.PrecompileSignatureId);
			if (signatureForPrecompileID != null)
			{
				IVariable variable2 = signatureForPrecompileID[variable.PrecompileVariableId];
				if (variable2 != null)
				{
					this.ToS.VarInOut = variable2.HasFlag(VarFlag.Inout);
				}
			}
			this._expCalledForAll.visit(variable, this.TopOfStack);
		}

		// Token: 0x0600086E RID: 2158 RVA: 0x00010DA0 File Offset: 0x0000EFA0
		public virtual void visit(_ICompiledPOU cpou)
		{
			if (cpou.GetFlag(CompiledPOUFlags.ContainsDirVarAccess))
			{
				cpou.GetParseTree().Accept(this);
			}
			this._expCalledForAll.visit(cpou);
		}

		// Token: 0x0600086F RID: 2159 RVA: 0x00010DC8 File Offset: 0x0000EFC8
		public virtual void visit(_IWhileStatement whilst)
		{
			this.Push(AccessFlag.Read);
			whilst._Condition.Accept(this);
			this.Pop();
			whilst._Controlled.Accept(this);
			this._expCalledForAll.visit(whilst);
		}

		// Token: 0x06000870 RID: 2160 RVA: 0x00010DFC File Offset: 0x0000EFFC
		public virtual void visit(_IRepeatStatement repeat)
		{
			this.Push(AccessFlag.Read);
			repeat._Condition.Accept(this);
			this.Pop();
			repeat._Controlled.Accept(this);
			this._expCalledForAll.visit(repeat);
		}

		// Token: 0x06000871 RID: 2161 RVA: 0x00010E30 File Offset: 0x0000F030
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
			this._expCalledForAll.visit(forloop);
		}

		// Token: 0x06000872 RID: 2162 RVA: 0x00010E9C File Offset: 0x0000F09C
		public virtual void visit(_ISequenceStatement seq)
		{
			if (this.Abort)
			{
				return;
			}
			this.Push(AccessFlag.Unknown);
			IList<_IStatement> statementList = seq._StatementList;
			for (int i = 0; i < statementList.Count; i++)
			{
				statementList[i].Accept(this);
				if (this.Abort)
				{
					break;
				}
			}
			this.Pop();
			this._expCalledForAll.visit(seq);
		}

		// Token: 0x06000873 RID: 2163 RVA: 0x00010EFC File Offset: 0x0000F0FC
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
			if (ifElse != null)
			{
				ifElse.Accept(this);
			}
			this._expCalledForAll.visit(ifst);
		}

		// Token: 0x06000874 RID: 2164 RVA: 0x00010FA8 File Offset: 0x0000F1A8
		public virtual void visit(_IExpressionStatement expstat)
		{
			this.Push(AccessFlag.Read);
			expstat._Expr.Accept(this);
			this.Pop();
			this._expCalledForAll.visit(expstat);
		}

		// Token: 0x06000875 RID: 2165 RVA: 0x00010FD0 File Offset: 0x0000F1D0
		public virtual void visit(_IAssignmentExpression assign)
		{
			if (this._expCalledForAll.DoAssignExpression)
			{
				this._expCalledForAll.visit(assign);
				return;
			}
			this.Push(AccessFlag.Write);
			assign._LValue.Accept(this);
			bool varInOut = this.ToS.VarInOut;
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
			if (varInOut && this.ToS.InputAssignment)
			{
				access = (AccessFlag.Read | AccessFlag.Write);
			}
			this.Push(access);
			assign._RValue.Accept(this);
			this.Pop();
			this._expCalledForAll.visit(assign);
		}

		// Token: 0x06000876 RID: 2166 RVA: 0x00011088 File Offset: 0x0000F288
		public static bool IsImplicitReferenceAssignment(_ICallExpression call, _IAssignmentExpression inputAssignParam, int iParameterIndex)
		{
			if (inputAssignParam.LValue is INullExpression)
			{
				_IVariableExpression ivariableExpression;
				if (call.Callee is _ICompoAccessExpression)
				{
					ivariableExpression = (((_ICompoAccessExpression)call.Callee)._Right as _IVariableExpression);
				}
				else if (call.Callee is IGlobalScopeExpression)
				{
					ivariableExpression = (((IGlobalScopeExpression)call.Callee).Base as _IVariableExpression);
				}
				else
				{
					ivariableExpression = (call.Callee as _IVariableExpression);
				}
				if (ivariableExpression == null)
				{
					return false;
				}
				_ISignature isignature = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(ivariableExpression.PrecompileSignatureId) as _ISignature;
				if (isignature != null && (isignature.POUType == Operator.Function || isignature.POUType == Operator.Method) && iParameterIndex < isignature.AllInputs.Length)
				{
					_IVariable ivariable = isignature.AllInputs[iParameterIndex] as _IVariable;
					if (ivariable != null && (ivariable.GetFlag(VarFlag.Inout) || (ivariable.Type != null && ivariable.Type.Class == TypeClass.Reference)))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06000877 RID: 2167 RVA: 0x00011174 File Offset: 0x0000F374
		public virtual void visit(_ICallExpression call)
		{
			if (this._expCalledForAll.DoCallExpression)
			{
				this._expCalledForAll.visit(call);
				return;
			}
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
							this.ToS.VarInOut = true;
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
							this.ToS.InputAssignment = true;
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
			this._expCalledForAll.visit(call);
		}

		// Token: 0x06000878 RID: 2168 RVA: 0x00011428 File Offset: 0x0000F628
		public virtual void visit(_IOperatorExpression op)
		{
			foreach (_IExprement iexprement in op._OperandsList)
			{
				if (op.Code == Operator.Adr || op.Code == Operator.__RefAdr)
				{
					this.TopOfStack = (AccessFlag.Write | AccessFlag.Address);
				}
				iexprement.Accept(this);
			}
			this._expCalledForAll.visit(op);
			if (op.Code == Operator.__CheckLicense || op.Code == Operator.__CheckLicenseBit)
			{
				this.CheckLicenseOperatorFound = true;
			}
		}

		// Token: 0x06000879 RID: 2169 RVA: 0x000114C0 File Offset: 0x0000F6C0
		public virtual void visit(_ICastExpression cast)
		{
			cast.BaseExpression.Accept(this);
			this._expCalledForAll.visit(cast);
		}

		// Token: 0x0600087A RID: 2170 RVA: 0x000114DC File Offset: 0x0000F6DC
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
			this._expCalledForAll.visit(newexp);
		}

		// Token: 0x0600087B RID: 2171 RVA: 0x0001154C File Offset: 0x0000F74C
		public virtual void visit(_ITypeExpression typeexp)
		{
			this._expCalledForAll.visit(typeexp);
		}

		// Token: 0x0600087C RID: 2172 RVA: 0x0001155C File Offset: 0x0000F75C
		public virtual void visit(_IConversionExpression conv)
		{
			conv._Exp.Accept(this);
			this._expCalledForAll.visit(conv);
		}

		// Token: 0x0600087D RID: 2173 RVA: 0x00011578 File Offset: 0x0000F778
		public virtual void visit(_IIndexAccessExpression indexaccess)
		{
			this.Push(AccessFlag.Read);
			for (int i = 0; i < indexaccess.NumAccesses; i++)
			{
				indexaccess.GetAccess(i).Accept(this);
			}
			this.Pop();
			indexaccess._Var.Accept(this);
			this._expCalledForAll.visit(indexaccess);
		}

		// Token: 0x0600087E RID: 2174 RVA: 0x000115C8 File Offset: 0x0000F7C8
		public virtual void visit(_ILiteralExpression literal)
		{
			this._expCalledForAll.visit(literal);
		}

		// Token: 0x0600087F RID: 2175 RVA: 0x000115D8 File Offset: 0x0000F7D8
		public virtual void visit(_ICompoAccessExpression compo)
		{
			if (this._expCalledForAll.bResolveCompoAccessExpression)
			{
				compo._Right.Accept(this);
				bool inLeftSideOfCompoAccess = this.ToS.InLeftSideOfCompoAccess;
				this.ToS.InLeftSideOfCompoAccess = true;
				compo._Left.Accept(this);
				if (!inLeftSideOfCompoAccess)
				{
					this.ToS.InLeftSideOfCompoAccess = false;
				}
			}
			this._expCalledForAll.visit(compo, this.TopOfStack);
		}

		// Token: 0x06000880 RID: 2176 RVA: 0x00011644 File Offset: 0x0000F844
		public virtual void visit(_IDeRefAccessExpression deref)
		{
			deref._Base.Accept(this);
			this._expCalledForAll.visit(deref);
		}

		// Token: 0x06000881 RID: 2177 RVA: 0x00011660 File Offset: 0x0000F860
		public virtual void visit(_ICopyScopeExpression copyexp)
		{
			copyexp._Base.Accept(this);
			this._expCalledForAll.visit(copyexp);
		}

		// Token: 0x06000882 RID: 2178 RVA: 0x0001167C File Offset: 0x0000F87C
		public virtual void visit(_IGlobalScopeExpression globexp)
		{
			globexp._Base.Accept(this);
			this._expCalledForAll.visit(globexp);
		}

		// Token: 0x06000883 RID: 2179 RVA: 0x00011698 File Offset: 0x0000F898
		public virtual void visit(_ISystemScopeExpression systemscope)
		{
			systemscope._Base.Accept(this);
			this._expCalledForAll.visit(systemscope);
		}

		// Token: 0x06000884 RID: 2180 RVA: 0x000116B4 File Offset: 0x0000F8B4
		public virtual void visit(_IPoolScopeExpression poolscope)
		{
			poolscope._Base.Accept(this);
			this._expCalledForAll.visit(poolscope);
		}

		// Token: 0x06000885 RID: 2181 RVA: 0x000116D0 File Offset: 0x0000F8D0
		public virtual void visit(_INamespaceAccessExpression namespaceaccess)
		{
			namespaceaccess._Namespace.Accept(this);
			_IExpression access = namespaceaccess._Access;
			if (access != null)
			{
				access.Accept(this);
			}
			IExprementVisitorNoTraversion351500 exprementVisitorNoTraversion = this._expCalledForAll as IExprementVisitorNoTraversion351500;
			if (exprementVisitorNoTraversion == null)
			{
				return;
			}
			exprementVisitorNoTraversion.visit(namespaceaccess);
		}

		// Token: 0x06000886 RID: 2182 RVA: 0x00011708 File Offset: 0x0000F908
		public virtual void visit(_ICurrentTaskExpression curTaskExp)
		{
			curTaskExp._Base.Accept(this);
			this._expCalledForAll.visit(curTaskExp);
		}

		// Token: 0x06000887 RID: 2183 RVA: 0x00011724 File Offset: 0x0000F924
		public virtual void visit(_ICaseRangeExpression caserange)
		{
			caserange._Low.Accept(this);
			caserange._High.Accept(this);
			this._expCalledForAll.visit(caserange);
		}

		// Token: 0x06000888 RID: 2184 RVA: 0x0001174C File Offset: 0x0000F94C
		public virtual void visit(_ICaseLabelStatement caselabel)
		{
			foreach (_IExpression iexpression in caselabel._cases)
			{
				iexpression.Accept(this);
			}
			this._expCalledForAll.visit(caselabel);
		}

		// Token: 0x06000889 RID: 2185 RVA: 0x000117A4 File Offset: 0x0000F9A4
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
			this._expCalledForAll.visit(casest);
		}

		// Token: 0x0600088A RID: 2186 RVA: 0x00011848 File Offset: 0x0000FA48
		public virtual void visit(_IExitStatement exit)
		{
			this._expCalledForAll.visit(exit);
		}

		// Token: 0x0600088B RID: 2187 RVA: 0x00011858 File Offset: 0x0000FA58
		public virtual void visit(_IContinueStatement cont)
		{
			this._expCalledForAll.visit(cont);
		}

		// Token: 0x0600088C RID: 2188 RVA: 0x00011868 File Offset: 0x0000FA68
		public virtual void visit(_IThisExpression thisexp)
		{
			this._expCalledForAll.visit(thisexp);
		}

		// Token: 0x0600088D RID: 2189 RVA: 0x00011878 File Offset: 0x0000FA78
		public virtual void visit(_IBaseExpression baseexp)
		{
			this._expCalledForAll.visit(baseexp);
		}

		// Token: 0x0600088E RID: 2190 RVA: 0x00011888 File Offset: 0x0000FA88
		public virtual void visit(_IEmptyStatement empty)
		{
			this._expCalledForAll.visit(empty);
		}

		// Token: 0x0600088F RID: 2191 RVA: 0x00011898 File Offset: 0x0000FA98
		public virtual void visit(_IReturnStatement returnst)
		{
			if (returnst._Condition != null)
			{
				this.Push(AccessFlag.Read);
				returnst._Condition.Accept(this);
				this.Pop();
			}
			this._expCalledForAll.visit(returnst);
		}

		// Token: 0x06000890 RID: 2192 RVA: 0x000118C8 File Offset: 0x0000FAC8
		public virtual void visit(_IJumpStatement gotost)
		{
			if (gotost._Condition != null)
			{
				this.Push(AccessFlag.Read);
				gotost._Condition.Accept(this);
				this.Pop();
			}
			this._expCalledForAll.visit(gotost);
		}

		// Token: 0x06000891 RID: 2193 RVA: 0x000118F8 File Offset: 0x0000FAF8
		public virtual void visit(_ILabelStatement label)
		{
			this._expCalledForAll.visit(label);
		}

		// Token: 0x06000892 RID: 2194 RVA: 0x00011908 File Offset: 0x0000FB08
		public virtual void visit(_ICommentStatement comment)
		{
			this._expCalledForAll.visit(comment);
		}

		// Token: 0x06000893 RID: 2195 RVA: 0x00011918 File Offset: 0x0000FB18
		public virtual void visit(_IPragmaStatement pragma)
		{
			if (pragma.Text == "implicit on")
			{
				this.ImplicitOn = true;
			}
			if (pragma.Text == "implicit off")
			{
				this.ImplicitOn = false;
			}
			this._expCalledForAll.visit(pragma);
		}

		// Token: 0x06000894 RID: 2196 RVA: 0x00011958 File Offset: 0x0000FB58
		public virtual void visit(_IErrorExpression errorexp)
		{
			this._expCalledForAll.visit(errorexp);
		}

		// Token: 0x06000895 RID: 2197 RVA: 0x00011968 File Offset: 0x0000FB68
		public virtual void visit(_IErrorStatement errorst)
		{
			this._expCalledForAll.visit(errorst);
		}

		// Token: 0x06000896 RID: 2198 RVA: 0x00011978 File Offset: 0x0000FB78
		public virtual void visit(_INullExpression errorexp)
		{
			this._expCalledForAll.visit(errorexp);
		}

		// Token: 0x06000897 RID: 2199 RVA: 0x00011988 File Offset: 0x0000FB88
		public virtual void visit(_INullStatement errorst)
		{
			this._expCalledForAll.visit(errorst);
		}

		// Token: 0x06000898 RID: 2200 RVA: 0x00011998 File Offset: 0x0000FB98
		public virtual void visit(_IQualifiedNameExpression qne)
		{
			this._expCalledForAll.visit(qne);
		}

		// Token: 0x06000899 RID: 2201 RVA: 0x000119A8 File Offset: 0x0000FBA8
		public virtual void visit(_IVariableDeclarationStatement vds)
		{
			foreach (_IExpression iexpression in vds.NameList)
			{
				iexpression.Accept(this);
			}
			if (vds.Initial != null && !this.m_bNoInit)
			{
				vds.Initial.Accept(this);
			}
			if (vds.InputAssigns != null && !this.m_bNoInit)
			{
				foreach (_IAssignmentExpression iassignmentExpression in vds.InputAssigns)
				{
					iassignmentExpression.Accept(this);
				}
			}
			this._expCalledForAll.visit(vds);
		}

		// Token: 0x0600089A RID: 2202 RVA: 0x00011A68 File Offset: 0x0000FC68
		public virtual void visit(_IVariableDeclarationListStatement vdls)
		{
			bool bNoInit = this.m_bNoInit;
			if (vdls.GetFlag(VarFlag.Constant))
			{
				this.m_bNoInit = false;
			}
			vdls.VariableDeclaration.Accept(this);
			this.m_bNoInit = bNoInit;
			this._expCalledForAll.visit(vdls);
		}

		// Token: 0x0600089B RID: 2203 RVA: 0x00011AB0 File Offset: 0x0000FCB0
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
			this._expCalledForAll.visit(pds);
		}

		// Token: 0x0600089C RID: 2204 RVA: 0x00011B60 File Offset: 0x0000FD60
		public virtual void visit(_ITypeDeclarationStatement tds)
		{
			tds.Declarations.Accept(this);
			if (tds.Initial != null && !this.m_bNoInit)
			{
				tds.Initial.Accept(this);
			}
			if (tds.Extends != null)
			{
				tds.Extends.Accept(this);
			}
			this._expCalledForAll.visit(tds);
		}

		// Token: 0x0600089D RID: 2205 RVA: 0x00011BB8 File Offset: 0x0000FDB8
		public virtual void visit(_IEnumDeclarationStatement eds)
		{
			if (eds._Value != null && !this.m_bNoInit)
			{
				eds._Value.Accept(this);
			}
			this._expCalledForAll.visit(eds);
		}

		// Token: 0x0600089E RID: 2206 RVA: 0x00011BE4 File Offset: 0x0000FDE4
		public virtual void visit(_IEnumDeclarationListStatement eds)
		{
			foreach (_IEnumDeclarationStatement ienumDeclarationStatement in eds.Enums)
			{
				ienumDeclarationStatement.Accept(this);
			}
			this._expCalledForAll.visit(eds);
		}

		// Token: 0x0600089F RID: 2207 RVA: 0x00011C3C File Offset: 0x0000FE3C
		public virtual void visit(_IMultipleIndexInitialization errorst)
		{
			errorst._Value.Accept(this);
			errorst._Number.Accept(this);
			this._expCalledForAll.visit(errorst);
		}

		// Token: 0x060008A0 RID: 2208 RVA: 0x00011C64 File Offset: 0x0000FE64
		public virtual void visit(_IArrayInitialization errorexp)
		{
			foreach (_IExpression iexpression in errorexp._InitValues)
			{
				iexpression.Accept(this);
			}
			this._expCalledForAll.visit(errorexp);
		}

		// Token: 0x060008A1 RID: 2209 RVA: 0x00011CBC File Offset: 0x0000FEBC
		public virtual void visit(_IStructureInitialization errorst)
		{
			foreach (_IAssignmentExpression iassignmentExpression in errorst._CompoInits)
			{
				iassignmentExpression.Accept(this);
			}
			this._expCalledForAll.visit(errorst);
		}

		// Token: 0x060008A2 RID: 2210 RVA: 0x00011D14 File Offset: 0x0000FF14
		public virtual void visit(_IDefineReference defref)
		{
			this._expCalledForAll.visit(defref);
		}

		// Token: 0x060008A3 RID: 2211 RVA: 0x00011D24 File Offset: 0x0000FF24
		public virtual void visit(_IVariableReference varref)
		{
			if (varref.InstancePath != null)
			{
				varref.InstancePath.Accept(this);
			}
			this._expCalledForAll.visit(varref);
		}

		// Token: 0x060008A4 RID: 2212 RVA: 0x00011D48 File Offset: 0x0000FF48
		public virtual void visit(_ITypeReference typeref)
		{
			if (typeref.InstancePath != null)
			{
				typeref.InstancePath.Accept(this);
			}
			this._expCalledForAll.visit(typeref);
		}

		// Token: 0x060008A5 RID: 2213 RVA: 0x00011D6C File Offset: 0x0000FF6C
		public virtual void visit(_IPouReference pouref)
		{
			if (pouref.InstancePath != null)
			{
				pouref.InstancePath.Accept(this);
			}
			this._expCalledForAll.visit(pouref);
		}

		// Token: 0x060008A6 RID: 2214 RVA: 0x00011D90 File Offset: 0x0000FF90
		public virtual void visit(_ITaskReference taskref)
		{
			this._expCalledForAll.visit(taskref);
		}

		// Token: 0x060008A7 RID: 2215 RVA: 0x00011DA0 File Offset: 0x0000FFA0
		public virtual void visit(_IResourceReference resref)
		{
			this._expCalledForAll.visit(resref);
		}

		// Token: 0x060008A8 RID: 2216 RVA: 0x00011DB0 File Offset: 0x0000FFB0
		public virtual void visit(_IDefinedExpression defexp)
		{
			this.Push(AccessFlag.Read);
			defexp.ItemReference.Accept(this);
			this.Pop();
			this._expCalledForAll.visit(defexp);
		}

		// Token: 0x060008A9 RID: 2217 RVA: 0x00011DD8 File Offset: 0x0000FFD8
		public virtual void visit(_IPragmaOperatorExpression popexp)
		{
			foreach (_IExpression iexpression in popexp.Operands)
			{
				iexpression.Accept(this);
			}
			this._expCalledForAll.visit(popexp);
		}

		// Token: 0x060008AA RID: 2218 RVA: 0x00011E30 File Offset: 0x00010030
		public virtual void visit(_IPragmaIfStatement pifst)
		{
			if (this._expCalledForAll is IExprementVisitorNoTraversion2)
			{
				bool flag;
				(this._expCalledForAll as IExprementVisitorNoTraversion2).visit(pifst, out flag);
				if (flag)
				{
					return;
				}
			}
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
			this._expCalledForAll.visit(pifst);
		}

		// Token: 0x060008AB RID: 2219 RVA: 0x00011EE8 File Offset: 0x000100E8
		public virtual void visit(_IBreakPointStatement bpstate)
		{
			this._expCalledForAll.visit(bpstate);
		}

		// Token: 0x060008AC RID: 2220 RVA: 0x00011EF8 File Offset: 0x000100F8
		public virtual void visit(_IDefineStatement defstate)
		{
			this._expCalledForAll.visit(defstate);
		}

		// Token: 0x060008AD RID: 2221 RVA: 0x00011F08 File Offset: 0x00010108
		public virtual void visit(_IXRefExpression xref)
		{
			xref.XRef.Accept(this);
			xref.XRefFrom.Accept(this);
			this._expCalledForAll.visit(xref);
		}

		// Token: 0x060008AE RID: 2222 RVA: 0x00011F30 File Offset: 0x00010130
		public virtual void visit(_IHasTypeExpression hastype)
		{
			this.Push(AccessFlag.Read);
			hastype.Variable.Accept(this);
			this.Pop();
			this._expCalledForAll.visit(hastype);
		}

		// Token: 0x060008AF RID: 2223 RVA: 0x00011F58 File Offset: 0x00010158
		public virtual void visit(_IIsEnumTypeExpression isenumtype)
		{
			this._expCalledForAll.visit(isenumtype);
		}

		// Token: 0x060008B0 RID: 2224 RVA: 0x00011F68 File Offset: 0x00010168
		public virtual void visit(_IHasAttributeExpression hasattribute)
		{
			this.Push(AccessFlag.Read);
			hasattribute.ItemReference.Accept(this);
			this.Pop();
			this._expCalledForAll.visit(hasattribute);
		}

		// Token: 0x060008B1 RID: 2225 RVA: 0x00011F90 File Offset: 0x00010190
		public virtual void visit(_IHasValueExpression hasvalue)
		{
			this._expCalledForAll.visit(hasvalue);
		}

		// Token: 0x060008B2 RID: 2226 RVA: 0x00011FA0 File Offset: 0x000101A0
		public virtual void visit(_IHasConstantValueExpression hasvalue)
		{
			this.Push(AccessFlag.Read);
			hasvalue._Constant.Accept(this);
			this.Pop();
			this._expCalledForAll.visit(hasvalue);
		}

		// Token: 0x060008B3 RID: 2227 RVA: 0x00011FC8 File Offset: 0x000101C8
		public void visit(_IHasConstantTypeExpression hasConstantTypeExpression)
		{
			this.Push(AccessFlag.Read);
			hasConstantTypeExpression._Constant.Accept(this);
			this.Pop();
			IExprementVisitorNoTraversion351800 exprementVisitorNoTraversion = this._expCalledForAll as IExprementVisitorNoTraversion351800;
			if (exprementVisitorNoTraversion == null)
			{
				return;
			}
			exprementVisitorNoTraversion.visit(hasConstantTypeExpression);
		}

		// Token: 0x060008B4 RID: 2228 RVA: 0x00011FFC File Offset: 0x000101FC
		public virtual void visit(_IPragmaAssertion assertion)
		{
			assertion.Condition.Accept(this);
			this._expCalledForAll.visit(assertion);
		}

		// Token: 0x060008B5 RID: 2229 RVA: 0x00012018 File Offset: 0x00010218
		public virtual void visit(_ICompilerVersionExpression compversion)
		{
			this._expCalledForAll.visit(compversion);
		}

		// Token: 0x060008B6 RID: 2230 RVA: 0x00012028 File Offset: 0x00010228
		public virtual void visit(_IRuntimeVersionExpression runExp)
		{
			IExprementVisitorNoTraversion351400 exprementVisitorNoTraversion = this._expCalledForAll as IExprementVisitorNoTraversion351400;
			if (exprementVisitorNoTraversion == null)
			{
				return;
			}
			exprementVisitorNoTraversion.visit(runExp);
		}

		// Token: 0x060008B7 RID: 2231 RVA: 0x00012040 File Offset: 0x00010240
		public void visit(_ITryCatchStatement trycatchstatement)
		{
			this._expCalledForAll.visit(trycatchstatement);
			trycatchstatement.DefaultTraverse(this);
		}

		// Token: 0x060008B8 RID: 2232 RVA: 0x00012058 File Offset: 0x00010258
		public void visit(_IPartialAccessExpression partialAccessExpression)
		{
			IExprementVisitorNoTraversion351900 exprementVisitorNoTraversion = this._expCalledForAll as IExprementVisitorNoTraversion351900;
			if (exprementVisitorNoTraversion != null)
			{
				exprementVisitorNoTraversion.visit(partialAccessExpression);
			}
			partialAccessExpression._Left.Accept(this);
		}

		// Token: 0x060008B9 RID: 2233 RVA: 0x00012080 File Offset: 0x00010280
		public void visit(_IProjectDefinedExpression projectDefinedExpression)
		{
			IExprementVisitorNoTraversion352000 exprementVisitorNoTraversion = this._expCalledForAll as IExprementVisitorNoTraversion352000;
			if (exprementVisitorNoTraversion != null)
			{
				exprementVisitorNoTraversion.visit(projectDefinedExpression);
			}
			_IDefineReference defineReference = projectDefinedExpression.DefineReference;
			if (defineReference == null)
			{
				return;
			}
			defineReference.Accept(this);
		}

		// Token: 0x17000400 RID: 1024
		// (get) Token: 0x060008BA RID: 2234 RVA: 0x000120AC File Offset: 0x000102AC
		public bool InLeftSideOfCompoAccess
		{
			get
			{
				return this.ToS.InLeftSideOfCompoAccess;
			}
		}

		// Token: 0x04000125 RID: 293
		protected IExprementVisitorNoTraversion351300 _expCalledForAll;

		// Token: 0x04000126 RID: 294
		private readonly LStack<StandardTraverser.StackContent> \u0001 = new LStack<StandardTraverser.StackContent>();

		// Token: 0x04000127 RID: 295
		protected bool m_bNoInit;

		// Token: 0x04000128 RID: 296
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x04000129 RID: 297
		[CompilerGenerated]
		private bool \u0002;

		// Token: 0x0400012A RID: 298
		[CompilerGenerated]
		private bool \u0003;

		// Token: 0x0200006E RID: 110
		public class StackContent
		{
			// Token: 0x060008BB RID: 2235 RVA: 0x000120BC File Offset: 0x000102BC
			public StackContent(AccessFlag access)
			{
				this.Access = access;
				this.VarInOut = false;
				this.InputAssignment = false;
			}

			// Token: 0x17000401 RID: 1025
			// (get) Token: 0x060008BC RID: 2236 RVA: 0x000120DC File Offset: 0x000102DC
			// (set) Token: 0x060008BD RID: 2237 RVA: 0x000120E4 File Offset: 0x000102E4
			public AccessFlag Access { get; set; }

			// Token: 0x17000402 RID: 1026
			// (get) Token: 0x060008BE RID: 2238 RVA: 0x000120F0 File Offset: 0x000102F0
			// (set) Token: 0x060008BF RID: 2239 RVA: 0x000120F8 File Offset: 0x000102F8
			public bool VarInOut { get; set; }

			// Token: 0x17000403 RID: 1027
			// (get) Token: 0x060008C0 RID: 2240 RVA: 0x00012104 File Offset: 0x00010304
			// (set) Token: 0x060008C1 RID: 2241 RVA: 0x0001210C File Offset: 0x0001030C
			public bool InputAssignment { get; set; }

			// Token: 0x17000404 RID: 1028
			// (get) Token: 0x060008C2 RID: 2242 RVA: 0x00012118 File Offset: 0x00010318
			// (set) Token: 0x060008C3 RID: 2243 RVA: 0x00012120 File Offset: 0x00010320
			public bool InLeftSideOfCompoAccess { get; internal set; }

			// Token: 0x0400012B RID: 299
			[CompilerGenerated]
			private AccessFlag \u0001;

			// Token: 0x0400012C RID: 300
			[CompilerGenerated]
			private bool \u0001;

			// Token: 0x0400012D RID: 301
			[CompilerGenerated]
			private bool \u0002;

			// Token: 0x0400012E RID: 302
			[CompilerGenerated]
			private bool \u0003;
		}
	}
}
