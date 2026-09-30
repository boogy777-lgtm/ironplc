using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICodegenerator7 : ICodegenerator6, ICodegenerator5, ICodegenerator4, ICodegenerator3, ICodegenerator2, ICodegenerator
	{
		bool SupportsTryCatch { get; }

		bool GenerateInstanceAccess(IDeRefAccessExpression deref, int nOffset, IIndexInfo indexinfo, ICompiledType ctype, IAccessMode am, bool bMisaligned);

		bool Generate(IDeRefAccessExpression deref, int nOffset, IIndexInfo indexinfo, ICompiledType ctype, IAccessMode am, bool bMisaligned);
	}
}
