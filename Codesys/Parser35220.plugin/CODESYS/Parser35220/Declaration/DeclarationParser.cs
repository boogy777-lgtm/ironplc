using System;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Declaration
{
	// Token: 0x0200004E RID: 78
	public class DeclarationParser
	{
		// Token: 0x06000526 RID: 1318 RVA: 0x000162AD File Offset: 0x000144AD
		internal DeclarationParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x17000139 RID: 313
		// (get) Token: 0x06000527 RID: 1319 RVA: 0x000162BC File Offset: 0x000144BC
		private ParserContext Context { get; }

		// Token: 0x1700013A RID: 314
		// (get) Token: 0x06000528 RID: 1320 RVA: 0x000162C4 File Offset: 0x000144C4
		// (set) Token: 0x06000529 RID: 1321 RVA: 0x000162CC File Offset: 0x000144CC
		internal bool Library { get; set; }

		// Token: 0x1700013B RID: 315
		// (get) Token: 0x0600052A RID: 1322 RVA: 0x000162D5 File Offset: 0x000144D5
		// (set) Token: 0x0600052B RID: 1323 RVA: 0x000162DD File Offset: 0x000144DD
		internal bool AllowPaths { get; set; }

		// Token: 0x1700013C RID: 316
		// (get) Token: 0x0600052C RID: 1324 RVA: 0x000162E6 File Offset: 0x000144E6
		// (set) Token: 0x0600052D RID: 1325 RVA: 0x000162EE File Offset: 0x000144EE
		internal Operator OpCurrentVariableList { get; set; }

		// Token: 0x0600052E RID: 1326 RVA: 0x000162F7 File Offset: 0x000144F7
		internal _IStatement ParsePOUDeclaration(IToken tokenPOUType, Operator opParam)
		{
			return POUDeclarationParser.ParsePOUDeclaration(this.Context, tokenPOUType, opParam);
		}

		// Token: 0x0600052F RID: 1327 RVA: 0x00016306 File Offset: 0x00014506
		internal _IStatement ParseVariableList(IToken tokenVar)
		{
			return VariableListParser.ParseVariableList(this.Context, tokenVar);
		}

		// Token: 0x06000530 RID: 1328 RVA: 0x00016314 File Offset: 0x00014514
		internal _IStatement ParseVariableDeclaration(IToken tokenIdent)
		{
			return VariableDeclarationParser.ParseVariableDeclaration(this.Context, tokenIdent);
		}

		// Token: 0x06000531 RID: 1329 RVA: 0x00016322 File Offset: 0x00014522
		internal _IStatement ParseTypeDeclaration(IToken tokenType)
		{
			return TypeDeclarationParser.ParseTypeDeclaration(this.Context, tokenType);
		}
	}
}
