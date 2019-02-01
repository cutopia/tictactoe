using UnityEngine;

public class RibController : MonoBehaviour {

    public void AnimateRibs()
    {
        GetComponent<Animation>().Play();
    }
}
