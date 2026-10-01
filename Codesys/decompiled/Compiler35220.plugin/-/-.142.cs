using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using \u0017;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0001
{
	// Token: 0x02000190 RID: 400
	internal sealed class \u0006 : ICommonScope2, ICommonScope, _IPrecompileScope, IPrecompileScope6, IPrecompileScope5, IPrecompileScope4, IPrecompileScope3, IPrecompileScope2, IPrecompileScope, \u0006
	{
		// Token: 0x170005BA RID: 1466
		// (get) Token: 0x06001C05 RID: 7173 RVA: 0x0005BEA8 File Offset: 0x0005A0A8
		private \u0006 CommonScope3
		{
			get
			{
				return (\u0006)this.Scope;
			}
		}

		// Token: 0x170005BB RID: 1467
		// (get) Token: 0x06001C06 RID: 7174 RVA: 0x0005BEB8 File Offset: 0x0005A0B8
		private Dictionary<string, _IExpression> CurrentGenerics { get; }

		// Token: 0x170005BC RID: 1468
		// (get) Token: 0x06001C07 RID: 7175 RVA: 0x0005BEC0 File Offset: 0x0005A0C0
		private _IPrecompileScope3 Scope { get; }

		// Token: 0x06001C08 RID: 7176 RVA: 0x0005BEC8 File Offset: 0x0005A0C8
		internal \u0006(_IPrecompileScope3 \u001B\u0008, Dictionary<string, _IExpression> \u001C\u0008)
		{
			this.Scope = \u001B\u0008;
			this.CurrentGenerics = \u001C\u0008;
		}

		// Token: 0x06001C09 RID: 7177 RVA: 0x0005BEE0 File Offset: 0x0005A0E0
		public bool \u0001(_IExpression \u0002)
		{
			return this.CommonScope3.\u0001(\u0002);
		}

		// Token: 0x06001C0A RID: 7178 RVA: 0x0005BEF0 File Offset: 0x0005A0F0
		public bool \u0002(_IExpression \u0002)
		{
			return this.CommonScope3.\u0002(\u0002);
		}

		// Token: 0x06001C0B RID: 7179 RVA: 0x0005BF00 File Offset: 0x0005A100
		public _IVariable \u0001(_IExpression \u0002)
		{
			return this.CommonScope3.\u0001(\u0002);
		}

		// Token: 0x06001C0C RID: 7180 RVA: 0x0005BF10 File Offset: 0x0005A110
		public _ISignature \u0001(_IExpression \u0002)
		{
			return this.CommonScope3.\u0001(\u0002);
		}

		// Token: 0x06001C0D RID: 7181 RVA: 0x0005BF20 File Offset: 0x0005A120
		private _IExpression \u0001(_IVariable \u0002)
		{
			_IExpression result;
			if (this.CurrentGenerics.TryGetValue(\u0002.Name, out result))
			{
				return result;
			}
			return null;
		}

		// Token: 0x06001C0E RID: 7182 RVA: 0x0005BF48 File Offset: 0x0005A148
		public _IExpression \u0001(_ISignature \u0002, _IVariable \u0003)
		{
			_IExpression iexpression = this.\u0001(\u0003);
			if (iexpression != null)
			{
				return iexpression;
			}
			return this.CommonScope3.\u0001(\u0002, \u0003);
		}

		// Token: 0x06001C0F RID: 7183 RVA: 0x0005BF70 File Offset: 0x0005A170
		public \u0006 \u0001(_IExpression \u0002, _IUserdefType \u0003)
		{
			return this.CommonScope3.\u0001(\u0002, \u0003);
		}

		// Token: 0x06001C10 RID: 7184 RVA: 0x0005BF80 File Offset: 0x0005A180
		public \u0006 \u0001(_ISignature \u0002)
		{
			return this.CommonScope3.\u0001(\u0002);
		}

		// Token: 0x170005BD RID: 1469
		// (get) Token: 0x06001C11 RID: 7185 RVA: 0x0005BF90 File Offset: 0x0005A190
		public bool ContainsCopyCode
		{
			get
			{
				return this.CommonScope3.ContainsCopyCode;
			}
		}

		// Token: 0x06001C12 RID: 7186 RVA: 0x0005BFA0 File Offset: 0x0005A1A0
		public void \u0001(_IExpression \u0002, out _IVariable \u0003, out _ISignature \u0004, out \u0006 \u0005)
		{
			this.CommonScope3.\u0001(\u0002, out \u0003, out \u0004, out \u0005);
		}

		// Token: 0x06001C13 RID: 7187 RVA: 0x0005BFB4 File Offset: 0x0005A1B4
		public bool \u0001(string \u0002, out IVariable \u0003, out ISignature \u0004, out IPrecompileScope \u0005)
		{
			return this.Scope.FindDeclaration(\u0002, out \u0003, out \u0004, out \u0005);
		}

		// Token: 0x170005BE RID: 1470
		// (get) Token: 0x06001C14 RID: 7188 RVA: 0x0005BFC8 File Offset: 0x0005A1C8
		public ISignature LocalSignature
		{
			get
			{
				return this.Scope.LocalSignature;
			}
		}

		// Token: 0x06001C15 RID: 7189 RVA: 0x0005BFD8 File Offset: 0x0005A1D8
		public ISignature \u0001(string \u0002)
		{
			return this.Scope.FindSignatureLocal(\u0002);
		}

		// Token: 0x06001C16 RID: 7190 RVA: 0x0005BFE8 File Offset: 0x0005A1E8
		[Obsolete("qualified name expression is obsolete")]
		public ISignature \u0001(IQualifiedNameExpression \u0002)
		{
			return this.Scope.FindSignatureGlobal(\u0002);
		}

		// Token: 0x06001C17 RID: 7191 RVA: 0x0005BFF8 File Offset: 0x0005A1F8
		public ISignature \u0002(string \u0002)
		{
			return this.Scope.FindSignatureGlobal(\u0002);
		}

		// Token: 0x06001C18 RID: 7192 RVA: 0x0005C008 File Offset: 0x0005A208
		public IPrecompileScope \u0001()
		{
			return this.Scope.GlobalScope();
		}

		// Token: 0x06001C19 RID: 7193 RVA: 0x0005C018 File Offset: 0x0005A218
		public IPrecompileScope \u0001(string \u0002)
		{
			return this.Scope.NewLocalScope(\u0002);
		}

		// Token: 0x06001C1A RID: 7194 RVA: 0x0005C028 File Offset: 0x0005A228
		public IPrecompileScope \u0001(ISignature \u0002)
		{
			return this.Scope.NewLocalScope(\u0002);
		}

		// Token: 0x06001C1B RID: 7195 RVA: 0x0005C038 File Offset: 0x0005A238
		public IIdentifierInfo[] \u0001()
		{
			return this.Scope.GetAllDeclarations();
		}

		// Token: 0x06001C1C RID: 7196 RVA: 0x0005C048 File Offset: 0x0005A248
		public ISignature \u0001(IExpression \u0002)
		{
			return this.Scope.FindSignatureGlobal(\u0002);
		}

		// Token: 0x06001C1D RID: 7197 RVA: 0x0005C058 File Offset: 0x0005A258
		public ISignature2 \u0001(IExpression \u0002, out string \u0003)
		{
			return this.Scope.FindSignatureGlobal(\u0002, out \u0003);
		}

		// Token: 0x06001C1E RID: 7198 RVA: 0x0005C068 File Offset: 0x0005A268
		public string \u0001(string \u0002)
		{
			return this.Scope.GetNamespace(\u0002);
		}

		// Token: 0x170005BF RID: 1471
		// (get) Token: 0x06001C1F RID: 7199 RVA: 0x0005C078 File Offset: 0x0005A278
		// (set) Token: 0x06001C20 RID: 7200 RVA: 0x0005C088 File Offset: 0x0005A288
		public Guid ApplicationGuid
		{
			get
			{
				return this.Scope.ApplicationGuid;
			}
			set
			{
				this.Scope.ApplicationGuid = value;
			}
		}

		// Token: 0x06001C21 RID: 7201 RVA: 0x0005C098 File Offset: 0x0005A298
		public IIdentifierInfo[] \u0001(string \u0002)
		{
			return this.Scope.GetIdentifierInfo(\u0002);
		}

		// Token: 0x06001C22 RID: 7202 RVA: 0x0005C0A8 File Offset: 0x0005A2A8
		public IIdentifierInfo[] \u0001(bool \u0002, bool \u0003)
		{
			return this.Scope.GetAllDeclarations(\u0002, \u0003);
		}

		// Token: 0x06001C23 RID: 7203 RVA: 0x0005C0B8 File Offset: 0x0005A2B8
		public void \u0001()
		{
			this.Scope.SetPointerSize();
		}

		// Token: 0x170005C0 RID: 1472
		// (get) Token: 0x06001C24 RID: 7204 RVA: 0x0005C0C8 File Offset: 0x0005A2C8
		public int PointerSize
		{
			get
			{
				return this.CommonScope3.PointerSize;
			}
		}

		// Token: 0x06001C25 RID: 7205 RVA: 0x0005C0D8 File Offset: 0x0005A2D8
		public int \u0001(IType \u0002)
		{
			return this.CommonScope3.GetSize(\u0002);
		}

		// Token: 0x06001C26 RID: 7206 RVA: 0x0005C0E8 File Offset: 0x0005A2E8
		public ISignature \u0001(IUserdefType \u0002)
		{
			return this.CommonScope3.FindSignature(\u0002);
		}

		// Token: 0x06001C27 RID: 7207 RVA: 0x0005C0F8 File Offset: 0x0005A2F8
		public ISignature[] \u0001(IExpression \u0002)
		{
			return this.CommonScope3.FindSignature(\u0002);
		}

		// Token: 0x06001C28 RID: 7208 RVA: 0x0005C108 File Offset: 0x0005A308
		public ISignature \u0001(IEnumType \u0002)
		{
			return this.CommonScope3.FindSignature(\u0002);
		}

		// Token: 0x06001C29 RID: 7209 RVA: 0x0005C118 File Offset: 0x0005A318
		public ILiteralValue \u0001(IExpression \u0002, bool \u0003)
		{
			return this.CommonScope3.GetLiteralValue(\u0002, \u0003);
		}

		// Token: 0x06001C2A RID: 7210 RVA: 0x0005C128 File Offset: 0x0005A328
		public bool \u0001(ISignature \u0002, ISignature \u0003, ICommonScope \u0004)
		{
			return this.CommonScope3.IsImplicitConvertable(\u0002, \u0003, \u0004);
		}

		// Token: 0x06001C2B RID: 7211 RVA: 0x0005C138 File Offset: 0x0005A338
		public bool \u0001(IUserdefType \u0002, IUserdefType \u0003)
		{
			return this.CommonScope3.IsEqual(\u0002, \u0003);
		}

		// Token: 0x170005C1 RID: 1473
		// (get) Token: 0x06001C2C RID: 7212 RVA: 0x0005C148 File Offset: 0x0005A348
		public bool IsPrecompileScope
		{
			get
			{
				return this.CommonScope3.IsPrecompileScope;
			}
		}

		// Token: 0x06001C2D RID: 7213 RVA: 0x0005C158 File Offset: 0x0005A358
		public int \u0001(IType \u0002, IRecursionGuard \u0003)
		{
			return this.CommonScope3.GetSize(\u0002, \u0003);
		}

		// Token: 0x040004C6 RID: 1222
		[CompilerGenerated]
		private readonly Dictionary<string, _IExpression> \u0001;

		// Token: 0x040004C7 RID: 1223
		[CompilerGenerated]
		private readonly _IPrecompileScope3 \u0001;
	}
}
