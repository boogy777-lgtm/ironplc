using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IMonitoringByteCodeService
	{
		IByteCmdCreator CreateByteCmdCreator(int iCharBit, int iPointerSizeBytes, Version vRuntimeIdentification, ByteOrder bo, bool bNewVFTable);

		byte[] CreateAnyByteCode(IMonitoringTargetProps tpr, Guid guidApplication, IStatement state, IScope scope, IExpressionTypifier6 typifier, out string stErrorMsg);

		byte[] CreateConditionByteCode(IMonitoringTargetProps tpr, Guid guidApplication, IStatement state, IScope scope, IExpressionTypifier6 typifier, out string stErrorMsg);

		bool CreateReadByteCode(IByteCmdCreator bc, IAddressInfo ai, out string stErrorMsg);

		bool CreateWriteByteCode(IByteCmdCreator bc, IAddressInfo ai, out string stErrorMsg);

		bool CreateForceByteCode(IByteCmdCreator bc, IAddressInfo ai, out string stErrorMsg);
	}
}
