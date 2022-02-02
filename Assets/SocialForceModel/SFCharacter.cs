using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR;
using UnityStandardAssets.Characters.ThirdPerson;
using Valve.VR;
using System; //JsonUtilityを扱うために必要
using System.IO; //ファイル読み書き用


public class SFCharacter : MonoBehaviour
{
    Vector3 velocity = new Vector3();
    public Vector3 Velocity
    {
        get { return velocity; }
    }

    AICharacterControl characterControl;
    NavMeshAgent characterAgent;
    float radius;

    Transform SFCharacterParent;
    Transform WallParent;
    
    Wall[] walls;
    SFCharacter[] agents;

    Vector3 destination;

    public float destination_x=-3.0f;
    public float destination_y=0.0f;
    public float destination_z=10.0f;
    
    public float desiredSpeed = 0.5f;
    public float alpha= 1;
    public float beta= 1;
    
    //VR agentの位置を取得
    //HMDの位置座標格納用
    private Vector3 HMDPosition;
    //HMDの回転座標格納用（クォータニオン）
    private Quaternion HMDRotationQ;
    //HMDの回転座標格納用（オイラー角）
    private Vector3 HMDRotation;

    //左コントローラの位置座標格納用
    private Vector3 LeftHandPosition;
    //左コントローラの回転座標格納用（クォータニオン）
    private Quaternion LeftHandRotationQ;
    //左コントローラの回転座標格納用
    private Vector3 LeftHandRotation;

    //右コントローラの位置座標格納用
    private Vector3 RightHandPosition;
    //右コントローラの回転座標格納用（クォータニオン）
    private Quaternion RightHandRotationQ;
    //右コントローラの回転座標格納用
    private Vector3 RightHandRotation;
    
    string filePath;
    
    // Start is called before the first frame update

    void Start()
    {
        characterControl = this.GetComponent<AICharacterControl>();
        characterAgent = this.GetComponent<NavMeshAgent>();
        
        //jsonに保存
        filePath = Application.dataPath + "/SocialForceModel"+ "/" + "sfm_data_save.json";
        string sfm_json=JsonUtility.ToJson(this, prettyPrint:true);
        StreamWriter streamWriter = new StreamWriter(filePath);
        streamWriter.Write(sfm_json);
        streamWriter.Flush();
        streamWriter.Close();
        
        Vector3 destination_tmp=new Vector3(destination_x,destination_y,destination_z);
        destination=destination_tmp;
        UnityEngine.Debug.Log("destination.transform.position:");

        if (this.destination == null) this.destination = this.transform.position;
        else 
            if (characterControl)
                characterControl.target.localPosition = velocity;
        if (characterAgent)
            characterAgent.speed = desiredSpeed;
        radius = 0.5f;

        GameObject simulation_manager=GameObject.Find("SimulationManager");
        
        SFMSimulation[] SFMSimulation=simulation_manager.GetComponents<SFMSimulation>();
        foreach(var sfm_simulation in SFMSimulation){
            walls=sfm_simulation.walls;
            agents=sfm_simulation.agents;
        }
        
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 acceleration;
        Vector3 vr_agent_interaction;

        acceleration = this.alpha*DrivingForce() + this.beta*AgentInteractForce() + WallInteractForce();//+VrAgentInteractForce();
        vr_agent_interaction=VrAgentInteractForce();
        // Debug.Log("HMDPosition:" + HMDPosition.x+ ", " + HMDPosition.y + ", " + HMDPosition.z );
        // Debug.Log("VrAgentInteractForce:", vr_agent_interaction);
        velocity += acceleration * Time.deltaTime * 3;


        // Limit maximum velocity
        if (Vector3.SqrMagnitude(velocity) > desiredSpeed * desiredSpeed)
        {
            velocity.Normalize();
            velocity *= desiredSpeed;
        }

        // Update current attributes to AICharacterControl
        if (characterControl)
            characterControl.target.transform.position = this.transform.position + velocity.normalized;
        else
           this.transform.position += velocity * Time.deltaTime * 5;

        if (characterAgent)
            characterAgent.speed = desiredSpeed;
    }

    Vector3 DrivingForce()
    {
        const float relaxationT = 0.54f;
        Vector3 desiredDirection = destination - this.transform.position;
        desiredDirection.Normalize();

        Vector3 drivingForce = (desiredSpeed * desiredDirection - velocity) / relaxationT;

        return drivingForce;
    }

    Vector3 AgentInteractForce()
    {
        const float lambda = 2.0f;
        const float gamma = 0.35f;
        const float nPrime = 3.0f;
        const float n = 2.0f;
        //const float A = 4.5f;
        const float A = 47f;
        float B, theta;
        int K;
        Vector3 interactionForce = new Vector3(0f, 0f, 0f);

        Vector3 vectorToAgent = new Vector3();

        foreach (SFCharacter agent in agents)
        {
            // Skip if agent is self
            if (agent == this) continue;

            vectorToAgent = agent.transform.position - this.transform.position;

            // Skip if agent is too far
            if (Vector3.SqrMagnitude(vectorToAgent) > 10f * 10f) continue;

            Vector3 directionToAgent = vectorToAgent.normalized;
            Vector3 interactionVector = lambda * (this.Velocity - agent.Velocity) + directionToAgent;

            B = gamma * Vector3.Magnitude(interactionVector);

            Vector3 interactionDir = interactionVector.normalized;

            theta = Mathf.Deg2Rad * Vector3.Angle(interactionDir, directionToAgent);

            if (theta == 0) K = 0;
            else if (theta > 0) K = 1;
            else K = -1;

            float distanceToAgent = Vector3.Magnitude(vectorToAgent);

            float deceleration = -A * Mathf.Exp(-distanceToAgent / B
                                                - (nPrime * B * theta) * (nPrime * B * theta));
            float directionalChange = -A * K * Mathf.Exp(-distanceToAgent / B
                                                         - (n * B * theta) * (n * B * theta));
            Vector3 normalInteractionVector = new Vector3(-interactionDir.z, interactionDir.y, interactionDir.x);
            //Vector3 normalInteractionVector = new Vector3(-interactionDir.y, interactionDir.x, 0);

            interactionForce += deceleration * interactionDir + directionalChange * normalInteractionVector; 
        }
        return interactionForce;
    }

    Vector3 VrAgentInteractForce()
    {
        const float lambda = 2.0f;
        const float gamma = 0.35f;
        const float nPrime = 3.0f;
        const float n = 2.0f;
        //const float A = 4.5f;
        const float A = 47f;
        float B, theta;
        int K;
        Vector3 interactionForce = new Vector3(0f, 0f, 0f);

        Vector3 vectorToAgent = new Vector3();
        
        vectorToAgent = HMDPosition - this.transform.position;

        // Skip if agent is too far
        if (Vector3.SqrMagnitude(vectorToAgent) < 10f * 10f){
            Vector3 directionToAgent = vectorToAgent.normalized;
            // 速度を取得できないと仮定
            Vector3 interactionVector = directionToAgent; //lambda * (this.Velocity - agent.Velocity) + directionToAgent;

            B = gamma * Vector3.Magnitude(interactionVector);

            Vector3 interactionDir = interactionVector.normalized;

            theta = Mathf.Deg2Rad * Vector3.Angle(interactionDir, directionToAgent);

            if (theta == 0) K = 0;
            else if (theta > 0) K = 1;
            else K = -1;

            float distanceToAgent = Vector3.Magnitude(vectorToAgent);

            float deceleration = -A * Mathf.Exp(-distanceToAgent / B
                                                - (nPrime * B * theta) * (nPrime * B * theta));
            float directionalChange = -A * K * Mathf.Exp(-distanceToAgent / B
                                                            - (n * B * theta) * (n * B * theta));
            Vector3 normalInteractionVector = new Vector3(-interactionDir.z, interactionDir.y, interactionDir.x);
            //Vector3 normalInteractionVector = new Vector3(-interactionDir.y, interactionDir.x, 0);

            interactionForce += deceleration * interactionDir + directionalChange * normalInteractionVector; 
        }
        return interactionForce;
    }


    Vector3 WallInteractForce()
    {
        const float A = 3f;
        const float B = 0.8f;

        float squaredDist = Mathf.Infinity;
        float minSquaredDist = Mathf.Infinity;
        Vector3 minDistVector = new Vector3();

        // Find distance to nearest obstacles
        foreach (Wall w in walls)
        {
            Vector3 vectorToNearestPoint = this.transform.position - w.GetNearestPoint(this.transform.position);
            squaredDist = Vector3.SqrMagnitude(vectorToNearestPoint);

            if (squaredDist < minSquaredDist)
            {
                minSquaredDist = squaredDist;
                minDistVector = vectorToNearestPoint;
            }
        }

        float distToNearestObs = Mathf.Sqrt(squaredDist) - radius;

        float interactionForce = A * Mathf.Exp(-distToNearestObs / B);

        minDistVector.Normalize();
        minDistVector.y = 0;
        Vector3 obsInteractForce = interactionForce * minDistVector.normalized;

        return obsInteractForce;

    }

    // 1フレーム毎に呼び出されるUpdateメゾット
    void Update()
    {

        /*InputTracking.GetLocalPosition(XRNode.機器名)で機器の位置や向きを呼び出せる*/

        //Head（ヘッドマウンドディスプレイ）の情報を一時保管-----------
        //位置座標を取得
        HMDPosition = InputTracking.GetLocalPosition(XRNode.Head);
        //回転座標をクォータニオンで値を受け取る
        HMDRotationQ = InputTracking.GetLocalRotation(XRNode.Head);
        //取得した値をクォータニオン → オイラー角に変換
        HMDRotation = HMDRotationQ.eulerAngles;
        //--------------------------------------------------------------


        //LeftHand（左コントローラ）の情報を一時保管--------------------
        //位置座標を取得
        LeftHandPosition = InputTracking.GetLocalPosition(XRNode.LeftHand);
        //回転座標をクォータニオンで値を受け取る
        LeftHandRotationQ = InputTracking.GetLocalRotation(XRNode.LeftHand);
        //取得した値をクォータニオン → オイラー角に変換
        LeftHandRotation = LeftHandRotationQ.eulerAngles;
        //--------------------------------------------------------------


        //RightHand（右コントローラ）の情報を一時保管--------------------
        //位置座標を取得
        RightHandPosition = InputTracking.GetLocalPosition(XRNode.RightHand);
        //回転座標をクォータニオンで値を受け取る
        RightHandRotationQ = InputTracking.GetLocalRotation(XRNode.RightHand);
        //取得した値をクォータニオン → オイラー角に変換
        RightHandRotation = RightHandRotationQ.eulerAngles;
        //--------------------------------------------------------------


        //取得したデータを表示（HMDP：HMD位置，HMDR：HMD回転，LFHR：左コン位置，LFHR：左コン回転，RGHP：右コン位置，RGHR：右コン回転）
        // Debug.Log("HMDP:" + HMDPosition.x + ", " + HMDPosition.y + ", " + HMDPosition.z + "\n" +
        //             "HMDR:" + HMDRotation.x + ", " + HMDRotation.y + ", " + HMDRotation.z);
        // Debug.Log("LFHP:" + LeftHandPosition.x + ", " + LeftHandPosition.y + ", " + LeftHandPosition.z + "\n" +
        //             "LFHR:" + LeftHandRotation.x + ", " + LeftHandRotation.y + ", " + LeftHandRotation.z);
        // Debug.Log("RGHP:" + RightHandPosition.x + ", " + RightHandPosition.y + ", " + RightHandPosition.z + "\n" +
        //             "RGHR:" + RightHandRotation.x + ", " + RightHandRotation.y + ", " + RightHandRotation.z);
    }


}
