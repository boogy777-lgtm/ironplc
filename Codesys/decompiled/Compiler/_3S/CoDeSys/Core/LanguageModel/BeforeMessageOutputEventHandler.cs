using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedDelegate]
	public delegate void BeforeMessageOutputEventHandler(object sender, MessageOutputEventArgs e);
}
