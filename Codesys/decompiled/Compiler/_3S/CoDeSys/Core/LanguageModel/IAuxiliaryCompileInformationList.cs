using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IAuxiliaryCompileInformationList
	{
		IAuxiliaryCompileInformation this[Guid id] { get; }

		IAuxiliaryCompileInformation[] GetAllOfType(string stType);

		IAuxiliaryCompileInformation[] GetAll();
	}
}
