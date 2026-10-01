using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICodegenerator8 : ICodegenerator7, ICodegenerator6, ICodegenerator5, ICodegenerator4, ICodegenerator3, ICodegenerator2, ICodegenerator
	{
		bool NeedsAtomicAccessCall(TypeClass tc, bool bWrite);
	}
}
