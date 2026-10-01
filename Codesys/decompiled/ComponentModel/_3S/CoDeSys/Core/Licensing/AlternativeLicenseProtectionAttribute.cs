using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Licensing
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
	[ReleasedClass]
	public class AlternativeLicenseProtectionAttribute : Attribute
	{
		[CompilerGenerated]
		private readonly string[] \u0001;

		public string[] Data
		{
			[CompilerGenerated]
			get
			{
				//IL_0009: Incompatible stack heights: 0 vs 1
				return ((AlternativeLicenseProtectionAttribute)/*Error near IL_0007: Stack underflow*/).\u0001;
			}
		}

		public AlternativeLicenseProtectionAttribute(params string[] data)
		{
			\u0001 = data;
		}
	}
}
