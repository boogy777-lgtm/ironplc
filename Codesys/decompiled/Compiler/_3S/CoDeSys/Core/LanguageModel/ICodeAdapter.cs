using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.TargetSettings;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICodeAdapter
	{
		ICompiledPOU CurrentPOU { get; }

		IExprement CurrentExpression { get; }

		IDataManager DataManager { get; }

		ITargetSettings TargetSettings { get; }

		bool AddError(string stError);

		void Generate(IExprement expr);

		void GenerateExternalConversion(TypeClass tcFrom, TypeClass tcTo, params IExpression[] Operands);

		void GenerateExternalFunctionCall(string stFunName, params IExpression[] Operands);

		void GenerateExternalFunctionCallWithType(string stFunName, TypeClass tc, params IExpression[] Operands);

		ISignature FindSignature(int nSignatureId);

		int GetInstancePtrOffset(ISignature signature);

		bool IsCommutative(Operator op);

		bool IsComparison(Operator op);

		Operator NegateCondition(Operator opCondition);

		int GetTypeSize(TypeClass tc);

		bool IsSigned(TypeClass tc);

		bool IsReal(TypeClass tc);

		bool IsLType(TypeClass tc);

		bool IsLInteger(TypeClass tc);

		bool IsScalar(TypeClass tc);

		bool IsString(TypeClass tc);

		string GetOperatorText(Operator op);

		string MakeUniqueLabel(string st);

		IJumpTable[] MakeJumpTables(ICaseStatement casest, int nMinCount);
	}
}
