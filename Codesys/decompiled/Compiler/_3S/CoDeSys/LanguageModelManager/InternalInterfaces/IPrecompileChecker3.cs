using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IPrecompileChecker3 : IPrecompileChecker2, IPrecompileChecker, IExprementVisitor2, IExprementVisitor, IExprementVisitor3590, IOperatorExpressionVisitor, IOperatorExpressionVisitor5, IOperatorExpressionVisitor4, IOperatorExpressionVisitor3, IOperatorExpressionVisitor2
	{
		void CheckStatement(_IStatement stmt, _ISignature signLocal, out IEnumerable<IMessage> messages);
	}
}
