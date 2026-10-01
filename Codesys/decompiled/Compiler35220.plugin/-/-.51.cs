using System;
using \u0005;
using \u0017;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;

namespace \u000E
{
	// Token: 0x020000B8 RID: 184
	internal sealed class \u0003 : ILMQualifierService
	{
		// Token: 0x06000E50 RID: 3664 RVA: 0x00026D1C File Offset: 0x00024F1C
		public IExpression \u0001(IExpression \u0002, string \u0003)
		{
			string stExpression = \u0017.\u000F.\u0001(\u0002, \u0003);
			return \u0019.\u0003.Builder.ParseExpression(stExpression);
		}

		// Token: 0x06000E51 RID: 3665 RVA: 0x00026D3C File Offset: 0x00024F3C
		public IExpression \u0001(IExpression \u0002, ILMPreCompileSet \u0003, ILMPreCompileSet \u0004)
		{
			string stExpression = \u0017.\u000F.\u0001(\u0002, \u0003, \u0004);
			return \u0019.\u0003.Builder.ParseExpression(stExpression);
		}

		// Token: 0x06000E52 RID: 3666 RVA: 0x00026D60 File Offset: 0x00024F60
		public IType \u0001(IType \u0002, ILMPreCompileSet \u0003, ILMPreCompileSet \u0004)
		{
			return new global::\u0005.\u0004().\u0001(\u0002, \u0003, \u0004);
		}
	}
}
