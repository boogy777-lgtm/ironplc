using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IArrayType : _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, IArrayType2, IArrayType
	{
		IList<_IArrayDimension> _Dimensions { get; }

		_IType _Base { get; set; }

		_IType OriginalBaseType { get; }

		void AddDimension(_IExpression expLower, _IExpression expUpper);
	}
}
