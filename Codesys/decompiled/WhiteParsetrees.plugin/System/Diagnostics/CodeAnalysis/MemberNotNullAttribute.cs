using System.Runtime.CompilerServices;

namespace System.Diagnostics.CodeAnalysis
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
	[ExcludeFromCodeCoverage]
	[DebuggerNonUserCode]
	internal sealed class MemberNotNullAttribute : Attribute
	{
		public string[] Members { get; }

		public MemberNotNullAttribute(string member)
		{
			Members = new string[1] { member };
		}

		public MemberNotNullAttribute(params string[] members)
		{
			Members = members;
		}
	}
}
