using System;
using System.Diagnostics.CodeAnalysis;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[SuppressMessage("Minor Code Smell", "S3376:Attribute, EventArgs, and Exception type names should end with the type being extended", Justification = "The rule that this class should end with 'EventArgs' contradicts our naming convention that derived classes and interfaces must end with a number.")]
	[ReleasedClass]
	public class MessageOutputEventArgs2 : MessageOutputEventArgs
	{
		private readonly Guid _gdApplication;

		public Guid ApplicationGuid => _gdApplication;

		public MessageOutputEventArgs2(Guid gdApplication)
		{
			_gdApplication = gdApplication;
		}
	}
}
