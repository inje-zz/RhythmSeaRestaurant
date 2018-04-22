using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class SignController : MonoBehaviour
{

    public GameObject[] objs;
    private int curIdx;
    // Use this for initialization
    IEnumerator Start()
    {
        int offset = Random.Range(1, 5);
        while (true)
        {
            GameObject go = objs[curIdx];
            go.transform.DOPunchPosition(Vector3.up * 0.2f, 0.25f);

            if (Mathf.FloorToInt((curIdx + offset) / objs.Length) > 0)
            {
                offset = Random.Range(1, 5);
                curIdx = 0;
            }
            else
            {
                curIdx = curIdx + offset;
            }
            //curIdx = (curIdx + offset) % objs.Length;
            yield return new WaitForSeconds(0.5f);
        }
    }

}
