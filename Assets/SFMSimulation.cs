using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFMSimulation : MonoBehaviour
{
    public Transform SFCharacterParent;
    public Transform WallParent;
    
    public Wall[] walls;
    public SFCharacter[] agents;
    
    // Start is called before the first frame update
    void Start()
    {
        // 壁の位置をすべて取得
       walls=WallParent.GetComponentsInChildren<Wall>();
       //SFCharcterの位置をすべて取得
       agents=SFCharacterParent.GetComponentsInChildren<SFCharacter>(); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
