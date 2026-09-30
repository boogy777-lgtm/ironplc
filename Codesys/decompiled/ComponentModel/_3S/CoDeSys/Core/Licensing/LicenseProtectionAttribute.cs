using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Licensing
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
	[ReleasedClass]
	public class LicenseProtectionAttribute : Attribute
	{
		private string[] \u0001;

		public string[] Data => ((LicenseProtectionAttribute)/*Error near IL_0007: Stack underflow*/).\u0001;

		public LicenseProtectionAttribute(params string[] data)
		{
			\u0001 = data;
		}
	}
}
