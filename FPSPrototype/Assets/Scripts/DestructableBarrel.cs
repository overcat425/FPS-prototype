using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructableBarrel : InteractionObject { 
    [Header("Destructable Barrel")]
    [SerializeField]
    private GameObject destructableBarrelPieces;
    
    private bool isDestroyed = false;

    public override void TakeDamage(int damage)
    {
        currentHP -= damage;

        if (currentHP <= 0 && isDestroyed == false)
        {
            isDestroyed = true;
            Instantiate(destructableBarrelPieces, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}
