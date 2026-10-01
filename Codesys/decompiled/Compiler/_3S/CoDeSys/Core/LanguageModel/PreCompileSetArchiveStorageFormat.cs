using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	public enum PreCompileSetArchiveStorageFormat
	{
		[ReleasedEnumMember]
		NotStoredAsCompiledLibrary,
		[ReleasedEnumMember]
		ClassicalFormat,
		[ReleasedEnumMember]
		MemoryOptimizedFormat
	}
}
