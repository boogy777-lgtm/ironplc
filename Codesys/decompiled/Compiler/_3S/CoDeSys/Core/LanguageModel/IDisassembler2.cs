using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IDisassembler2 : IDisassembler
	{
		bool GenerateDisassembleCode { get; set; }
	}
}
