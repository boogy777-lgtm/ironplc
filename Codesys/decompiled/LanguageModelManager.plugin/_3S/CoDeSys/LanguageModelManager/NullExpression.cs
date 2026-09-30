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
	// Token: 0x02000065 RID: 101
	[TypeGuid("{069e1c3c-1262-4a74-b69a-7bc02a4240b8}")]
	[StorageVersion("3.3.0.0")]
	public class NullExpression : PositionExpression, _INullExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, INullExpression
	{
		// Token: 0x0600064C RID: 1612 RVA: 0x0000B408 File Offset: 0x0000A408
		public NullExpression()
		{
		}

		// Token: 0x0600064D RID: 1613 RVA: 0x0000B9FB File Offset: 0x0000A9FB
		internal NullExpression(IToken token) : base(token)
		{
		}

		// Token: 0x0600064E RID: 1614 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x0600064F RID: 1615 RVA: 0x00010926 File Offset: 0x0000F926
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000650 RID: 1616 RVA: 0x0001092F File Offset: 0x0000F92F
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000651 RID: 1617 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
		}

		// Token: 0x06000652 RID: 1618 RVA: 0x00010938 File Offset: 0x0000F938
		public override _IExprement Duplicate()
		{
			NullExpression nullExpression = new NullExpression();
			this.DuplicateCommon(nullExpression);
			return nullExpression;
		}

		// Token: 0x17000163 RID: 355
		// (get) Token: 0x06000653 RID: 1619 RVA: 0x00005F0F File Offset: 0x00004F0F
		// (set) Token: 0x06000654 RID: 1620 RVA: 0x00003AE9 File Offset: 0x00002AE9
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
