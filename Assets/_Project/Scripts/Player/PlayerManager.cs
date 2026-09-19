using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Alteruna;

public class PlayerManager : AttributesSync
{
    [SynchronizableField] public int index;
    private Alteruna.Avatar avatar;
    private int id;

    private void Start()
    {
        avatar = GetComponent<Alteruna.Avatar>();

        if (!avatar.IsMe) {
            GetComponentsInChildren<GameObject>()[index].SetActive(true);
            return;
        }

        index = PlayerInformation.SkinIndex;
        GetComponentsInChildren<GameObject>()[index].SetActive(true);
        id = index;
    }

    private void Update()
    {
        if (!avatar.IsMe)
            return;
        index = id;
    }
}
