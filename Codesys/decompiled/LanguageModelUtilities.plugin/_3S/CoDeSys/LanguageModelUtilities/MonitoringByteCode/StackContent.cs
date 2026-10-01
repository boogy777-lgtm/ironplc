namespace _3S.CoDeSys.LanguageModelUtilities.MonitoringByteCode
{
	internal class StackContent
	{
		private readonly ByteProgramCreator.EResultMode _resultMode;

		private readonly int _bytesOnFStack;

		internal ByteProgramCreator.EResultMode ResultMode => _resultMode;

		public int ValueSizeInBytes => _bytesOnFStack;

		public StackContent(ByteProgramCreator.EResultMode resultMode, int bytesOnFStack)
		{
			_resultMode = resultMode;
			_bytesOnFStack = bytesOnFStack;
		}
	}
}
