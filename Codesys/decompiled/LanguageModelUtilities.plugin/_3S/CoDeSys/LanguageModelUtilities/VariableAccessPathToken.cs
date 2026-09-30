using System.Diagnostics;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[DebuggerDisplay("{StringRepresentation}")]
	internal sealed class VariableAccessPathToken : IAccessPathToken
	{
		public string StringRepresentation { get; private set; }

		public bool NeedsSeparator => true;

		internal VariableAccessPathToken(string stVariableExpression)
		{
			StringRepresentation = stVariableExpression;
		}
	}
}
