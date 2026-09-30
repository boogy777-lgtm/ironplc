using System;
using System.Collections.Generic;
using System.Diagnostics;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001F9 RID: 505
	internal class CallExpression_Green : Expression_Green, _ICallExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, ICallExpression4, ICallExpression3, ICallExpression2, ICallExpression
	{
		// Token: 0x06002287 RID: 8839 RVA: 0x0005AD55 File Offset: 0x00059D55
		public CallExpression_Green(_IExpression expCallee, _IExpression expCondition, _IType typeExpected, _IExpression[] expActualParams, _IExpression[] expFormalParams, _IExpression[] expActualOutputs, _IExpression[] expFormalOutputs)
		{
			this.m_expCallee = expCallee;
			this.m_expCondition = expCondition;
			this.m_typeExpected = typeExpected;
			this.m_expActualParams = expActualParams;
			this.m_expFormalParams = expFormalParams;
			this.m_expActualOutputs = expActualOutputs;
			this.m_expFormalOutputs = expFormalOutputs;
		}

		// Token: 0x06002288 RID: 8840 RVA: 0x00005F0F File Offset: 0x00004F0F
		public IBreakpoint SetCallBreakpoint(int nOffset, byte bySize)
		{
			return null;
		}

		// Token: 0x1700099A RID: 2458
		// (get) Token: 0x06002289 RID: 8841 RVA: 0x00005F0F File Offset: 0x00004F0F
		public IBreakpoint CallBreakpoint
		{
			get
			{
				return null;
			}
		}

		// Token: 0x0600228A RID: 8842 RVA: 0x00005F0F File Offset: 0x00004F0F
		public IBreakpoint SetBeforeCallBreakpoint(int nOffset, byte bySize)
		{
			return null;
		}

		// Token: 0x1700099B RID: 2459
		// (get) Token: 0x0600228B RID: 8843 RVA: 0x00005F0F File Offset: 0x00004F0F
		public IBreakpoint BeforeCallBreakpoint
		{
			get
			{
				return null;
			}
		}

		// Token: 0x1700099C RID: 2460
		// (get) Token: 0x0600228C RID: 8844 RVA: 0x0005AD92 File Offset: 0x00059D92
		public KindOfCall KindOfCall
		{
			get
			{
				if (this.CallInfo != null)
				{
					return this.CallInfo.KindOfCall;
				}
				return KindOfCall.None;
			}
		}

		// Token: 0x1700099D RID: 2461
		// (get) Token: 0x0600228D RID: 8845 RVA: 0x0005ADA9 File Offset: 0x00059DA9
		public IExpression Callee
		{
			get
			{
				return this._Callee;
			}
		}

		// Token: 0x1700099E RID: 2462
		// (get) Token: 0x0600228E RID: 8846 RVA: 0x0005ADB1 File Offset: 0x00059DB1
		// (set) Token: 0x0600228F RID: 8847 RVA: 0x0005A471 File Offset: 0x00059471
		public _IExpression _Callee
		{
			get
			{
				if (this.m_expCallee == null)
				{
					return new NullExpression_Green();
				}
				return this.m_expCallee;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x1700099F RID: 2463
		// (get) Token: 0x06002290 RID: 8848 RVA: 0x0005ADC7 File Offset: 0x00059DC7
		public IList<_IExpression> Inputs
		{
			get
			{
				if (this.m_expFormalParams == null)
				{
					return Array.Empty<_IExpression>();
				}
				return this.m_expFormalParams;
			}
		}

		// Token: 0x170009A0 RID: 2464
		// (get) Token: 0x06002291 RID: 8849 RVA: 0x0005ADE0 File Offset: 0x00059DE0
		public IAssignmentExpression[] InputAssigns
		{
			get
			{
				return Enumerable.ToLList<_IAssignmentExpression>(this._InputAssigns).ToArray();
			}
		}

		// Token: 0x170009A1 RID: 2465
		// (get) Token: 0x06002292 RID: 8850 RVA: 0x0005AE00 File Offset: 0x00059E00
		public IAssignmentExpression[] OutputAssigns
		{
			get
			{
				return Enumerable.ToLList<_IAssignmentExpression>(this._OutputAssigns).ToArray();
			}
		}

		// Token: 0x170009A2 RID: 2466
		// (get) Token: 0x06002293 RID: 8851 RVA: 0x0005AE20 File Offset: 0x00059E20
		public IList<_IAssignmentExpression> _InputAssigns
		{
			get
			{
				if (this.m_expActualParams == null)
				{
					return Array.Empty<_IAssignmentExpression>();
				}
				_IAssignmentExpression[] array = new _IAssignmentExpression[this.m_expActualParams.Length];
				for (int i = 0; i < this.m_expActualParams.Length; i++)
				{
					array[i] = new AssignmentExpression_Green(this.m_expFormalParams[i], this.m_expActualParams[i], Operator.Assign);
				}
				return array;
			}
		}

		// Token: 0x06002294 RID: 8852 RVA: 0x0005A8D1 File Offset: 0x000598D1
		[Obsolete("no list access")]
		public void RemoveInputAt(int i)
		{
			Debug.Assert(false);
		}

		// Token: 0x170009A3 RID: 2467
		// (get) Token: 0x06002295 RID: 8853 RVA: 0x0005AE7C File Offset: 0x00059E7C
		[Obsolete("no list access")]
		public IList<_IAssignmentExpression> _OutputAssigns
		{
			get
			{
				if (this.m_expActualOutputs == null)
				{
					return Array.Empty<_IAssignmentExpression>();
				}
				_IAssignmentExpression[] array = new _IAssignmentExpression[this.m_expActualOutputs.Length];
				for (int i = 0; i < this.m_expActualOutputs.Length; i++)
				{
					array[i] = new AssignmentExpression_Green(this.m_expActualOutputs[i], this.m_expFormalOutputs[i], Operator.AssignOut);
					if (array[i]._RValue.Type != null && !TypeTable.IsBlock(array[i]._RValue.Type.Class))
					{
						IExpression rvalue = array[i]._RValue;
						CompilerProxy.MakeImplicitConversionIfNecessary(array[i]._RValue.Type, array[i]._LValue.Type, ref rvalue);
						array[i]._RValue = (rvalue as _IExpression);
					}
				}
				return array;
			}
		}

		// Token: 0x170009A4 RID: 2468
		// (get) Token: 0x06002296 RID: 8854 RVA: 0x0005AF3D File Offset: 0x00059F3D
		public IList<_IExpression> ParamExpressions
		{
			get
			{
				if (this.m_expActualParams == null)
				{
					return Array.Empty<_IExpression>();
				}
				return this.m_expActualParams;
			}
		}

		// Token: 0x170009A5 RID: 2469
		public _IExpression this[int i]
		{
			get
			{
				if (this.m_expActualParams == null || i < 0 || i >= this.m_expActualParams.Length)
				{
					return null;
				}
				return this.m_expActualParams[i];
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x170009A6 RID: 2470
		// (get) Token: 0x06002299 RID: 8857 RVA: 0x0005AF76 File Offset: 0x00059F76
		public IList<_IExpression> Outputs
		{
			get
			{
				if (this.m_expFormalOutputs == null)
				{
					return Array.Empty<_IExpression>();
				}
				return this.m_expFormalOutputs;
			}
		}

		// Token: 0x170009A7 RID: 2471
		// (get) Token: 0x0600229A RID: 8858 RVA: 0x0005AF8C File Offset: 0x00059F8C
		public IList<_IExpression> OutputExpressions
		{
			get
			{
				if (this.m_expActualOutputs == null)
				{
					return Array.Empty<_IExpression>();
				}
				return this.m_expActualOutputs;
			}
		}

		// Token: 0x170009A8 RID: 2472
		// (get) Token: 0x0600229B RID: 8859 RVA: 0x00005F0F File Offset: 0x00004F0F
		// (set) Token: 0x0600229C RID: 8860 RVA: 0x0005A471 File Offset: 0x00059471
		public ICallExprInfo CallInfo
		{
			get
			{
				return null;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x0600229D RID: 8861 RVA: 0x0005A8D1 File Offset: 0x000598D1
		[Obsolete("no list access")]
		public void InsertParam(int index, _IExpression exp, _IExpression expVariable)
		{
			Debug.Assert(false);
		}

		// Token: 0x0600229E RID: 8862 RVA: 0x0005A8D1 File Offset: 0x000598D1
		[Obsolete("no list access")]
		public void AddParam(_IExpression exp, _IExpression expVariable)
		{
			Debug.Assert(false);
		}

		// Token: 0x0600229F RID: 8863 RVA: 0x0005AFA2 File Offset: 0x00059FA2
		[Obsolete("no list access")]
		public void AddParam(_IExpression exp)
		{
			this.AddParam(exp, null);
		}

		// Token: 0x060022A0 RID: 8864 RVA: 0x0005A471 File Offset: 0x00059471
		public void SetFormalParam(_IExpression expInput, int iIndex)
		{
			throw new NotSupportedException();
		}

		// Token: 0x060022A1 RID: 8865 RVA: 0x0005A471 File Offset: 0x00059471
		public void SetActualParam(_IExpression expInput, int iIndex)
		{
			throw new NotSupportedException();
		}

		// Token: 0x060022A2 RID: 8866 RVA: 0x0005A471 File Offset: 0x00059471
		public void SetActualOutput(_IExpression exp, int iIndex)
		{
			throw new NotSupportedException();
		}

		// Token: 0x060022A3 RID: 8867 RVA: 0x0005A471 File Offset: 0x00059471
		public void SetFormalOutput(_IExpression exp, int iIndex)
		{
			throw new NotSupportedException();
		}

		// Token: 0x060022A4 RID: 8868 RVA: 0x0005A471 File Offset: 0x00059471
		[Obsolete("no list access")]
		public void AddOutput(_IExpression exp, _IExpression expVariable)
		{
			throw new NotSupportedException();
		}

		// Token: 0x060022A5 RID: 8869 RVA: 0x0005A471 File Offset: 0x00059471
		[Obsolete("no lists")]
		public void AddEmptyAssign(_IExpression exp)
		{
			throw new NotSupportedException();
		}

		// Token: 0x170009A9 RID: 2473
		// (get) Token: 0x060022A6 RID: 8870 RVA: 0x0005AFAC File Offset: 0x00059FAC
		public virtual IList<_IExpression> EmptyAssigns
		{
			get
			{
				return Array.Empty<_IExpression>();
			}
		}

		// Token: 0x060022A7 RID: 8871 RVA: 0x0005A471 File Offset: 0x00059471
		public virtual void SetEmptyAssign(_IExpression exp, int i)
		{
			throw new NotSupportedException();
		}

		// Token: 0x060022A8 RID: 8872 RVA: 0x0000BFFD File Offset: 0x0000AFFD
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060022A9 RID: 8873 RVA: 0x0000C00F File Offset: 0x0000B00F
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x170009AA RID: 2474
		// (get) Token: 0x060022AA RID: 8874 RVA: 0x0005AFB3 File Offset: 0x00059FB3
		public IExpression Condition
		{
			get
			{
				return this._Condition;
			}
		}

		// Token: 0x170009AB RID: 2475
		// (get) Token: 0x060022AB RID: 8875 RVA: 0x0005AFBB File Offset: 0x00059FBB
		// (set) Token: 0x060022AC RID: 8876 RVA: 0x0005A471 File Offset: 0x00059471
		public _IExpression _Condition
		{
			get
			{
				return this.m_expCondition;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x170009AC RID: 2476
		// (get) Token: 0x060022AD RID: 8877 RVA: 0x0005AFC3 File Offset: 0x00059FC3
		// (set) Token: 0x060022AE RID: 8878 RVA: 0x0005A471 File Offset: 0x00059471
		public _IType ExpectedType
		{
			get
			{
				return this.m_typeExpected;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x040006A3 RID: 1699
		protected _IExpression m_expCallee;

		// Token: 0x040006A4 RID: 1700
		protected _IExpression[] m_expActualParams;

		// Token: 0x040006A5 RID: 1701
		protected _IExpression[] m_expFormalParams;

		// Token: 0x040006A6 RID: 1702
		protected _IExpression[] m_expActualOutputs;

		// Token: 0x040006A7 RID: 1703
		protected _IExpression[] m_expFormalOutputs;

		// Token: 0x040006A8 RID: 1704
		protected _IExpression m_expCondition;

		// Token: 0x040006A9 RID: 1705
		protected _IType m_typeExpected;
	}
}
