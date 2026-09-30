using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IVariableDeclarationListStatement : _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IVariableDeclarationListStatement
	{
		new VarFlag Flags { get; set; }

		_IStatement VariableDeclaration { get; set; }

		bool GetFlag(VarFlag vfFlag);

		void SetFlag(VarFlag vfFlag, bool bSetTrue);

		void SetVariableDeclarations(IList<IVariableDeclarationStatement> declarations);
	}
}
