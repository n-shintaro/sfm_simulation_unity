using System;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;


public class VrPosition : MonoBehaviour
{
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

    //1フレーム毎に呼び出されるUpdateメゾット
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
        Debug.Log("HMDP:" + HMDPosition.x + ", " + HMDPosition.y + ", " + HMDPosition.z + "\n" +
                    "HMDR:" + HMDRotation.x + ", " + HMDRotation.y + ", " + HMDRotation.z);
        Debug.Log("LFHP:" + LeftHandPosition.x + ", " + LeftHandPosition.y + ", " + LeftHandPosition.z + "\n" +
                    "LFHR:" + LeftHandRotation.x + ", " + LeftHandRotation.y + ", " + LeftHandRotation.z);
        Debug.Log("RGHP:" + RightHandPosition.x + ", " + RightHandPosition.y + ", " + RightHandPosition.z + "\n" +
                    "RGHR:" + RightHandRotation.x + ", " + RightHandRotation.y + ", " + RightHandRotation.z);
    }
}