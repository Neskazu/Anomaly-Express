using UnityEngine;

public class TrainMenuLights : MonoBehaviour
{
    public GameObject[] wagon1;
    public GameObject[] wagon2;
    public GameObject[] wagon3;

    public void ToggleWagon1()
    {
        ToggleObjects(wagon1);
    }

    public void ToggleWagon2()
    {
        ToggleObjects(wagon2);
    }

    public void ToggleWagon3()
    {
        ToggleObjects(wagon3);
    }

    private void ToggleObjects(GameObject[] objects)
    {
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] == null)
                continue;

            objects[i].SetActive(!objects[i].activeSelf);
        }
    }
}