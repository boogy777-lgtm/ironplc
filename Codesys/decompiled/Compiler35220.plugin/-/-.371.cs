using System;
using \u0005;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0001
{
	// Token: 0x020003BD RID: 957
	internal static class \u0014
	{
		// Token: 0x060036C5 RID: 14021 RVA: 0x000DE724 File Offset: 0x000DC924
		internal static void \u0001(_ISignature \u0002, _ISignature \u0003, _ICompileContext \u0004)
		{
			if (\u0002.POUType != Operator.Method)
			{
				return;
			}
			_ISignature isignature = \u0004[\u0002.ParentSignatureId];
			_IUserdefType iuserdefType = \u0019.\u0003.\u0001(isignature.Name);
			_ISignature isignature2 = global::\u0005.\u0008.\u0001(\u0004, isignature);
			if (isignature2 != null)
			{
				iuserdefType.SignatureId = isignature2.Id;
			}
			else
			{
				iuserdefType.SignatureId = isignature.Id;
			}
			_IVariable ivariable = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001());
			ivariable.Name = IdentifierConstants.InstancePointer;
			ivariable._Type = \u0019.\u0003.\u0001(iuserdefType);
			ivariable.SetFlag(VarFlag.Input | VarFlag.IsCompiled | VarFlag.Implicit | VarFlag.Typified, true);
			if (\u0003 != null && \u0003[ivariable.VersionedName] != null)
			{
				ivariable.Id = \u0003[ivariable.VersionedName].Id;
			}
			else
			{
				ivariable.Id = \u0002.NextId;
			}
			\u0002.AddVariable(ivariable);
		}
	}
}
