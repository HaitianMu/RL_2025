using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
public partial class HumanControl: MonoBehaviour
{
    // ��ײ��������
    public int dazingCountDown;
    public GameObject myTopLevelLeader;
    public List<HumanControl> myDirectFollowers;
    // Start is called before the first frame update



    //�쵼��ģʽ�������Լ��ƶ�
    private void LeaderUpdate()
    {
        if (myTargetDoor is null)
        {
            //print(this.gameObject.name+"��Ŀǰû��Ŀ����");
         
            //Ŀǰ��֪��ȥ�ģ�����������û���ҵ������ˣ���ʼ�Լ��ҹ�
            //print("��ǰû�мƻ�ǰ�����ţ���ʼɨ�裬Ȼ��ɸѡ");
            (List<GameObject> doorCandidates, List<Vector3> unknownDirections) = GetCandidate(new List<string> { "Door", "Exit" }, 360, visionLimit);

            GameObject exit = FilterTargetDoorCandidates(ref doorCandidates, unknownDirections.Count > 0 ? "Explore" : "Normal");
            if (exit is not null)
            {
                // print("���ֳ��ڣ�ֱ��ѡ�������ΪĿ��");
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
                // print("��ѡ�����в����ڳ��ڣ����ѡ��һ�ȿ�ͨ��������Ϊ�ƶ�Ŀ��");
                if (lastDoorWentThrough == null)//"�Լ���û�о�������"
                {
                    myTargetDoor = doorCandidates[Random.Range(0, doorCandidates.Count)];
                }
                // Case 2: �м�¼���Ҵ���������ѡ�� -> �ų���¼�ź����ѡ��
                else if (doorCandidates.Count >= 1)
                {
                    // �����ų��б� ��������е�������
                    var excludedDoors = new HashSet<GameObject>(_doorMemoryQueue);
                    var validDoors = doorCandidates.Where(door => !excludedDoors.Contains(door)).ToList();
                    //door => !excludedDoors.Contains(door)��Lambda���ʽ 

                    if (validDoors.Count > 0)//̽�����˼����в����ڵ��ţ�
                    {//Debug.Log($"{this.name} �������������У�{string.Join(", ", validDoors.Select(door => door.name))}");
                        myTargetDoor = validDoors[Random.Range(0, validDoors.Count)];
                    }
                    else
                    {
                        // ��������Ŷ����ų�����ǿ��ѡ�����ʷ�ŵ�����ţ�fallback���ƣ�
                        var fallbackDoors = doorCandidates.Where(door => door != lastDoorWentThrough).ToList();
                        myTargetDoor = fallbackDoors.Count > 0 ? fallbackDoors[Random.Range(0, fallbackDoors.Count)] : doorCandidates[0]; // ���ջ���

                    }
                }


                // print("ѡ������ǣ�" + myTargetDoor.transform.name);
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
                //������Ƿ��ջ�
                if (myTargetDoor.GetComponent<DoorControl>().isBurnt == false)
                {
                    // print("��ǰ���ڼƻ�ǰ�����ţ��������Ŷ����ƶ�");
                    Vector3 myPosition = transform.position;
                    myPosition.y -= 0.5f;
                    float distanceRemain = Vector3.Distance(myPosition, myDestination);
                    if (distanceRemain > 0.5f)
                    {//print($"�����Ŷ��滹��{distanceRemain}�ף�ɨ����·�Ƿ��к��ʵ��쵼��");

                        List<GameObject> leaderCandidates = GetCandidate(new List<string> { "Human", "Robot" }, 360, 8).Item1;
                      

                        if (leaderCandidates.Count > 0)
                        {
                            Debug.Log("�����п��Ը�����쵼�ߣ������л�Ϊ������ģʽ");
                                SwitchBehaviourMode();
   
                            return;
                        }
                    }
                    else
                    {  //print($"�����Ŷ��滹��{distanceRemain}�ף�������Ϊ�Ѿ���Ϊ�Ѿ�����Ŀ�ĵأ���ʼ����ɨ��");
                        myTargetDoor = null;
                        return;
                    }
                }
                else { myTargetDoor = null; }

            }
        }
    }
    //������ģʽ
    private void FollowerUpdate()
    {
        //print("�л�ģʽ:׷����");

        if (myLeader == null)//�����ǰû���쵼�ߣ�����Ѱ���쵼��
        {
            List<GameObject> leaderCandidates = GetCandidate(new List<string> { "Human", "Robot" }, 360, 5).Item1;
            //FilterLeaderCandidates(ref leaderCandidates);//ɸѡ��������Ϊ�Լ��쵼�ߵ��ˣ�
            Debug.Log("�Ҽ�⵽����������Ϊ��"+leaderCandidates.Count);
            // GameObject targetLeader= DecideMyLeader(leaderCandidates);//��ʣ����������ѡ��һ����Ϊ�Լ����쵼��

            // �������ˣ��ų��Ѿ����쵼�ߵ�����
            leaderCandidates = leaderCandidates
                .Where(candidate =>
                    candidate != gameObject &&  // �ų��Լ�
                    (candidate.CompareTag("Robot") ||  // ����������������
                     (candidate.CompareTag("Human") &&
                      candidate.GetComponent<HumanControl>().myLeader == null)) // ���������쵼��
                ).ToList();


            GameObject targetLeader;
            if (leaderCandidates.Count > 0)
            {
                 targetLeader = leaderCandidates[Random.Range(0, leaderCandidates.Count)];
                // ʹ�� targetLeader...
                //Debug.Log("���������ҵ��쵼���ǣ�"+targetLeader);
                if (targetLeader.CompareTag("Human"))
                {
                    print("�쵼��������");
                    //add

                    if (targetLeader.GetComponent<HumanControl>().dazingCountDown < 2)
                    {
                        dazingCountDown = Random.Range(2, 8);
                        return;
                    }
                    else
                    {
                        myLeader = targetLeader;
                        myLeader.GetComponent<HumanControl>().myDirectFollowers.Add(gameObject.GetComponent<HumanControl>());
                    }
                }
                else if (targetLeader.CompareTag("Robot"))
                {
                    // ֱ�Ӹ��������
                    myLeader = targetLeader;
                    myLeader.GetComponent<RobotControl>().myDirectFollowers.Add(gameObject.GetComponent<HumanControl>());
                }
            }
            else
            {
                
                Debug.LogWarning("û�п�ѡ���쵼�ߺ�ѡ�����л����쵼��ģʽ");
                // �����޺�ѡ�����������Ϊ null ��Ĭ��ֵ��
                 targetLeader = null;
                SwitchBehaviourMode();
            }
        }
        else//�Ѿ����쵼���ˣ�������쵼�ߣ�ֱ�����ڡ�
        {
            Vector3 leaderPosition = myLeader.transform.position;
            List<GameObject> exitList = GetCandidate(new List<string> { "Exit" }, 360, visionLimit).Item1;

            //�ڸ���Ĺ����У��������м���Ƿ��г��ڣ��еĻ���ֱ���뿪,û�еĻ��ͼ������������
            if (exitList.Count > 0)
            {
                if (myLeader.tag == "Robot")
                {
                    myLeader.GetComponent<RobotControl>().myDirectFollowers.Remove(gameObject.GetComponent<HumanControl>());
                }
                else if (myLeader.tag == "Human")
                {
                    myLeader.GetComponent<HumanControl>().myDirectFollowers.Remove(gameObject.GetComponent<HumanControl>());
                }
                print(this.name + "���Լ��Ƴ������˵ĸ����б�");
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
                // ���� leader �� Transform ���
                Vector3 leaderForward = myLeader.transform.forward;
                Vector3 targetPosition = leaderPosition - leaderForward * 1f;
                _myNavMeshAgent.SetDestination(targetPosition);
            }
        }
    }


    private Vector3 GetCrossDoorDestination(GameObject targetDoor)//ȥ�������ŵ�λ��
    {
        //Debug.Log("ִ����GetCrossDoorDestionation����");
        Vector3 myPosition = transform.position;

        if (targetDoor.CompareTag("Door"))
        {
            string doorDirection = targetDoor.GetComponent<DoorControl>().doorDirection;
            Vector3 doorPosition = targetDoor.transform.position + new Vector3(0, -1.5f, 0);
            switch (doorDirection)
            {
                case "Vertical":
                    if (myPosition.z < doorPosition.z)
                        return doorPosition + new Vector3(0, 0, 2);
                    return doorPosition - new Vector3(0, 0, 2);
                case "Horizontal":  //ˮƽ
                    if (myPosition.x < doorPosition.x)
                        return doorPosition + new Vector3(2, 0, 0);
                    return doorPosition - new Vector3(2, 0, 0);
                default:
                    return myPosition;
            }
        }
        else if (targetDoor.CompareTag("Exit"))
        {
            Vector3 doorPosition = targetDoor.transform.position + new Vector3(0, -1.5f, 0);
            return doorPosition;
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
    private Tuple<List<GameObject>, List<Vector3>> GetCandidate(List<string> targetTags, int visionWidth, int visionDiff)
    {
        // ��ʼ����ѡ�����б��δ֪�����б�
        List<GameObject> candidateList = new();
        List<Vector3> unknownDirections = new();
        Vector3 myPosition = transform.position;
        // ��ȡ��ǰ�����λ��
        String layer = "Door";//���ݱ�ǩ����ȡ���߼��Ĳ��,Ĭ��ɨ����
        if (targetTags.Contains("Door") || targetTags.Contains("Exit"))
        {
            layer = "Default";
        }
        else if (targetTags.Contains("Robot")|| targetTags.Contains("Human"))//����̽��������
        {
            layer = "Follower";
        }
        foreach (Vector3 vision in GetVision(visionWidth, visionDiff))
        {
            // �ӵ�ǰλ�������߷���������
            if (Physics.Raycast(myPosition, vision, out RaycastHit hit, visionLimit, LayerMask.GetMask(layer)))
            {
                // ������߻��еĶ���ı�ǩ��Ŀ���ǩ�б��У����Ҹö����ں�ѡ�б��У�����ӵ���ѡ�б�
                if (targetTags.Contains(hit.transform.tag) && !candidateList.Contains(hit.transform.gameObject))
                    // print("ɨ�赽�����У�"+hit.transform.gameObject.name);
                    candidateList.Add(hit.transform.gameObject);
            }
            else
            {
                // �������û�л����κζ����򽫸÷�����ӵ�δ֪�����б�
                //print("û��ɨ�赽����");
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
        if (leaderCandidates.Count > 0)
        {
            for (int candidateIndex = leaderCandidates.Count - 1; candidateIndex >= 0; candidateIndex--)
            {
                if (leaderCandidates.Count <= 0)
                    break;

                if (leaderCandidates[candidateIndex] == this.gameObject) //�Ǳ�����
                {
                    //Debug.Log(leaderCandidates[candidateIndex]);
                    leaderCandidates.Remove(leaderCandidates[candidateIndex]);
                }
                
              /*  else if (leaderCandidates[candidateIndex].CompareTag("Human"))
                {
                    HumanControl candidateInfo = leaderCandidates[candidateIndex].GetComponent<HumanControl>();
                    //add
                    if (candidateInfo.myTopLevelLeader.CompareTag("Human"))
                        leaderCandidates.Remove(leaderCandidates[candidateIndex]);

                    //minus
                    //if (candidateInfo.myFollowerCounter < myFollowerCounter &&
                    //    candidateInfo.myTopLevelLeader is not null &&
                    //    candidateInfo.myTopLevelLeader.CompareTag("Human") &&
                    //    candidateInfo.myTopLevelLeader.GetComponent<Human>().myFollowerCounter <= myFollowerCounter)
                    //    leaderCandidates.Remove(leaderCandidates[candidateIndex]);
                }*/
            }
        }
    }

    private GameObject DecideMyLeader(List<GameObject> leaderCandidates)
    {
        List<GameObject> humanCandidates = new();
        List<GameObject> robotCandidates = new();

        foreach (GameObject candidate in leaderCandidates)
        {
            if (candidate.CompareTag("Human"))
            {
                humanCandidates.Add(candidate);
                continue;
            }

            if (candidate.CompareTag("Robot"))
            {
                robotCandidates.Add(candidate);
                continue;
            }
        }
        //robotCandidates = RbtList;///
        if (robotCandidates.Count > 0)
            return robotCandidates[Random.Range(0, robotCandidates.Count)];
        else if (humanCandidates.Count > 0)
        {
            Debug.Log("���������б�");
            return humanCandidates[Random.Range(0, humanCandidates.Count)];
        }
        else
            return null;
    }
}
