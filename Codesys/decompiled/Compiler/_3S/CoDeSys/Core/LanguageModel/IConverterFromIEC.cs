using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IConverterFromIEC
	{
		bool GetBoolean(string stIEC);

		DateTime GetDate(string stIEC);

		DateTime GetDateAndTime(string stIEC);

		string GetDoubleByteString(string stIEC);

		long GetDuration(string stIEC);

		void GetInteger(string stIEC, out object value, out TypeClass typeClass);

		void GetReal(string stIEC, out object value, out TypeClass typeClass);

		string GetSingleByteString(string stIEC);

		DateTime GetTimeOfDay(string stIEC);

		void GetLiteralValue(string stIEC, out object value, out TypeClass typeClass);
	}
}
