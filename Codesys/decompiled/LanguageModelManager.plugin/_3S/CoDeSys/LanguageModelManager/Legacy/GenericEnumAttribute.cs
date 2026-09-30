using System;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.Legacy
{
	// Token: 0x02000273 RID: 627
	internal class GenericEnumAttribute : GenericCheckedAttribute
	{
		// Token: 0x06002A47 RID: 10823 RVA: 0x0006B580 File Offset: 0x0006A580
		public GenericEnumAttribute(string name, string description, Version compilerVersion, AttributeScope scope, string[] options) : base(name, description, compilerVersion, scope, delegate(string value, AttributeScope checkedScope, ISignature sign, IVariable var)
		{
			if (!string.IsNullOrEmpty(value))
			{
				foreach (string value2 in options)
				{
					if (value.Equals(value2, StringComparison.InvariantCultureIgnoreCase))
					{
						return "";
					}
				}
			}
			string arg = "[" + string.Join(", ", from o in options
			select "'" + o + "'") + "]";
			return string.Format(Strings.AttributeValueMustBeOneOf, value, name, arg);
		})
		{
		}
	}
}
