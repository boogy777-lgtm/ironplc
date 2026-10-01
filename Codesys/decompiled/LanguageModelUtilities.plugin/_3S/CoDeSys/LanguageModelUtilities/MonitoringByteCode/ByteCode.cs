using System;
using System.Linq;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities.MonitoringByteCode
{
	[TypeGuid("{deadbeef-d04a-47e0-925d-d9ff2a5ed501}")]
	public class ByteCode : IMonitoringByteCodeService
	{
		public byte[] CreateAnyByteCode(IMonitoringTargetProps tpr, Guid guidApplication, IStatement state, IScope scope, IExpressionTypifier6 typifier, out string stErrorMsg)
		{
			ByteCmdCreator byteCmdCreator = new ByteCmdCreator(tpr.ByteSupport ? 8 : 16, tpr.PointerSizeBytes, tpr.RuntimeIdentification, MonH.TargetByteOrderToByteOrder(tpr.TargetByteOrder), tpr.NewVFTable);
			ByteProgramCreator bpc = new ByteProgramCreator(byteCmdCreator);
			bool num = new InterpreterCodeGenerator(guidApplication, bpc, byteCmdCreator, scope, typifier, state).CreateCode(out stErrorMsg);
			byte[] result = null;
			if (!num)
			{
				result = byteCmdCreator.ByteCode.ToArray();
			}
			return result;
		}

		public byte[] CreateConditionByteCode(IMonitoringTargetProps tpr, Guid guidApplication, IStatement state, IScope scope, IExpressionTypifier6 typifier, out string stErrorMsg)
		{
			ByteCmdCreator byteCmdCreator = new ByteCmdCreator(tpr.ByteSupport ? 8 : 16, tpr.PointerSizeBytes, tpr.RuntimeIdentification, MonH.TargetByteOrderToByteOrder(tpr.TargetByteOrder), tpr.NewVFTable);
			ByteProgramCreator bpc = new ByteProgramCreator(byteCmdCreator);
			bool num = new InterpreterCodeGenerator(guidApplication, bpc, byteCmdCreator, scope, typifier, state).CreateReadCode(out stErrorMsg);
			byte[] result = null;
			if (!num)
			{
				result = byteCmdCreator.ByteCode.ToArray();
			}
			return result;
		}

		public bool CreateReadByteCode(IByteCmdCreator bc, IAddressInfo ai, out string stErrorMsg)
		{
			return new ByteProgramCreator((ByteCmdCreator)bc).CompileRead(ai, out stErrorMsg);
		}

		public bool CreateWriteByteCode(IByteCmdCreator bc, IAddressInfo ai, out string stErrorMsg)
		{
			return new ByteProgramCreator((ByteCmdCreator)bc).CompileWrite(ai, out stErrorMsg);
		}

		public bool CreateForceByteCode(IByteCmdCreator bc, IAddressInfo ai, out string stErrorMsg)
		{
			return new ByteProgramCreator((ByteCmdCreator)bc).CompileForce(ai, out stErrorMsg);
		}

		public IByteCmdCreator CreateByteCmdCreator(int iCharBit, int iPointerSizeBytes, Version vRuntimeIdentification, ByteOrder bo, bool bNewVFTable)
		{
			return new ByteCmdCreator(iCharBit, iPointerSizeBytes, vRuntimeIdentification, bo, bNewVFTable);
		}
	}
}
