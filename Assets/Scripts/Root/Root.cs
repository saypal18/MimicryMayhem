using UnityEngine;
// class to assign root object of a collider or such objects
public class Root : MonoBehaviour
{
    [SerializeField] private GameObject obj;
    public GameObject GO => obj;

    public static void Assign(GameObject parent, GameObject child)
    {
        child.AddComponent<Root>().obj = parent;
    }

    public void Assign(GameObject parent)
    {
        obj = parent;
    }
}
