using System.Collections;
using UnityEngine;

public class BigWin : MonoBehaviour {
    [SerializeField] CoinSpewer coinSpewer;
    // Use this for initialization
    void Start () {
        gameObject.SetActive(false);
	}

    public void PlayBigWinAnimation(SimpleDel callback)
    {
        coinSpewer.StartCoins();
        gameObject.SetActive(true);
        GetComponent<Animation>().Play();
        if (callback != null)
        {
            StartCoroutine(onComplete(GetComponent<Animation>(), callback));
        }
    }

    IEnumerator onComplete(Animation anim, SimpleDel callback)
    {
        while (anim.isPlaying)
        {
            yield return null;
        }
        gameObject.SetActive(false);
        callback();
    }
}
