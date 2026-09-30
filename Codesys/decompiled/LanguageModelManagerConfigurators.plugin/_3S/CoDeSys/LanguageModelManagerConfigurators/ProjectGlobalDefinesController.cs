using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.Controls.Controls;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	public class ProjectGlobalDefinesController : IProjectGlobalDefinesViewListener
	{
		private readonly IProjectGlobalDefinesModelListener _modelListener;

		private IList<string> projectDefines;

		private ProjectGlobalDefinesTableModel _tableModel;

		private IAPEnvironmentFacade _apEnvironmentFacade;

		private int _iSelectionIndex;

		public ProjectGlobalDefinesController(IProjectGlobalDefinesModelListener modelListener)
		{
			_modelListener = modelListener;
		}

		public void Initialize(IAPEnvironmentFacade apEnvironmentFacade, IList<string> projectDefines)
		{
			_apEnvironmentFacade = apEnvironmentFacade;
			_iSelectionIndex = -1;
			_tableModel = new ProjectGlobalDefinesTableModel();
			_modelListener.SetTableModel(_tableModel);
			this.projectDefines = new List<string>(projectDefines);
			foreach (string projectDefine in this.projectDefines)
			{
				ProjectGlobalDefinesTreeTableNode node = new ProjectGlobalDefinesTreeTableNode(projectDefine, _apEnvironmentFacade, this, _tableModel);
				_tableModel.AddNode((ITreeTableNode)(object)node);
			}
			checkValidityOfListAndDisplayWarning();
		}

		public void TableContentChanged(string oldEntry, string newEntry)
		{
			int index = projectDefines.IndexOf(oldEntry);
			projectDefines[index] = newEntry;
			checkValidityOfListAndDisplayWarning();
		}

		public void SelectionChanged(int iIndex)
		{
			_iSelectionIndex = iIndex;
		}

		private void checkValidityOfListAndDisplayWarning()
		{
			_tableModel.UpdateAllNodes();
			HashSet<string> hashSet = new HashSet<string>();
			bool flag = !projectDefines.Any((string entry) => !ValidateIdentifier(entry));
			if (flag)
			{
				foreach (string item in projectDefines.Select((string entry) => entry))
				{
					if (hashSet.Contains(item))
					{
						flag = false;
						break;
					}
					hashSet.Add(item);
				}
			}
			_modelListener.SetEnabled(EProjectGlobalDefinesDialogPosition.EndDialogOK, flag);
		}

		public bool ValidateIdentifier(string defineName)
		{
			IScanner scanner = APEnvironment.LMServiceProvider.CreatorService.CreateScanner(defineName, bIncludeComments: true, bIncludeEndOfLines: true, bIncludePragmas: true, bIncludeWhitespaces: true);
			IToken token;
			TokenType next = scanner.GetNext(out token);
			TokenType next2 = scanner.GetNext(out token);
			if (next == TokenType.Identifier)
			{
				return next2 == TokenType.End;
			}
			return false;
		}

		public void AddDefine()
		{
			string newProjectDefine = Strings.NewProjectDefine;
			projectDefines.Add(newProjectDefine);
			ProjectGlobalDefinesTreeTableNode node = new ProjectGlobalDefinesTreeTableNode(newProjectDefine, _apEnvironmentFacade, this, _tableModel);
			_tableModel.AddNode((ITreeTableNode)(object)node);
			checkValidityOfListAndDisplayWarning();
		}

		public void RemoveDefine()
		{
			if (-1 != _iSelectionIndex)
			{
				projectDefines.RemoveAt(_iSelectionIndex);
				_tableModel.RemoveNode(_iSelectionIndex);
			}
			checkValidityOfListAndDisplayWarning();
		}

		public void ApplyChanges()
		{
			_modelListener.DialogAnswer = new List<string>(projectDefines);
		}
	}
}
