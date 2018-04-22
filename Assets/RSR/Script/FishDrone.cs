using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class FishDrone : MonoBehaviour
{
    public enum State
    {
        Idle = 0,
        Fising,
        FishingEnd,
        Cook,
        CookEnd,
        
    }

    public Transform fishDropPos;
    public Transform lineStartPos;
    public Transform[] mousePos;
    public LineRenderer line;

    public float speed = 5f;

    public SpriteRenderer hitBase;
    public SpriteRenderer rangeCircle;
    public SpriteRenderer btnGuide;

    private RSRMgr rsrMgr = null;
    private float raduis = 3.5f;
    public SpriteRenderer spRender;
    public State state = State.Idle;

    public Sprite[] spFish;

    private Vector3 oldPos = Vector3.zero;
    // Use this for initialization
    void Start()
    {
        spRender = GetComponent<SpriteRenderer>();
        rsrMgr = FindObjectOfType<RSRMgr>();

        Camera.main.GetComponent<CameraController>().maxY = 11f;
        //state = State.FishingEnd;
        hitBase.gameObject.SetActive(false);
        state = State.Idle;
    }

    public void Init()
    {
        state = FishDrone.State.Idle;
        spRender.flipX = true;
        transform.position = lineStartPos.position - Vector3.up * 3f;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, -90f));

        spRender.sprite = spFish[0];
        hitBase.color = new Color(1, 1, 1, 80f / 255f);
    }

    // Update is called once per frame
    void Update()
    {
        if(state == State.Idle)
        {
            line.gameObject.SetActive(true);

            line.SetPosition(0, lineStartPos.transform.position);
            Vector3 pos = spRender.flipX == true ? mousePos[0].transform.position : mousePos[1].transform.position;
            line.SetPosition(1, pos);
        }
        else if (state == State.Fising)
        {
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");

            transform.Translate(new Vector3(moveHorizontal, moveVertical, 0) * speed * Time.deltaTime);

            Vector3 dest = transform.position;
            dest.x = Mathf.Clamp(dest.x, -17f, 17f);
            dest.y = Mathf.Clamp(dest.y, -4, 10f);
            transform.position = dest;



            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
            {
                pressBtnType = Input.GetKeyDown(KeyCode.A) == true ? 0 : 1;
                HitBit();
            }


            if (moveHorizontal > 0)
            {
                spRender.flipX = false;
            }
            else if (moveHorizontal < 0)
            {
                spRender.flipX = true;
            }

            line.gameObject.SetActive(true);

            line.SetPosition(0, lineStartPos.transform.position);
            Vector3 pos = spRender.flipX == true ? mousePos[0].transform.position : mousePos[1].transform.position;
            line.SetPosition(1, pos);
        }
        else if (state == State.FishingEnd)
        {
            Camera.main.GetComponent<CameraController>().maxY = 17f;

            transform.DOLocalPath(new Vector3[] { Vector3.zero, lineStartPos.position, lineStartPos.position + Vector3.up * 10f, fishDropPos.position + Vector3.up * 10f, fishDropPos.position }, 5f, PathType.CatmullRom)
            .OnComplete(() =>
            {
                line.gameObject.SetActive(false);
                gameObject.SetActive(false);

                rsrMgr.StartCoroutine(rsrMgr.CookEffect());
                
                state = State.CookEnd;
            });
            hitBase.gameObject.SetActive(false);
            rsrMgr.fishingGuide.SetActive(false);
            
            state = State.Cook;
        }
        else if (state == State.Cook)
        {
            spRender.flipX = false;
            transform.right = (transform.position - oldPos).normalized;

            line.gameObject.SetActive(true);
            line.SetPosition(0, lineStartPos.transform.position);
            Vector3 pos = spRender.flipX == true ? mousePos[0].transform.position : mousePos[1].transform.position;
            line.SetPosition(1, pos);
        }
        else if (state == State.CookEnd)
        {

        }


        oldPos = transform.position;

    }
    Tweener tweener = null;

    void StartBit()
    {
        //Debug.Log("StartBit");
        bitCnt++;

        hitBase.transform.localPosition = Vector3.zero;
        rangeCircle.gameObject.SetActive(true);
        rangeCircle.transform.localScale = Vector3.one * 2f;
        tweener = rangeCircle.transform.DOScale(0.01f, delayTime);
    }

    void HitBit()
    {
        //Debug.Log("HitBit");
        if (bitCnt <= 0)
        {
            //Debug.Log("HitBit return");
            return;
        }
        bitCnt--;

        if (tweener != null)
            tweener.Kill();

        rangeCircle.gameObject.SetActive(false);

        bool isMatchTime = false;
        bool isMatchBtn = false;

        if (pressBtnType == curBtnType)
        {
            isMatchBtn = true;
        }

        float progress = rangeCircle.transform.localScale.x;
        if (progress <= 1f && progress >= 0.65f)
        {
            isMatchTime = true;
        }

        if (isMatchTime && isMatchBtn)
        {
            List<Fish> fish = rsrMgr.activeFishes;
            bool isFind = false;
            for (int i = 0; i < fish.Count; i++)
            {
                if((int)fish[i].type!= curBtnType)
                {
                    continue;
                }

                float dist = Vector3.Distance(transform.position, fish[i].transform.position);
                if (dist < raduis)
                {
                    fish[i].Fascinate();
                    rsrMgr.AddLikePoint(10);
                    isFind = true;
                }
            }
            transform.DOPunchScale(Vector3.one * 0.2f, 0.5f);
            rsrMgr.Play(rhythm[hitCnt].scale);
            if (isFind)
                rsrMgr.Combo();
        }
        else
        {
            hitBase.transform.DOShakePosition(0.3f, new Vector3(1, 1, 0) * 0.4f, 50);
            rsrMgr.ComboFail();
        }

        hitCnt++;

    }

    int bitCnt = 0;
    float delayTime = 2f;
    int curBtnType = 0;
    int pressBtnType = 0;
    int curFishType = 0;
    int hitCnt = 0;
    List<Rhythm> rhythm = null;

    public IEnumerator RhythmLoop()
    {
        
        hitBase.gameObject.SetActive(true);

        bitCnt = 0;
        hitCnt = 0;
        int curIdx = 0;
        rhythm = new List<Rhythm>()
        {
            new Rhythm(Piano.P_Sol, 1f),
            new Rhythm(Piano.P_Sol, 1f),
            new Rhythm(Piano.P_Ra, 1f),
            new Rhythm(Piano.P_Ra, 1f),

            new Rhythm(Piano.P_Sol, 1f),
            new Rhythm(Piano.P_Sol, 1f),
            new Rhythm(Piano.P_Mi, 2f),

            new Rhythm(Piano.P_Sol, 1f),
            new Rhythm(Piano.P_Sol, 1f),
            new Rhythm(Piano.P_Mi, 1f),
            new Rhythm(Piano.P_Mi, 1f),

            new Rhythm(Piano.P_Re, 4f),


            new Rhythm(Piano.P_Sol, 1f),
            new Rhythm(Piano.P_Sol, 1f),
            new Rhythm(Piano.P_Ra, 1f),
            new Rhythm(Piano.P_Ra, 1f),

            new Rhythm(Piano.P_Sol, 1f),
            new Rhythm(Piano.P_Sol, 1f),
            new Rhythm(Piano.P_Mi, 2f),

            new Rhythm(Piano.P_Sol, 1f),
            new Rhythm(Piano.P_Mi, 1f),
            new Rhythm(Piano.P_Re, 1f),
            new Rhythm(Piano.P_Mi, 1f),

            new Rhythm(Piano.P_Do, 4f)
        };


        //Ready
        yield return StartCoroutine(rsrMgr.Ready());

        state = State.Fising;

        yield return new WaitForSeconds(1.5f);

        while (curIdx < rhythm.Count)
        {
            if (curIdx == 0)
            {
                curFishType = 0;
            }
            else
            {
                if (rhythm[curIdx].scale == rhythm[curIdx - 1].scale)
                {
                }
                else
                {
                    curFishType = (curFishType + 1) % 2;
                }
            }

            curBtnType = curFishType;
            spRender.sprite = spFish[curBtnType];
            if (curBtnType == 0)
            {
                hitBase.color = new Color(124f / 255f, 242f / 255f, 135f / 255f, 0.5f);
            }
            else
            {
                hitBase.color = new Color(255f / 255f, 183f / 255f, 107f / 255f, 0.5f);
            }

            if (bitCnt > 0)
            {
                HitBit();
            }
            StartBit();
            yield return new WaitForSeconds(rhythm[curIdx].time);
            curIdx++;
        }


        state = State.FishingEnd;
    }
}
