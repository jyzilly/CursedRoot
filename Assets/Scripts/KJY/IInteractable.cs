using UnityEngine;

public interface IInteractable
{
    //상호작용 아이템 없어도 됨, 근데 함수는 있어야 함
    void OnInteract(GameObject heldItem);
}

