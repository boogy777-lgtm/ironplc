using System;
using System.Drawing;
using _3S.CoDeSys.Controls.Controls;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	public class ProjectGlobalDefinesTreeTableNode : ITreeTableNode2, ITreeTableNode
	{
		internal const int COLINDEX_NAME = 0;

		private readonly IAPEnvironmentFacade _apEnvironmentFacade;

		private readonly IProjectGlobalDefinesViewListener _viewListener;

		private readonly ProjectGlobalDefinesTableModel _tableModel;

		public bool HasChildren => false;

		public int ChildCount => 0;

		public ITreeTableNode Parent => null;

		public string DefineName { get; private set; }

		public bool NameIsValid { get; private set; }

		internal bool NameIsUnique { get; set; }

		public ITreeTableNode GetChild(int nIndex)
		{
			return null;
		}

		public ProjectGlobalDefinesTreeTableNode(string defineName, IAPEnvironmentFacade apEnvironmentFacade, IProjectGlobalDefinesViewListener viewListener, ProjectGlobalDefinesTableModel tableModel)
		{
			DefineName = defineName;
			_viewListener = viewListener;
			_tableModel = tableModel;
			NameIsValid = _viewListener.ValidateIdentifier(defineName);
			_apEnvironmentFacade = apEnvironmentFacade;
		}

		public int GetIndex(ITreeTableNode node)
		{
			return -1;
		}

		public object GetValue(int nColumnIndex)
		{
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Expected O, but got Unknown
			if (nColumnIndex == 0)
			{
				NameIsUnique = _tableModel.IsDefineUnique(DefineName, out var _);
				_tableModel.SetDefineUnique(DefineName, NameIsUnique);
				return (object)new IconLabelTreeTableViewCellData((Image)GetImageByValidity(NameIsValid && NameIsUnique), (object)DefineName);
			}
			throw new ArgumentOutOfRangeException("nColumnIndex");
		}

		public bool IsEditable(int nColumnIndex)
		{
			return true;
		}

		public void SetValue(int nColumnIndex, object value)
		{
			string defineName = DefineName;
			if (nColumnIndex == 0)
			{
				IconLabelTreeTableViewCellData val = (IconLabelTreeTableViewCellData)((value is IconLabelTreeTableViewCellData) ? value : null);
				if (val != null)
				{
					DefineName = (string)val.get_Label();
					NameIsValid = _viewListener.ValidateIdentifier(DefineName);
				}
			}
			_tableModel.UpdateAllNodes();
			_viewListener.TableContentChanged(defineName, DefineName);
		}

		public void SwapChildren(int nIndex1, int nIndex2)
		{
		}

		public string GetToolTipText(int nColumnIndex)
		{
			if (nColumnIndex == 0)
			{
				if (!NameIsValid)
				{
					return Strings.DefineNoIECIdent_Error;
				}
				if (!NameIsUnique)
				{
					return Strings.DefineNotUnique_Error;
				}
			}
			return string.Empty;
		}

		private Bitmap GetImageByValidity(bool validity)
		{
			Bitmap result = null;
			if (!validity)
			{
				result = _apEnvironmentFacade.GetIcon(GetType(), "_3S.CoDeSys.LanguageModelManagerConfigurators.Resources.Problem.ico")?.ToBitmap();
			}
			return result;
		}
	}
}
