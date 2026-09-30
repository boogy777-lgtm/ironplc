using System;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0006
{
	// Token: 0x02000060 RID: 96
	internal static class \u0001
	{
		// Token: 0x06000685 RID: 1669 RVA: 0x0000DC44 File Offset: 0x0000BE44
		internal static bool \u0001(Type \u0002, string \u0003)
		{
			bool flag = false;
			Type[] interfaces = \u0002.GetInterfaces();
			int num = 0;
			while (!flag && num < interfaces.Length)
			{
				flag = \u0003.Equals(interfaces[num].FullName);
				if (!flag)
				{
					num++;
				}
			}
			return flag;
		}

		// Token: 0x06000686 RID: 1670 RVA: 0x0000DC80 File Offset: 0x0000BE80
		public static string \u0001(IScope5 \u0002, _ISignature \u0003)
		{
			string text = string.Empty;
			if (\u0003.ParentSignatureId != Helper.InvalidId)
			{
				text = "???";
				ISignature signature = \u0002[\u0003.ParentSignatureId];
				if (signature != null)
				{
					text = signature.OrgName;
				}
			}
			string text2 = \u0006.\u0001.\u0001(\u0003);
			if (string.IsNullOrEmpty(text))
			{
				return text2;
			}
			if (string.IsNullOrEmpty(text2))
			{
				return text;
			}
			return text + "." + text2;
		}

		// Token: 0x06000687 RID: 1671 RVA: 0x0000DCE4 File Offset: 0x0000BEE4
		public static string \u0001(_ISignature \u0002)
		{
			if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_PROPERTY))
			{
				string text = \u0002.OrgName.Substring(5);
				if (\u0002.Name.StartsWith("__GET", StringComparison.OrdinalIgnoreCase))
				{
					return text + ".Get";
				}
				if (\u0002.Name.StartsWith("__SET", StringComparison.OrdinalIgnoreCase))
				{
					return text + ".Set";
				}
				return text;
			}
			else
			{
				if (\u0002.OrgName != IdentifierConstants.MainSignatureName)
				{
					return \u0002.OrgName;
				}
				return string.Empty;
			}
		}
	}
}
