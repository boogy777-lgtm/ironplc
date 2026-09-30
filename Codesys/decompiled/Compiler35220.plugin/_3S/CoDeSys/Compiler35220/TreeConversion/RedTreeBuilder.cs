using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.TreeConversion
{
	// Token: 0x02000014 RID: 20
	public class RedTreeBuilder : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor, IExprementVisitor2
	{
		// Token: 0x060003EB RID: 1003 RVA: 0x00007824 File Offset: 0x00005A24
		internal RedTreeBuilder(ITreeFactory factory)
		{
			this._factory = factory;
		}

		// Token: 0x060003EC RID: 1004 RVA: 0x00007834 File Offset: 0x00005A34
		public static _IExprement BuildRedTree(_IExprement exp, ITreeFactory factory, ICompactedParseTreeInformation parseTreeInfo)
		{
			RedTreeBuilder redTreeBuilder = new RedTreeBuilder(factory);
			exp.Accept(redTreeBuilder);
			_IExprement builtExprement = redTreeBuilder._builtExprement;
			PrecompileParseTreeInformationSetter.SetInformationInParseTree(builtExprement, parseTreeInfo);
			return builtExprement;
		}

		// Token: 0x1700034A RID: 842
		// (get) Token: 0x060003ED RID: 1005 RVA: 0x0000785C File Offset: 0x00005A5C
		public _IExpression BuiltExpression
		{
			get
			{
				return (_IExpression)this._builtExprement;
			}
		}

		// Token: 0x1700034B RID: 843
		// (get) Token: 0x060003EE RID: 1006 RVA: 0x0000786C File Offset: 0x00005A6C
		public _IStatement BuiltStatement
		{
			get
			{
				return (_IStatement)this._builtExprement;
			}
		}

		// Token: 0x060003EF RID: 1007 RVA: 0x0000787C File Offset: 0x00005A7C
		public void visit(_ICompiledPOU cpou)
		{
			throw new NotImplementedException();
		}

		// Token: 0x060003F0 RID: 1008 RVA: 0x00007884 File Offset: 0x00005A84
		public void visit(_ISequenceStatement seqx)
		{
			_IStatement[] array = new _IStatement[seqx._StatementList.Count];
			for (int i = 0; i < seqx._StatementList.Count; i++)
			{
				seqx._StatementList[i].Accept(this);
				array[i] = this.BuiltStatement;
				((_IStatement2)array[i]).Flags = ((_IStatement2)seqx._StatementList[i]).Flags;
			}
			_ISequenceStatement builtExprement = this._factory.CreateSequenceStatement(array);
			this._builtExprement = builtExprement;
		}

		// Token: 0x060003F1 RID: 1009 RVA: 0x0000790C File Offset: 0x00005B0C
		public void visit(_IWhileStatement whilst)
		{
			whilst._Condition.Accept(this);
			_IExpression builtExpression = this.BuiltExpression;
			whilst._Controlled.Accept(this);
			_IStatement builtStatement = this.BuiltStatement;
			_IWhileStatement builtExprement = this._factory.CreateWhileStatement(builtExpression, builtStatement);
			this._builtExprement = builtExprement;
		}

		// Token: 0x060003F2 RID: 1010 RVA: 0x00007954 File Offset: 0x00005B54
		public void visit(_IRepeatStatement repeat)
		{
			repeat._Condition.Accept(this);
			_IExpression builtExpression = this.BuiltExpression;
			repeat._Controlled.Accept(this);
			_IStatement builtStatement = this.BuiltStatement;
			_IRepeatStatement builtExprement = this._factory.CreateRepeatStatement(builtExpression, builtStatement);
			this._builtExprement = builtExprement;
		}

		// Token: 0x060003F3 RID: 1011 RVA: 0x0000799C File Offset: 0x00005B9C
		public void visit(_IForStatement forloop)
		{
			forloop._CounterStart.Accept(this);
			_IExpression builtExpression = this.BuiltExpression;
			forloop._UpperBound.Accept(this);
			_IExpression builtExpression2 = this.BuiltExpression;
			_IExpression by = null;
			if (forloop.By != null)
			{
				forloop._By.Accept(this);
				by = this.BuiltExpression;
			}
			_IExpression condition = null;
			if (forloop._Condition != null)
			{
				forloop._Condition.Accept(this);
				condition = this.BuiltExpression;
			}
			_IExpression counter = null;
			if (forloop._Counter != null)
			{
				forloop._Counter.Accept(this);
				counter = this.BuiltExpression;
			}
			forloop._Controlled.Accept(this);
			_IStatement builtStatement = this.BuiltStatement;
			_IForStatement builtExprement = this._factory.CreateForStatement(builtExpression, counter, by, builtExpression2, condition, builtStatement);
			this._builtExprement = builtExprement;
		}

		// Token: 0x060003F4 RID: 1012 RVA: 0x00007A5C File Offset: 0x00005C5C
		public virtual void visit(_IExitStatement exit)
		{
			_IExitStatement builtExprement = this._factory.CreateExitStatement();
			this._builtExprement = builtExprement;
		}

		// Token: 0x060003F5 RID: 1013 RVA: 0x00007A7C File Offset: 0x00005C7C
		public virtual void visit(_IContinueStatement cont)
		{
			_IContinueStatement builtExprement = this._factory.CreateContinueStatement();
			this._builtExprement = builtExprement;
		}

		// Token: 0x060003F6 RID: 1014 RVA: 0x00007A9C File Offset: 0x00005C9C
		public void visit(_IIfStatement ifst)
		{
			ifst._Condition.Accept(this);
			_IExpression builtExpression = this.BuiltExpression;
			ifst._IfThen.Accept(this);
			_IStatement builtStatement = this.BuiltStatement;
			_IElseIf[] array = new _IElseIf[ifst._ElseIf.Count];
			for (int i = 0; i < array.Length; i++)
			{
				_IElseIf ielseIf = ifst._ElseIf[i];
				ielseIf._Condition.Accept(this);
				_IExpression builtExpression2 = this.BuiltExpression;
				ielseIf._Controlled.Accept(this);
				_IStatement builtStatement2 = this.BuiltStatement;
				_IElseIf ielseIf2 = this._factory.CreateElseIf(builtExpression2, builtStatement2);
				array[i] = ielseIf2;
			}
			_IStatement stateElse = null;
			if (ifst._IfElse != null)
			{
				ifst._IfElse.Accept(this);
				stateElse = this.BuiltStatement;
			}
			_IIfStatement builtExprement = this._factory.CreateIfStatement(builtExpression, builtStatement, stateElse, array);
			this._builtExprement = builtExprement;
		}

		// Token: 0x060003F7 RID: 1015 RVA: 0x00007B74 File Offset: 0x00005D74
		public virtual void visit(_IReturnStatement returnst)
		{
			_IExpression expCondition = null;
			if (returnst._Condition != null)
			{
				returnst._Condition.Accept(this);
				expCondition = this.BuiltExpression;
			}
			_IReturnStatement builtExprement = this._factory.CreateReturnStatement(expCondition);
			this._builtExprement = builtExprement;
		}

		// Token: 0x060003F8 RID: 1016 RVA: 0x00007BB4 File Offset: 0x00005DB4
		public void visit(_IJumpStatement gotost)
		{
			_IExpression expCondition = null;
			if (gotost._Condition != null)
			{
				gotost._Condition.Accept(this);
				expCondition = this.BuiltExpression;
			}
			_IJumpStatement builtExprement = this._factory.CreateJumpStatement(expCondition, gotost.Label);
			this._builtExprement = builtExprement;
		}

		// Token: 0x060003F9 RID: 1017 RVA: 0x00007BF8 File Offset: 0x00005DF8
		public void visit(_ILabelStatement label)
		{
			_ILabelStatement builtExprement = this._factory.CreateLabelStatement(label.OrgText);
			this._builtExprement = builtExprement;
		}

		// Token: 0x060003FA RID: 1018 RVA: 0x00007C20 File Offset: 0x00005E20
		public void visit(_ICommentStatement comment)
		{
			_ICommentStatement builtExprement = this._factory.CreateCommentStatement(comment.Text, comment.DocComment);
			this._builtExprement = builtExprement;
		}

		// Token: 0x060003FB RID: 1019 RVA: 0x00007C4C File Offset: 0x00005E4C
		public void visit(_IPragmaStatement pragma)
		{
			_IMessageGuidPragmaStatement imessageGuidPragmaStatement = pragma as _IMessageGuidPragmaStatement;
			_IStatement builtExprement;
			if (imessageGuidPragmaStatement == null)
			{
				_IImplicitCodeSectionPragma iimplicitCodeSectionPragma = pragma as _IImplicitCodeSectionPragma;
				if (iimplicitCodeSectionPragma == null)
				{
					_ILocalSignatureIdPragma ilocalSignatureIdPragma = pragma as _ILocalSignatureIdPragma;
					if (ilocalSignatureIdPragma == null)
					{
						_IWarningDisableRestorePragmaStatement iwarningDisableRestorePragmaStatement = pragma as _IWarningDisableRestorePragmaStatement;
						if (iwarningDisableRestorePragmaStatement == null)
						{
							builtExprement = this._factory.CreatePragmaStatement(pragma.Text);
						}
						else
						{
							builtExprement = this._factory.CreateWarningDisableRestorePragmaStatement(iwarningDisableRestorePragmaStatement.Restore, iwarningDisableRestorePragmaStatement.Id, pragma.Text);
						}
					}
					else
					{
						ITreeFactory7 treeFactory = this._factory as ITreeFactory7;
						if (treeFactory != null)
						{
							builtExprement = treeFactory.CreateLocalSignatureIdPragma(ilocalSignatureIdPragma.LocalSignatureId, pragma.Text);
						}
						else
						{
							builtExprement = this._factory.CreatePragmaStatement(pragma.Text);
						}
					}
				}
				else
				{
					ITreeFactory7 treeFactory2 = this._factory as ITreeFactory7;
					if (treeFactory2 != null)
					{
						builtExprement = treeFactory2.CreateImplicitCodeSectionPragma(iimplicitCodeSectionPragma.ImplicitOn, pragma.Text);
					}
					else
					{
						builtExprement = this._factory.CreatePragmaStatement(pragma.Text);
					}
				}
			}
			else
			{
				builtExprement = this._factory.CreateMessageGuidPragmaStatement(imessageGuidPragmaStatement.MessageGuid, pragma.Text);
			}
			this._builtExprement = builtExprement;
		}

		// Token: 0x060003FC RID: 1020 RVA: 0x00007D5C File Offset: 0x00005F5C
		public void visit(_IExpressionStatement expstat)
		{
			expstat._Expr.Accept(this);
			_IExpression builtExpression = this.BuiltExpression;
			_IExpressionStatement builtExprement = this._factory.CreateExpressionStatement(builtExpression);
			this._builtExprement = builtExprement;
		}

		// Token: 0x060003FD RID: 1021 RVA: 0x00007D90 File Offset: 0x00005F90
		public virtual void visit(_IAssignmentExpression assign)
		{
			Operator kindOf = assign.KindOf;
			assign._LValue.Accept(this);
			_IExpression builtExpression = this.BuiltExpression;
			assign._RValue.Accept(this);
			_IExpression builtExpression2 = this.BuiltExpression;
			_IAssignmentExpression builtExprement = this._factory.CreateAssignmentExpression(builtExpression, builtExpression2, kindOf);
			this._builtExprement = builtExprement;
		}

		// Token: 0x060003FE RID: 1022 RVA: 0x00007DE0 File Offset: 0x00005FE0
		public void visit(_ICallExpression call)
		{
			_IExpression[] array = new _IExpression[call.ParamExpressions.Count];
			_IExpression[] array2 = new _IExpression[call.Inputs.Count];
			_IExpression[] array3 = new _IExpression[call.OutputExpressions.Count];
			_IExpression[] array4 = new _IExpression[call.Outputs.Count];
			_IExpression[] array5 = new _IExpression[call.EmptyAssigns.Count];
			call._Callee.Accept(this);
			_IExpression builtExpression = this.BuiltExpression;
			_IExpression expCondition = null;
			_IType typeExpected = null;
			if (call._Condition != null)
			{
				call._Condition.Accept(this);
				expCondition = this.BuiltExpression;
			}
			if (call.ExpectedType != null)
			{
				typeExpected = call.ExpectedType.Duplicate;
			}
			if (call.ParamExpressions.Count > 0)
			{
				for (int i = 0; i < call.ParamExpressions.Count; i++)
				{
					call.ParamExpressions[i].Accept(this);
					array[i] = this.BuiltExpression;
				}
			}
			if (call.Inputs.Count > 0)
			{
				for (int j = 0; j < call.Inputs.Count; j++)
				{
					_IExpression iexpression = call.Inputs[j];
					if (iexpression != null)
					{
						iexpression.Accept(this);
						iexpression = this.BuiltExpression;
					}
					array2[j] = iexpression;
				}
			}
			if (call.Outputs.Count > 0)
			{
				for (int k = 0; k < call.Outputs.Count; k++)
				{
					call.Outputs[k].Accept(this);
					array4[k] = this.BuiltExpression;
				}
			}
			if (call.OutputExpressions.Count > 0)
			{
				for (int l = 0; l < call.OutputExpressions.Count; l++)
				{
					_IExpression iexpression2 = call.OutputExpressions[l];
					if (iexpression2 != null)
					{
						call.OutputExpressions[l].Accept(this);
						iexpression2 = this.BuiltExpression;
					}
					array3[l] = iexpression2;
				}
			}
			for (int m = 0; m < call.EmptyAssigns.Count; m++)
			{
				call.EmptyAssigns[m].Accept(this);
				array5[m] = this.BuiltExpression;
			}
			_ICallExpression builtExprement = this._factory.CreateCallExpression(builtExpression, expCondition, typeExpected, array, array2, array3, array4, array5);
			this._builtExprement = builtExprement;
		}

		// Token: 0x060003FF RID: 1023 RVA: 0x00008020 File Offset: 0x00006220
		public virtual void visit(_IOperatorExpression opx)
		{
			IList<_IExpression> operandsList = opx._OperandsList;
			_IExpression[] array = new _IExpression[operandsList.Count];
			int num = 0;
			foreach (_IExpression iexpression in operandsList)
			{
				iexpression.Accept(this);
				array[num++] = this.BuiltExpression;
			}
			_IOperatorExpression builtExprement = this._factory.CreateOperatorExpression(opx.Code, array);
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000400 RID: 1024 RVA: 0x000080A0 File Offset: 0x000062A0
		public void visit(_IConversionExpression conv)
		{
			conv._Exp.Accept(this);
			_IExpression builtExpression = this.BuiltExpression;
			_IConversionExpression builtExprement = this._factory.CreateConversionExpression(conv.From, conv.To, builtExpression);
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000401 RID: 1025 RVA: 0x000080E0 File Offset: 0x000062E0
		public virtual void visit(_IThisExpression thisexp)
		{
			_IThisExpression builtExprement = this._factory.CreateThisExpression();
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000402 RID: 1026 RVA: 0x00008100 File Offset: 0x00006300
		public virtual void visit(_IBaseExpression baseexp)
		{
			_IBaseExpression builtExprement = this._factory.CreateBaseExpression();
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000403 RID: 1027 RVA: 0x00008120 File Offset: 0x00006320
		public virtual void visit(_ILiteralExpression literal)
		{
			_ILiteralExpression builtExprement = null;
			if (literal is _IIntegerLiteralExpression && literal.Base != 10)
			{
				builtExprement = this._factory.CreateBasedIntegerLiteralExpression(literal.LongValue, literal.ConstantType, literal.Base, literal.Negative);
			}
			else if (literal is _IIntegerLiteralExpression)
			{
				builtExprement = this._factory.CreateIntegerLiteralExpression(literal.LongValue, literal.ConstantType, literal.Negative);
			}
			else if (literal is _IFloatLiteralExpression)
			{
				builtExprement = this._factory.CreateFloatLiteralExpression(literal.RealValue, literal.ConstantType);
			}
			else if (literal is _IStringLiteralExpression)
			{
				builtExprement = (this._factory as ITreeFactory3).CreateStringLiteralExpression(literal.StringValue, literal.ConstantType, (literal as _IStringLiteralExpression2).StringEncoding);
			}
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000404 RID: 1028 RVA: 0x000081E8 File Offset: 0x000063E8
		public void visit(_IAddressExpression address)
		{
			_IAddressExpression builtExprement = this._factory.CreateAddressExpression(address.DirectAddress);
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000405 RID: 1029 RVA: 0x00008210 File Offset: 0x00006410
		public virtual void visit(_IVariableExpression variable)
		{
			_IVariableExpression builtExprement = this._factory.CreateVariableExpression(variable.Name);
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000406 RID: 1030 RVA: 0x00008238 File Offset: 0x00006438
		public virtual void visit(_IIndexAccessExpression indexaccess)
		{
			indexaccess._Var.Accept(this);
			_IExpression builtExpression = this.BuiltExpression;
			_IExpression[] array = new _IExpression[indexaccess.Accesses.Length];
			for (int i = 0; i < indexaccess.Accesses.Length; i++)
			{
				(indexaccess.Accesses[i] as _IExpression).Accept(this);
				array[i] = this.BuiltExpression;
			}
			_IIndexAccessExpression builtExprement = this._factory.CreateIndexAccessExpression(builtExpression, array);
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000407 RID: 1031 RVA: 0x000082AC File Offset: 0x000064AC
		public virtual void visit(_ICompoAccessExpression compo)
		{
			compo._Left.Accept(this);
			_IExpression builtExpression = this.BuiltExpression;
			compo._Right.Accept(this);
			_IExpression builtExpression2 = this.BuiltExpression;
			_ICompoAccessExpression builtExprement = this._factory.CreateCompoAccessExpression(builtExpression, builtExpression2);
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000408 RID: 1032 RVA: 0x000082F4 File Offset: 0x000064F4
		public virtual void visit(_IDeRefAccessExpression deref)
		{
			deref._Base.Accept(this);
			_IExpression builtExpression = this.BuiltExpression;
			_IDeRefAccessExpression builtExprement = this._factory.CreateDeRefAccessExpression(builtExpression);
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000409 RID: 1033 RVA: 0x00008328 File Offset: 0x00006528
		public void visit(_ICopyScopeExpression copyexp)
		{
			throw new NotImplementedException();
		}

		// Token: 0x0600040A RID: 1034 RVA: 0x00008330 File Offset: 0x00006530
		public void visit(_IGlobalScopeExpression globexp)
		{
			globexp._Base.Accept(this);
			_IExpression builtExpression = this.BuiltExpression;
			_IGlobalScopeExpression builtExprement = this._factory.CreateGlobalScopeExpression(builtExpression);
			this._builtExprement = builtExprement;
		}

		// Token: 0x0600040B RID: 1035 RVA: 0x00008364 File Offset: 0x00006564
		public void visit(_ISystemScopeExpression systemscope)
		{
			systemscope._Base.Accept(this);
			_IExpression builtExpression = this.BuiltExpression;
			_ISystemScopeExpression builtExprement = this._factory.CreateSystemScopeExpression(builtExpression);
			this._builtExprement = builtExprement;
		}

		// Token: 0x0600040C RID: 1036 RVA: 0x00008398 File Offset: 0x00006598
		public virtual void visit(_IEmptyStatement empty)
		{
			_IEmptyStatement builtExprement = this._factory.CreateEmptyStatement();
			this._builtExprement = builtExprement;
		}

		// Token: 0x0600040D RID: 1037 RVA: 0x000083B8 File Offset: 0x000065B8
		public void visit(_ICaseRangeExpression caserange)
		{
			caserange._High.Accept(this);
			_IExpression builtExpression = this.BuiltExpression;
			caserange._Low.Accept(this);
			_IExpression builtExpression2 = this.BuiltExpression;
			_ICaseRangeExpression builtExprement = this._factory.CreateCaseRangeExpression(builtExpression2, builtExpression);
			this._builtExprement = builtExprement;
		}

		// Token: 0x0600040E RID: 1038 RVA: 0x00008400 File Offset: 0x00006600
		public void visit(_ICaseLabelStatement caselabel)
		{
			_IExpression[] array = new _IExpression[caselabel._cases.Count];
			for (int i = 0; i < caselabel._cases.Count; i++)
			{
				caselabel._cases[i].Accept(this);
				array[i] = this.BuiltExpression;
			}
			_ICaseLabelStatement builtExprement = this._factory.CreateCaseLabelStatement(array);
			this._builtExprement = builtExprement;
		}

		// Token: 0x0600040F RID: 1039 RVA: 0x00008464 File Offset: 0x00006664
		public void visit(_ICaseStatement casest)
		{
			casest._Switch.Accept(this);
			_IExpression builtExpression = this.BuiltExpression;
			_ICase[] array = new _ICase[casest._Cases.Count];
			for (int i = 0; i < casest._Cases.Count; i++)
			{
				_ICase icase = casest._Cases[i];
				icase._Label.Accept(this);
				_ICaseLabelStatement caselabel = this._builtExprement as _ICaseLabelStatement;
				icase._Controlled.Accept(this);
				_IStatement builtStatement = this.BuiltStatement;
				_ICase icase2 = this._factory.CreateCase(caselabel, builtStatement);
				array[i] = icase2;
			}
			_IStatement elsecase = null;
			if (casest._Else != null)
			{
				casest._Else.Accept(this);
				elsecase = this.BuiltStatement;
			}
			_ICaseStatement builtExprement = this._factory.CreateCaseStatement(builtExpression, array, elsecase);
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000410 RID: 1040 RVA: 0x00008534 File Offset: 0x00006734
		public void visit(_IErrorExpression errorexp)
		{
			_IErrorExpression builtExprement = this._factory.CreateErrorExpression();
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000411 RID: 1041 RVA: 0x00008554 File Offset: 0x00006754
		public void visit(_IErrorStatement errorst)
		{
			_IErrorStatement builtExprement = this._factory.CreateErrorStatement();
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000412 RID: 1042 RVA: 0x00008574 File Offset: 0x00006774
		public void visit(_INullExpression errorexp)
		{
			_INullExpression builtExprement = this._factory.CreateNullExpression();
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000413 RID: 1043 RVA: 0x00008594 File Offset: 0x00006794
		public void visit(_INullStatement errorst)
		{
			_INullStatement builtExprement = this._factory.CreateNullStatement();
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000414 RID: 1044 RVA: 0x000085B4 File Offset: 0x000067B4
		public void visit(_IQualifiedNameExpression qne)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000415 RID: 1045 RVA: 0x000085BC File Offset: 0x000067BC
		public void visit(_IVariableDeclarationStatement vds)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000416 RID: 1046 RVA: 0x000085C4 File Offset: 0x000067C4
		public void visit(_IVariableDeclarationListStatement vdls)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000417 RID: 1047 RVA: 0x000085CC File Offset: 0x000067CC
		public void visit(_IPOUDeclarationStatement pds)
		{
			_IErrorStatement builtExprement = this._factory.CreateErrorStatement();
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000418 RID: 1048 RVA: 0x000085EC File Offset: 0x000067EC
		public void visit(_ITypeDeclarationStatement tds)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000419 RID: 1049 RVA: 0x000085F4 File Offset: 0x000067F4
		public void visit(_IEnumDeclarationStatement eds)
		{
			throw new NotImplementedException();
		}

		// Token: 0x0600041A RID: 1050 RVA: 0x000085FC File Offset: 0x000067FC
		public void visit(_IEnumDeclarationListStatement eds)
		{
			throw new NotImplementedException();
		}

		// Token: 0x0600041B RID: 1051 RVA: 0x00008604 File Offset: 0x00006804
		public void visit(_IMultipleIndexInitialization errorst)
		{
			errorst._Number.Accept(this);
			_IExpression builtExpression = this.BuiltExpression;
			errorst._Value.Accept(this);
			_IExpression builtExpression2 = this.BuiltExpression;
			_IMultipleIndexInitialization builtExprement = this._factory.CreateMultipleIndexInitialisation(builtExpression2, builtExpression);
			this._builtExprement = builtExprement;
		}

		// Token: 0x0600041C RID: 1052 RVA: 0x0000864C File Offset: 0x0000684C
		public void visit(_IArrayInitialization errorexp)
		{
			LList<_IExpression> llist = new LList<_IExpression>();
			foreach (_IExpression iexpression in errorexp._InitValues)
			{
				iexpression.Accept(this);
				llist.Add(this.BuiltExpression);
			}
			_IArrayInitialization builtExprement = this._factory.CreateArrayInitialisation(llist);
			this._builtExprement = builtExprement;
		}

		// Token: 0x0600041D RID: 1053 RVA: 0x000086C0 File Offset: 0x000068C0
		public void visit(_IStructureInitialization errorst)
		{
			LList<_IAssignmentExpression> llist = new LList<_IAssignmentExpression>();
			foreach (_IAssignmentExpression iassignmentExpression in errorst._CompoInits)
			{
				iassignmentExpression.Accept(this);
				llist.Add(this.BuiltExpression as _IAssignmentExpression);
			}
			_IStructureInitialization builtExprement = this._factory.CreateStructureInitialisation(llist);
			this._builtExprement = builtExprement;
		}

		// Token: 0x0600041E RID: 1054 RVA: 0x00008738 File Offset: 0x00006938
		public void visit(_IDefineReference defref)
		{
			_IDefineReference builtExprement = this._factory.CreateDefineReference(defref.Define);
			this._builtExprement = builtExprement;
		}

		// Token: 0x0600041F RID: 1055 RVA: 0x00008760 File Offset: 0x00006960
		public void visit(_IVariableReference varref)
		{
			varref.InstancePath.Accept(this);
			_IVariableReference builtExprement = this._factory.CreateVariableReference(this.BuiltExpression);
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000420 RID: 1056 RVA: 0x00008794 File Offset: 0x00006994
		public void visit(_ITypeReference typeref)
		{
			typeref.InstancePath.Accept(this);
			_ITypeReference builtExprement = this._factory.CreateTypeReference(this.BuiltExpression);
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000421 RID: 1057 RVA: 0x000087C8 File Offset: 0x000069C8
		public void visit(_IPouReference pouref)
		{
			pouref.InstancePath.Accept(this);
			_IPouReference builtExprement = this._factory.CreatePouReference(this.BuiltExpression);
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000422 RID: 1058 RVA: 0x000087FC File Offset: 0x000069FC
		public void visit(_ITaskReference taskref)
		{
			_ITaskReference builtExprement = this._factory.CreateTaskReference(taskref.TaskName);
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000423 RID: 1059 RVA: 0x00008824 File Offset: 0x00006A24
		public void visit(_IResourceReference resref)
		{
			_IResourceReference builtExprement = this._factory.CreateResourceReference(resref.ResourceName);
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000424 RID: 1060 RVA: 0x0000884C File Offset: 0x00006A4C
		public void visit(_IDefinedExpression defexp)
		{
			defexp.ItemReference.Accept(this);
			_IDefinedExpression builtExprement = this._factory.CreateDefinedExpression(this.BuiltExpression as _IItemReference);
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000425 RID: 1061 RVA: 0x00008884 File Offset: 0x00006A84
		public void visit(_IXRefExpression xref)
		{
			xref.XRef.Accept(this);
			_IExpression builtExpression = this.BuiltExpression;
			xref.XRefFrom.Accept(this);
			_IExpression builtExpression2 = this.BuiltExpression;
			_IXRefExpression builtExprement = this._factory.CreateXRefExpression(builtExpression as _IItemReference, builtExpression2 as _IItemReference);
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000426 RID: 1062 RVA: 0x000088D8 File Offset: 0x00006AD8
		public void visit(_ICompilerVersionExpression compiversionexp)
		{
			_ICompilerVersionExpression builtExprement = this._factory.CreateCompilerVersionExpression(compiversionexp.VersionToTest, compiversionexp.OpComparison);
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000427 RID: 1063 RVA: 0x00008904 File Offset: 0x00006B04
		public void visit(_IPragmaOperatorExpression popexp)
		{
			_IExpression[] array = new _IExpression[popexp.Operands.Count];
			for (int i = 0; i < popexp.Operands.Count; i++)
			{
				popexp.Operands[i].Accept(this);
				array[i] = this.BuiltExpression;
			}
			_IPragmaOperatorExpression builtExprement = this._factory.CreatePragmaOperatorExpression(popexp.Code, array);
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000428 RID: 1064 RVA: 0x00008970 File Offset: 0x00006B70
		public void visit(_IPragmaIfStatement pifst)
		{
			pifst.Condition.Accept(this);
			_IExpression builtExpression = this.BuiltExpression;
			pifst.IfThen.Accept(this);
			_IStatement builtStatement = this.BuiltStatement;
			_IStatement ifelse = null;
			_IPragmaElseIf[] array = null;
			if (pifst.ElseIf.Count > 0)
			{
				array = new _IPragmaElseIf[pifst.ElseIf.Count];
				for (int i = 0; i < pifst.ElseIf.Count; i++)
				{
					_IPragmaElseIf ipragmaElseIf = pifst.ElseIf[i];
					ipragmaElseIf.Condition.Accept(this);
					_IExpression builtExpression2 = this.BuiltExpression;
					ipragmaElseIf.Controlled.Accept(this);
					_IStatement builtStatement2 = this.BuiltStatement;
					_IPragmaElseIf ipragmaElseIf2 = this._factory.CreatePragmaElseIf(builtExpression2 as _IPragmaExpression, builtStatement2);
					if (ipragmaElseIf2.Condition is _INullExpression)
					{
						_IErrorExpression ierrorExpression = ipragmaElseIf.Condition as _IErrorExpression;
						if (ierrorExpression != null)
						{
							ipragmaElseIf2.Condition = (_IErrorExpression)ierrorExpression.Duplicate();
						}
					}
					array[i] = ipragmaElseIf2;
				}
			}
			if (pifst.IfElse != null)
			{
				pifst.IfElse.Accept(this);
				ifelse = this.BuiltStatement;
			}
			_IPragmaIfStatement builtExprement = this._factory.CreatePragmaIfStatement(builtExpression, builtStatement, ifelse, array);
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000429 RID: 1065 RVA: 0x00008AA8 File Offset: 0x00006CA8
		public void visit(_IBreakPointStatement bpstate)
		{
			ITreeFactory2 treeFactory = this._factory as ITreeFactory2;
			if (treeFactory != null)
			{
				this._builtExprement = treeFactory.CreateBreakPointStatement(bpstate.BPPosition, bpstate.SuccessorPosition);
				return;
			}
			throw new NotImplementedException();
		}

		// Token: 0x0600042A RID: 1066 RVA: 0x00008AE4 File Offset: 0x00006CE4
		public void visit(_IDefineStatement defstate)
		{
			_IDefineStatement builtExprement = this._factory.CreateDefineStatement(defstate.Define, defstate.Value, defstate.Ident);
			this._builtExprement = builtExprement;
		}

		// Token: 0x0600042B RID: 1067 RVA: 0x00008B18 File Offset: 0x00006D18
		public void visit(_IHasCompatibleTypeExpression hastype)
		{
			hastype.Variable.Accept(this);
			_IHasCompatibleTypeExpression builtExprement = this._factory.CreateHasCompatibleTypeExpression(this.BuiltExpression as _IVariableReference, ((_IType)hastype.ReferencedType).Duplicate);
			this._builtExprement = builtExprement;
		}

		// Token: 0x0600042C RID: 1068 RVA: 0x00008B60 File Offset: 0x00006D60
		public void visit(_IHasTypeExpression hastype)
		{
			if (hastype is _IHasCompatibleTypeExpression)
			{
				this.visit((_IHasCompatibleTypeExpression)hastype);
				return;
			}
			hastype.Variable.Accept(this);
			_IHasTypeExpression builtExprement = this._factory.CreateHasTypeExpression(this.BuiltExpression as _IVariableReference, ((_IType)hastype.ReferencedType).Duplicate);
			this._builtExprement = builtExprement;
		}

		// Token: 0x0600042D RID: 1069 RVA: 0x00008BBC File Offset: 0x00006DBC
		public void visit(_IIsEnumTypeExpression isenumtype)
		{
			_IIsEnumTypeExpression builtExprement = this._factory.CreateIsEnumTypeExpression(((_IType)isenumtype.ReferencedType).Duplicate);
			this._builtExprement = builtExprement;
		}

		// Token: 0x0600042E RID: 1070 RVA: 0x00008BEC File Offset: 0x00006DEC
		public void visit(_IHasAttributeExpression hasattribute)
		{
			hasattribute.ItemReference.Accept(this);
			_IHasAttributeExpression builtExprement = this._factory.CreateHasAttributeExpression(this.BuiltExpression as _IItemReference, hasattribute.Attribute);
			this._builtExprement = builtExprement;
		}

		// Token: 0x0600042F RID: 1071 RVA: 0x00008C2C File Offset: 0x00006E2C
		public void visit(_IHasValueExpression hasvalue)
		{
			_IHasValueExpression builtExprement = this._factory.CreateHasValueExpression(hasvalue.Define, hasvalue.DefineValue);
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000430 RID: 1072 RVA: 0x00008C58 File Offset: 0x00006E58
		public void visit(_IHasConstantValueExpression hasvalue)
		{
			hasvalue._Constant.Accept(this);
			_IExpression builtExpression = this.BuiltExpression;
			hasvalue._ConstantValue.Accept(this);
			_IExpression builtExpression2 = this.BuiltExpression;
			Operator opComparison = hasvalue.OpComparison;
			_IHasConstantValueExpression builtExprement = this._factory.CreateHasConstantValueExpression(builtExpression, builtExpression2, opComparison);
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000431 RID: 1073 RVA: 0x00008CA8 File Offset: 0x00006EA8
		public void visit(_IHasConstantTypeExpression hasConstantTypeExpression)
		{
			hasConstantTypeExpression._Constant.Accept(this);
			_IExpression builtExpression = this.BuiltExpression;
			_IHasConstantTypeExpression builtExprement = (this._factory as ITreeFactory3).CreateHasConstantTypeExpression(builtExpression, hasConstantTypeExpression._ConstantTypeReplaced);
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000432 RID: 1074 RVA: 0x00008CE8 File Offset: 0x00006EE8
		public void visit(_IPragmaAssertion assertion)
		{
			assertion.Condition.Accept(this);
			_IExpression builtExpression = this.BuiltExpression;
			string errorOutput = assertion.ErrorOutput;
			_IPragmaAssertion builtExprement = this._factory.CreatePragmaAssertion(builtExpression, errorOutput);
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000433 RID: 1075 RVA: 0x00008D24 File Offset: 0x00006F24
		public void visit(_ICastExpression castexp)
		{
			castexp.BaseExpression.Accept(this);
			_IExpression builtExpression = this.BuiltExpression;
			_IExpression expWithType = null;
			if (castexp.ExpWithType != null)
			{
				castexp.ExpWithType.Accept(this);
				expWithType = this.BuiltExpression;
			}
			_IType duplicate = ((_IType)castexp.ExplicitelySpecifiedType).Duplicate;
			_ICastExpression builtExprement = this._factory.CreateCastExpression(expWithType, builtExpression, duplicate);
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000434 RID: 1076 RVA: 0x00008D88 File Offset: 0x00006F88
		public void visit(_INewExpression newexp)
		{
			_IExpression expCount = null;
			IAssignmentExpression[] array = null;
			if (newexp._Count != null)
			{
				newexp._Count.Accept(this);
				expCount = this.BuiltExpression;
			}
			if (newexp._FBInitParams != null)
			{
				array = new IAssignmentExpression[newexp._FBInitParams.Count];
				for (int i = 0; i < newexp._FBInitParams.Count; i++)
				{
					(newexp._FBInitParams[i] as _IExpression).Accept(this);
					array[i] = (this.BuiltExpression as IAssignmentExpression);
				}
			}
			_IType typeToCast = newexp._TypeToCast;
			_IType typeIn = (typeToCast != null) ? typeToCast.Duplicate : null;
			_INewExpression builtExprement = this._factory.CreateNewExpression(typeIn, expCount, array);
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000435 RID: 1077 RVA: 0x00008E38 File Offset: 0x00007038
		public void visit(_ITypeExpression typeexp)
		{
			_ITypeExpression builtExprement = this._factory.CreateTypeExpression(typeexp.ExpressionType);
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000436 RID: 1078 RVA: 0x00008E60 File Offset: 0x00007060
		public void visit(_INamespaceAccessExpression namespaceaccess)
		{
			namespaceaccess._Access.Accept(this);
			_IExpression builtExpression = this.BuiltExpression;
			namespaceaccess._Namespace.Accept(this);
			_IExpression builtExpression2 = this.BuiltExpression;
			_INamespaceAccessExpression builtExprement = this._factory.CreateNamespaceAccessExpression(builtExpression2, builtExpression);
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000437 RID: 1079 RVA: 0x00008EA8 File Offset: 0x000070A8
		public void visit(_IRuntimeVersionExpression runtimeversionexp)
		{
			_IRuntimeVersionExpression builtExprement = this._factory.CreateRuntimeVersionExpression(runtimeversionexp.VersionToTest, runtimeversionexp.OpComparison);
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000438 RID: 1080 RVA: 0x00008ED4 File Offset: 0x000070D4
		public void visit(_ICurrentTaskExpression currentTask)
		{
			currentTask._Base.Accept(this);
			_ICurrentTaskExpression builtExprement = this._factory.CreateCurrentTaskExpression(this.BuiltExpression);
			this._builtExprement = builtExprement;
		}

		// Token: 0x06000439 RID: 1081 RVA: 0x00008F08 File Offset: 0x00007108
		public void visit(_IPoolScopeExpression poolscope)
		{
			poolscope._Base.Accept(this);
			_IPoolScopeExpression builtExprement = this._factory.CreatePoolScopeExpression(this.BuiltExpression);
			this._builtExprement = builtExprement;
		}

		// Token: 0x0600043A RID: 1082 RVA: 0x00008F3C File Offset: 0x0000713C
		public void visit(_ITryCatchStatement trycatchstatement)
		{
			_ISequenceStatement seqCatch = null;
			_ISequenceStatement seqFinally = null;
			_IExpression expException = null;
			trycatchstatement._Try.Accept(this);
			_ISequenceStatement seqTry = this.BuiltStatement as _ISequenceStatement;
			if (trycatchstatement._Catch != null)
			{
				trycatchstatement._Catch.Accept(this);
				seqCatch = (this.BuiltStatement as _ISequenceStatement);
			}
			if (trycatchstatement._Exception != null)
			{
				trycatchstatement._Exception.Accept(this);
				expException = this.BuiltExpression;
			}
			if (trycatchstatement._Finally != null)
			{
				trycatchstatement._Finally.Accept(this);
				seqFinally = (this.BuiltStatement as _ISequenceStatement);
			}
			_ITryCatchStatement builtExprement = this._factory.CreateTryCatchStatement(seqTry, seqCatch, seqFinally, expException);
			this._builtExprement = builtExprement;
		}

		// Token: 0x0600043B RID: 1083 RVA: 0x00008FE0 File Offset: 0x000071E0
		public void visit(_IPartialAccessExpression partialAccessExpression)
		{
			partialAccessExpression._Left.Accept(this);
			_IExpression builtExpression = this.BuiltExpression;
			this._builtExprement = ((ITreeFactory4)this._factory).CreatePartialAccessExpression(builtExpression, partialAccessExpression.PartSize, partialAccessExpression.PartOffset);
		}

		// Token: 0x0600043C RID: 1084 RVA: 0x00009024 File Offset: 0x00007224
		public void visit(_IProjectDefinedExpression projectDefinedExpression)
		{
			ITreeFactory5 treeFactory = this._factory as ITreeFactory5;
			if (treeFactory != null)
			{
				_IDefineReference defineReference = projectDefinedExpression.DefineReference;
				if (defineReference != null)
				{
					defineReference.Accept(this);
				}
				_IProjectDefinedExpression builtExprement = treeFactory.CreateProjectDefinedExpression(this.BuiltExpression as _IDefineReference);
				this._builtExprement = builtExprement;
			}
		}

		// Token: 0x04000037 RID: 55
		protected _IExprement _builtExprement;

		// Token: 0x04000038 RID: 56
		protected ITreeFactory _factory;
	}
}
