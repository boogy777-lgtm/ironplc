using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Services
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	internal struct ErrorInformation : IWhiteErrorInformation
	{
		public IWhiteErrorStatement ErrorStatement
		{
			[IsReadOnly]
			get;
			set; }

		public int CharacterOffset
		{
			[IsReadOnly]
			get;
			set; }

		public ErrorInformation(IWhiteErrorStatement errorStatement, int characterOffset)
		{
			ErrorStatement = errorStatement;
			CharacterOffset = characterOffset;
		}
	}
}
