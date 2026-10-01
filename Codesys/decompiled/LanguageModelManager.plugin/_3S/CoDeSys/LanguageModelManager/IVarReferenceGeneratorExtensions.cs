using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000102 RID: 258
	internal static class IVarReferenceGeneratorExtensions
	{
		// Token: 0x060012D7 RID: 4823 RVA: 0x00034C81 File Offset: 0x00033C81
		public static IAddressInfo GenerateBitAccess(this IVarReferenceGenerator self, ICompoAccessExpression compo, IAddressInfo aiBase, int nBitOffset, int accessedElementSize)
		{
			return self.GenerateBitAccess(compo, compo.Left.Type, compo.Right.Position, aiBase, nBitOffset, accessedElementSize);
		}

		// Token: 0x060012D8 RID: 4824 RVA: 0x00034CA4 File Offset: 0x00033CA4
		public static IAddressInfo GenerateBitAccess(this IVarReferenceGenerator self, IPartialAccessExpression compo, IAddressInfo aiBase, int nBitOffset, int accessedElementSize)
		{
			return self.GenerateBitAccess(compo, compo.Left.Type, compo.Position, aiBase, nBitOffset, accessedElementSize);
		}
	}
}
