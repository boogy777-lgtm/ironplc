using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000FE RID: 254
	internal class PropertyCallerSearcher : EmptyVisitor, IExprementVisitorNoTraversion351300, IExprementVisitorNoTraversion3590, IExprementVisitorNoTraversion
	{
		// Token: 0x17000551 RID: 1361
		// (get) Token: 0x06001261 RID: 4705 RVA: 0x000348A5 File Offset: 0x000338A5
		private ICompiledType CallerType
		{
			get
			{
				if (this._callerType == null && 2 <= this._typeStack.Count)
				{
					this._typeStack.Pop();
					this._callerType = this._typeStack.Peek();
				}
				return this._callerType;
			}
		}

		// Token: 0x06001262 RID: 4706 RVA: 0x000348E0 File Offset: 0x000338E0
		internal IUserdefType DetermineCallerType(IExpression expInstancePath)
		{
			this._trav = CompilerProxy.CreateStandardTraverser();
			this._trav.Reset(this);
			((_IExpression)expInstancePath).Accept(this._trav);
			IUserdefType userdefType = this.CallerType as IUserdefType;
			if (userdefType != null)
			{
				return userdefType;
			}
			IArrayType arrayType = this.CallerType as IArrayType;
			if (arrayType != null)
			{
				return this.DetermineCallerType(arrayType);
			}
			IPointerType pointerType = this.CallerType as IPointerType;
			if (pointerType != null)
			{
				return this.DetermineCallerType(pointerType);
			}
			IReferenceType referenceType = this.CallerType as IReferenceType;
			if (referenceType != null)
			{
				return this.DetermineCallerType(referenceType);
			}
			return null;
		}

		// Token: 0x06001263 RID: 4707 RVA: 0x0003496C File Offset: 0x0003396C
		private IUserdefType DetermineCallerType(IArrayType arrayType)
		{
			IUserdefType userdefType = arrayType.Base as IUserdefType;
			if (userdefType != null)
			{
				return userdefType;
			}
			IArrayType arrayType2 = arrayType.Base as IArrayType;
			if (arrayType2 != null)
			{
				return this.DetermineCallerType(arrayType2);
			}
			IPointerType pointerType = arrayType.Base as IPointerType;
			if (pointerType != null)
			{
				return this.DetermineCallerType(pointerType);
			}
			IReferenceType referenceType = arrayType.Base as IReferenceType;
			if (referenceType != null)
			{
				return this.DetermineCallerType(referenceType);
			}
			return null;
		}

		// Token: 0x06001264 RID: 4708 RVA: 0x000349D0 File Offset: 0x000339D0
		private IUserdefType DetermineCallerType(IPointerType pointerType)
		{
			IUserdefType userdefType = pointerType.Base as IUserdefType;
			if (userdefType != null)
			{
				return userdefType;
			}
			IArrayType arrayType = pointerType.Base as IArrayType;
			if (arrayType != null)
			{
				return this.DetermineCallerType(arrayType);
			}
			IPointerType pointerType2 = pointerType.Base as IPointerType;
			if (pointerType2 != null)
			{
				return this.DetermineCallerType(pointerType2);
			}
			IReferenceType referenceType = pointerType.Base as IReferenceType;
			if (referenceType != null)
			{
				return this.DetermineCallerType(referenceType);
			}
			return null;
		}

		// Token: 0x06001265 RID: 4709 RVA: 0x00034A34 File Offset: 0x00033A34
		private IUserdefType DetermineCallerType(IReferenceType refType)
		{
			IUserdefType userdefType = refType.Base as IUserdefType;
			if (userdefType != null)
			{
				return userdefType;
			}
			IArrayType arrayType = refType.Base as IArrayType;
			if (arrayType != null)
			{
				return this.DetermineCallerType(arrayType);
			}
			IPointerType pointerType = refType.Base as IPointerType;
			if (pointerType != null)
			{
				return this.DetermineCallerType(pointerType);
			}
			IReferenceType referenceType = refType.Base as IReferenceType;
			if (referenceType != null)
			{
				return this.DetermineCallerType(referenceType);
			}
			return null;
		}

		// Token: 0x06001266 RID: 4710 RVA: 0x00034A98 File Offset: 0x00033A98
		public override void visit(_IVariableExpression variable, AccessFlag access)
		{
			this._typeStack.Push(variable.Type);
		}

		// Token: 0x06001267 RID: 4711 RVA: 0x00034AAB File Offset: 0x00033AAB
		public override void visit(_ICompoAccessExpression compo, AccessFlag access)
		{
			((_IExpression)compo.Left).Accept(this._trav);
			((_IExpression)compo.Right).Accept(this._trav);
		}

		// Token: 0x06001268 RID: 4712 RVA: 0x00034AD9 File Offset: 0x00033AD9
		public override void visit(_IDeRefAccessExpression deref)
		{
			((_IExpression)deref.Base).Accept(this._trav);
		}

		// Token: 0x06001269 RID: 4713 RVA: 0x00034AF1 File Offset: 0x00033AF1
		public override void visit(_IIndexAccessExpression indexaccess)
		{
			((_IExpression)indexaccess.Var).Accept(this._trav);
		}

		// Token: 0x0600126A RID: 4714 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_ICurrentTaskExpression currentTaskExp)
		{
		}

		// Token: 0x0600126B RID: 4715 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IPoolScopeExpression poolscope)
		{
		}

		// Token: 0x04000459 RID: 1113
		private IStandardTraverser _trav;

		// Token: 0x0400045A RID: 1114
		private readonly Stack<ICompiledType> _typeStack = new Stack<ICompiledType>();

		// Token: 0x0400045B RID: 1115
		private ICompiledType _callerType;
	}
}
