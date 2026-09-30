using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200004F RID: 79
	[TypeGuid("{c6337f16-a6c2-4dd4-8736-0bbe2ca840d4}")]
	[StorageVersion("3.3.0.0")]
	public class ErrorExpression : PositionExpression, _IErrorExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IErrorExpression
	{
		// Token: 0x06000494 RID: 1172 RVA: 0x0000B408 File Offset: 0x0000A408
		public ErrorExpression()
		{
		}

		// Token: 0x06000495 RID: 1173 RVA: 0x0000B9FB File Offset: 0x0000A9FB
		internal ErrorExpression(IToken token) : base(token)
		{
		}

		// Token: 0x06000496 RID: 1174 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x06000497 RID: 1175 RVA: 0x0000E533 File Offset: 0x0000D533
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000498 RID: 1176 RVA: 0x0000E53C File Offset: 0x0000D53C
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000499 RID: 1177 RVA: 0x0000E545 File Offset: 0x0000D545
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			IExprVisitor6 exprVisitor = visitor as IExprVisitor6;
			if (exprVisitor == null)
			{
				return;
			}
			exprVisitor.visit(this);
		}

		// Token: 0x0600049A RID: 1178 RVA: 0x0000E558 File Offset: 0x0000D558
		public override _IExprement Duplicate()
		{
			ErrorExpression errorExpression = new ErrorExpression();
			this.DuplicateCommon(errorExpression);
			return errorExpression;
		}

		// Token: 0x170000F0 RID: 240
		// (get) Token: 0x0600049B RID: 1179 RVA: 0x00005F0F File Offset: 0x00004F0F
		// (set) Token: 0x0600049C RID: 1180 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[DefaultSerialization("Type")]
		[StorageVersion("3.3.0.10")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		[SuppressMessage("Blocker Code Smell", "S3237:\"value\" parameters should be used", Justification = "Setter cannot be changed for compatibility reasons")]
		public override ICompiledType _CompiledType
		{
			get
			{
				return null;
			}
			set
			{
			}
		}
	}
}
