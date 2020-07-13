using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SnapshotsWindow : EditorWindow
{
    SnapshotDatabase snapshotDB;
    List<Snapshot> snapshots => snapshotDB.snapshots;
    Dictionary<string, int> idToNumSnapshots;

    VisualTreeAsset snapshotDetailTemplate;
    TextField newSnapshotTitle;
    Button createSnapshotBtn;
    VisualElement snapshotDetailsContainer;
    Label selectedInfoHeader;
    Label selectedInfoDetails;
    bool showSelectedInfo;

    [MenuItem("ExploringUnity/SnapshotsWindow %#q")]
    public static void OpenWindow()
    {
        SnapshotsWindow wnd = GetWindow<SnapshotsWindow>();
        wnd.titleContent = new GUIContent("SnapshotsWindow");
    }

    public void OnEnable()
    {
        InitializeSnapshotIdMap();
        snapshotDB = SnapshotDatabase.GetSnapshotDB();
        InitializeSnapshotCounter();
        snapshotDetailTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/SnapshotDetail.uxml");
        var uiTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/SnapshotsWindow.uxml");
        var ui = uiTemplate.CloneTree();
        rootVisualElement.Add(ui);

        selectedInfoHeader = ui.Q<Label>("selectedInfoHeader");
        selectedInfoDetails = ui.Q<Label>("selectedInfoDetails");
        newSnapshotTitle = ui.Q<TextField>("newSnapshotTitle");
        createSnapshotBtn = ui.Q<Button>("createSnapshotBtn");
        snapshotDetailsContainer = ui.Q("snapshotDetailsContainer");

        createSnapshotBtn.clicked += CreateSnapshot;
        selectedInfoHeader.RegisterCallback<MouseDownEvent>(ToggleSelectedInfo);

        foreach (var snapshot in snapshots)
        {
            snapshotDetailsContainer.Add(CreateSnapshotDetailUI(snapshot));
        }

        HandleSelectionChange();
    }

    void DeleteSnapshot(Snapshot snapshot, VisualElement detailUi)
    {
        snapshots.Remove(snapshot);
        SyncDatabase();
        detailUi.RemoveFromHierarchy();

        foreach (var snap in snapshot.snaps)
        {
            DecrementSnapshotCount(snap);
            if (idToNumSnapshots[snap.id] == 0)
            {
                var gameObj = SnapshotId.idToObj[snap.id];
                var snapshotId = gameObj.GetComponent<SnapshotId>();
                DestroyImmediate(snapshotId);
                idToNumSnapshots.Remove(snap.id);
            }
        }
    }

    void CreateSnapshot()
    {
        var selected = Selection.gameObjects;
        var numSelected = selected.Length;
        if (numSelected == 0) { return; }

        var titleRaw = newSnapshotTitle.text;
        var title = string.IsNullOrEmpty(titleRaw) ? "Untitled" : titleRaw;

        var snaps = new List<Snap>();
        foreach (var obj in selected)
        {
            var uniqueId = obj.GetComponent<SnapshotId>();
            if (uniqueId == null)
            {
                uniqueId = obj.AddComponent<SnapshotId>();
            }

            var snap = new Snap(uniqueId.id, obj.transform);
            snaps.Add(snap);
            IncrementSnapshotCount(snap);
        }
        var snapshot = new Snapshot(title, snaps);
        snapshots.Add(snapshot);
        SyncDatabase();
        snapshotDetailsContainer.Add(CreateSnapshotDetailUI(snapshot));
    }

    VisualElement CreateSnapshotDetailUI(Snapshot snapshot)
    {
        var detailUi = snapshotDetailTemplate.CloneTree();
        var snapshotHeader = detailUi.Q<Label>("snapshotHeader");
        var snapDetails = detailUi.Q<Label>("snapDetails");
        var restoreBtn = detailUi.Q<Button>("restoreBtn");
        restoreBtn.clicked += () => RestoreSnapshot(snapshot);
        var deleteBtn = detailUi.Q<Button>("deleteBtn");
        deleteBtn.clicked += () => DeleteSnapshot(snapshot, detailUi);
        var showDetails = detailUi.Q<Toggle>("showDetails");
        showDetails.style.display = DisplayStyle.None;
        UpdateSnapshotDetails(snapshot, snapshotHeader, snapDetails, showDetails);
        void onClick(MouseDownEvent evt) => ToggleSnapshotDetails(snapshot,
                                                                  snapshotHeader,
                                                                  snapDetails,
                                                                  showDetails);
        snapshotHeader.RegisterCallback<MouseDownEvent>(onClick);

        return detailUi;
    }

    void OnSelectionChange() { HandleSelectionChange(); }

    void HandleSelectionChange()
    {
        var selected = Selection.gameObjects;
        var numSelected = selected.Length;

        UpdateSelectedInfoHeader(numSelected);
        UpdateSelectedInfoDetails(selected);
    }

    void UpdateSelectedInfoHeader(int numSelected)
    {
        var prefix = showSelectedInfo ? "\u25BC" : "\u25BA";
        selectedInfoHeader.text = $"{prefix} Selected GameObjects ({numSelected})";
    }

    void UpdateSelectedInfoDetails(GameObject[] selected)
    {
        if (showSelectedInfo)
        {
            selectedInfoDetails.style.display = DisplayStyle.Flex;
            var details = from x in selected
                          let name = x.name
                          let tf = x.transform
                          let pos = tf.position
                          let rot = tf.rotation.eulerAngles
                          let scale = tf.localScale
                          select $"{name}: {pos}, {rot}, {scale}";
            selectedInfoDetails.text = string.Join("\n", details);
        }
        else
        {
            selectedInfoDetails.style.display = DisplayStyle.None;
        }
    }

    void ToggleSelectedInfo(MouseDownEvent evt)
    {
        showSelectedInfo = !showSelectedInfo;
        HandleSelectionChange();
    }

    void UpdateSnapshotDetails(Snapshot snapshot,
                               Label headerLbl,
                               Label detailsLbl,
                               Toggle showDetails)
    {
        var prefix = showDetails.value ? "\u25BC" : "\u25BA";
        headerLbl.text = $"{prefix} {snapshot.title} ({snapshot.snaps.Count})";

        if (showDetails.value)
        {
            detailsLbl.style.display = DisplayStyle.Flex;
            var snapDetails = from snap in snapshot.snaps
                              let id = snap.id
                              let pos = snap.position
                              let rot = snap.rotation.eulerAngles
                              let scale = snap.scale
                              select $"{id}: {pos}, {rot}, {scale}";
            detailsLbl.text = string.Join("\n", snapDetails);
        }
        else
        {
            detailsLbl.style.display = DisplayStyle.None;
        }
    }

    void ToggleSnapshotDetails(Snapshot snapshot,
                               Label headerLbl,
                               Label detailsLbl,
                               Toggle showDetails)
    {
        showDetails.value = !showDetails.value;
        UpdateSnapshotDetails(snapshot, headerLbl, detailsLbl, showDetails);
    }

    void InitializeSnapshotIdMap()
    {
        var uniqueIds = Resources.FindObjectsOfTypeAll<SnapshotId>();
        foreach (var uniqueId in uniqueIds)
        {
            SnapshotId.idToObj[uniqueId.id] = uniqueId.gameObject;
        }
    }

    void RestoreSnapshot(Snapshot snapshot)
    {
        var transforms = snapshot.snaps.Select(x => x.GetTransform()).ToArray();
        Undo.RecordObjects(transforms, $"Restoring: {snapshot}");
        foreach (var snap in snapshot.snaps)
        {
            snap.Restore();
        }
    }

    void SyncDatabase()
    {
        EditorUtility.SetDirty(snapshotDB);
        AssetDatabase.SaveAssets();
    }

    void InitializeSnapshotCounter()
    {
        idToNumSnapshots = new Dictionary<string, int>();
        foreach (var snapshot in snapshots)
        {
            foreach (var snap in snapshot.snaps)
            {
                IncrementSnapshotCount(snap);
            }
        }
    }

    private void IncrementSnapshotCount(Snap snap)
    {
        idToNumSnapshots.TryGetValue(snap.id, out var count);
        idToNumSnapshots[snap.id] = count + 1;
    }

    private void DecrementSnapshotCount(Snap snap)
    {
        idToNumSnapshots[snap.id] = idToNumSnapshots[snap.id] - 1;
    }
}
