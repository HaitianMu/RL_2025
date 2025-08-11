using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class RobotControl : MonoBehaviour
{
    public string robotCommand;
    public List<HumanControl> myDirectFollowers;
    public int robotFollowerCounter;
    public RobotBrain myAgent;
    // bot��NavMeshAgent���
    private NavMeshAgent _botNavMeshAgent;
    public bool isRunning;//�������Ƿ��ڹ���״̬
    // Start is called before the first frame update
    public void Start()
    {
        this.gameObject.SetActive(true);
        isRunning = true;//������Ĭ�Ϲ���
        myDirectFollowers = new List<HumanControl>();
        _botNavMeshAgent = GetComponent<NavMeshAgent>();
    }

    public void Update()
    {
        robotFollowerCounter = myDirectFollowers.Count;

    }

    private void OnTriggerEnter(Collider trigger)
    {
        // Debug.Log("��ײ��������ײ��ı�ǩ�ǣ�" + trigger.transform.tag);
        GameObject triggerObject = trigger.gameObject;
      
        switch (trigger.transform.tag)
        {
            case "Fire":
                print("��������������");
                myAgent.stuckCounter++;
                myAgent.AddReward(-10);//��һ�θ�ʮ��ͷ�
                myAgent.LogReward("�����˴�������ͷ�",-10);
                break;
        }
    }


}
