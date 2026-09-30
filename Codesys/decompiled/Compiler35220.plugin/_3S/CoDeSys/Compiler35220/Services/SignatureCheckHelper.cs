using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Services
{
	// Token: 0x020000F4 RID: 244
	internal static class SignatureCheckHelper
	{
		// Token: 0x060010C9 RID: 4297 RVA: 0x00030F88 File Offset: 0x0002F188
		public static bool \u0001(ISignature \u0002)
		{
			if (\u0002 == null)
			{
				return false;
			}
			if (\u0002.Name != IdentifierConstants.ReInitMethodName)
			{
				return false;
			}
			bool flag = \u0002.AllInputs.Where(new Func<IVariable, bool>(SignatureCheckHelper.<>c.<>9.\u0001)).Count<IVariable>() == 0;
			IEnumerable<IVariable> source = \u0002.AllOutputs.Where(new Func<IVariable, bool>(SignatureCheckHelper.<>c.<>9.\u0002));
			bool flag2 = source.Count<IVariable>() == 1 && source.First<IVariable>().Name == \u0002.Name && source.First<IVariable>().Type.Class == TypeClass.Bool;
			return flag && flag2;
		}

		// Token: 0x060010CA RID: 4298 RVA: 0x00031048 File Offset: 0x0002F248
		public static bool \u0002(ISignature \u0002)
		{
			if (\u0002 == null)
			{
				return false;
			}
			if (\u0002.Name != IdentifierConstants.ExitMethodName)
			{
				return false;
			}
			IEnumerable<IVariable> source = \u0002.AllInputs.Where(new Func<IVariable, bool>(SignatureCheckHelper.<>c.<>9.\u0003));
			IEnumerable<IVariable> source2 = \u0002.AllOutputs.Where(new Func<IVariable, bool>(SignatureCheckHelper.<>c.<>9.\u0004));
			bool flag = source.Count<IVariable>() == 1 && source.First<IVariable>().Name == "BINCOPYCODE" && source.First<IVariable>().Type.Class == TypeClass.Bool;
			bool flag2 = source2.Count<IVariable>() == 1 && source2.First<IVariable>().Name == \u0002.Name && source2.First<IVariable>().Type.Class == TypeClass.Bool;
			return flag && flag2;
		}
	}
}
