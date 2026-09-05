#if DEBUG
using System;
using Godot;
using FrontsOfWar.Editor.Documents;
using FrontsOfWar.Map.Authoring;
using FrontsOfWar.UI.Theme;

namespace FrontsOfWar.Editor.Application;

// UI adapter for MapDocument. Dialogs collect intent; MapDocument remains the
// authority that refuses unsafe replacement or close operations.
public partial class MapEditorFileWorkflow : Node
{
    private enum FileCommand { New = 1, Open, Save, SaveAs, Close }

    private readonly MapDocument _document = new();
    private MenuButton _fileMenu;
    private Label _documentLabel;
    private Label _statusLabel;
    private FileDialog _openDialog;
    private FileDialog _saveDialog;
    private ConfirmationDialog _unsavedDialog;
    private AcceptDialog _errorDialog;
    private Action _pendingAction;
    private bool _continueAfterSave;

    public MapDocument Document => _document;

    public void SaveCommand() => SaveCurrent();

    public void RequestApplicationClose()
        => RequestReplacement(() => GetTree().Quit());

    public void Configure(MenuButton fileMenu, Label documentLabel, Label statusLabel)
    {
        _fileMenu = fileMenu;
        _documentLabel = documentLabel;
        _statusLabel = statusLabel;
        BuildMenu();
        BuildDialogs();
        _document.StateChanged += RefreshState;
        RefreshState();
    }

    public override void _ExitTree()
    {
        _document.StateChanged -= RefreshState;
    }

    private void BuildMenu()
    {
        var popup = _fileMenu.GetPopup();
        popup.AddItem("New map", (int)FileCommand.New);
        popup.AddItem("Open…", (int)FileCommand.Open);
        popup.AddSeparator();
        popup.AddItem("Save", (int)FileCommand.Save);
        popup.AddItem("Save As…", (int)FileCommand.SaveAs);
        popup.AddSeparator();
        popup.AddItem("Close map", (int)FileCommand.Close);
        popup.IdPressed += OnFileCommand;
    }

    private void BuildDialogs()
    {
        _openDialog = CreateFileDialog(FileDialog.FileModeEnum.OpenFile, "Open production map");
        _openDialog.FileSelected += path => RequestReplacement(() => OpenSelected(path));
        AddChild(_openDialog);

        _saveDialog = CreateFileDialog(FileDialog.FileModeEnum.SaveFile, "Save production map");
        _saveDialog.FileSelected += SaveSelected;
        _saveDialog.Canceled += CancelPendingSave;
        AddChild(_saveDialog);

        _unsavedDialog = new ConfirmationDialog
        {
            Title = "Unsaved map changes",
            DialogText = "Save changes before continuing?",
        };
        _unsavedDialog.GetOkButton().Text = "Save";
        _unsavedDialog.AddButton("Discard", right: true, action: "discard");
        _unsavedDialog.Confirmed += SaveBeforePendingAction;
        _unsavedDialog.Canceled += ClearPendingAction;
        _unsavedDialog.CustomAction += action =>
        {
            if (action.ToString() != "discard") return;
            _unsavedDialog.Hide();
            RunPendingAction();
        };
        AddChild(_unsavedDialog);

        _errorDialog = new AcceptDialog { Title = "Map editor" };
        AddChild(_errorDialog);
    }

    private static FileDialog CreateFileDialog(FileDialog.FileModeEnum mode, string title)
    {
        var dialog = new FileDialog
        {
            Title = title,
            Access = FileDialog.AccessEnum.Resources,
            FileMode = mode,
            CurrentDir = "res://assets/data/maps",
            MinSize = new Vector2I(900, 620),
        };
        dialog.AddFilter("*.tres", "Godot map resources");
        return dialog;
    }

    private void OnFileCommand(long id)
    {
        switch ((FileCommand)id)
        {
            case FileCommand.New:
                RequestReplacement(NewMap);
                break;
            case FileCommand.Open:
                _openDialog.PopupCenteredRatio(0.78f);
                break;
            case FileCommand.Save:
                SaveCurrent();
                break;
            case FileCommand.SaveAs:
                ShowSaveAs();
                break;
            case FileCommand.Close:
                RequestReplacement(CloseMap);
                break;
        }
    }

    private void NewMap()
    {
        if (_document.TryNew(
            MapDefinition.CreateNew("untitled_map", "Untitled Map"),
            () => UnsavedChangesChoice.Discard))
            SetStatus("NEW MAP — SAVE AS TO CHOOSE A REPOSITORY PATH");
    }

    private void OpenSelected(string path)
    {
        try
        {
            if (_document.TryOpen(path, () => UnsavedChangesChoice.Discard))
                SetStatus($"OPENED {path.GetFile()}");
        }
        catch (Exception exception)
        {
            ShowError(exception.Message);
        }
    }

    private void CloseMap()
    {
        if (_document.TryClose(() => UnsavedChangesChoice.Discard))
            SetStatus("READY — NO MAP OPEN");
    }

    private void SaveCurrent()
    {
        if (!_document.IsOpen) return;
        if (string.IsNullOrEmpty(_document.FilePath))
        {
            ShowSaveAs();
            return;
        }

        try
        {
            _document.Save();
            SetStatus($"SAVED {_document.FilePath.GetFile()}");
        }
        catch (Exception exception)
        {
            ShowError(exception.Message);
        }
    }

    private void ShowSaveAs()
    {
        if (!_document.IsOpen) return;
        _saveDialog.CurrentFile = string.IsNullOrEmpty(_document.FilePath)
            ? $"{_document.Current.Metadata.Id}.tres"
            : _document.FilePath.GetFile();
        _saveDialog.PopupCenteredRatio(0.78f);
    }

    private void SaveSelected(string path)
    {
        try
        {
            _document.SaveAs(path);
            SetStatus($"SAVED {path.GetFile()}");
            if (_continueAfterSave) RunPendingAction();
        }
        catch (Exception exception)
        {
            _continueAfterSave = false;
            ShowError(exception.Message);
        }
    }

    private void RequestReplacement(Action action)
    {
        if (!_document.IsDirty)
        {
            action();
            return;
        }
        _pendingAction = action;
        _unsavedDialog.PopupCentered();
    }

    private void SaveBeforePendingAction()
    {
        if (string.IsNullOrEmpty(_document.FilePath))
        {
            _continueAfterSave = true;
            ShowSaveAs();
            return;
        }

        SaveCurrent();
        if (!_document.IsDirty) RunPendingAction();
    }

    private void RunPendingAction()
    {
        Action action = _pendingAction;
        _pendingAction = null;
        _continueAfterSave = false;
        action?.Invoke();
    }

    private void CancelPendingSave()
    {
        if (_continueAfterSave) ClearPendingAction();
    }

    private void ClearPendingAction()
    {
        _pendingAction = null;
        _continueAfterSave = false;
        SetStatus("ACTION CANCELLED — MAP REMAINS OPEN");
    }

    private void RefreshState()
    {
        if (_fileMenu == null) return;
        string name = _document.IsOpen ? _document.Current.Metadata.DisplayName : "No map open";
        _documentLabel.Text = _document.IsDirty ? $"{name}  *" : name;
        SetDisabled(FileCommand.Save, !_document.IsOpen || !_document.IsDirty);
        SetDisabled(FileCommand.SaveAs, !_document.IsOpen);
        SetDisabled(FileCommand.Close, !_document.IsOpen);
    }

    private void SetDisabled(FileCommand command, bool disabled)
    {
        var popup = _fileMenu.GetPopup();
        popup.SetItemDisabled(popup.GetItemIndex((int)command), disabled);
    }

    private void ShowError(string message)
    {
        _errorDialog.DialogText = message;
        _errorDialog.PopupCentered();
        SetStatus("MAP OPERATION FAILED — SEE DIALOG");
    }

    private void SetStatus(string text)
    {
        if (_statusLabel != null) _statusLabel.Text = text;
    }
}
#endif
