using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000076 RID: 118
	[TypeGuid("{E48083DA-21C6-46f2-923C-201341C61ABC}")]
	[StorageVersion("3.4.0.0")]
	public class TypeExpression : PositionExpression, _ITypeExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, ITypeExpression
	{
		// Token: 0x06000793 RID: 1939 RVA: 0x0000B408 File Offset: 0x0000A408
		public TypeExpression()
		{
		}

		// Token: 0x06000794 RID: 1940 RVA: 0x00012F45 File Offset: 0x00011F45
		public TypeExpression(ICompiledType cType)
		{
			this._CompiledType = cType;
		}

		// Token: 0x06000795 RID: 1941 RVA: 0x00012F54 File Offset: 0x00011F54
		public TypeExpression(ICompiledType cType, IToken token) : base(token)
		{
			this._CompiledType = cType;
		}

		// Token: 0x06000796 RID: 1942 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x06000797 RID: 1943 RVA: 0x00012F64 File Offset: 0x00011F64
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000798 RID: 1944 RVA: 0x00012F6D File Offset: 0x00011F6D
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000799 RID: 1945 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
		}

		// Token: 0x0600079A RID: 1946 RVA: 0x00012F78 File Offset: 0x00011F78
		public override _IExprement Duplicate()
		{
			TypeExpression typeExpression = new TypeExpression(this._CompiledType);
			this.DuplicateCommon(typeExpression);
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35720)
			{
				typeExpression._CompiledType = (this._CompiledType as _IType)._Duplicate(false);
			}
			return typeExpression;
		}

		// Token: 0x170001AA RID: 426
		// (get) Token: 0x0600079B RID: 1947 RVA: 0x00012FC1 File Offset: 0x00011FC1
		// (set) Token: 0x0600079C RID: 1948 RVA: 0x00012FC9 File Offset: 0x00011FC9
		[DefaultSerialization("Type")]
		[StorageVersion("3.4.0.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		public override ICompiledType _CompiledType { get; set; }

		// Token: 0x170001AB RID: 427
		// (get) Token: 0x0600079D RID: 1949 RVA: 0x00012FD2 File Offset: 0x00011FD2
		public ICompiledType2 ExpressionType
		{
			get
			{
				return this._CompiledType as ICompiledType2;
			}
		}
	}
}
