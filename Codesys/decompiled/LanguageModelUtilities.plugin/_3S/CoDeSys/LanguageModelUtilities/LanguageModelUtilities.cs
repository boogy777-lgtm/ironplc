using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[TypeGuid("{1C9DCA7D-434A-4a1f-8DDC-0180B852AAE9}")]
	[SystemInterface("_3S.CoDeSys.LanguageModelUtilities.ILanguageModelUtilities")]
	public class LanguageModelUtilities : ILanguageModelUtilities2, ILanguageModelUtilities
	{
		private PreCompileUtilities _precomUtils = new PreCompileUtilities();

		private CompileUtilities _compUtils = new CompileUtilities();

		public IPreCompileUtilities PreCompileUtils => _precomUtils;

		public ICompileUtilities CompileUtils => _compUtils;
	}
}
