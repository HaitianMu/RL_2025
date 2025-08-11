using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public partial class  FireControl : MonoBehaviour
{
        /// <summary>
        /// �ж�ĳ���ذ���Ƿ������ɻ���
        /// </summary>
        /// <param name="targetGroundBlock"></param>
        /// <returns></returns>


    public void CheckSurroundingStatus()
    //1,��Խ�磻
    //2.�����Ļ��淢����ײ/����ǽ�ڷ�����ײ/������ڷ�����ײ
    //3. ����Ѿ��ﵽ������������������ɢ
    {
        for (int i = 0; i < 4; i++)
        {
            Vector3 direction = _directionSequence[i];
            Vector3 checkPos = transform.position + direction;

            // ���ÿռ�������ټ��
            _surroundingsStatus[i] = !fireSpatialPartition.HasNearbyFire(checkPos, 0.4f);

            // ����ռ�������ͨ�����ٽ��о�ȷ���
            if (_surroundingsStatus[i])
            {
                _surroundingsStatus[i] = CanSpawnAtPosition(checkPos);
            }
        }

        // �����Ƿ�����ɢ
        isPossibleToSpread = _surroundingsStatus[0] || _surroundingsStatus[1] ||
                            _surroundingsStatus[2] || _surroundingsStatus[3];
        /*       if (activeFireCount >= maxActiveFires)
               {
                   _surroundingsStatus = new List<bool> { false, false, false, false };
                   return;
               }

               //1,��Խ�磻
               //2.�����Ļ��淢����ײ/����ǽ�ڷ�����ײ/������ڷ�����ײ

               *//*if (Physics.Raycast(FirePosition, Vector3.up, out RaycastHit hit, 1.0f))
                   return !(hit.transform.CompareTag("Fire") || hit.transform.CompareTag("Wall") || hit.transform.CompareTag("Exit"));
               else
                   return true;*//*



               //�������
               for (int i = 0; i < 4; i++)
               {
                   Vector3 direction = _directionSequence[i];

                   Vector3 halfExtents = new Vector3(0.249f, 4f, 0.249f);
                   Collider[] hits = Physics.OverlapBox(this.transform.position+direction, halfExtents, Quaternion.identity);


                       foreach (Collider col in hits)
                       {
                       if (col.tag == "Wall" || col.tag == "Fire" || col.tag == "Exit") //��ɢ�ĵط�����ǽ�����Ѿ��л�������ˣ����λ�ò������ɻ���
                        { _surroundingsStatus[i] = false;
                           print($"��⵽����: {col.name} (Tag: {col.tag})"+"����λ�ò��ܽ�����ɢ");
                           }


                       else if (col.tag == "Door")//��������
                       {
                               _surroundingsStatus[i] = true;
                               string doorname= col.name;
                               GameObject gameObject = GameObject.Find(doorname);
                               gameObject.SetActive(false);
                           print($"��⵽����: {col.name} (Tag: {col.tag})" + "����������Ϊ������");
                       }

                   }

               }
               // �����Ƿ�����ɢ
               isPossibleToSpread = _surroundingsStatus[0] || _surroundingsStatus[1] ||_surroundingsStatus[2] || _surroundingsStatus[3];
          */
    }
    /// <summary>
    /// �жϵ�ǰ�ذ���ϵĻ����Ƿ�����������ɢ
    /// </summary>
    /// <returns></returns>

    private IEnumerator TrySpreadFire()
    {
        for (int i = 0; i < 4; i++)
        {
            if (_surroundingsStatus[i])
            {
                Vector3 targetPos = transform.position + _directionSequence[i];

                // ʹ�ÿռ�������ټ��
                if (!fireSpatialPartition.HasNearbyFire(targetPos, 0.4f))
                {
                    // ��ȷ���
                    if (CanSpawnAtPosition(targetPos))
                    {
                        // �Ӷ���ػ�ȡ����
                        GameObject newFire = FirePoolManager.Instance.GetFire(
                            targetPos,
                            Quaternion.identity,
                            myEnv);

                        if (newFire != null)
                        {
                            yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
                        }
                    }
                }
            }
        }
    }

    private bool CanSpawnAtPosition(Vector3 position)
    {
        HashSet<string> tags = new HashSet<string>();
        List<GameObject> doorsFound = new List<GameObject>(); // �洢�ҵ����Ŷ���
        bool hasProcessedDoor = false; // ����Ƿ��Ѵ������

        Collider[] colliders = Physics.OverlapBox(
            position,
            new Vector3(0.2f, 4f, 0.2f),
            Quaternion.identity);

        // ��һ��������ռ���ǩ���Ŷ���
        foreach (var col in colliders)
        {
            string currentTag = col.tag;
            tags.Add(currentTag);

            if (currentTag == "Door"|| currentTag == "BurnedDoor" && !hasProcessedDoor)
            {
                doorsFound.Add(col.gameObject);
            }
        }

        // �ڶ�������������Ŷ���
        foreach (GameObject door in doorsFound)
        {
            // ȷ���Ŷ�����Ȼ��Ч
            if (door == null) continue;

            door.GetComponent<DoorControl>().BurnDoor();//�ջ���
        }

        // �ж��߼�������ԭ�й���
        bool hasDoor = tags.Contains("Door")|| tags.Contains("BurnedDoor");
        bool hasWall = tags.Contains("Wall");
        bool hasExit = tags.Contains("Exit");
        bool onFloor = tags.Contains("Floor");
        // ��ͬʱ�����ź�ǽʱ�������ɻ���
        if (hasDoor && hasWall) return true;
        // ��������ǽ�����ʱ��ֹ����
        if (hasWall || hasExit) return false;

        // �������,������ڵذ��ϣ�����������
        if (onFloor)
        {
            return true;
        }

        return false;
    }

}
