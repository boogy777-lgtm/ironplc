using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities.StructuredLanguageModel
{
	[TypeGuid("{DDDDC1B5-7642-4C86-B969-A04B901A64FC}")]
	public sealed class Method : AbstractPOUWithReturnValue, IMethodBuilder2, IMethodBuilder, IPouBuilder, IHasVarDeclaration, IHasAttributes
	{
		private Guid _gdOwningPou;

		internal Guid OwningPouGuid
		{
			set
			{
				_gdOwningPou = value;
			}
		}

		protected override Operator PouType => Operator.Method;

		protected override void AddToLanguageModel(IExprementPosition pos, ILanguageModel languageModel, ILMPOU pou, IPOUDeclarationStatement pouDeclaration)
		{
			base.AddToLanguageModel(pos, languageModel, pou, pouDeclaration);
			pou.ParentObjectGuid = _gdOwningPou;
		}

		public void Initialize(ILanguageModelBuilder lmbuilder, Guid gdParent, Guid gdObject, Guid gdMessage)
		{
			Initialize(lmbuilder, gdObject, gdMessage);
			_gdOwningPou = gdParent;
		}
	}
}
