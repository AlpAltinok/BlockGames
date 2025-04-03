using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundTile : MonoBehaviour
{
    public int hitPoints;
    private SpriteRenderer sprite;
    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();

    }

    private void Update()
    {
        if (hitPoints <= 0)
        {
            Destroy(this.gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        hitPoints -= damage;
        MakeLighter();
    }
    void MakeLighter()
    {
        Color color=sprite.color;
        float newalpha = color.a * .5f;
       sprite.color =new Color(color.r,color.g,color.b,newalpha);    
    }
    //public GameObject[] dots;
    //private void Start()
    //{
    //    Initialize();
    //}
    //void Initialize()
    //{
    //    int dotToUse = Random.Range(0, dots.Length);
    //    GameObject dot = Instantiate(dots[dotToUse], transform.position, Quaternion.identity);
    //    dot.transform.parent = this.transform;
    //    dot.name = this.gameObject.name;
    //}


}
