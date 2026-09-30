using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IConverterToIEC
	{
		string GetBoolean(bool bValue);

		string GetDate(DateTime value);

		string GetDateAndTime(DateTime value);

		string GetDoubleByteString(string stValue);

		string GetDuration(long nValue);

		string GetInteger(object value, TypeClass typeClass);

		string GetReal(object value, TypeClass typeClass);

		string GetSingleByteString(string stValue);

		string GetTimeOfDay(DateTime value);

		string GetPointer(object value);

		string GetLiteralText(object value, TypeClass typeClass);
	}
}
