using DunGen;
using UnityEditor;
using UnityEngine;

namespace itolib.editor.UI
{
    internal static class DoorwayEditor
    {
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected, typeof(Doorway))]
        internal static void DoorwayOutlineGizmo(Doorway doorway, GizmoType _)
        {
            if (doorway.Socket == null)
            {
                return;
            }

            Transform doorwayTransform = doorway.transform;

            Vector2 size = doorway.Socket.Size;
            Vector2 extents = size * 0.5f;
            float origin = Mathf.Min(size.x, size.y);

            Gizmos.color = EditorConstants.DoorDirectionColour;
            Gizmos.DrawLine(doorwayTransform.position + (doorwayTransform.up * extents.y), doorwayTransform.position + (doorwayTransform.up * extents.y) + (doorwayTransform.forward * origin));
            Gizmos.color = EditorConstants.DoorUpColour;
            Gizmos.DrawLine(doorwayTransform.position + (doorwayTransform.up * extents.y), doorwayTransform.position + (doorwayTransform.up * size.y));

            Gizmos.color = !doorway.ValidateTransform(out Bounds _, out bool isAxisAligned, out bool _)
                ? ((!isAxisAligned) ? EditorConstants.DoorRectColourError : EditorConstants.DoorRectColourWarning) : EditorConstants.DoorRectColourValid;

            Vector3 topLeftCorner = doorwayTransform.position - (doorwayTransform.right * extents.x) + (doorwayTransform.up * size.y);
            Vector3 topRightCorner = doorwayTransform.position + (doorwayTransform.right * extents.x) + (doorwayTransform.up * size.y);
            Vector3 bottomLeftCorner = doorwayTransform.position - (doorwayTransform.right * extents.x);
            Vector3 bottomRightCorner = doorwayTransform.position + (doorwayTransform.right * extents.x);

            Gizmos.DrawLineList([topLeftCorner, topRightCorner, topRightCorner, bottomRightCorner, bottomRightCorner, bottomLeftCorner, bottomLeftCorner, topLeftCorner]);
        }
    }
}