using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001E
{
	// Token: 0x02000407 RID: 1031
	internal static class \u001B
	{
		// Token: 0x06003965 RID: 14693 RVA: 0x000EDAD4 File Offset: 0x000EBCD4
		public static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003)
		{
			if (\u0002 == null)
			{
				throw new ArgumentNullException("comconNew");
			}
			if (\u0003 == null)
			{
				return;
			}
			foreach (_ISignature isignature in \u0002.AllFlat)
			{
				_ISignature u = \u0003[isignature.Id];
				\u001B.\u0001(isignature, u, \u0003);
			}
		}

		// Token: 0x06003966 RID: 14694 RVA: 0x000EDB44 File Offset: 0x000EBD44
		private static void \u0001(_ISignature \u0002, _ISignature \u0003, _ICompileContext \u0004)
		{
			if (\u0003 == null)
			{
				return;
			}
			foreach (int nId in \u0003.ReferencerIds)
			{
				ICompiledPOU compiledPOUById = \u0004.GetCompiledPOUById(nId);
				if (compiledPOUById != null && !compiledPOUById.GetFlag(CompiledPOUFlags.ToTypify))
				{
					\u0002.AddReferencer(nId);
				}
			}
			foreach (_IVariable ivariable in \u0002.AllVariables)
			{
				_IVariable u = (_IVariable)\u0003[ivariable.Id];
				\u001B.\u0001(ivariable, u, \u0004);
			}
		}

		// Token: 0x06003967 RID: 14695 RVA: 0x000EDBEC File Offset: 0x000EBDEC
		private static void \u0001(_IVariable \u0002, _IVariable \u0003, _ICompileContext \u0004)
		{
			if (\u0003 == null)
			{
				return;
			}
			foreach (ICrossReference crossReference in \u0003.CrossReferences)
			{
				ICompiledPOU compiledPOUById = \u0004.GetCompiledPOUById(crossReference.CodeId);
				if (compiledPOUById != null && !compiledPOUById.GetFlag(CompiledPOUFlags.ToTypify))
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
						ICompiledPOU compiledPOUById2 = \u0004.GetCompiledPOUById(num);
						if (compiledPOUById2 != null && !compiledPOUById2.GetFlag(CompiledPOUFlags.ToTypify))
						{
							variableWithModifyingAccesses.AddModifyingCrossReference(num);
						}
					}
				}
			}
		}
	}
}
