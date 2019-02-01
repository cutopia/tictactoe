using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinSpewer : MonoBehaviour
{
    [SerializeField] private GameObject coinPrefab;
    static int INITIAL_COINS_REMAINING = 300;
    int coinsRemaining = INITIAL_COINS_REMAINING;
    private List<GameObject> spewingCoins = new List<GameObject>();
    private Stack<GameObject> deletedCoins = new Stack<GameObject>();
    Dictionary<GameObject, float[]> initialTrajectories = new Dictionary<GameObject, float[]>();

    // Use this for initialization
    void Awake()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Advances the coins.
    /// </summary>
    /// <returns>The coins.</returns>
    IEnumerator AdvanceCoins()
    {
        int delay = 10;
        while (spewingCoins.Count > 0)
        {
            // update coin positions
            foreach (GameObject coin in spewingCoins)
            {
                var pos = coin.transform.localPosition;
                pos.x += (8.0f * initialTrajectories[coin][2]);

                pos.y = GetProjectileY(pos.x, initialTrajectories[coin][0], initialTrajectories[coin][1]);
                if (coin.transform.position.y <= 0)
                {
                    deletedCoins.Push(coin);
                    initialTrajectories.Remove(coin);
                    Destroy(coin);
                }
                else
                {
                    coin.transform.localPosition = pos;
                }
            }
            if (coinsRemaining > 0 && (delay--) <= 0)
            {
                CreateCoins();
                delay = 10;
            }
            while (deletedCoins.Count > 0)
            {
                spewingCoins.Remove(deletedCoins.Pop());
            }
            yield return null;
        }
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Creates a set of random coins
    /// </summary>
    /// <param name="min">Minimum.</param>
    /// <param name="max">Max.</param>
    public void CreateCoins(int min = 20, int max = 40)
    {
        float width = GetComponent<RectTransform>().rect.width;
        float height = GetComponent<RectTransform>().rect.height;
        float coinDim = height / 8;
        int numberOfCoins = Random.Range(min, max);
        for (int i = 0; i < numberOfCoins; i++)
        {
            if (coinsRemaining <= 0)
            {
                break;
            }
            coinsRemaining--;
            GameObject coin = Instantiate(coinPrefab) as GameObject;
            coin.GetComponent<RectTransform>().sizeDelta = new Vector2(coinDim, coinDim);

            coin.transform.localPosition = new Vector3(Random.Range(width * -0.2f, 0), Random.Range(0.1f, height * 0.8f), 0);
            initialTrajectories[coin] = new float[3];
            if (Random.Range(0, 10) < 5)
            {
                initialTrajectories[coin][1] = Random.Range(-10, -30);
                coin.transform.localPosition = new Vector3(Random.Range(width * -0.2f, 0), Random.Range(0.1f, height * 0.8f), 0);
                initialTrajectories[coin][2] = 1;
            }
            else
            {
                initialTrajectories[coin][2] = -1;
                initialTrajectories[coin][1] = Random.Range(40, 50);
                coin.transform.localPosition = new Vector3(Random.Range(width * 2.2f, width * 2.4f), Random.Range(0.1f, height * 0.8f), 0);
            }
            initialTrajectories[coin][0] = coin.transform.localPosition.y;
            coin.transform.SetParent(gameObject.transform);
            spewingCoins.Add(coin);
        }
    }

    /// <summary>
    /// Starts the coins.
    /// </summary>
    public void StartCoins()
    {
        gameObject.SetActive(true);
        coinsRemaining = INITIAL_COINS_REMAINING;
        CreateCoins(30, 80);
        StartCoroutine(AdvanceCoins());
    }

    /// <summary>
    /// Gets the projectile y.
    /// formula for projectile found on https://www.desmos.com/calculator/gjnco6mzjo
    /// </summary>
    /// <returns>The projectile y.</returns>
    /// <param name="paramX">Parameter x.</param>
    /// <param name="initialHeight">Initial height.</param>
    /// <param name="angle">Angle.</param>
    float GetProjectileY(float paramX, float initialHeight, float angle)
    {
        float x = paramX;
        float A = angle;
        float a = A * (Mathf.PI / 180);
        float v = 40;
        float h = initialHeight;
        return (h - 4.9f * Mathf.Pow(x / (v * Mathf.Cos(a)), 2) + Mathf.Tan(a) * x);
    }
}
