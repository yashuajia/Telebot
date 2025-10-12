using UnityEngine;
public interface ICanDrag
{
    public void OnDragStart();
    public void OnDragUpdate(Vector2 mouseWorldPos);
    public void OnDragEnd();
}