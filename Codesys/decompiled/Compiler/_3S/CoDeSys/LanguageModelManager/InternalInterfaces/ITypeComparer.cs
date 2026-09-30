using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ITypeComparer
	{
		bool Imitates(TypeClass tc1, TypeClass tc2, bool bTreatLRealAsReal, bool bTreatInt64AsInt32, bool b64BitPointer);

		bool IsEqual(ICompiledType type1, ICompiledType type2, ICommonScope scope);

		bool IsEqual(ICompiledType type1, ICompiledType type2, ICommonScope scope1, ICommonScope scope2);

		bool IsEqual(ICompiledType type1, ICompiledType type2, ICommonScope scope1, ICommonScope scope2, bool bForVarInout);

		bool IsEqual(_IUserdefType udt1, _IUserdefType udt2, IScope2 scope1, IScope2 scope2);

		bool IsImplicitConvertable(ISignature signSource, ISignature signDest, IScope2 scope);

		bool IsImplicitConvertable(ICompiledType typeSource, ICompiledType typeDest, ICommonScope scope);

		bool IsImplicitPointerConversion(ICompiledType typeSource, ICompiledType typeDest, ICommonScope scope);

		bool IsCopyable(ICompiledType typeSource, ICompiledType typeDest, ICommonScope scopeSource, ICommonScope scopeDest);

		bool IsImplicitConvertable(ISignature signSource, ISignature signDest, IScope2 scopeSource, IScope2 scopeDest);

		bool IsInterface(ICompiledType ctype, ICommonScope scope);

		bool IsImplicitLiteralConvertable(_ILiteralExpression litexp, ICompiledType typeSource, ICompiledType typeDest, ICommonScope scopeSource, ICommonScope scopeDest);

		ICompiledType EvaluateAliasAndEnumType(ICompiledType ctype, ICommonScope scope);

		bool IsImplicitPointerConversion(ICompiledType typeSource, ICompiledType typeDest, ICommonScope scopeSource, ICommonScope scopeDest);

		bool IsImplicitConvertable(ICompiledType typeSource, ICompiledType typeDest, ICommonScope scopeSource, ICommonScope scopeDest);
	}
}
