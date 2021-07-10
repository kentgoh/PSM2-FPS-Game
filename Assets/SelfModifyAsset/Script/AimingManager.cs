using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimingManager : MonoBehaviour
{
    private GameObject scope;
    private GameObject aimingTarget;
    private GameObject cameraInput;
    public GameObject gameManager;
    private Vector3 scopeOriPos;
    private Vector3 targetPos;
    public int targetCount;
    public bool aoeStrike = false;
    public bool shieldActivate = false;
    private ShieldManager shieldScript;

    private int fireCooldown = 2;
    Coroutine shieldTimer;
    Coroutine fireTimer;

    void Start()
    {
        scope = this.transform.Find("Scope").gameObject;
        scopeOriPos = scope.transform.position;

        cameraInput = GameObject.FindGameObjectWithTag("WebcamInput");
        shieldScript = gameObject.GetComponent<ShieldManager>();
    }

    void Update()
    {
        if (shieldScript.shieldCooldown == 0)
        {
            StopCoroutine(shieldTimer);
            shieldScript.shieldCooldown = 5;
        }

        if(fireCooldown == 0)
        {
            StopCoroutine(fireTimer);
            fireCooldown = 2;
        }

        //Change target according to the user input
        if (aimingTarget != null && scope.transform.position == (aimingTarget.transform.position - new Vector3(0,0,0.5f)))
        {
            ImageDetectionControl();
            KeyboardControl();
            //MouseControl();
        }

        //Target destroyed, move to the closest enemy
        else
        {
            targetPos = FindClosestEnemy(scope.transform.position, 3);
            if (targetPos != new Vector3(0, 0, 0))
            {
                scope.transform.position = targetPos;
            }
            else if (targetCount != 0)
            {
                scope.transform.position = scopeOriPos;
            }
        }
        
    }

    void KeyboardControl()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            shieldScript.shieldTrigger = true;
            StartCoroutine(shieldScript.shieldGenerate());
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (aoeStrike)
            {
                GameObject[] allTargetList = GameObject.FindGameObjectsWithTag("Target");
                foreach (GameObject target in allTargetList)
                {
                    Drone droneScript = target.GetComponent<Drone>();
                    droneScript.DroneDamage();
                    if (droneScript.DroneHealth == 0)
                        Destroy(target, 0.8f);
                }
                aoeStrike = false;
            }

            else
            {
                //fireTimer = StartCoroutine(FireCooldownTimer());
                Drone droneScript = aimingTarget.GetComponent<Drone>();
                droneScript.DroneDamage();
                if (droneScript.DroneHealth == 0)
                    Destroy(aimingTarget, 0.8f);
            }

        }
    //}
        
        //Check closest enemy according to the keyboard input
        //Left = 1
        //Right = 2
        if (Input.GetKeyDown(KeyCode.A))
        {
            targetPos = FindClosestEnemy(scope.transform.position, 1);
            scope.transform.position = targetPos;

        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            targetPos = FindClosestEnemy(scope.transform.position, 2);
            scope.transform.position = targetPos;
        }

    }

    void MouseControl()
    {
        FPSGameManager gameScript = gameManager.GetComponent<FPSGameManager>();

        if (Input.GetMouseButtonDown(0) && !gameScript.GameIsPaused)
        {
            Drone droneScript = aimingTarget.GetComponent<Drone>();
            droneScript.DroneDamage();
            if(droneScript.DroneHealth == 0)
                 Destroy(aimingTarget,0.8f);

        }
    }

    void ImageDetectionControl()
    {
        ImageDetection imageDetectionScript = cameraInput.GetComponent<ImageDetection>();
        FPSGameManager gameScript = gameManager.GetComponent<FPSGameManager>();

        if (!gameScript.GameIsPaused)
        {
            //Shield Activation
            if (imageDetectionScript.shieldGesture && shieldScript.shieldCooldown == 5)
            {
                if (!shieldActivate)
                {
                    shieldScript.shieldTrigger = true;
                    StartCoroutine(shieldScript.shieldGenerate());
                    shieldTimer = StartCoroutine(shieldScript.ShieldCooldownTimer());
                    shieldActivate = true;     
                }
                else if (shieldScript.shieldComplete)
                {
                    shieldActivate = false;
                    shieldScript.shieldComplete = false;
                    imageDetectionScript.resetAllFingerTrigger();
                }
            }
            else
            {
                //Fire Activation
                if (imageDetectionScript.fireGesture)
                {
                    if(fireCooldown == 2)
                    {
                        fireTimer = StartCoroutine(FireCooldownTimer());
                        Drone droneScript = aimingTarget.GetComponent<Drone>();
                        droneScript.DroneDamage();
                        if (droneScript.DroneHealth == 0)
                            Destroy(aimingTarget, 0.8f);
                    }

                    imageDetectionScript.resetAllFingerTrigger();
                }
                else
                {
                    imageDetectionScript.fireGesture = false;
                }

                if (imageDetectionScript.moveLeftGesture)
                {
                    Debug.Log("Left");
                    targetPos = FindClosestEnemy(scope.transform.position, 1);
                    scope.transform.position = targetPos;
                }
                else if (imageDetectionScript.moveRightGesture)
                {
                    Debug.Log("Right");
                    targetPos = FindClosestEnemy(scope.transform.position, 2);
                    scope.transform.position = targetPos;
                }
                else
                {
                }
            }

        }

    }

    //Move aiming scope to the closest enemy
    public Vector3 FindClosestEnemy(Vector3 currentAimingPosition, int command)
    {
        GameObject[] allTargetList;
        Vector3 closestPos = new Vector3(0,0,0);
        Vector3 furthestPos = new Vector3(0,0,0);
        float distance = Mathf.Infinity;
        float furthestDistance = 0;

        allTargetList = GameObject.FindGameObjectsWithTag("Target");
        GameObject[] currentTargetList = new GameObject[allTargetList.Length];
        FPSGameManager gameScript = gameManager.GetComponent<FPSGameManager>();

        if (gameScript.currentArea == 1)
            checkArea(1, allTargetList, currentTargetList);
        else if(gameScript.currentArea == 2)
            checkArea(2, allTargetList, currentTargetList);
        else if (gameScript.currentArea == 3)
            checkArea(3, allTargetList, currentTargetList);

        foreach (GameObject target in currentTargetList)
        {
            if (target != null)
            {
                //Get target position and make the z distance to be constant 2.0f
                Vector3 targetPos = target.transform.position;
                targetPos.z -= 0.5f;

                //Find differences between current aiming position and the target
                float Xdiff = Mathf.Abs(currentAimingPosition.x - targetPos.x);


                //Left Arrow clicked
                if (command == 1)
                {
                    //Target is the closest and on the left of current aiming position
                    if ((targetPos.x < currentAimingPosition.x) && (Xdiff < distance) && (distance != 0))
                    {
                        closestPos = targetPos;
                        aimingTarget = target;
                        distance = Xdiff;
                    }
                    else if (targetPos.x > currentAimingPosition.x)
                    {
                        if (Xdiff > furthestDistance)
                        {
                            furthestDistance = Xdiff;
                            furthestPos = targetPos;
                        }
                    }
                }
                //Right Arrow clicked
                if (command == 2)
                {
                    //Target is the closest and on the right of current aiming position
                    if ((targetPos.x > currentAimingPosition.x) && (Xdiff < distance) && (distance != 0))
                    {
                        closestPos = targetPos;
                        aimingTarget = target;
                        distance = Xdiff;
                    }
                    else if (targetPos.x < currentAimingPosition.x)
                    {
                        if (Xdiff > furthestDistance)
                        {
                            furthestDistance = Xdiff;
                            furthestPos = targetPos;
                        }
                    }
                }
                //Previous target destroyed, auto find new one
                else if (command == 3)
                {
                    if (Xdiff < distance)
                    {
                        closestPos = targetPos;
                        aimingTarget = target;
                        distance = Xdiff;
                    }
                }

            }
        }

        if (float.IsInfinity(distance))
            closestPos = furthestPos;


        return closestPos;

    }

    public void checkArea(int area, GameObject[] alltargetList, GameObject[] list)
    {
        string currentAreaString = "Area1";
        if (area == 2)
            currentAreaString = "Area2";
        else if (area == 3)
            currentAreaString = "Area3";

        int j = 0;
        for (int i = 0; i < alltargetList.Length; i++)
        {
            if (string.Equals(alltargetList[i].transform.parent.parent.name, currentAreaString))
            {
                list[j] = alltargetList[i];
                j++;
            }
            
        }

        targetCount = j;
    }

    public IEnumerator CameraShake(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;

        float elapsed = 0.0f;

        while(elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(x,y,originalPos.z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = originalPos;

    }

    public IEnumerator FireCooldownTimer()
    {
        while (true)
        {
            fireCooldown--;

            yield return new WaitForSeconds(1);
        }

    }

}
