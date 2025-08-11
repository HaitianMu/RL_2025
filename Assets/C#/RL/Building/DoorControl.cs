using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class DoorControl : MonoBehaviour
{
    public string doorDirection; // �ŵĳ���
    //!!!!!!!!!�������ŵĽ���
    private NavMeshLink Link;     //���������������ɢ����֮�󣬽��øõ�����������µ�������
    public bool isBurnt = false; //���Ƿ��ջ�

    public void AddNavMeshLink()
    {
        Link = this.AddComponent<Unity.AI.Navigation.NavMeshLink>();
        Link.autoUpdate = true;
        Link.width = 1;
        Link.bidirectional = true;//����˫�򵼺�

        if (doorDirection == "Vertical") //ˮƽ
        {
            Link.startPoint = new Vector3(0, -1.5f, 0.4f);
            Link.endPoint = new Vector3(0, -1.5f, -0.4f);
        }
        else if (doorDirection == "Horizontal")
        {
            Link.startPoint = new Vector3(0.4f, -1.5f, 0);
            Link.endPoint = new Vector3(-0.4f, -1.5f, 0);
        }
        // �ر���Ҫ�����ú��ʵ��������
      /*  Link.height = 2.0f; // ����ĸ߶Ȳ�*/
    }

    public void BurnDoor()
    {
        if (isBurnt || Link == null) return;
        // �Ӿ�Ч��
        GetComponent<Renderer>().material.color = Color.black; //����ɫ��ɺ�ɫ
        //�����ŵĵ�������
        // 2.���������ӣ��°�Unity��ʽ��
        Link.enabled = false; // �������
        // 3. ��ȫ�Ƴ���������
        Destroy(Link);
        // 4. ���Ϊ���ջ�
        isBurnt = true;
        // 5.�ı��ŵı�ǩ����ֹ�ٴα���ΪĿ��ص�
        this.tag = "BurnedDoor";
        // 5. ���µ�������
        StartCoroutine(DelayedNavMeshUpdate());
    }

    private IEnumerator DelayedNavMeshUpdate()
    {
        yield return new WaitForSeconds(0.5f); // �ӳٸ��±��⿨��

        // ��ȡ�����е�NavMeshSurface������
        var surface = FindObjectOfType<NavMeshSurface>();
        if (surface != null)
        {
            surface.UpdateNavMesh(surface.navMeshData);
        }
    }
}
