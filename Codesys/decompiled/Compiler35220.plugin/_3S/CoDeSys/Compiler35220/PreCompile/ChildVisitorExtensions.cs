using System;
using System.Collections.Generic;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.PreCompile
{
	// Token: 0x02000159 RID: 345
	public static class ChildVisitorExtensions
	{
		// Token: 0x06001817 RID: 6167 RVA: 0x0004A8BC File Offset: 0x00048ABC
		public static IReadOnlyCollection<_IExprement> GetChildren(this _IExprement exprement)
		{
			return ChildVisitor.GetChildren(exprement);
		}
	}
}
