public class UndoAction
{
    public PlaceModel tray;
    public CupModel cup; 
    public BoardSlot previousSlot;

    public UndoAction(PlaceModel tray, CupModel cup, BoardSlot previousSlot)
    {
        this.tray = tray;
        this.cup = cup;
        this.previousSlot = previousSlot;
    }
}