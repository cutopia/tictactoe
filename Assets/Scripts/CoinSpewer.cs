using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinSpewer : MonoBehaviour
{
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int minAngle = 20;
    [SerializeField] private int maxAngle = 70;
    [SerializeField] private float minGravity = 2.2f;
    [SerializeField] private float maxGravity = 5.5f;
    [SerializeField] private int minVelocity = 30;
    [SerializeField] private int maxVelocity = 40;
    [SerializeField] private int minBurstCoinAmount = 5;
    [SerializeField] private int maxBurstCoinAmount = 20;
    [SerializeField] private int burstRateDelay = 5;
    [SerializeField] private float xPositionAdjustmentLeft = 200;
    [SerializeField] private float xPositionAdjustmentRight = -100;
    [SerializeField] private int totalCoinsToSpew = 200;
    int coinsRemaining;
    private List<GameObject> spewingCoins = new List<GameObject>();
    private Stack<GameObject> deletedCoins = new Stack<GameObject>();
    Dictionary<GameObject, CoinStruct> coinData = new Dictionary<GameObject, CoinStruct>();

    private struct CoinStruct
    {
        public float gravity;
        public float angle;
        public int directionalModifier;
        public float velocity;
    }

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
        int delay = burstRateDelay;
        Rect rect = GetComponent<RectTransform>().rect;
        while (spewingCoins.Count > 0)
        {
            // update coin positions
            foreach (GameObject coin in spewingCoins)
            {
                var pos = coin.transform.localPosition;
                pos.x += (coinData[coin].velocity * 0.3f * coinData[coin].directionalModifier);
                if (coinData[coin].directionalModifier > 0)
                {
                    pos.y = GetProjectileY(pos.x + xPositionAdjustmentLeft, coinData[coin].angle, coinData[coin].velocity, coinData[coin].gravity);
                }
                else
                {
                    pos.y = GetProjectileY(pos.x + xPositionAdjustmentRight, coinData[coin].angle, coinData[coin].velocity, coinData[coin].gravity);
                }

                if (coin.transform.position.y > -800) { 
                    coin.transform.localPosition = pos;
                } else {
                    deletedCoins.Push(coin);
                    coinData.Remove(coin);
                    Destroy(coin);
                }
            }
            if (coinsRemaining > 0 && (delay--) <= 0)
            {
                CreateCoins(minBurstCoinAmount, maxBurstCoinAmount);
                delay = burstRateDelay;
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
    public void CreateCoins(int min, int max)
    {
        Rect rect = GetComponent<RectTransform>().rect;
        float coinDim = rect.height / 8;
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
            CoinStruct coinDataItem = new CoinStruct();

            // which side of screen does the coin come from
            if (Random.Range(0, 10) < 5)
            {
                coin.transform.localPosition = new Vector3(rect.xMin, -400, 0);
                coinDataItem.directionalModifier = 1;
            }
            else
            {
                coin.transform.localPosition = new Vector3(rect.xMax, -400, 0);
                coinDataItem.directionalModifier = -1;
            }
            coinDataItem.gravity = Random.Range(minGravity, maxGravity);
            coinDataItem.angle = Random.Range(minAngle, maxAngle);
            coinDataItem.velocity = Random.Range(minVelocity, maxVelocity);
            coin.transform.SetParent(gameObject.transform, false);
            coinData[coin] = coinDataItem;
            spewingCoins.Add(coin);
        }
    }

    /// <summary>
    /// Starts the coins.
    /// </summary>
    public void StartCoins()
    {
        gameObject.SetActive(true);
        coinsRemaining = totalCoinsToSpew;
        CreateCoins(minBurstCoinAmount, maxBurstCoinAmount);
        StartCoroutine(AdvanceCoins());
    }

    /// <summary>
    /// Gets the projectile y.
    /// formula for projectile found on https://www.desmos.com/calculator/gjnco6mzjo
    /// </summary>
    /// <returns>The projectile y.</returns>
    float GetProjectileY(float x, float angle, float velocity, float gravity)
    {
        float a = angle * (Mathf.PI / 180);
        float v = velocity;
        float h = 0; // launching from the screen corners. height is 0
        return (h - gravity * Mathf.Pow(x / (v * Mathf.Cos(a)), 2) + Mathf.Tan(a) * x);
    }
}
