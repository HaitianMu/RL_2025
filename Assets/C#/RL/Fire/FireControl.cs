using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public partial class FireControl : MonoBehaviour
{


    //4.21��Ŀǰ�������Ĺ������أ�������ʱ�ܸ��ܵ����ԵĿ��٣������Ż�
    //

    // ���ܵؿ��״̬(�Ƿ���������µĻ�Դ)
    private List<bool> _surroundingsStatus;

    // ���ذ���˳��
    private readonly List<Vector3> _directionSequence = new() { Vector3.left/2, Vector3.right / 2, Vector3.forward / 2, Vector3.back / 2 };

    // ����Ԥ����
    public GameObject firePrefab;

    // �����ʱ����Ŀ�����
    public GameObject tmpFireWrapper;

    // ��ǰ����
    public EnvControl myEnv;

    // ��ǰλ�õĻ����Ƿ�������������
    private bool isPossibleToSpread = true;

    private int spreadingCountDown;

    private static readonly WaitForSeconds waitTime = new WaitForSeconds(0.1f);
    //��6֡��0.1��󣩣�100������ͬʱִ�м�飨����60FPS��
    //���Ի��ǻ���ֻ����ص�����������Բ������ʱ�������ɢ

    private static SpatialPartition fireSpatialPartition = new SpatialPartition(1f);


    void Start() //��ʼ��
    {

        _surroundingsStatus = new List<bool> { true, true, true, true };
       // print("����ĳ�ʼ������");
        this.enabled = true;    //�����ű�  
        isPossibleToSpread = true;

        // �������ɢ��ʱ��
        spreadingCountDown = Random.Range(200, 300);

        // ��ʼ�����Χ״̬
        StartCoroutine(DelayedInitialCheck());

        // ע�ᵽ�ռ����
        fireSpatialPartition.Add(transform.position, this);
    }


    private IEnumerator DelayedInitialCheck()
    {
        yield return new WaitForSeconds(Random.Range(0.05f, 0.3f));
        CheckSurroundingStatus();
    }

    // Update is called once per frame
    void FixedUpdate() //��ʼ��ɢ,����д��ÿһ֡�û��涼�������ɢ��������
    {
        if (!isPossibleToSpread) return;

        if (spreadingCountDown <= 0)
        {
            StartCoroutine(TrySpreadFire());
            enabled = false; // ��ɢ�����
        }
        spreadingCountDown--; 
    }

    private void OnTriggerEnter(Collider collision)//��ײ����������ײ��������ǽʱ�������û��棬����ײ���������ǣ�������
    {
        GameObject triggerObject = collision.gameObject;
       // print("��ײ������"+collision.gameObject.tag);
       
    }
    private void OnDestroy()
    {
        // ȷ�������й���ϵͳ���Ƴ�
        if (fireSpatialPartition != null)
        {
            fireSpatialPartition.Remove(transform.position);
        }

        if (myEnv != null)
        {
            lock (myEnv.FireList)
            {
                myEnv.FireList.Remove(this);
            }
        }

        StopAllCoroutines();
    }
}
