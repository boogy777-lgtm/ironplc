using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICodegenerator9 : ICodegenerator8, ICodegenerator7, ICodegenerator6, ICodegenerator5, ICodegenerator4, ICodegenerator3, ICodegenerator2, ICodegenerator
	{
		void GenerateVarRelative(IVariableExpression varexp, int nOffset, IIndexInfo indexinfo, ICompiledType ctype, int nTypeSize, IAccessMode am, bool bMisaligned);
	}
}
