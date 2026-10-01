using System;
using System.Diagnostics;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Declaration
{
	// Token: 0x02000058 RID: 88
	[DebuggerDisplay("Statement: {Statement}")]
	internal class StatementElement : SyntaxElement
	{
		// Token: 0x17000159 RID: 345
		// (get) Token: 0x0600059B RID: 1435 RVA: 0x00017C49 File Offset: 0x00015E49
		internal _IStatement Statement { get; }

		// Token: 0x0600059C RID: 1436 RVA: 0x00017C51 File Offset: 0x00015E51
		internal StatementElement(_IStatement statement)
		{
			this.Statement = statement;
		}
	}
}
