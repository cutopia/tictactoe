using System.Collections;
using UnityEngine;

public class BigWin : MonoBehaviour {
    [SerializeField] CoinSpewer coinSpewer;
    // Use this for initialization
    void Start () {
        gameObject.SetActive(false);
	}
    SimpleDel _registeredCallback;


    public void PlayBigWinAnimation(SimpleDel callback)
    {
        _registeredCallback = callback;
        coinSpewer.StartCoins();
        gameObject.SetActive(true);
        GetComponent<Animator>().SetTrigger("ActivateBigWinTrigger");
    }

    public void OnPlayBigWinComplete()
    {
        gameObject.SetActive(false);
        if (_registeredCallback != null)
        {
            _registeredCallback();
        }
    }
}
