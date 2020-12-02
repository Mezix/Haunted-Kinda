using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterParticle : MonoBehaviour
{
    private Vector3 startingVector;
    public BoxCollider2D waterCollider;
    public GameObject[] Flowers;
    private List<Flower> FlowersWaterHasPassedThrough = new List<Flower>();
    private void Awake()
    {
        startingVector = transform.position;
        Color spriteColor = GetComponent<SpriteRenderer>().color;
        spriteColor.a = Random.Range(0.4f,0.8f);
        spriteColor.b = Random.Range(0.7f, 1);
        GetComponent<SpriteRenderer>().color = spriteColor;
    }
    void FixedUpdate()
    {
        WaterBehaviour();
    }
    private void WaterBehaviour()
    {
        if(Vector3.Distance(startingVector, transform.position) < 0.5f)
        {
            transform.position -= new Vector3(0, 0.03f, 0);
        }
        else
        {
            Explode();
        }
    }
    private void Explode()
    {
        //Spawn Particle for water hitting floor
        if(!waterCollider.IsTouching(LevelSceneManager.level._nonCollidableTiles))
        {
            if(FlowersWaterHasPassedThrough.Count == 0)
            {
                Instantiate(Flowers[Random.Range(0, Flowers.Length)], transform.position, transform.rotation);
            }
            else
            {
                foreach(Flower flower in FlowersWaterHasPassedThrough)
                {
                    flower.Grow(0.05f);
                }
            }
        }
        Destroy(gameObject);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out Flower flower))
        {
            FlowersWaterHasPassedThrough.Add(flower);
            //print("found flower");
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Flower flower))
        {
            FlowersWaterHasPassedThrough.Remove(flower);
            //print("bye flower");
        }
    }
}
