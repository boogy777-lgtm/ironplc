using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0010
{
	// Token: 0x02000151 RID: 337
	internal sealed class \u0002
	{
		// Token: 0x0600179C RID: 6044 RVA: 0x00048A94 File Offset: 0x00046C94
		public \u0002(_IPreCompileContext \u001E\u0003)
		{
			this.OverrideSignaturesPrecompileContext = \u001E\u0003;
			_ILibraryTable ilibraryTable = \u001E\u0003._GetLibraryTable(\u001E\u0003.ApplicationGuid);
			foreach (_ISignature isignature in \u001E\u0003._AllSignatures)
			{
				string attributeValue = isignature.GetAttributeValue(CompileAttributes.ATTRIBUTE_SIGNATURE_OVERLOAD);
				if (!string.IsNullOrEmpty(attributeValue))
				{
					string value = attributeValue;
					string text = isignature.GetAttributeValue(CompileAttributes.ATTRIBUTE_SIGNATURE_OVERLOAD_LIBRARY) ?? string.Empty;
					string attributeValue2 = isignature.GetAttributeValue(CompileAttributes.ATTRIBUTE_SIGNATURE_OVERLOAD_NAMESPACE);
					if (!string.IsNullOrEmpty(attributeValue2) && string.IsNullOrEmpty(text))
					{
						text = (ilibraryTable.GetLibraryIdByNamespace(attributeValue2) ?? string.Empty);
					}
					LList<KeyValuePair<_ISignature, string>> llist;
					if (!this.\u0001.TryGetValue(text, ref llist))
					{
						llist = new LList<KeyValuePair<_ISignature, string>>();
						this.\u0001.Add(text, llist);
					}
					llist.Add(new KeyValuePair<_ISignature, string>(isignature, value));
				}
			}
		}

		// Token: 0x0600179D RID: 6045 RVA: 0x00048BA0 File Offset: 0x00046DA0
		internal ICollection<KeyValuePair<_ISignature, string>> \u0001(string \u0002)
		{
			if (\u0002 == null)
			{
				\u0002 = string.Empty;
			}
			LList<KeyValuePair<_ISignature, string>> result;
			if (!this.\u0001.TryGetValue(\u0002, ref result))
			{
				return new LList<KeyValuePair<_ISignature, string>>();
			}
			return result;
		}

		// Token: 0x17000577 RID: 1399
		// (get) Token: 0x0600179E RID: 6046 RVA: 0x00048BD0 File Offset: 0x00046DD0
		internal _IPreCompileContext OverrideSignaturesPrecompileContext { get; }

		// Token: 0x0400042E RID: 1070
		private readonly LDictionary<string, LList<KeyValuePair<_ISignature, string>>> \u0001 = new LDictionary<string, LList<KeyValuePair<_ISignature, string>>>();

		// Token: 0x0400042F RID: 1071
		[CompilerGenerated]
		private readonly _IPreCompileContext \u0001;
	}
}
