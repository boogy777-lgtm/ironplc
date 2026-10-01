using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000C7 RID: 199
	internal static class TypeComparerProxy
	{
		// Token: 0x17000314 RID: 788
		// (get) Token: 0x06000C33 RID: 3123 RVA: 0x0001F53C File Offset: 0x0001E53C
		private static ITypeComparer _TypeComparer
		{
			get
			{
				return VersionedCompilerFactory._TypeComparer;
			}
		}

		// Token: 0x06000C34 RID: 3124 RVA: 0x0001F543 File Offset: 0x0001E543
		internal static bool IsImplicitConvertable(ICompiledType typeSource, ICompiledType typeDest, ICommonScope scope)
		{
			return TypeComparerProxy._TypeComparer.IsImplicitConvertable(typeSource, typeDest, scope);
		}

		// Token: 0x06000C35 RID: 3125 RVA: 0x0001F552 File Offset: 0x0001E552
		internal static ICompiledType EvaluateAliasAndEnumType(ICompiledType ctype, ICommonScope scope)
		{
			return TypeComparerProxy._TypeComparer.EvaluateAliasAndEnumType(ctype, scope);
		}
	}
}
