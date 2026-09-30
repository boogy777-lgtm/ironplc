using System;
using System.Collections.Generic;
using System.Linq;
using \u0003;
using \u001C;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.OnlineChange.FastOnlinechange.Steps.ChangedSignatureProcessor
{
	// Token: 0x0200038C RID: 908
	internal sealed class NewLocalOffsetsCalculator : \u001C.\u0012
	{
		// Token: 0x060034B1 RID: 13489 RVA: 0x000CFD4C File Offset: 0x000CDF4C
		public bool \u0001(\u0016 \u0002)
		{
			if (!\u0002.signChanges.\u0003 && !\u0002.signChanges.\u0004 && !\u0002.signChanges.\u0006)
			{
				return true;
			}
			if (\u0002.sign.POUType == Operator.FunctionBlock)
			{
				if (!Locator.\u0001(\u0002.focContext.ComconNew.DataManager, \u0002.focContext.ComconNew, \u0002.focContext.ComconOld, \u0002.signCompiled, \u0002.signRef))
				{
					goto IL_121;
				}
				using (IEnumerator<_IVariable> enumerator = global::\u0003.\u0015.\u0001(\u0002.sign, \u0002.signCompiled).Where(new Func<_IVariable, bool>(NewLocalOffsetsCalculator.<>c.<>9.\u0001)).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						_IVariable ivariable = enumerator.Current;
						ivariable.SetFlag(VarFlag.OnlChangeInit, true);
						\u0002.focContext.\u0001 = true;
					}
					goto IL_121;
				}
			}
			Locator.\u0001(\u0002.focContext.ComconNew.DataManager, \u0002.focContext.ComconNew, \u0002.focContext.ComconOld, \u0002.signCompiled, \u0002.signRef);
			IL_121:
			return \u0002.signCompiled.HasFlag(SignatureFlag.Located) && \u0002.signCompiled.Size == \u0002.sign.Size;
		}
	}
}
