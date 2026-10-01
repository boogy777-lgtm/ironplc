using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IVariableDeclarationStatement : _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IVariableDeclarationStatement
	{
		IList<_IExpression> NameList { get; }

		_IType Type { get; set; }

		_IExpression Initial { get; set; }

		IDirectVariable Address { get; set; }

		ICollection<_IAssignmentExpression> InputAssigns { get; }

		bool OldInputAssigns { get; set; }

		bool RefAssignInitialisation { get; set; }

		void AddName(_IExpression exp);

		void SetFormalParam(_IExpression expInput, int iIndex);

		void SetActualParam(_IExpression expInput, int iIndex);

		void AddParam(_IExpression exp, _IExpression expVariable);
	}
}
