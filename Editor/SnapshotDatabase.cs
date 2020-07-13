using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SnapshotDatabase : ScriptableObject
{
    public List<Snapshot> snapshots;

    public static SnapshotDatabase GetSnapshotDB()
    {
        const string dbFilepath = "Assets/Editor/SnapshotDB.asset";
        var snapshotDB = AssetDatabase.LoadAssetAtPath<SnapshotDatabase>(dbFilepath);

        if (snapshotDB == null)
        {
            snapshotDB = CreateInstance<SnapshotDatabase>();
            snapshotDB.snapshots = new List<Snapshot>();
            AssetDatabase.CreateAsset(snapshotDB, dbFilepath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        return snapshotDB;
    }
}
