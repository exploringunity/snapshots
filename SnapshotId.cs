using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SnapshotId : MonoBehaviour
{
#if UNITY_EDITOR
    public static Dictionary<string, GameObject> idToObj = new Dictionary<string, GameObject>();

    [ReadonlyInInspector]
    public string id;

    void Awake()
    {
        if (id == null)
        {
            id = NewId();
            idToObj[id] = gameObject;
        }
        else if (idToObj.ContainsKey(id))
        {
            DestroyImmediate(this);
        }
        else
        {
            idToObj[id] = gameObject;
        }

    }

    void OnDestroy()
    {
        idToObj.Remove(id);
    }

    string NewId() { return System.Guid.NewGuid().ToString(); }
#endif
}
