using Alteruna;
using UnityEngine;

[RequireComponent(typeof(Multiplayer))]
public class NetworkManager : MonoBehaviour {
    private void Awake() {
        DontDestroyOnLoad(gameObject);
    }
}
