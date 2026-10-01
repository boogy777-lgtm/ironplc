using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IPartialAccessExpression : _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IPartialAccessExpression
	{
		_IExpression _Left { get; set; }

		new DirectVariableSize PartSize { get; set; }

		new int PartOffset { get; set; }

		bool GetByteOffsetAndSize(IScope5 scope, out int byteOffset, out int byteSize);
	}
}
