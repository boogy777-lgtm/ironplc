using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000042 RID: 66
	[TypeGuid("{0ed4e3a5-7d7e-4f9b-b0d2-b21d1b80d6fc}")]
	[StorageVersion("3.3.0.0")]
	public class BitAccess : PositionExpression, _IBitAccess, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IBitAccess
	{
		// Token: 0x0600034D RID: 845 RVA: 0x0000B408 File Offset: 0x0000A408
		public BitAccess()
		{
		}

		// Token: 0x0600034E RID: 846 RVA: 0x0000BA80 File Offset: 0x0000AA80
		public BitAccess(_IExpression expBase, byte byBitNr)
		{
			this.m_expBase = expBase;
			this.m_byBitNr = byBitNr;
		}

		// Token: 0x17000095 RID: 149
		// (get) Token: 0x0600034F RID: 847 RVA: 0x0000BA96 File Offset: 0x0000AA96
		// (set) Token: 0x06000350 RID: 848 RVA: 0x0000BA9E File Offset: 0x0000AA9E
		public byte BitNr
		{
			get
			{
				return this.m_byBitNr;
			}
			set
			{
				this.m_byBitNr = value;
			}
		}

		// Token: 0x17000096 RID: 150
		// (get) Token: 0x06000351 RID: 849 RVA: 0x0000BAA7 File Offset: 0x0000AAA7
		public IExpression Base
		{
			get
			{
				return this._Base;
			}
		}

		// Token: 0x17000097 RID: 151
		// (get) Token: 0x06000352 RID: 850 RVA: 0x0000BAAF File Offset: 0x0000AAAF
		// (set) Token: 0x06000353 RID: 851 RVA: 0x0000BAB7 File Offset: 0x0000AAB7
		public _IExpression _Base
		{
			get
			{
				return this.m_expBase;
			}
			set
			{
				this.m_expBase = value;
			}
		}

		// Token: 0x06000354 RID: 852 RVA: 0x0000BAC0 File Offset: 0x0000AAC0
		public override _IExprement Duplicate()
		{
			Debug.Assert(false);
			return null;
		}

		// Token: 0x06000355 RID: 853 RVA: 0x0000BAC9 File Offset: 0x0000AAC9
		public override void Accept(IExprementVisitor visitor)
		{
			ICodegeneratorVisitor codegeneratorVisitor = visitor as ICodegeneratorVisitor;
			Debug.Assert(codegeneratorVisitor != null);
			codegeneratorVisitor.visit(this);
		}

		// Token: 0x06000356 RID: 854 RVA: 0x0000BAE0 File Offset: 0x0000AAE0
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000357 RID: 855 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
		}

		// Token: 0x06000358 RID: 856 RVA: 0x0000BAE9 File Offset: 0x0000AAE9
		public override bool IsEqual(_IExprement exprementRight)
		{
			Debug.Assert(false);
			return false;
		}

		// Token: 0x06000359 RID: 857 RVA: 0x0000BAF2 File Offset: 0x0000AAF2
		public override bool IsConstant(IScope scope, bool bAllocatedOK)
		{
			return this._Base.IsConstant(scope, bAllocatedOK);
		}

		// Token: 0x0600035A RID: 858 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsVarInOutInput(IScope scope, bool bWriteToConstants, bool bVarInoutConstant)
		{
			return false;
		}

		// Token: 0x0600035B RID: 859 RVA: 0x0000BB01 File Offset: 0x0000AB01
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return this._Base.IsLValue(scope, bWriteToConstants);
		}

		// Token: 0x17000098 RID: 152
		// (get) Token: 0x0600035C RID: 860 RVA: 0x0000BB10 File Offset: 0x0000AB10
		// (set) Token: 0x0600035D RID: 861 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[DefaultSerialization("Type")]
		[StorageVersion("3.3.0.10")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		[SuppressMessage("Blocker Code Smell", "S3237:\"value\" parameters should be used", Justification = "Setter cannot be changed for compatibility reasons")]
		public override ICompiledType _CompiledType
		{
			get
			{
				return TypeTable.Get(TypeClass.Bool);
			}
			set
			{
			}
		}

		// Token: 0x0400007F RID: 127
		[DefaultSerialization("BitNr")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private byte m_byBitNr;

		// Token: 0x04000080 RID: 128
		[DefaultSerialization("Base")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expBase;
	}
}
