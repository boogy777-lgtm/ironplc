using System;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Compile.Phase4_TypeCheck
{
	// Token: 0x020002E9 RID: 745
	public interface IErrorAdder
	{
		// Token: 0x06002D5B RID: 11611
		void AddError(_IExprement exp, MessageId mid, params object[] args);
	}
}
