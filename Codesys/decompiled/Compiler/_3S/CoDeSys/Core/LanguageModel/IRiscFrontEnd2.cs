using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRiscFrontEnd2 : IRiscFrontEnd
	{
		IRegister Generate(IExpression expr);
	}
}
