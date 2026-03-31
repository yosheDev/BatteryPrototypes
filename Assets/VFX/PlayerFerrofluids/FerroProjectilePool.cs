using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FerroProjectilePool : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    private List<GameObject> ferroProjectiles = new List<GameObject>();
    
    public void ShootProjectile(Vector3 pos, Quaternion rot, Vector3 scale, float speed = 1f)
    {
        GameObject projectile;
        if (IsProjectileAvaliable())
        {
            projectile = GetFirstAvaliableProjectile();
        }
        else
        {
            projectile = Instantiate(projectilePrefab, pos, rot);
            projectile.transform.localScale = scale;
        }

        projectile.GetComponent<FerroProjectile>().Shoot(pos, rot, scale, speed);
    }


    private bool IsProjectileAvaliable()
    {
        foreach(GameObject p in ferroProjectiles)
        {
            if (!p.GetComponent<FerroProjectile>().IsInPlay())
            {
                return true;
            }
        }

        return false;
    }

    private GameObject GetFirstAvaliableProjectile()
    {
        for (int i = 0; i < ferroProjectiles.Count; i++)
        {
            if (!ferroProjectiles[i].GetComponent<FerroProjectile>().IsInPlay())
            {
                return ferroProjectiles[i];
            }
        }

        return null;
    }
    private int AvaliableProjectileCount()
    {
        int count = 0;
        foreach (GameObject p in ferroProjectiles)
        {
            if (!p.GetComponent<FerroProjectile>().IsInPlay())
            {
                count++;
            }
        }
        return count;
    }
}
