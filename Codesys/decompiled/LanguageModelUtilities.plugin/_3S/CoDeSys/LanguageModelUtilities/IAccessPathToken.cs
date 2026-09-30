namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal interface IAccessPathToken
	{
		string StringRepresentation { get; }

		bool NeedsSeparator { get; }
	}
}
