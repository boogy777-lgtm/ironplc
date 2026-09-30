using System;
using System.Diagnostics.CodeAnalysis;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[SuppressMessage("Minor Code Smell", "S3376:Attribute, EventArgs, and Exception type names should end with the type being extended", Justification = "The rule that this class should end with 'EventArgs' contradicts our naming convention that derived classes and interfaces must end with a number.")]
	[ReleasedClass]
	public class FilterMessageOutputEventArgs2 : FilterMessageOutputEventArgs
	{
		private readonly Guid _gdApplication;

		public Guid ApplicationGuid => _gdApplication;

		public FilterMessageOutputEventArgs2(IMessageCategory messageCategory, IMessage message, Guid gdApplication)
			: base(messageCategory, message)
		{
			_gdApplication = gdApplication;
		}
	}
}
