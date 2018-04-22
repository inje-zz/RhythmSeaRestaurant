using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish : MonoBehaviour
{
    public SpriteRenderer spRender;
    public FishDrone drone = null;
    public Sprite[] spFish;
    public bool fascinated = false;

    private Vector3 offset = Vector3.zero;
    private Vector3 oldPos = Vector3.zero;

    private Vector3 waitPos = Vector3.zero;
    private Vector3 floatingPos = Vector3.zero;

    private bool filpFloating = false;

    private float followSpeed = 5f;
    private float floatingSpeed = 5f;

    public FishType type = FishType.A;
    public void Init(FishDrone drone, FishType type)
    {
        this.drone = drone;
        this.type = type;

        fascinated = false;
        transform.rotation = Quaternion.identity;
        transform.position = new Vector3(Random.Range(-17f, 17f), Random.Range(-4, 10f), 0);


        oldPos = transform.position;
        waitPos = transform.position;

        SetFloatingPos();

        followSpeed = Random.Range(2f, 4f);
        floatingSpeed = Random.Range(0.3f, 1.5f);

        spRender.sprite = spFish[(int)this.type];
    }

    // Update is called once per frame
    void Update()
    {

        if (fascinated)
        {
            transform.position = Vector3.Lerp(transform.position, drone.transform.position + offset, Time.deltaTime * followSpeed);

            if (drone.state == FishDrone.State.Cook)
            {
                spRender.flipX = false;
                transform.right = (transform.position - oldPos).normalized;
            }
            else if (drone.state == FishDrone.State.CookEnd)
            {
                gameObject.SetActive(false);
            }
            else
            {
                if (transform.position.x > oldPos.x)
                {
                    spRender.flipX = false;
                }
                else if (transform.position.x < oldPos.x)
                {
                    spRender.flipX = true;
                }
            }
        }
        else
        {
            if (filpFloating == false)
            {
                transform.position = Vector3.Lerp(transform.position, floatingPos, Time.deltaTime * floatingSpeed);

                if (Vector3.Distance(transform.position, floatingPos) < 0.25f)
                {
                    filpFloating = !filpFloating;
                }
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, waitPos, Time.deltaTime * floatingSpeed);

                if (Vector3.Distance(transform.position, waitPos) < 0.25f)
                {
                    filpFloating = !filpFloating;
                }
            }

            if (transform.position.x > oldPos.x)
            {
                spRender.flipX = false;
            }
            else if (transform.position.x < oldPos.x)
            {
                spRender.flipX = true;
            }
        }

        

        oldPos = transform.position;
    }

    void SetFloatingPos()
    {
        float offset = 3f;
        float value = 7f;
        floatingPos = waitPos + new Vector3(Random.Range(-value - offset, value + offset), Random.Range(-value - offset, value + offset), 0);
        floatingPos.x = Mathf.Clamp(floatingPos.x, -17f, 17f);
        floatingPos.y = Mathf.Clamp(floatingPos.y, -4, 10f);


        filpFloating = false;
    }

    public void Fascinate()
    {
        float value = 2f;
        offset.x = Random.Range(-value, value);
        offset.y = Random.Range(-value, value);

        fascinated = true;
    }
}
