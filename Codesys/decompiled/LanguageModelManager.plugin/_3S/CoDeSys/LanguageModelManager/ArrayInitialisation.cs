using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200003E RID: 62
	[TypeGuid("{1315f3d2-84a9-4bcd-a718-efa3d0501534}")]
	[StorageVersion("3.3.0.0")]
	public class ArrayInitialisation : PositionExpression, _IArrayInitialization, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IArrayInitialization
	{
		// Token: 0x06000311 RID: 785 RVA: 0x0000B4DE File Offset: 0x0000A4DE
		public ArrayInitialisation()
		{
		}

		// Token: 0x06000312 RID: 786 RVA: 0x0000B4F2 File Offset: 0x0000A4F2
		internal ArrayInitialisation(IToken token) : base(token)
		{
		}

		// Token: 0x06000313 RID: 787 RVA: 0x0000B507 File Offset: 0x0000A507
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000314 RID: 788 RVA: 0x0000B510 File Offset: 0x0000A510
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000315 RID: 789 RVA: 0x0000B51C File Offset: 0x0000A51C
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			IExprVisitor3 exprVisitor = visitor as IExprVisitor3;
			if (exprVisitor != null)
			{
				exprVisitor.visit(this);
			}
		}

		// Token: 0x06000316 RID: 790 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x17000085 RID: 133
		// (get) Token: 0x06000317 RID: 791 RVA: 0x0000B53C File Offset: 0x0000A53C
		public IExpression[] InitValues
		{
			get
			{
				Expression[] array = new Expression[this.m_initlist.Count];
				LList<_IExpression> initlist = this.m_initlist;
				_IExpression[] array2 = array;
				initlist.CopyTo(array2);
				return array;
			}
		}

		// Token: 0x17000086 RID: 134
		// (get) Token: 0x06000318 RID: 792 RVA: 0x0000B56B File Offset: 0x0000A56B
		public IList<_IExpression> _InitValues
		{
			get
			{
				return this.m_initlist;
			}
		}

		// Token: 0x06000319 RID: 793 RVA: 0x0000B574 File Offset: 0x0000A574
		[SuppressMessage("Major Code Smell", "S1168:Empty arrays and collections should be returned instead of null", Justification = "Behaviour cannot be changed because of released interfaces")]
		public IList<_IExpression> GetFlatList(IScope scope, out bool bValid)
		{
			List<_IExpression> list = new List<_IExpression>();
			bValid = true;
			foreach (_IExpression iexpression in this.m_initlist)
			{
				if (iexpression is MultipleIndexInitialisation)
				{
					MultipleIndexInitialisation multipleIndexInitialisation = iexpression as MultipleIndexInitialisation;
					ILiteralValue literalValue = multipleIndexInitialisation._Number.Literal(scope);
					if (literalValue == null)
					{
						bValid = false;
						return null;
					}
					int @int = literalValue.GetInt(out bValid);
					if (!bValid)
					{
						return null;
					}
					for (int i = 0; i < @int; i++)
					{
						list.Add(multipleIndexInitialisation._Value);
					}
				}
				else
				{
					list.Add(iexpression);
				}
			}
			return list;
		}

		// Token: 0x17000087 RID: 135
		public _IExpression this[int i]
		{
			get
			{
				return this.m_initlist[i];
			}
			set
			{
				this.m_initlist[i] = value;
			}
		}

		// Token: 0x0600031C RID: 796 RVA: 0x0000B649 File Offset: 0x0000A649
		public void AddInitValue(_IExpression exp)
		{
			this.m_initlist.Add(exp);
		}

		// Token: 0x0600031D RID: 797 RVA: 0x0000B658 File Offset: 0x0000A658
		public override _IExprement Duplicate()
		{
			ArrayInitialisation arrayInitialisation = new ArrayInitialisation();
			this.DuplicateCommon(arrayInitialisation);
			foreach (_IExpression iexpression in this.m_initlist)
			{
				arrayInitialisation.AddInitValue(iexpression.Duplicate() as _IExpression);
			}
			arrayInitialisation._CompiledType = this._CompiledType;
			arrayInitialisation.DefaultInitializationDone = this.DefaultInitializationDone;
			return arrayInitialisation;
		}

		// Token: 0x0600031E RID: 798 RVA: 0x0000B6D8 File Offset: 0x0000A6D8
		public override bool IsConstant(IScope scope, bool bAllocatedOK)
		{
			using (IEnumerator<_IExpression> enumerator = this.m_initlist.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (!enumerator.Current.IsConstant(scope, bAllocatedOK))
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x17000088 RID: 136
		// (get) Token: 0x0600031F RID: 799 RVA: 0x0000B730 File Offset: 0x0000A730
		// (set) Token: 0x06000320 RID: 800 RVA: 0x0000B738 File Offset: 0x0000A738
		public bool DefaultInitializationDone
		{
			get
			{
				return this._bDefaultInitializationDone;
			}
			set
			{
				this._bDefaultInitializationDone = value;
			}
		}

		// Token: 0x17000089 RID: 137
		// (get) Token: 0x06000321 RID: 801 RVA: 0x0000B741 File Offset: 0x0000A741
		// (set) Token: 0x06000322 RID: 802 RVA: 0x0000B749 File Offset: 0x0000A749
		[DefaultSerialization("Type")]
		[StorageVersion("3.3.0.10")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		public override ICompiledType _CompiledType
		{
			get
			{
				return this.m_ctype;
			}
			set
			{
				this.m_ctype = value;
			}
		}

		// Token: 0x1700008A RID: 138
		// (get) Token: 0x06000323 RID: 803 RVA: 0x0000B752 File Offset: 0x0000A752
		// (set) Token: 0x06000324 RID: 804 RVA: 0x0000B760 File Offset: 0x0000A760
		[DefaultSerialization("InitList")]
		[StorageVersion("3.3.0.0-3.5.1.39")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private ArrayList OldList
		{
			get
			{
				return new ArrayList(this.m_initlist);
			}
			set
			{
				this.m_initlist = new LList<_IExpression>();
				foreach (object obj in value)
				{
					Expression expression = (Expression)obj;
					this.m_initlist.Add(expression);
				}
			}
		}

		// Token: 0x04000075 RID: 117
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("compiledtype")]
		[StorageVersion("3.3.0.0")]
		private ICompiledType m_ctype;

		// Token: 0x04000076 RID: 118
		[DefaultSerialization("InitListNew")]
		[StorageVersion("3.5.1.40")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private LList<_IExpression> m_initlist = new LList<_IExpression>(1);

		// Token: 0x04000077 RID: 119
		[DefaultSerialization("DefInitDone")]
		[StorageVersion("3.5.10.0")]
		[StorageIgnorable]
		private bool _bDefaultInitializationDone;
	}
}
