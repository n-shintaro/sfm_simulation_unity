using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFMSimulation : MonoBehaviour
{
    public Transform SFCharacterParent;
    public Transform WallParent;
    
    public Wall[] walls;
    public SFCharacter[] agents;

    public GameObject SFMCharacter;
    public GameObject EndPoint;
    //敵生成時間間隔
    private float interval;
    //敵生成位置間隔
    private float random_pos_x;
    //経過時間
    private float time = 0f;
    //時間間隔の最小値
    public float minTime = 2f;
    //時間間隔の最大値
    public float maxTime = 5f;
    //位置間隔の最小値
    public float minPos = 0f;
    //位置間隔の最大値
    public float maxPos = 10f;
    // Start is called before the first frame update
    void Start()
    {
        // 壁の位置をすべて取得
       walls=WallParent.GetComponentsInChildren<Wall>();
       //SFCharcterの位置をすべて取得
       agents=SFCharacterParent.GetComponentsInChildren<SFCharacter>();

        //時間間隔を決定する
        interval = GetRandomTime();
       
    }

    // Update is called once per frame
    void Update()
    {
        //時間計測
        time += Time.deltaTime;
 
        //経過時間が生成時間になったとき(生成時間より大きくなったとき)
        if(time > interval)
        {
            //SFCharacterを(生成する)
            GameObject character = Instantiate(SFMCharacter);
            //生成した敵の座標を決定する(現状X=0,Y=1,Z=20の位置に出力)
            random_pos_x=GetRandomPos();
            character.transform.position = new Vector3(random_pos_x,-0.5f,-30f);

            // SFMCharcterの親を設定
            character.transform.parent=SFCharacterParent;
            UnityEngine.AI.NavMeshAgent nav_mesh_agent=character.GetComponent<UnityEngine.AI.NavMeshAgent>();
            nav_mesh_agent.enabled=true;
            //目的地を決定
            SFCharacter sfm_character=character.GetComponent<SFCharacter>();
            sfm_character.destination_x=GetRandomPos();
            sfm_character.destination_y=-0.93f;
            sfm_character.destination_z=80f;

            
            // AI Character Controllerに目的地を与える
            // AICharacterControl ai_character_control=character.GetComponent<AICharacterControl>();
            // Transform destination_transform;
            // destination_transform.position=new Vector3(sfm_character.destination_x,sfm_character.destination_y,sfm_character.destination_z);
            // ai_character_control.SetTarget(destination_transform);
            
            //経過時間を初期化して再度時間計測を始める
            time = 0f;
            //次に発生する時間間隔を決定する
            interval = GetRandomTime();
        }
    }

    //ランダムな時間を生成する関数
    private float GetRandomTime()
    {
        float random_time=Random.Range(minTime, maxTime);

        return random_time;
    }
    //ランダムな位置を生成する関数
    private float GetRandomPos()
    {
        float random_position=Random.Range(minPos, maxPos)-5f;
        return random_position;
    }

}
