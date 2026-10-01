using System;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x02000256 RID: 598
	public interface IExpressionStatementReplacer
	{
		// Token: 0x06002739 RID: 10041
		_IStatement ReplaceExpressionStatement(_IExpressionStatement expressionStatement, _ICompiledPOU cpou);
	}
}
