using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public enum FishType
{
    A,
    B,
}

public enum Piano
{
    P_Do,
    P_Re,
    P_Mi,
    P_Fa,
    P_Sol,
    P_Ra,
    P_Si,
    P_HDo,
}

public class Rhythm
{
    public Piano scale;
    public float time;

    public Rhythm(Piano s, float t)
    {
        scale = s;
        time = t;
    }
}


public class RSRMgr : MonoBehaviour
{
    public GameObject fishPrefabs;
    public GameObject cookedFishPrefabs;

    public CameraController cam;
    public GameObject cat;
    public GameObject uiResult;
    public GameObject uiPoint;
    public Text readyText;
    public Text likePointText;
    public Text resultText;
    public FishDrone drone;

    public GameObject startGuide;
    public GameObject fishingGuide;

    private GameObject FishRoot;
    private int poolCnt = 50;
    private List<Fish> fishPool = new List<Fish>();
    public List<Fish> activeFishes = new List<Fish>();

    private List<SpriteRenderer> cookedFishes = new List<SpriteRenderer>();

    public AudioSource[] piano;
    public AudioSource s_Success;
    public AudioSource s_Fail;
    public AudioSource s_Shot;

    public GameObject[] consumer;
    public GameObject cookedFishPos;
    public GameObject TrowingGamePos;

    public GameObject catFishingPos;
    public GameObject catCookPos;

    public GameObject maketEffectPos;
    public GameObject[] cookEye;
    public GameObject[] cookBody;

    public Sprite[] cookedFishSprite;

    private int cookedFishCnt = 0;

    private int likePoint = 0;

    public int comboCnt = 0;

    public GameObject[] trowingGameGuide;

    // Use this for initialization
    void Start()
    {
        FishRoot = GameObject.Find("Fishes");

        Init();

        StartCoroutine(IdleStart());
    }

    void Init()
    {
        for (int i = 0; i < poolCnt; i++)
        {
            GameObject go = Instantiate(fishPrefabs);
            Fish fish = go.GetComponent<Fish>();

            go.transform.parent = FishRoot.transform;
            go.SetActive(false);
            fishPool.Add(fish);
        }

        for (int i = 0; i < poolCnt; i++)
        {
            GameObject go = Instantiate(cookedFishPrefabs);
            SpriteRenderer spRender = go.GetComponent<SpriteRenderer>();

            go.transform.parent = transform.Find("CookedFish");
            go.SetActive(false);
            cookedFishes.Add(spRender);
        }
    }

    void SpawnFish(int cnt)
    {
        activeFishes.Clear();
        for (int i = 0; i < cnt; i++)
        {
            fishPool[i].gameObject.SetActive(true);
            fishPool[i].Init(drone, (FishType)Random.Range(0, 2));
            activeFishes.Add(fishPool[i]);
        }
    }

    void DisableFishPool()
    {
        for (int i = 0; i < fishPool.Count; i++)
        {
            fishPool[i].gameObject.SetActive(false);
        }
    }

    IEnumerator IdleStart()
    {
        uiResult.gameObject.SetActive(false);
        uiPoint.gameObject.SetActive(false);
        fishingGuide.SetActive(false);
        for (int i = 0; i < trowingGameGuide.Length; i++)
            trowingGameGuide[i].SetActive(false);

        DisableFishPool();
        SpawnFish(25);


        likePoint = 0;
        likePointText.text = likePoint.ToString();

        comboCnt = 0;

        cookedFishCnt = 0;

        cam.SetTarget(maketEffectPos);
        cam.clampPos = false;
        cam.SetOrthoSize(4f);

        drone.gameObject.SetActive(true);
        drone.Init();

        startGuide.SetActive(true);
        cat.transform.position = catFishingPos.transform.position;
        cat.transform.Find("hat").gameObject.SetActive(false);

        while (true)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                break;
            }
            yield return null;
        }

        startGuide.SetActive(false);
        cam.SetTarget(drone.gameObject);

        drone.transform.DORotate(Vector3.zero, 1f);

        drone.transform.DOMove(Vector3.zero, 4f)
        .OnComplete(() =>
        {
            cam.clampPos = true;

            cam.SetDestOrthoSize(5f);
            uiPoint.gameObject.SetActive(true);
            StartCoroutine(drone.RhythmLoop());

            fishingGuide.SetActive(true);
        });
    }


    // Update is called once per frame
    void Update()
    {

    }

    public void Play(Piano scale)
    {
        piano[(int)scale].Play();
    }

    public IEnumerator CookEffect()
    {
        bool isCatchFish = false;
        for (int i = 0; i < activeFishes.Count; i++)
        {
            if (activeFishes[i].fascinated == true)
            {
                isCatchFish = true;
                break;
            }
        }

        if (isCatchFish)
        {
            MoveCatAtCook();
            yield return new WaitForSeconds(1f);
            StartCoroutine(IECookEffect(cookEye));
            StartCoroutine(IECookEffect(cookBody));
            StartCoroutine(IEStackCookedFish());
        }
        else
        {
            StartCoroutine(GameEndStart());
        }
    }

    IEnumerator IECookEffect(GameObject[] target)
    {
        Vector3 pos = maketEffectPos.transform.position;
        float xOffset = 3f;
        float width = 4f;
        for (int i = 0; i < target.Length; i++)
        {
            GameObject go = target[i];
            go.SetActive(true);
            go.transform.position = pos;
            go.transform.DORotate(new Vector3(0, 0, Random.Range(-180f, 180f)), 1.5f, RotateMode.FastBeyond360);
            go.transform.DOJump(new Vector3(Random.Range(pos.x - xOffset - width, pos.x + xOffset + width), Random.Range(pos.y - 4f, pos.y - 0f), 0), Random.Range(0.5f, 1f), Random.Range(1, 3), Random.Range(0.2f, 1f))
            .OnComplete(() =>
            {
                StartCoroutine(IESetActive(go, Random.Range(0.25f, 1.0f)));
            });
            yield return new WaitForSeconds(Random.Range(0.05f, 0.25f));
        }
    }

    IEnumerator IESetActive(GameObject go, float time)
    {
        yield return new WaitForSeconds(time);
        go.SetActive(false);
    }

    IEnumerator IEStackCookedFish()
    {
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < activeFishes.Count; i++)
        {
            if (activeFishes[i].fascinated == false)
                continue;
            StackCookedFish((int)activeFishes[i].type);
            yield return new WaitForSeconds(Random.Range(0.01f, 0.05f));
        }

        StartCoroutine(TrowingGameStart());
    }

    void StackCookedFish(int type)
    {
        float offset = 0.25f;
        SpriteRenderer cookedFish = cookedFishes[cookedFishCnt];
        cookedFish.gameObject.SetActive(true);
        cookedFish.transform.localScale = Vector3.one;
        cookedFish.sprite = cookedFishSprite[type];
        cookedFish.sortingOrder = cookedFishCnt;

        cookedFish.transform.position = cookedFishPos.transform.position + Vector3.up * offset * cookedFishCnt;

        cookedFishCnt++;
    }

    IEnumerator TrowingGameStart()
    {
        float offset = 0.25f;
        List<Tweener> oldTweener = new List<Tweener>();

        for (int i = 0; i < trowingGameGuide.Length; i++)
            trowingGameGuide[i].SetActive(true);
        cam.SetDestOrthoSize(4f);

        //Ready
        yield return StartCoroutine(Ready());

        yield return null;


        for (int i = 0; i < cookedFishCnt; i++)
        {
            for (int twIdx = 0; twIdx < oldTweener.Count; twIdx++)
            {
                oldTweener[twIdx].Kill();
            }
            oldTweener.Clear();

            SpriteRenderer cookedFish = cookedFishes[i];
            cookedFish.transform.DOScale(1.5f, 0.2f).SetEase(Ease.OutBounce);
            cookedFish.transform.position = TrowingGamePos.transform.position;

            for (int k = i + 1; k < cookedFishCnt; k++)
            {
                Tweener tw = cookedFishes[k].transform.DOMoveY(cookedFishes[k].transform.position.y - offset, 0.15f);
                oldTweener.Add(tw);
            }

            float waitTime = 0;
            while (true)
            {
                if (Input.GetKeyDown(KeyCode.A))
                {
                    cat.transform.position = catCookPos.transform.position;
                    cat.transform.DOPunchPosition(Vector3.up * 0.15f, 0.15f);
                    s_Shot.Stop();
                    s_Shot.Play();
                    cookedFish.transform.DOJump(consumer[0].transform.Find("pos").position, 1, 1, 0.3f)
                    .OnComplete(() =>
                    {
                        CheckThrowing(cookedFish, consumer[0], waitTime);
                    });
                    break;
                }
                else if (Input.GetKeyDown(KeyCode.D))
                {
                    cat.transform.position = catCookPos.transform.position;
                    cat.transform.DOPunchPosition(Vector3.up * 0.15f, 0.15f);
                    s_Shot.Stop();
                    s_Shot.Play();
                    cookedFish.transform.DOJump(consumer[1].transform.Find("pos").position, 1, 1, 0.3f)
                    .OnComplete(() =>
                    {
                        CheckThrowing(cookedFish, consumer[1], waitTime);
                    });
                    break;
                }

                waitTime += Time.deltaTime;
                yield return null;
            }
            yield return null;
        }

        yield return new WaitForSeconds(1f);
        for (int i = 0; i < trowingGameGuide.Length; i++)
            trowingGameGuide[i].SetActive(false);

        StartCoroutine(GameEndStart());
    }

    void CheckThrowing(SpriteRenderer fish, GameObject consumer, float waitTime)
    {
        int fishType = 0;
        int consumerType = 0;

        if (fish.sprite.name.Contains("1"))
        {
            fishType = 0;
        }
        else if (fish.sprite.name.Contains("2"))
        {
            fishType = 1;
        }

        if (consumer.name.Contains("1"))
        {
            consumerType = 0;
        }
        else if (consumer.name.Contains("2"))
        {
            consumerType = 1;
        }

        //Debug.Log(fish.sprite.name + " / " + consumer.name);
        if (fishType == consumerType)
        {
            int pt = 0;
            if (waitTime < 0.2f)
            {
                pt = 20;
            }
            else if (waitTime < 0.4f)
            {
                pt = 16;
            }
            else if (waitTime < 0.6f)
            {
                pt = 12;
            }
            else if (waitTime < 0.8f)
            {
                pt = 8;
            }
            else if (waitTime < 1.5f)
            {
                pt = 4;
            }
            else
            {
                pt = 2;
            }
            AddLikePoint(pt);

            //Debug.Log("Yes");
            consumer.transform.localScale = Vector3.one * 0.78172f;
            consumer.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f);
            Combo();
            PlaySuccess();
        }
        else
        {
            //Debug.Log("No");
            consumer.transform.DOShakeScale(0.2f, new Vector3(1, 1, 0) * 0.2f, 50);
            ComboFail();
        }
        fish.gameObject.SetActive(false);

        
    }


    IEnumerator GameEndStart()
    {
        uiResult.gameObject.SetActive(true);
        uiPoint.gameObject.SetActive(false);

        resultText.text = likePoint.ToString();

        while (true)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                break;
            }
            yield return null;
        }
        Debug.Log("GameEndStart");
        yield return null;

        StartCoroutine(IdleStart());
    }

    public IEnumerator Ready()
    {
        //Ready
        readyText.gameObject.SetActive(true);
        readyText.text = "3";
        readyText.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f);
        Play(Piano.P_Mi);
        yield return new WaitForSeconds(1f);
        readyText.text = "2";
        readyText.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f);
        Play(Piano.P_Mi);
        yield return new WaitForSeconds(1f);
        readyText.text = "1";
        readyText.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f);
        Play(Piano.P_Mi);
        yield return new WaitForSeconds(1f);

        readyText.text = "♪~";
        readyText.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f);
        Play(Piano.P_HDo);

        readyText.gameObject.SetActive(false);
    }

    Tweener likeTweener = null;
    public void AddLikePoint(int value)
    {
        float mul = 1f + comboCnt / 3f * 0.1f;
        int result = (int)(value * mul);

        //Debug.Log(comboCnt + " / " + mul + " / " + value + " / " + result);
        likePoint += result;
        likePointText.text = likePoint.ToString();

        if (likeTweener != null)
        {
            likeTweener.Kill();
            likePointText.transform.localScale = Vector3.one;
        }
        likeTweener = likePointText.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f);
    }

    public void Combo()
    {
        comboCnt++;
    }

    public void ComboFail()
    {
        comboCnt = 0;
        PlayFail();
    }

    public void PlaySuccess()
    {
        s_Success.Stop();
        s_Success.Play();
    }
    public void PlayFail()
    {
        s_Fail.Stop();
        s_Fail.Play();
    }

    void MoveCatAtCook()
    {
        //cat.transform.position = catCookPos.transform.position;
        //cat.transform.DOMove(catCookPos.transform.position, 1.2f).SetEase(Ease.InBack);
        cat.transform.Find("hat").gameObject.SetActive(true);
        cat.transform.DOJump(catCookPos.transform.position, 1f, 4, 1.5f);
    }
}
