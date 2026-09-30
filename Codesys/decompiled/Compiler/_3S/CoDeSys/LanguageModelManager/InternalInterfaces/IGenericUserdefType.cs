using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IGenericUserdefType : _IUserdefType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, IUserdefType2, IUserdefType
	{
		IEnumerable<_IExpression> GenericConstantsInitializations { get; }

		void AddGenericConstantInitialization(_IExpression exp);

		void SetGenericConstantInitialization(_IExpression exp, int index);

		_IUserdefType GetNonGenericBaseType();
	}
}
