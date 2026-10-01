using System;
using System.Collections.Generic;
using System.Linq;
using \u0019;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace \u0080
{
	// Token: 0x02000178 RID: 376
	internal sealed class \u000E : IExprementVisitor3590, IExprementVisitor
	{
		// Token: 0x06001968 RID: 6504 RVA: 0x0004F6F4 File Offset: 0x0004D8F4
		internal \u000E(CaseInsensitiveDictionary<_ICompoAccessExpression> \u0082\u0003)
		{
			this.\u0001 = \u0082\u0003;
		}

		// Token: 0x06001969 RID: 6505 RVA: 0x0004F710 File Offset: 0x0004D910
		private static CaseInsensitiveDictionary<_ICompoAccessExpression> \u0001(_ISignature \u0002, _IScope \u0003)
		{
			CaseInsensitiveDictionary<_ICompoAccessExpression> caseInsensitiveDictionary = new CaseInsensitiveDictionary<_ICompoAccessExpression>();
			foreach (_IVariable ivariable in \u0002.All)
			{
				if (ivariable.HasAttribute(CompileAttributes.ATTRIBUTE_IMPLICIT_ENUM_TYPE))
				{
					string stName = string.Empty;
					_IType itype = null;
					if (ivariable._Type is _IArrayType)
					{
						itype = (ivariable._Type as _IArrayType)._Base;
					}
					else if (ivariable._Type is _IEnumType)
					{
						itype = ivariable._Type;
					}
					else if (ivariable._Type is _IUserdefType)
					{
						itype = ivariable._Type;
					}
					if (itype != null)
					{
						stName = itype.ToString();
						ISignature[] array = \u0003.FindSignature(stName);
						if (array != null && array.Length == 1)
						{
							_IVariableExpression u = \u0019.\u0003.\u0001(array[0].OrgName);
							foreach (_IVariable ivariable2 in array[0].All)
							{
								_IVariableExpression u2 = \u0019.\u0003.\u0001(ivariable2.OrgName);
								_ICompoAccessExpression icompoAccessExpression = \u0019.\u0003.\u0001(u, u2);
								caseInsensitiveDictionary[ivariable2.Name] = icompoAccessExpression;
							}
						}
					}
				}
			}
			return caseInsensitiveDictionary;
		}

		// Token: 0x0600196A RID: 6506 RVA: 0x0004F838 File Offset: 0x0004DA38
		internal static void \u0001(_ICompileContext \u0002, _IExprement \u0003, int \u0004, _IScope \u0005)
		{
			_ISignature isignature = \u0002[\u0004];
			_ISignature isignature2 = null;
			if (isignature != null && isignature.ParentSignatureId != Helper.InvalidId)
			{
				isignature2 = \u0002[isignature.ParentSignatureId];
			}
			if (isignature != null && isignature.HasAttribute("contains_implicit_enum"))
			{
				\u0080.\u000E.\u0001(\u0003, isignature, \u0005);
			}
			if (isignature2 != null && isignature2.HasAttribute("contains_implicit_enum"))
			{
				\u0080.\u000E.\u0001(\u0003, isignature2, \u0005);
			}
		}

		// Token: 0x0600196B RID: 6507 RVA: 0x0004F89C File Offset: 0x0004DA9C
		internal static void \u0001(_ICompiledPOU \u0002, _ISignature \u0003, _IScope \u0004)
		{
			\u0080.\u000E.\u0001(\u0002.GetParseTree(), \u0003, \u0004);
		}

		// Token: 0x0600196C RID: 6508 RVA: 0x0004F8AC File Offset: 0x0004DAAC
		private static void \u0001(_IExprement \u0002, _ISignature \u0003, _IScope \u0004)
		{
			\u0080.\u000E ivisit = new \u0080.\u000E(\u0080.\u000E.\u0001(\u0003, \u0004));
			\u0002.Accept(ivisit);
		}

		// Token: 0x0600196D RID: 6509 RVA: 0x0004F8D0 File Offset: 0x0004DAD0
		internal static void \u0001(_ISignature \u0002, \u0081.\u0008 \u0003)
		{
			\u0080.\u000E u000E = new \u0080.\u000E(\u0080.\u000E.\u0001(\u0002, \u0003));
			foreach (_IVariable ivariable in \u0002.AllVariables)
			{
				if (ivariable.Initial != null)
				{
					u000E.\u0001.Push(null);
					(ivariable.Initial as _IExprement).Accept(u000E);
					_IExpression iexpression = u000E.\u0001.Pop();
					if (iexpression != null)
					{
						ivariable.Initial = iexpression;
					}
				}
			}
		}

		// Token: 0x0600196E RID: 6510 RVA: 0x0004F960 File Offset: 0x0004DB60
		public void \u0001(_IVariableExpression \u0002)
		{
			if (this.\u0001.ContainsKey(\u0002.Name))
			{
				Debug.\u0001(this.\u0001.Peek() == null);
				this.\u0001.Pop();
				_IExpression iexpression = this.\u0001[\u0002.Name].Duplicate() as _IExpression;
				iexpression._Position = \u0002._Position;
				this.\u0001.Push(iexpression);
			}
		}

		// Token: 0x0600196F RID: 6511 RVA: 0x0004F9D4 File Offset: 0x0004DBD4
		public void \u0001(_ICompiledPOU \u0002)
		{
			\u0002.GetParseTree().Accept(this);
		}

		// Token: 0x06001970 RID: 6512 RVA: 0x0004F9E4 File Offset: 0x0004DBE4
		public void \u0001(_IRepeatStatement \u0002)
		{
			this.\u0001.Push(null);
			\u0002._Condition.Accept(this);
			_IExpression iexpression = this.\u0001.Pop();
			if (iexpression != null)
			{
				\u0002._Condition = iexpression;
			}
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x06001971 RID: 6513 RVA: 0x0004FA2C File Offset: 0x0004DC2C
		public void \u0001(_ISequenceStatement \u0002)
		{
			foreach (_IStatement istatement in \u0002._StatementList)
			{
				istatement.Accept(this);
			}
		}

		// Token: 0x06001972 RID: 6514 RVA: 0x0004FA78 File Offset: 0x0004DC78
		public void \u0001(_IIfStatement \u0002)
		{
			this.\u0001.Push(null);
			\u0002._Condition.Accept(this);
			_IExpression iexpression = this.\u0001.Pop();
			if (iexpression != null)
			{
				\u0002._Condition = iexpression;
			}
			\u0002._IfThen.Accept(this);
			foreach (_IElseIf ielseIf in \u0002._ElseIf)
			{
				this.\u0001.Push(null);
				ielseIf._Condition.Accept(this);
				_IExpression iexpression2 = this.\u0001.Pop();
				if (iexpression2 != null)
				{
					ielseIf._Condition = iexpression2;
				}
				ielseIf._Controlled.Accept(this);
			}
			if (\u0002._IfElse != null)
			{
				\u0002._IfElse.Accept(this);
			}
		}

		// Token: 0x06001973 RID: 6515 RVA: 0x0004FB48 File Offset: 0x0004DD48
		public void \u0001(_IJumpStatement \u0002)
		{
			if (\u0002._Condition != null)
			{
				this.\u0001.Push(null);
				\u0002._Condition.Accept(this);
				_IExpression iexpression = this.\u0001.Pop();
				if (iexpression != null)
				{
					\u0002._Condition = iexpression;
				}
			}
		}

		// Token: 0x06001974 RID: 6516 RVA: 0x0004FB8C File Offset: 0x0004DD8C
		public void \u0001(_IExpressionStatement \u0002)
		{
			this.\u0001.Push(null);
			\u0002._Expr.Accept(this);
			_IExpression iexpression = this.\u0001.Pop();
			if (iexpression != null)
			{
				\u0002._Expr = iexpression;
			}
		}

		// Token: 0x06001975 RID: 6517 RVA: 0x0004FBC8 File Offset: 0x0004DDC8
		public void \u0001(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			for (int i = 0; i < operandsList.Count; i++)
			{
				_IExprement iexprement = operandsList[i];
				this.\u0001.Push(null);
				iexprement.Accept(this);
				_IExpression iexpression = this.\u0001.Pop();
				if (iexpression != null)
				{
					\u0002[i] = iexpression;
				}
			}
		}

		// Token: 0x06001976 RID: 6518 RVA: 0x0004FC20 File Offset: 0x0004DE20
		public void \u0001(_ICaseRangeExpression \u0002)
		{
			this.\u0001.Push(null);
			\u0002._Low.Accept(this);
			_IExpression iexpression = this.\u0001.Pop();
			if (iexpression != null)
			{
				\u0002._Low = iexpression;
			}
			this.\u0001.Push(null);
			\u0002._High.Accept(this);
			iexpression = this.\u0001.Pop();
			if (iexpression != null)
			{
				\u0002._High = iexpression;
			}
		}

		// Token: 0x06001977 RID: 6519 RVA: 0x0004FC8C File Offset: 0x0004DE8C
		public void \u0001(_ICaseStatement \u0002)
		{
			this.\u0001.Push(null);
			\u0002._Switch.Accept(this);
			_IExpression iexpression = this.\u0001.Pop();
			if (iexpression != null)
			{
				\u0002._Switch = iexpression;
			}
			foreach (_ICase icase in \u0002._Cases)
			{
				icase._Label.Accept(this);
				icase._Controlled.Accept(this);
			}
			if (\u0002._Else != null)
			{
				\u0002._Else.Accept(this);
			}
		}

		// Token: 0x06001978 RID: 6520 RVA: 0x0004FD2C File Offset: 0x0004DF2C
		public void \u0001(_ICaseLabelStatement \u0002)
		{
			IList<_IExpression> cases = \u0002._cases;
			for (int i = 0; i < cases.Count; i++)
			{
				_IExprement iexprement = cases[i];
				this.\u0001.Push(null);
				iexprement.Accept(this);
				_IExpression iexpression = this.\u0001.Pop();
				if (iexpression != null)
				{
					\u0002[i] = iexpression;
				}
			}
		}

		// Token: 0x06001979 RID: 6521 RVA: 0x0004FD84 File Offset: 0x0004DF84
		public void \u0001(_IIndexAccessExpression \u0002)
		{
			for (int i = 0; i < \u0002.NumAccesses; i++)
			{
				this.\u0001.Push(null);
				\u0002.GetAccess(i).Accept(this);
				_IExpression iexpression = this.\u0001.Pop();
				if (iexpression != null)
				{
					\u0002[i] = iexpression;
				}
			}
		}

		// Token: 0x0600197A RID: 6522 RVA: 0x0004FDD4 File Offset: 0x0004DFD4
		public void \u0001(_IConversionExpression \u0002)
		{
			this.\u0001.Push(null);
			\u0002._Exp.Accept(this);
			_IExpression iexpression = this.\u0001.Pop();
			if (iexpression != null)
			{
				\u0002._Exp = iexpression;
			}
		}

		// Token: 0x0600197B RID: 6523 RVA: 0x0004FE10 File Offset: 0x0004E010
		public void \u0001(_ICallExpression \u0002)
		{
			IList<_IExpression> paramExpressions = \u0002.ParamExpressions;
			for (int i = 0; i < paramExpressions.Count<_IExpression>(); i++)
			{
				this.\u0001.Push(null);
				paramExpressions[i].Accept(this);
				_IExpression iexpression = this.\u0001.Pop();
				if (iexpression != null)
				{
					\u0002[i] = iexpression;
				}
			}
		}

		// Token: 0x0600197C RID: 6524 RVA: 0x0004FE68 File Offset: 0x0004E068
		public void \u0001(_IAssignmentExpression \u0002)
		{
			this.\u0001.Push(null);
			\u0002._RValue.Accept(this);
			_IExpression iexpression = this.\u0001.Pop();
			if (iexpression != null)
			{
				\u0002._RValue = iexpression;
			}
			this.\u0001.Push(null);
			\u0002._LValue.Accept(this);
			this.\u0001.Pop();
		}

		// Token: 0x0600197D RID: 6525 RVA: 0x0004FEC8 File Offset: 0x0004E0C8
		public void \u0001(_IForStatement \u0002)
		{
			\u0002._CounterStart.Accept(this);
			this.\u0001.Push(null);
			\u0002._UpperBound.Accept(this);
			_IExpression iexpression = this.\u0001.Pop();
			if (iexpression != null)
			{
				\u0002._UpperBound = iexpression;
			}
			if (\u0002.By != null)
			{
				this.\u0001.Push(null);
				\u0002._By.Accept(this);
				if (iexpression != null)
				{
					\u0002._By = iexpression;
				}
			}
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x0600197E RID: 6526 RVA: 0x0004FF48 File Offset: 0x0004E148
		public void \u0001(_IWhileStatement \u0002)
		{
			this.\u0001.Push(null);
			\u0002._Condition.Accept(this);
			_IExpression iexpression = this.\u0001.Pop();
			if (iexpression != null)
			{
				\u0002._Condition = iexpression;
			}
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x0600197F RID: 6527 RVA: 0x0004FF90 File Offset: 0x0004E190
		public void \u0001(_IArrayInitialization \u0002)
		{
			for (int i = 0; i < \u0002._InitValues.Count; i++)
			{
				_IExpression iexpression = \u0002._InitValues[i];
				this.\u0001.Push(null);
				iexpression.Accept(this);
				iexpression = this.\u0001.Pop();
				if (iexpression != null)
				{
					\u0002._InitValues[i] = iexpression;
				}
			}
		}

		// Token: 0x06001980 RID: 6528 RVA: 0x0004FFF0 File Offset: 0x0004E1F0
		public void \u0001(_IMultipleIndexInitialization \u0002)
		{
			this.\u0001.Push(null);
			\u0002._Number.Accept(this);
			_IExpression iexpression = this.\u0001.Pop();
			if (iexpression != null)
			{
				\u0002._Number = iexpression;
			}
			this.\u0001.Push(null);
			\u0002._Value.Accept(this);
			iexpression = this.\u0001.Pop();
			if (iexpression != null)
			{
				\u0002._Value = iexpression;
			}
		}

		// Token: 0x06001981 RID: 6529 RVA: 0x0005005C File Offset: 0x0004E25C
		public void \u0001(_IPragmaStatement \u0002)
		{
		}

		// Token: 0x06001982 RID: 6530 RVA: 0x00050060 File Offset: 0x0004E260
		public void \u0001(_ILabelStatement \u0002)
		{
		}

		// Token: 0x06001983 RID: 6531 RVA: 0x00050064 File Offset: 0x0004E264
		public void \u0001(_IReturnStatement \u0002)
		{
		}

		// Token: 0x06001984 RID: 6532 RVA: 0x00050068 File Offset: 0x0004E268
		public void \u0001(_IContinueStatement \u0002)
		{
		}

		// Token: 0x06001985 RID: 6533 RVA: 0x0005006C File Offset: 0x0004E26C
		public void \u0001(_IEmptyStatement \u0002)
		{
		}

		// Token: 0x06001986 RID: 6534 RVA: 0x00050070 File Offset: 0x0004E270
		public void \u0001(_IGlobalScopeExpression \u0002)
		{
		}

		// Token: 0x06001987 RID: 6535 RVA: 0x00050074 File Offset: 0x0004E274
		public void \u0001(_IDeRefAccessExpression \u0002)
		{
		}

		// Token: 0x06001988 RID: 6536 RVA: 0x00050078 File Offset: 0x0004E278
		public void \u0001(_IAddressExpression \u0002)
		{
		}

		// Token: 0x06001989 RID: 6537 RVA: 0x0005007C File Offset: 0x0004E27C
		public void \u0001(_IBaseExpression \u0002)
		{
		}

		// Token: 0x0600198A RID: 6538 RVA: 0x00050080 File Offset: 0x0004E280
		public void \u0001(_IExitStatement \u0002)
		{
		}

		// Token: 0x0600198B RID: 6539 RVA: 0x00050084 File Offset: 0x0004E284
		public void \u0001(_IPoolScopeExpression \u0002)
		{
		}

		// Token: 0x0600198C RID: 6540 RVA: 0x00050088 File Offset: 0x0004E288
		public void \u0001(_INamespaceAccessExpression \u0002)
		{
		}

		// Token: 0x0600198D RID: 6541 RVA: 0x0005008C File Offset: 0x0004E28C
		public void \u0001(_ICommentStatement \u0002)
		{
		}

		// Token: 0x0600198E RID: 6542 RVA: 0x00050090 File Offset: 0x0004E290
		public void \u0001(_IThisExpression \u0002)
		{
		}

		// Token: 0x0600198F RID: 6543 RVA: 0x00050094 File Offset: 0x0004E294
		public void \u0001(_ILiteralExpression \u0002)
		{
		}

		// Token: 0x06001990 RID: 6544 RVA: 0x00050098 File Offset: 0x0004E298
		public void \u0001(_ICompoAccessExpression \u0002)
		{
		}

		// Token: 0x06001991 RID: 6545 RVA: 0x0005009C File Offset: 0x0004E29C
		public void \u0001(_ICopyScopeExpression \u0002)
		{
		}

		// Token: 0x06001992 RID: 6546 RVA: 0x000500A0 File Offset: 0x0004E2A0
		public void \u0001(_ISystemScopeExpression \u0002)
		{
		}

		// Token: 0x06001993 RID: 6547 RVA: 0x000500A4 File Offset: 0x0004E2A4
		public void \u0001(_IErrorStatement \u0002)
		{
		}

		// Token: 0x06001994 RID: 6548 RVA: 0x000500A8 File Offset: 0x0004E2A8
		public void \u0001(_INullStatement \u0002)
		{
		}

		// Token: 0x06001995 RID: 6549 RVA: 0x000500AC File Offset: 0x0004E2AC
		public void \u0001(_IVariableDeclarationStatement \u0002)
		{
		}

		// Token: 0x06001996 RID: 6550 RVA: 0x000500B0 File Offset: 0x0004E2B0
		public void \u0001(_IPOUDeclarationStatement \u0002)
		{
		}

		// Token: 0x06001997 RID: 6551 RVA: 0x000500B4 File Offset: 0x0004E2B4
		public void \u0001(_IEnumDeclarationStatement \u0002)
		{
		}

		// Token: 0x06001998 RID: 6552 RVA: 0x000500B8 File Offset: 0x0004E2B8
		public void \u0001(_IStructureInitialization \u0002)
		{
		}

		// Token: 0x06001999 RID: 6553 RVA: 0x000500BC File Offset: 0x0004E2BC
		public void \u0001(_IVariableReference \u0002)
		{
		}

		// Token: 0x0600199A RID: 6554 RVA: 0x000500C0 File Offset: 0x0004E2C0
		public void \u0001(_IPouReference \u0002)
		{
		}

		// Token: 0x0600199B RID: 6555 RVA: 0x000500C4 File Offset: 0x0004E2C4
		public void \u0001(_IResourceReference \u0002)
		{
		}

		// Token: 0x0600199C RID: 6556 RVA: 0x000500C8 File Offset: 0x0004E2C8
		public void \u0001(_IPragmaOperatorExpression \u0002)
		{
		}

		// Token: 0x0600199D RID: 6557 RVA: 0x000500CC File Offset: 0x0004E2CC
		public void \u0001(_IBreakPointStatement \u0002)
		{
		}

		// Token: 0x0600199E RID: 6558 RVA: 0x000500D0 File Offset: 0x0004E2D0
		public void \u0001(_IXRefExpression \u0002)
		{
		}

		// Token: 0x0600199F RID: 6559 RVA: 0x000500D4 File Offset: 0x0004E2D4
		public void \u0001(_IIsEnumTypeExpression \u0002)
		{
		}

		// Token: 0x060019A0 RID: 6560 RVA: 0x000500D8 File Offset: 0x0004E2D8
		public void \u0001(_IHasValueExpression \u0002)
		{
		}

		// Token: 0x060019A1 RID: 6561 RVA: 0x000500DC File Offset: 0x0004E2DC
		public void \u0001(_IPragmaAssertion \u0002)
		{
		}

		// Token: 0x060019A2 RID: 6562 RVA: 0x000500E0 File Offset: 0x0004E2E0
		public void \u0001(_ICastExpression \u0002)
		{
		}

		// Token: 0x060019A3 RID: 6563 RVA: 0x000500E4 File Offset: 0x0004E2E4
		public void \u0001(_ITypeExpression \u0002)
		{
		}

		// Token: 0x060019A4 RID: 6564 RVA: 0x000500E8 File Offset: 0x0004E2E8
		public void \u0001(_INewExpression \u0002)
		{
		}

		// Token: 0x060019A5 RID: 6565 RVA: 0x000500EC File Offset: 0x0004E2EC
		public void \u0001(_ICompilerVersionExpression \u0002)
		{
		}

		// Token: 0x060019A6 RID: 6566 RVA: 0x000500F0 File Offset: 0x0004E2F0
		public void \u0001(_IHasConstantValueExpression \u0002)
		{
		}

		// Token: 0x060019A7 RID: 6567 RVA: 0x000500F4 File Offset: 0x0004E2F4
		public void \u0001(_IHasAttributeExpression \u0002)
		{
		}

		// Token: 0x060019A8 RID: 6568 RVA: 0x000500F8 File Offset: 0x0004E2F8
		public void \u0001(_IHasTypeExpression \u0002)
		{
		}

		// Token: 0x060019A9 RID: 6569 RVA: 0x000500FC File Offset: 0x0004E2FC
		public void \u0001(_IDefineStatement \u0002)
		{
		}

		// Token: 0x060019AA RID: 6570 RVA: 0x00050100 File Offset: 0x0004E300
		public void \u0001(_IPragmaIfStatement \u0002)
		{
		}

		// Token: 0x060019AB RID: 6571 RVA: 0x00050104 File Offset: 0x0004E304
		public void \u0001(_IDefinedExpression \u0002)
		{
		}

		// Token: 0x060019AC RID: 6572 RVA: 0x00050108 File Offset: 0x0004E308
		public void \u0001(_ITaskReference \u0002)
		{
		}

		// Token: 0x060019AD RID: 6573 RVA: 0x0005010C File Offset: 0x0004E30C
		public void \u0001(_ITypeReference \u0002)
		{
		}

		// Token: 0x060019AE RID: 6574 RVA: 0x00050110 File Offset: 0x0004E310
		public void \u0001(_IDefineReference \u0002)
		{
		}

		// Token: 0x060019AF RID: 6575 RVA: 0x00050114 File Offset: 0x0004E314
		public void \u0001(_IEnumDeclarationListStatement \u0002)
		{
		}

		// Token: 0x060019B0 RID: 6576 RVA: 0x00050118 File Offset: 0x0004E318
		public void \u0001(_ITypeDeclarationStatement \u0002)
		{
		}

		// Token: 0x060019B1 RID: 6577 RVA: 0x0005011C File Offset: 0x0004E31C
		public void \u0001(_IVariableDeclarationListStatement \u0002)
		{
		}

		// Token: 0x060019B2 RID: 6578 RVA: 0x00050120 File Offset: 0x0004E320
		public void \u0001(_IQualifiedNameExpression \u0002)
		{
		}

		// Token: 0x060019B3 RID: 6579 RVA: 0x00050124 File Offset: 0x0004E324
		public void \u0001(_INullExpression \u0002)
		{
		}

		// Token: 0x060019B4 RID: 6580 RVA: 0x00050128 File Offset: 0x0004E328
		public void \u0001(_IErrorExpression \u0002)
		{
		}

		// Token: 0x04000473 RID: 1139
		private readonly CaseInsensitiveDictionary<_ICompoAccessExpression> \u0001;

		// Token: 0x04000474 RID: 1140
		private readonly Stack<_IExpression> \u0001 = new Stack<_IExpression>();
	}
}
