using System;
using System.Diagnostics.CodeAnalysis;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000052 RID: 82
	[TypeGuid("{8D1E9C5B-4B83-4162-8C6E-90E71B7D99AA}")]
	[StorageVersion("3.3.0.0")]
	internal class FramePointerExpression : Expression, _IFramePointerExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression
	{
		// Token: 0x060004D9 RID: 1241 RVA: 0x0000E92F File Offset: 0x0000D92F
		internal FramePointerExpression()
		{
			this._ctype = new PointerType(TypeTable.Byte);
		}

		// Token: 0x060004DA RID: 1242 RVA: 0x0000E947 File Offset: 0x0000D947
		public override void Accept(IExprementVisitor visitor)
		{
			if (visitor is IExprementVisitorAdapter)
			{
				(visitor as IExprementVisitorAdapter).visit(this);
			}
		}

		// Token: 0x060004DB RID: 1243 RVA: 0x0000E95D File Offset: 0x0000D95D
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x060004DC RID: 1244 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
		}

		// Token: 0x060004DD RID: 1245 RVA: 0x0000E968 File Offset: 0x0000D968
		public override _IExprement Duplicate()
		{
			FramePointerExpression framePointerExpression = new FramePointerExpression();
			this.DuplicateCommon(framePointerExpression);
			return framePointerExpression;
		}

		// Token: 0x060004DE RID: 1246 RVA: 0x00005E58 File Offset: 0x00004E58
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return true;
		}

		// Token: 0x17000104 RID: 260
		// (get) Token: 0x060004DF RID: 1247 RVA: 0x0000E983 File Offset: 0x0000D983
		// (set) Token: 0x060004E0 RID: 1248 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[SuppressMessage("Blocker Code Smell", "S3237:\"value\" parameters should be used", Justification = "Setter cannot be changed for compatibility reasons")]
		public override ICompiledType _CompiledType
		{
			get
			{
				return this._ctype;
			}
			set
			{
			}
		}

		// Token: 0x17000105 RID: 261
		// (get) Token: 0x060004E1 RID: 1249 RVA: 0x00005F0F File Offset: 0x00004F0F
		// (set) Token: 0x060004E2 RID: 1250 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[SuppressMessage("Blocker Code Smell", "S3237:\"value\" parameters should be used", Justification = "Setter cannot be changed for compatibility reasons")]
		public override IMinimalPosition PositionIntern
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		// Token: 0x17000106 RID: 262
		// (get) Token: 0x060004E3 RID: 1251 RVA: 0x00004E6B File Offset: 0x00003E6B
		// (set) Token: 0x060004E4 RID: 1252 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[SuppressMessage("Blocker Code Smell", "S3237:\"value\" parameters should be used", Justification = "Setter cannot be changed for compatibility reasons")]
		public override short LengthIntern
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		// Token: 0x040000B4 RID: 180
		private readonly ICompiledType _ctype;
	}
}
