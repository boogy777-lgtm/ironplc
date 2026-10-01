using System;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u000F
{
	// Token: 0x020000EE RID: 238
	internal static class \u0005
	{
		// Token: 0x06001043 RID: 4163 RVA: 0x0002DE3C File Offset: 0x0002C03C
		internal static IExternalReference[] \u0001(_ICompileContext \u0002, bool \u0003)
		{
			LList<IExternalReference> llist = new LList<IExternalReference>();
			string[] functionsToLinkAlways = \u0002.Codegenerator.FunctionsToLinkAlways;
			foreach (_ISignature isignature in \u0002.AllSignatureList)
			{
				string text = isignature.Name;
				if (isignature.HasAttribute(CompileAttributes.ATTRIBUTE_EXTERNAL_NAME))
				{
					text = isignature.GetAttributeValue(CompileAttributes.ATTRIBUTE_EXTERNAL_NAME);
				}
				\u0005.\u0001(llist, isignature, text);
				if (isignature.GetFlag(SignatureFlag.External) && isignature.FPDataLocation != null && isignature.POUType == Operator.Function)
				{
					uint uiCrc = \u0005.\u0001(isignature);
					if (\u0005.\u0001(\u0003, functionsToLinkAlways, isignature))
					{
						llist.Add(new ExternalReference(text, isignature.FPDataLocation, isignature, uiCrc));
					}
				}
			}
			IExternalReference[] array = new IExternalReference[llist.Count];
			llist.CopyTo(array);
			return array;
		}

		// Token: 0x06001044 RID: 4164 RVA: 0x0002DF2C File Offset: 0x0002C12C
		private static bool \u0001(bool \u0002, string[] \u0003, _ISignature \u0004)
		{
			bool flag = !\u0002 || \u0004.IsLibraryObject || \u0004.CallerIds.Length != 0 || \u0004.HasAttribute("DoLink");
			if (!flag && \u0003 != null && \u0003.Length != 0)
			{
				foreach (string text in \u0003)
				{
					if (\u0004.Name == text.ToUpperInvariant())
					{
						flag = true;
						break;
					}
				}
			}
			return flag;
		}

		// Token: 0x06001045 RID: 4165 RVA: 0x0002DF94 File Offset: 0x0002C194
		private static uint \u0001(_ISignature \u0002)
		{
			string attributeValue = \u0002.GetAttributeValue(CompileAttributes.ATTRIBUTE_SIGNATURE_CRC);
			uint result;
			try
			{
				result = uint.Parse(attributeValue);
			}
			catch
			{
				result = 0U;
			}
			return result;
		}

		// Token: 0x06001046 RID: 4166 RVA: 0x0002DFCC File Offset: 0x0002C1CC
		private static void \u0001(LList<IExternalReference> \u0002, _ISignature \u0003, string \u0004)
		{
			foreach (object obj in \u0003._SubSignatures)
			{
				_ISignature isignature = (_ISignature)obj;
				if (isignature.GetFlag(SignatureFlag.External) && isignature.FPDataLocation != null)
				{
					uint uiCrc = \u0005.\u0001(isignature);
					string stName = \u0004 + "__" + isignature.Name;
					if (isignature.Name.StartsWith("__"))
					{
						stName = \u0004 + isignature.Name;
					}
					\u0002.Add(new ExternalReference(stName, isignature.FPDataLocation, isignature, uiCrc));
				}
			}
		}
	}
}
