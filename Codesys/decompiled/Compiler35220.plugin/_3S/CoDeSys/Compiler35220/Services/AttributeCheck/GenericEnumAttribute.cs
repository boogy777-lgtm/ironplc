using System;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0003;
using \u0011;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.Compiler35220.Services.AttributeCheck
{
	// Token: 0x02000136 RID: 310
	internal sealed class GenericEnumAttribute : global::\u0003.\u0005
	{
		// Token: 0x060015D4 RID: 5588 RVA: 0x0003FFCC File Offset: 0x0003E1CC
		public GenericEnumAttribute(string name, string description, Version compilerVersion, AttributeScope scope, string[] options)
		{
			GenericEnumAttribute.\u0001 u = new GenericEnumAttribute.\u0001();
			u.\u0001 = options;
			u.\u0001 = name;
			base..ctor(u.\u0001, description, compilerVersion, scope, new Func<string, AttributeScope, ISignature, IVariable, string>(u.\u0001));
		}

		// Token: 0x02000137 RID: 311
		[CompilerGenerated]
		private new sealed class \u0001
		{
			// Token: 0x060015D6 RID: 5590 RVA: 0x00040014 File Offset: 0x0003E214
			internal string \u0001(string \u0002, AttributeScope \u0003, ISignature \u0004, IVariable \u0005)
			{
				if (!string.IsNullOrEmpty(\u0002))
				{
					foreach (string value in this.\u0001)
					{
						if (\u0002.Equals(value, StringComparison.InvariantCultureIgnoreCase))
						{
							return "";
						}
					}
				}
				string arg = "[" + string.Join(", ", this.\u0001.Select(new Func<string, string>(GenericEnumAttribute.<>c.<>9.\u0001))) + "]";
				return string.Format(global::\u0011.\u0001.AttributeValueMustBeOneOf, \u0002, this.\u0001, arg);
			}

			// Token: 0x040003C9 RID: 969
			public string[] \u0001;

			// Token: 0x040003CA RID: 970
			public string \u0001;
		}
	}
}
