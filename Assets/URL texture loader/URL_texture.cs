using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class URL_texture : MonoBehaviour {
	public string url = "https://lh3.googleusercontent.com/rDOyKaphK1TO13HmTTy61wQctUyu6qXR_UduaZvvPIbX_Aa5JJhu71zbG7N1IHSJlqqf=w2247-h1264";

	IEnumerator Start()
	{
		if (Application.internetReachability != NetworkReachability.NotReachable)
		{
			if (null == url || url.Length < 1) {
				Debug.Log ("No url");
				yield return null;
			} else {
				UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
				yield return request.SendWebRequest();
				if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
					Debug.Log(request.error);
				else
					GetComponent<Renderer>().material.mainTexture = ((DownloadHandlerTexture)request.downloadHandler).texture;
			}
		}
	}
}