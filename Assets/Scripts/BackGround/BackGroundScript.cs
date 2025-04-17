using UnityEngine;

public class BackGroundScript : MonoBehaviour
{
    public float speed;
    [SerializeField] float PositionDissapear; // ������� ��� ��������� ������

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(transform.position.x + speed * Time.deltaTime, transform.position.y, 0); // ������� ������ ����� ��� ������� ������ �����
        if (transform.position.x > PositionDissapear)
        {
            Destroy(gameObject); // ������� ������
        }
    }
}
