using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using HoloToolkit.Unity.SpatialMapping;

public class HoloDog : MonoBehaviour
{
    static float lastXPos;
    static float lastYPos;
    static float lastZPos;
    static Animator anim;
    public int cntWalkingAway = 0;              //distance for walking away
    //private int cntComeOn=0;
    //private int cnt2;
    //private int cnt3;
    //private int cnt4;
    AudioSource[] audio;
    Renderer renderer;
    private bool stop = false;             //when stop == true -> dog stops
    private bool walkAway = false;           //for checking if the dog has to walk away
    private bool comeOn = false;             //if its true the dog is allowed to come near the person
    private bool cameOn = false;
    private bool camIsNear = false;
    private bool isNotStooping = false;
    private bool personWasStooping = false;
    private float probability = 0f;        //to calculate the probability if the dog comes or not
    private float probWalkAway = 0f;
    private double actspeed = -1f;
    private int growlingCnt = 0;
    private bool collision = false;
    void Start()
    {
        // save the last pos of the dog
        lastXPos = Camera.main.transform.position.x;
        lastYPos = Camera.main.transform.position.y;
        lastZPos = Camera.main.transform.position.z;

        anim = GetComponent<Animator>();
        audio = GetComponents<AudioSource>();
        renderer = new Renderer();
        //InvokeRepeating("DeleteProbability", 4, 10);
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("probabbility"+probability);
        //starts that the dog does on the ground and dosen't stand in the air (why in update?, because there is no function where you get iformend when the spatial processing is finished.)
        StartPhysics();
        //checks if the person Comes near without stooping  -> next time with specific speed
        CheckIfPersonIsNear();
        if (PersIsStooping() && !personWasStooping)
        {
            probability += UnityEngine.Random.Range(15, 32);
            personWasStooping = true;
        }
        //checks if the probability is high enough that the dog can come on
        checkProbability();
        CheckIfPersonWalkedAway();
        //if the probability to come on is high enough and the dog dosen't run againts anithing he can come on
        if (comeOn && !stop)
        {
            //Debug.Log("Come On Called");
            ComeToPerson();
        }
        //if the person comes without stooping // to much speed the dog runs away 30px
        else if (!comeOn && !cameOn && walkAway && cntWalkingAway <= 30)
        {
            cntWalkingAway++;
            WalkAway();
        }
        else if (cntWalkingAway == 30)
        {
            personWasStooping = false;
            actspeed = -1;
        }
        else if (!cameOn && camIsNear && growlingCnt <= 1)
        {
            PersonComesWithoutStooping();
        }
        else if (!camIsNear)
        {
            growlingCnt = 0;
        }
        //else he have to stay
        else
        {
            anim.SetBool("isWalking", false);
            stop = true;
            walkAway = false;
            comeOn = false;
            cntWalkingAway = 0;
        }
    }
    /// <summary>
    /// To start the Physics
    /// </summary>
    public void StartPhysics()
    {
        if (SpatialMappingManager.Instance != null)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(transform.position, transform.up * -1, out hitInfo, 20f, SpatialMappingManager.Instance.LayerMask))
                transform.Translate(0, -hitInfo.distance, 0);
        }
    }

    /// <summary>
    /// checkes if the person comes near and 
    /// calls function PersonComesWithoutStooping and trought this makes a noise or run away when a person is not stooping
    /// </summary>
    public void CheckIfPersonIsNear()
    {
        //Debug.Log("Position Camera: " + Camera.main.transform.position);
        //Debug.Log("Position Hund: " + transform.position);
        if (Camera.main.transform.position.x <= transform.position.x + 1 && Camera.main.transform.position.x >= transform.position.x - 1 && Camera.main.transform.position.z <= transform.position.z + 1 && Camera.main.transform.position.z >= transform.position.z - 1)
        {
            //Debug.Log("got it");
            camIsNear = true;
        }
        else
        {
            camIsNear = false;
        }
    }

    /// <summary>
    ///check if the person comes near the dog
    /// </summary>
    /// <returns></returns>
    public bool CameraComesNear()
    {
        if (this.transform.position.x > 0 && Camera.main.transform.position.x > lastXPos)
        {
            return true;
        }
        else if (this.transform.position.x < 0 && Camera.main.transform.position.x < lastXPos)
        {
            return true;
        }
        else if (this.transform.position.z > 0 && Camera.main.transform.position.z > lastZPos)
        {
            return true;
        }
        else if (this.transform.position.z < 0 && Camera.main.transform.position.z < lastZPos)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// makes a noise or run away when a person is not stooping
    /// </summary>
    public void PersonComesWithoutStooping()
    {
        int num = UnityEngine.Random.Range(0, 2);
        if (!comeOn || !cameOn && num ==0)
        {
            GoAway();
        }
        if(num==1)
        {
            audio[0].Play();
            growlingCnt++;
        }
    }
    public bool PersIsStooping()
    {
        if (Camera.main.transform.position.y <= -0.5)
        {
            //Debug.Log("true true true true true true");
            return true;
        }
        else
        {
            //Debug.Log("false false false false false false");
            return false;
        }
    }
    public void GoAway()
    {
        if (!cameOn&&!comeOn)
        {
            stop = false;
            walkAway = true;
        }
        else
        {
            walkAway = false;
            stop = false;
            comeOn = false;
            cameOn = false;
        }
    }

    /// <summary>
    /// Checkes if the probability is high enough that the dog comes on or walkes away etc...
    /// </summary>
    private void checkProbability()
    {
        CheckActSpeed();

        if (probability >= UnityEngine.Random.Range(20, 40) && !comeOn)
        {
            walkAway = false;
            comeOn = true;
            stop = false;
            personWasStooping = false;
        }
    }

    /// <summary>
    /// Calculates the actual speed
    /// </summary>
    /// <returns></returns>
    public void CheckActSpeed()
    {
        float xPose = Camera.main.transform.position.x;
        float zPose = Camera.main.transform.position.z;
        StartCoroutine(Wait(1));
        float xPose2 = Camera.main.transform.position.x;
        float zPose2 = Camera.main.transform.position.z;

        double xWay = Math.Abs(xPose - xPose2);
        double zWay = Math.Abs(zPose - zPose2);

        double way = Math.Pow(xWay, 2) + Math.Pow(zWay, 2); ;
        double s = 0;
        s = Math.Sqrt(way);
        double v = s / 1;
        if (v != actspeed && v <= 6)
        {
            actspeed = v;
            probability += UnityEngine.Random.Range(6, 15);
        }
        if (v >= 6 && v != actspeed)
        {
            actspeed = v;
            probWalkAway = UnityEngine.Random.RandomRange(80, 120);
        }
        //else if (probability >= 20)
        //    probability -= 10;
    }

    /// <summary>
    /// Checkes if the person is stooping or not
    /// </summary>
    private void CheckIfStooping()
    {
        if (PersIsStooping())
        {
            //Debug.Log("Person is stooping");
            isNotStooping = false;
            probability += UnityEngine.Random.Range(10, 31);
            walkAway = false;
        }
        else
        {
            isNotStooping = true;
        }
    }

    /// <summary>
    /// Makes the element to walk to the person
    /// </summary>
    public void ComeToPerson()
    {
        CheckElementIsNearYou();
        anim.SetBool("isSitting", false);
        Vector3 mainCam = new Vector3(Camera.main.transform.position.x, transform.position.y, Camera.main.transform.position.z);
        this.transform.LookAt(mainCam);

        if (!stop && !cameOn)
        {
            anim.SetBool("isWalking", true);
            GoForward();
        }
        else
        {
            stop = true;
            Vector3 vect = new Vector3(UnityEngine.Random.Range(-100, 100), -1.6f, UnityEngine.Random.Range(-100, 100));
            transform.LookAt(vect);
            StartCoroutine("SetCameOnFalse");
        }
    }

    public IEnumerable SetCameOnFalse()
    {
        yield return new WaitForSeconds(UnityEngine.Random.RandomRange(4,10));
        cameOn = false;
        comeOn = false;
    }

    /// <summary>
    /// To get the Dog to make a dog Forward
    /// </summary>
    public void GoForward()
    {
        float translation;
        anim.SetBool("isSitting", false);
        CheckForward();               //wichtig zwecks collision
        if (!stop)
        {
            translation = 1f;
            anim.SetBool("isWalking", true);
            translation *= Time.deltaTime;
            transform.Translate(0, 0, translation);
        }
        else
        {
            anim.SetBool("isWalking", false);
        }
    }
    /// <summary>
    /// Checkes the enviroment in front of the dog
    /// </summary>
    public void CheckForward()
    {
        Debug.Log(collision);
        if(!collision)
        Debug.Log("Check Forward");
            if (SpatialMappingManager.Instance != null)
            {
                Ray ray = new Ray(new Vector3(transform.position.x, transform.position.y + 0.2f, transform.position.z), Vector3.forward);
                if (Physics.Raycast(ray, 0.5f))
                {
                    collision = true;
                    Debug.Log("Stopped Walking");
                    anim.SetBool("isWalking", false);
                    stop = true;
                    //turn around 180 degrece
                    transform.localRotation = this.transform.rotation * Quaternion.Euler(0, 180, 0);
                    transform.eulerAngles = new Vector3(0, transform.rotation.y, 0);
                    comeOn = false;
                    probability = 0;
                    //cameOn = true;
                    walkAway = false;
                    Vector3 vect = new Vector3(UnityEngine.Random.Range(0, 100), -1.6f, UnityEngine.Random.Range(0, 100));
                    transform.LookAt(vect);
                }
            }
        
    }

    /// <summary>
    /// Makes the Elemant to go a step away
    /// </summary>
    public void WalkAway()
    {
        if(cntWalkingAway < 1)
            transform.localRotation = Camera.main.transform.rotation * Quaternion.Euler(0, 180, 0);
        transform.eulerAngles = new Vector3(0, transform.rotation.y, 0);
        stop = false;
        walkAway = true;
        cameOn = false;
        comeOn = false;
        GoForward();
    }

    /// <summary>
    /// Makes the probability higeher that the element comes near
    /// </summary>
    public void ComeOn()
    {
        //Debug.Log("said ComeOn");
        if (PersIsStooping())
            probability += 100;
        else
            probability += UnityEngine.Random.Range(6, 35);
        stop = false;
    }

    /// <summary>
    /// check if the element is near the Person
    /// </summary>
    public void CheckElementIsNearYou()
    {
        if (Camera.main.transform.position.x + 1.5 >= transform.position.x && Camera.main.transform.position.x - 1.5 <= transform.position.x && Camera.main.transform.position.z + 1.5 >= transform.position.z && Camera.main.transform.position.z - 1.5 <= transform.position.z)
        {
            stop = true;
            anim.SetBool("isWalking", false);
            if (comeOn)
                cameOn = true;
            comeOn = false;
            personWasStooping = false;
            actspeed = -1;
            probability = 0;
        }
        else
        {
            stop = false;
            comeOn = true;
            cameOn = false;
        }
    }

    /// <summary>
    /// Makes the element say hello
    /// </summary>
    public void SayHello()
    {
        //Debug.Log("Hello I am a Hologram");
        audio[1].Play();
    }

    /// <summary>
    /// Makes the element to sit down
    /// </summary>
    public void SitDown()
    {
        Debug.Log("SitDown");
        anim.SetBool("isSitting", true);
        anim.SetBool("isWalking", false);
        stop = true;
        StartCoroutine("StandUp");
    }

    public IEnumerator StandUp()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(2, 8));
        anim.SetBool("isSitting", false);
    }

    public IEnumerator Wait(float duration)
    {
        yield return new WaitForSeconds(duration);   //Wait
    }

    public void DeleteProbability()
    {
        probability = 0;
    }


    public void CheckIfPersonWalkedAway()
    {
        if (cameOn && !camIsNear)
        {
            cameOn = false;
            probability = 0;
        }
    }
}
