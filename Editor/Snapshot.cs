using System.Collections.Generic;

[System.Serializable]
public class Snapshot
{
    public string title;
    public List<Snap> snaps;

    public Snapshot(string title, List<Snap> snaps)
    {
        this.title = title;
        this.snaps = snaps;
    }

    public override string ToString()
    {
        return $"[Snapshot] {title} -- # Snaps: {snaps.Count}";
    }
}
