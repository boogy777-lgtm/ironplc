namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal enum EState
	{
		Start,
		IdentifierFound,
		ColonFound,
		TypeFound,
		NamspaceFound,
		CommentAfterTypeFound,
		AssignFound,
		LeftParenthesisFound,
		End,
		Error,
		CalleeFound,
		IgnoreParenthesis,
		IgnoreBrackets,
		DotFound,
		ExpectDotOrColon,
		ExpectStructMemberInitialization,
		InitializationSkipped,
		SkipInitialization,
		SkipPreviousStructMemberInitialization,
		StructMemberInitializationSkipped
	}
}
