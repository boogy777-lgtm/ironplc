using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IGreenTreeConverter
	{
		_IExprement BuildGreenTree(_IExprement exp, IGreenTreeTables tables, ITreeFactory factory, ICompactedParseTreeInformation parseTreeInfo);

		_IExprement BuildRedTree(_IExprement exp, ITreeFactory factory, ICompactedParseTreeInformation parseTreeInfo);
	}
}
