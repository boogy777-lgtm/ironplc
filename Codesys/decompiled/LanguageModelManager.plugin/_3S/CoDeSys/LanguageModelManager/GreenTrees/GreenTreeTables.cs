using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.CommonCompilerData;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000232 RID: 562
	public class GreenTreeTables : IGreenTreeTables
	{
		// Token: 0x17000A61 RID: 2657
		// (get) Token: 0x060024F0 RID: 9456 RVA: 0x0005CCA0 File Offset: 0x0005BCA0
		public object TableLock { get; } = new object();

		// Token: 0x17000A62 RID: 2658
		// (get) Token: 0x060024F1 RID: 9457 RVA: 0x0005CCA8 File Offset: 0x0005BCA8
		public IDictionary<string, _IVariableExpression> VarTable { get; } = new LDictionary<string, _IVariableExpression>();

		// Token: 0x17000A63 RID: 2659
		// (get) Token: 0x060024F2 RID: 9458 RVA: 0x0005CCB0 File Offset: 0x0005BCB0
		public IDictionary<long, _ILiteralExpression> IntegerTable { get; } = new LDictionary<long, _ILiteralExpression>();

		// Token: 0x17000A64 RID: 2660
		// (get) Token: 0x060024F3 RID: 9459 RVA: 0x0005CCB8 File Offset: 0x0005BCB8
		public IDictionary<uint, _IExprement> ExprTable { get; } = new LDictionary<uint, _IExprement>();

		// Token: 0x17000A65 RID: 2661
		// (get) Token: 0x060024F4 RID: 9460 RVA: 0x0005CCC0 File Offset: 0x0005BCC0
		public _ILiteralExpression TrueExpression { get; } = new IntegerLiteralExpression_Green(1L, TypeClass.Bool);

		// Token: 0x17000A66 RID: 2662
		// (get) Token: 0x060024F5 RID: 9461 RVA: 0x0005CCC8 File Offset: 0x0005BCC8
		public _ILiteralExpression FalseExpression { get; } = new IntegerLiteralExpression_Green(0L, TypeClass.Bool);

		// Token: 0x17000A67 RID: 2663
		// (get) Token: 0x060024F6 RID: 9462 RVA: 0x0005CCD0 File Offset: 0x0005BCD0
		public _IEmptyStatement EmptyStatement { get; } = new EmptyStatement_Green();

		// Token: 0x17000A68 RID: 2664
		// (get) Token: 0x060024F7 RID: 9463 RVA: 0x0005CCD8 File Offset: 0x0005BCD8
		public _IExitStatement ExitStatement { get; } = new ExitStatement_Green();

		// Token: 0x17000A69 RID: 2665
		// (get) Token: 0x060024F8 RID: 9464 RVA: 0x0005CCE0 File Offset: 0x0005BCE0
		public _IContinueStatement ContinueStatement { get; } = new ContinueStatement_Green();

		// Token: 0x17000A6A RID: 2666
		// (get) Token: 0x060024F9 RID: 9465 RVA: 0x0005CCE8 File Offset: 0x0005BCE8
		public _IReturnStatement UnconditionalReturnStatement { get; } = new ReturnStatement_Green(null);

		// Token: 0x17000A6B RID: 2667
		// (get) Token: 0x060024FA RID: 9466 RVA: 0x0005CCF0 File Offset: 0x0005BCF0
		public _IThisExpression ThisExpression { get; } = new ThisExpression_Green();

		// Token: 0x17000A6C RID: 2668
		// (get) Token: 0x060024FB RID: 9467 RVA: 0x0005CCF8 File Offset: 0x0005BCF8
		public _IBaseExpression BaseExpression { get; } = new BaseExpression_Green();

		// Token: 0x060024FC RID: 9468 RVA: 0x0005CD00 File Offset: 0x0005BD00
		private static uint GetHashCode(_IExprement expr)
		{
			ICheckSumVisitor checkSumVisitor = CompilerProxy.CreateChecksumVisitor(true);
			expr.Accept(checkSumVisitor.Traverser);
			return checkSumVisitor.Checksum;
		}

		// Token: 0x060024FD RID: 9469 RVA: 0x0005CD26 File Offset: 0x0005BD26
		private static bool IsEqual(_IExprement expr1, _IExprement expr2)
		{
			return expr1.ToString() == expr2.ToString();
		}

		// Token: 0x060024FE RID: 9470 RVA: 0x0005CD3C File Offset: 0x0005BD3C
		public void Clear()
		{
			object tableLock = this.TableLock;
			lock (tableLock)
			{
				this.VarTable.Clear();
				this.IntegerTable.Clear();
				this.ExprTable.Clear();
			}
		}

		// Token: 0x060024FF RID: 9471 RVA: 0x0005CD98 File Offset: 0x0005BD98
		private _IExprement GetHashedExprement(_IExprement expr, out bool bConflict, out uint hash)
		{
			hash = GreenTreeTables.GetHashCode(expr);
			bConflict = false;
			_IExprement iexprement;
			if (!this.ExprTable.TryGetValue(hash, out iexprement))
			{
				return iexprement;
			}
			if (GreenTreeTables.IsEqual(iexprement, expr))
			{
				return iexprement;
			}
			bConflict = true;
			return null;
		}

		// Token: 0x06002500 RID: 9472 RVA: 0x0005CDD2 File Offset: 0x0005BDD2
		public void AddExprementToHashTable(_IExprement expr, uint hash)
		{
			this.ExprTable.Add(hash, expr);
		}

		// Token: 0x06002501 RID: 9473 RVA: 0x0005CDE4 File Offset: 0x0005BDE4
		public _ICompoAccessExpression GetHashedGreenCompoAccess(_ICompoAccessExpression compo, out bool bConflict, out uint hash)
		{
			_IExprement hashedExprement = this.GetHashedExprement(compo, out bConflict, out hash);
			Debug.Assert(hashedExprement == null || hashedExprement is CompoAccessExpression_Green);
			return hashedExprement as CompoAccessExpression_Green;
		}

		// Token: 0x06002502 RID: 9474 RVA: 0x0005CE18 File Offset: 0x0005BE18
		public _IOperatorExpression GetHashedGreenOperatorExpression(_IOperatorExpression op, out bool bConflict, out uint hash)
		{
			_IExprement hashedExprement = this.GetHashedExprement(op, out bConflict, out hash);
			Debug.Assert(hashedExprement == null || hashedExprement is OperatorExpression_Green);
			return hashedExprement as OperatorExpression_Green;
		}

		// Token: 0x06002503 RID: 9475 RVA: 0x0005CE4C File Offset: 0x0005BE4C
		public _IAssignmentExpression GetHashedGreenAssignmentExpression(_IAssignmentExpression assign, out bool bConflict, out uint hash)
		{
			_IExprement hashedExprement = this.GetHashedExprement(assign, out bConflict, out hash);
			Debug.Assert(hashedExprement == null || hashedExprement is AssignmentExpression_Green);
			return hashedExprement as AssignmentExpression_Green;
		}

		// Token: 0x06002504 RID: 9476 RVA: 0x0005CE80 File Offset: 0x0005BE80
		public _IIndexAccessExpression GetHashedGreenIndexAccessExpression(_IIndexAccessExpression assign, out bool bConflict, out uint hash)
		{
			_IExprement hashedExprement = this.GetHashedExprement(assign, out bConflict, out hash);
			Debug.Assert(hashedExprement == null || hashedExprement is IndexAccessExpression_Green);
			return hashedExprement as IndexAccessExpression_Green;
		}

		// Token: 0x06002505 RID: 9477 RVA: 0x0005CEB4 File Offset: 0x0005BEB4
		public _IDeRefAccessExpression GetHashedGreenDerefAccessExpression(_IDeRefAccessExpression assign, out bool bConflict, out uint hash)
		{
			_IExprement hashedExprement = this.GetHashedExprement(assign, out bConflict, out hash);
			Debug.Assert(hashedExprement == null || hashedExprement is DeRefAccessExpression_Green);
			return hashedExprement as DeRefAccessExpression_Green;
		}
	}
}
