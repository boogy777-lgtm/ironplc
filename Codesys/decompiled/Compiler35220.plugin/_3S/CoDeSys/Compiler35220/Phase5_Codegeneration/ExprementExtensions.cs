using System;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration
{
	// Token: 0x02000231 RID: 561
	public static class ExprementExtensions
	{
		// Token: 0x0600253E RID: 9534 RVA: 0x00081ADC File Offset: 0x0007FCDC
		public static bool IsLocalSignaturePragma(this _IPragmaStatement pragma, out int nId)
		{
			_ILocalSignatureIdPragma ilocalSignatureIdPragma = pragma as _ILocalSignatureIdPragma;
			if (ilocalSignatureIdPragma != null)
			{
				nId = ilocalSignatureIdPragma.LocalSignatureId;
				return true;
			}
			string[] array = pragma.Text.Split(new char[]
			{
				' '
			});
			if (array.Length == 2 && array[0] == "localsignature")
			{
				nId = int.Parse(array[1]);
				return true;
			}
			nId = -1;
			return false;
		}
	}
}
