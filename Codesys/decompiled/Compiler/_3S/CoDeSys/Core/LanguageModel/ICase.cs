using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICase
	{
		ICaseLabelStatement Label { get; }

		IStatement Controlled { get; }
	}
}
