using UnityEngine;

[System.Serializable]
public class Snap
{
    public string id;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public Snap(string id_, Transform tf)
    {
        id = id_;
        position = tf.position;
        rotation = tf.rotation;
        scale = tf.localScale;
    }

    public void Restore()
    {
        var go = SnapshotId.idToObj[id];
        var tf = go.transform;
        tf.position = position;
        tf.rotation = rotation;
        tf.localScale = scale;
    }

    public Transform GetTransform()
    {
        return SnapshotId.idToObj[id].transform;
    }

    public override string ToString()
    {
        var rotDegrees = rotation.eulerAngles;
        return $"[Snap] {id} : {position}, {rotDegrees}, {scale}";
    }
}
