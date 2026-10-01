using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.Legacy
{
	// Token: 0x02000274 RID: 628
	internal class GenericIntegerAttribute : GenericCheckedAttribute
	{
		// Token: 0x06002A48 RID: 10824 RVA: 0x0006B5C0 File Offset: 0x0006A5C0
		public GenericIntegerAttribute(string name, string description, Version compilerVersion, AttributeScope scope, long minValue, long maxValue) : base(name, description, compilerVersion, scope, delegate(string value, AttributeScope checkedScope, ISignature sign, IVariable var)
		{
			long num;
			if (!long.TryParse(value, out num))
			{
				return string.Format(Strings.AttributeInvalidTypeInteger, value, name);
			}
			if (num < minValue || num > maxValue)
			{
				return string.Format(Strings.AttributeInvalidRange, new object[]
				{
					value,
					name,
					minValue,
					maxValue
				});
			}
			return string.Empty;
		})
		{
		}
	}
}
