using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;
    private Vector2[] length;
    private Vector3[] startPos;
    [SerializeField] private Transform[] transforms;
    [SerializeField] private float[] parallaxAmountX;
    [SerializeField] private float[] parallaxAmountY;

    void Start()
    {
        length = new Vector2[transforms.Length];
        startPos = new Vector3[transforms.Length];
        for (int i = 0; i < transforms.Length; i++)
        {
            length[i] = transforms[i].GetComponent<SpriteRenderer>().bounds.size;
            startPos[i] = transforms[i].position;
        }
    }

    
    void Update()
    {
        for (int i = 0; i < transforms.Length; i++)
        {
            Vector3 relativePos = new Vector3(targetTransform.position.x * parallaxAmountX[i] , targetTransform.position.y * parallaxAmountY[i] , 0);
            transforms[i].position = startPos[i] + relativePos;
            Vector3 dist = targetTransform.position - relativePos; 

            if(dist.x < startPos[i].x - length[i].x)  startPos[i].x-=length[i].x;
            if(dist.x > startPos[i].x + length[i].x)  startPos[i].x += length[i].x;
        }
    }
}
