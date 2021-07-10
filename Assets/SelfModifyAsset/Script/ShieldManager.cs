using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShieldManager : MonoBehaviour
{
    public GameObject shield;
    public bool shieldTrigger = false;
    public bool shieldComplete = false;
    public int blockCount = 0;
    public int count = 2;
    public int shieldCooldown = 5;

    void Update()
    {
        if(blockCount == 3)
        {
            gameObject.GetComponent<AimingManager>().aoeStrike = true;
            blockCount = 0;
        }

    }

    public IEnumerator shieldGenerate()
    {
        while (shieldTrigger)
        {
            if (count != 0)
            {
                shield.SetActive(true);
                count--;
            }
            else
            {
                shield.SetActive(false);
                shieldTrigger = false;
                shieldComplete = true;
                count = 2;
            }

            yield return new WaitForSeconds(1);
        }

    }

    public IEnumerator ShieldCooldownTimer()
    {
        while (true)
        {
            shieldCooldown--;

            yield return new WaitForSeconds(1);
        }

    }

}
