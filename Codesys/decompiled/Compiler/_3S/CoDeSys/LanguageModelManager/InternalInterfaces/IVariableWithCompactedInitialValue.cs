using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IVariableWithCompactedInitialValue
	{
		ICompactedParseTreeInformation CompactedInitialValueInformation { get; set; }

		_IExpression OriginalInitial { get; }
	}
}
