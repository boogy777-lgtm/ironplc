using System;
using \u001C;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0003
{
	// Token: 0x0200036E RID: 878
	internal sealed class \u0013 : \u0012
	{
		// Token: 0x06003434 RID: 13364 RVA: 0x000CD39C File Offset: 0x000CB59C
		public bool \u0001(\u0016 \u0002)
		{
			foreach (_IVariable ivariable in \u0002.signRef.AllVariables)
			{
				_IVariable ivariable2 = \u0002.signCompiled[ivariable.Id] as _IVariable;
				if (ivariable2 == null && ivariable.GetFlag(VarFlag.Implicit))
				{
					ivariable2 = (ivariable.Duplicate() as _IVariable);
					ivariable2.Id = ivariable.Id;
					\u0002.signCompiled.AddVariable(ivariable2);
					ivariable2 = (\u0002.signCompiled[ivariable.Id] as _IVariable);
				}
				if (ivariable2 != null)
				{
					ivariable2.DataLocation = ivariable.DataLocation;
					ivariable2.SetFlag(VarFlag.Absolut, ivariable.GetFlag(VarFlag.Absolut));
					ivariable2.SetFlag(VarFlag.RelativeInstance, ivariable.GetFlag(VarFlag.RelativeInstance));
					ivariable2.SetFlag(VarFlag.RelativeStack, ivariable.GetFlag(VarFlag.RelativeStack));
				}
			}
			return true;
		}
	}
}
