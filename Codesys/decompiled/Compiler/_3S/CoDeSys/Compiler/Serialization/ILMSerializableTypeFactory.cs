using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler.Serialization
{
	[ReleasedInterface]
	public interface ILMSerializableTypeFactory : _ILanguageModelBuilder2, _ILanguageModelBuilder, ILanguageModelBuilder12, ILanguageModelBuilder11, ILanguageModelBuilder10, ILanguageModelBuilder9, ILanguageModelBuilder8, ILanguageModelBuilder7, ILanguageModelBuilder6, ILanguageModelBuilder5, ILanguageModelBuilder4, ILanguageModelBuilder3, ILanguageModelBuilder2, ILanguageModelBuilder
	{
		IVirtualFunctionTableSerializable CreateVirtualFunctionTable();

		_IFunctionPointerEntry CreateVirtualFunctionTableFunctionPointerEntry();

		IInterfaceOffsetEntrySerializable CreateVirtualFunctionTableInterfaceOffsetEntry();

		_ISubrangeType CreateSubrangeType();

		IImplicitEnumerationType CreateImplicitEnumerationType();

		_IAnyBitButBoolIsPreferred CreateAnyBitButBoolIsPreferred();

		_IXStringType CreateXStringType();

		_IVectorType CreateVectorType();

		_IParamsType CreateParamsType();

		_IEnumType CreateEnumType();

		_IAliasType CreateAliasType();

		ICrossReferenceSerializable CreateCrossReference();

		IBreakpointSerializable CreateBreakpoint();

		IBreakpointListSerializable CreateBreakpointList();

		ICompileOptionsSerializable CreateCompileOptions();

		ILibInfoSerializable CreateLibInfo();

		_ISlotPOUList2 CreateSlotPOUList();

		IDirectVariableCrossRefTableSerializable CreateDirVariableCrossRefTable();

		IAddressCrossReferenceSerializable CreateAddressCrossReference(int nCodeId);

		IAddressCodePositionSerializable CreateAddressCodePosition();

		_IImplicitReferenceVariable CreateImplitReferenceVariable(int nSignatureId, _IVariable variable);

		_ITaskList CreateTaskList();

		_ILibraryTable2 CreateLibraryTable();

		IMemoryManagerSerializable CreateMemMan();

		_IArea CreateArea();

		IBreakpointSerializable CreateTryCatchBreakpoint();

		ICompiledCodeSerializable CreateCompiledCodeDataStub();

		ICompiledCodeDataRelocSerializable CreateCompiledCodeDataReloc();

		ICompiledCodeDataSerializable CreateCompiledCodeData();
	}
}
