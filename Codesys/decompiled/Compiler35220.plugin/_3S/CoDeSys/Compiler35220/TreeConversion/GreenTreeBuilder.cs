using System;
using System.Collections.Generic;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.TreeConversion
{
	// Token: 0x02000013 RID: 19
	public class GreenTreeBuilder : RedTreeBuilder
	{
		// Token: 0x060003DB RID: 987 RVA: 0x0000730C File Offset: 0x0000550C
		private GreenTreeBuilder(IGreenTreeTables tables, ITreeFactory factory) : base(factory)
		{
			this.\u0001 = tables;
		}

		// Token: 0x060003DC RID: 988 RVA: 0x0000731C File Offset: 0x0000551C
		public static _IExprement BuildGreenTree(_IExprement exp, IGreenTreeTables tables, ITreeFactory factory, ICompactedParseTreeInformation parseTreeInfo)
		{
			GreenTreeBuilder greenTreeBuilder = new GreenTreeBuilder(tables, factory);
			exp.Accept(greenTreeBuilder);
			_IExprement builtExprement = greenTreeBuilder._builtExprement;
			PrecompileParseTreeInformationCollector.CollectParseTreeInformation(exp, parseTreeInfo);
			return builtExprement;
		}

		// Token: 0x060003DD RID: 989 RVA: 0x00007348 File Offset: 0x00005548
		public bool CheckLeaf(IExpression exp)
		{
			return exp is ILiteralExpression || exp is IVariableExpression;
		}

		// Token: 0x060003DE RID: 990 RVA: 0x00007360 File Offset: 0x00005560
		public override void visit(_IExitStatement exit)
		{
			this._builtExprement = this.\u0001.ExitStatement;
		}

		// Token: 0x060003DF RID: 991 RVA: 0x00007374 File Offset: 0x00005574
		public override void visit(_IContinueStatement cont)
		{
			this._builtExprement = this.\u0001.ContinueStatement;
		}

		// Token: 0x060003E0 RID: 992 RVA: 0x00007388 File Offset: 0x00005588
		public override void visit(_IEmptyStatement emptystatement)
		{
			this._builtExprement = this.\u0001.EmptyStatement;
		}

		// Token: 0x060003E1 RID: 993 RVA: 0x0000739C File Offset: 0x0000559C
		public override void visit(_IReturnStatement returnst)
		{
			_IReturnStatement builtExprement;
			if (returnst._Condition != null)
			{
				returnst._Condition.Accept(this);
				_IExpression builtExpression = base.BuiltExpression;
				builtExprement = this._factory.CreateReturnStatement(builtExpression);
			}
			else
			{
				builtExprement = this.\u0001.UnconditionalReturnStatement;
			}
			this._builtExprement = builtExprement;
		}

		// Token: 0x060003E2 RID: 994 RVA: 0x000073E8 File Offset: 0x000055E8
		public override void visit(_IAssignmentExpression assign)
		{
			base.visit(assign);
			_IAssignmentExpression iassignmentExpression = this._builtExprement as _IAssignmentExpression;
			if (this.CheckLeaf(assign.LValue) && this.CheckLeaf(assign.RValue))
			{
				bool flag;
				uint hash;
				_IAssignmentExpression hashedGreenAssignmentExpression = this.\u0001.GetHashedGreenAssignmentExpression(assign, out flag, out hash);
				if (hashedGreenAssignmentExpression != null)
				{
					iassignmentExpression = hashedGreenAssignmentExpression;
				}
				else if (!flag)
				{
					this.\u0001.AddExprementToHashTable(iassignmentExpression, hash);
				}
			}
			this._builtExprement = iassignmentExpression;
		}

		// Token: 0x060003E3 RID: 995 RVA: 0x00007454 File Offset: 0x00005654
		public override void visit(_IOperatorExpression op)
		{
			base.visit(op);
			_IOperatorExpression ioperatorExpression = this._builtExprement as _IOperatorExpression;
			IList<_IExpression> operandsList = op._OperandsList;
			if (op._OperandsList.Count == 2 && this.CheckLeaf(operandsList[0]) && this.CheckLeaf(operandsList[1]))
			{
				bool flag;
				uint hash;
				_IOperatorExpression hashedGreenOperatorExpression = this.\u0001.GetHashedGreenOperatorExpression(op, out flag, out hash);
				if (hashedGreenOperatorExpression != null)
				{
					ioperatorExpression = hashedGreenOperatorExpression;
				}
				else if (!flag)
				{
					this.\u0001.AddExprementToHashTable(ioperatorExpression, hash);
				}
			}
			this._builtExprement = ioperatorExpression;
		}

		// Token: 0x060003E4 RID: 996 RVA: 0x000074DC File Offset: 0x000056DC
		public override void visit(_IThisExpression thisexp)
		{
			_IThisExpression thisExpression = this.\u0001.ThisExpression;
			this._builtExprement = thisExpression;
		}

		// Token: 0x060003E5 RID: 997 RVA: 0x000074FC File Offset: 0x000056FC
		public override void visit(_IBaseExpression baseexp)
		{
			_IBaseExpression baseExpression = this.\u0001.BaseExpression;
			this._builtExprement = baseExpression;
		}

		// Token: 0x060003E6 RID: 998 RVA: 0x0000751C File Offset: 0x0000571C
		public override void visit(_ILiteralExpression literal)
		{
			_ILiteralExpression iliteralExpression = null;
			if (literal is _IIntegerLiteralExpression && literal.Base != 10)
			{
				iliteralExpression = this._factory.CreateBasedIntegerLiteralExpression(literal.LongValue, literal.ConstantType, literal.Base, literal.Negative);
			}
			else if (literal is _IIntegerLiteralExpression)
			{
				_ILiteralExpression iliteralExpression2;
				if (literal.ConstantType == TypeClass.Bool)
				{
					if (literal.LongValue == 0L)
					{
						iliteralExpression = this.\u0001.FalseExpression;
					}
					else
					{
						iliteralExpression = this.\u0001.TrueExpression;
					}
				}
				else if (literal.Negative || literal.ConstantType != TypeClass.AnyInt)
				{
					iliteralExpression = this._factory.CreateIntegerLiteralExpression(literal.LongValue, literal.ConstantType, literal.Negative);
				}
				else if (this.\u0001.IntegerTable.TryGetValue(literal.LongValue, out iliteralExpression2))
				{
					iliteralExpression = iliteralExpression2;
				}
				else
				{
					iliteralExpression = this._factory.CreateIntegerLiteralExpression(literal.LongValue, literal.ConstantType, literal.Negative);
					this.\u0001.IntegerTable.Add(literal.LongValue, iliteralExpression);
				}
			}
			else if (literal is _IFloatLiteralExpression)
			{
				iliteralExpression = this._factory.CreateFloatLiteralExpression(literal.RealValue, literal.ConstantType);
			}
			else if (literal is _IStringLiteralExpression)
			{
				iliteralExpression = (this._factory as ITreeFactory3).CreateStringLiteralExpression(literal.StringValue, literal.ConstantType, (literal as _IStringLiteralExpression2).StringEncoding);
			}
			else
			{
				Debug.\u0001(false);
			}
			this._builtExprement = iliteralExpression;
		}

		// Token: 0x060003E7 RID: 999 RVA: 0x00007694 File Offset: 0x00005894
		public override void visit(_IVariableExpression variable)
		{
			_IVariableExpression ivariableExpression = null;
			if (!this.\u0001.VarTable.TryGetValue(variable.Name, out ivariableExpression))
			{
				ivariableExpression = this._factory.CreateVariableExpression(variable.Name);
				this.\u0001.VarTable.Add(variable.Name, ivariableExpression);
			}
			this._builtExprement = ivariableExpression;
		}

		// Token: 0x060003E8 RID: 1000 RVA: 0x000076F0 File Offset: 0x000058F0
		public override void visit(_IIndexAccessExpression indexaccess)
		{
			base.visit(indexaccess);
			_IIndexAccessExpression iindexAccessExpression = this._builtExprement as _IIndexAccessExpression;
			IExpression[] accesses = indexaccess.Accesses;
			if (accesses.Length == 1 && this.CheckLeaf(accesses[0]) && this.CheckLeaf(indexaccess.Var))
			{
				bool flag;
				uint hash;
				_IIndexAccessExpression hashedGreenIndexAccessExpression = this.\u0001.GetHashedGreenIndexAccessExpression(indexaccess, out flag, out hash);
				if (hashedGreenIndexAccessExpression != null)
				{
					iindexAccessExpression = hashedGreenIndexAccessExpression;
				}
				else if (!flag)
				{
					this.\u0001.AddExprementToHashTable(iindexAccessExpression, hash);
				}
			}
			this._builtExprement = iindexAccessExpression;
		}

		// Token: 0x060003E9 RID: 1001 RVA: 0x00007768 File Offset: 0x00005968
		public override void visit(_ICompoAccessExpression compo)
		{
			base.visit(compo);
			_ICompoAccessExpression icompoAccessExpression = this._builtExprement as _ICompoAccessExpression;
			if (compo.Left is IVariableExpression)
			{
				bool flag;
				uint hash;
				_ICompoAccessExpression hashedGreenCompoAccess = this.\u0001.GetHashedGreenCompoAccess(compo, out flag, out hash);
				if (hashedGreenCompoAccess != null)
				{
					icompoAccessExpression = hashedGreenCompoAccess;
				}
				else if (!flag)
				{
					this.\u0001.AddExprementToHashTable(icompoAccessExpression, hash);
				}
			}
			this._builtExprement = icompoAccessExpression;
		}

		// Token: 0x060003EA RID: 1002 RVA: 0x000077C4 File Offset: 0x000059C4
		public override void visit(_IDeRefAccessExpression deref)
		{
			base.visit(deref);
			_IDeRefAccessExpression ideRefAccessExpression = this._builtExprement as _IDeRefAccessExpression;
			if (this.CheckLeaf(deref._Base))
			{
				bool flag;
				uint hash;
				_IDeRefAccessExpression hashedGreenDerefAccessExpression = this.\u0001.GetHashedGreenDerefAccessExpression(deref, out flag, out hash);
				if (hashedGreenDerefAccessExpression != null)
				{
					ideRefAccessExpression = hashedGreenDerefAccessExpression;
				}
				else if (!flag)
				{
					this.\u0001.AddExprementToHashTable(ideRefAccessExpression, hash);
				}
			}
			this._builtExprement = ideRefAccessExpression;
		}

		// Token: 0x04000036 RID: 54
		private readonly IGreenTreeTables \u0001;
	}
}
