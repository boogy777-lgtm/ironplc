using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface INewExpression2 : INewExpression, IExpression2, IExpression, IExprement
	{
		IEnumerable<IAssignmentExpression> FBInitParams { get; set; }
	}
}
