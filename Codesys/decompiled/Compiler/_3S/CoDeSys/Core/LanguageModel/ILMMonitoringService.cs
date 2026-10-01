using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMMonitoringService
	{
		IVarRef GetVarReference(Guid guidApplication, string stInstancePath, string stExpression, int nProjectHandle, Guid guidObject);

		IVarRef GetVarReference(string stExpression, string stInstancePath);

		IVarRef GetVarReference(Guid guidApplication, string stExpression, bool bAllowShortExpressions);

		IEnumerable<IVarRef> GetAllVarReferences(Guid objectguid, Guid guidExplicitApplicationGuid, string stInstance, long[] alPositionsOfInterest);

		IEnumerable<IVarRef> GetAllVarReferences(Guid objectguid, string stInstance, long[] alPositionsOfInterest);

		IAddressInfo GetAddressInfo(Guid guidApplication, IExpression exp, IScope scope);

		IVarRef GetVarReference(Guid guidApplication, string stPOUName, string stExpression);

		IVarRef GetVarReference(Guid guidApplication, string stExpression);

		IVarRef GetVarReference(string stExpression);

		IEnumerable<IVarRef> GetAllVarReferences(string stInstance, long[] alPositionsOfInterest);

		IConverterFromIEC GetConverterFromIEC();

		IConverterToIEC GetConverterToIEC(bool bOmitPrefixesWherePossible, bool bUseShortPrefixes, DisplayMode displayMode);

		bool CanConvertRaw(byte[] raw, IType typeVarRef, Guid guidApplication, ByteOrder byteOrder);

		object ConvertRaw(byte[] raw, IType typeVarRef, Guid guidApplication, ByteOrder byteOrder);

		byte[] ConvertToRaw(object value, IType typeVarRef, Guid guidApplication, ByteOrder byteOrder);
	}
}
