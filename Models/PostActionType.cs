namespace PrintMaster.Models;

/// <summary>Optionale Aktion nach dem Drucken/Verarbeiten der Datei.</summary>
public enum PostActionType
{
    None = 0,
    Delete,
    Move,
    Rename
}
