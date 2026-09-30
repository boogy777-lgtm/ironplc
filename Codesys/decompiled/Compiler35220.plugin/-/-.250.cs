using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;
using \u0083;

namespace \u0001
{
	// Token: 0x020002B2 RID: 690
	internal static class \u000E
	{
		// Token: 0x06002AB7 RID: 10935 RVA: 0x000960F4 File Offset: 0x000942F4
		public static \u0081.\u000F \u0001(IScope5 \u0002, string \u0003, ISignature \u0004, _ICompoAccessExpression \u0005)
		{
			string text = "__GET" + \u0003;
			string text2 = "__SET" + \u0003;
			IScope5 scope = \u0002.CreateLocalScope(\u0004);
			bool flag = scope.FindSignatureLocal(text) != null;
			bool flag2 = scope.FindSignatureLocal(text2) != null;
			string str = \u0004.Name;
			if (\u0005 != null)
			{
				str = \u0005._Left.ToString();
			}
			return new \u0081.\u000F
			{
				\u0001 = (flag2 ? (str + "." + text2) : null),
				\u0002 = (flag ? (str + "." + text) : null)
			};
		}

		// Token: 0x06002AB8 RID: 10936 RVA: 0x0009618C File Offset: 0x0009438C
		public static \u0083.\u0005 \u0001(IScope5 \u0002, string \u0003, ISignature \u0004, ISignature \u0005)
		{
			string text = "__GET" + \u0003;
			string text2 = "__SET" + \u0003;
			_IVirtualFunctionTable ivirtualFunctionTable = (_IVirtualFunctionTable)\u0004.VirtualFunctionTable;
			IScope5 scope = \u0002.CreateLocalScope(\u0004);
			bool flag = scope.FindSignatureLocal(text) != null;
			bool flag2 = scope.FindSignatureLocal(text2) != null;
			int num = flag ? (ivirtualFunctionTable[text.ToUpperInvariant()] / \u0002.PointerSize) : -1;
			int num2 = flag2 ? (ivirtualFunctionTable[text2.ToUpperInvariant()] / \u0002.PointerSize) : -1;
			if (\u0005.HasAttribute(CompileAttributes.ATTRIBUTE_CPPCOMPATIBLE))
			{
				num--;
				num2--;
			}
			return new \u0083.\u0005
			{
				\u0001 = num2,
				\u0002 = num
			};
		}
	}
}
