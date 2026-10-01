using System;
using CODESYS.Parser;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.PragmaScanner
{
	// Token: 0x0200001A RID: 26
	internal class PragmaToken : IPragmaToken
	{
		// Token: 0x060001F0 RID: 496 RVA: 0x0000BCA2 File Offset: 0x00009EA2
		public PragmaToken(IToken tokenOrg)
		{
			this.OrgToken = tokenOrg;
		}

		// Token: 0x17000043 RID: 67
		// (get) Token: 0x060001F1 RID: 497 RVA: 0x0000BCB1 File Offset: 0x00009EB1
		public PragmaTokenType Type
		{
			get
			{
				return this._type;
			}
		}

		// Token: 0x17000044 RID: 68
		// (get) Token: 0x060001F2 RID: 498 RVA: 0x0000BCB9 File Offset: 0x00009EB9
		public IToken OrgToken { get; }

		// Token: 0x17000045 RID: 69
		// (get) Token: 0x060001F3 RID: 499 RVA: 0x0000BCC1 File Offset: 0x00009EC1
		public long Integer
		{
			get
			{
				return this._lInteger;
			}
		}

		// Token: 0x17000046 RID: 70
		// (get) Token: 0x060001F4 RID: 500 RVA: 0x0000BCC9 File Offset: 0x00009EC9
		public string Identifier
		{
			get
			{
				return this._stIdent;
			}
		}

		// Token: 0x17000047 RID: 71
		// (get) Token: 0x060001F5 RID: 501 RVA: 0x0000BCD1 File Offset: 0x00009ED1
		public string String
		{
			get
			{
				return this._stString;
			}
		}

		// Token: 0x17000048 RID: 72
		// (get) Token: 0x060001F6 RID: 502 RVA: 0x0000BCD9 File Offset: 0x00009ED9
		public PragmaOperator Operator
		{
			get
			{
				return this._pop;
			}
		}

		// Token: 0x0400005F RID: 95
		internal PragmaTokenType _type;

		// Token: 0x04000060 RID: 96
		internal PragmaOperator _pop;

		// Token: 0x04000061 RID: 97
		internal string _stIdent;

		// Token: 0x04000062 RID: 98
		internal long _lInteger;

		// Token: 0x04000063 RID: 99
		internal string _stString;
	}
}
