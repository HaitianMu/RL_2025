using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public partial class Person : MonoBehaviour
{
    public Env myEnv;
    public Transform targetPosition; // Ŀ��λ��
    public int visionLimit = 1; // ���߼�����
    private NavMeshAgent _myNavMeshAgent; // �����������
    private Queue<GameObject> _doorMemoryQueue;//���ڼ�¼�˿�������
    public GameObject myTargetDoor = null;  // ��ǰ�ƻ�ǰ������                              
    public GameObject lastDoorWentThrough;// ��һ�Ⱦ�������
    public Vector3 myDestination;     // �ƶ���Ŀ�ĵ� 
    public bool isReturnFromLastDoor;  // �Ƿ������ͬ����
    public String myBehaviourMode; //���������Ϊģʽ
    public int myFollowerCounter;  //�����ߵ�����
    public List<GameObject> RbtList;  //���ֵĻ������б�
    public GameObject myLeader;//�쵼��Ŀǰֻ���ǻ����� 1.19

    public void Start()
    {
        myBehaviourMode = "Leader";
        _myNavMeshAgent = GetComponent<NavMeshAgent>();
        myDestination = new Vector3();
        _doorMemoryQueue = new Queue<GameObject>();
        myTargetDoor = null;
        lastDoorWentThrough = null;
    }
    private void FixedUpdate()
    {
        //ÿ���˸տ�ʼ���Ƕ������쵼�ߣ��������ų���Ľ��У�
        //������������ʱ���������и���
        switch (myBehaviourMode)
        {
            case "Follower":
                FollowerUpdate();
                break;
            case "Leader":
                LeaderUpdate();
                break;
        }

        foreach (Vector3 vision in GetVision(360, 5))//�����Ұ��Χ���Ƿ��л�����
        {
            // ������λ�÷�������
            if (Physics.Raycast(transform.position, vision, out RaycastHit hit, visionLimit))//����0.5f��Χ���Ƿ�������
            {
                GameObject hitObject = hit.collider.gameObject;
                // ������߻��е������Ƿ�����ض���ǩ
                if (hitObject.tag == "Robot" && !RbtList.Contains(hitObject))
                {

                }
            }
        }
    }

    private List<Vector3> GetVision(int visionWidth, int visionDiff)//�����������ߣ�������һ����������
    {
        List<Vector3> myVisions = new();

        int visionBias = visionWidth / (2 * visionDiff);
        for (int visionIndex = -visionBias; visionIndex <= visionBias; visionIndex++)
        {
            Vector3 vision = Quaternion.AngleAxis(visionDiff * visionIndex, Vector3.up) * transform.forward;
            // �����ɵ����߷���������ӵ��б���
            myVisions.Add(vision);
            Debug.DrawRay(transform.position, vision * 0.4f, Color.green);
            // �����õĴ��룬�����ڳ����л������߷��򣨿�ע�͵���
        }

        return myVisions;
    }

    //�쵼��ģʽ�������Լ��ƶ�
    private void LeaderUpdate()
    {
        if (myTargetDoor is null)
        {
            // print("��ǰû�мƻ�ǰ�����ţ���ʼɨ�裬Ȼ��ɸѡ");
            (List<GameObject> doorCandidates, List<Vector3> unknownDirections) = GetCandidate(new List<string> { "Door", "Exit" }, 360, visionLimit);
            GameObject exit = FilterTargetDoorCandidates(ref doorCandidates, unknownDirections.Count > 0 ? "Explore" : "Normal");
            if (exit is not null)
            {

                //print("���ֳ��ڣ�ֱ��ѡ�������ΪĿ��");
                myTargetDoor = exit;
                myDestination = GetCrossDoorDestination(exit);
                _myNavMeshAgent.SetDestination(myDestination);
                return;
            }
            else if (doorCandidates.Count <= 0)
            {
                if (unknownDirections.Count <= 0)
                {
                    if (lastDoorWentThrough is not null)
                    {
                        myTargetDoor = lastDoorWentThrough;
                        myDestination = GetCrossDoorDestination(lastDoorWentThrough);
                        _myNavMeshAgent.SetDestination(myDestination);
                    }

                    return;
                }
                else if (unknownDirections.Count > 0)
                {
                    Vector3 exploreDirection = unknownDirections[Random.Range(0, unknownDirections.Count)];
                    _myNavMeshAgent.SetDestination(transform.position + exploreDirection * visionLimit);
                    return;
                }
            }
            else if (doorCandidates.Count > 0)
            {
                //print("��ѡ�����в����ڳ��ڣ����ѡ��һ������Ϊ�ƶ�Ŀ��");

                myTargetDoor = doorCandidates[Random.Range(0, doorCandidates.Count)];//Random.Range(0, doorCandidates.Count)
                //print("ѡ������ǣ�" + myTargetDoor.transform.name);
                myDestination = GetCrossDoorDestination(myTargetDoor);

                if (_myNavMeshAgent.SetDestination(myDestination))
                {
                    // print("Ŀ�ĵ��ǣ�" + myDestination);
                }
                else { print("����Ŀ�ĵ�ʧ��"); };

                return;
            }
        }
        else
        {
            if (myTargetDoor.tag.Contains("Exit"))//���Ŀ�����ǳ��ڵĻ���ֱ�ӹ���
            {
                return;
            }
            else
            {
                // print("��ǰ���ڼƻ�ǰ�����ţ��������Ŷ����ƶ�");
                Vector3 myPosition = transform.position;
                myPosition.y -= 0.5f;
                float distanceRemain = Vector3.Distance(myPosition, myDestination);
                if (distanceRemain > 0.5f)
                {//print($"�����Ŷ��滹��{distanceRemain}�ף�ɨ����·�Ƿ��к��ʵ��쵼��");
                    List<GameObject> leaderCandidates = GetCandidate(new List<string> { "Human", "Robot" }, 360, visionLimit).Item1;
                    if (leaderCandidates.Count > 0)
                    {
                        //print("�����˷���׷��������������߻����ˣ�����׷����ģʽ");

                        myLeader = leaderCandidates[0];

                        if (myLeader.GetComponent<Robot>().isRunning)
                        {//����������ڹ������ͽ��и���
                         //print("�ҵ����ڹ����Ļ����ˣ��ҵ��쵼���ǣ�" + leaderCandidates[0].name);
                            if (!myLeader.GetComponent<Robot>().myDirectFollowers.Contains(gameObject.GetComponent<Person>()))
                            {
                                //print(this.name + "���Լ���������˵ĸ������б�");
                                myLeader.GetComponent<Robot>().myDirectFollowers.Add(gameObject.GetComponent<Person>());//���Լ���������˵ĸ������б�
                            }
                            //print(myLeader.GetComponent<Robot>().myDirectFollowers);
                            SwitchBehaviourMode();
                        }
                        return;
                    }
                }
                else
                {  //print($"�����Ŷ��滹��{distanceRemain}�ף�������Ϊ�Ѿ���Ϊ�Ѿ�����Ŀ�ĵأ���ʼ����ɨ��");
                    myTargetDoor = null;
                    return;
                }
            }
            }
        }
    //������ģʽ
    private void FollowerUpdate()
    {
        //print("�л�ģʽ���ҵ�׷�����ǣ�" + myLeader.name);
        Vector3 leaderPosition = myLeader.transform.position;

        List<GameObject> exitList = GetCandidate(new List<string> { "Exit" }, 360, visionLimit).Item1;
        //�ڸ���Ĺ����У��������м���Ƿ��г��ڣ��еĻ���ֱ���뿪,û�еĻ��ͼ������������
        if (exitList.Count > 0)
        {
            myLeader.GetComponent<Robot>().myDirectFollowers.Remove(gameObject.GetComponent<Person>());
            print(this.name+"���Լ��Ƴ������˵ĸ����б�");
            //set
            GameObject exit = exitList[0];
            SwitchBehaviourMode();
            myTargetDoor = exit;
            myDestination = GetCrossDoorDestination(exit);
            _myNavMeshAgent.SetDestination(myDestination);
            return;
        }
        else //һֱ���棬ֱ����������
        {
            _myNavMeshAgent.SetDestination(leaderPosition);
         }
    }

    private Tuple<List<GameObject>, List<Vector3>> GetCandidate(List<string> targetTags, int visionWidth, int visionDiff)
    {
        // ��ʼ����ѡ�����б��δ֪�����б�
        List<GameObject> candidateList = new();
        List<Vector3> unknownDirections = new();
        Vector3 myPosition = transform.position;
        // ��ȡ��ǰ�����λ��
        String layer="Door";//���ݱ�ǩ����ȡ���߼��Ĳ��,Ĭ��ɨ����
        if (targetTags.Contains("Door")||targetTags.Contains("Exit"))
        {
            layer = "Default";
        }
        else if (targetTags.Contains("Robot"))
        {
            layer = "Robot";
        }

        foreach (Vector3 vision in GetVision(visionWidth, visionDiff))
        {
            // �ӵ�ǰλ�������߷���������
            if (Physics.Raycast(myPosition, vision, out RaycastHit hit, visionLimit,LayerMask.GetMask(layer)))
            {
                // ������߻��еĶ���ı�ǩ��Ŀ���ǩ�б��У����Ҹö����ں�ѡ�б��У�����ӵ���ѡ�б�
                if (targetTags.Contains(hit.transform.tag) && !candidateList.Contains(hit.transform.gameObject))
                    //print("ɨ�赽�����У�"+hit.transform.gameObject.name);
                    candidateList.Add(hit.transform.gameObject);
            }
            else
            {
                // �������û�л����κζ����򽫸÷�����ӵ�δ֪�����б�
                unknownDirections.Add(vision);
               // print("ɨ�赽��δ֪�����У�" + vision);
            }
        }

        //RbtList = candidateList;
        // ���غ�ѡ�����б��δ֪�����б�
        return Tuple.Create(candidateList, unknownDirections);
    }
    private GameObject FilterTargetDoorCandidates(ref List<GameObject> targetDoorCandidates, string filterMode)
    {
       // Debug.Log("����ִ�г���ɸѡ����");
        GameObject exit = null;

        // ��ɸһ����Ծ����ų���
        if (targetDoorCandidates.Count > 0)
        {
            for (int doorCandidateIndex = targetDoorCandidates.Count - 1; doorCandidateIndex >= 0; doorCandidateIndex--)
            {
                // ���һ����ѡ��û�о�ֱ���˳�
                if (targetDoorCandidates.Count <= 0)
                    break;

                // ������ڳ��ھͷ��س���
                if (targetDoorCandidates[doorCandidateIndex].CompareTag("Exit"))
                {
                    exit = targetDoorCandidates[doorCandidateIndex];
                    break;
                }
                // ���ڻ���û̽�����ķ�����������ڼ��������
                if (filterMode is "Explore" && _doorMemoryQueue.Contains(targetDoorCandidates[doorCandidateIndex]))
                {

                    targetDoorCandidates.Remove(targetDoorCandidates[doorCandidateIndex]);
                    continue;
                }
            }

        }
        // ��ɸһ����Ҫ�ȽϺ��ų���
        if (targetDoorCandidates.Count > 1)
        {
            for (int doorCandidateIndex = targetDoorCandidates.Count - 1; doorCandidateIndex >= 0; doorCandidateIndex--)
            {
                // ��ͬʱ����"��¥¥��"��"�߹�����"ʱ�� ����ɸ��"��¥¥��"
                // ���ǵ������ǴӾ������ŷ���ʱ������ɸ��"�߹�����"
                if (filterMode is "Normal" && targetDoorCandidates[doorCandidateIndex] == lastDoorWentThrough)
                {
                    targetDoorCandidates.Remove(targetDoorCandidates[doorCandidateIndex]);
                    continue;
                }
            }
        }
        return exit;
    }
    private void FilterLeaderCandidates(ref List<GameObject> leaderCandidates)
    {
        myLeader = leaderCandidates[0].gameObject;
    }
    private bool IsOnNavMesh(Vector3 targetPosition)
    {
        return NavMesh.SamplePosition(targetPosition, out NavMeshHit _, 0.1f, 1);
    }

    private Vector3 GetCrossDoorDestination(GameObject targetDoor)//ȥ�������ŵ�λ��
    {
        Vector3 myPosition = transform.position;

        if (targetDoor.CompareTag("Door") || targetDoor.CompareTag("Exit"))
        {
            string doorDirection = targetDoor.GetComponent<Door>().doorDirection;
            Vector3 doorPosition = targetDoor.transform.position + new Vector3(0, -1.5f, 0);
            switch (doorDirection)
            {
                case "Horizontal":
                    if (myPosition.z < doorPosition.z)
                        return doorPosition + new Vector3(0, 0, 2);
                    return doorPosition - new Vector3(0, 0, 1);
                case "Vertical":
                    if (myPosition.x < doorPosition.x)
                        return doorPosition + new Vector3(2, 0, 0);
                    return doorPosition - new Vector3(1, 0, 0);
                default:
                    return myPosition;
            }
        }
        else
        {
            return myPosition;
        }
    }

    /// <summary>
    /// �л���Ϊģʽ
    /// </summary>
    private void SwitchBehaviourMode()
    {
        if (myBehaviourMode == "Follower")
        {
            myBehaviourMode = "Leader";
            
            myTargetDoor = null;
            // _myRigidBody.mass = 4;
        }
        else
        {
            myBehaviourMode = "Follower";
            
            //׷�������
            myTargetDoor = null;
            // _myRigidBody.mass = 2;
        }
    }
    private void OnTriggerEnter(Collider trigger)
    {
       // Debug.Log("��ײ��������ײ��ı�ǩ�ǣ�" + trigger.transform.tag);
        GameObject triggerObject = trigger.gameObject;
        isReturnFromLastDoor = triggerObject == lastDoorWentThrough;
        switch (trigger.transform.tag)
        {
            case "Door":
                lastDoorWentThrough = triggerObject;
                //print("��һ�Ⱦ��������ǣ�"+ triggerObject.name);
                if (!_doorMemoryQueue.Contains(triggerObject))
                    _doorMemoryQueue.Enqueue(triggerObject);
                //print(triggerObject.name + "�Ѿ����������");
                if (_doorMemoryQueue.Count > 4)
                    _doorMemoryQueue.Dequeue();
                break;
            case "Exit":
                // print("�ҳɹ�������");
              /*  myEnv.personList.Remove(this);*/
                this.gameObject.SetActive(false);
                
                break;
        }
    }
}


