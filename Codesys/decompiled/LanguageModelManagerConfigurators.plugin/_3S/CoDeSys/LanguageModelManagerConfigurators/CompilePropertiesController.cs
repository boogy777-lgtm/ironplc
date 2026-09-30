using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	public class CompilePropertiesController : ICompilePropertiesViewListener
	{
		private readonly ICompilePropertiesModelListener _modelListener;

		private readonly IAPEnvironmentFacade _apEnvironmentFacade;

		private CompilePropertiesModel _model;

		internal CompilePropertiesModel Model => _model;

		public CompilePropertiesController(ICompilePropertiesModelListener modelListener, IAPEnvironmentFacade apEnvironmentFacade)
		{
			_modelListener = modelListener;
			_apEnvironmentFacade = apEnvironmentFacade;
		}

		public void Initialize(bool runsInDebugmode)
		{
			InitializeModel(runsInDebugmode);
			_modelListener.InitializeView();
			if (_model.IsLibraryWithPinnedStorageVersion)
			{
				InitCompilerVersionSelectableForLibraryWithPinnedStorageVersion();
			}
			else
			{
				InitCompilerVersionSelectable();
			}
			InitProjectDefinesInput();
			InitMaxWarningsSelectable();
			UpdateOptionsGuiFromModel();
		}

		private void InitializeModel(bool runsInDebugmode)
		{
			_model = new CompilePropertiesModel();
			_model.debugMode = runsInDebugmode;
			_model.IsLibraryWithPinnedStorageVersion = _apEnvironmentFacade.IsLibraryWithPinnedStorageVersion();
			Version version = _apEnvironmentFacade.CompilerVersionToUse();
			_model.DesiredVersion = new Compilerversion(version, _apEnvironmentFacade.MapFromInternalToOEMTextSave(version));
			List<Compilerversion> list = _apEnvironmentFacade.AvailableCompilerVersionsOEMFilteredNotReplaced.Select((Version v) => new Compilerversion(v, _apEnvironmentFacade.MapFromInternalToOEMText(v))).ToList();
			if (_model.debugMode || _model.IsLibraryWithPinnedStorageVersion)
			{
				_model.DesiredVersionIsAvailable = list.Any((Compilerversion cv) => _model.DesiredVersion.VersionText == cv.VersionText);
				if (!_model.DesiredVersionIsAvailable)
				{
					list.Add(_model.DesiredVersion);
				}
			}
			_model.SelectableCompilerVersions = list.OrderByDescending((Compilerversion cv) => cv.Version);
			_model.NewestVersion = _model.SelectableCompilerVersions.First();
			if (!_model.debugMode && !_model.IsLibraryWithPinnedStorageVersion)
			{
				List<Compilerversion> list2 = new List<Compilerversion> { _model.NewestVersion };
				if (_model.DesiredVersion.Version != _model.NewestVersion.Version)
				{
					list2.Add(_model.DesiredVersion);
				}
				_model.SelectableCompilerVersions = list2.OrderByDescending((Compilerversion cv) => cv.Version);
				_model.DesiredVersionIsAvailable = list.Any((Compilerversion cv) => cv.Version == _model.DesiredVersion.Version);
			}
			if (_apEnvironmentFacade.CompileOptions is ILMCompileOptions3 iLMCompileOptions && !string.IsNullOrWhiteSpace(iLMCompileOptions.ProjectDefines))
			{
				_model.ProjectDefines = iLMCompileOptions.ProjectDefines.Split(',').ToList();
			}
			else
			{
				_model.ProjectDefines = new List<string>();
			}
			_model.AllowUnicodeInIdentifiers = new VersionDependendOption
			{
				Enabled = true,
				Selected = _apEnvironmentFacade.CompileOptions.UnicodeIdentifiers
			};
			_model.ReplaceConstants = new VersionDependendOption
			{
				Enabled = true,
				Selected = _apEnvironmentFacade.CompileOptions.ReplaceConstants
			};
			_model.EnableLoggingInBreakpoints = new VersionDependendOption
			{
				Enabled = true,
				Selected = _apEnvironmentFacade.CompileOptions.EnableBreakpointLogging
			};
			if (_apEnvironmentFacade.CompileOptions is ILMCompileOptions2 iLMCompileOptions2)
			{
				_model.Utf8EncodedStrings = new VersionDependendOption
				{
					Enabled = true,
					Selected = iLMCompileOptions2.UTF8Encoding
				};
			}
			else
			{
				_model.Utf8EncodedStrings = new VersionDependendOption
				{
					Enabled = false,
					Selected = false
				};
			}
			if (_apEnvironmentFacade.CompileOptions is ILMCompileOptions3 iLMCompileOptions3)
			{
				_model.ReportCompiledPousDuringIncrementalCompile = new VersionDependendOption
				{
					Enabled = true,
					Selected = iLMCompileOptions3.ReportCompiledPousDuringIncrementalCompile
				};
			}
			else
			{
				_model.ReportCompiledPousDuringIncrementalCompile = new VersionDependendOption
				{
					Enabled = false,
					Selected = true
				};
			}
			ChangeOptionsInModelVersiondependend();
			_model.CompilerVersionFixed = _apEnvironmentFacade.GetCustomizationVersion() != null;
			_model.MaxNumberOfWarnings = _apEnvironmentFacade.CompileOptions.MaxCompilerWarnings;
		}

		private void InitCompilerVersionSelectable()
		{
			if (!_model.debugMode)
			{
				_modelListener.DisableCustomTextInput(ECompilePropertiesEditorPosition.CompilerVersionSelection);
				if (_model.CompilerVersionFixed)
				{
					_modelListener.SetEnabled(ECompilePropertiesEditorPosition.CompilerVersionSelection, enabled: false);
				}
			}
			_modelListener.ShowCompilerversions(_model.SelectableCompilerVersions.Select((Compilerversion x) => x.VersionText));
			if (_model.DesiredVersionIsAvailable)
			{
				_modelListener.ShowCompilerversion(_model.DesiredVersion.VersionText);
				_modelListener.HideMessage();
			}
			else
			{
				_modelListener.ShowCompilerversion(_model.DesiredVersion.VersionText);
				_modelListener.SetMessage(ECompilePropertiesEditorPosition.CompilerVersionWarningDisplay, Strings.SelectedVersionNotAvailable, "Problem");
			}
			if (_model.DesiredVersion.Version == _model.NewestVersion.Version && !_model.debugMode)
			{
				_modelListener.SetEnabled(ECompilePropertiesEditorPosition.CompilerVersionSelection, enabled: false);
				_modelListener.SetMessage(ECompilePropertiesEditorPosition.CompilerVersionWarningDisplay, Strings.ComProp_CompilerVersionAlreadyNewest, "Info");
			}
		}

		private static bool IsPatchVersion(Version v)
		{
			return v.Revision != 0;
		}

		private void InitCompilerVersionSelectableForLibraryWithPinnedStorageVersion()
		{
			Version minimumSupportedVersion = new Version(3, 5, 16, 0);
			IEnumerable<string> compilerversions = from x in _model.SelectableCompilerVersions
				where minimumSupportedVersion <= x.Version
				where !IsPatchVersion(x.Version)
				select x.VersionText;
			_modelListener.ShowCompilerversions(compilerversions);
			if (_model.DesiredVersionIsAvailable)
			{
				_modelListener.ShowCompilerversion(_model.DesiredVersion.VersionText);
				if (IsPatchVersion(_model.DesiredVersion.Version))
				{
					_modelListener.SetMessage(ECompilePropertiesEditorPosition.CompilerVersionWarningDisplay, Strings.InvalidPatchVersion, "Problem");
				}
				else
				{
					_modelListener.HideMessage();
				}
			}
			else
			{
				_modelListener.ShowCompilerversion(_model.DesiredVersion.VersionText);
				_modelListener.SetMessage(ECompilePropertiesEditorPosition.CompilerVersionWarningDisplay, Strings.SelectedVersionNotAvailable, "Problem");
			}
		}

		private void InitProjectDefinesInput()
		{
			_model.ProjectDefinesEnabled = _model.DesiredVersion.Version >= new Version(3, 5, 20, 0);
			_modelListener.SetEnabled(ECompilePropertiesEditorPosition.ProjectDefinesTrigger, _model.ProjectDefinesEnabled);
		}

		private void InitMaxWarningsSelectable()
		{
			_modelListener.ShowMaxNumbersOfWarnings(new string[1] { "<" + Strings.NoLimit + ">" });
			if (_model.MaxNumberOfWarnings == int.MaxValue)
			{
				_modelListener.ShowMaxNumberOfWarnings("<" + Strings.NoLimit + ">");
			}
			else
			{
				_modelListener.ShowMaxNumberOfWarnings(_model.MaxNumberOfWarnings.ToString());
			}
		}

		private void ChangeOptionsInModelVersiondependend()
		{
			if (!(_model.DesiredVersion.Version == null))
			{
				if (_model.DesiredVersion.Version < new Version(3, 3, 2, 0))
				{
					_model.ReplaceConstants.Selected = true;
					_model.ReplaceConstants.Enabled = false;
				}
				else
				{
					_model.ReplaceConstants.Enabled = true;
				}
				if (_model.DesiredVersion.Version < new Version(3, 5, 5, 0))
				{
					_model.EnableLoggingInBreakpoints.Selected = false;
					_model.EnableLoggingInBreakpoints.Enabled = false;
				}
				else
				{
					_model.EnableLoggingInBreakpoints.Enabled = true;
				}
				if (_model.DesiredVersion.Version < new Version(3, 5, 18, 0))
				{
					_model.Utf8EncodedStrings.Selected = false;
					_model.Utf8EncodedStrings.Enabled = false;
				}
				else
				{
					_model.Utf8EncodedStrings.Enabled = true;
				}
				if (_model.DesiredVersion.Version >= new Version(3, 5, 20, 0))
				{
					_model.ProjectDefinesEnabled = true;
					_model.ReportCompiledPousDuringIncrementalCompile.Enabled = true;
				}
				else
				{
					_model.ProjectDefinesEnabled = false;
					_model.ReportCompiledPousDuringIncrementalCompile.Enabled = false;
				}
			}
		}

		private void UpdateOptionsGuiFromModel()
		{
			_modelListener.SetOptionState(ECompilePropertiesEditorPosition.AllowUnicodeInIdentifiers, _model.AllowUnicodeInIdentifiers.Selected);
			_modelListener.SetEnabled(ECompilePropertiesEditorPosition.AllowUnicodeInIdentifiers, _model.AllowUnicodeInIdentifiers.Enabled);
			_modelListener.SetOptionState(ECompilePropertiesEditorPosition.ReplaceConstants, _model.ReplaceConstants.Selected);
			_modelListener.SetEnabled(ECompilePropertiesEditorPosition.ReplaceConstants, _model.ReplaceConstants.Enabled);
			_modelListener.SetOptionState(ECompilePropertiesEditorPosition.EnableLoggingInBreakpoints, _model.EnableLoggingInBreakpoints.Selected);
			_modelListener.SetEnabled(ECompilePropertiesEditorPosition.EnableLoggingInBreakpoints, _model.EnableLoggingInBreakpoints.Enabled);
			_modelListener.SetOptionState(ECompilePropertiesEditorPosition.Utf8EncodedStrings, _model.Utf8EncodedStrings.Selected);
			_modelListener.SetEnabled(ECompilePropertiesEditorPosition.Utf8EncodedStrings, _model.Utf8EncodedStrings.Enabled);
			_modelListener.SetOptionState(ECompilePropertiesEditorPosition.ReportCompiledPousDuringIncrementalCompile, _model.ReportCompiledPousDuringIncrementalCompile.Selected);
			_modelListener.SetEnabled(ECompilePropertiesEditorPosition.ReportCompiledPousDuringIncrementalCompile, _model.ReportCompiledPousDuringIncrementalCompile.Enabled);
			_modelListener.SetEnabled(ECompilePropertiesEditorPosition.ProjectDefinesTrigger, _model.ProjectDefinesEnabled);
		}

		public void CompilerVersionChanged(string stNewCompilerVersion)
		{
			Version version = _apEnvironmentFacade.MapFromOEMTextToInternal(stNewCompilerVersion);
			_model.DesiredVersion = new Compilerversion(version, stNewCompilerVersion);
			_model.DesiredVersionIsAvailable = _model.SelectableCompilerVersions.Any((Compilerversion cv) => cv.Version == _model.DesiredVersion.Version);
			if (stNewCompilerVersion == null || _model.DesiredVersionIsAvailable)
			{
				_modelListener.HideMessage();
			}
			else
			{
				_modelListener.SetMessage(ECompilePropertiesEditorPosition.CompilerVersionWarningDisplay, string.Format(Strings.ComProp_CompilerVersionUnavailable, stNewCompilerVersion), "Problem");
			}
			if (version == null)
			{
				return;
			}
			if (!_model.debugMode && !_model.IsLibraryWithPinnedStorageVersion)
			{
				if (version == _model.NewestVersion.Version)
				{
					_modelListener.SetMessage(ECompilePropertiesEditorPosition.CompilerVersionWarningDisplay, Strings.ComProp_NoDowngradePossibleWarning, "Warning");
				}
				else
				{
					_modelListener.HideMessage();
				}
			}
			ChangeOptionsInModelVersiondependend();
			UpdateOptionsGuiFromModel();
		}

		public void OptionChanged(ECompilePropertiesEditorPosition position, bool selected)
		{
			switch (position)
			{
			case ECompilePropertiesEditorPosition.AllowUnicodeInIdentifiers:
				_model.AllowUnicodeInIdentifiers.Selected = selected;
				break;
			case ECompilePropertiesEditorPosition.ReplaceConstants:
				_model.ReplaceConstants.Selected = selected;
				break;
			case ECompilePropertiesEditorPosition.EnableLoggingInBreakpoints:
				_model.EnableLoggingInBreakpoints.Selected = selected;
				break;
			case ECompilePropertiesEditorPosition.Utf8EncodedStrings:
				_model.Utf8EncodedStrings.Selected = selected;
				break;
			case ECompilePropertiesEditorPosition.ReportCompiledPousDuringIncrementalCompile:
				_model.ReportCompiledPousDuringIncrementalCompile.Selected = selected;
				break;
			}
		}

		public void Triggered(ECompilePropertiesEditorPosition position)
		{
			if (position == ECompilePropertiesEditorPosition.ProjectDefinesTrigger)
			{
				_model.ProjectDefines = _modelListener.StartDialog(ECompilePropertiesEditorPosition.ProjectDefinesDialog, _model.ProjectDefines);
			}
		}

		public ValidationResult ValidateAndSaveModelFromView()
		{
			RefreshModelFromView();
			ValidationResult validationResult = ValidateVersionSelection();
			if (validationResult.IsValid)
			{
				SaveVersionSelection();
				bool flag = ProjectDefinesChanged();
				SaveProjectDefines();
				bool num = ValidateOptions();
				SaveOptions();
				if (num || flag)
				{
					_apEnvironmentFacade.ClearLanguageModel();
				}
				ValidationResult validationResult2 = ValidateMaxNumberWarnings();
				if (validationResult2.IsValid)
				{
					SaveMaxNumberWarning();
					return new ValidationResult
					{
						IsValid = true
					};
				}
				return validationResult2;
			}
			return validationResult;
		}

		private void RefreshModelFromView()
		{
			_model.DesiredVersion = ParseCompilerversionfieldSelection();
			_model.AllowUnicodeInIdentifiers.Selected = _modelListener.GetOption(ECompilePropertiesEditorPosition.AllowUnicodeInIdentifiers);
			_model.ReplaceConstants.Selected = _modelListener.GetOption(ECompilePropertiesEditorPosition.ReplaceConstants);
			_model.EnableLoggingInBreakpoints.Selected = _modelListener.GetOption(ECompilePropertiesEditorPosition.EnableLoggingInBreakpoints);
			_model.Utf8EncodedStrings.Selected = _modelListener.GetOption(ECompilePropertiesEditorPosition.Utf8EncodedStrings);
			_model.ReportCompiledPousDuringIncrementalCompile.Selected = _modelListener.GetOption(ECompilePropertiesEditorPosition.ReportCompiledPousDuringIncrementalCompile);
			string text = _modelListener.GetText(ECompilePropertiesEditorPosition.MaxNumberOfWarnings);
			uint result;
			if (text == "<" + Strings.NoLimit + ">")
			{
				_model.MaxNumberOfWarnings = int.MaxValue;
			}
			else if (uint.TryParse(text, out result))
			{
				_model.MaxNumberOfWarnings = (int)result;
			}
			else
			{
				_model.MaxNumberOfWarnings = null;
			}
		}

		private Compilerversion ParseCompilerversionfieldSelection()
		{
			object selected = _modelListener.GetSelected(ECompilePropertiesEditorPosition.CompilerVersionSelection);
			if (selected == null)
			{
				string text = _modelListener.GetText(ECompilePropertiesEditorPosition.CompilerVersionSelection);
				if (string.IsNullOrEmpty(text))
				{
					return null;
				}
				if (Version.TryParse(text, out var result) && result != null)
				{
					_modelListener.ShowCompilerversion(text);
				}
			}
			selected = _modelListener.GetSelected(ECompilePropertiesEditorPosition.CompilerVersionSelection);
			if (string.IsNullOrEmpty((string)selected))
			{
				return null;
			}
			Version version = _apEnvironmentFacade.MapFromOEMTextToInternalSave((string)selected);
			if (version == null)
			{
				return null;
			}
			return new Compilerversion(version, (string)selected);
		}

		private ValidationResult ValidateVersionSelection()
		{
			if (_model.DesiredVersion != null)
			{
				return new ValidationResult
				{
					IsValid = true
				};
			}
			string optionalErrorMessage = string.Empty;
			string text = _modelListener.GetText(ECompilePropertiesEditorPosition.CompilerVersionSelection);
			object selected = _modelListener.GetSelected(ECompilePropertiesEditorPosition.CompilerVersionSelection);
			Version result;
			if (string.IsNullOrEmpty(text))
			{
				optionalErrorMessage = Strings.ComProp_CompilerVersionEmpty;
			}
			else if (!Version.TryParse(text, out result) || result == null)
			{
				optionalErrorMessage = string.Format(Strings.ComProp_CompilerVersionInvalid, text);
			}
			else if (selected == null)
			{
				optionalErrorMessage = string.Format(Strings.ComProp_CompilerVersionUnavailable, result.ToString());
			}
			return new ValidationResult
			{
				IsValid = false,
				OptionalErrorMessage = optionalErrorMessage,
				FailedControl = ECompilePropertiesEditorPosition.CompilerVersionSelection
			};
		}

		private void SaveVersionSelection()
		{
			_apEnvironmentFacade.SetCompilerVersionExact(_model.DesiredVersion.Version);
		}

		private void SaveProjectDefines()
		{
			if (_apEnvironmentFacade.CompileOptions is ILMCompileOptions3 iLMCompileOptions)
			{
				iLMCompileOptions.ProjectDefines = string.Join(",", _model.ProjectDefines);
			}
		}

		private bool ValidateOptions()
		{
			ChangeOptionsInModelVersiondependend();
			UpdateOptionsGuiFromModel();
			ILMCompileOptions compileOptions = _apEnvironmentFacade.CompileOptions;
			if (compileOptions.UnicodeIdentifiers != _model.AllowUnicodeInIdentifiers.Selected || compileOptions.ReplaceConstants != _model.ReplaceConstants.Selected || compileOptions.EnableBreakpointLogging != _model.EnableLoggingInBreakpoints.Selected || (compileOptions is ILMCompileOptions2 iLMCompileOptions && iLMCompileOptions.UTF8Encoding != _model.Utf8EncodedStrings.Selected) || (compileOptions is ILMCompileOptions3 iLMCompileOptions2 && iLMCompileOptions2.ReportCompiledPousDuringIncrementalCompile != _model.ReportCompiledPousDuringIncrementalCompile.Selected))
			{
				return true;
			}
			return false;
		}

		private bool ProjectDefinesChanged()
		{
			if (_apEnvironmentFacade.CompileOptions is ILMCompileOptions3 iLMCompileOptions)
			{
				string text = string.Join(",", _model.ProjectDefines);
				return iLMCompileOptions.ProjectDefines != text;
			}
			return false;
		}

		private void SaveOptions()
		{
			ILMCompileOptions compileOptions = _apEnvironmentFacade.CompileOptions;
			compileOptions.UnicodeIdentifiers = _model.AllowUnicodeInIdentifiers.Selected;
			compileOptions.ReplaceConstants = _model.ReplaceConstants.Selected;
			compileOptions.EnableBreakpointLogging = _model.EnableLoggingInBreakpoints.Selected;
			if (compileOptions is ILMCompileOptions2 iLMCompileOptions)
			{
				iLMCompileOptions.UTF8Encoding = _model.Utf8EncodedStrings.Selected;
			}
			if (compileOptions is ILMCompileOptions3 iLMCompileOptions2)
			{
				iLMCompileOptions2.ReportCompiledPousDuringIncrementalCompile = _model.ReportCompiledPousDuringIncrementalCompile.Selected;
			}
		}

		private ValidationResult ValidateMaxNumberWarnings()
		{
			if (!_model.MaxNumberOfWarnings.HasValue)
			{
				return new ValidationResult
				{
					IsValid = false,
					OptionalErrorMessage = Strings.InvalidValue,
					FailedControl = ECompilePropertiesEditorPosition.MaxNumberOfWarnings
				};
			}
			return new ValidationResult
			{
				IsValid = true
			};
		}

		private void SaveMaxNumberWarning()
		{
			_apEnvironmentFacade.CompileOptions.MaxCompilerWarnings = _model.MaxNumberOfWarnings.Value;
		}
	}
}
