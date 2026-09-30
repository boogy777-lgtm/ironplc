using System;
using \u0011;
using \u0018;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0084;

namespace \u0080
{
	// Token: 0x0200036F RID: 879
	internal sealed class \u0016 : \u001E
	{
		// Token: 0x06003436 RID: 13366 RVA: 0x000CD4B4 File Offset: 0x000CB6B4
		public bool \u0001(\u0018.\u0010 \u0002)
		{
			\u0080.\u0016.\u0001(\u0002);
			return true;
		}

		// Token: 0x06003437 RID: 13367 RVA: 0x000CD4C0 File Offset: 0x000CB6C0
		private static void \u0001(\u0018.\u0010 \u0002)
		{
			foreach (global::\u0011.\u0014 u in \u0002.changedSignatures)
			{
				_ISignature u2 = \u0002.ComconNew[u.CompiledSignature.Id];
				_ISignature u3 = \u0002.ComconOld[u.CompiledSignature.Id];
				\u0080.\u0016.\u0001(u2, u3, \u0002);
			}
		}

		// Token: 0x06003438 RID: 13368 RVA: 0x000CD53C File Offset: 0x000CB73C
		private static void \u0001(_ISignature \u0002, _ISignature \u0003, \u0018.\u0010 \u0004)
		{
			if (\u0002 == null || \u0003 == null)
			{
				return;
			}
			foreach (int num in \u0003.ReferencerIds)
			{
				if (!\u0004.changedpous.ContainsKey(num))
				{
					\u0002.AddReferencer(num);
				}
			}
			foreach (_IVariable ivariable in \u0002.AllVariables)
			{
				_IVariable u = (_IVariable)\u0003[ivariable.Id];
				\u0080.\u0016.\u0001(ivariable, u, \u0004);
			}
		}

		// Token: 0x06003439 RID: 13369 RVA: 0x000CD5D8 File Offset: 0x000CB7D8
		private static void \u0001(_IVariable \u0002, _IVariable \u0003, \u0018.\u0010 \u0004)
		{
			if (\u0003 == null)
			{
				return;
			}
			foreach (ICrossReference crossReference in \u0003.CrossReferences)
			{
				if (!\u0004.changedpous.ContainsKey(crossReference.CodeId))
				{
					\u0002.AddCrossReference(crossReference.CodeId, null);
				}
			}
			IVariableWithModifyingAccesses variableWithModifyingAccesses = \u0002 as IVariableWithModifyingAccesses;
			if (variableWithModifyingAccesses != null)
			{
				IVariableWithModifyingAccesses variableWithModifyingAccesses2 = \u0003 as IVariableWithModifyingAccesses;
				if (variableWithModifyingAccesses2 != null)
				{
					foreach (int num in variableWithModifyingAccesses2.GetModifyingCrossReferences())
					{
						if (!\u0004.changedpous.ContainsKey(num))
						{
							variableWithModifyingAccesses.AddModifyingCrossReference(num);
						}
					}
				}
			}
		}
	}
}
