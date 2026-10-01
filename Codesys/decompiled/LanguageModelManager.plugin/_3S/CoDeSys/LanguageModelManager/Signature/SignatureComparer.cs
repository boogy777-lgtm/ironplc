using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.Signature
{
	// Token: 0x0200025F RID: 607
	public static class SignatureComparer
	{
		// Token: 0x06002989 RID: 10633 RVA: 0x000692FC File Offset: 0x000682FC
		public static bool IsEqualCompile(Signature signIn1, Signature signIn2, bool bCompareInitValues)
		{
			return SignatureComparer.IsEqualIds(signIn1, signIn2) && SignatureComparer.IsEqualSubSignatures(signIn1, signIn2) && SignatureComparer.HasEqualAttributes(signIn1, signIn2) && !(signIn1.Name != signIn2.Name) && signIn1.POUType == signIn2.POUType && SignatureComparer.IsEqualNonImplicitVariables(signIn1, signIn2, bCompareInitValues) && SignatureComparer.IsEqualRelevantFlags(signIn1, signIn2);
		}

		// Token: 0x0600298A RID: 10634 RVA: 0x00069368 File Offset: 0x00068368
		private static bool IsEqualRelevantFlags(Signature signIn1, Signature signIn2)
		{
			SignatureFlag signatureFlag = SignatureFlag.OnlineChanged | SignatureFlag.Located;
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35600)
			{
				signatureFlag = SignatureFlag.OnlineChanged;
			}
			return (signIn1.Flags & ~signatureFlag) == (signIn2.Flags & ~signatureFlag);
		}

		// Token: 0x0600298B RID: 10635 RVA: 0x000693AC File Offset: 0x000683AC
		private static bool IsEqualNonImplicitVariables(Signature signIn1, Signature signIn2, bool bCompareInitValues)
		{
			object varlock = signIn1._varlock;
			lock (varlock)
			{
				object varlock2 = signIn2._varlock;
				lock (varlock2)
				{
					_IVariable[] array = (from v in signIn1.AllVariables
					where !v.GetFlag(VarFlag.Implicit)
					select v).ToArray<_IVariable>();
					_IVariable[] array2 = (from v in signIn2.AllVariables
					where !v.GetFlag(VarFlag.Implicit)
					select v).ToArray<_IVariable>();
					if (array.Length != array2.Length)
					{
						return false;
					}
					for (int i = 0; i < array.Length; i++)
					{
						IVariable variable = array[i];
						_IVariable varRight = array2[i];
						if (!variable.IsEqual(varRight, bCompareInitValues))
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		// Token: 0x0600298C RID: 10636 RVA: 0x000694B0 File Offset: 0x000684B0
		private static bool IsEqualSubSignatures(Signature signIn1, Signature signIn2)
		{
			return (signIn1.SubSignatureTable == null || signIn2.SubSignatureTable != null) && (signIn1.SubSignatureTable != null || signIn2.SubSignatureTable == null) && (signIn1.SubSignatureTable == null || signIn1.SubSignatureTable.IsEqual(signIn2.SubSignatureTable));
		}

		// Token: 0x0600298D RID: 10637 RVA: 0x00069500 File Offset: 0x00068500
		private static bool IsEqualIds(Signature signIn1, Signature signIn2)
		{
			if (signIn1.BaseSignatureId != signIn2.BaseSignatureId)
			{
				return false;
			}
			if (signIn1.Id != signIn2.Id)
			{
				return false;
			}
			int[] interfaceIds = signIn1.InterfaceIds;
			int[] interfaceIds2 = signIn2.InterfaceIds;
			if (interfaceIds.Length != interfaceIds2.Length)
			{
				return false;
			}
			for (int i = 0; i < interfaceIds.Length; i++)
			{
				if (interfaceIds[i] != interfaceIds2[i])
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x0600298E RID: 10638 RVA: 0x00069560 File Offset: 0x00068560
		private static bool HasEqualAttributes(Signature this1, Signature other)
		{
			object attributesLock = this1._attributesLock;
			bool result;
			lock (attributesLock)
			{
				object attributesLock2 = other._attributesLock;
				lock (attributesLock2)
				{
					IDictionary<string, string> attributeTable = other.AttributeTable;
					result = CompilerProxy.ComparisonService.HaveEqualAttributes(attributeTable, this1.AttributeTable, IgnoreAttributes.AllForSignature);
				}
			}
			return result;
		}
	}
}
