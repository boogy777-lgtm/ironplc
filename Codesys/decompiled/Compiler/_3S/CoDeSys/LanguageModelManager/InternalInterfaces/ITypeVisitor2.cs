using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ITypeVisitor2 : ITypeVisitor
	{
		void visit(_ILDateType type);

		void visit(_ILTimeOfDayType type);

		void visit(_ILDateAndTimeType type);
	}
}
