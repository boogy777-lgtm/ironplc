using System;
using System.Xml;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IAuxiliaryCompileInformation
	{
		Guid Id { get; }

		string Type { get; }

		XmlNode Content { get; }
	}
}
